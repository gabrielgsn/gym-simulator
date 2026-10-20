using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float knockbackDecay = 25f;

    Rigidbody2D rb;
    AudioSource audioSource;
    InputAction moveAction;
    Vector2 movement;
    Vector2 knockback;
    float stunTimer;

    bool CanMove => stunTimer <= 0f && (GameManager.Instance == null || GameManager.Instance.State == GameState.Playing);

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/w").With("Down", "<Keyboard>/s")
            .With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
        moveAction.AddCompositeBinding("2DVector")
            .With("Up", "<Keyboard>/upArrow").With("Down", "<Keyboard>/downArrow")
            .With("Left", "<Keyboard>/leftArrow").With("Right", "<Keyboard>/rightArrow");
        moveAction.AddBinding("<Gamepad>/leftStick");
        moveAction.AddBinding("<Gamepad>/dpad");
    }

    void OnEnable() => moveAction.Enable();
    void OnDisable() => moveAction.Disable();

    void Update()
    {
        movement = CanMove ? moveAction.ReadValue<Vector2>() : Vector2.zero;
        movement.Normalize();
    }

    void FixedUpdate()
    {
        stunTimer -= Time.fixedDeltaTime;
        rb.MovePosition(rb.position + (movement * speed + knockback) * Time.fixedDeltaTime);
        knockback = Vector2.MoveTowards(knockback, Vector2.zero, knockbackDecay * Time.fixedDeltaTime);
    }

    public void Knock(Vector2 force, float stun)
    {
        knockback = force;
        stunTimer = stun;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Coletavel"))
        {
            audioSource.Play();
            Destroy(other.gameObject);
            if (GameManager.Instance != null)
                GameManager.Instance.CollectShake();
        }
    }
}
