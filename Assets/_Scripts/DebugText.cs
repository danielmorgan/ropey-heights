using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class DebugText : Singleton<DebugText>
{
    private TMP_Text text;

    protected override void Awake()
    {
        base.Awake();

        text = GetComponent<TMP_Text>();
    }

    public void Set(string _text, Color? _color = null)
    {
        text.text = _text;
        text.color = _color ?? Color.white;
    }
}
