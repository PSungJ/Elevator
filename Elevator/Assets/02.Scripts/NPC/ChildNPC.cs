using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChildNPC : BaseNPC
{
    bool hasArrived = false;

    public override bool CanInteract => hasArrived;

    public override void OnArrivedInElevator()
    {
        hasArrived = true;
    }

    public float gazeBreakTime = 2f;
    public float avoidDuration = 3f;

    float gazeTimer = 0f;
    bool isAvoiding = false;

    public override void OnGazed(float deltaTime)
    {
        // 쳐다보는 동안 민망함 증가
        PlayerController.Instance.AddAwkward(deltaTime);

        if (isAvoiding) return;

        gazeTimer += deltaTime;

        if (gazeTimer >= gazeBreakTime)
        {
            StartCoroutine(AvoidGaze());
        }
    }

    public override void ResetGaze()
    {
        gazeTimer = 0f;
    }

    IEnumerator AvoidGaze()
    {
        isAvoiding = true;
        gazeTimer = 0f;

        // 아이 시선 회피 (고개 돌리기 등)
        LookAway();

        yield return new WaitForSeconds(avoidDuration);

        isAvoiding = false;
    }

    void LookAway()
    {
        // 간단히 랜덤 방향
        transform.Rotate(0f, Random.Range(120f, 200f), 0f);
    }
}
