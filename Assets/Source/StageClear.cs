using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
/**************************************/
/*GameClear gameclear ゲームクリア    */
/*ゲームクリア時の処理を行うクラス    */
/**************************************/
//ステージクリア表示
public class StageClear: MonoBehaviour
{
    public GameObject gobjSceneGraph;
    public Fade fade;
    public Fade fadeout;

    bool fadestartflag = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(fadestartflag)
        {
            gobjSceneGraph.SetActive(true);
        }
        else
        {
            fade.FadeIn(1);
            fadestartflag = true;
        }
    }


    //ボタン処理
    //GameVal.nextscenenameのシーンへと遷移
    public void SceneChange()
    {
        fadeout.FadeIn(1, ()=> 
        {
            SceneManager.LoadScene("BlackScene", LoadSceneMode.Single);
            Resources.UnloadUnusedAssets();
        });
        
    }
}
