using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 노인 NPC
/// - 이상행동 중 플레이어 시선 강제
/// - 쳐다보면 행동 중단 + 민망함 증가
/// - 시선을 피하면 일정 시간 후 다시 이상행동
/// </summary>
public class ElderNPC : BaseNPC
{
    enum State
    {
        Idle,       // 아무 행동 안 함
        Acting      // 이상행동 중
    }
    State state = State.Idle;
    bool hasArrived = false;

    Animator ani;
    Coroutine behaviorRoutine;

    float calmDelayMin = 5f;
    float calmDelayMax = 10f;

    public override bool IsActing => state == State.Acting;
    public override bool CanInteract => hasArrived;

    void Start()
    {
        ani = GetComponent<Animator>();
    }

    /// <summary>
    /// 엘리베이터 자리 도착
    /// </summary>
    public override void OnArrivedInElevator()
    {
        if (hasArrived) return;
        hasArrived = true;

        behaviorRoutine = StartCoroutine(BehaviorLoop());
    }

    /// <summary>
    /// 평온 → 이상행동 반복 루프
    /// </summary>
    IEnumerator BehaviorLoop()
    {
        while (true)
        {
            // 평온 시간
            yield return new WaitForSeconds(Random.Range(calmDelayMin, calmDelayMax));
            StartWeirdAction();

            // 행동이 끝날 때까지 대기
            yield return new WaitUntil(() => state == State.Idle);
        }
    }

    /// <summary>
    /// 이상행동 시작
    /// </summary>
    void StartWeirdAction()
    {
        if (!hasArrived || state == State.Acting)
            return;

        state = State.Acting;

        PlayRandomAction();

        // 이상행동 중 시선 강제
        PlayerController.Instance.ForceLookAt(transform);
    }

    /// <summary>
    /// 플레이어가 노인을 쳐다보고 있을 때 호출됨
    /// </summary>
    public override void OnGazed(float deltaTime)
    {
        if (!CanInteract || state != State.Acting) return;

        // 민망함 증가
        PlayerController.Instance.AddAwkward(deltaTime * 1.5f);
        StopWeirdAction();
    }

    public override void ResetGaze()
    {
        // 시선을 피했을 때는 아무것도 안 함
        // → BehaviorLoop가 다시 행동을 시작시킴
    }

    /// <summary>
    /// 이상행동 중단
    /// </summary>
    void StopWeirdAction()
    {
        state = State.Idle;
        PlayerController.Instance.ReleaseForceLook();
        //StopActionAnimation();
    }

    /// <summary>
    /// 6가지 중 랜덤 행동 실행
    /// </summary>
    void PlayRandomAction()
    {
        int index = Random.Range(0, 5); // Action0 ~ Action5
        ani.SetTrigger("Action" + index);
    }

    void StopActionAnimation()
    {
        // Trigger 기반이면 굳이 Reset 안 해도 됨
        // 필요하면 여기서 Idle 상태용 파라미터 처리
    }
}