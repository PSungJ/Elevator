using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildNPC : BaseNPC
{
    enum State
    {
        Pressuring,
        Cooldown
    }

    State state = State.Pressuring;

    [Header("Cooldown")]
    public float cooldownMin = 4f;
    public float cooldownMax = 8f;

    [Header("Gaze Clear Condition")]
    public float requiredGazeTime = 5f;
    public float pressureAwkwardPerSecond = 1f;

    [Header("Pressure UI")]
    public PressureBorderUI pressureUI;

    float gazeTimer = 0f;
    NPCController npcController;

    public override bool CanInteract =>
        isActive && npcController.IsSettled && state == State.Pressuring;

    void Start()
    {
        npcController = GetComponent<NPCController>();
    }

    protected override void Update()
    {
        base.Update();  // 공통 기믹
        if (state != State.Pressuring)
        {
            Debug.Log("ChildNPC Update blocked");
            return;
        }
        if (!npcController.IsSettled)
            return;

        // 아이 기믹
        Debug.Log("ChildNPC Update ADD AWKWARD");
        PlayerController.Instance.AddAwkward(
                pressureAwkwardPerSecond * Time.deltaTime
        );
    }

    public override void OnRideStart()
    {
        base.OnRideStart();
        state = State.Pressuring;
        gazeTimer = 0f;
    }

    /// <summary>
    /// 엘리베이터 도착
    /// </summary>
    public override void OnArrivedInElevator()
    {
        // 아이가 먼저 플레이어를 쳐다봄
        npcController.SetLookPlayer(true);

        // 압박 UI 호출
        pressureUI.Show();
    }

    /// <summary>
    /// 플레이어가 아이를 쳐다보고 있을 때
    /// </summary>
    public override void OnGazed(float deltaTime)
    {
        base.OnGazed(deltaTime); // 공통 민망함 압박

        if (!CanInteract) return;
        gazeTimer += deltaTime;

        Debug.Log($"{gazeTimer:F2} / {requiredGazeTime} - Player Gazing");

        if (gazeTimer >= requiredGazeTime)
        {
            FinishGimmick();
        }
    }

    /// <summary>
    /// 플레이어가 시선을 떼면
    /// </summary>
    public override void ResetGaze()
    {
        base.ResetGaze(); // 공통 gaze 처리
        gazeTimer = 0f;
    }

    /// <summary>
    /// 기믹 종료
    /// </summary>
    void FinishGimmick()
    {
        state = State.Cooldown;

        npcController.SetLookPlayer(false);
        LookAway();

        // UI 종료
        pressureUI.Hide();
        
        gazeTimer = 0f;

        StartCoroutine(CooldownRoutine());
    }

    void LookAway()
    {
        float angle = Random.Range(120f, 200f);
        transform.Rotate(0f, angle, 0f);
    }

    IEnumerator CooldownRoutine()
    {
        float wait = Random.Range(cooldownMin, cooldownMax);
        yield return new WaitForSeconds(wait);

        state = State.Pressuring;

        npcController.SetLookPlayer(true);
        pressureUI.Show();
    }
}
