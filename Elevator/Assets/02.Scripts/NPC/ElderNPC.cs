using System.Collections;
using UnityEngine;

/// <summary>
/// 노인 NPC
/// - 이상행동 중 시선 강제
/// - 3~5초 이상 바라보면 민망함 크게 증가
/// - 5초간 시선 강제 버티면 자동 해제
/// </summary>
public class ElderNPC : BaseNPC
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

    // ===== 타이머 =====
    float gazeTimer = 0f;        // 플레이어가 바라본 시간
    float forceLookTimer = 0f;   // 강제 시선 유지 시간

    // ===== 설정값 =====
    float gazeRequiredTime;      // 3~5초 랜덤
    const float FORCE_LOOK_LIMIT = 5f;

    float calmDelayMin = 5f;
    float calmDelayMax = 10f;

    public override bool IsActing => state == State.Acting;
    public override bool CanInteract => hasArrived;

    void Start()
    {
        ani = GetComponent<Animator>();
        npcController = GetComponent<NPCController>();
    }

    public override void OnArrivedInElevator()
    {
        if (hasArrived) return;

        hasArrived = true;
        StartCoroutine(BehaviorLoop());
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
        if (state == State.Acting)
            return;

        state = State.Acting;

        // 타이머 초기화
        gazeTimer = 0f;
        forceLookTimer = 0f;
        gazeRequiredTime = Random.Range(3f, 5f);

        gazeGaugeUI.Show(true);
        gazeGaugeUI.SetGauge(0f);

        PlayRandomAction();

        npcController.SetLookPlayer(true);
        PlayerController.Instance.ForceLookAt(transform);
    }

    protected override void Update()
    {
        base.Update();
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

        if (state != State.Acting)
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
        state = State.Idle;

        ani.SetBool("isWalk", false);

        gazeGaugeUI.Show(false);

        npcController.SetLookPlayer(false);
        PlayerController.Instance.ReleaseForceLook();
    }

    void PlayRandomAction()
    {
        int index = Random.Range(0, 4);
        ani.SetTrigger("Action" + index);
    }
}