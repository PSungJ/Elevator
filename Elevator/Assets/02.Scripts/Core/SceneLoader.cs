using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLoader : MonoBehaviour
{
    public AudioSource sound;

    void Start()
    {
        sound = GetComponent<AudioSource>();
        AudioListener.pause = false;
    }

    public void PlayGame()
    {
        sound.Play();
        FadeManager.Instance.FadeAndLoad("GameScene");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
