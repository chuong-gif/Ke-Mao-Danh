using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    public static Player localPlayer;

    /*
     * Lưu những điểm spawn đã được Puppet sử dụng.
     * Nhờ vậy 2 Puppet không bị spawn trùng một chỗ.
     */
    private static readonly List<Transform> usedPuppetSpawnPoints = new List<Transform>();

    /*
     * Tự xóa dữ liệu cũ mỗi lần bắt đầu chạy game.
     * Tránh trường hợp Stop rồi Play lại nhưng danh sách spawn vẫn còn.
     */
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        localPlayer = null;
        usedPuppetSpawnPoints.Clear();
    }

    [Header("Network Authority")]
    public bool isLocalPlayer = true;

    [Header("Components")]
    public Animator PlayerAnimator;
    public Camera PlayerCamera;
    public float Speed = 10f;

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

    [Header("Movement Control")]
    public bool hasControl = true;

    public enum TypePlayer
    {
        Impostor,
        Crew
    }

    [Header("Audio")]
    public AudioSource footstepSource;

    [Header("Puppet Random Spawn")]
    [Tooltip("Bật cho các Puppet_Crewmate để chúng random vị trí khi bắt đầu game.")]
    public bool RandomizeSpawnPosition = false;

    [Tooltip("Tên object cha đang chứa tất cả các điểm Spawn trong Hierarchy.")]
    public string SpawnPointParentName = "PuppetSpawnPoint";

    [Tooltip("Không cho Puppet xuất hiện quá gần Player chính.")]
    public float MinDistanceFromPlayer = 2f;

    private void Start()
    {
        hasControl = true;
        originalScale = transform.localScale;
        PlayerRigidbody = GetComponent<Rigidbody2D>();

        if (isLocalPlayer)
        {
            SetupLocalPlayer();
        }
        else
        {
            SetupPuppet();
        }
    }

    // =========================================================
    // KHỞI TẠO PLAYER CHÍNH
    // =========================================================

    private void SetupLocalPlayer()
    {
        localPlayer = this;

        PlayerHud = GameObject.FindWithTag("PlayerUI");

        if (PlayerCamera == null)
        {
            PlayerCamera = Camera.main;
        }

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

        // Random vai trò: 50% cơ hội làm Impostor
        Team = Random.value > 0.5f ? TypePlayer.Impostor : TypePlayer.Crew;

        Debug.Log("Vai trò của bạn ván này là: " + Team);

        UpdateRoleUI();
    }

    // =========================================================
    // KHỞI TẠO PUPPET
    // =========================================================

    private void SetupPuppet()
    {
        if (PlayerCamera != null)
        {
            PlayerCamera.gameObject.SetActive(false);
        }

        if (RandomizeSpawnPosition)
        {
            StartCoroutine(RandomizePuppetAtSpawnPoint());
        }
    }

    // =========================================================
    // RANDOM PUPPET VÀO CÁC ĐIỂM SPAWN
    // =========================================================

    private IEnumerator RandomizePuppetAtSpawnPoint()
    {
        /*
         * Chờ Player chính khởi tạo xong.
         * Việc này giúp kiểm tra Puppet không xuất hiện quá gần Player.
         */
        while (localPlayer == null)
        {
            Player[] allPlayers = FindObjectsOfType<Player>();

            foreach (Player player in allPlayers)
            {
                if (player.isLocalPlayer)
                {
                    localPlayer = player;
                    break;
                }
            }

            yield return null;
        }

        GameObject spawnParent = GameObject.Find(SpawnPointParentName);

        if (spawnParent == null)
        {
            Debug.LogError(
                "Không tìm thấy object tên '" +
                SpawnPointParentName +
                "'. Hãy kiểm tra Hierarchy."
            );

            yield break;
        }

        List<Transform> validSpawnPoints = new List<Transform>();

        /*
         * Tự lấy tất cả object con bên trong PuppetSpawnPoint.
         * Ví dụ: Spawn_Cafeteria_1, Spawn_Admin_1, Spawn_Storage_1...
         */
        foreach (Transform spawnPoint in spawnParent.transform)
        {
            if (spawnPoint == null)
            {
                continue;
            }

            // Không để các Puppet dùng trùng một điểm spawn.
            if (usedPuppetSpawnPoints.Contains(spawnPoint))
            {
                continue;
            }

            float distanceFromPlayer = Vector2.Distance(
                spawnPoint.position,
                localPlayer.transform.position
            );

            // Không chọn điểm quá gần Player chính.
            if (distanceFromPlayer < MinDistanceFromPlayer)
            {
                continue;
            }

            validSpawnPoints.Add(spawnPoint);
        }

        /*
         * Trường hợp hiếm: mọi điểm chưa dùng đều quá gần Player.
         * Khi đó vẫn chọn một điểm chưa được Puppet khác dùng,
         * để Puppet không nằm ở vị trí ban đầu ngoài ý muốn.
         */
        if (validSpawnPoints.Count == 0)
        {
            foreach (Transform spawnPoint in spawnParent.transform)
            {
                if (spawnPoint != null &&
                    !usedPuppetSpawnPoints.Contains(spawnPoint))
                {
                    validSpawnPoints.Add(spawnPoint);
                }
            }
        }

        if (validSpawnPoints.Count == 0)
        {
            Debug.LogWarning(
                gameObject.name +
                ": Không còn điểm spawn trống. " +
                "Hãy tạo thêm điểm con trong " +
                SpawnPointParentName +
                "."
            );

            yield break;
        }

        Transform selectedPoint = validSpawnPoints[
            Random.Range(0, validSpawnPoints.Count)
        ];

        usedPuppetSpawnPoints.Add(selectedPoint);

        transform.position = new Vector3(
            selectedPoint.position.x,
            selectedPoint.position.y,
            transform.position.z
        );

        Debug.Log(
            gameObject.name +
            " được random vào " +
            selectedPoint.name +
            " tại vị trí " +
            transform.position
        );
    }

    // =========================================================
    // DI CHUYỂN PLAYER CHÍNH
    // =========================================================

    private void Update()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        // Nếu đang làm Task hoặc trận đấu kết thúc thì Player đứng im.
        if (!hasControl)
        {
            movement = Vector3.zero;

            if (PlayerAnimator != null)
            {
                PlayerAnimator.SetFloat("Speed", 0f);
            }

            if (PlayerRigidbody != null)
            {
                PlayerRigidbody.linearVelocity = Vector2.zero;
            }

            StopFootstepAudio();

            return;
        }

        UpdateTaskProgress();

        movement = new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"),
            0f
        );

        UpdateFootstepAudio();
        UpdateAnimation();
        UpdateFacingDirection();
    }

    private void FixedUpdate()
    {
        if (!isLocalPlayer)
        {
            return;
        }

        if (!hasControl)
        {
            if (PlayerRigidbody != null)
            {
                PlayerRigidbody.linearVelocity = Vector2.zero;
            }

            return;
        }

        if (PlayerRigidbody != null)
        {
            PlayerRigidbody.MovePosition(
                transform.position + movement * Speed * Time.fixedDeltaTime
            );
        }

        if (PlayerCamera != null)
        {
            PlayerCamera.transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                PlayerCamera.transform.position.z
            );
        }
    }

    private void UpdateTaskProgress()
    {
        if (PlayerHud == null)
        {
            return;
        }

        Slider progressSlider = PlayerHud.GetComponentInChildren<Slider>();

        if (progressSlider != null)
        {
            progressSlider.value = ProgressTasks.GetProgress();
        }
    }

    private void UpdateFootstepAudio()
    {
        if (footstepSource == null)
        {
            return;
        }

        if (movement.magnitude > 0f && hasControl && !isGhost)
        {
            if (!footstepSource.isPlaying)
            {
                footstepSource.Play();
            }
        }
        else
        {
            StopFootstepAudio();
        }
    }

    private void StopFootstepAudio()
    {
        if (footstepSource != null && footstepSource.isPlaying)
        {
            footstepSource.Stop();
        }
    }

    private void UpdateAnimation()
    {
        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetFloat("Speed", movement.magnitude);
        }
    }

    private void UpdateFacingDirection()
    {
        if (movement.x < -0.001f)
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }
        else if (movement.x > 0.001f)
        {
            transform.localScale = new Vector3(
                Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }
    }

    // =========================================================
    // HỆ THỐNG KILL
    // =========================================================

    public void KillTarget()
    {
        if (!isLocalPlayer ||
            Team != TypePlayer.Impostor ||
            targetPlayer == null)
        {
            return;
        }

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.killSFX);
        }

        Debug.Log("Impostor đã giết nạn nhân: " + targetPlayer.name);

        if (bodyDeadPrefab != null)
        {
            GameObject newBody = Instantiate(
                bodyDeadPrefab,
                targetPlayer.transform.position,
                Quaternion.identity
            );

            Body_Dead deadScript = newBody.GetComponent<Body_Dead>();
            Transform victimSprite = targetPlayer.transform.Find("Sprite");

            if (deadScript != null && victimSprite != null)
            {
                SpriteRenderer spriteRenderer =
                    victimSprite.GetComponent<SpriteRenderer>();

                if (spriteRenderer != null)
                {
                    deadScript.SetColor(spriteRenderer.color);
                }
            }
        }

        targetPlayer.DieAndBecomeGhost();
        targetPlayer = null;

        SetKillButtonInteractable(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        Player hitPlayer = collision.GetComponent<Player>();

        if (hitPlayer == null)
        {
            hitPlayer = collision.GetComponentInParent<Player>();
        }

        if (hitPlayer == null || hitPlayer == this)
        {
            return;
        }

        if (Team == TypePlayer.Impostor &&
            hitPlayer.Team == TypePlayer.Crew &&
            !hitPlayer.isGhost)
        {
            targetPlayer = hitPlayer;
            SetKillButtonInteractable(true);

            Debug.Log("Đã lọt vào tầm đánh Crewmate: " + hitPlayer.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!isLocalPlayer)
        {
            return;
        }

        Player hitPlayer = collision.GetComponent<Player>();

        if (hitPlayer == null)
        {
            hitPlayer = collision.GetComponentInParent<Player>();
        }

        if (hitPlayer != null && hitPlayer == targetPlayer)
        {
            targetPlayer = null;
            SetKillButtonInteractable(false);
        }
    }

    private void SetKillButtonInteractable(bool interactable)
    {
        if (killButton == null)
        {
            return;
        }

        Button btn = killButton.GetComponent<Button>();

        if (btn != null)
        {
            btn.interactable = interactable;
        }
    }

    // =========================================================
    // HỆ THỐNG GHOST
    // =========================================================

    public void DieAndBecomeGhost()
    {
        if (isGhost)
        {
            return;
        }

        isGhost = true;

        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetBool("IsDead", true);
        }

        Transform mySprite = transform.Find("Sprite");

        if (mySprite != null)
        {
            SpriteRenderer spriteRenderer =
                mySprite.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                Color ghostColor = spriteRenderer.color;
                ghostColor.a = 0.5f;
                spriteRenderer.color = ghostColor;
            }
        }

        Collider2D myCollider = GetComponent<Collider2D>();

        if (myCollider != null)
        {
            myCollider.isTrigger = true;
        }

        ProgressTasks.CheckMatchState();
    }

    private void OnDisable()
    {
        if (isLocalPlayer)
        {
            StopFootstepAudio();
        }
    }

    // =========================================================
    // ROLE VÀ MÀU NHÂN VẬT
    // =========================================================

    private void UpdateRoleUI()
    {
        if (Team == TypePlayer.Impostor)
        {
            if (useButton != null)
            {
                useButton.SetActive(false);
            }

            if (killButton != null)
            {
                killButton.SetActive(true);
            }
        }
        else
        {
            if (useButton != null)
            {
                useButton.SetActive(true);
            }

            if (killButton != null)
            {
                killButton.SetActive(false);
            }
        }
    }

    public void SetColor(Color newColor)
    {
        Transform spriteChild = transform.Find("Sprite");

        if (spriteChild == null)
        {
            return;
        }

        SpriteRenderer spriteRenderer =
            spriteChild.GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            spriteRenderer.color = newColor;
        }
    }
}