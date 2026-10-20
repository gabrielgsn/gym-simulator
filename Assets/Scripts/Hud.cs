using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class Hud : MonoBehaviour
{
    public static readonly Color GoodColor = new Color(0.45f, 1f, 0.45f);
    public static readonly Color BadColor = new Color(1f, 0.4f, 0.4f);

    public Text scoreText;
    public Text timerText;
    public Text staminaText;
    public Text checklistText;
    public Text toastText;
    public RectTransform staminaFill;
    public Image staminaFillImage;
    public float toastDuration = 2f;

    float toastTimer;

    void Update()
    {
        var gm = GameManager.Instance;
        scoreText.text = $"SCORE {gm.Score}";
        timerText.text = GameManager.FormatTime(gm.TimeLeft);
        timerText.color = gm.TimeLeft <= 30f && Mathf.Repeat(Time.unscaledTime, 1f) < 0.5f ? BadColor : Color.white;

        float ratio = gm.Stamina / gm.maxStamina;
        staminaFill.anchorMax = new Vector2(ratio, 1f);
        staminaFillImage.color = gm.Stamina < gm.setCost ? BadColor : new Color(1f, 0.8f, 0.2f);
        staminaText.text = $"ENERGIA {gm.Stamina:0}";

        if (toastTimer > 0f)
        {
            toastTimer -= Time.unscaledDeltaTime;
            var c = toastText.color;
            c.a = Mathf.Clamp01(toastTimer / 0.4f);
            toastText.color = c;
        }
    }

    public void Toast(string message, Color color)
    {
        toastText.text = message;
        toastText.color = color;
        toastTimer = toastDuration;
    }

    public void RefreshChecklist(IList<GymMachine> machines)
    {
        var sb = new StringBuilder("FICHA DE TREINO\n");
        foreach (var m in machines)
            sb.Append(m.Completed ? $"<color=#77ff77>[x] {m.exerciseName}</color>\n" : $"[  ] {m.exerciseName}\n");
        checklistText.text = sb.ToString();
    }
}
