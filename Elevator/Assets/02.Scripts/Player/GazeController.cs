using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Ä«¸Þ¶ó ½Ã¼± = Ray
// NPC ¾ó±¼ È÷Æ® ½Ã ¸àÅ» Áõ°¡
public class GazeController : MonoBehaviour
{
    ChildNPC currentChild;
    public float gazeDistance = 5f;

    void Update()
    {
        PlayerController.Instance.SetState(PlayerState.Looking);

        Vector3 origin = Camera.main.transform.position
               + Camera.main.transform.up * -0.2f;

        Ray ray = new Ray(origin, Camera.main.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, gazeDistance))
        {
            ChildNPC child = hit.collider.GetComponentInParent<ChildNPC>();

            if (child != currentChild)
            {
                ResetCurrent();
                currentChild = child;
            }

            if (child != null)
            {
                child.OnGazed(Time.deltaTime);
            }
            else
            {
                ResetCurrent();
            }
        }
        else
        {
            ResetCurrent();
        }

        Debug.DrawRay(ray.origin, ray.direction * gazeDistance, Color.red);
    }

    void ResetCurrent()
    {
        if (currentChild != null)
            currentChild.ResetGaze();

        currentChild = null;
    }
}
