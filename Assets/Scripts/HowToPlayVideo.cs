using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HowToPlayVideo : MonoBehaviour
{
    public GameObject panel;
    public VideoPlayer videoPlayer;
    public Button pauseButton;
    public Button closeButton;
    public TMP_Text pauseText;
    public VideoClip tutorialClip;

    private bool isPaused = false;

    private void Start()
    {
        panel.SetActive(false);

        videoPlayer.playOnAwake = false;
        videoPlayer.playbackSpeed = 1f;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;

        pauseButton.onClick.AddListener(TogglePause);
        closeButton.onClick.AddListener(CloseVideo);
    }

    public void OpenVideo()
    {
        panel.SetActive(true);

        videoPlayer.Stop();
        videoPlayer.clip = tutorialClip;
        videoPlayer.time = 0;
        videoPlayer.playbackSpeed = 1f;

        videoPlayer.Prepare();
        videoPlayer.prepareCompleted += OnVideoPrepared;

        isPaused = false;

        if (pauseText != null)
            pauseText.text = "⏸";
    }

    private void OnVideoPrepared(VideoPlayer vp)
    {
        videoPlayer.prepareCompleted -= OnVideoPrepared;
        videoPlayer.Play();
    }

    public void TogglePause()
    {
        if (!isPaused)
        {
            videoPlayer.Pause();
            isPaused = true;

            if (pauseText != null)
                pauseText.text = "▶";
        }
        else
        {
            videoPlayer.Play();
            isPaused = false;

            if (pauseText != null)
                pauseText.text = "⏸";
        }
    }

    public void CloseVideo()
    {
        videoPlayer.Stop();
        panel.SetActive(false);

        isPaused = false;

        if (pauseText != null)
            pauseText.text = "⏸";
    }
}