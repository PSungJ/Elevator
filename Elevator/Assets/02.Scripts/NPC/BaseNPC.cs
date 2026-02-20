using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 NPC의 공통 베이스
/// - "쳐다보면 증가 / 안 보면 감소"는 항상 여기서 동작
/// - 아이/노인은 이 위에 추가 압박만 얹는다
/// </summary>
public class BaseNPC : MonoBehaviour
{
    [Header("Look Point (Face)")]
    public Transform lookPoint;

    [Header("Common Gaze Pressure")]
    public float gazeStartDelay = 2f;          // 응시 시작까지 필요한 시간
    public float baseAwkwardPerSec = 1f;        // 기본 증가량
    public float acceleration = 0.5f;           // 시간당 가속

    protected float gazeHoldTimer = 0f;
    protected float awkwardTimer = 0f;
    protected bool isGazing = false;
    protected bool isActive = false;

    // 공통 기믹용
    public virtual bool CanInteract => isActive;

    // 기믹용 (아이 / 노인에서 override)
    public virtual bool CanGimmickInteract => false;
    public virtual bool IsActing => false;

    public virtual void OnRideStart()
    {
        isActive = true;
        gazeHoldTimer = 0f;
        awkwardTimer = 0f;
    }

    public virtual void OnRideEnd()
    {
        isActive = false;
    }

    public virtual void OnGazed(float deltaTime)    // 공통 응시 처리 로직
    {
        if (!CanInteract) return;

        gazeHoldTimer += deltaTime;

        if (gazeHoldTimer < gazeStartDelay)
            return;

        awkwardTimer += deltaTime;

        float speed =
            baseAwkwardPerSec +
            awkwardTimer * acceleration;

        PlayerController.Instance.AddAwkward(
            speed * deltaTime
        );
    }

    public virtual void ResetGaze() // 시선 해제 시 처리
    {
        if (!CanInteract) return;

        isGazing = false;
        gazeHoldTimer = 0f;
    }
}
