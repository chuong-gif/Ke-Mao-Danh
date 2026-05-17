using UnityEngine;
using TMPro; // Thêm thư viện này để đọc dữ liệu từ ô nhập

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    public GameObject menuMain;
    public GameObject menuOnline;
    public GameObject menuCreateGame;
    public GameObject menuFindGame;
    public GameObject menuEnterCode;

    [Header("Private Room")]
    public TMP_InputField codeInputField;

    public void SwitchToMenu(int menuIndex)
    {
        // Tắt tất cả các menu
        menuMain.SetActive(false);
        menuOnline.SetActive(false);
        menuCreateGame.SetActive(false);
        menuFindGame.SetActive(false);
        menuEnterCode.SetActive(false);

        // Bật menu theo số thứ tự
        if (menuIndex == 1) menuMain.SetActive(true);
        else if (menuIndex == 2) menuOnline.SetActive(true);
        else if (menuIndex == 3) menuCreateGame.SetActive(true);
        else if (menuIndex == 4) menuFindGame.SetActive(true);
        else if (menuIndex == 5) menuEnterCode.SetActive(true);
    }

    // Hàm gọi khi nhấn JOIN ở menu Enter Code
    public void JoinPrivateRoom()
    {
        string code = codeInputField.text;
        if (code.Length >= 4)
        {
            Debug.Log("Đang kết nối vào phòng với mã: " + code);
            // Sau này sẽ thêm code Photon/Netcode vào đây để kết nối thật
        }
    }
}