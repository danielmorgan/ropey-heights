using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
public class PulseText : MonoBehaviour
{
    private TMP_Text text;
    [SerializeField]
    private float frequency;
    [SerializeField]
    private float amplitude;
    [SerializeField]
    private float minScale;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    void Update()
    {
        float scale = Mathf.Sin(Time.time * frequency) * amplitude;
        scale = Mathf.Abs(scale);
        scale += minScale;
        text.rectTransform.localScale = Vector3.one * scale;
    }
}
