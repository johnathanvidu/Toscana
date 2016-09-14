using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using Toscana.Exceptions;
using Toscana.Fluent.Models;

namespace Toscana.Fluent
{
    /// <summary>
    /// Part of the Tosca Cloud Service Archive builder responsible for collecting service templates and artifacts
    /// </summary>
    public interface IToscaCSARBuilder
    {
        /// <summary>
        /// Collects one tosca service template (yaml)
        /// </summary>
        /// <param name="serviceTemplateName">Tosca service template name</param>
        /// <param name="serviceTemplate">A configured service template</param>
        /// <returns>The same builder for fluent construction</returns>
        IToscaCSARBuilder AddServiceTemplate(string serviceTemplateName, ToscaServiceTemplate serviceTemplate);
        /// <summary>
        /// Collects one artifact
        /// </summary>
        /// <param name="artifactRelativePath">Artifact relative path</param>
        /// <param name="artifactData">Binary representation of one artifact</param>
        /// <returns>The same builder for fluent construction</returns>
        IToscaCSARBuilder AddArtifact(string artifactRelativePath, byte[] artifactData);
    }

    /// <summary>
    /// Part of the Tosca Cloud Service Archive builder. This part builds some of the fields related to TOSCA.meta files
    /// </summary>
    public interface IToscaCSARMetadataBuilder
    {
        /// <summary>
        /// Adds some metadata required by TOSCA.meta file 
        /// </summary>
        /// <param name="slimMetadata">The several fields</param>
        /// <returns>The same builder for fluent construction</returns>
        IToscaCSAREntryDefinitionBuilder AddMetadata(SlimMetadata slimMetadata);
    }

    /// <summary>
    /// Represents the fluent syntax of setting entry definition section required by TOSCA.meta file
    /// </summary>
    public interface IToscaCSAREntryDefinitionBuilder
    {
        /// <summary>
        /// Sets the entry definition section required by TOSCA.meta file
        /// </summary>
        /// <param name="entryDefinitions">The entry definition file name</param>
        /// <param name="toscaServiceTemplate">The entry tosca service template matching the entry definition</param>
        /// <returns>Fluent syntax which allows creating new CSAR</returns>
        IToscaCSARBuilder EntryDefinitions(string entryDefinitions, ToscaServiceTemplate toscaServiceTemplate);
    }

    internal class ToscaCSARBuilder : IToscaCSARMetadataBuilder, IToscaCSAREntryDefinitionBuilder, IToscaCSARBuilder
    {
        private readonly ToscaMetadata toscaMetadata;
        private readonly ICollection<KeyValuePair<string, ToscaServiceTemplate>> toscaServiceTemplates;
        private readonly ICollection<KeyValuePair<string, byte[]>> artifacts;

        public ToscaCSARBuilder()
        {
            toscaMetadata = new ToscaMetadata();
            toscaServiceTemplates = new List<KeyValuePair<string, ToscaServiceTemplate>>();
            artifacts = new List<KeyValuePair<string, byte[]>>();
        }

        /// <summary>
        /// Adds some metadata required by TOSCA.meta file 
        /// </summary>
        /// <param name="slimMetadata">The several fields</param>
        /// <returns>The same builder for fluent construction</returns>
        public IToscaCSAREntryDefinitionBuilder AddMetadata(SlimMetadata slimMetadata)
        {
            toscaMetadata.ToscaMetaFileVersion = slimMetadata.ToscaMetaFileVersion;
            toscaMetadata.CsarVersion = slimMetadata.CSARVersion;
            toscaMetadata.CreatedBy = slimMetadata.CreatedBy;
            return this;
        }

        /// <summary>
        /// Sets the entry definition section required by TOSCA.meta file
        /// </summary>
        /// <param name="entryDefinitions">The entry definition file name</param>
        /// <param name="toscaServiceTemplate">The entry tosca service template matching the entry definition</param>
        /// <returns>Fluent syntax which allows creating new CSAR</returns>
        public IToscaCSARBuilder EntryDefinitions(string entryDefinitions, ToscaServiceTemplate toscaServiceTemplate)
        {
            AddServiceTemplate(entryDefinitions, toscaServiceTemplate);
            SetEntryDefinition(entryDefinitions);
            return this;
        }

        /// <summary>
        /// Collects one tosca service template (yaml)
        /// </summary>
        /// <param name="serviceTemplateName">Tosca service template name</param>
        /// <param name="serviceTemplate">A configured service template</param>
        /// <returns>The same builder for fluent construction</returns>
        public IToscaCSARBuilder AddServiceTemplate(string serviceTemplateName, ToscaServiceTemplate serviceTemplate)
        {
            toscaServiceTemplates.Add(Pair(serviceTemplateName, serviceTemplate));
            return this;
        }

        /// <summary>
        /// Collects one artifact
        /// </summary>
        /// <param name="artifactRelativePath">Artifact relative path</param>
        /// <param name="artifactData">Binary representation of one artifact</param>
        /// <returns>The same builder for fluent construction</returns>
        public IToscaCSARBuilder AddArtifact(string artifactRelativePath, byte[] artifactData)
        {
            artifacts.Add(Pair(artifactRelativePath, artifactData));
            return this;
        }

        /// <summary>
        /// Builds a zip file from collected configuration and saves it in memory
        /// </summary>
        /// <returns>Tosca Cloud Service Archive as a zip file in memory</returns>
        public byte[] BuildPreConfigured()
        {
            var csar = BuildCsarPreConfigured();
            
            using (var memoryStream = new MemoryStream())
            {
                csar.Save(memoryStream);
                return memoryStream.ToArray();
            }
        }

        /// <summary>
        /// Builds Tosca Cloud Service Archive from collected configuration
        /// </summary>
        /// <returns>Configured Tosca Cloud Service Archive</returns>
        public ToscaCloudServiceArchive BuildCsarPreConfigured()
        {
            var csar = new ToscaCloudServiceArchive(toscaMetadata);
            SetServiceTemplates(csar);
            SetArtifacts(csar);
            // Validate Tosca Cloud Service Archive
            ValidateCsar(csar);
            return csar;
        }

        private static void ValidateCsar(ToscaCloudServiceArchive csar)
        {
            List<ValidationResult> validationResults;
            if (!csar.TryValidate(out validationResults)) throw new ToscaValidationException(CreateErrorMessageFromValidationResults(validationResults));
            if (string.IsNullOrEmpty(csar.EntryDefinitions)) // May be unnecessary in the future
                throw new ToscaValidationException("Entry definitions is missing. Please use EntryDefinitions(string, ToscaServiceTemplate) in your fluent syntax.");
        }

        private static string CreateErrorMessageFromValidationResults(List<ValidationResult> validationResults)
        {
            return string.Join(Environment.NewLine, validationResults.Select(result => result.ErrorMessage));
        }

        private void SetArtifacts(ToscaCloudServiceArchive csar)
        {
            AddTo(artifacts, csar.AddArtifact);
        }

        private void SetServiceTemplates(ToscaCloudServiceArchive csar)
        {
            AddTo(toscaServiceTemplates, csar.AddToscaServiceTemplate);
        }

        /// <summary>
        /// Sets the entry definition section required by TOSCA.meta file
        /// </summary>
        /// <param name="entryDefinition">The entry definition file name</param>
        private void SetEntryDefinition(string entryDefinition)
        {
            toscaMetadata.EntryDefinitions = entryDefinition;
        }

        #region Helper methods

        private static KeyValuePair<TKey, TValue> Pair<TKey, TValue>(TKey key, TValue value)
        {
            return new KeyValuePair<TKey, TValue>(key, value);
        }

        private static void AddTo<TKey, TValue>(IEnumerable<KeyValuePair<TKey, TValue>> collection, Action<TKey, TValue> addToAction)
        {
            foreach (var pair in collection)
            {
                var key = pair.Key;
                var value = pair.Value;
                addToAction(key, value);
            }
        }

        #endregion
    }
}