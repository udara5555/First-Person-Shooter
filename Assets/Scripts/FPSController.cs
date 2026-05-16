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

    [Header("Gun")]
    public Gun gun;

    [Header("ADS (Aim Down Sight)")]
    public float adsZoom = 40f;
    public float normalZoom = 60f;
    public float adsSpeed = 10f;
    public Vector3 adsPosition = new Vector3(0.3f, -0.2f, 0.5f);
    public Vector3 normalPosition = Vector3.zero;

    private CharacterController cc;
    private Camera cam;
    private float verticalRotation = 0f;
    private Vector3 velocity;
    private bool isMoving = false;
    private bool isAiming = false;
    private float targetFOV;
    private float currentFOV;
    private WeaponManager weaponManager;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        weaponManager = GetComponent<WeaponManager>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (gun == null)
            gun = GetComponentInChildren<Gun>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set initial FOV
        if (cam != null)
        {
            targetFOV = normalZoom;
            currentFOV = targetFOV;
            cam.fieldOfView = currentFOV;
        }
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleADS();
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
            // Use the correct parameter name from your Animator
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

    private void HandleADS()
    {
        isAiming = Input.GetMouseButton(1); // Right mouse button

        if (cam != null)
        {
            targetFOV = isAiming ? adsZoom : normalZoom;
            currentFOV = Mathf.Lerp(currentFOV, targetFOV, adsSpeed * Time.deltaTime);
            cam.fieldOfView = currentFOV;
        }
    }

    private void HandleShooting()
    {
        if (gun != null && Input.GetMouseButton(0))
        {
            gun.Shoot();
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