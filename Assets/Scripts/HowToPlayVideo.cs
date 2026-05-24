using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HowToPlayVideo : MonoBehaviour
{
    public GameObject panel;
    public VideoPlayer videoPlayer;
    public AudioSource audioSource;
    public Button pauseButton;
    public Button closeButton;
    public TMP_Text pauseText;
    public VideoClip tutorialClip;

    void Start()
    {
        panel.SetActive(false);

        videoPlayer.playOnAwake = false;

        videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
        videoPlayer.SetTargetAudioSource(0, audioSource);

        audioSource.playOnAwake = false;

        pauseButton.onClick.AddListener(TogglePause);
        closeButton.onClick.AddListener(CloseVideo);
    }

    public void OpenVideo()
    {
        panel.SetActive(true);

        videoPlayer.clip = tutorialClip;
        videoPlayer.time = 0;

        videoPlayer.Play();

        pauseText.text = "⏸";
    }

    public void TogglePause()
    {
        if (videoPlayer.isPlaying)
        {
            videoPlayer.Pause();
            audioSource.Pause();
            pauseText.text = "▶";
        }
        else
        {
            videoPlayer.Play();
            audioSource.UnPause();
            pauseText.text = "⏸";
        }
    }

    public void CloseVideo()
    {
        videoPlayer.Stop();
        audioSource.Stop();

        panel.SetActive(false);
        pauseText.text = "⏸";
    }
}