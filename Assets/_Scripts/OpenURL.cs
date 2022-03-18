using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenURL : MonoBehaviour
{
    public static void Open(string url)
    {
        Application.OpenURL(url);
    }
}
