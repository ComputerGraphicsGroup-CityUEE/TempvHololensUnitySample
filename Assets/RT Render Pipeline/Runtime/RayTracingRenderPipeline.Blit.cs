using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        public void FinalBlitPass(CommandBuffer cmd, RTHandle source)
        {
            using (new ProfilingScope(cmd, new ProfilingSampler("Final Blit")))
            { 
                cmd.Blit(source, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
            }
        }
        public void FinalBlitPass(CommandBuffer cmd, RenderTexture source)
        {
            using (new ProfilingScope(cmd, new ProfilingSampler("Final Blit")))
            {
                cmd.Blit(source, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
            }
        }
        public void FinalBlitPass(CommandBuffer cmd, Texture2D source)
        {
            using (new ProfilingScope(cmd, new ProfilingSampler("Final Blit")))
            {
                cmd.Blit(source, BuiltinRenderTextureType.CameraTarget, Vector2.one, Vector2.zero);
            }
        }
    }
}