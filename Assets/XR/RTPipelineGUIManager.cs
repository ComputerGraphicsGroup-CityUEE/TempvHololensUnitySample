using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class RTPipelineGUIManager : MonoBehaviour
{
    [SerializeField] RayTracingRenderPipelineAsset rayTracingRenderPipelineAsset;

    [Header("Mode")]
    [SerializeField] private Dropdown modeDropdown;
    [SerializeField] private Toggle indirectToggle;
    [SerializeField] private Toggle denoiseToggle;
    [SerializeField] GameObject DenoiseGO;

    [Header("Model")]
    [SerializeField] private Dropdown modelDropdown;
    [SerializeField] private Slider roughnessSlider;
    [SerializeField] private Slider metallicSlider;
    [SerializeField] GameObject animationGO;

    public Material foxMat;
    public Material planeMat;

    float fox_metallic;
    float fox_roughness;
    float plane_metallic;
    float plane_roughness;

    float perTimeMatChange;

    int currentModel;

    void Start()
    {
        fox_metallic = 0f;
        fox_roughness = 1f;
        plane_roughness = 0.05f;
        plane_metallic = 0.35f;

        foxMat.SetFloat("_Metallic", fox_metallic);
        foxMat.SetFloat("_Roughness", fox_roughness);
        planeMat.SetFloat("_Metallic", plane_metallic);
        planeMat.SetFloat("_Roughness", plane_roughness);

        currentModel = 0;
        roughnessSlider.value = fox_roughness;
        metallicSlider.value = fox_metallic;

        perTimeMatChange = 0.05f;
    }

    public void SetRendermode()
    {
        int modeValue = modeDropdown.value;
        if (modeValue == 0)
        { 
            rayTracingRenderPipelineAsset.RenderMode = RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.VSAT;
            DenoiseGO.SetActive(false);
        }
        else if (modeValue == 1)
        { 
            rayTracingRenderPipelineAsset.RenderMode = RayTracingRenderPipelineAsset.RTRenderSetting.RTRenderMode.MIS;
            DenoiseGO.SetActive(true);
        }
    }

    public void SetIndirect() 
    {
        bool isSetIndirect = indirectToggle.isOn;

        if (isSetIndirect)
            rayTracingRenderPipelineAsset.EnableIndirect = true;
        else
            rayTracingRenderPipelineAsset.EnableIndirect = false;
    }

    public void SetDenoise()
    {
        bool isSetDenoise = denoiseToggle.isOn;

        if (isSetDenoise) 
            RayTracingRenderPipelineAsset.EnableDenoise = true;
        else
            RayTracingRenderPipelineAsset.EnableDenoise = false;
    }

    public void SetModel()
    {
        int modelValue = modelDropdown.value;
        // fox
        if (modelValue == 0)
        {
            currentModel = 0;
            animationGO.SetActive(true);
            roughnessSlider.value = fox_roughness;
            metallicSlider.value = fox_metallic;
        }
        // plane
        else if (modelValue == 1)
        {
            currentModel = 1;
            animationGO.SetActive(false);
            roughnessSlider.value = plane_roughness;
            metallicSlider.value = plane_metallic;
        }
    }

    public void OnMetallicValueChanged()
    {
        // set fox
        if (currentModel == 0)
        {
            fox_metallic = metallicSlider.value;
            foxMat.SetFloat("_Metallic", fox_metallic);
        }
        // set plane
        else if (currentModel == 1)
        {
            plane_metallic = metallicSlider.value;
            planeMat.SetFloat("_Metallic", plane_metallic);
        }
    }

    public void OnRoughnessValueChanged()
    {
        // set fox
        if (currentModel == 0)
        {
            fox_roughness = roughnessSlider.value;
            foxMat.SetFloat("_Roughness", fox_roughness);
        }
        // set plane
        else if (currentModel == 1)
        {
            plane_roughness = roughnessSlider.value;
            planeMat.SetFloat("_Roughness", plane_roughness);
        }
    }

    public void EnableAnimation()
    {
        SceneManager.isPlayAnimation = true;
    }

    public void DisableAnimation()
    {
        SceneManager.isPlayAnimation = false;
    }

}
