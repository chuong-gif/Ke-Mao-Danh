using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player localPlayer;

    [Header("Network Authority")]
    // Khi test offline, ta để true. Khi làm online, Photon sẽ tự đặt true cho bạn, và false cho các "con rối"
    public bool isLocalPlayer = true;

    [Header("Components")]
    public Animator PlayerAnimator;
    public Camera PlayerCamera;
    public float Speed;
    private Rigidbody2D PlayerRigidbody;
    private Vector3 movement;

    [Header("Identity")]
    public TypePlayer Team = TypePlayer.Crew;
    private GameObject PlayerHud;
    private Vector3 originalScale;

    [Header("Kill System")]
    [SerializeField] private GameObject bodyDeadPrefab;
    [SerializeField] private GameObject useButton;
    [SerializeField] private GameObject killButton;
    private Player targetPlayer; // Đổi từ GameObject thành Player để check vai trò trực tiếp
    private bool isGhost = false;

    public enum TypePlayer { Impostor, Crew }

    void Start()
    {
        // CHỈ NGƯỜI CHƠI THẬT (BẢN THÂN BẠN) MỚI CHẠY LOGIC KHỞI TẠO NÀY
        if (isLocalPlayer)
        {
            localPlayer = this;
            PlayerHud = GameObject.FindWithTag("PlayerUI");
            if (PlayerCamera == null) PlayerCamera = Camera.main;

            // ĐOẠN ĐỒNG BỘ MÀU: Đã được đưa vào ĐÂY để CHỈ nhuộm màu cho đúng máy bạn
            if (GameManager.Instance != null)
            {
                SetColor(GameManager.Instance.SelectedColor);
                Debug.Log("Player chính đã tự động lấy lại màu từ GameManager!");
            }

            // Kết nối nút KILL bấm bằng chuột trên UI
            if (killButton != null)
            {
                Button btn = killButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(KillTarget);
                }
            }

            UpdateRoleUI();
        }
        else // ĐỐI VỚI CÁC CON RỐI (PUPPET)
        {
            // Tắt Camera đi để không bị đè góc nhìn của người chơi chính
            if (PlayerCamera != null) PlayerCamera.gameObject.SetActive(false);

            // Dòng này vẫn comment lại để giữ va chạm cứng/mềm cho Bot test offline như cũ
            // if (PlayerRigidbody != null) PlayerRigidbody.simulated = false;
        }

        // Những thành phần dùng chung cho cả bạn lẫn con rối (như Rigidbody, kích thước gốc) thì để ở ngoài
        originalScale = transform.localScale;
        PlayerRigidbody = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // NẾU LÀ CON RỐI: Tuyệt đối không đọc Input bàn phím, không cập nhật UI Slider nhiệm vụ
        if (!isLocalPlayer) return;

        if (PlayerHud != null)
        {
            var ProgressSlider = PlayerHud.GetComponentInChildren<Slider>();
            if (ProgressSlider != null) ProgressSlider.value = ProgressTasks.GetProgress();
        }

        // Lấy Input di chuyển bàn phím
        movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);

        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetFloat("Speed", movement.magnitude);
        }

        // Lật mặt nhân vật
        if (Input.GetAxis("Horizontal") < -0.001f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
        else if (Input.GetAxis("Horizontal") > 0.001f)
        {
            transform.localScale = new Vector3(Mathf.Abs(originalScale.x), originalScale.y, originalScale.z);
        }
    }

    private void FixedUpdate()
    {
        // NẾU LÀ CON RỐI: Không tự di chuyển bằng code vật lý, vị trí của họ sẽ do mạng kéo đi
        if (!isLocalPlayer) return;

        if (PlayerRigidbody != null)
        {
            PlayerRigidbody.MovePosition(transform.position + movement * Speed * Time.fixedDeltaTime);
        }

        if (PlayerCamera != null)
        {
            PlayerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, PlayerCamera.transform.position.z);
        }
    }

    // --- LOGIC XỬ LÝ GIẾT NGƯỜI DÀNH CHO IMPOSTOR THẬT ---
    public void KillTarget()
    {
        // Điều kiện an toàn: Mình là LocalPlayer, là Impostor, và đang có mục tiêu Crew hợp lệ đứng gần
        if (!isLocalPlayer || Team != TypePlayer.Impostor || targetPlayer == null) return;

        Debug.Log("Impostor đã giết nạn nhân: " + targetPlayer.name);

        // 1. SINH XÁC CHẾT tại vị trí của nạn nhân
        if (bodyDeadPrefab != null)
        {
            GameObject newBody = Instantiate(bodyDeadPrefab, targetPlayer.transform.position, Quaternion.identity);
            Body_Dead deadScript = newBody.GetComponent<Body_Dead>();

            // Lấy màu Sprite từ con rối bị giết để nhuộm cho cái xác
            Transform victimSprite = targetPlayer.transform.Find("Sprite");
            if (deadScript != null && victimSprite != null)
            {
                SpriteRenderer sr = victimSprite.GetComponent<SpriteRenderer>();
                if (sr != null) deadScript.SetColor(sr.color);
            }
        }

        // 2. KÍCH HOẠT TRẠNG THÁI CHẾT CỦA CON RỐI
        // Gọi hàm DieAndBecomeGhost() của chính con rối đó để nó tự đổi Animation thành ma và mờ đi
        targetPlayer.DieAndBecomeGhost();

        // Giết xong thì xóa mục tiêu hiện tại và làm mờ nút KILL
        targetPlayer = null;
        if (killButton != null) killButton.GetComponent<Button>().interactable = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLocalPlayer) return; // Nếu mình là con rối thì không chạy code này

        // Kiểm tra xem thực thể chạm phải có component Player không (nghĩa là một người chơi khác)
        Player hitPlayer = collision.GetComponent<Player>();

        if (hitPlayer != null && hitPlayer != this) // Không tự chọn chính mình
        {
            // ĐIỀU KIỆN QUYẾT ĐỊNH: Mình là Impostor VÀ nạn nhân phải là Crewmate
            if (Team == TypePlayer.Impostor && hitPlayer.Team == TypePlayer.Crew)
            {
                targetPlayer = hitPlayer; // Khóa mục tiêu

                if (killButton != null) killButton.GetComponent<Button>().interactable = true; // Sáng nút KILL
                Debug.Log("Đã lọt vào tầm đánh Crewmate: " + hitPlayer.name);
            }
            else if (Team == TypePlayer.Impostor && hitPlayer.Team == TypePlayer.Impostor)
            {
                Debug.Log("Chạm phải Impostor đồng đội, nút KILL không hoạt động!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isLocalPlayer) return;

        Player hitPlayer = collision.GetComponent<Player>();
        // Nếu đi xa khỏi mục tiêu hiện tại
        if (hitPlayer != null && hitPlayer == targetPlayer)
        {
            targetPlayer = null;
            if (killButton != null) killButton.GetComponent<Button>().interactable = false; // Tắt nút KILL
        }
    }
    // Hàm này bổ sung để con rối hoặc chính mình gọi khi bị giết
    public void DieAndBecomeGhost()
    {
        if (isGhost) return;
        isGhost = true;

        // Bật hoạt ảnh chết
        if (PlayerAnimator != null) PlayerAnimator.SetBool("IsDead", true);

        // Làm mờ sprite
        Transform mySprite = transform.Find("Sprite");
        if (mySprite != null)
        {
            SpriteRenderer sr = mySprite.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color; c.a = 0.5f; sr.color = c;
            }
        }

        // Chuyển Collider thành Trigger để đi xuyên tường
        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null) myCollider.isTrigger = true;
    }

    private void UpdateRoleUI()
    {
        if (Team == TypePlayer.Impostor)
        {
            if (useButton != null) useButton.SetActive(false);
            if (killButton != null) killButton.SetActive(true);
        }
        else
        {
            if (useButton != null) useButton.SetActive(true);
            if (killButton != null) killButton.SetActive(false);
        }
    }

    public void SetColor(Color newColor)
    {
        Transform spriteChild = transform.Find("Sprite");
        if (spriteChild != null)
        {
            SpriteRenderer spriteRenderer = spriteChild.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) spriteRenderer.color = newColor;
        }
    }
}