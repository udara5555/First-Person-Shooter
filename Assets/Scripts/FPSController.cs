using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("Animation")]
    public Animator animator;
    
    private CharacterController cc;
    private Camera cam;
    private float verticalRotation = 0f;
    private Vector3 velocity;
    private bool isMoving = false;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        
        if (animator == null)
            animator = GetComponent<Animator>();
        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        MouseLook();
        Move();
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        transform.Rotate(0, mouseX, 0);

        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);
        cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 move = transform.right * h + transform.forward * v;
        move = move.normalized * moveSpeed;

        // Check if player is moving (horizontal or vertical input)
        isMoving = (h != 0 || v != 0) && cc.isGrounded;

        if (cc.isGrounded) velocity.y = -2f;
        else velocity.y += gravity * Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded)
            velocity.y = jumpForce;

        cc.Move((move + velocity) * Time.deltaTime);
        
        // Trigger animation
        UpdateAnimation();
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetBool("isWalking", isMoving);
        }
    }

    public bool GetIsMoving()
    {
        return isMoving;
    }
}