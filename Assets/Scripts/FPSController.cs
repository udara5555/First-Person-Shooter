using UnityEngine;

public class FPSController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float sprintSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    [Header("Sprint")]
    public float maxStamina = 100f;
    public float staminaDrainRate = 30f;
    public float staminaRegenRate = 20f;

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
    private bool isSprinting = false;
    private float currentStamina;
    private WeaponManager weaponManager;
    private bool isReloading = false;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        cam = GetComponentInChildren<Camera>();
        weaponManager = GetComponent<WeaponManager>();

        if (animator == null)
            animator = GetComponent<Animator>();

        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMovement();
        HandleMouseLook();
        HandleWeaponSwitching();
        HandleShooting();
        HandleReload();
        UpdateStamina();
    }

    private void HandleMovement()
    {
        // Get input
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Check sprint input - cannot sprint while reloading
        bool sprintInput = Input.GetKey(KeyCode.LeftShift);
        isSprinting = sprintInput && (x != 0 || z != 0) && cc.isGrounded && currentStamina > 0 && !isReloading;

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;

        // Handle jumping
        if (Input.GetKeyDown(KeyCode.Space) && cc.isGrounded)
        {
            velocity.y = jumpForce;
        }

        // Determine current speed
        float currentSpeed = isSprinting ? sprintSpeed : moveSpeed;

        // Move character
        Vector3 move = transform.forward * z + transform.right * x;
        cc.Move((move * currentSpeed + velocity) * Time.deltaTime);

        // Update animation
        isMoving = x != 0 || z != 0;
        if (animator != null)
        {
            // Keep walking animation when reloading while moving
            animator.SetBool("isWalking", isMoving && !isSprinting);
            animator.SetBool("isSprinting", isSprinting);
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
        // Cannot fire while sprinting or reloading
        if (weaponManager != null && Input.GetMouseButton(0) && !isReloading && !isSprinting)
        {
            weaponManager.Shoot();
        }
        
        // Stop sprinting when player tries to shoot (exit sprint to walk mode)
        if (Input.GetMouseButton(0) && isSprinting)
        {
            isSprinting = false;
        }
    }

    private void HandleReload()
    {
        // Press R to reload
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (weaponManager != null)
            {
                weaponManager.Reload();
                // Stop sprinting when reload starts
                isSprinting = false;
            }
        }
    }

    private void UpdateStamina()
    {
        if (isSprinting)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            currentStamina = Mathf.Max(0, currentStamina);
        }
        else
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            currentStamina = Mathf.Min(maxStamina, currentStamina);
        }
    }

    /// <summary>
    /// Updates the reloading state - called by WeaponManager
    /// </summary>
    public void SetIsReloading(bool reloading)
    {
        isReloading = reloading;
    }

    /// <summary>
    /// Returns true if the player is currently moving
    /// </summary>
    public bool GetIsMoving()
    {
        return isMoving;
    }

    /// <summary>
    /// Returns true if the player is currently sprinting
    /// </summary>
    public bool GetIsSprinting()
    {
        return isSprinting;
    }

    /// <summary>
    /// Returns the current stamina value
    /// </summary>
    public float GetStamina()
    {
        return currentStamina;
    }

    /// <summary>
    /// Returns the maximum stamina value
    /// </summary>
    public float GetMaxStamina()
    {
        return maxStamina;
    }
}