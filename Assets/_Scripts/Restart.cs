using UnityEngine;

public class Restart : MonoBehaviour
{
    public void Handle()
    {
        GameManager.Instance.Restart();
    }
}
