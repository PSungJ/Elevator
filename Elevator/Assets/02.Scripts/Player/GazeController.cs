using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ä«¸Þ¶ó ½Ã¼± = Ray
// NPC ¾ó±¼ È÷Æ® ½Ã ¸àÅ» Áõ°¡
public class GazeController : MonoBehaviour
{
    ChildNPC currentNPC;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 5f))
        {
            ChildNPC npc = hit.collider.GetComponentInParent<ChildNPC>();

            if (npc != null)
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
