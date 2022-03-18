using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RestartManager : MonoBehaviour
{
    public void HandleRestart(InputAction.CallbackContext context)
    {
        if (context.performed) {
            GameManager.Instance.Restart();
        }
    }
}
