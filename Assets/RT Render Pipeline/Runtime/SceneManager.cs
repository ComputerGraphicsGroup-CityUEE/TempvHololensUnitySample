using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
public class SceneManager : MonoBehaviour
{
    public GameObject[] tempVecObjs;        // the objects that is used to calculate the visbility
    public static bool isPlayAnimation = false;
    public static bool onClickAddFrame = false;
    public static int checkFrame = 0;
    [System.NonSerialized]
    public bool isDirty = true;

    [SerializeField] RayTracingRenderPipelineAsset rayTracingRenderPipelineAsset;

    const int maxNumSubMeshes = 32;
    private bool[] subMeshFlagArray = new bool[maxNumSubMeshes];
    private bool[] subMeshCutoffArray = new bool[maxNumSubMeshes];

    private static SceneManager s_Instance;

    public static SceneManager Instance
    {
        get
        {
            if (s_Instance != null) return s_Instance;

            s_Instance = GameObject.FindObjectOfType<SceneManager>();
            s_Instance?.Init();
            return s_Instance;
        }
    }


    public void Awake()
    {
        if (Application.isPlaying)
            DontDestroyOnLoad(this);

        isDirty = true;
        RayTracingResources.Instance.IsProgramRunning = true;

        isPlayAnimation = true;
        rayTracingRenderPipelineAsset.RenderMode = RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.VSAT;
        rayTracingRenderPipelineAsset.EnableIndirect = true;
        RayTracingRenderPipelineAsset.enableDenoise = false;
    }

    public void OnDisable()
    {
        isPlayAnimation = false;
    }

    public void Update()
    {
        // Switch render mode
        if (Input.GetKeyDown(KeyCode.F1))
        {
            if (rayTracingRenderPipelineAsset.RtRenderSetting.renderMode == RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.MIS)
                rayTracingRenderPipelineAsset.RenderMode = RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.VSAT;
            else
                rayTracingRenderPipelineAsset.RenderMode = RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.MIS;
        }

        // Switch direct/indirect
        if (Input.GetKeyDown(KeyCode.F2))
        {
            if (rayTracingRenderPipelineAsset.RtRenderSetting.EnableIndirect == true)
                    rayTracingRenderPipelineAsset.EnableIndirect = false;
                else
                    rayTracingRenderPipelineAsset.EnableIndirect = true;
        }

        // Switch Animation
        if (Input.GetKeyDown(KeyCode.F3))
        {
            if (isPlayAnimation)
            {
                isPlayAnimation = false;
                checkFrame = -1;
            }
            else
                isPlayAnimation = true;
        }

        // Enable / disable denoiser
        if (Input.GetKeyDown(KeyCode.F4) && rayTracingRenderPipelineAsset.RenderMode == RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.MIS)
        {
            RayTracingRenderPipelineAsset.EnableDenoise = RayTracingRenderPipelineAsset.EnableDenoise ? false : true;
        }


        // Exit
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("F5 button press!");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }

    public void FillAccelerationStructure(ref RayTracingAccelerationStructure accelerationStructure, ref GameObject mergeGO)
    {
        Renderer m_renderer = mergeGO.GetComponent<Renderer>();
        if (m_renderer)
        {
            accelerationStructure.AddInstance(m_renderer, subMeshFlagArray, subMeshCutoffArray);
        }
    }

    public void UpdateAccelerationStructure(ref RayTracingAccelerationStructure accelerationStructure, ref GameObject mergeGO)
    {
        Renderer m_renderer = mergeGO.GetComponent<Renderer>();
        if (m_renderer)
        {
            accelerationStructure.UpdateInstanceTransform(m_renderer);
        }
    }

    private void Init()
    {
        for (var i = 0; i < maxNumSubMeshes; ++i)
        {
            subMeshFlagArray[i] = true;
            subMeshCutoffArray[i] = false;
        }
    }
}