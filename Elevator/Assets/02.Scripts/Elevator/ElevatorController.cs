using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// πÆ ø≠∏≤/¥›»˚
// »ÁµÈ∏≤ ø¨√‚
public class ElevatorController : MonoBehaviour
{
    [Header("NPCs Outside Elevator")]
    public List<NPCController> npcList;

    [Header("Stand Points Inside Elevator")]
    public Transform[] standPoints;

    [Header("Timings")]
    public float enterDelay = 1.5f;
    public float npcInterval = 0.5f;

    Animator ani;
    List<Transform> availablePoints;

    void Start()
    {
        ani = GetComponent<Animator>();

        // ∫Û ¿⁄∏Æ ∏Ò∑œ √ ±‚»≠
        availablePoints = new List<Transform>(standPoints);

        StartCoroutine(ElevatorArrivedSequence());
    }

    IEnumerator ElevatorArrivedSequence()
    {
        OpenDoor();
        yield return new WaitForSeconds(enterDelay);

        foreach (var npc in npcList)
        {
            Transform targetPoint = GetAvailablePoint();
            if (targetPoint == null)
                break;

            npc.EnterElevator(targetPoint);
            yield return new WaitForSeconds(npcInterval);
        }

        yield return new WaitForSeconds(2f);
        CloseDoor();
    }

    Transform GetAvailablePoint()
    {
        if (availablePoints.Count == 0)
            return null;

        int index = Random.Range(0, availablePoints.Count);
        Transform point = availablePoints[index];
        availablePoints.RemoveAt(index);
        return point;
    }

    void OpenDoor()
    {
        ani.SetBool("isOpen", true);
    }

    void CloseDoor()
    {
        ani.SetBool("isOpen", false);
    }
}
