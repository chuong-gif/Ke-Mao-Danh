using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ProgressTasks : MonoBehaviour
{
    private static float MaxProgress = 0; // giá trị tối đa
    private static float Progress = 0f; //giá trị hiện tại
    private static int TaskCount = 0; // Số lượng task trong map

    [Header("End Game UI")]
    [SerializeField] private GameObject victoryPanel; // Panel chữ VICTORY màu xanh lá
    [SerializeField] private GameObject defeatPanel; // Panel chữ DEFEAT màu đỏ
    [SerializeField] private float delayBeforeMenu = 4.0f; // thời gian delay

    private static ProgressTasks instance; // Singleton instance để dễ dàng truy cập từ các lớp khác

    public void Start()
    {
        instance = this;
        // dữ liệu lúc bắt đầu
        Progress = 0f;
        MaxProgress = 0f;

        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);

        // tìm UI của player
        GameObject playerUi = GameObject.FindWithTag("PlayerUI");
        if (playerUi != null)
        {
            Slider slider = playerUi.GetComponentInChildren<Slider>();
            if (slider != null) MaxProgress = slider.maxValue; // lấy giá trị tối đa từ slider để đảm bảo đồng bộ với UI
        }
    }

    // Hàm đếm người và phán xử thắng thua
    public static void CheckMatchState()
    {
        if (instance == null) return;

        // Tìm TẤT CẢ Player (bao gồm player và các con rối) đang có trên bản đồ
        Player[] allPlayers = FindObjectsByType<Player>(FindObjectsSortMode.None);
        // khởi tạo giá trị đếm
        int aliveCrew = 0;
        int aliveImpostors = 0;

        foreach (Player p in allPlayers)
        {
            if (!p.isGhost) // Chỉ đếm những người chưa chết
            {
                if (p.Team == Player.TypePlayer.Crew) aliveCrew++;
                else if (p.Team == Player.TypePlayer.Impostor) aliveImpostors++;
            }
        }

        Debug.Log($"Trạng thái sinh tồn: Crew ({aliveCrew}) vs Impostor ({aliveImpostors})");

        // ĐIỀU KIỆN 1: IMPOSTOR THẮNG (Số Crew sống <= Số Impostor sống)
        if (aliveCrew <= aliveImpostors)
        {
            instance.EndMatch(false); // Gửi False = Crew không thắng (Tức là Impostor thắng)
        }
        // ĐIỀU KIỆN 2: CREW THẮNG (Đã làm đầy thanh Task)
        else if (MaxProgress > 0f && Progress >= (MaxProgress - 0.01f))
        {
            instance.EndMatch(true); // Gửi True = Crew thắng
        }
    }

    // Hàm được gọi mỗi khi hoàn thành task
    public static void SetProgress(float value)
    {
        Progress = value;
        // Tìm rồi cập nhật thanh progress trên UI của player
        GameObject playerUi = GameObject.FindWithTag("PlayerUI");
        if (playerUi != null)
        {
            Slider slider = playerUi.GetComponentInChildren<Slider>();
            if (slider != null) slider.value = Progress;
        }

        // Gọi hàm kiểm tra ván đấu mỗi khi có task hoàn thành
        CheckMatchState();
    }

    public static float GetProgress() { return Progress; } // trả về tiến độ hiện tại

    // giá trị mỗi nhiệm vụ đóng góp vào thanh
    public static float GetDistributedValue() { return TaskCount == 0 ? 0f : MaxProgress / TaskCount; }
    // Hàm này được gọi khi một task mới được tạo ra để tăng số lượng task và đảm bảo tính toán giá trị phân phối chính xác
    public static void TaskSetup() { ++TaskCount; }

    // HÀM KẾT THÚC VÁN CHƠI VÀ BẬT UI TƯƠNG ỨNG
    private void EndMatch(bool crewWin)
    {
        // Xét xem bản thân người chơi (Local Player) có thuộc phe thắng lợi không
        bool isLocalPlayerCrew = (Player.localPlayer.Team == Player.TypePlayer.Crew);
        bool isLocalPlayerWon = (crewWin && isLocalPlayerCrew) || (!crewWin && !isLocalPlayerCrew);

        if (isLocalPlayerWon)
        {
            if (victoryPanel != null) victoryPanel.SetActive(true);
            // PHÁT NHẠC THẮNG
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.winSFX);
        }
        else
        {
            if (defeatPanel != null) defeatPanel.SetActive(true);
            // PHÁT NHẠC THUA
            if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.loseSFX);
        }

        // KHÓA CHÂN NGƯỜI CHƠI
        if (Player.localPlayer != null)
        {
            Rigidbody2D rb = Player.localPlayer.GetComponent<Rigidbody2D>();
            if (rb != null) { rb.linearVelocity = Vector2.zero; rb.angularVelocity = 0f; }
            Animator anim = Player.localPlayer.GetComponentInChildren<Animator>();
            if (anim != null) anim.SetFloat("Speed", 0f);

            Player.localPlayer.enabled = false;
        }

        StartCoroutine(LoadMainMenuRoutine());
    }

    //load scene main menu 
    private IEnumerator LoadMainMenuRoutine()
    {
        yield return new WaitForSeconds(delayBeforeMenu);

        if (Player.localPlayer != null) Player.localPlayer.enabled = true;

        Progress = 0f;
        TaskCount = 0;
        MaxProgress = 0;
        SceneManager.LoadScene("MainMenu");
    }
}