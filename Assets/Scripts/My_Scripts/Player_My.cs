using UnityEngine;
using UnityEngine.InputSystem;

public class Player_My : MonoBehaviour
{
    public static Player_My localPlayer;

    [Header("Settings")]
    public bool hasControl = true;
    [SerializeField] float movementSpeed = 10f;
    [SerializeField] InputAction WASD;

    [Header("References")]
    [SerializeField] Transform spriteTransform;

    // Components
    Rigidbody2D myRB;
    Animator myAnim;
    SpriteRenderer myAvatarSprite;

    Vector2 movementInput;
    static Color myColor = Color.white; // Để static để lưu màu qua các Scene

    private void OnEnable() => WASD.Enable();
    private void OnDisable() => WASD.Disable();

    void Start()
    {
        if (hasControl)
        {
            localPlayer = this;
        }

        myRB = GetComponent<Rigidbody2D>();

        // Tự động tìm Sprite và Animator
        if (spriteTransform == null)
        {
            spriteTransform = transform.Find("Sprite");
        }

        // Lấy SpriteRenderer từ object Sprite để đổi màu
        if (spriteTransform != null)
        {
            myAvatarSprite = spriteTransform.GetComponent<SpriteRenderer>();
            myAnim = spriteTransform.GetComponent<Animator>();
        }

        // Nếu Animator không nằm ở Sprite mà nằm ở cha, dùng dòng này:
        if (myAnim == null) myAnim = GetComponent<Animator>();

        // Cập nhật màu sắc nhân vật
        if (myAvatarSprite != null)
        {
            myAvatarSprite.color = myColor;
        }
    }

    void Update()
    {
        if (!hasControl)
        {
            movementInput = Vector2.zero;
            if (myAnim != null) myAnim.SetFloat("Speed", 0);
            return;
        }

        movementInput = WASD.ReadValue<Vector2>();

        // Xử lý lật mặt
        if (movementInput.x != 0 && spriteTransform != null)
        {
            float direction = Mathf.Sign(movementInput.x);
            spriteTransform.localScale = new Vector3(direction, 1, 1);
        }

        // Cập nhật tham số Speed cho Animator
        if (myAnim != null)
        {
            myAnim.SetFloat("Speed", movementInput.magnitude);
        }
    }

    private void FixedUpdate()
    {
        myRB.linearVelocity = movementInput * movementSpeed;
    }

    public void SetColor(Color newColor)
    {
        myColor = newColor;
        if (myAvatarSprite != null)
        {
            myAvatarSprite.color = myColor;
        }
    }
}