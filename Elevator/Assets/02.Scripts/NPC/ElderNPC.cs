using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 노인 NPC
/// - 이상행동 중 시선 강제
/// - 3~5초 이상 바라보면 민망함 크게 증가
/// - 5초간 시선 강제 버티면 자동 해제
/// </summary>
public class ElderNPC : GimmickNPC
{
    [Header("Gaze Gauge UI")]
    public GazeGaugeUI gazeGaugeUI;

    enum State
    {
        Idle,
        Acting
    }

    State state = State.Idle;
    bool hasArrived = false;

    Animator ani;
    NPCController npcController;
    NavMeshAgent agent;

    // ===== 타이머 =====
    float gazeTimer = 0f;        // 플레이어가 바라본 시간
    float forceLookTimer = 0f;   // 강제 시선 유지 시간

    // ===== 설정값 =====
    float gazeRequiredTime;      // 시간 랜덤
    const float FORCE_LOOK_LIMIT = 5f;

    float calmDelayMin = 3f;
    float calmDelayMax = 5f;

    public override bool IsActing => state == State.Acting;
    public override bool CanGimmickInteract => isActive && state == State.Acting;

    Coroutine behaviorRoutine;

    void Start()
    {
        ani = GetComponent<Animator>();
        npcController = GetComponent<NPCController>();
        agent = GetComponent<NavMeshAgent>();
    }

    public override void OnRideStart()
    {
        base.OnRideStart();
        hasArrived = true;
    }

    public override void OnArrivedInElevator()
    {
        behaviorRoutine = StartCoroutine(BehaviorLoop());
    }

    public override void OnRideEnd()
    {
        base.OnRideEnd();

        // 행동 루프 완전 종료
        if (behaviorRoutine != null)
        {
            StopCoroutine(behaviorRoutine);
            behaviorRoutine = null;
        }

        // 상태 강제 종료
        state = State.Idle;

        // 강제 시선 해제
        PlayerController.Instance.ReleaseForceLook();

        // UI 정리
        if (gazeGaugeUI != null)
            gazeGaugeUI.Show(false);

        // 애니메이션 정리
        ani.SetBool("isWalk", false);

        // NPC 시선 원복
        npcController.SetLookPlayer(false);
    }

    IEnumerator BehaviorLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(calmDelayMin, calmDelayMax));
            StartWeirdAction();

            // Acting이 끝날 때까지 대기
            yield return new WaitWhile(() => state == State.Acting);
        }
    }

    void StartWeirdAction()
    {
        // 하차 중이거나 비활성 상태면 절대 시작 금지
        if (!isActive)
            return;
        if (npcController.CurrentState != NPCController.NPCState.Riding)
            return;
        if (state == State.Acting)
            return;

        state = State.Acting;

        // 타이머 초기화
        gazeTimer = 0f;
        forceLookTimer = 0f;
        gazeRequiredTime = Random.Range(2f, 3f);

        gazeGaugeUI.Show(true);
        gazeGaugeUI.SetGauge(0f);

        PlayRandomAction();

        npcController.SetLookPlayer(true);
        PlayerController.Instance.ForceLookAt(transform);
    }

    void Update()
    {
        if (state != State.Acting)
            return;

        // 강제 시선 버티기 타이머
        forceLookTimer += Time.deltaTime;

        // 5초 버티면 자동 해제 (플레이어 승리)
        if (forceLookTimer >= FORCE_LOOK_LIMIT)
        {
            StopWeirdAction();
        }
    }

    /// <summary>
    /// 플레이어가 노인을 바라보고 있을 때
    /// </summary>
    public override void OnGazed(float deltaTime)
    {
        base.OnGazed(deltaTime); // 공통 압박

        if (!CanGimmickInteract)
            return;

        gazeTimer += deltaTime;
        
        float normalized = gazeTimer / gazeRequiredTime;
        gazeGaugeUI.SetGauge(normalized);

        // 3~5초 이상 바라봤을 때만 민망함 증가
        if (gazeTimer >= gazeRequiredTime)
        {
            PlayerController.Instance.AddAwkward(25f); // 한 번에 크게 증가
            StopWeirdAction();
        }
    }

    public override void ResetGaze()
    {
        base.ResetGaze();
        if (state != State.Acting) return;

        // 시선을 피하면 gazeTimer 감소 (완전 리셋은 아님)
        gazeTimer = Mathf.Max(0f, gazeTimer - Time.deltaTime);
        gazeGaugeUI.SetGauge(gazeTimer / gazeRequiredTime);
    }

    void StopWeirdAction()
    {
        ani.SetBool("isAction", false);
        state = State.Idle;      

        gazeGaugeUI.Show(false);

        npcController.SetLookPlayer(false);
        PlayerController.Instance.ReleaseForceLook();
    }

    void PlayRandomAction()
    {
        int index = Random.Range(0, 4);
        ani.SetTrigger("Action" + index);
        ani.SetBool("isAction", true);
    }
}