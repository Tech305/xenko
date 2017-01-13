using System.Linq;
using System.Threading.Tasks;
using SiliconStudio.Assets;
using SiliconStudio.Assets.Compiler;
using SiliconStudio.BuildEngine;
using SiliconStudio.Core;
using SiliconStudio.Core.Extensions;
using SiliconStudio.Core.Serialization;
using SiliconStudio.Core.Serialization.Contents;
using SiliconStudio.Xenko.Engine;
using SiliconStudio.Xenko.Extensions;

namespace SiliconStudio.Xenko.Assets.Entities
{
    public abstract class EntityHierarchyCompilerBase<T> : AssetCompilerBase where T : EntityHierarchyAssetBase
    {
        protected override void Compile(AssetCompilerContext context, AssetItem assetItem, string targetUrlInStorage, AssetCompilerResult result)
        {
            var asset = (T)assetItem.Asset;
            foreach (var entityData in asset.Hierarchy.Parts)
            {
                // TODO: How to make this code pluggable?
                var modelComponent = entityData.Entity.Components.Get<ModelComponent>();
                if (modelComponent != null)
                {
                    if (modelComponent.Model == null)
                    {
                        result.Warning($"The entity [{targetUrlInStorage}:{entityData.Entity.Name}] has a model component that does not reference any model.");
                    }
                    else
                    {
                        var modelAttachedReference = AttachedReferenceManager.GetAttachedReference(modelComponent.Model);
                        var modelId = modelAttachedReference.Id;

                        // compute the full path to the source asset.
                        var modelAssetItem = assetItem.Package.Session.FindAsset(modelId);
                        if (modelAssetItem == null)
                        {
                            result.Error($"The entity [{targetUrlInStorage}:{entityData.Entity.Name}] is referencing an unreachable model.");
                        }
                    }
                }

                var nodeLinkComponent = entityData.Entity.Components.Get<ModelNodeLinkComponent>();
                if (nodeLinkComponent != null)
                {
                    if (!nodeLinkComponent.IsValid)
                    {
                        result.Warning($"The Model Node Link between {entityData.Entity.Name} and {nodeLinkComponent.Target?.Entity.Name} is invalid.");
                        nodeLinkComponent.Target = null;
                    }
                }
            }

            result.BuildSteps = new AssetBuildStep(assetItem) { Create(targetUrlInStorage, asset) };
        }

        protected abstract EntityHierarchyCommandBase Create(string url, T assetParameters);

        protected abstract class EntityHierarchyCommandBase : AssetCommand<T>
        {
            protected EntityHierarchyCommandBase(string url, T parameters) : base(url, parameters)
            {
            }

            protected override Task<ResultStatus> DoCommandOverride(ICommandContext commandContext)
            {
                var assetManager = new ContentManager();

                var prefab = Create(Parameters);
                foreach (var rootEntity in Parameters.Hierarchy.RootPartIds)
                {
                    prefab.Entities.Add(Parameters.Hierarchy.Parts[rootEntity].Entity);
                }

                assetManager.Save(Url, prefab);

                return Task.FromResult(ResultStatus.Successful);
            }

            protected abstract PrefabBase Create(T prefabAsset);

            public override string ToString()
            {
                return "Prefab command for entity asset '{0}'.".ToFormat(Url);
            }
        }
    }
}
