﻿using UnityEngine;

public class Sound : MonoBehaviour {

    public static Sound Instance;

    public AudioSource typeSource;
    public AudioSource computerSource;
    public AudioClip[] typeSounds;
    public AudioClip[] computerSounds;

    public AudioSource wrongSource;
    public AudioClip wrongBip;
    public AudioSource correctSource;
    public AudioClip correctBip;

    public float minPitch = 0.9f;
    public float maxPitch = 1.0f;

    void Awake() {
        Instance = this;
    }

    // Use this for initialization
    void Start() {

    }

    public void PlayRandomTypeSound() {
        typeSource.clip = typeSounds[Random.Range(0, typeSounds.Length)];
        typeSource.pitch = Random.Range(minPitch, maxPitch);
        typeSource.Play();
    }
    public void PlayRandomComputerSound() {
        computerSource.clip = computerSounds[Random.Range(0, computerSounds.Length)];
        computerSource.Play();
    }
    public void PlayWrongBip() {
        wrongSource.clip = wrongBip;
        wrongSource.Play();
    }
    public void PlayCorrectBip() {
        correctSource.clip = correctBip;
        correctSource.Play();
    }
}
