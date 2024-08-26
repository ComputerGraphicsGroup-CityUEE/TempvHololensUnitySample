using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class Fps : MonoBehaviour
{
    public Text fpsText;

    private int count;
    private float deltaTime;

    private void Update()
    {
        count++;
        deltaTime += Time.deltaTime;

        if (count % 60 == 0)
        {
            count = 1;
            var fps = 60f / deltaTime;
            deltaTime = 0;
            fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
        }
    }
}