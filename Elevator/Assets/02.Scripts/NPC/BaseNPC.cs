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

    /// <summary>
    /// 이상행동 중인지 여부
    /// (PlayerController 시선 강제 판단용)
    /// </summary>
    public virtual bool IsActing => false;

    /// <summary>
    /// 플레이어와 상호작용 가능한 상태인지
    /// (엘리베이터 안, 도착 이후 등)
    /// </summary>
    public virtual bool CanInteract => false;

    public abstract void OnArrivedInElevator();
    public abstract void OnGazed(float deltaTime);
    public abstract void ResetGaze();
}
