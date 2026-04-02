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
        Failure,      // 엘리베이터 고장
        Unboarding    // NPC 하차 중
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

    [Header("Failure Event")]
    [SerializeField] Light warningLight;     // 엘리베이터 경고 조명
    [SerializeField] Light elevatorLight;     // 엘리베이터 기본 조명
    [SerializeField] AudioClip alarmSound;    // 경보음
    [SerializeField] float failureDuration = 6f;

    int failureCount = 0;
    const int MAX_FAILURE = 2;

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

        // 빈 자리 초기화
        ResetStandPoints();

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
            StartCoroutine(OpenDoor());

            yield return ExitNPCs();
            // 마지막 층 체크
            if (FloorManager.Instance.IsLastFloor())
            {
                Debug.Log("GAME CLEAR");
                UIManager.Instance.ShowGameClear();
                UIManager.Instance.SetUICursor();
                yield break;
            }

            ResetStandPoints();
            yield return new WaitForSeconds(enterDelay);

            // ===== NPC 탑승 =====
            CurrentState = ElevatorState.Boarding;
            yield return EnterNPCs();

            yield return new WaitForSeconds(stayDuration);

            // ===== 이동 =====
            StartCoroutine(CloseDoor());
            CurrentState = ElevatorState.Moving;

            // 3층 이후부터 고장 이벤트 가능
            if (FloorManager.Instance.CurrentFloor.floorNumber >= 3 &&
                failureCount < MAX_FAILURE && Random.value < 0.35f)
            {
                yield return FailureRoutine();
            }

            yield return new WaitForSeconds(40f); // 다음 층 까지의 체류시간

            SoundManager.Instance.PlaySFX(bellSound);
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
        float timeout = 6f;
        float timer = 0f;

        // 스냅샷 사용
        var exitingNPCs = new List<NPCController>(currentNPCs);

        Debug.Log($"[Elevator] Exit start count = {exitingNPCs.Count}");

        foreach (var npc in exitingNPCs)
        {
            if (npc == null) continue;

            npc.OnExitCompleted += OnNPCExitCompleted;
            npc.ExitElevator(npcExitPoint);
            npc.ForceStopGimmick();
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
    // NPC 승차
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
    // 빈 자리 배정
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
    // 문 제어
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

    // 문이 "완전히" 열렸을 때
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

    // 고장 연출
    IEnumerator FailureRoutine()
    {
        failureCount++;
        CurrentState = ElevatorState.Failure;

        Debug.Log("ELEVATOR FAILURE START");

        // NPC 기믹 전부 중지
        foreach (var npc in currentNPCs)
        {
            npc.ForceStopGimmick();
            npc.SetLookPlayer(true);
        }

        // 경보음 + 빨간 깜빡임
        SoundManager.Instance.PlaySFX(alarmSound);
        StartCoroutine(BlinkRedLight());

        // 조명 OFF
        elevatorLight.enabled = false;
        warningLight.enabled = true;

        float timer = 0f;

        // 강제 민망함 상승
        while (timer < failureDuration)
        {
            PlayerController.Instance.AddAwkward(8f * Time.deltaTime);
            timer += Time.deltaTime;
            yield return null;
        }

        // 조명 복구
        elevatorLight.enabled = true;
        warningLight.enabled = false;

        foreach (var npc in currentNPCs)
            npc.SetLookPlayer(false);

        Debug.Log("ELEVATOR FAILURE END");

        // 연출 여유
        yield return new WaitForSeconds(2f);
    }

    IEnumerator BlinkRedLight()
    {
        while (true)
        {
            warningLight.enabled = !warningLight.enabled;
            yield return new WaitForSeconds(0.2f);
        }
    }
}
