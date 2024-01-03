using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Rendering;
using UnityEngine.Rendering;

public class MaterialChanger : IComponentData
{
    public Material mat0;
    public Material mat1;
    public uint frequency;
    public uint frame;
    public uint active;

}

[DisallowMultipleComponent]
public class MaterialChangerAuthoring : MonoBehaviour
{
    public Material mat0;
    public Material mat1;
    [RegisterBinding(typeof(MaterialChanger), "frequency")]
    public uint frequency;
    [RegisterBinding(typeof(MaterialChanger), "frame")]
    public uint frame;
    [RegisterBinding(typeof(MaterialChanger), "active")]
    public uint active;

    class MaterialChangerBaker : Baker<MaterialChangerAuthoring>
    {
        public override void Bake(MaterialChangerAuthoring authoring)
        {
            MaterialChanger component = new MaterialChanger();
            component.mat0 = authoring.mat0;
            component.mat1 = authoring.mat1;
            component.frequency = authoring.frequency;
            component.frame = authoring.frame;
            component.active = authoring.active;
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponentObject(entity, component);
        }
    }
}

[RequireMatchingQueriesForUpdate]
public partial class MaterialChangerSystem : SystemBase
{
    private Dictionary<Material, BatchMaterialID> m_MaterialMapping;

    private void RegisterMaterial(EntitiesGraphicsSystem hybridRendererSystem, Material material)
    {
        if (!m_MaterialMapping.ContainsKey(material))
        {
            var id = hybridRendererSystem.RegisterMaterial(material);
            m_MaterialMapping[material] = id;
            Debug.LogError("id; ===> " + id);
            Debug.LogError("m_MaterialMapping ===> " + m_MaterialMapping.Count);
        }
    }

    private void UnregisterMaterial()
    {
        var hybridRenderer = World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
        if (hybridRenderer == null)
        {
            return;
        }

        foreach (var kv in m_MaterialMapping)
        {
            hybridRenderer.UnregisterMaterial(kv.Value);
        }
    }

    protected override void OnStartRunning()
    {
       var hybridRenderer = World.GetOrCreateSystemManaged<EntitiesGraphicsSystem>();
       m_MaterialMapping = new Dictionary<Material, BatchMaterialID>();
       Entities.WithoutBurst().ForEach((in MaterialChanger changer) =>
       {
           RegisterMaterial(hybridRenderer, changer.mat0);
           RegisterMaterial(hybridRenderer, changer.mat1);
       }).Run();
    }

    protected override void OnUpdate()
    {
        EntityManager entityManager = EntityManager;
        
        Entities
            .WithoutBurst()
            .ForEach((MaterialChanger changer, ref MaterialMeshInfo mmi) =>
            {
                changer.frame = changer.frame + 1;

                if (changer.frame >= changer.frequency)
                {
                    changer.frame = 0;
                    changer.active = changer.active == 0 ? 1u : 0u;
                    var material = changer.active == 0 ? changer.mat0 : changer.mat1;
                    mmi.MaterialID = m_MaterialMapping[material];
                }
            }).Run();
    }
}
