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

    [Header("PlayerUI")]
    public GameObject playerUI;

    private bool isMenuOpen = false;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    // ================================
    // GameOver
    // ================================
    public void ShowGameOver()
    {
        StartCoroutine(GameOverSequence());
    }

    IEnumerator GameOverSequence()
    {
        SoundManager.Instance.StopBGM();
        SoundManager.Instance.StopSFX();

        yield return new WaitForSecondsRealtime(0.3f);

        gameOverPanel.SetActive(true);
        Time.timeScale = 0f;

        PlayerController.Instance.SetControl(false);
    }

    // ================================
    // Menu
    // ================================
    public void ToggleMenu()
    {
        if (gameOverPanel.activeSelf) return; // GameOver 중에는 ESC 막기

        isMenuOpen = !isMenuOpen;
        menuPanel.SetActive(isMenuOpen);
        playerUI.SetActive(!isMenuOpen);

        Time.timeScale = isMenuOpen ? 0f : 1f;

        PlayerController.Instance.SetControl(!isMenuOpen);

        if (isMenuOpen)
            AudioListener.pause = true;
        else
            AudioListener.pause = false;
    }

    public void ResumeGame()
    {
        isMenuOpen = false;
        AudioListener.pause = false;
        menuPanel.SetActive(false);
        playerUI.SetActive(true);
        Time.timeScale = 1f;

        PlayerController.Instance.SetControl(true);
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
