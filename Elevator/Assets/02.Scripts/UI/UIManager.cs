using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// UI 중앙 통제
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void ShowResult(bool success)
    {
        // 결과 패널 활성화
    }
}
