using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using static ElevatorController;
public enum NPCType
{
    Child,
    Elder,
    Adult
}
/// <summary>
/// NPC 이동 / 정착 / 시선 제어 담당
/// 개별 NPC 기믹 로직은 BaseNPC 클래스에서 처리
/// </summary>
public class NPCController : MonoBehaviour
{
    public NPCType npcType;
    public bool IsSettled { get; private set; }   // 엘리베이터 내부 정착 여부
    public bool HasArrived => hasArrived;         // 목적지 도착 여부 (외부 참조용)
    public System.Action<NPCController> OnExitCompleted;

    [Header("Look Direction")]
    public Transform defaultLookTarget; // 엘리베이터 정면 (문 방향)

    enum LookState
    {
        None,
        Default,   // 정면
        Player     // 플레이어
    }

    public enum NPCState
    {
        Idle,
        Boarding,
        Riding,      // 기믹 ON
        Unboarding,  // 기믹 OFF
        Exited
    }

    public NPCState CurrentState { get; private set; }
    LookState lookState = LookState.None;

    NavMeshAgent agent;
    Animator ani;
    Transform player;
    BaseNPC npcLogic;
    GimmickNPC gimmickNPC;

    bool hasArrived = false;
    bool isEnteringElevator = false;
    
    // 이동 시작 여부 감지용
    bool hasEverMoved = false;

    // 애니메이션 안정화용
    bool isWalking;
    const float WALK_START = 0.15f;
    const float WALK_STOP = 0.05f;

    float rotateSpeed = 5f;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ani = GetComponent<Animator>();
        npcLogic = GetComponent<BaseNPC>();
        gimmickNPC = GetComponent<GimmickNPC>();
        player = Camera.main.transform;
        if (defaultLookTarget == null && ElevatorController.Instance != null)
        {
            defaultLookTarget = ElevatorController.Instance.elevatorLookTarget;
        }
    }

    /// <summary>
    /// 엘리베이터 정면 LookTarget 설정
    /// </summary>
    public void SetDefaultLookTarget(Transform target)
    {
        defaultLookTarget = target;
    }

    /// <summary>
    /// 엘리베이터 안 목적지로 이동 시작
    /// </summary>
    public void EnterElevator(Transform standPoint)
    {
        hasArrived = false;
        IsSettled = false;
        hasEverMoved = false;
        isEnteringElevator = true;

        lookState = LookState.None;
        PlayerController.Instance.ReleaseForceLook();

        StartCoroutine(EnterAfterDoorOpen(standPoint));
    }

    /// <summary>
    /// 층 이동 시 NPC 하차 처리
    /// </summary>
    public void ExitElevator(Transform exitPoint)
    {
        hasArrived = false;
        IsSettled = false;
        isEnteringElevator = false;

        CurrentState = NPCState.Unboarding;
        lookState = LookState.None;

        npcLogic?.OnRideEnd();   // ← 노인 NPC 포함 강제 종료 핵심
        PlayerController.Instance.ReleaseForceLook();

        StartCoroutine(ExitRoutine(exitPoint));
    }

    IEnumerator EnterAfterDoorOpen(Transform standPoint)
    {
        // 문 열릴 때까지 대기
        yield return new WaitUntil(() => ElevatorController.Instance.IsDoorOpen);

        var agent = GetComponent<NavMeshAgent>();

        agent.enabled = true;
        yield return null; // NavMesh 인식 대기

        if (!agent.isOnNavMesh)
        {
            Debug.LogError("NPC not on NavMesh at spawn point");
            yield break;
        }

        agent.isStopped = false;
        agent.SetDestination(standPoint.position);

        agent.isStopped = false;
        agent.SetDestination(standPoint.position);
    }

    void Update()
    {
        UpdateWalkAnimation();

        // 실제 이동 시작 감지
        if (isEnteringElevator && !hasEverMoved &&
            agent.velocity.sqrMagnitude > 0.01f)
        {
            hasEverMoved = true;
        }

        // 도착 판정
        if (isEnteringElevator && hasEverMoved && !hasArrived && HasReached())
        {
            OnReachedStandPoint();
        }

        UpdateLook();
    }

    // =========================
    // 도착 처리
    // =========================

    bool HasReached()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;
        if (agent.velocity.sqrMagnitude > 0.01f) return false;

        return true;
    }

    void OnReachedStandPoint()
    {
        hasArrived = true;
        isEnteringElevator = false;

        agent.isStopped = true;
        IsSettled = true;

        CurrentState = NPCState.Riding;
        lookState = LookState.Default;

        npcLogic?.OnRideStart(); // 여기서만 기믹 시작
        gimmickNPC?.OnArrivedInElevator();
    }

    IEnumerator ExitRoutine(Transform exitPoint)
    {
        // 문 + NavMesh 대기
        yield return new WaitUntil(() =>
            ElevatorController.Instance.CurrentState == ElevatorState.Unboarding &&
            ElevatorController.Instance.IsNavMeshPossible
        );

        agent.enabled = true;
        yield return null;

        agent.isStopped = false;
        agent.SetDestination(exitPoint.position);

        bool hasStartedMoving = false;
        float failSafeTimer = 0f;
        const float FAIL_SAFE_TIME = 3f;

        while (true)
        {
            failSafeTimer += Time.deltaTime;

            // 이동 시작 감지
            if (!hasStartedMoving && agent.velocity.sqrMagnitude > 0.01f)
                hasStartedMoving = true;

            // 정상 도착
            if (hasStartedMoving &&
                !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance &&
                agent.velocity.sqrMagnitude < 0.01f)
            {
                break;
            }

            // 이동 실패 → 강제 종료
            if (failSafeTimer >= FAIL_SAFE_TIME)
            {
                Debug.LogWarning($"[NPC Exit] Fail-safe triggered: {name}");
                break;
            }

            yield return null;
        }

        OnExitCompleted?.Invoke(this);
    }

    // =========================
    // 애니메이션 안정화
    // =========================

    void UpdateWalkAnimation()
    {
        float speed = agent.velocity.magnitude;

        if (!isWalking && speed > WALK_START)
            isWalking = true;
        else if (isWalking && speed < WALK_STOP)
            isWalking = false;

        ani.SetBool("isWalk", isWalking);
    }

    // =========================
    // 시선 처리
    // =========================

    void UpdateLook()
    {
        switch (lookState)
        {
            case LookState.Default:
                RotateToward(defaultLookTarget);
                break;

            case LookState.Player:
                RotateToward(player);
                break;
        }
    }

    void RotateToward(Transform target)
    {
        if (target == null) return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotateSpeed
        );
    }

    public void SetLookPlayer(bool lookPlayer)
    {
        lookState = lookPlayer ? LookState.Player : LookState.Default;
    }

    public bool IsLookingPlayer
    {
        get { return lookState == LookState.Player; }
    }
}
