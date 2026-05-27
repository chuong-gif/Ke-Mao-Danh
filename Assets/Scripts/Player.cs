using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // Tham chiếu đến Player chính tại client hiện tại.
    public static Player localPlayer;

    // Danh sách các điểm spawn của Puppet đã được sử dụng.
    // Dùng để ngăn các Puppet khác spawn trùng chỗ nhau.
    private static readonly List<Transform> usedPuppetSpawnPoints = new List<Transform>();

    // Reset các dữ liệu tĩnh mỗi khi game được tải lại.
    // Điều này đảm bảo dữ liệu không bị giữ lại giữa các lần Play/Stop trong Editor.
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticData()
    {
        localPlayer = null;
        usedPuppetSpawnPoints.Clear();
    }
    // Giá trị cho biết có phải player này của mình không
    [Header("Network Authority")]
    public bool isLocalPlayer = true;

    [Header("Components")]
    public Animator PlayerAnimator;
    public Camera PlayerCamera;
    public float Speed = 10f;

    private Rigidbody2D PlayerRigidbody;
    private Vector3 movement;

    // giá trị sát định vai trò player
    [Header("Identity")]
    public TypePlayer Team = TypePlayer.Crew;

    private GameObject PlayerHud;
    private Vector3 originalScale;

    // Dưới là giá trị cái xác, nút use và nút kill.
    [Header("Kill System")]
    [SerializeField] private GameObject bodyDeadPrefab;
    [SerializeField] private GameObject useButton;
    [SerializeField] private GameObject killButton;

    // giá trị sống hay chết
    private Player targetPlayer;
    public bool isGhost = false;

    // giá trị có thể di chuyển, true có thể di chuyển
    [Header("Movement Control")]
    public bool hasControl = true;

    // tạo kiểu dữ liệu mà bên trong có vài giá trị
    public enum TypePlayer
    {
        Impostor,
        Crew
    }

    [Header("Audio")]
    public AudioSource footstepSource; // âm thanh bước chân

    [Header("Puppet Random Spawn")]
    [Tooltip("Bật cho các Puppet_Crewmate để chúng random vị trí khi bắt đầu game.")]
    public bool RandomizeSpawnPosition = false;

    [Tooltip("Tên object cha đang chứa tất cả các điểm Spawn trong Hierarchy.")]
    public string SpawnPointParentName = "PuppetSpawnPoint";

    [Tooltip("Không cho Puppet xuất hiện quá gần Player chính.")]
    public float MinDistanceFromPlayer = 2f; //khoảng cách tối thiểu với player

    private void Start()
    {
        // Khởi tạo các giá trị cơ bản cho player khi scene bắt đầu.
        hasControl = true;
        originalScale = transform.localScale;
        PlayerRigidbody = GetComponent<Rigidbody2D>();

        if (isLocalPlayer)
        {
            SetupLocalPlayer(); // nếu là player
        }
        else
        {
            SetupPuppet(); // nếu là con rối
        }
    }

    //khởi tạo player chính, chỉ có player chính mới có quyền điều khiển và tương tác với UI
    private void SetupLocalPlayer()
    {
        // Đánh dấu player này là client local và lưu tham chiếu.
        localPlayer = this;

        PlayerHud = GameObject.FindWithTag("PlayerUI"); // có thể đổi tên UI của player ở chỗ này

        if (PlayerCamera == null)
        {
            PlayerCamera = Camera.main;
        }

        // truy cập vào GameManager để lấy giá trị mầu được chọn ở sảnh chờ
        if (GameManager.Instance != null)
        {
            SetColor(GameManager.Instance.SelectedColor);
            Debug.Log("Player chính đã tự động lấy lại màu từ GameManager!");
        }

        // tìm nút kill và gán chức năng cho nó
        if (killButton != null)
        {
            Button btn = killButton.GetComponent<Button>();

            if (btn != null)
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(KillTarget); //nút kill gọi hàm KillTarget khi được nhấn
            }
        }

        // Phân vai cho player chính: 50% cơ hội là Impostor.
        Team = Random.value > 0.5f ? TypePlayer.Impostor : TypePlayer.Crew;

        Debug.Log("Vai trò của bạn ván này là: " + Team);

        UpdateRoleUI();
    }

    // trường hợp xử lý nếu là con rối
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
         * Việc này giúp Puppet không spawn trước khi biết vị trí Player chính.
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

            // Bỏ qua điểm đã bị một Puppet khác dùng.
            if (usedPuppetSpawnPoints.Contains(spawnPoint))
            {
                continue;
            }

            float distanceFromPlayer = Vector2.Distance(
                spawnPoint.position,
                localPlayer.transform.position
            );

            // Bỏ qua các điểm quá gần Player chính để tránh xuất hiện sát nhau.
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


    private void Update()
    {
        // nếu không phải player thì cút
        if (!isLocalPlayer)
        {
            return;
        }

        // trường hợp ko được điều khiển sẽ
        if (!hasControl)
        {
            movement = Vector3.zero;  // dừng đi chuyển

            if (PlayerAnimator != null)
            {
                PlayerAnimator.SetFloat("Speed", 0f); // tốc độ về 0 -> run animation idle
            }

            if (PlayerRigidbody != null)
            {
                PlayerRigidbody.linearVelocity = Vector2.zero;
            }

            StopFootstepAudio(); // hàm dừng tiếng bước chân

            return;
        }

        UpdateTaskProgress(); // hàm cập nhật tiến độ

        // Đọc đầu vào từ phím WASD / Arrow để điều khiển Player.
        movement = new Vector3(
            Input.GetAxis("Horizontal"),
            Input.GetAxis("Vertical"),
            0f
        );

        UpdateFootstepAudio(); //hàm phát tiếng bước chân
        UpdateAnimation(); // hàm cập nhật animation
        UpdateFacingDirection(); // hàm cập nhật hướng mặt theo chiều di chuyển
    }

    // xử lý vật lý di chuyển cho mượt mà
    private void FixedUpdate()
    {
        // nếu không phải player thì thoát
        if (!isLocalPlayer)
        {
            return;
        }

        // nếu không được điều khiển thì dừng mọi chuyển động và thoát.
        if (!hasControl)
        {
            if (PlayerRigidbody != null)
            {
                PlayerRigidbody.linearVelocity = Vector2.zero;
            }

            return;
        }

        // Di chuyển vật lý mượt mà theo velocity tính toán.
        if (PlayerRigidbody != null)
        {
            PlayerRigidbody.MovePosition(
                transform.position + movement * Speed * Time.fixedDeltaTime
            );
        }

        // Di chuyển camera theo player để camera luôn dõi theo.
        if (PlayerCamera != null)
        {
            PlayerCamera.transform.position = new Vector3(
                transform.position.x,
                transform.position.y,
                PlayerCamera.transform.position.z
            );
        }
    }

    // hàm cập nhật thanh tiến trình
    private void UpdateTaskProgress()
    {
        if (PlayerHud == null)
        {
            return;
        }

        // tìm slider trong HUD
        Slider progressSlider = PlayerHud.GetComponentInChildren<Slider>();

        if (progressSlider != null)
        {
            progressSlider.value = ProgressTasks.GetProgress(); //cập nhật giá trị thanh tiến trình
        }
    }

    // hàm phát tiếng bước chân
    private void UpdateFootstepAudio()
    {
        if (footstepSource == null)
        {
            return;
        }

        // Chỉ phát âm bước khi player đang di chuyển, có quyền điều khiển và không phải ghost.
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

    // hàm dừng âm thanh bước chân
    private void StopFootstepAudio()
    {
        if (footstepSource != null && footstepSource.isPlaying) // kiểm tra tồn tại và có đang phát ko
        {
            footstepSource.Stop();
        }
    }

    // hàm cập nhật animation
    private void UpdateAnimation()
    {
        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetFloat("Speed", movement.magnitude); // giá trị speed được cập nhật bởi giá trị đi chuyển
        }
    }

    // dùng để lật player khi đổi hướng đi chuyển
    private void UpdateFacingDirection()
    {
        if (movement.x < -0.001f) // khi sang trái
        {
            transform.localScale = new Vector3(
                -Mathf.Abs(originalScale.x),
                originalScale.y,
                originalScale.z
            );
        }
        else if (movement.x > 0.001f) // khi sang phải
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
        // Chỉ có player local là Impostor và có mục tiêu hợp lệ mới được kill.
        if (!isLocalPlayer ||
            Team != TypePlayer.Impostor ||
            targetPlayer == null)
        {
            return;
        }

        // phát âm thanh kill khi nhấn nút kill
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.killSFX);
        }

        Debug.Log("Impostor đã giết nạn nhân: " + targetPlayer.name);

        // tạo xác chết
        if (bodyDeadPrefab != null)
        {
            // nạn nhân chết đâu thì sinh xác chỗ đó
            GameObject newBody = Instantiate(
                bodyDeadPrefab,
                targetPlayer.transform.position,
                Quaternion.identity // không xoay
            );

            Body_Dead deadScript = newBody.GetComponent<Body_Dead>(); // lấy script Body_Dead để truyền màu cho xác chết
            Transform victimSprite = targetPlayer.transform.Find("Sprite"); // tìm con Sprite của nạn nhân để lấy màu

            if (deadScript != null && victimSprite != null) // nếu tìm thấy cả script và sprite của nạn nhân
            {
                //lấy màu của nạn nhân
                SpriteRenderer spriteRenderer =
                    victimSprite.GetComponent<SpriteRenderer>();

                // Tô mầu xác chết
                if (spriteRenderer != null)
                {
                    deadScript.SetColor(spriteRenderer.color);
                }
            }
        }

        targetPlayer.DieAndBecomeGhost(); // gọi hàm giết
        targetPlayer = null; // reset giá trị sau khi kill

        SetKillButtonInteractable(false); // cho nút kill quay về trạng thái tắt
    }

    // phát hiện va chạm thì chạy
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isLocalPlayer)
        {
            return;
        }
        // Kiểm tra xem đối tượng va chạm có phải là Player không
        Player hitPlayer = collision.GetComponent<Player>();

        // Tìm sâu hơn trong parent nếu không tìm thấy ở collider chính
        if (hitPlayer == null)
        {
            hitPlayer = collision.GetComponentInParent<Player>();
        }

        // bỏ qua bản thân player
        if (hitPlayer == null || hitPlayer == this)
        {
            return;
        }

        // Nếu Impostor tiếp cận Crewmate sống thì bật nút kill.
        if (Team == TypePlayer.Impostor &&
            hitPlayer.Team == TypePlayer.Crew &&
            !hitPlayer.isGhost)
        {
            targetPlayer = hitPlayer;
            SetKillButtonInteractable(true); // kích hoạt nút kill

            Debug.Log("Đã lọt vào tầm đánh Crewmate: " + hitPlayer.name);
        }
    }

    //khi rời vùng va chạm
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

        // Khi đối thủ rời vùng va chạm, reset mục tiêu kill và tắt nút.
        if (hitPlayer != null && hitPlayer == targetPlayer)
        {
            targetPlayer = null;
            SetKillButtonInteractable(false);
        }
    }

    // Kích hoạt hoặc vô hiệu hoá nút kill thông qua UI Button.
    private void SetKillButtonInteractable(bool interactable)
    {
        if (killButton == null)
        {
            return;
        }

        Button btn = killButton.GetComponent<Button>();

        if (btn != null)
        {
            // Kích hoạt hoặc vô hiệu hoá nút kill thông qua UI Button.
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

        // Chuyển player sang trạng thái ghost.
        isGhost = true;

        //chuyển sang animation chết
        if (PlayerAnimator != null)
        {
            PlayerAnimator.SetBool("IsDead", true);
        }

        // tìm Sprite để tô màu ghost
        Transform mySprite = transform.Find("Sprite");

        // Tô màu player.
        if (mySprite != null)
        {
            // lấy SpriteRenderer để chỉnh sửa màu
            SpriteRenderer spriteRenderer =
                mySprite.GetComponent<SpriteRenderer>();

            if (spriteRenderer != null)
            {
                Color ghostColor = spriteRenderer.color; // lấy mầu hiện tại
                ghostColor.a = 0.5f; // độ trong suốt
                spriteRenderer.color = ghostColor; // áp dụng mầu mới cho sprite
            }
        }

        // Tìm Collider 2D và chuyển nó thành trigger để ghost có thể đi xuyên qua vật thể.
        Collider2D myCollider = GetComponent<Collider2D>();

        if (myCollider != null)
        {
            myCollider.isTrigger = true;
        }

        // thông báo lại cho hệ thống trạng thái trò chơi
        ProgressTasks.CheckMatchState();
    }

    // khi player bị vô hiệu hoá (ví dụ khi chết hoặc rời khỏi scene), dừng âm thanh bước chân nếu đang phát.
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
        // Cập nhật button UI theo vai trò hiện tại của player.
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

    // Hàm này cho phép thay đổi màu của player, có thể được gọi từ GameManager khi chọn màu ở sảnh chờ.
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