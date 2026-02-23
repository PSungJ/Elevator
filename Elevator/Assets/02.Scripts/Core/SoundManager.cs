using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [Header("Audio Sources")]
    public AudioSource bgmSource;
    public AudioSource loopSfxSource;   // 심박동 전용
    public AudioSource oneShotSource;   // 벨, 문소리 등

    [Header("Volume")]
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // =========================
    // BGM
    // =========================
    public void PlayBGM(AudioClip clip, bool loop = true)
    {
        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.volume = bgmVolume;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void SetBGMVolume(float value)
    {
        bgmVolume = value;
        bgmSource.volume = value;
    }

    // =========================
    // SFX
    // =========================
    public void PlaySFX(AudioClip clip)
    {
        oneShotSource.PlayOneShot(clip, sfxVolume);
    }

    public void StopSFX()
    {
        oneShotSource.Stop();
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        loopSfxSource.volume = value;
        oneShotSource.volume = value;
    }

    public void PlayHeartBeat(AudioClip clip)
    {
        loopSfxSource.clip = clip;
        loopSfxSource.loop = true;
        loopSfxSource.Play();
    }

    public void StopHeartBeat()
    {
        loopSfxSource.Stop();
    }
}
