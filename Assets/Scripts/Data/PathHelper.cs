using System;
using Unity.Entities;

[Serializable]
public struct PathHelper : IComponentData
{
    public int CurrentTargetIndex;
    public bool IsReachEnd;

    public int CurrentIndexInRoad;
}