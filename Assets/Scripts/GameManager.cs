using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Singleton để các script khác dễ dàng gọi tới
    public static GameManager Instance;

    // Biến lưu trữ màu sắc nhân vật đã chọn
    public Color SelectedColor = Color.white;

    private void Awake()
{
    // Logic tạo Singleton chuẩn
    if (Instance == null)
    {
        Instance = this;
        
        // LỆNH QUYẾT ĐỊNH: Giữ lại GameManager này xuyên suốt các Scene
        DontDestroyOnLoad(gameObject); 
    }
    else
    {
        Destroy(gameObject);
    }
}
}