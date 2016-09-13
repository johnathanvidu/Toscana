using System;

namespace Toscana.Fluent
{
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
        private readonly ToscaCSARBuilder toscaCsarBuilder;

        private FluentCSAR()
        {
            toscaCsarBuilder = new ToscaCSARBuilder();
        }

        /// <summary>
        /// Start configuring how Tosca Cloud Service Archive will be created
        /// </summary>
        /// <param name="metadataBuilder">TOSCA.meta file builder that is part of the Tosca Cloud Service Archive</param>
        /// <returns>Fluent syntax of creating new Tosca Cloud Service Archive</returns>
        public static IFluentCSARConstruction Config(Action<IToscaCSARMetadataBuilder> metadataBuilder)
        {
            return new FluentCSAR().InternalConfig(metadataBuilder);
        }

        private IFluentCSARConstruction InternalConfig(Action<IToscaCSARMetadataBuilder> metadataBuilder)
        {
            metadataBuilder(toscaCsarBuilder);
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