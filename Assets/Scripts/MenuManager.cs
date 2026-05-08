using UnityEngine;

public class MenuManager : MonoBehaviour
{
    public GameObject menMenu_Main;
    public GameObject Menu_Online;
    public GameObject Menu_Create_Game;

    // Hàm này sẽ gọi khi bạn bấm nút
    public void SwitchToMenu(int menuIndex)
    {
        // Tắt tất cả trước
        menMenu_Main.SetActive(false);
        Menu_Online.SetActive(false);
        Menu_Create_Game.SetActive(false);

        // Bật cái được chọn
        if (menuIndex == 1) menMenu_Main.SetActive(true);
        else if (menuIndex == 2) Menu_Online.SetActive(true);
        else if (menuIndex == 3) Menu_Create_Game.SetActive(true);
    }
}