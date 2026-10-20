using System.Collections;
using UnityEngine;

public class TimingMinigame : MinigameBase
{
    public RectTransform target;
    public RectTransform approachRing;

    public float startApproachTime = 1.2f;
    public float endApproachTime = 0.6f;
    public float perfectWindow = 0.07f;
    public float goodWindow = 0.16f;
    public float ignoreBefore = 0.35f;
    public float ringStartScale = 3f;
    public Vector2 targetArea = new Vector2(240, 70);

    protected override void OnSetup()
    {
        SetBarbell(1f);
        ShowNote(false);
    }

    protected override void OnCleanup() => ShowNote(false);

    protected override IEnumerator PlayRep()
    {
        float approachTime = Mathf.Lerp(startApproachTime, endApproachTime, Progress);
        target.anchoredPosition = new Vector2(Random.Range(-targetArea.x, targetArea.x), Random.Range(-targetArea.y, targetArea.y));
        approachRing.anchoredPosition = target.anchoredPosition;
        ShowNote(true);
        feedbackText.text = "";

        float elapsed = 0f;
        while (true)
        {
            if (CancelPressed())
                yield break;

            elapsed += Time.deltaTime;
            float remaining = approachTime - elapsed;
            float t = Mathf.Clamp01(remaining / approachTime);
            approachRing.localScale = Vector3.one * Mathf.Lerp(1f, ringStartScale, t);
            SetBarbell(t);

            if (hitAction.WasPressedThisFrame() && remaining <= ignoreBefore)
            {
                Judge(Mathf.Abs(remaining));
                break;
            }

            if (remaining < -goodWindow)
            {
                Miss("ATRASOU!");
                break;
            }

            yield return null;
        }

        ShowNote(false);
    }

    void Judge(float error)
    {
        if (error <= perfectWindow)
            Hit("PERFECT!", 300, PerfectColor);
        else if (error <= goodWindow)
            Hit("GOOD", 100, GoodColor);
        else
            Miss("CEDO DEMAIS!");

        if (error <= goodWindow)
            SetBarbell(1f);
    }

    void ShowNote(bool visible)
    {
        target.gameObject.SetActive(visible);
        approachRing.gameObject.SetActive(visible);
    }
}
