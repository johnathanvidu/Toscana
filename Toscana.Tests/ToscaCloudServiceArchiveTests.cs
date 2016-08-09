﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.IO.Abstractions.TestingHelpers;
using System.IO.Compression;
using System.Linq;
using System.Text;
using FluentAssertions;
using NUnit.Framework;
using Toscana.Common;
using Toscana.Exceptions;
using Toscana.Tests.Engine;

namespace Toscana.Tests
{
    [TestFixture]
    public class ToscaCloudServiceArchiveTests
    {
        [Test]
        public void EntryPointServiceTemplate_Returns_EntryDefinitions_Template()
        {
            // Act
            var toscaCloudServiceArchive =
                new ToscaCloudServiceArchive(new ToscaMetadata {EntryDefinitions = "tosca1.yaml"});
            toscaCloudServiceArchive.AddToscaServiceTemplate("tosca1.yaml",
                new ToscaServiceTemplate {Description = "tosca1 description"});
            toscaCloudServiceArchive.AddToscaServiceTemplate("base.yaml",
                new ToscaServiceTemplate {Description = "base description"});

            // Assert
            toscaCloudServiceArchive.GetEntryPointServiceTemplate().Description.Should().Be("tosca1 description");
        }

        [Test]
        public void GetArtifactBytes_Should_Return_Empty_Array_When_File_Is_Empty()
        {
            // Arrange
            var toscaMetadata = new ToscaMetadata
            {
                CreatedBy = "Devil",
                CsarVersion = new Version(1, 1),
                ToscaMetaFileVersion = new Version(1, 0),
                EntryDefinitions = @"definitions/tosca_elk.yaml"
            };

            var fileSystem = new MockFileSystem();
            fileSystem.CreateArchive("tosca.zip", new[]
            {
                new FileContent("some_icon.png", "IMAGE")
            });

            var archiveEntriesDictionary =
                new ZipArchive(fileSystem.File.Open("tosca.zip", FileMode.Open)).GetArchiveEntriesDictionary();

            var toscaCloudServiceArchive = new ToscaCloudServiceArchive(toscaMetadata, archiveEntriesDictionary);
            var toscaServiceTemplate = new ToscaServiceTemplate {Description = "Devil created the world."};
            var toscaNodeType = new ToscaNodeType();
            toscaNodeType.Artifacts.Add("icon", new ToscaArtifact {File = "some_icon.png"});
            toscaServiceTemplate.NodeTypes.Add("nut-shell", toscaNodeType);

            // Act
            toscaCloudServiceArchive.AddToscaServiceTemplate(@"definitions/tosca_elk.yaml", toscaServiceTemplate);

            // Assert
            toscaCloudServiceArchive.GetArtifactBytes("some_icon.png")
                .Should()
                .BeEquivalentTo(new byte[] {73, 77, 65, 71, 69});
        }

        [Test]
        public void GetArtifactsBytes_Should_Return_Artifact_Content()
        {
            // Act
            var fileSystem = new MockFileSystem();
            fileSystem.CreateArchive("tosca.zip", new[] {new FileContent("device.png", "IMAGE_CONTENT")});
            var zipArchive = new ZipArchive(fileSystem.File.Open("tosca.zip", FileMode.Open));
            var zipArchiveEntries = zipArchive.GetArchiveEntriesDictionary();

            var toscaNodeType = new ToscaNodeType();
            toscaNodeType.Artifacts.Add("icon", new ToscaArtifact
            {
                File = "device.png"
            });
            var toscaServiceTemplate = new ToscaServiceTemplate();
            toscaServiceTemplate.NodeTypes.Add("device", toscaNodeType);
            var toscaCloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata
            {
                EntryDefinitions = "tosca.yaml"
            }, zipArchiveEntries);

            // Act
            toscaCloudServiceArchive.AddToscaServiceTemplate("definition", toscaServiceTemplate);
            var artifactsBytes = toscaCloudServiceArchive.GetArtifactBytes("device.png");

            // Assert
            artifactsBytes.ShouldAllBeEquivalentTo("IMAGE_CONTENT".ToByteArray(Encoding.ASCII));
        }

        [Test]
        public void AddToscaServiceTemplate_Should_Throw_ArtifactNotFoundException_When_File_Missing_In_Archive()
        {
            // Act
            var fileSystem = new MockFileSystem();
            fileSystem.CreateArchive("tosca.zip", new FileContent[0]);
            var zipArchive = new ZipArchive(fileSystem.File.Open("tosca.zip", FileMode.Open));
            var zipArchiveEntries = zipArchive.GetArchiveEntriesDictionary();

            var toscaNodeType = new ToscaNodeType();
            toscaNodeType.Artifacts.Add("icon", new ToscaArtifact
            {
                File = "device.png"
            });
            var toscaServiceTemplate = new ToscaServiceTemplate();
            toscaServiceTemplate.NodeTypes.Add("device", toscaNodeType);
            var toscaCloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata
            {
                EntryDefinitions = "tosca.yaml"
            }, zipArchiveEntries);

            // Act
            Action action = () => toscaCloudServiceArchive.AddToscaServiceTemplate("definition", toscaServiceTemplate);

            // Assert
            action.ShouldThrow<ArtifactNotFoundException>()
                .WithMessage("Artifact 'device.png' not found in Cloud Service Archive.");
        }

        [Test]
        public void GetArtifactBytes_Should_Throw_ArtifactNotFoundException_When_File_Missing_In_Archive()
        {
            // Act
            var toscaServiceTemplate = new ToscaServiceTemplate();
            var toscaNodeType = new ToscaNodeType();
            toscaServiceTemplate.NodeTypes.Add("device", toscaNodeType);
            var toscaCloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata
            {
                EntryDefinitions = "tosca.yaml"
            }, new Dictionary<string, ZipArchiveEntry>());

            // Act
            Action action = () => toscaCloudServiceArchive.GetArtifactBytes("NOT_EXISTING.png");

            // Assert
            action.ShouldThrow<ArtifactNotFoundException>()
                .WithMessage("Artifact 'NOT_EXISTING.png' not found in Cloud Service Archive.");
        }

        [Test]
        public void NodeTypes_Should_Not_Be_Null_Upon_Initialization()
        {
            // Act
            var toscaCloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata());

            // Assert
            toscaCloudServiceArchive.NodeTypes.Should().NotBeNull();
        }

        [Test]
        public void ToscaMetadata_Should_Not_Be_Null_Upon_Initialization()
        {
            // Act
            var toscaCloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata
            {
                CreatedBy = "me",
                CsarVersion = new Version(1, 12),
                EntryDefinitions = "tosca.yaml",
                ToscaMetaFileVersion = new Version(2, 23)
            });

            // Assert
            toscaCloudServiceArchive.ToscaMetadata.CreatedBy.Should().Be("me");
            toscaCloudServiceArchive.ToscaMetadata.CsarVersion.Should().Be(new Version(1, 12));
            toscaCloudServiceArchive.ToscaMetadata.EntryDefinitions.Should().Be("tosca.yaml");
            toscaCloudServiceArchive.ToscaMetadata.ToscaMetaFileVersion.Should().Be(new Version(2, 23));
        }

        [Test]
        public void ToscaServiceTemplates_Should_Not_Be_Null_Upon_Initialization()
        {
            // Act
            var toscaCloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata());

            // Assert
            toscaCloudServiceArchive.ToscaServiceTemplates.Should().NotBeNull();
        }

        [Test]
        public void GetArtifactBytes_Should_Return_File_Content_Of_File_That_Is_Not_An_Artifact()
        {
            // Arrange
            var fileSystem = new MockFileSystem();

            var toscaMetadata = new ToscaMetadata
            {
                CreatedBy = "Devil",
                CsarVersion = new Version(1, 1),
                ToscaMetaFileVersion = new Version(1, 0),
                EntryDefinitions = @"definitions/tosca_elk.yaml"
            };

            fileSystem.CreateArchive("tosca.zip", new[]
            {
                new FileContent("nut_shell.png", "IMAGE_CONTENT")
            });

            var archiveEntriesDictionary = new ZipArchive(fileSystem.File.Open("tosca.zip", FileMode.Open)).GetArchiveEntriesDictionary();

            var toscaCloudServiceArchive = new ToscaCloudServiceArchive(toscaMetadata, archiveEntriesDictionary);
            var toscaServiceTemplate = new ToscaServiceTemplate();
            toscaServiceTemplate.NodeTypes.Add("nut-shell", new ToscaNodeType());
            toscaCloudServiceArchive.AddToscaServiceTemplate(@"definitions/tosca_elk.yaml", toscaServiceTemplate);

            // Act
            var artifactBytes = toscaCloudServiceArchive.GetArtifactBytes("nut_shell.png");

            artifactBytes.ShouldAllBeEquivalentTo("IMAGE_CONTENT".ToByteArray(Encoding.ASCII));
        }

        [Test]
        public void GetArtifactBytes_OnCloudServiceArchive_Without_Artifacts_Should_Throw_ArtifactNotFoundException()
        {
            var toscaCloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata());

            Action action = () => toscaCloudServiceArchive.GetArtifactBytes("not_existing_file.png");

            action.ShouldThrow<ArtifactNotFoundException>()
                .WithMessage("Artifact 'not_existing_file.png' not found in Cloud Service Archive.");
        }

        [Test]
        public void Base_Property_Set_To_NodeType_Instance_Of_Derived_From()
        {
            // Arrange
            var deviceNodeType = new ToscaNodeType();
            deviceNodeType.Properties.Add("vendor", new ToscaPropertyDefinition { Type = "string" });

            var switchNodeType = new ToscaNodeType { DerivedFrom = "tosca.nodes.Device" };
            switchNodeType.Properties.Add("speed", new ToscaPropertyDefinition { Type = "integer" });

            // Act
            var cloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata());
            cloudServiceArchive.AddNodeType("tosca.nodes.Switch", switchNodeType);
            cloudServiceArchive.AddNodeType("tosca.nodes.Device", deviceNodeType);

            // Assert
            switchNodeType.Base.Properties.Should().ContainKey("vendor");
        }

        [Test]
        public void Base_Property_Of_Root_NodeType_Should_Be_Null()
        {
            // Arrange
            var deviceNodeType = new ToscaNodeType();
            deviceNodeType.Properties.Add("vendor", new ToscaPropertyDefinition { Type = "string" });

            var switchNodeType = new ToscaNodeType { DerivedFrom = "tosca.nodes.Device" };
            switchNodeType.Properties.Add("speed", new ToscaPropertyDefinition { Type = "integer" });

            // Act
            var cloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata());
            cloudServiceArchive.AddNodeType("tosca.nodes.Switch", switchNodeType);
            cloudServiceArchive.AddNodeType("tosca.nodes.Device", deviceNodeType);

            // Assert
            deviceNodeType.Base.Should().BeNull();
        }

        [Test]
        public void Base_Property_Of_Capability_Type_Is_Capability_Of_Derived_From()
        {
            // Arrange
            var serviceTemplate = new ToscaServiceTemplate();
            var basicCapabilityType = new ToscaCapabilityType();
            basicCapabilityType.Properties.Add("username", new ToscaPropertyDefinition { Type = "string"});
            serviceTemplate.CapabilityTypes.Add("basic", basicCapabilityType);
            serviceTemplate.CapabilityTypes.Add("connectable", new ToscaCapabilityType
            {
                DerivedFrom = "basic"
            });

            // Act
            var cloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata());

            cloudServiceArchive.AddToscaServiceTemplate("sample.yaml", serviceTemplate);

            // Assert
            cloudServiceArchive.CapabilityTypes["connectable"].Base.Properties.Should().ContainKey("username");
        }

        [Test]
        public void Node_Type_Without_Derived_From_Should_Have_Root_As_Their_Derived_From()
        {
            // Arrange
            var serviceTemplate = new ToscaServiceTemplate();
            serviceTemplate.NodeTypes.Add("device", new ToscaNodeType());

            // Act
            var cloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata());

            cloudServiceArchive.AddToscaServiceTemplate("sample.yaml", serviceTemplate);
            cloudServiceArchive.FillDefaults();

            // Assert
            cloudServiceArchive.NodeTypes["device"].DerivedFrom.Should().Be("tosca.nodes.Root");
            cloudServiceArchive.NodeTypes["device"].Base.Should().NotBeNull();
        }

        [Test]
        public void TraverseNodeTypesInheritance_Traverses_Nodes_From_Root_To_Its_Derived_Node_Types()
        {
            // Arrange
            var serviceTemplate = new ToscaServiceTemplate();
            serviceTemplate.NodeTypes.Add("device", new ToscaNodeType());
            serviceTemplate.NodeTypes.Add("switch", new ToscaNodeType { DerivedFrom = "device"});
            serviceTemplate.NodeTypes.Add("router", new ToscaNodeType { DerivedFrom = "device"});

            var cloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata());

            cloudServiceArchive.AddToscaServiceTemplate("sample.yaml", serviceTemplate);
            cloudServiceArchive.FillDefaults();
            
            // Act
            var discoveredNodeTypeNames = new List<string>();
            cloudServiceArchive.TraverseNodeTypesInheritance((nodeTypeName, nodeType) => { discoveredNodeTypeNames.Add(nodeTypeName );});

            // Assert
            discoveredNodeTypeNames.ShouldBeEquivalentTo(new[] { "tosca.nodes.Root", "device", "switch", "router" });
        }

        [Test]
        public void TraverseNodeTypesByRequirements_Traverses_Nodes_From_Specific_Node_Type_By_It_Requirements()
        {
            // Arrange
            var serviceTemplate = new ToscaServiceTemplate{ ToscaDefinitionsVersion = "tosca_simple_yaml_1_0" };

            var powerNodeType = new ToscaNodeType();
            powerNodeType.AddRequirement("power.cable", new ToscaRequirement { Node = "tosca.nodes.cable", Capability = "cable" });
            powerNodeType.AddRequirement("power.switch", new ToscaRequirement { Node = "tosca.nodes.switch", Capability = "cable" });

            var deviceNodeType = new ToscaNodeType();
            deviceNodeType.AddRequirement("power", new ToscaRequirement { Node = "tosca.nodes.power", Capability = "power"} );

            var cableNodeType = new ToscaNodeType();

            var switchNodeType = new ToscaNodeType();

            serviceTemplate.NodeTypes.Add("tosca.nodes.mic", new ToscaNodeType());
            serviceTemplate.NodeTypes.Add("tosca.nodes.cable", cableNodeType);
            serviceTemplate.NodeTypes.Add("tosca.nodes.switch", switchNodeType);
            serviceTemplate.NodeTypes.Add("tosca.nodes.power", powerNodeType);
            serviceTemplate.NodeTypes.Add("tosca.nodes.device", deviceNodeType);

            var cloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata
            {
                CsarVersion = new Version(1,1),
                EntryDefinitions = "sample.yaml",
                ToscaMetaFileVersion = new Version(1,1),
                CreatedBy = "Anonymous"
            });
            cloudServiceArchive.AddToscaServiceTemplate("sample.yaml", serviceTemplate);
            cloudServiceArchive.FillDefaults();

            List<ValidationResult> validationResults;
            cloudServiceArchive.TryValidate(out validationResults);
            var validationErrors = string.Join(Environment.NewLine, validationResults.Select(a => a.ErrorMessage).ToArray());
            validationErrors.Should().BeEmpty();

            // Act
            var discoveredNodeTypeNames = new List<string>();
            cloudServiceArchive.TraverseNodeTypesByRequirements("tosca.nodes.device", (nodeTypeName, nodeType) => { discoveredNodeTypeNames.Add(nodeTypeName); });

            // Assert
            discoveredNodeTypeNames.ShouldBeEquivalentTo(new[] { "tosca.nodes.device", "tosca.nodes.power", "tosca.nodes.switch", "tosca.nodes.cable" });
        }

        [Test]
        public void TraverseNodeTypesByRequirements_Should_Throw_Exception_When_NodeType_ToStart_NotFound()
        {
            // Arrange
            var serviceTemplate = new ToscaServiceTemplate{ ToscaDefinitionsVersion = "tosca_simple_yaml_1_0" };

            serviceTemplate.NodeTypes.Add("tosca.nodes.device", new ToscaNodeType());

            var cloudServiceArchive = new ToscaCloudServiceArchive(new ToscaMetadata
            {
                CsarVersion = new Version(1,1),
                EntryDefinitions = "not_existing.yaml",
                ToscaMetaFileVersion = new Version(1,1),
                CreatedBy = "Anonymous"
            });
            cloudServiceArchive.AddToscaServiceTemplate("sample.yaml", serviceTemplate);
            cloudServiceArchive.FillDefaults();

            // Act
            Action action = () => cloudServiceArchive.TraverseNodeTypesByRequirements("NOT_EXISTING", (nodeTypeName, nodeType) => { });

            // Assert
            action.ShouldThrow<ToscaNodeTypeNotFoundException>().WithMessage("Node type 'NOT_EXISTING' not found");
        }
    }
}