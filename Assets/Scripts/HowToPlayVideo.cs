using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HowToPlayVideo : MonoBehaviour
{
    [Header("UI")]
    public GameObject panel;
    public Button pauseButton;
    public Button closeButton;
    public TMP_Text pauseText;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public VideoClip tutorialClip;

    private bool isPaused = false;

    private void Start()
    {
        panel.SetActive(false);

        videoPlayer.playOnAwake = false;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        audioSource.playOnAwake = false;
        audioSource.Stop();

        pauseButton.onClick.AddListener(TogglePause);
        closeButton.onClick.AddListener(CloseVideo);

        if (pauseText != null)
            pauseText.text = "⏸";
    }

    public void OpenVideo()
    {
        panel.SetActive(true);

        videoPlayer.Stop();
        audioSource.Stop();

        videoPlayer.clip = tutorialClip;
        videoPlayer.time = 0;

        videoPlayer.Play();

        isPaused = false;

        if (pauseText != null)
            pauseText.text = "⏸";
    }

    public void TogglePause()
    {
        if (!isPaused)
        {
            videoPlayer.Pause();
            audioSource.Pause();

            isPaused = true;

            if (pauseText != null)
                pauseText.text = "▶";
        }
        else
        {
            videoPlayer.Play();
            audioSource.UnPause();

            isPaused = false;

            if (pauseText != null)
                pauseText.text = "⏸";
        }
    }

    public void CloseVideo()
    {
        videoPlayer.Stop();
        audioSource.Stop();

        panel.SetActive(false);

        isPaused = false;

        if (pauseText != null)
            pauseText.text = "⏸";
    }
}