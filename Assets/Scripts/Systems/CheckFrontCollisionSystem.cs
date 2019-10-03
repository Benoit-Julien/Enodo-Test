using System.Linq;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(MoveForwardSystem))]
public class CheckFrontCollisionSystem : ComponentSystem
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity entity, ref Translation pos, ref Rotation rot, ref MoveSpeed speed, ref PathHelper helper) => {
            if (helper.CurrentIndexInRoad == 0) return;

            var manager = World.Active.EntityManager;
            var prevCar = RoadsManager.Instance.Cars.ElementAt(helper.CurrentIndexInRoad - 1);
            var previousCarPos = manager.GetComponentData<Translation>(prevCar);
            var distance = math.length(previousCarPos.Value - pos.Value);

            if (distance - speed.SafeDistance <= 0)
                speed.CurrentSpeed = manager.GetComponentData<MoveSpeed>(prevCar).CurrentSpeed;
            else
                speed.CurrentSpeed = speed.OriginalSpeed;
        });
    }
}