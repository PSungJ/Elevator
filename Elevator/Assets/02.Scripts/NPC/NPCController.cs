using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class NPCController : MonoBehaviour
{
    public Transform elevatorTarget;
    NavMeshAgent agent;
    Animator ani;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        ani = GetComponent<Animator>();
    }

    public void EnterElevator()
    {
        agent.isStopped = false;
        agent.SetDestination(elevatorTarget.position);
    }

    void Update()
    {
        ani.SetBool("isWalk", agent.velocity.magnitude > 0.1f);

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
        }
    }

    void OnArrived()
    {
        // 이후 상태 전환 (서있기, 쳐다보기 등)
    }
}
