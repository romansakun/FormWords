namespace GameLogic.UI.Gameplay
{
    public class OnEndDragUndistributedCluster : BaseGameplayViewModelQualifier
    {
        public override float Score(GameplayViewModelContext context)
        {
            if (context.Input.Type != UserInputType.OnEndDrag)
                return 0;

            if (context.Swipe.DraggedCluster == null)
                return 0;

            if (context.UndistributedClusters.Contains(context.Swipe.OriginDraggedCluster) == false)
                return 0;

            context.Input = default;
            return 1;
        }
    }
}