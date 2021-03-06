using System;
using System.Collections.Generic;
using Toscana.Exceptions;

namespace Toscana
{
    /// <summary>
    /// A Data Type definition defines the schema for new named datatypes in TOSCA. 
    /// </summary>
    public class ToscaDataTypeDefinition : ToscaObject<ToscaDataTypeDefinition>
    {
        /// <summary>
        /// Initializes an instance of <see cref="ToscaDataTypeDefinition"/>
        /// </summary>
        public ToscaDataTypeDefinition()
        {
            Properties = new Dictionary<string, ToscaPropertyDefinition>();
        }

        /// <summary>
        /// An optional version for the Data Type definition.
        /// </summary>
        public Version Version { get; set; }

        /// <summary>
        /// The optional description for the Data Type.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The optional list of sequenced constraint clauses for the Data Type. 
        /// </summary>
        public List<Dictionary<string, object>> Constraints { get; set; }

        /// <summary>
        /// The optional list property definitions that comprise the schema for a complex Data Type in TOSCA.
        /// </summary>
        public Dictionary<string, ToscaPropertyDefinition> Properties { get; set; }

        /// <summary>
        /// Returns an entity that this entity derives from.
        /// If this entity is root, null will be returned
        /// If this entity derives from a non existing entity exception will be thrown
        /// </summary>
        /// <exception cref="ToscaDataTypeNotFoundException" accessor="get">When derived from data type not found in the Cloud Service Archive.</exception>
        public override ToscaDataTypeDefinition Base
        {
            get
            {
                if (CloudServiceArchive == null || IsRoot()) return null;
                ToscaDataTypeDefinition datatype;
                if (CloudServiceArchive.DataTypes.TryGetValue(DerivedFrom, out datatype))
                {
                    return datatype;
                }
                throw new ToscaDataTypeNotFoundException(string.Format("Data type '{0}' not found", DerivedFrom));
            }
        }

        /// <summary>
        /// Sets DerivedFrom to point to root if it's not set
        /// </summary>
        /// <param name="name">Object name</param>
        public override void SetDerivedFromToRoot(string name)
        {
            if (name != ToscaDefaults.ToscaDataTypeRoot && IsRoot())
            {
                DerivedFrom = ToscaDefaults.ToscaDataTypeRoot;
            }
        }
    }
}