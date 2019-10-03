using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace DefaultNamespace
{
    public class CarBehaviour : MonoBehaviour, IConvertGameObjectToEntity
    {
        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            dstManager.AddComponent(entity, typeof(MoveForward));

            var moveSpeed = new MoveSpeed();
            dstManager.AddComponentData(entity, moveSpeed);
            
            var helper = new PathHelper { CurrentTargetIndex = 1, IsReachEnd = false };
            dstManager.AddComponentData(entity, helper);
        }
    }
}