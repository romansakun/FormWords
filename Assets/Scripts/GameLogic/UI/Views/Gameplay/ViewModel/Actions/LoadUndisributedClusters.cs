using Cysharp.Threading.Tasks;
using GameLogic.Bootstrapper;
using Infrastructure.Extensions;
using Zenject;

namespace GameLogic.UI.Gameplay
{
    public class LoadUndistributedClusters : BaseGameplayViewModelAction
    {
        [Inject] private Cluster.Factory _clusterFactory;

        public override async UniTask ExecuteAsync(GameplayViewModelContext context)
        {
            context.UndistributedClustersHolder.DestroyChildren();
            var levelProgress = context.LevelProgress;
            for (var i = 0; i < levelProgress.UndistributedClusters.Count; i++)
            {
                var clusterText = levelProgress.UndistributedClusters[i];

                var cluster = _clusterFactory.Create();
                cluster.SetText(clusterText);
                cluster.SetParent(context.UndistributedClustersHolder);
                cluster.SetColorAlpha(1);
                context.UndistributedClusters.Add(cluster);

                context.AllClusters.Add(cluster);
            }
            await UniTask.Yield();
        }

    }
}