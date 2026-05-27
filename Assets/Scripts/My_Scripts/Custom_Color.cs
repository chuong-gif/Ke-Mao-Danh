using UnityEngine;
using UnityEngine.SceneManagement;

public class Custom_Color : MonoBehaviour
{
    [SerializeField] Color[] allColors; // Mảng này sẽ chứa tất cả màu sắc có thể chọn, kéo và thả màu vào đây trong Inspector

    public void SetColor(int colorIndex)
    {
        // 1. Nhuộm màu trực tiếp cho nhân vật ở sảnh chờ xem trước
        if (Player.localPlayer != null)
        {
            Player.localPlayer.SetColor(allColors[colorIndex]);
        }

        // 2. QUAN TRỌNG: Lưu màu này vào GameManager để mang sang map Skeld
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SelectedColor = allColors[colorIndex];
            Debug.Log("Đã lưu màu vào GameManager để chuẩn bị chuyển map!");
        }
    }

    // Hàm này có thể được gọi khi nhấn nút "Start Game" để chuyển sang map Skeld
    public void NextScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}