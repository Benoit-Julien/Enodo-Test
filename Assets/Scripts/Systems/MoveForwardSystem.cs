using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class MoveForwardSystem : JobComponentSystem
{
    [BurstCompile]
    [RequireComponentTag(typeof(MoveForward))]
    private struct MoveForwardRotation : IJobForEach<Translation, Rotation, MoveSpeed>
    {
        public float dt;
        
        public void Execute(ref Translation pos, ref Rotation rot, ref MoveSpeed speed)
        {
            pos.Value = pos.Value + (dt * speed.CurrentSpeed * math.forward(rot.Value));
        }
    }
    
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var moveForwardRotationJob = new MoveForwardRotation
        {
            dt = Time.deltaTime
        };

        return moveForwardRotationJob.Schedule(this, inputDeps);
    }
}