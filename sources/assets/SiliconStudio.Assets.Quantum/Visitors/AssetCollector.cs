// Copyright (c) 2011-2017 Silicon Studio Corp. All rights reserved. (https://www.siliconstudio.co.jp)
// See LICENSE.md for full license information.
using System;
using System.Collections.Generic;
using SiliconStudio.Core.Reflection;
using SiliconStudio.Quantum;
using SiliconStudio.Quantum.References;

namespace SiliconStudio.Assets.Quantum.Visitors
{
    /// <summary>
    /// A visitor that collects all assets from an object containing multiple assets
    /// </summary>
    public class AssetCollector : GraphVisitorBase
    {
        private readonly Dictionary<GraphNodePath, Asset> assets = new Dictionary<GraphNodePath, Asset>();

        private AssetCollector()
        {
            
        }

        /// <summary>
        /// Collects all assets referenced by the object in the given node.
        /// </summary>
        /// <param name="root">The root node to visit to collect assets.</param>
        /// <returns>A collection containing all assets found by visiting the given root.</returns>
        public static IReadOnlyDictionary<GraphNodePath, Asset> Collect(IObjectNode root)
        {
            var visitor = new AssetCollector();
            visitor.Visit(root);
            return visitor.assets;
        }

        /// <inheritdoc/>
        protected override void VisitReference(IGraphNode referencer, ObjectReference reference)
        {
            var asset = reference.TargetNode.Retrieve() as Asset;
            if (asset != null)
            {
                assets.Add(CurrentPath.Clone(), asset);
            }
            // Don't continue the visit once we found an asset, we cannot have nested assets.
            else
            {
                base.VisitReference(referencer, reference);
            }
        }
    }
}