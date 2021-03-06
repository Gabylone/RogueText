using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using DG.Tweening;

public class DisplayRecordFeedback : MonoBehaviour
{
    public static DisplayRecordFeedback Instance;

    public GameObject group;

    public float duration = 0.3f;

    public CanvasGroup canvasGroup;

    public Transform image_Transform;

    public Text uiText;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideDelay();

        InputManager.onTouchDown += HandleOnTouchDown;
        InputManager.onTouchUp += HandleOnTouchUp;
    }

    private void HandleOnTouchUp()
    {
        Hide();
        AudioInteraction.Instance.StopRecording();
    }

    private void HandleOnTouchDown()
    {
        Show();
        AudioInteraction.Instance.StartRecording();

        AudioInteraction.Instance.ClearSpeaking();
    }

    public void Show()
    {
        group.SetActive(true);

        //canvasGroup.alpha = 1f;
        canvasGroup.alpha = 0f;
        canvasGroup.DOFade(1f, duration);

        Tween.Bounce(image_Transform);
    }

    public void Display(string str)
    {
        uiText.text = str;
    }

    public void Hide()
    {
        Debug.Log("parsing phrase : " + uiText.text);
        InputInfo.ParsePhrase(uiText.text);

        canvasGroup.DOFade(0f, duration);

        CancelInvoke("HideDelay");
        Invoke("HideDelay", duration);
    }
    void HideDelay()
    {
        group.SetActive(false);
    }
}
