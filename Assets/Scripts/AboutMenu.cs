using UnityEngine;

public class AboutMenu : MonoBehaviour
{
    [SerializeField] private GameObject aboutPanel;

    private void Start()
    {
        aboutPanel.SetActive(false);
    }

    public void OpenAbout()
    {
        aboutPanel.SetActive(true);
    }

    public void CloseAbout()
    {
        aboutPanel.SetActive(false);
    }
}