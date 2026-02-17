using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ä«¸Þ¶ó ½Ã¼± = Ray
// NPC ¾ó±¼ È÷Æ® ½Ã ¸àÅ» Áõ°¡
public class GazeController : MonoBehaviour
{
    BaseNPC currentNPC;

    [Header("Gaze Settings")]
    public float gazeDistance = 5f;
    public float recoverPerSec = 1f;

    [Header("Debug")]
    public Color rayColor = Color.red;

    void Update()
    {
        bool gazingAnyNPC = false;

        if (Physics.Raycast(
            Camera.main.transform.position,
            Camera.main.transform.forward,
            out RaycastHit hit,
            gazeDistance))
        {
            BaseNPC npc = hit.collider.GetComponentInParent<BaseNPC>();

            if (npc != null && npc.CanInteract)
            {
                gazingAnyNPC = true;

                if (npc != currentNPC)
                {
                    currentNPC = npc;
                }

                npc.OnGazed(Time.deltaTime);
            }
        }

        if (!gazingAnyNPC && !PlayerController.Instance.IsUnderPressure)
        {
            PlayerController.Instance.RecoverAwkward(
                recoverPerSec * Time.deltaTime
            );
        }
    }
}
