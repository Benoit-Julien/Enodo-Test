using System;
using Unity.Entities;

[Serializable]
public struct MoveSpeed : IComponentData
{
    public float OriginalSpeed;
    public float CurrentSpeed;
    public float SafeDistance;
}