using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AwkwardUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] Slider awkwardSlider;

    public void SetAwkward(float normalized)
    {
        awkwardSlider.value = Mathf.Clamp01(normalized);
    }

    void Update()
    {
        float normalized = PlayerController.Instance.awkward / PlayerController.Instance.maxAwkward;
        awkwardSlider.value = normalized;
    }
}
