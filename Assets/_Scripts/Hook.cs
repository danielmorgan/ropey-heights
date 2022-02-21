using UnityEngine;

public class Hook : MonoBehaviour
{
    [SerializeField]
    private Transform _ropeOrigin;
    public Vector2 ropeOrigin {
        get => _ropeOrigin.transform.position;
    }
}
