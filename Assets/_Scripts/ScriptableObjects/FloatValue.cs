using UnityEngine;

[CreateAssetMenu(menuName = "Value Objects/Float")]
public class FloatValue : ScriptableObject
{
    [SerializeField]
    private float _value;
    
    public float value {
        get { return _value; }
    }
}
