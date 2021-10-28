using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using E.Rendering;

[VolumeComponentMenu("Render Pass/Capture Cameras")]
public class CaptureCameras : CustomRenderPassComponent
{
    protected override CustomRenderPassComponentData Data => CaptureCameraData.Instance;

    protected override void Initialize()
    {
        //ªÒ»°gameloop
    }
    
    public override bool IsTileCompatible()
    {
        return false;
    }

    public override bool IsActive()
    {
        return true;
    }

    public override void OnCameraSetup(ref RenderingData renderingData)
    {
        // CommandBuffer cmd = Command;
    }

    public override void Render(ref RenderingData renderingData)
    {
        // CommandBuffer cmd = Command;
        // ScriptableRenderContext context = Context;
        //VirtualCamera virtualCamera = new VirtualCamera(renderingData.cameraData.camera);

        
    }

    public override void OnCameraCleanup()
    {
        // CommandBuffer cmd = Command;
    }

    protected override void DisposeUnmanaged()
    {

    }

    protected override void DisposeManaged()
    {
        
    }
}