using UnityEngine;
using System;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class FastestTimeDisplay : MonoBehaviour
{
    private TMP_Text timerText;

    private void Awake()
    {
        timerText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        if (GameManager.Instance.fastestTime > 0) {
            timerText.enabled = true;
            TimeSpan ts = TimeSpan.FromSeconds(GameManager.Instance.fastestTime);
            timerText.text = "Fastest: " + ts.ToString("mm\\:ss") + "<alpha=#CC><size=70%>" + ts.ToString("\\.ff") + "</size>";
        } else {
            timerText.enabled = false;
        }
    }
}
