using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Denoising;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        public void RenderDenoise(ScriptableRenderContext context, CommandBuffer cmd, RTHandle src, ref Texture2D dstTexture)
        {

            using (new ProfilingScope(cmd, new ProfilingSampler("Denoising")))
            {
                denoiser.DenoiseRequest(cmd, "color", src);

                denoiserState = denoiser.WaitForCompletion(context, cmd);

                denoiserState = denoiser.GetResults(cmd, ref dstTexture);

            }
        }

        public void RenderDenoise(ScriptableRenderContext context, CommandBuffer cmd, RenderTexture src, ref Texture2D dstTexture)
        {
            using (new ProfilingScope(cmd, new ProfilingSampler("Denoising")))
            {
                denoiser.DenoiseRequest(cmd, "color", src);

                denoiserState = denoiser.WaitForCompletion(context, cmd);

                denoiserState = denoiser.GetResults(cmd, ref dstTexture);

            }
        }

        CommandBufferDenoiser InitDenoiser(int fixWidth, int fixHeight)
        {
            denoiser = new CommandBufferDenoiser();
            denoiserState = denoiser.Init(DenoiserType.Optix, fixWidth, fixHeight);

            return denoiser;
        }

        void DepositeDenoiser()
        {
            denoiser.DisposeDenoiser();
        }
    }
}