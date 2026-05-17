using UnityEngine;
using UnityEngine.SceneManagement;

public class Custom_Color : MonoBehaviour
{
    [SerializeField] Color[] allColors;

    public void SetColor(int colorIndex)
    {
        if (Player.localPlayer != null)
        {
            Color newColor = allColors[colorIndex];

            // 1. Đổi màu ngay lập tức cho nhân vật ở Lobby xem thử
            Player.localPlayer.SetColor(newColor);

            // 2. CẤT MÀU NÀY VÀO GAMEMANAGER ĐỂ MANG SANG MAP SKELD
            if (GameManager.Instance != null)
            {
                GameManager.Instance.SelectedColor = newColor;
            }
        }
        else
        {
            Debug.LogWarning("Chưa tìm thấy localPlayer trong Scene!");
        }
    }

    public void NextScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}