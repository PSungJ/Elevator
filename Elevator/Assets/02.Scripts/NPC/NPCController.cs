using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    NavMeshAgent agent;
    Animator ani;
    Transform player;
    BaseNPC npcLogic;

    bool hasArrived = false;
    bool isLookingPlayer = false;

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
        player = Camera.main.transform;
    }

    /// <summary>
    /// 엘리베이터 안 목적지로 이동 시작
    /// </summary>
    public void EnterElevator(Transform standPoint)
    {
        hasArrived = false;
        isLookingPlayer = false;
        isWalking = false;

        // ?? 이동 중 시선 강제 완전 차단
        PlayerController.Instance.ReleaseForceLook();

        agent.isStopped = false;
        agent.SetDestination(standPoint.position);
    }

    void Update()
    {
        UpdateWalkAnimation();

        if (!hasArrived && HasArrived())
        {
            hasArrived = true;
            agent.isStopped = true;
            isLookingPlayer = true;

            npcLogic?.OnArrivedInElevator();
        }

        if (isLookingPlayer)
        {
            RotateTowardPlayer();
        }
    }

    // =========================
    // 이동 / 도착 판정
    // =========================

    bool HasArrived()
    {
        if (agent.pathPending) return false;
        if (agent.remainingDistance > agent.stoppingDistance) return false;
        if (agent.velocity.sqrMagnitude > 0.01f) return false;

        return true;
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
    // 플레이어 바라보기
    // =========================

    void RotateTowardPlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.01f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRot,
            Time.deltaTime * rotateSpeed
        );
    }
}
