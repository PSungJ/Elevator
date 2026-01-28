using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 게임 시작 / 종료
// 성공 & 실패 판정
// 점수 계산 호출
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    void Awake()
    {
        Instance = this;
    }

    public void GameOver(bool success)
    {
        Time.timeScale = 0f;
        UIManager.Instance.ShowResult(success);
    }
}
