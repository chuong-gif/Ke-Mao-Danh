using UnityEngine;
using UnityEngine.UI; // Cần thư viện này để điều khiển Button

public class LaptopInteract : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] GameObject colorMenuUI; // Panel bảng màu Custom_Color_Player
    [SerializeField] Button useButton;       // Kéo nút USE từ Canvas vào đây

    [Header("Button Visuals")]
    [SerializeField] Color normalColor = Color.gray;   // Màu khi ở xa
    [SerializeField] Color highlightColor = Color.white; // Màu sáng lên khi ở gần

    private bool isPlayerInside = false;

    void Start()
    {
        // Ban đầu ở xa thì tắt hiệu ứng nút đi
        if (useButton != null)
        {
            useButton.image.color = normalColor;
            useButton.interactable = false;

            // Lắng nghe sự kiện khi người chơi lấy chuột click thẳng vào nút USE trên màn hình
            useButton.onClick.RemoveAllListeners();
            useButton.onClick.AddListener(OpenColorMenu);
        }
    }

    void Update()
    {
        // Người chơi ở gần và bấm phím E trên bàn phím cũng mở được
        if (isPlayerInside && Input.GetKeyDown(KeyCode.E))
        {
            OpenColorMenu();
        }
    }

    void OpenColorMenu()
    {
        if (colorMenuUI != null)
        {
            colorMenuUI.SetActive(true);
        }
    }

    // Khi Player bước vào vùng Collider (Is Trigger) của máy tính
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = true;

            // Làm nút USE sáng lên và bấm được
            if (useButton != null)
            {
                useButton.image.color = highlightColor;
                useButton.interactable = true;
            }
        }
    }

    // Khi Player đi ra xa khỏi máy tính
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInside = false;

            // Làm nút USE tối lại và khóa không cho bấm
            if (useButton != null)
            {
                useButton.image.color = normalColor;
                useButton.interactable = false;
            }

            // Tự động đóng bảng màu nếu người chơi cố tình chạy ra xa
            if (colorMenuUI != null) colorMenuUI.SetActive(false);
        }
    }
}