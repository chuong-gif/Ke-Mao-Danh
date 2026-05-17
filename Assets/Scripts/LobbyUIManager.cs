using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // BẮT BUỘC phải thêm dòng này để chuyển Scene

public class LobbyUIManager : MonoBehaviour
{
    [Header("Lobby UI Elements")]
    [SerializeField] GameObject startButton;     // Kéo nút Start vào đây
    [SerializeField] TextMeshProUGUI playerCountText; // Kéo Text hiển thị số người vào đây

    // Giả lập trạng thái phòng (Sau này Photon mạng sẽ thay thế các biến này)
    public bool isHost = true; // Kiểu tra xem có phải chủ phòng không
    private int currentPlayers = 1;
    private int maxPlayers = 9;

    void Start()
    {
        // Logic 1: Chỉ có chủ phòng (Host) mới nhìn thấy nút START để bắt đầu game
        if (startButton != null)
        {
            startButton.SetActive(isHost);
        }

        UpdatePlayerCountUI();
    }

    // Logic 2: Cập nhật hiển thị số lượng người chơi không vượt quá 9
    public void UpdatePlayerCountUI()
    {
        if (playerCountText != null)
        {
            playerCountText.text = currentPlayers + "/" + maxPlayers + " Players";
        }
    }

    // Hàm gọi khi nhấn nút START
    public void PressStartGame()
    {
        Debug.Log("Đang tải Scene bản đồ chính...");

        // Thay chữ "Skeld" bằng TÊN CHÍNH XÁC của Scene map Skeld trong dự án của bạn
        SceneManager.LoadScene("Skeld");
    }
}