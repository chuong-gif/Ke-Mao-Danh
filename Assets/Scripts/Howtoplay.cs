using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class HowToPlayPopup : MonoBehaviour
{
    [Header("UI")]
    public GameObject popupPanel;
    public Button howToPlayButton;
    public Button closeButton;
    public RawImage videoRawImage;

    [Header("Video")]
    public VideoPlayer videoPlayer;
    public RenderTexture renderTexture;
    public string videoUrl = "https://your-video-link.mp4"; //drive link, and youtobe link

    void Start()
    {
        popupPanel.SetActive(false);

        howToPlayButton.onClick.AddListener(OpenPopup);
        closeButton.onClick.AddListener(ClosePopup);

        videoPlayer.source = VideoSource.Url;
        videoPlayer.url = videoUrl;

        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = renderTexture;
        videoRawImage.texture = renderTexture;
    }

    public void OpenPopup()
    {
        popupPanel.SetActive(true);
        videoPlayer.Play();
    }

    public void ClosePopup()
    {
        videoPlayer.Stop();
        popupPanel.SetActive(false);
    }
}