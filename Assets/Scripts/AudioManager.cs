using UnityEngine;
using UnityEngine.SceneManagement; // Bắt buộc phải có để nhận diện Scene

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Các bộ phát âm thanh (Loa)")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("Danh sách nhạc nền (BGM)")]
    public AudioClip mainMenuBGM; // Nhạc cho Scene MainMenu
    public AudioClip lobbyBGM;    // Nhạc cho Scene Lobby
    public AudioClip skeldBGM;    // Nhạc cho Scene Skeld

    [Header("Danh sách hiệu ứng (SFX)")]
    public AudioClip winSFX;
    public AudioClip loseSFX;
    public AudioClip killSFX;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return; // Dừng lại ngay để tránh chạy các dòng dưới nếu là bản sao
        }
    }

    // --- CƠ CHẾ TỰ ĐỘNG ĐỔI NHẠC KHI CHUYỂN SCENE ---
    private void OnEnable()
    {
        // Đăng ký lắng nghe sự kiện: Mỗi khi load xong một Scene thì gọi hàm OnSceneLoaded
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // Hủy đăng ký khi object bị tắt để tránh lỗi rò rỉ bộ nhớ
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Kiểm tra TÊN SCENE hiện tại là gì để phát đúng bài nhạc đó
        if (scene.name == "MainMenu")
        {
            PlayBGM(mainMenuBGM);
        }
        else if (scene.name == "Lobby")
        {
            PlayBGM(lobbyBGM);
        }
        else if (scene.name == "Skeld")
        {
            PlayBGM(skeldBGM);
        }
    }
    // -------------------------------------------------

    public void PlayBGM(AudioClip bgm)
    {
        if (bgm == null) return;

        // Nếu bài nhạc yêu cầu ĐANG PHÁT rồi thì thôi, không bật lại từ đầu (tránh giật nhạc)
        if (bgmSource.clip == bgm) return;

        bgmSource.clip = bgm;
        bgmSource.Play();
    }

    public void PlaySFX(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}