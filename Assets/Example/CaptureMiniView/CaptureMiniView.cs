using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using E.Rendering;


[VolumeComponentMenu("Custom/Capture Mini View")]
public class CaptureMiniView : CustomRenderPassComponent
{
    // Resources data
    protected override CustomRenderPassComponentData Data => CaptureMiniViewData.Instance;

    // Parameters here like
    // public ClampedIntParameter size = new ClampedIntParameter(0, 0, 10);

    // private Material material;

    protected override void Initialize()
    {
        // if (material == null && CaptureMiniViewData.Instance.shader != null)
        //     material = CoreUtils.CreateEngineMaterial(CaptureMiniViewData.Instance);        
    }
    
    public override bool IsTileCompatible()
    {
        return false;
    }

    public override bool IsActive()
    {
        //return size.Value > 0 && material != null;
        return false;
    }

    public override void OnCameraSetup(ref RenderingData renderingData)
    {
        // CommandBuffer cmd = Command;
    }

    public override void Render(ref RenderingData renderingData)
    {
        // CommandBuffer cmd = Command;
        // ScriptableRenderContext context = Context;
    }

    public override void OnCameraCleanup()
    {
        // CommandBuffer cmd = Command;
    }

    protected override void DisposeUnmanaged()
    {
        // if (material != null)
        //     CoreUtils.Destroy(material);
    }

    protected override void DisposeManaged()
    {
        
    }
}
