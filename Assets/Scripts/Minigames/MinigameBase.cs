using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public abstract class MinigameBase : MonoBehaviour
{
    public event Action<bool> Finished;

    public Text repsText;
    public Text feedbackText;
    public RectTransform barbell;
    public float barbellLow = -235f;
    public float barbellHigh = -110f;
    public AudioClip hitClip;
    public AudioClip missClip;
    public AudioClip completeClip;
    public int repsRequired = 5;
    public int maxMisses = 3;

    public int Score { get; protected set; }

    protected static readonly Color PerfectColor = new Color(0.35f, 0.85f, 1f);
    protected static readonly Color GoodColor = new Color(0.5f, 1f, 0.5f);
    protected static readonly Color BadColor = new Color(1f, 0.35f, 0.35f);

    protected int reps;
    protected int misses;
    protected InputAction hitAction;
    protected InputAction cancelAction;
    protected AudioSource audioSource;

    protected float Progress => repsRequired > 1 ? reps / (float)(repsRequired - 1) : 0f;
    protected virtual float RepPause => 0.35f;

    protected virtual void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        hitAction = new InputAction("Hit", InputActionType.Button);
        hitAction.AddBinding("<Keyboard>/space");
        hitAction.AddBinding("<Keyboard>/z");
        hitAction.AddBinding("<Keyboard>/x");
        hitAction.AddBinding("<Mouse>/leftButton");
        hitAction.AddBinding("<Gamepad>/buttonSouth");
        hitAction.AddBinding("<Gamepad>/rightShoulder");

        cancelAction = new InputAction("Cancel", InputActionType.Button);
        cancelAction.AddBinding("<Keyboard>/escape");
        cancelAction.AddBinding("<Gamepad>/buttonEast");
    }

    protected virtual void OnEnable()
    {
        hitAction.Enable();
        cancelAction.Enable();
    }

    protected virtual void OnDisable()
    {
        hitAction.Disable();
        cancelAction.Disable();
        StopAllCoroutines();
    }

    public void Begin()
    {
        Score = 0;
        reps = 0;
        misses = 0;
        gameObject.SetActive(true);
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        UpdateReps();
        OnSetup();
        ShowFeedback("Prepare-se!", Color.white);
        yield return new WaitForSeconds(1f);

        while (reps < repsRequired && misses < maxMisses)
        {
            yield return PlayRep();
            if (RepPause > 0f)
                yield return new WaitForSeconds(RepPause);
        }

        OnCleanup();

        bool success = reps >= repsRequired;
        if (success)
        {
            ShowFeedback("SÉRIE COMPLETA!", GoodColor);
            audioSource.PlayOneShot(completeClip);
        }
        else
        {
            ShowFeedback("FALHOU!", BadColor);
        }

        yield return new WaitForSeconds(1.2f);
        Finish(success);
    }

    protected abstract IEnumerator PlayRep();
    protected virtual void OnSetup() { }
    protected virtual void OnCleanup() { }

    protected bool CancelPressed()
    {
        if (!cancelAction.WasPressedThisFrame())
            return false;

        Finish(false);
        return true;
    }

    protected void Hit(string label, int points, Color color)
    {
        reps++;
        Score += points;
        audioSource.PlayOneShot(hitClip);
        ShowFeedback(label, color);
        UpdateReps();
    }

    protected void Miss(string label)
    {
        misses++;
        audioSource.PlayOneShot(missClip);
        ShowFeedback(label, BadColor);
        UpdateReps();
    }

    public void Abort() => gameObject.SetActive(false);

    protected void SetBarbell(float t)
    {
        if (barbell == null)
            return;
        barbell.anchoredPosition = new Vector2(barbell.anchoredPosition.x, Mathf.Lerp(barbellLow, barbellHigh, t));
    }

    protected static void SetFill(RectTransform fill, float value, bool vertical)
    {
        value = Mathf.Clamp01(value);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = vertical ? new Vector2(1f, value) : new Vector2(value, 1f);
        fill.offsetMin = Vector2.zero;
        fill.offsetMax = Vector2.zero;
    }

    protected void ShowFeedback(string text, Color color)
    {
        feedbackText.text = text;
        feedbackText.color = color;
    }

    void UpdateReps()
    {
        repsText.text = $"Reps {reps}/{repsRequired}    Erros {misses}/{maxMisses}    Score {Score}";
    }

    void Finish(bool success)
    {
        gameObject.SetActive(false);
        Finished?.Invoke(success);
    }
}
