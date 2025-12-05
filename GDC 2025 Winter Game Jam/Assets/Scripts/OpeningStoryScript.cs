using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimedTypewriter : MonoBehaviour
{
    [Header("References")]
    public TMP_Text textDisplay;
    public string fullText;
    public float totalDuration = 20f;
    public Button proceedButton;
    public TMP_Text timerDisplay;

    private float timer;
    private bool finished = false;

    void Start()
    {
        if (proceedButton != null)
            proceedButton.gameObject.SetActive(false);

        textDisplay.text = "";
        timer = totalDuration;
    }

    void Update()
    {
        if (finished)
            return;

        // Update timer
        timer -= Time.unscaledDeltaTime;   // Unscaled time avoids WebGL timeScale throttling
        if (timer < 0)
            timer = 0.00f;

        timerDisplay.text = timer.ToString("F2");

        // Compute typewriter progress
        float progress = 1f - (timer / totalDuration);
        int charsToShow = Mathf.FloorToInt(fullText.Length * progress);

        charsToShow = Mathf.Clamp(charsToShow, 0, fullText.Length);
        textDisplay.text = fullText.Substring(0, charsToShow);

        if (timer <= 0f)
            Finish();
    }

    void Finish()
    {
        if (finished)
            return;

        finished = true;

        textDisplay.text = fullText;

        if (proceedButton != null)
            proceedButton.gameObject.SetActive(true);
    }
}
