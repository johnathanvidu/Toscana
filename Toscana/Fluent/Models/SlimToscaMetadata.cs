using System;

namespace Toscana.Fluent.Models
{
    /// <summary>
    /// Representation of stripped down ToscaMetadata object used for fluent syntax
    /// </summary>
    public struct SlimToscaMetadata
    {
        /// <summary>
        /// Specifies TOSCA.meta file version
        /// </summary>
        public Version ToscaMetaFileVersion { get; set; }
        /// <summary>
        /// Denotes the verison of CSAR
        /// Due to the simplified structure of the CSAR file and TOSCA.meta file compared to TOSCA 1.0, 
        /// the CSAR-Version keyword listed in block_0 of the meta-file is required to denote version 1.1.
        /// </summary>
        public Version CSARVersion { get; set; }
        /// <summary>
        /// Specifies who created the CSAR  
        /// </summary>
        public string CreatedBy { get; set; }
    }
}