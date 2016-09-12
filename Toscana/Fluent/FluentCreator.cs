using System;

namespace Toscana.Fluent
{
    /// <summary>
    /// Represents the fluent syntax of setting entry definition section required by TOSCA.meta file
    /// </summary>
    public interface IFluentCSARSetEntryDefinition
    {
        /// <summary>
        /// Sets the entry definition section required by TOSCA.meta file
        /// </summary>
        /// <param name="entryDefinition">The entry definition file name</param>
        /// <returns>Fluent syntax which allows creating new CSAR</returns>
        IFluentCSARConstruction SetEntryDefinition(string entryDefinition);
    }

    /// <summary>
    /// Represents the fluent syntax of creating new Tosca Cloud Service Archive
    /// </summary>
    public interface IFluentCSARConstruction
    {
        /// <summary>
        /// Creates zip file from fluent configuration and saves it in memory
        /// </summary>
        /// <returns>Binary representation of configured Tosca Cloud Service Archive zip file</returns>
        byte[] BuildZip();

        /// <summary>
        /// Creates configured Tosca Cloud Service Archive 
        /// </summary>
        /// <returns>Configured Tosca Cloud Service Archive</returns>
        ToscaCloudServiceArchive BuildCSAR();
    }

    /// <summary>
    /// Entry point for creating Tosca Cloud Service Archive using fluent syntax
    /// </summary>
    public class FluentCSAR
    {
        private readonly ToscaCSARBuilder csarToscaCsarBuilder;

        private FluentCSAR()
        {
            csarToscaCsarBuilder = new ToscaCSARBuilder();
        }

        /// <summary>
        /// Start configuring how Tosca Cloud Service Archive will be created
        /// </summary>
        /// <param name="metadataBuilder">TOSCA.meta file builder that is part of the Tosca Cloud Service Archive</param>
        /// <returns>Fluent syntax of creating new Tosca Cloud Service Archive</returns>
        public static IFluentCSARSetEntryDefinition Config(Action<IToscaCSARMetadataBuilder> metadataBuilder)
        {
            return new FluentCSAR().InternalConfig(metadataBuilder);
        }

        private IFluentCSARSetEntryDefinition InternalConfig(Action<IToscaCSARMetadataBuilder> metadataBuilder)
        {
            metadataBuilder(csarToscaCsarBuilder);
            return new FluentCsarFluentCsarSetEntryDefinition(csarToscaCsarBuilder);
        }
    }

    internal class FluentCsarFluentCsarSetEntryDefinition : IFluentCSARSetEntryDefinition
    {
        private readonly ToscaCSARBuilder toscaCsarBuilder;

        public FluentCsarFluentCsarSetEntryDefinition(ToscaCSARBuilder toscaCsarBuilder)
        {
            this.toscaCsarBuilder = toscaCsarBuilder;
        }
        public IFluentCSARConstruction SetEntryDefinition(string entryDefinition)
        {
            toscaCsarBuilder.SetEntryDefinition(entryDefinition);
            return new FluentCSARConstruction(toscaCsarBuilder);
        }
    }

    internal class FluentCSARConstruction : IFluentCSARConstruction
    {
        private readonly ToscaCSARBuilder toscaCsarBuilder;

        public FluentCSARConstruction(ToscaCSARBuilder toscaCsarBuilder)
        {
            this.toscaCsarBuilder = toscaCsarBuilder;
        }

        /// <summary>
        /// Creates zip file from fluent configuration and saves it in memory
        /// </summary>
        /// <returns>Binary representation of configured Tosca Cloud Service Archive zip file</returns>
        public byte[] BuildZip()
        {
            return toscaCsarBuilder.BuildPreConfigured();
        }

        /// <summary>
        /// Creates configured Tosca Cloud Service Archive 
        /// </summary>
        /// <returns>Configured Tosca Cloud Service Archive</returns>
        public ToscaCloudServiceArchive BuildCSAR()
        {
            return toscaCsarBuilder.BuildCsarPreConfigured();
        }
    }
}