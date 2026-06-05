using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasPixelPerfectSync : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;

    private Camera uiCamera;

    void Awake()
    {
        uiCamera = GetComponent<Camera>();
    }

    void Update()
    {
        // sync the UI Camera's viewport with the pixel perfect viewport of Main Camera
        uiCamera.rect = mainCamera.rect;
    }
}