using System;
using Unity.Entities;

[Serializable]
public struct AsPriority : IComponentData
{
    public bool Value;
}