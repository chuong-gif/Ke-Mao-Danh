using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player localPlayer;

    [Header("Network Authority")]
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
    private Player targetPlayer;
    public bool isGhost = false;

    // QUAN TRỌNG: Mặc định vào game phải bằng TRUE để được đi lại tự do
    [Header("Movement Control")]
    public bool hasControl = true;

    public enum TypePlayer { Impostor, Crew }

    [Header("Audio")]
    public AudioSource footstepSource; // Kéo Audio Source bước chân vào đây

    void Start()
    {
        hasControl = true;
        originalScale = transform.localScale;
        PlayerRigidbody = GetComponent<Rigidbody2D>();

        if (isLocalPlayer)
        {
            localPlayer = this;
            PlayerHud = GameObject.FindWithTag("PlayerUI");
            if (PlayerCamera == null) PlayerCamera = Camera.main;

            if (GameManager.Instance != null)
            {
                SetColor(GameManager.Instance.SelectedColor);
                Debug.Log("Player chính đã tự động lấy lại màu từ GameManager!");
            }

            if (killButton != null)
            {
                Button btn = killButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(KillTarget);
                }
            }
            // RANDOM VAI TRÒ (50% cơ hội làm Impostor)
            Team = (Random.value > 0.5f) ? TypePlayer.Impostor : TypePlayer.Crew;
            Debug.Log("Vai trò của bạn ván này là: " + Team);

            UpdateRoleUI();
        }
        else
        {
            if (PlayerCamera != null) PlayerCamera.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        // CHẶN ĐỌC PHÍM: Nếu đang làm Task hoặc đã thắng game thì đứng im
        if (!hasControl)
        {
            movement = Vector3.zero;
            if (PlayerAnimator != null) PlayerAnimator.SetFloat("Speed", 0f);
            if (PlayerRigidbody != null) PlayerRigidbody.linearVelocity = Vector2.zero;
            return;
        }

        if (PlayerHud != null)
        {
            var ProgressSlider = PlayerHud.GetComponentInChildren<Slider>();
            if (ProgressSlider != null) ProgressSlider.value = ProgressTasks.GetProgress();
        }

        movement = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);

        // QUẢN LÝ TIẾNG BƯỚC CHÂN
        if (footstepSource != null)
        {
            if (movement.magnitude > 0 && hasControl && !isGhost)
            {
                if (!footstepSource.isPlaying) footstepSource.Play(); // Vừa đi vừa phát nhạc
            }
            else
            {
                footstepSource.Stop(); // Đứng im hoặc làm ma thì tắt tiếng
            }
        }

        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetFloat("Speed", movement.magnitude);
        }

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
        if (!isLocalPlayer) return;

        // CHẶN DI CHUYỂN VẬT LÝ VÀ TRIỆT TIÊU VẬN TỐC TRƯỢT
        if (!hasControl)
        {
            if (PlayerRigidbody != null) PlayerRigidbody.linearVelocity = Vector2.zero;
            return;
        }

        if (PlayerRigidbody != null)
        {
            PlayerRigidbody.MovePosition(transform.position + movement * Speed * Time.fixedDeltaTime);
        }

        if (PlayerCamera != null)
        {
            PlayerCamera.transform.position = new Vector3(transform.position.x, transform.position.y, PlayerCamera.transform.position.z);
        }
    }

    public void KillTarget()
    {
        if (!isLocalPlayer || Team != TypePlayer.Impostor || targetPlayer == null) return;

        // PHÁT TIẾNG GIẾT NGƯỜI
        if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.killSFX);

        Debug.Log("Impostor đã giết nạn nhân: " + targetPlayer.name);

        if (bodyDeadPrefab != null)
        {
            GameObject newBody = Instantiate(bodyDeadPrefab, targetPlayer.transform.position, Quaternion.identity);
            Body_Dead deadScript = newBody.GetComponent<Body_Dead>();

            Transform victimSprite = targetPlayer.transform.Find("Sprite");
            if (deadScript != null && victimSprite != null)
            {
                SpriteRenderer sr = victimSprite.GetComponent<SpriteRenderer>();
                if (sr != null) deadScript.SetColor(sr.color);
            }
        }

        targetPlayer.DieAndBecomeGhost();
        targetPlayer = null;
        if (killButton != null) killButton.GetComponent<Button>().interactable = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLocalPlayer) return;

        Player hitPlayer = collision.GetComponent<Player>();
        if (hitPlayer != null && hitPlayer != this)
        {
            if (Team == TypePlayer.Impostor && hitPlayer.Team == TypePlayer.Crew)
            {
                targetPlayer = hitPlayer;
                if (killButton != null) killButton.GetComponent<Button>().interactable = true;
                Debug.Log("Đã lọt vào tầm đánh Crewmate: " + hitPlayer.name);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isLocalPlayer) return;

        Player hitPlayer = collision.GetComponent<Player>();
        if (hitPlayer != null && hitPlayer == targetPlayer)
        {
            targetPlayer = null;
            if (killButton != null) killButton.GetComponent<Button>().interactable = false;
        }
    }

    public void DieAndBecomeGhost()
    {
        if (isGhost) return;
        isGhost = true;

        if (PlayerAnimator != null) PlayerAnimator.SetBool("IsDead", true);

        Transform mySprite = transform.Find("Sprite");
        if (mySprite != null)
        {
            SpriteRenderer sr = mySprite.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                Color c = sr.color; c.a = 0.5f; sr.color = c;
            }
        }

        Collider2D myCollider = GetComponent<Collider2D>();
        if (myCollider != null) myCollider.isTrigger = true;

        // BÁO CÁO CÁI CHẾT ĐỂ KIỂM TRA XEM IMPOSTOR ĐÃ THẮNG CHƯA
        ProgressTasks.CheckMatchState();
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