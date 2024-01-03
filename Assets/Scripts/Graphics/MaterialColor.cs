using System;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEditor.SceneManagement;

[Serializable]
[MaterialProperty("_Color")]
public struct MaterialColor : IComponentData
{
    public float4 Value;
}

[DisallowMultipleComponent]
public class MaterialColorAuthoring : MonoBehaviour
{
    public Color color;
}

public class MaterialColorBaker : Baker<MaterialColorAuthoring>
{
    public override void Bake(MaterialColorAuthoring authoring)
    {
        Color linearCol = authoring.color.linear;
        var data = new MaterialColor { Value = new float4(linearCol.r, linearCol.g, linearCol.b, linearCol.a) };
        var entity = GetEntity(TransformUsageFlags.Renderable);
        AddComponent(entity, data);
    }
}
