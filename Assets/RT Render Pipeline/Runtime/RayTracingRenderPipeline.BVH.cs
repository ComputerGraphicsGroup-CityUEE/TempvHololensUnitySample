using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

namespace Rendering.RayTrace
{
    public partial class RayTracingRenderPipeline
    {
        private void BuildAccelerationStructure(ref GameObject mergeGO)
        {
            // Do not build AS every frame
            // if (SceneManager.Instance == null || !SceneManager.Instance.isDirty) return;

            accelerationStructure.Dispose();
            accelerationStructure = new RayTracingAccelerationStructure();

            SceneManager.Instance.FillAccelerationStructure(ref accelerationStructure, ref mergeGO);

            accelerationStructure.Build();

            SceneManager.Instance.isDirty = false;
        }

        private void UpdateAccelerationStructure(ref GameObject mergeGO)
        {
            SceneManager.Instance.UpdateAccelerationStructure(ref accelerationStructure, ref mergeGO);
        }

        public RayTracingAccelerationStructure RequestAccelerationStructure()
        {
            return this.accelerationStructure;
        }
    }
}