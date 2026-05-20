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
    private WeaponManager weaponManager;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        weaponManager = GetComponent<WeaponManager>();

        if (animator == null)
            animator = GetComponent<Animator>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleWeaponSwitching();
        HandleShooting();
    }

    private void HandleMovement()
    {
        // Get input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Handle jumping
        if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded)
        {
            velocity.y = jumpForce;
        }

        // Move character
        Vector3 move = transform.forward * z + transform.right * x;
        cc.Move((move * moveSpeed + velocity) * Time.deltaTime);

        // Update animation
        isMoving = x != 0 || z != 0;
        if (animator != null)
        {
            animator.SetBool("isWalking", isMoving);
        }
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, -maxLookAngle, maxLookAngle);

        if (cam != null)
            cam.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleWeaponSwitching()
    {
        // Press 1 to switch to first weapon
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (weaponManager != null)
                weaponManager.EquipWeapon(0);
        }

        // Press 2 to switch to second weapon
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            if (weaponManager != null)
                weaponManager.EquipWeapon(1);
        }
    }

    private void HandleShooting()
    {
        if (weaponManager != null && Input.GetMouseButton(0))
        {
            weaponManager.Shoot();
        }
    }

    /// <summary>
    /// Returns true if the player is currently moving
    /// </summary>
    public bool GetIsMoving()
    {
        return isMoving;
    }
}