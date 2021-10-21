using UnityEngine;
using UnityEngine.Rendering;

namespace E.Rendering
{
    public partial class CustomRenderPass
    {

        internal void SafeBlit(in CommandBuffer cmd,
            RenderTargetIdentifier sources, RenderTargetIdentifier destination,
            in Material material = null, in int pass = 0)
        {
            if (destination == currentTargetID)
            {
                if (sources == destination)
                {
                    destination = currentTargetID =
                        currentTargetID == m_TempColorTargetID ? m_ColorTargetID : m_TempColorTargetID;
                }
                else if (IsFinalComponent())
                {
                    destination = currentTargetID = m_ColorTargetID;
                }
            }
            cmd.SetRenderTarget(destination);
            cmd.Blit(sources, destination, material, pass);
        }

        internal void DrawRenderers(in CullingResults cullingResults, ref DrawingSettings drawingSettings, ref FilteringSettings filteringSettings)
        {
            using (new ProfilingScope(command, m_InsideProfilingSampler))
            {
                context.ExecuteCommandBuffer(command);
                command.Clear();
                context.DrawRenderers(cullingResults, ref drawingSettings, ref filteringSettings);
            }
        }
    }
}