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
        fillImage.fillAmount = Mathf.Clamp01(normalized);
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }
}
