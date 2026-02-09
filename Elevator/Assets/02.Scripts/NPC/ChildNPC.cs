using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildNPC : BaseNPC
{
    bool gimmickFinished = false;
    bool playerIsLookingAtMe = false;

    [Header("Gaze Clear Condition")]
    public float requiredGazeTime = 5f;
    public float pressureAwkwardPerSecond = 1f;

    [Header("Pressure UI")]
    public PressureBorderUI pressureUI;

    float gazeTimer = 0f;

    NPCController npcController;

    public override bool CanInteract => npcController.HasArrived && !gimmickFinished;

    void Start()
    {
        npcController = GetComponent<NPCController>();
    }

    void Update()
    {
        if (!CanInteract)
        {
            Debug.Log("ChildNPC Update blocked");
            return;
        }
        if (!npcController.IsSettled)
            return;

        Debug.Log("ChildNPC Update ADD AWKWARD");
        PlayerController.Instance.AddAwkward(
            pressureAwkwardPerSecond * Time.deltaTime
        );
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
        if (!CanInteract)
            return;

        playerIsLookingAtMe = true;

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
        if (!CanInteract)
            return;

        playerIsLookingAtMe = false;
        gazeTimer = 0f; // 연속 응시 조건
    }

    /// <summary>
    /// 기믹 종료
    /// </summary>
    void FinishGimmick()
    {
        gimmickFinished = true;

        npcController.SetLookPlayer(false);
        LookAway();

        // UI 종료
        pressureUI.Hide();
    }

    void LookAway()
    {
        float angle = Random.Range(120f, 200f);
        transform.Rotate(0f, angle, 0f);
    }
}
