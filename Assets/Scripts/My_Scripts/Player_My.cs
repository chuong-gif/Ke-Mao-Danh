using UnityEngine;
using UnityEngine.InputSystem;

public class Player_My : MonoBehaviour
{
    // Thay đổi thành Rigidbody2D để khớp với Component bạn vừa thêm
    Rigidbody2D myRB;
    Animator myAnim;

    [Header("Settings")]
    [SerializeField] float movementSpeed = 10f;
    [SerializeField] InputAction WASD;

    [Header("References")]
    [SerializeField] Transform spriteTransform;


    Vector2 movementInput;

    private void OnEnable() => WASD.Enable();
    private void OnDisable() => WASD.Disable();

    void Start()
    {
        // Lấy đúng Rigidbody2D
        myRB = GetComponent<Rigidbody2D>();

        // Tự động tìm Sprite nếu chưa kéo vào Inspector
        if (spriteTransform == null)
        {
            spriteTransform = transform.Find("Sprite");
        }
        myAnim = GetComponent<Animator>();
    }

    void Update()
    {
        movementInput = WASD.ReadValue<Vector2>();

        // Xử lý lật mặt (Flip) dựa trên hướng di chuyển x
        if (movementInput.x != 0 && spriteTransform != null)
        {
            float direction = Mathf.Sign(movementInput.x);
            spriteTransform.localScale = new Vector3(direction, 1, 1);
        }

        myAnim.SetFloat("Speed", movementInput.magnitude);
    }

    private void FixedUpdate()
    {
        // Trong Rigidbody2D, chúng ta dùng velocity (hoặc linearVelocity trong Unity mới)
        // Vì là game 2D top-down, di chuyển sẽ dùng cả x và y của movementInput
        myRB.linearVelocity = movementInput * movementSpeed;
    }
}