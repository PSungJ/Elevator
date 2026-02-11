using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 엘리베이터 전체 흐름 제어
/// - 층 도착
/// - 문 열림 / 닫힘
/// - NPC 승하차
/// </summary>
public class ElevatorController : MonoBehaviour
{
    public static ElevatorController Instance;

    [Header("Elevator Look Target")]
    public Transform elevatorLookTarget; // Box004

    public bool IsDoorOpen { get; private set; }
    
    [Header("NPC Spawn")]
    public Transform[] npcSpawnPoint; // 엘리베이터 외부
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
            OpenDoor();

            // 기존 NPC 하차
            yield return StartCoroutine(ExitNPCs());

            // 약간의 여유
            yield return new WaitForSeconds(enterDelay);

            // 다음 층 NPC 생성 & 탑승
            yield return StartCoroutine(EnterNPCs());

            // 잠시 정차
            yield return new WaitForSeconds(stayDuration);

            // 문 닫기
            CloseDoor();

            // 층 이동 연출 대기
            yield return new WaitForSeconds(2f);

            // 다음 층으로
            FloorManager.Instance.MoveToNextFloor();
        }
    }

    // =========================
    // NPC 하차
    // =========================

    IEnumerator ExitNPCs()
    {
        foreach (var npc in currentNPCs)
        {
            npc.ExitElevator();

            // 풀링 시스템과 연결된다면 여기서 Despawn
            NPCSpawner.Instance.Despawn(npc);

            yield return new WaitForSeconds(0.25f);
        }

        currentNPCs.Clear();
        availablePoints = new List<Transform>(standPoints);
    }

    // =========================
    // NPC 승차
    // =========================

    IEnumerator EnterNPCs()
    {
        FloorData floor = FloorManager.Instance.CurrentFloor;

        int count = Random.Range(floor.minNPCCount, floor.maxNPCCount + 1);

        for (int i = 0; i < count; i++)
        {
            Transform standPoint = GetAvailablePoint();
            if (!standPoint) break;

            NPCController npc = NPCSpawner.Instance.GetRandomNPC();
            if (!npc) continue;

            // 1. 외부 Spawn 위치에 배치
            foreach (var sp in npcSpawnPoint)
            {
                npc.transform.position = sp.position;
            }

            // 2. 엘리베이터 탑승 시작
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
    }

    void CloseDoor()
    {
        ani.SetBool("isOpen", false);
        IsDoorOpen = false;
    }
}
