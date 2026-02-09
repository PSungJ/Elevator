using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 카메라 시선 = Ray
// NPC 얼굴 히트 시 멘탈 증가
public class GazeController : MonoBehaviour
{
    BaseNPC currentNPC;

    [Header("Gaze Settings")]
    public float gazeDistance = 5f;

    [Header("Debug")]
    public Color rayColor = Color.red;

    void Update()
    {
        Camera cam = Camera.main;

        // 카메라 정면 기준 Ray
        Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);

        // 디버그용 Ray 표시 (Scene View)
        Debug.DrawRay(
            ray.origin,
            ray.direction * gazeDistance,
            rayColor
        );

        if (Physics.Raycast(ray, out RaycastHit hit, gazeDistance))
        {
            BaseNPC npc = hit.collider.GetComponentInParent<BaseNPC>();

            // NPC + 쳐다볼 수 있는 상태만 허용
            if (npc != null && npc.CanInteract)
            {
                if (npc != currentNPC)
                {
                    ResetCurrent();
                    currentNPC = npc;
                }

                npc.OnGazed(Time.deltaTime);
                return;
            }
        }

        ResetCurrent();
    }

    void ResetCurrent()
    {
        if (currentNPC != null)
        {
            currentNPC.ResetGaze();
            currentNPC = null;
        }
    }
}
