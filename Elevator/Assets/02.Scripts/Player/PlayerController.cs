using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 시점 제어 및 민망함 게이지 관리
/// - 기본 마우스 시점
/// - NPC 이상행동 시 시선 강제
/// - 강제 시선 + 플레이어 저항 혼합 방식
/// </summary>
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance;

    [SerializeField] AwkwardUI awkwardUI;
    private bool isGameOver = false;

    [Header("Awkward Gauge")]
    public float awkward;              // 현재 민망함
    public float maxAwkward = 100f;     // 최대 민망함

    [Header("Look Control")]
    public float lookSpeed = 3f;        // 기본 마우스 감도
    public float minPitch = -30f;       // 아래 시점 제한
    public float maxPitch = 30f;        // 위 시점 제한

    [Header("Force Look")]
    public float forceStrength = 3.5f;  // 시선이 끌리는 힘 (노인 행동 강도)
    public float resistStrength = 1.0f; // 플레이어 저항력 (마우스 영향력)

    Camera cam;

    float yaw;      // 좌우 회전
    float pitch;    // 상하 회전

    // 현재 시선을 강제로 끄는 대상 (노인 이상행동 중)
    Transform forcedTarget;

    public bool IsUnderPressure { get; private set; }
    public bool CanControl { get; private set; } = true;

    void Awake()
    {
        Instance = this;
        cam = Camera.main;

        // 초기 카메라 각도 저장
        Vector3 rot = cam.transform.eulerAngles;
        yaw = rot.y;
        pitch = rot.x;
    }

    void Update()
    {
        HandleLook();
    }

    // 아이 기믹 중에는 회복차단
    public void SetPressure(bool value)
    {
        IsUnderPressure = value;
    }

    /// <summary>
    /// 시선 처리 메인 로직
    /// </summary>
    void HandleLook()
    {
        if (!CanControl) return;
        // =========================
        // 1️. 플레이어 마우스 입력 (항상 적용)
        // =========================
        float mx = Input.GetAxis("Mouse X");
        float my = Input.GetAxis("Mouse Y");

        yaw += mx * lookSpeed * resistStrength;
        pitch -= my * lookSpeed * resistStrength;

        // =========================
        // 2️. 시선 강제 처리 (이상행동 중일 때만)
        // =========================
        if (forcedTarget != null)
        {
            // 기본은 대상 Transform 위치
            Vector3 targetPos = forcedTarget.position;

            // NPC에 LookPoint(얼굴 기준점)가 있다면 그것을 사용
            BaseNPC npc = forcedTarget.GetComponent<BaseNPC>();

            if (npc == null || !npc.IsActing)
            {
                forcedTarget = null;
                return;
            }

            if (npc != null && npc.lookPoint != null)
            {
                targetPos = npc.lookPoint.position;
            }

            // 강제로 바라봐야 할 방향 계산
            Vector3 dir = targetPos - cam.transform.position;
            Quaternion targetRot = Quaternion.LookRotation(dir);

            // 현재 회전값
            Quaternion currentRot = Quaternion.Euler(pitch, yaw, 0f);

            // 플레이어 입력 + 강제 회전 혼합
            Quaternion forcedRot = Quaternion.Slerp(
                currentRot,
                targetRot,
                Time.deltaTime * forceStrength
            );

            // 결과값 반영
            yaw = forcedRot.eulerAngles.y;
            pitch = forcedRot.eulerAngles.x;
        }

        // =========================
        // 3. 상하 각도 제한
        // =========================
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // =========================
        // 4. 카메라 회전 적용
        // =========================
        cam.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    // =========================
    // 5. 민망함 게이지
    // =========================

    /// <summary>
    /// 민망함 증가
    /// </summary>
    public void AddAwkward(float amount)
    {
        if (isGameOver) return;

        awkward += amount;
        awkward = Mathf.Clamp(awkward, 0f, maxAwkward);

        awkwardUI.SetAwkward(awkward / maxAwkward);

        if (awkward >= maxAwkward)
        {
            if (isGameOver) return;
            UIManager.Instance.ShowGameOver();
        }
    }

    public void RecoverAwkward(float amount)
    {
        AddAwkward(-amount);
    }

    // =========================
    // 6. 시선 강제 제어 API
    // =========================

    /// <summary>
    /// 특정 대상에게 시선을 강제로 끌림
    /// (노인 이상행동 시작 시 호출)
    /// </summary>
    public void ForceLookAt(Transform target)
    {
        forcedTarget = target;
    }

    /// <summary>
    /// 시선 강제 해제
    /// (이상행동 종료 시 호출)
    /// </summary>
    public void ReleaseForceLook()
    {
        forcedTarget = null;
    }

    // UI 활성화 시 입력 차단 구조
    public void SetControl(bool value)
    {
        CanControl = value;

        if (value)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
