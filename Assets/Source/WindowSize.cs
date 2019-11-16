using UnityEngine;
using System.Collections;

public class WindowSize : MonoBehaviour
{

    [RuntimeInitializeOnLoadMethod]

    static void OnRuntimeMethodLoad()

    {

        Screen.SetResolution(1024, 768, false, 60);

    }

}