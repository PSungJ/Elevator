using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PressureBorderUI : MonoBehaviour
{
    [Header("UI")]
    public Image borderImage;

    [Header("Blink Settings")]
    public float blinkSpeed = 2f;      // 깜빡임 속도
    public float maxAlpha = 0.6f;      // 최대 알파
    public float minAlpha = 0.1f;      // 최소 알파
    public AudioClip heartBeat;

    bool isActive = false;
    float timer = 0f;

    void Awake()
    {
        SetAlpha(0f);
    }

    void Update()
    {
        if (!isActive) return;

        timer += Time.deltaTime * blinkSpeed;

        // 0~1 사이 Sin 파형
        float t = (Mathf.Sin(timer) + 1f) * 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        SetAlpha(alpha);
    }

    void SetAlpha(float a)
    {
        Color c = borderImage.color;
        c.a = a;
        borderImage.color = c;
    }

    // =========================
    // 외부 제어 API
    // =========================

    public void Show()
    {
        SoundManager.Instance.PlayHeartBeat(heartBeat);
        isActive = true;
        timer = 0f;
    }

    public void Hide()
    {
        SoundManager.Instance.StopHeartBeat();
        isActive = false;
        SetAlpha(0f);
    }
}
