using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(MoveForwardSystem))]
public class FollowPathSystem : JobComponentSystem
{
    [RequireComponentTag(typeof(MoveForward))]
    private struct FollowPath : IJobForEach<Translation, Rotation, PathHelper>
    {
        public void Execute(ref Translation pos, ref Rotation rot, ref PathHelper helper)
        {
            if (helper.IsReachEnd) return;
            
            var target = RoadsManager.Instance.Path[helper.CurrentTargetIndex];
            
            if (checkReachTile(ref pos, ref rot, ref target))
            {
                pos.Value = new float3(RoadsManager.Instance.Path[helper.CurrentTargetIndex].x,
                                       pos.Value.y,
                                       RoadsManager.Instance.Path[helper.CurrentTargetIndex].y);

                helper.CurrentTargetIndex++;
                if (helper.CurrentTargetIndex == RoadsManager.Instance.Path.Length)
                {
                    helper.IsReachEnd = true;
                    return;
                }
                
                target = RoadsManager.Instance.Path[helper.CurrentTargetIndex];
                RotateToTarget(ref pos, ref rot, ref target);
            }
        }

        [BurstCompile]
        private void RotateToTarget(ref Translation pos, ref Rotation rot, ref int2 target)
        {
            var heading = new float3(target.x, 0, target.y) - pos.Value;
            heading.y = 0;
            rot.Value = quaternion.LookRotation(heading, math.up());
        }
        
        [BurstCompile]
        private bool checkReachTile(ref Translation pos, ref Rotation rot, ref int2 target)
        {
            var v = math.forward(rot.Value);

            if (v.x > 0)
                return pos.Value.x >= target.x; 
            if (v.x < 0)
                return pos.Value.x <= target.x;
            if (v.z > 0)
                return pos.Value.z >= target.y;
            if (v.z < 0)
                return pos.Value.z <= target.y;

            return false;
        }
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var followPath = new FollowPath();
        return followPath.Schedule(this, inputDeps);
    }
}