using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 엘리베이터 전체 흐름 제어
/// - 층 도착
/// - 문 열림 / 닫힘
/// - NPC 승하차
/// </summary>
public class ElevatorController : MonoBehaviour
{
    public static ElevatorController Instance;

    public enum ElevatorState
    {
        Idle,
        Boarding,     // NPC 탑승 중
        Moving,       // 층 이동 중
        Unboarding    // NPC 하차 중
    }

    public ElevatorState CurrentState { get; private set; }
    public bool IsNavMeshPossible { get; private set; }
    [SerializeField] NavMeshObstacle[] doorObstacles;

    [Header("Elevator Look Target")]
    public Transform elevatorLookTarget; // Box004

    public bool IsDoorOpen { get; private set; }
    
    [Header("NPC Spawn")]
    public Transform npcSpawnPoint; // 엘리베이터 외부
    
    [Header("NPC Exit")]
    public Transform npcExitPoint; // 엘리베이터 밖 (문 앞)

    [Header("Stand Points Inside Elevator")]
    public Transform[] standPoints;

    [Header("Timings")]
    public float enterDelay = 1.5f;
    public float npcInterval = 0.4f;
    public float stayDuration = 2f;

    [Header("Settings")]
    public int maxNPCCount = 4;

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

        // 빈 자리 초기화
        availablePoints = new List<Transform>(standPoints);

        StartCoroutine(ElevatorLoop());
    }

    // =========================
    // 엘리베이터 메인 루프
    // =========================

    IEnumerator ElevatorLoop()
    {
        while (true)
        {
            // ===== 층 도착 =====
            CurrentState = ElevatorState.Unboarding;
            OpenDoor();

            yield return StartCoroutine(ExitNPCs());

            yield return new WaitForSeconds(enterDelay);

            // ===== NPC 탑승 =====
            CurrentState = ElevatorState.Boarding;

            yield return StartCoroutine(EnterNPCs());

            yield return new WaitForSeconds(stayDuration);

            CloseDoor();

            // ===== 이동 =====
            CurrentState = ElevatorState.Moving;
            yield return new WaitForSeconds(10f);

            FloorManager.Instance.MoveToNextFloor();
        }
    }

    // =========================
    // NPC 하차
    // =========================

    IEnumerator ExitNPCs()
    {
        if (currentNPCs.Count == 0)
            yield break;

        int completed = 0;

        foreach (var npc in currentNPCs)
        {
            npc.OnExitCompleted += OnNPCExitCompleted;
            npc.ExitElevator(npcExitPoint); // 실제 하차 명령
        }

        void OnNPCExitCompleted(NPCController npc)
        {
            npc.OnExitCompleted -= OnNPCExitCompleted;
            NPCSpawner.Instance.Despawn(npc);
            completed++;
        }

        // 전원 하차 대기
        yield return new WaitUntil(() => completed >= currentNPCs.Count);

        currentNPCs.Clear();
    }

    // =========================
    // NPC 승차
    // =========================

    IEnumerator EnterNPCs()
    {
        FloorData floor = FloorManager.Instance.CurrentFloor;

        int count = Random.Range(floor.minNPCCount, floor.maxNPCCount + 1);

        HashSet<NPCType> usedTypes = new HashSet<NPCType>();

        for (int i = 0; i < count; i++)
        {
            Transform standPoint = GetAvailablePoint();
            if (!standPoint) break;

            NPCController npc =
                NPCSpawner.Instance.GetRandomNPCExcludeTypes(usedTypes);

            if (!npc) break;

            // 사용된 타입 기록
            usedTypes.Add(npc.npcType);

            // 외부 스폰 위치
            npc.transform.position = npcSpawnPoint.position;

            npc.EnterElevator(standPoint);
            currentNPCs.Add(npc);

            yield return new WaitForSeconds(npcInterval);
        }
    }

    // =========================
    // 빈 자리 배정
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
    // 문 제어
    // =========================
    void OpenDoor()
    {
        ani.SetBool("isOpen", true);
        IsDoorOpen = true;
        IsNavMeshPossible = false;
    }

    void CloseDoor()
    {
        ani.SetBool("isOpen", false);
        IsDoorOpen = false;
        IsNavMeshPossible = false;

        foreach (var obs in doorObstacles)
        {
            if (obs != null)
                obs.enabled = true;
        }
    }

    // 문이 "완전히" 열렸을 때
    public void OnDoorFullyOpened()
    {
        IsDoorOpen = true;

        foreach (var obs in doorObstacles)
        {
            if (obs != null)
                obs.enabled = false;
        }

        IsNavMeshPossible = true;
        CurrentState = ElevatorState.Unboarding;
    }
}
