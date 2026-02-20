using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ¿¤¸®º£ÀÌÅÍ ÀüÃ¼ Èå¸§ Á¦¾î
/// - Ãþ µµÂø
/// - ¹® ¿­¸² / ´ÝÈû
/// - NPC ½ÂÇÏÂ÷
/// </summary>
public class ElevatorController : MonoBehaviour
{
    public static ElevatorController Instance;

    public enum ElevatorState
    {
        Idle,
        Boarding,     // NPC Å¾½Â Áß
        Moving,       // Ãþ ÀÌµ¿ Áß
        Unboarding    // NPC ÇÏÂ÷ Áß
    }

    public ElevatorState CurrentState { get; private set; }
    public bool IsNavMeshPossible { get; private set; }
    public bool IsDoorOpen { get; private set; }
    [SerializeField] NavMeshObstacle[] doorObstacles;

    [Header("Elevator Look Target")]
    public Transform elevatorLookTarget; // Box004

    [Header("NPC Spawn / Exit")]
    public Transform npcSpawnPoint;
    public Transform npcExitPoint;

    [Header("Stand Points Inside Elevator")]
    public Transform[] standPoints;

    [Header("Timings")]
    public float enterDelay = 1.5f;
    public float npcInterval = 0.4f;
    public float stayDuration = 1f;

    [Header("Settings")]
    public int maxNPCCount = 4;

    [Header("Sound")]
    [SerializeField] AudioClip bellSound;
    [SerializeField] AudioClip elevatorSound;
    [SerializeField] AudioClip doorSound;

    Animator ani;

    List<Transform> availablePoints;
    List<NPCController> currentNPCs = new List<NPCController>();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ani = GetComponent<Animator>();
        SoundManager.Instance.PlayBGM(elevatorSound);

        // ºó ÀÚ¸® ÃÊ±âÈ­
        ResetStandPoints();

        StartCoroutine(ElevatorLoop());
    }

    // =========================
    // ¿¤¸®º£ÀÌÅÍ ¸ÞÀÎ ·çÇÁ
    // =========================

    IEnumerator ElevatorLoop()
    {
        while (true)
        {
            // ===== Ãþ µµÂø =====
            CurrentState = ElevatorState.Unboarding;
            StartCoroutine(OpenDoor());

            yield return ExitNPCs();

            ResetStandPoints();
            yield return new WaitForSeconds(enterDelay);

            // ===== NPC Å¾½Â =====
            CurrentState = ElevatorState.Boarding;
            yield return EnterNPCs();

            yield return new WaitForSeconds(stayDuration);

            // ===== ÀÌµ¿ =====
            StartCoroutine(CloseDoor());
            CurrentState = ElevatorState.Moving;
            yield return new WaitForSeconds(30f); // ´ÙÀ½ Ãþ ±îÁöÀÇ Ã¼·ù½Ã°£

            SoundManager.Instance.PlaySFX(bellSound);
            FloorManager.Instance.MoveToNextFloor();
        }
    }

    // =========================
    // NPC ÇÏÂ÷
    // =========================

    IEnumerator ExitNPCs()
    {
        if (currentNPCs.Count == 0)
            yield break;

        int completed = 0;
        float timeout = 6f;
        float timer = 0f;

        // ½º³À¼¦ »ç¿ë
        var exitingNPCs = new List<NPCController>(currentNPCs);

        Debug.Log($"[Elevator] Exit start count = {exitingNPCs.Count}");

        foreach (var npc in exitingNPCs)
        {
            if (npc == null) continue;

            npc.OnExitCompleted += OnNPCExitCompleted;
            npc.ExitElevator(npcExitPoint);
        }

        void OnNPCExitCompleted(NPCController npc)
        {
            npc.OnExitCompleted -= OnNPCExitCompleted;
            NPCSpawner.Instance.Despawn(npc);
            completed++;
        }

        yield return new WaitUntil(() =>
        {
            timer += Time.deltaTime;
            return completed >= exitingNPCs.Count || timer >= timeout;
        });

        if (timer >= timeout)
        {
            Debug.LogError($"[Elevator] Exit timeout! {completed}/{exitingNPCs.Count}");

            foreach (var npc in exitingNPCs)
            {
                if (npc != null)
                    NPCSpawner.Instance.Despawn(npc);
            }
        }

        currentNPCs.Clear();
    }

    // =========================
    // NPC ½ÂÂ÷
    // =========================

    IEnumerator EnterNPCs()
    {
        FloorData floor = FloorManager.Instance.CurrentFloor;
        int count = Random.Range(floor.minNPCCount, floor.maxNPCCount + 1);

        HashSet<NPCType> usedTypes = new();

        for (int i = 0; i < count; i++)
        {
            Transform standPoint = GetAvailablePoint();
            if (!standPoint)
                break;

            NPCController npc =
                NPCSpawner.Instance.GetRandomNPCExcludeTypes(usedTypes);

            if (!npc)
                break;

            usedTypes.Add(npc.npcType);

            npc.transform.position = npcSpawnPoint.position;
            npc.EnterElevator(standPoint);

            currentNPCs.Add(npc);
            yield return new WaitForSeconds(npcInterval);
        }
    }

    // =========================
    // ºó ÀÚ¸® ¹èÁ¤
    // =========================
    void ResetStandPoints()
    {
        availablePoints = new List<Transform>(standPoints);
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

    // =========================
    // ¹® Á¦¾î
    // =========================
    IEnumerator OpenDoor()
    {
        ani.SetBool("isOpen", true);
        IsDoorOpen = true;
        IsNavMeshPossible = false;

        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySFX(doorSound);
    }

    IEnumerator CloseDoor()
    {
        ani.SetBool("isOpen", false);
        IsDoorOpen = false;
        IsNavMeshPossible = false;

        foreach (var obs in doorObstacles)
        {
            if (obs) obs.enabled = true;
        }

        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySFX(doorSound);        
    }

    // ¹®ÀÌ "¿ÏÀüÈ÷" ¿­·ÈÀ» ¶§
    public void OnDoorFullyOpened()
    {
        foreach (var obs in doorObstacles)
        {
            if (obs) obs.enabled = false;
        }

        IsNavMeshPossible = true;
        IsDoorOpen = true;
        CurrentState = ElevatorState.Unboarding;
    }
}
