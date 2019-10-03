using Unity.Entities;

[UpdateAfter(typeof(MoveForwardSystem))]
public class DeleteFinishEntitySystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        var deletedEntity = 0;
        
        Entities.ForEach((Entity entity, ref PathHelper finishPath) => {
            if (finishPath.IsReachEnd)
            {
                PostUpdateCommands.DestroyEntity(entity);
                RoadsManager.Instance.Cars.Dequeue();
                deletedEntity++;
            }
        });
        
        Entities.ForEach((Entity entity, ref PathHelper finishPath) => {
            finishPath.CurrentIndexInRoad -= deletedEntity;
        });
    }
}