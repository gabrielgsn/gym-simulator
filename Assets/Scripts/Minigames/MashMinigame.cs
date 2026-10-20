using System.Collections;
using UnityEngine;

public class MashMinigame : MinigameBase
{
    public RectTransform heightFill;
    public RectTransform stickZone;
    public RectTransform strainFill;
    public RectTransform gripFill;
    public GameObject strainRoot;
    public GameObject gripRoot;

    public float pushPerPress = 0.075f;
    public float gravity = 0.35f;
    public float stickStart = 0.45f;
    public float stickEnd = 0.62f;
    public float stickMultiplier = 0.65f;
    public float repTimeLimit = 5f;
    public float perfectTime = 2.4f;

    public bool useGrip;
    public float gripTime = 0.6f;
    public bool useStrain;
    public float strainPerPress = 0.25f;
    public float strainCooldown = 1.2f;

    protected override void OnSetup()
    {
        strainRoot.SetActive(useStrain);
        gripRoot.SetActive(useGrip);
        SetHeight(0f);
        SetFill(strainFill, 0f, false);
        SetFill(gripFill, 0f, false);

        var stick = stickZone.parent as RectTransform;
        float h = stick.rect.height;
        stickZone.anchoredPosition = new Vector2(0f, (stickStart + stickEnd) / 2f * h - h / 2f);
        stickZone.sizeDelta = new Vector2(stickZone.sizeDelta.x, (stickEnd - stickStart) * h);
    }

    protected override IEnumerator PlayRep()
    {
        float height = 0f;
        float strain = 0f;
        SetHeight(0f);
        SetFill(strainFill, 0f, false);

        if (useGrip)
        {
            ShowFeedback("SEGURE!", Color.white);
            float grip = 0f;
            while (grip < gripTime)
            {
                if (CancelPressed())
                    yield break;
                grip = hitAction.IsPressed() ? grip + Time.deltaTime : 0f;
                SetFill(gripFill, grip / gripTime, false);
                yield return null;
            }
            ShowFeedback("PUXA! APERTE RÁPIDO!", Accent);
        }
        else
        {
            ShowFeedback("APERTE RÁPIDO!", Accent);
        }

        float elapsed = 0f;
        float currentGravity = gravity * (1f + 0.25f * Progress);

        while (true)
        {
            if (CancelPressed())
                yield break;

            elapsed += Time.deltaTime;
            bool inStick = height >= stickStart && height <= stickEnd;

            if (hitAction.WasPressedThisFrame())
            {
                height += pushPerPress * (inStick ? stickMultiplier : 1f);
                strain += strainPerPress;
            }

            if (height >= 1f)
            {
                SetHeight(1f);
                if (elapsed <= perfectTime)
                    Hit("PERFECT!", 300, PerfectColor);
                else
                    Hit("GOOD", 100, GoodColor);
                height = 1f;
                yield return new WaitForSeconds(0.25f);
                break;
            }

            height = Mathf.Max(0f, height - currentGravity * Time.deltaTime);
            strain = Mathf.Max(0f, strain - strainCooldown * Time.deltaTime);
            SetHeight(height);
            SetFill(strainFill, strain, false);

            if (useStrain && strain >= 1f)
            {
                Miss("LOMBAR CURVOU!");
                break;
            }

            if (elapsed >= repTimeLimit)
            {
                Miss("NÃO SUBIU!");
                break;
            }

            yield return null;
        }

        for (float t = 0f; t < 0.3f; t += Time.deltaTime)
        {
            SetHeight(Mathf.Lerp(height, 0f, t / 0.3f));
            yield return null;
        }
        SetHeight(0f);
        SetFill(gripFill, 0f, false);
    }

    void SetHeight(float h)
    {
        SetFill(heightFill, h, true);
        SetBarbell(h);
    }

    static readonly Color Accent = new Color(1f, 0.8f, 0.2f);
}
