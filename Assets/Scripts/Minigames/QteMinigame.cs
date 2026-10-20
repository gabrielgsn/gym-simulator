using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class QteMinigame : MinigameBase
{
    public Image[] arrows;
    public RectTransform timerFill;
    public Color pendingColor = new Color(0.85f, 0.85f, 0.9f);
    public Color currentColor = new Color(1f, 0.8f, 0.2f);
    public Color doneColor = new Color(0.45f, 1f, 0.45f);

    public int startLength = 4;
    public int endLength = 6;
    public float startTime = 2.8f;
    public float endTime = 2f;
    public float perfectRatio = 0.45f;

    static readonly float[] Rotations = { 0f, 90f, 180f, 270f };

    InputAction[] directions;

    protected override void Awake()
    {
        base.Awake();
        directions = new[]
        {
            CreateDirection("Up", "w", "upArrow", "dpad/up"),
            CreateDirection("Left", "a", "leftArrow", "dpad/left"),
            CreateDirection("Down", "s", "downArrow", "dpad/down"),
            CreateDirection("Right", "d", "rightArrow", "dpad/right"),
        };
    }

    static InputAction CreateDirection(string name, string key, string arrow, string dpad)
    {
        var action = new InputAction(name, InputActionType.Button);
        action.AddBinding($"<Keyboard>/{key}");
        action.AddBinding($"<Keyboard>/{arrow}");
        action.AddBinding($"<Gamepad>/{dpad}");
        return action;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        foreach (var d in directions)
            d.Enable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        foreach (var d in directions)
            d.Disable();
    }

    protected override void OnSetup()
    {
        foreach (var a in arrows)
            a.gameObject.SetActive(false);
        SetFill(timerFill, 1f, false);
    }

    protected override IEnumerator PlayRep()
    {
        int length = Mathf.Min(arrows.Length, Mathf.RoundToInt(Mathf.Lerp(startLength, endLength, Progress)));
        float limit = Mathf.Lerp(startTime, endTime, Progress);
        var sequence = new int[length];

        for (int i = 0; i < arrows.Length; i++)
        {
            bool used = i < length;
            arrows[i].gameObject.SetActive(used);
            if (!used)
                continue;
            sequence[i] = Random.Range(0, 4);
            arrows[i].rectTransform.localEulerAngles = new Vector3(0f, 0f, Rotations[sequence[i]]);
            arrows[i].color = i == 0 ? currentColor : pendingColor;
        }

        ShowFeedback("Digite as setas!", Color.white);

        int index = 0;
        float elapsed = 0f;
        while (true)
        {
            if (CancelPressed())
                yield break;

            elapsed += Time.deltaTime;
            SetFill(timerFill, 1f - elapsed / limit, false);

            int pressed = -1;
            for (int d = 0; d < directions.Length; d++)
                if (directions[d].WasPressedThisFrame())
                    pressed = d;

            if (pressed >= 0)
            {
                if (pressed != sequence[index])
                {
                    Miss("ERROU A SEQUÊNCIA!");
                    break;
                }

                arrows[index].color = doneColor;
                index++;
                if (index >= length)
                {
                    if (1f - elapsed / limit >= perfectRatio)
                        Hit("PERFECT!", 300, PerfectColor);
                    else
                        Hit("GOOD", 100, GoodColor);
                    break;
                }
                arrows[index].color = currentColor;
            }

            if (elapsed >= limit)
            {
                Miss("A PERNA VOLTOU!");
                break;
            }

            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        foreach (var a in arrows)
            a.gameObject.SetActive(false);
    }
}
