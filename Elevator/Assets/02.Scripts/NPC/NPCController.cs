using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public bool IsSettled { get; private set; }

    [Header("Look Direction")]
    public Transform defaultLookTarget; // 엘리베이터 정면 (문 방향)

    enum LookState
    {
        None,
        Default,   // 정면
        Player     // 플레이어
    }

    LookState lookState = LookState.None;

    NavMeshAgent agent;
    Animator ani;
    Transform player;
    BaseNPC npcLogic;

    bool hasArrived = false;
    bool isEnteringElevator = false;
    bool hasStartedMoving = false;
    bool hasEverMoved = false;

    // 애니메이션 안정화용
    bool isWalking;
    const float WALK_START = 0.15f;
    const float WALK_STOP = 0.05f;

    float rotateSpeed = 5f;

    public bool HasArrived => hasArrived;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ani = GetComponent<Animator>();
        npcLogic = GetComponent<BaseNPC>();
        player = Camera.main.transform;
    }

    /// <summary>
    /// 엘리베이터 안 목적지로 이동 시작
    /// </summary>
    public void EnterElevator(Transform standPoint)
    {
        hasArrived = false;
        isWalking = false;
        isEnteringElevator = true;
        IsSettled = false;

        // ?? 이동 중 시선 강제 완전 차단
        lookState = LookState.None;
        PlayerController.Instance.ReleaseForceLook();

        StartCoroutine(EnterAfterDoorOpen(standPoint));
    }

    IEnumerator EnterAfterDoorOpen(Transform standPoint)
    {
        // 문 열릴 때까지 대기
        yield return new WaitUntil(() => ElevatorController.Instance.IsDoorOpen);

        agent.isStopped = false;
        agent.SetDestination(standPoint.position);
        hasStartedMoving = true;
    }

    void Update()
    {
        UpdateWalkAnimation();

        // 실제 이동 시작 감지
        if (hasStartedMoving && !hasEverMoved && agent.velocity.sqrMagnitude > 0.01f)
        {
            hasEverMoved = true;
        }

        if (isEnteringElevator && hasEverMoved && !hasArrived && CheckArrivedByAgent())
        {
            OnReachedStandPoint();
        }

        UpdateLook();
    }

    // =========================
    // 이동 / 도착 판정
    // =========================
    bool CheckArrivedByAgent() // 내부 전용 도착 판정 함수
    {
        if (agent.pathPending) return false;
        return agent.remainingDistance <= agent.stoppingDistance;
    }

    void OnReachedStandPoint()
    {
        hasArrived = true;
        isEnteringElevator = false;

        agent.isStopped = true;
        IsSettled = true;

        lookState = LookState.Default;

        npcLogic?.OnArrivedInElevator();
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
}
