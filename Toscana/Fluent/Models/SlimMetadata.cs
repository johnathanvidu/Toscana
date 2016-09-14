using System;

namespace Toscana.Fluent.Models
{
    public struct SlimMetadata
    {
        public Version ToscaMetaFileVersion { get; set; }
        public Version CSARVersion { get; set; }
        public string CreatedBy { get; set; }
    }
}