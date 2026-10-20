using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GymMachine : MonoBehaviour
{
    public string exerciseName = "Supino";
    public MinigameBase minigame;
    public Text promptText;
    public Color completedColor = new Color(0.55f, 1f, 0.55f);

    public bool Completed { get; private set; }

    bool playerInRange;
    InputAction interactAction;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        interactAction = new InputAction("Interact", InputActionType.Button);
        interactAction.AddBinding("<Keyboard>/e");
        interactAction.AddBinding("<Keyboard>/enter");
        interactAction.AddBinding("<Gamepad>/buttonSouth");
    }

    void OnEnable() => interactAction.Enable();
    void OnDisable() => interactAction.Disable();

    void Update()
    {
        if (!playerInRange || Completed || !interactAction.WasPressedThisFrame())
            return;

        if (!GameManager.Instance.TryStartSet(minigame))
            return;

        promptText.gameObject.SetActive(false);
        minigame.Finished += OnMinigameFinished;
        minigame.Begin();
    }

    void OnMinigameFinished(bool success)
    {
        minigame.Finished -= OnMinigameFinished;

        if (success)
        {
            Completed = true;
            spriteRenderer.color = completedColor;
        }

        GameManager.Instance.EndSet(this, success, minigame.Score);
        ShowPrompt();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = true;
        ShowPrompt();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInRange = false;
        promptText.gameObject.SetActive(false);
    }

    void ShowPrompt()
    {
        if (!playerInRange)
            return;

        promptText.text = Completed ? $"{exerciseName}: concluído!" : $"[E / A] {exerciseName}";
        promptText.gameObject.SetActive(true);
    }
}
