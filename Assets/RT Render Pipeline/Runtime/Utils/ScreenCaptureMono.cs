using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class ScreenCaptureMono : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F4))
        {
            ScreenCapture.isScreenCapture = true;
            Debug.Log("button press!");
        }
    }
}
