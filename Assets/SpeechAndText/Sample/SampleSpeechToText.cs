﻿using TextSpeech;
using UnityEngine;
using UnityEngine.UI;

public class SampleSpeechToText : MonoBehaviour {
    public GameObject loading;
    public InputField inputLocale;
    public InputField inputText;
    public float pitch;
    public float rate;

    public Text txtLocale;
    public Text txtPitch;
    public Text txtRate;
    void Start() {
        Setting("en-US");
        loading.SetActive(false);
        SpeechToText.instance.onResultCallback = OnResultSpeech;
    }


    public void StartRecording() {
        SpeechToText.instance.StartRecording("Speak any");

#if UNITY_EDITOR
#else
#endif
    }

    public void StopRecording() {
        SpeechToText.instance.StopRecording();
    }
    void OnResultSpeech(string _data) {
        inputText.text = _data;
    }
    public void OnClickSpeak() {
        TextToSpeech.instance.StartSpeak(inputText.text);
    }
    public void OnClickStopSpeak() {
        TextToSpeech.instance.StopSpeak();
    }
    public void Setting(string code) {
        TextToSpeech.instance.Setting(code, pitch, rate);
        SpeechToText.instance.Setting(code);
        txtLocale.text = "Locale: " + code;
        txtPitch.text = "Pitch: " + pitch;
        txtRate.text = "Rate: " + rate;
    }
    public void OnClickApply() {
        Setting(inputLocale.text);
    }
}
