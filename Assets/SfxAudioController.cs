using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SfxAudioController : MonoBehaviour
{
    public static SfxAudioController instance;
    public AudioSource audioSourceSfx;
    public AudioClip correct;
    public AudioClip incorrect;

    private void Awake()
    {
        instance = this;
    }
    public void CorrectSound()
    {
        audioSourceSfx.Stop();
        audioSourceSfx.clip = correct;
        audioSourceSfx.Play();
    }

    public void IncorrectSound()
    {
        audioSourceSfx.Stop();
        audioSourceSfx.clip = incorrect;
        audioSourceSfx.Play();
    }

    public void PlaySound(AudioClip audioClip)
    {
        audioSourceSfx.Stop();
        audioSourceSfx.clip = audioClip;
        audioSourceSfx.Play();
    }
}
