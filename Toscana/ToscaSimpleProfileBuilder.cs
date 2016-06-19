﻿using System.Collections.Generic;
using Toscana.Exceptions;

namespace Toscana
{
    public interface IToscaSimpleProfileBuilder
    {
        IToscaSimpleProfileBuilder Append(ToscaSimpleProfile toscaSimpleProfile);
        IToscaSimpleProfileBuilder Append(string toscaAsString);
        ToscaSimpleProfile Build();
    }

    public class ToscaSimpleProfileBuilder : IToscaSimpleProfileBuilder
    {
        private readonly List<ToscaSimpleProfile> toscaSimpleProfiles = new List<ToscaSimpleProfile>();

        public IToscaSimpleProfileBuilder Append(string toscaAsString)
        {
            toscaSimpleProfiles.Add(ToscaSimpleProfile.Parse(toscaAsString));
            return this;
        }

        public IToscaSimpleProfileBuilder Append(ToscaSimpleProfile toscaSimpleProfile)
        {
            toscaSimpleProfiles.Add(toscaSimpleProfile);
            return this;
        }

        public ToscaSimpleProfile Build()
        {
            var combinedTosca = new ToscaSimpleProfile();
            CombineNodeTypes(combinedTosca);
            BuildNodeTypeHierarchy(combinedTosca);
            return combinedTosca;
        }

        private void CombineNodeTypes(ToscaSimpleProfile combinedTosca)
        {
            foreach (var simpleProfile in toscaSimpleProfiles)
            {
                foreach (var nodeType in simpleProfile.NodeTypes)
                {
                    if (combinedTosca.NodeTypes.ContainsKey(nodeType.Key))
                    {
                        throw new ToscanaValidationException(string.Format("Node type {0} is duplicate", nodeType.Key));
                    }
                    combinedTosca.NodeTypes.Add(nodeType.Key, nodeType.Value);
                }
            }
        }

        private static void BuildNodeTypeHierarchy(ToscaSimpleProfile combinedTosca)
        {
            foreach (var nodeType in combinedTosca.NodeTypes)
            {
                for (var baseNodeType = nodeType.Value.DerivedFrom;
                    !string.IsNullOrEmpty(baseNodeType);
                    baseNodeType = combinedTosca.NodeTypes[baseNodeType].DerivedFrom)
                {
                    if (!combinedTosca.NodeTypes.ContainsKey(baseNodeType))
                    {
                        throw new ToscanaValidationException(string.Format("Definition of Node Type {0} is missing",
                            baseNodeType));
                    }

                    foreach (var capability in combinedTosca.NodeTypes[baseNodeType].Capabilities)
                    {
                        if (nodeType.Value.Capabilities.ContainsKey(capability.Key))
                        {
                            throw new ToscanaValidationException(string.Format("Duplicate capability definition of capability {0}", capability.Key));
                        }
                        nodeType.Value.Capabilities.Add(capability.Key, capability.Value);
                    }
                }
            }
        }
    }
}