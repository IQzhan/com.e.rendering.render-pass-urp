using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using E.Rendering;

[VolumeComponentMenu("Custom/CaptureCamera")]
public class CaptureCamera : CustomRenderPassComponent
{
    // Resources data
    protected override CustomRenderPassComponentData Data => CaptureCameraData.Instance;

    // Parameters here like
    public ClampedIntParameter size = new ClampedIntParameter(0, 0, 10);

    // private Material material;

    protected override void Initialize()
    {
        // if (material == null && CaptureCameraData.Instance.shader != null)
        //     material = CoreUtils.CreateEngineMaterial(CaptureCameraData.Instance);        
    }
    
    public override bool IsTileCompatible()
    {
        return false;
    }

    public override bool IsActive()
    {
        //return size.Value > 0 && material != null;
        return size.value > 0;
    }

    public override void OnCameraSetup(ref RenderingData renderingData)
    {
        // CommandBuffer cmd = Command;
    }

    public override void Render(ref RenderingData renderingData)
    {
        // CommandBuffer cmd = Command;
        // ScriptableRenderContext context = Context;
        Debug.Log(nameof(CaptureCamera));
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

    protected override void DisposedManaged()
    {
        
    }
}