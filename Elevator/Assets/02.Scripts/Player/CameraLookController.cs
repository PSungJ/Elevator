using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLookController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPivot;

    [Header("Angle Limits")]
    public float maxYaw = 25f;        // ÁÂ¿ì
    public float maxPitchUp = 10f;    // À§
    public float maxPitchDown = 15f;  // ¾Æ·¡

    [Header("Settings")]
    public float lookSpeed = 6f;
    public float deadZone = 0.04f;

    float currentYaw;
    float currentPitch;

    void Update()
    {
        Vector2 mouse = new Vector2(
             Input.mousePosition.x / Screen.width,
             Input.mousePosition.y / Screen.height
         );

        float offsetX = mouse.x - 0.5f;
        float offsetY = mouse.y - 0.5f;

        if (Mathf.Abs(offsetX) < deadZone) offsetX = 0f;
        if (Mathf.Abs(offsetY) < deadZone) offsetY = 0f;

        float targetYaw = offsetX * maxYaw;
        float targetPitch = -offsetY * maxPitchUp;

        targetPitch = Mathf.Clamp(
            targetPitch,
            -maxPitchDown,
            maxPitchUp
        );

        currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * lookSpeed);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * lookSpeed);

        cameraPivot.localRotation = Quaternion.Euler(
            currentPitch,
            currentYaw,
            0f
        );
    }
}
