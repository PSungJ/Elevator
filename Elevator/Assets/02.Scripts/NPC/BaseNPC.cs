using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 모든 NPC의 공통 베이스
/// 게임 로직 판단은 전부 여기 기준
/// </summary>
public abstract class BaseNPC : MonoBehaviour
{
    [Header("Look Point (Face)")]
    public Transform lookPoint;

    [Header("Common Gaze Pressure")]
    public float gazeStartDelay = 2f;          // 응시 시작까지 필요한 시간
    public float baseAwkwardPerSec = 1f;        // 기본 증가량
    public float acceleration = 0.5f;           // 시간당 가속
    public float recoverPerSec = 1f;             // 시선 해제 시 감소량

    protected float gazeHoldTimer = 0f;
    protected float awkwardTimer = 0f;
    protected bool isGazing = false;
    protected bool isActive = false;

    /// <summary>
    /// 이상행동 중인지 여부
    /// (PlayerController 시선 강제 판단용)
    /// </summary>
    public virtual bool IsActing => isActive;

    /// <summary>
    /// 플레이어와 상호작용 가능한 상태인지
    /// (엘리베이터 안, 도착 이후 등)
    /// </summary>
    public virtual bool CanInteract => isActive;

    public virtual void OnRideStart()
    {
        isActive = true;
    }

    public virtual void OnRideEnd()
    {
        isActive = false;

        // 상태 정리
        isGazing = false;
        gazeHoldTimer = 0f;
        awkwardTimer = 0f;
    }

    public abstract void OnArrivedInElevator();
    public virtual void OnGazed(float deltaTime)    // 공통 응시 처리 로직
    {
        if (!CanInteract) return;

        isGazing = true;
        gazeHoldTimer += deltaTime;

        // 2초 이상 응시했을 때부터 발동
        if (gazeHoldTimer >= gazeStartDelay)
        {
            awkwardTimer += deltaTime;

            float speed =
                baseAwkwardPerSec +
                awkwardTimer * acceleration;

            PlayerController.Instance.AddAwkward(
                speed * deltaTime
            );
        }
    }
    public virtual void ResetGaze() // 시선 해제 시 처리
    {
        if (!CanInteract) return;

        isGazing = false;
        gazeHoldTimer = 0f;
    }

    protected virtual void Update() // 시선 안 볼 때 민망함 감소
    {
        if (!isActive) return;

        if (!isGazing && awkwardTimer > 0f)
        {
            awkwardTimer -= Time.deltaTime;
            awkwardTimer = Mathf.Max(0f, awkwardTimer);

            PlayerController.Instance.AddAwkward(
                -recoverPerSec * Time.deltaTime
            );
        }
    }
}
