using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildNPC : MonoBehaviour
{
    [Header("Gaze Settings")]
    public float gazeRequiredTime = 2f;
    public float lookAwayDuration = 5f;

    [Header("Stress")]
    public float stressPerSecond = 5f;

    float gazeTimer = 0f;
    bool isLookingAway = false;
    float lookAwayTimer = 0f;

    public Transform head;          // 머리 본
    public float lookSpeed = 5f;    // 고개 회전 속도
    public float maxYaw = 45f;      // 좌우 제한

    MentalGauge mental;

    void Start()
    {
        mental = FindObjectOfType<MentalGauge>();
    }

    void Update()
    {
        if (!isLookingAway)
        {
            // 아이가 나를 쳐다보는 상태 → 불안 증가
            mental.AddStress(stressPerSecond * Time.deltaTime);
        }
        else
        {
            lookAwayTimer -= Time.deltaTime;
            if (lookAwayTimer <= 0f)
            {
                isLookingAway = false;
            }
        }
    }

    void LateUpdate()
    {
        if (isLookingAway) return;

        LookAtPlayer();
    }

    void LookAtPlayer()
    {
        Vector3 targetPos = Camera.main.transform.position;
        Vector3 dir = targetPos - head.position;
        //dir.y = 0; // 상하 고정 (무서움 방지)

        Quaternion targetRot = Quaternion.LookRotation(dir);

        // 현재 회전 → 목표 회전 부드럽게
        Quaternion smoothRot = Quaternion.Slerp(
            head.rotation,
            targetRot,
            Time.deltaTime * lookSpeed
        );

        // 회전 제한
        float yaw = Mathf.DeltaAngle(
            transform.eulerAngles.y,
            smoothRot.eulerAngles.y
        );

        yaw = Mathf.Clamp(yaw, -maxYaw, maxYaw);

        head.localRotation = Quaternion.Euler(0, yaw, 0);
    }

    public void OnGazed(float delta)
    {
        if (isLookingAway) return;

        gazeTimer += delta;

        if (gazeTimer >= gazeRequiredTime)
        {
            StartCoroutine(LookAwayRoutine());
        }
    }

    void TriggerLookAway()
    {
        isLookingAway = true;
        lookAwayTimer = lookAwayDuration;
        gazeTimer = 0f;

        // 고개 돌리기
        Vector3 awayDir = -Camera.main.transform.forward;
        awayDir.y = 0;

        Quaternion awayRot = Quaternion.LookRotation(awayDir);
        head.rotation = awayRot;
        Debug.Log("아이가 시선을 피한다");
    }

    IEnumerator LookAwayRoutine()
    {
        Quaternion start = head.rotation;
        Quaternion end = Quaternion.Euler(0, Random.Range(90f, 150f), 0);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            head.rotation = Quaternion.Slerp(start, end, t);
            yield return null;
        }
        Debug.Log("아이가 시선을 피한다");
    }

    void OnDisable()
    {
        gazeTimer = 0f;
    }

    public void ResetGaze()
    {
        gazeTimer = 0f;
    }
}
