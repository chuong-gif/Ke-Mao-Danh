using UnityEngine;
using UnityEngine.SceneManagement;

public class Custom_Color : MonoBehaviour
{
    [SerializeField] Color[] allColors;
    public void SetColor(int colorIndex)
    {
        Player_My.localPlayer.SetColor(allColors[colorIndex]);
    }
    public void NextScene(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }
}
