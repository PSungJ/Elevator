using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ¹® ¿­¸²/´ÝÈû
// Èçµé¸² ¿¬Ãâ
public class ElevatorController : MonoBehaviour
{
    public static ElevatorController Instance;
    public bool IsDoorOpen { get; private set; }

    [Header("NPCs Outside Elevator")]
    public List<NPCController> npcList;

    [Header("Stand Points Inside Elevator")]
    public Transform[] standPoints;

    [Header("Timings")]
    public float enterDelay = 1.5f;
    public float npcInterval = 0.5f;

    [Header("Settings")]
    public int maxNPCCount = 4;

    Animator ani;
    List<Transform> availablePoints;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ani = GetComponent<Animator>();

        // ºó ÀÚ¸® ÃÊ±âÈ­
        availablePoints = new List<Transform>(standPoints);

        StartCoroutine(ElevatorArrivedSequence());
    }

    IEnumerator ElevatorArrivedSequence()
    {
        OpenDoor();
        yield return new WaitForSeconds(enterDelay);

        // ·£´ý NPC ¼±º°
        List<NPCController> selectedNPCs = GetRandomNPCs();

        foreach (var npc in selectedNPCs)
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

    // =========================
    // ·£´ý NPC ¼±ÅÃ
    // =========================
    List<NPCController> GetRandomNPCs()
    {
        List<NPCController> tempList = new List<NPCController>(npcList);
        List<NPCController> result = new List<NPCController>();

        int count = Mathf.Min(maxNPCCount, tempList.Count);

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, tempList.Count);
            result.Add(tempList[index]);
            tempList.RemoveAt(index);
        }

        return result;
    }

    // =========================
    // ºó ÀÚ¸® ¹èÁ¤
    // =========================
    Transform GetAvailablePoint()
    {
        if (availablePoints.Count == 0)
            return null;

        int index = Random.Range(0, availablePoints.Count);
        Transform point = availablePoints[index];
        availablePoints.RemoveAt(index);
        return point;
    }

    // =========================
    // ¹® Á¦¾î
    // =========================
    void OpenDoor()
    {
        ani.SetBool("isOpen", true);
        IsDoorOpen = true;
    }

    void CloseDoor()
    {
        ani.SetBool("isOpen", false);
        IsDoorOpen = false;
    }
}
