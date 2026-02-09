using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 노인 NPC 머리 위 시선 게이지 UI
/// </summary>
public class GazeGaugeUI : MonoBehaviour
{
    [SerializeField] Image fillImage;

    [Header("Gauge Colors")]
    public Color yellow = new Color(1f, 0.9f, 0.2f);
    public Color orange = new Color(1f, 0.5f, 0.1f);
    public Color red = new Color(0.9f, 0.1f, 0.1f);

    Camera cam;

    void Awake()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        // 항상 카메라를 바라보게 (빌보드)
        transform.forward = cam.transform.forward;
    }

    /// <summary>
    /// 게이지 갱신 (0~1)
    /// </summary>
    public void SetGauge(float normalized)
    {
        normalized = Mathf.Clamp01(normalized);
        fillImage.fillAmount = normalized;

        fillImage.color = EvaluateColor(normalized);

        // 위험 구간 맥동 연출
        if (normalized > 0.6f)
        {
            float pulse = Mathf.Sin(Time.time * 8f) * 0.03f;
            fillImage.fillAmount = Mathf.Clamp01(normalized + pulse);
        }

        if (normalized > 0.8f)
        {
            Color c = fillImage.color;
            fillImage.color = new Color(
                Mathf.Clamp01(c.r * 1.2f),
                Mathf.Clamp01(c.g * 1.2f),
                Mathf.Clamp01(c.b * 1.2f),
                c.a
            );
        }
    }

    /// <summary>
    /// 게이지 값에 따른 색상 계산
    /// </summary>
    Color EvaluateColor(float t)
    {
        // 0 ~ 30% : 노랑
        if (t <= 0.3f)
        {
            // 아주 약간 진해지는 느낌만
            float local = t / 0.3f;
            return Color.Lerp(yellow * 0.7f, yellow, local);
        }
        // 30 ~ 60% : 노랑 → 주황
        else if (t <= 0.6f)
        {
            float local = (t - 0.3f) / 0.3f;
            return Color.Lerp(yellow, orange, local);
        }
        // 60 ~ 100% : 주황 → 빨강
        else
        {
            float local = (t - 0.6f) / 0.4f;
            return Color.Lerp(orange, red, local);
        }
    }


    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
