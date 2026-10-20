using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RhythmMinigame : MinigameBase
{
    public RectTransform leftTarget;
    public RectTransform rightTarget;
    public RectTransform approachRing;
    public RectTransform leftWeight;
    public RectTransform rightWeight;
    public Color idleColor = new Color(0.3f, 0.3f, 0.36f);
    public Color activeColor = new Color(1f, 0.6f, 0.2f);

    public float startBeat = 0.8f;
    public float endBeat = 0.5f;
    public float perfectWindow = 0.07f;
    public float goodWindow = 0.15f;
    public float ignoreBefore = 0.3f;
    public float ringStartScale = 2.6f;
    public float weightLift = 60f;

    InputAction leftAction;
    InputAction rightAction;
    Vector2 leftWeightHome;
    Vector2 rightWeightHome;

    protected override float RepPause => 0f;

    protected override void Awake()
    {
        base.Awake();
        leftAction = new InputAction("Left", InputActionType.Button);
        leftAction.AddBinding("<Keyboard>/a");
        leftAction.AddBinding("<Keyboard>/leftArrow");
        leftAction.AddBinding("<Gamepad>/leftShoulder");
        leftAction.AddBinding("<Gamepad>/dpad/left");
        leftAction.AddBinding("<Gamepad>/leftTrigger");

        rightAction = new InputAction("Right", InputActionType.Button);
        rightAction.AddBinding("<Keyboard>/d");
        rightAction.AddBinding("<Keyboard>/rightArrow");
        rightAction.AddBinding("<Gamepad>/rightShoulder");
        rightAction.AddBinding("<Gamepad>/dpad/right");
        rightAction.AddBinding("<Gamepad>/rightTrigger");

        leftWeightHome = leftWeight.anchoredPosition;
        rightWeightHome = rightWeight.anchoredPosition;
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        leftAction.Enable();
        rightAction.Enable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        leftAction.Disable();
        rightAction.Disable();
    }

    protected override void OnSetup()
    {
        approachRing.gameObject.SetActive(false);
        Highlight(null);
        leftWeight.anchoredPosition = leftWeightHome;
        rightWeight.anchoredPosition = rightWeightHome;
    }

    protected override void OnCleanup()
    {
        approachRing.gameObject.SetActive(false);
        Highlight(null);
    }

    protected override IEnumerator PlayRep()
    {
        bool left = (reps + misses) % 2 == 0;
        var target = left ? leftTarget : rightTarget;
        var weight = left ? leftWeight : rightWeight;
        var home = left ? leftWeightHome : rightWeightHome;
        float beat = Mathf.Lerp(startBeat, endBeat, Progress);

        Highlight(target);
        approachRing.anchoredPosition = target.anchoredPosition;
        approachRing.gameObject.SetActive(true);

        float elapsed = 0f;
        while (true)
        {
            if (CancelPressed())
                yield break;

            elapsed += Time.deltaTime;
            float remaining = beat - elapsed;
            approachRing.localScale = Vector3.one * Mathf.Lerp(1f, ringStartScale, Mathf.Clamp01(remaining / beat));

            bool correct = left ? leftAction.WasPressedThisFrame() : rightAction.WasPressedThisFrame();
            bool wrong = left ? rightAction.WasPressedThisFrame() : leftAction.WasPressedThisFrame();

            if (remaining <= ignoreBefore && (correct || wrong))
            {
                float error = Mathf.Abs(remaining);
                bool cheated = leftAction.IsPressed() && rightAction.IsPressed();
                if (cheated)
                {
                    Miss("ROUBOU!");
                    break;
                }
                if (wrong)
                    Miss("LADO ERRADO!");
                else if (error <= perfectWindow)
                    Hit("PERFECT!", 300, PerfectColor);
                else if (error <= goodWindow)
                    Hit("GOOD", 100, GoodColor);
                else
                    Miss("CEDO DEMAIS!");

                if (correct && error <= goodWindow)
                    weight.anchoredPosition = home + Vector2.up * weightLift;
                break;
            }

            if (remaining < -goodWindow)
            {
                Miss("ATRASOU!");
                break;
            }

            yield return null;
        }

        approachRing.gameObject.SetActive(false);
        Highlight(null);

        for (float t = 0f; t < 0.2f; t += Time.deltaTime)
        {
            weight.anchoredPosition = Vector2.Lerp(weight.anchoredPosition, home, t / 0.2f);
            yield return null;
        }
        weight.anchoredPosition = home;
    }

    void Highlight(RectTransform target)
    {
        leftTarget.GetComponent<Image>().color = target == leftTarget ? activeColor : idleColor;
        rightTarget.GetComponent<Image>().color = target == rightTarget ? activeColor : idleColor;
    }
}
