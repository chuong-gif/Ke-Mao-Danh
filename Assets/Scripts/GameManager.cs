using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton để các script khác dễ dàng gọi tới
    public static GameManager Instance;

    // Biến lưu trữ màu sắc nhân vật đã chọn
    public Color SelectedColor = Color.white;

    void Awake()
    {
        // Kiểm tra nếu đã có GameManager rồi thì xóa cái mới này đi, tránh trùng lặp
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // LỆNH QUAN TRỌNG: Giữ lại Object này khi đổi Scene
        }
        else
        {
            Destroy(gameObject);
        }
    }
}