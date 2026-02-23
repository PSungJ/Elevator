using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// UI 중앙 통제
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public PressureBorderUI pressureBorderUI;

    [Header("Panels")]
    public GameObject gameOverPanel;
    public GameObject menuPanel;
    public GameObject gameClearPanel;

    [Header("PlayerUI")]
    public GameObject playerUI;

    private bool isMenuOpen = false;

    void Awake()
    {
        Instance = this;
        SetGameplayCursor();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!isMenuOpen)
                OpenMenu();
            else
            {
                Debug.Log("ResumeGame Called");
                ResumeGame();
            }
        }
    }

    // 게임 플레이 상태
    public void SetGameplayCursor()
    {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    // UI 상태 (메뉴, 게임오버 등)
    public void SetUICursor()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    // ================================
    // GameOver
    // ================================
    public void ShowGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    public void ShowGameClear()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();

        gameClearPanel.SetActive(true);
        playerUI.SetActive(false);
        Time.timeScale = 0f;

        PlayerController.Instance.SetControl(false);
    }

    IEnumerator GameOverSequence()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();

        yield return new WaitForSecondsRealtime(0.3f);

        gameOverPanel.SetActive(true);
        playerUI.SetActive(false);
        Time.timeScale = 0f;

        PlayerController.Instance.SetControl(false);
    }

    // ================================
    // Menu
    // ================================
    public void OpenMenu()
    {
        if (gameOverPanel.activeSelf || gameClearPanel.activeSelf)
            return;

        isMenuOpen = true;

        menuPanel.SetActive(true);
        playerUI.SetActive(false);

        Time.timeScale = 0f;
        AudioListener.pause = true;

        PlayerController.Instance.SetControl(false);

        SetUICursor();
    }

    public void ResumeGame()
    {
        isMenuOpen = false;
        AudioListener.pause = false;
        menuPanel.SetActive(false);
        playerUI.SetActive(true);
        Time.timeScale = 1f;

        PlayerController.Instance.SetControl(true);
        SetGameplayCursor();
    }

    // ================================
    // Scene
    // ================================
    public void GoToLobby()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("LobbyScene");
    }
}
