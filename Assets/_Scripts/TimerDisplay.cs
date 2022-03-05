using UnityEngine;
using System;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class TimerDisplay : MonoBehaviour
{
    private TMP_Text timerText;

    private void Awake()
    {
        timerText = GetComponent<TMP_Text>();
    }

    private void Update()
    {
        TimeSpan ts = TimeSpan.FromSeconds(GameManager.Instance.time);
        timerText.text = ts.ToString("mm\\:ss") + "<alpha=#CC><size=70%>" + ts.ToString("\\.f") + "</size>";
    }
}
