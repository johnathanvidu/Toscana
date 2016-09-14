﻿using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Toscana.Exceptions;
using Toscana.Fluent;
using Toscana.Fluent.Models;

namespace Toscana.Tests.Fluent
{
    [TestFixture]
    public class ToscaCSARBuilderTests
    {
        [Test]
        public void CreatingToscaCSARFluently_OnlyPartialMetadata_BuildObjectFailed()
        {
            // Arrange
            var dto = new SlimMetadata
            {
                ToscaMetaFileVersion = new Version(1, 0),
                CSARVersion = new Version(0, 1),
                CreatedBy = "AutoGenerated"
            };

            // Act
            var constructor = FluentCSAR.Config(
                builder =>
                    builder.AddMetadata(dto));

            Action action = () => constructor.BuildCSAR();

            // Assert
            action.ShouldThrow<ToscaValidationException>();
        }

        [Test]
        public void CreatingToscaCSARFluently_EntryDefinition_BuildObjectSuccess()
        {
            // Arrange
            const string EntryDefinition = "tosca.yml";
            var dto = new SlimMetadata
            {
                ToscaMetaFileVersion = new Version(1, 0),
                CSARVersion = new Version(0, 1),
                CreatedBy = "AutoGenerated"
            };

            var toscaServiceTemplate = new ToscaServiceTemplate { ToscaDefinitionsVersion = "tosca_simple_yaml_1_0" };

            // Act
            var constructor = FluentCSAR.Config(
                builder =>
                    builder
                        .AddMetadata(dto)
                        .EntryDefinitions(EntryDefinition, toscaServiceTemplate));

            var result = constructor.BuildCSAR();

            // Assert
            result.EntryDefinitions.Should().Be(EntryDefinition);
            result.ToscaServiceTemplates.Should().HaveCount(1);
            result.ToscaServiceTemplates.Should().Match(templates =>
                templates.All(template =>
                    template.Value.ToscaDefinitionsVersion == "tosca_simple_yaml_1_0"));
        }

        [Test]
        public void CreatingToscaCSARFluently_EntryDefinitionAndServiceTemplate_BuildObjectSuccess()
        {
            // Arrange
            const string EntryDefinition = "tosca.yml";
            const string SomeYmlFileName = "someymlfile.yml";

            var dto = new SlimMetadata
            {
                ToscaMetaFileVersion = new Version(1, 0),
                CSARVersion = new Version(0, 1),
                CreatedBy = "AutoGenerated"
            };

            var toscaServiceTemplate = new ToscaServiceTemplate { ToscaDefinitionsVersion = "tosca_simple_yaml_1_0" };

            // Act
            var constructor = FluentCSAR.Config(
                builder =>
                    builder
                        .AddMetadata(dto)
                        .EntryDefinitions(EntryDefinition, toscaServiceTemplate)
                        .AddServiceTemplate(SomeYmlFileName, toscaServiceTemplate));

            var result = constructor.BuildCSAR();

            // Assert
            result.EntryDefinitions.Should().Be(EntryDefinition);
            result.ToscaServiceTemplates.Should().HaveCount(2);
            result.ToscaServiceTemplates.Should().Match(templates =>
                templates.All(template =>
                    template.Value.ToscaDefinitionsVersion == "tosca_simple_yaml_1_0"));
        }

        [Test]
        public void CreatingToscaCSARFluently_EntryDefinitionServiceTemplateAndArtifact_BuildObjectSuccess()
        {
            // Arrange
            const string EntryDefinition = "tosca.yml";
            const string SomeYmlFileName = "someymlfile.yml";
            const string SomeArtifactFileName = "artifact.zip";

            var dto = new SlimMetadata
            {
                ToscaMetaFileVersion = new Version(1, 0),
                CSARVersion = new Version(0, 1),
                CreatedBy = "AutoGenerated"
            };

            var toscaServiceTemplate = new ToscaServiceTemplate { ToscaDefinitionsVersion = "tosca_simple_yaml_1_0" };

            var someFileData = new byte[] { 12, 35, 65, 1, 3, 65 };

            // Act
            var constructor = FluentCSAR.Config(
                builder =>
                    builder
                        .AddMetadata(dto)
                        .EntryDefinitions(EntryDefinition, toscaServiceTemplate)
                        .AddServiceTemplate(SomeYmlFileName, toscaServiceTemplate)
                        .AddArtifact(SomeArtifactFileName, someFileData));

            var result = constructor.BuildCSAR();

            // Assert
            result.EntryDefinitions.Should().Be(EntryDefinition);
            result.ToscaServiceTemplates.Should().HaveCount(2);
            result.ToscaServiceTemplates.Should().Match(templates =>
                templates.All(template =>
                    template.Value.ToscaDefinitionsVersion == "tosca_simple_yaml_1_0"));
            result.Artifacts.Should().HaveCount(1);
            result.Artifacts.Should().ContainSingle(pair => pair.Key == SomeArtifactFileName && pair.Value == someFileData);
        }

        [Test]
        public void CreatingToscaCSARFluently_EntryDefinitionServiceTemplateAndArtifact_BuildZipSuccess()
        {
            // Arrange
            const string EntryDefinition = "tosca.yml";
            const string SomeYmlFileName = "someymlfile.yml";
            const string SomeArtifactFileName = "artifact.zip";

            var dto = new SlimMetadata
            {
                ToscaMetaFileVersion = new Version(1, 0),
                CSARVersion = new Version(0, 1),
                CreatedBy = "AutoGenerated"
            };

            var toscaServiceTemplate = new ToscaServiceTemplate { ToscaDefinitionsVersion = "tosca_simple_yaml_1_0" };

            var someFileData = new byte[] { 12, 35, 65, 1, 3, 65 };

            // Act
            var constructor = FluentCSAR.Config(
                builder =>
                    builder
                        .AddMetadata(dto)
                        .EntryDefinitions(EntryDefinition, toscaServiceTemplate)
                        .AddServiceTemplate(SomeYmlFileName, toscaServiceTemplate)
                        .AddArtifact(SomeArtifactFileName, someFileData));

            var result = constructor.BuildZip();

            // Assert
            result.Should().NotBeEmpty();
        }

        [Test]
        public void CreatingToscaCSARFluently_NonValid_ThrowsValidationError()
        {
            // Arrange
            const string EntryDefinition = "tosca.yml";

            var dto = new SlimMetadata
            {
                ToscaMetaFileVersion = new Version(1, 0),
                CSARVersion = new Version(0, 1),
                CreatedBy = "AutoGenerated"
            };

            var toscaServiceTemplate = new ToscaServiceTemplate(); // Non valid template - tosca definition version is missing

            // Act
            var constructor = FluentCSAR.Config(
                builder =>
                    builder
                        .AddMetadata(dto)
                        .EntryDefinitions(EntryDefinition, toscaServiceTemplate));

            Action action = () => constructor.BuildZip();

            // Assert
            action.ShouldThrow<ToscaValidationException>();
        }

        [Test]
        public void CreatingToscaCSARFluently_NoEntryDefinitions_ThrowsValidationError()
        {
            // Arrange
            var dto = new SlimMetadata
            {
                ToscaMetaFileVersion = new Version(1, 0),
                CSARVersion = new Version(0, 1),
                CreatedBy = "AutoGenerated"
            };

            // Act
            var constructor = FluentCSAR.Config(
                builder =>
                    builder
                        .AddMetadata(dto));

            Action action = () => constructor.BuildZip();

            // Assert
            action.ShouldThrow<ToscaValidationException>();
        }
    }
}

