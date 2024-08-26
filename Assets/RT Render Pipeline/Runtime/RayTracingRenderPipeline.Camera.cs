using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        private static void SetupCamera(Camera camera)
        {
            var projMatrix = GL.GetGPUProjectionMatrix(camera.projectionMatrix, false);
            var viewMatrix = camera.worldToCameraMatrix;
            var viewProjMatrix = projMatrix * viewMatrix;
            var invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
            Shader.SetGlobalMatrix(CameraShaderParams._InvCameraViewProj, invViewProjMatrix);
            Shader.SetGlobalFloat(CameraShaderParams._CameraFarDistance, camera.farClipPlane);
            Shader.SetGlobalVector(CameraShaderParams._WorldSpaceCameraPos, camera.transform.position);
        }
        private static void SetupCamera(Camera camera, Matrix4x4 stereoProjMatrix)
        {
            var projMatrix = GL.GetGPUProjectionMatrix(stereoProjMatrix, false);
            var viewMatrix = camera.worldToCameraMatrix;
            var viewProjMatrix = projMatrix * viewMatrix;
            var invViewProjMatrix = Matrix4x4.Inverse(viewProjMatrix);
            Shader.SetGlobalMatrix(CameraShaderParams._InvCameraViewProj, invViewProjMatrix);
            Shader.SetGlobalFloat(CameraShaderParams._CameraFarDistance, camera.farClipPlane);
            Shader.SetGlobalVector(CameraShaderParams._WorldSpaceCameraPos, camera.transform.position);
        }
    }
}