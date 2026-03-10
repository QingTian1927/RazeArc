using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Transform cameraTransform;
    public Transform graphicsTransform;
    public CharacterController bodyController;

    [Header("Settings")]
    public float cameraOffsetY = -0.35f;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float gravity = -20f;
    Vector3 velocity;
    Vector3 explosionVelocity;

    [Header("Jumping")]
    public float jumpForce = 8f;
    public int maxJumps = 2;
    int jumpCount = 0;

    [Header("Crouching")]
    public float standingHeight = 2f;
    public float crouchingHeight = 1f;
    public float crouchSpeed = 3f;


    // Start is called before the first frame update
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        bool isGrounded = bodyController.isGrounded;

        if (isGrounded)
        {
            if (velocity.y < 0)
                velocity.y = -2f;

            jumpCount = 0;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        float currentSpeed = moveSpeed;

        if (Input.GetKey(KeyCode.LeftControl))
        {
            bodyController.height = crouchingHeight;
            bodyController.center = new Vector3(0, crouchingHeight / 2f, 0);

            cameraTransform.localPosition = new Vector3(0, crouchingHeight + cameraOffsetY, 0);
            graphicsTransform.localPosition = new Vector3(0, crouchingHeight / 2f, 0);

            currentSpeed = crouchSpeed;
        }
        else
        {
            bodyController.height = standingHeight;
            bodyController.center = new Vector3(0, standingHeight / 2f, 0);

            cameraTransform.localPosition = new Vector3(0, standingHeight + cameraOffsetY, 0);
            graphicsTransform.localPosition = new Vector3(0, standingHeight / 2f, 0);
        }

        // Jump
        if (Input.GetButtonDown("Jump") && jumpCount < maxJumps)
        {
            velocity.y = jumpForce;
            jumpCount++;
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;

        // Combine movement
        Vector3 finalMove =
            move * currentSpeed +
            explosionVelocity +
            velocity;

        bodyController.Move(finalMove * Time.deltaTime);

        // Decay explosion pushback
        explosionVelocity = Vector3.Lerp(explosionVelocity, Vector3.zero, 5f * Time.deltaTime);
    }

    public void AddExplosionForce(Vector3 explosionPosition, float force, float radius)
    {
        // Use player's feet position instead of center
        Vector3 playerFeet = transform.position;

        Vector3 direction = playerFeet - explosionPosition;
        float distance = direction.magnitude;

        if (distance > radius)
            return;

        float falloff = 1f - (distance / radius);

        Vector3 push = direction.normalized * force * falloff;

        // Strong vertical boost
        push.y = Mathf.Max(push.y, force * 0.8f * falloff);

        explosionVelocity += push;
    }
}
