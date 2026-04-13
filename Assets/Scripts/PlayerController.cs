using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 8f;
    public float rotationSpeed = 15f;


    private Rigidbody rb;
    private Camera mainCamera;
    private PlayerKeys controls;
    private Vector2 moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        mainCamera = Camera.main;
        controls = new PlayerKeys();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void Update()
    {
        moveInput = controls.Player.Move.ReadValue<Vector2>();
        HandleRotation();
    }

    void FixedUpdate()
    {
        Vector3 moveDir = new Vector3(moveInput.x, 0, moveInput.y).normalized;
        rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
    }

    private void HandleRotation()
    {
        // DIRECT MOUSE READING (ignoring PlayerKeys settings for reliability)
        Vector2 mousePos = Mouse.current.position.ReadValue();

        Ray ray = mainCamera.ScreenPointToRay(mousePos);
        Plane groundPlane = new Plane(Vector3.up, transform.position);

        if (groundPlane.Raycast(ray, out float hitDist))
        {
            Vector3 targetPoint = ray.GetPoint(hitDist);
            Vector3 lookDir = targetPoint - transform.position;
            lookDir.y = 0;

            if (lookDir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDir);

                // Smooth rotation. For instant rotation, replace the line below with:
                // rb.MoveRotation(targetRotation);
                rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, rotationSpeed * Time.deltaTime));
            }
        }
    }
}