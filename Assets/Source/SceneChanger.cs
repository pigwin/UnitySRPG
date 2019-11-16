using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneChanger : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Invoke("SceneChange", 1);
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    void SceneChange()
    {
        SceneManager.LoadScene(GameVal.nextscenename, LoadSceneMode.Single);
        Resources.UnloadUnusedAssets();
    }
}
