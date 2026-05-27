using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    protected bool CanInteract = true; // Biến cho biết đối tượng có tương tác được ko
    public Sprite CustomActionSprite; // Sprite riêng lẻ của nút use
    private Sprite BaseUseSprite; // lưu sprite mặt định của nút use
    protected Player PlayerEnt; // tham chiếu đến Player để bật tắt điều khiển khi tương tác
    protected Button ActionButton; // tham chiếu đến nút use trên UI để bật tắt và đổi sprite khi cần
    protected bool InArea = false; //biến trong vùng tương tác
    protected bool Using = false; // cho biết hiện có tương tác được không


    void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        //tìm tag là player
        PlayerEnt = GameObject.FindWithTag("Player").GetComponentInChildren<Player>();

        // tìm UI của player
        GameObject playerUiCanvas = GameObject.FindWithTag("PlayerUI");
        if (playerUiCanvas != null)
        {
            // tìm nút use
            Transform useBtnTransform = playerUiCanvas.transform.Find("Canvas/Btn_Use");
            // thử tìm tên khác nếu không thấy
            if (useBtnTransform == null) useBtnTransform = playerUiCanvas.transform.Find("Use_Button");

            if (useBtnTransform != null)
            {
                ActionButton = useBtnTransform.GetComponent<Button>(); // lấy component Button
                BaseUseSprite = ActionButton.gameObject.GetComponent<Image>().sprite; // lưu lại sprite mặt định để sau dùng
            }
        }
    }

    // Hàm này sẽ được gọi liên tục trong Update để kiểm tra input tương tác
    public virtual void CheckInteractionInput()
    {
        if (InArea && !Using) // trong vùng và vật thể chưa được sử dụng
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                PlayerEnt.enabled = false;
                Using = true;

            }

        }

        // khi đang tương có thể nhân ESC để thoát
        if (Using)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitAction();

            }
        }
    }

    void Update()
    {
        CheckInteractionInput();

    }

    // hàm này được chạy khi nhân nút Use
    public virtual void Interact()
    {
        if (!CanInteract) // nếu không thể tương tác thì thôi, không làm gì cả
            return;

        PlayerEnt.enabled = false; // dừng di chuyển của player
        Using = true; // đánh dấu đang tương tác
        ActionButton.interactable = false; // tắt nút use để tránh spam tương tác
    }

    // hàm này được chạy khi nhân nút ESC để thoát tương tác
    public virtual void ExitAction()
    {
        Using = false; // đánh dấu không còn tương tác
        PlayerEnt.enabled = true; // bật lại điều khiển di chuyển cho player
        ActionButton.interactable = true; // bật lại nút use để có thể tương tác tiếp
    }

    // hàm này được chạy khi player rời khỏi vùng tương tác
    public virtual void ExitArea()
    {
        ActionButton.gameObject.GetComponent<Image>().sprite = BaseUseSprite;
        Using = false;
        ActionButton.interactable = false;
        InArea = false;
        ActionButton.onClick.RemoveAllListeners();
    }

    // hàm này được chạy khi player vào vùng tương tác
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (!CanInteract)
                return;

            ActionButton.onClick.AddListener((UnityEngine.Events.UnityAction)this.Interact);
            if (CustomActionSprite)
            {
                ActionButton.gameObject.GetComponent<Image>().sprite = CustomActionSprite;
            }
            ActionButton.interactable = true;
            InArea = true;


        }
    }

    // bật tắt khả năng tương tác
    public void SetCanInteract(bool value)
    {
        CanInteract = value;
        if (InArea && !value)
        {
            ExitArea();
        }

    }

    // khi player rời khỏi vùng tương tác thì gọi hàm ExitArea để reset trạng thái
    private void OnTriggerExit2D(Collider2D collision)
    {

        if (collision.CompareTag("Player"))
        {
            ExitArea();

        }
    }
}
