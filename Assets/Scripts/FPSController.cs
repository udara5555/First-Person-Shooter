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
    public float adsZoom = 40f; // Zoomed FOV
    public float normalZoom = 60f; // Normal FOV
    public float adsSpeed = 10f; // How fast to zoom
    public Vector3 adsPosition = new Vector3(0.3f, -0.2f, 0.5f); // Gun position when ADS
    public Vector3 normalPosition = Vector3.zero; // Gun position when not ADS

    private CharacterController cc;
    private Camera cam;
    private float verticalRotation = 0f;
    private Vector3 velocity;
    private bool isMoving = false;
    private bool isAiming = false;
    private float targetFOV;
    private float currentFOV;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (gun == null)
            gun = GetComponentInChildren<Gun>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set initial FOV
        currentFOV = normalZoom;
        targetFOV = normalZoom;
        if (cam != null)
            cam.fieldOfView = currentFOV;
    }

    void Update()
    {
        MouseLook();
        Move();
        HandleADS();
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

    void HandleADS()
    {
        isAiming = Input.GetMouseButton(1); // Right mouse button

        if (isAiming)
        {
            targetFOV = adsZoom;
        }
        else
        {
            targetFOV = normalZoom;
        }

        // Smoothly transition FOV
        currentFOV = Mathf.Lerp(currentFOV, targetFOV, Time.deltaTime * adsSpeed);
        if (cam != null)
            cam.fieldOfView = currentFOV;

        // Smoothly move gun position
        if (gun != null)
        {
            Vector3 targetPosition = isAiming ? adsPosition : normalPosition;
            gun.transform.localPosition = Vector3.Lerp(gun.transform.localPosition, targetPosition, Time.deltaTime * adsSpeed);
        }

        // Update crosshair visibility
        var crosshair = gun?.GetComponent<Crosshair>();
        if (crosshair == null)
            crosshair = Object.FindAnyObjectByType<Crosshair>();

        if (crosshair != null)
        {
            crosshair.SetAiming(isAiming);
        }
    }

    public bool GetIsMoving()
    {
        return isMoving;
    }

    public bool IsAiming()
    {
        return isAiming;
    }
}