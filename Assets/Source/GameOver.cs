using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/**************************************/
/*GameOver gameover ゲームオーバー    */
/*ゲームオーバー時の処理を行うクラス  */
/**************************************/


//ゲームオーバー表示
public class GameOver : MonoBehaviour
{
    public GameObject gobjImage;
    public GameObject Buttons;
    public Fade fade;
    public Fade fadeout;
    bool fadeflag = false;
    // Start is called before the first frame update
    void Start()
    {
        fadeflag = false;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(!fadeflag)
        {
            fade.FadeIn(3, () =>
            {
                //3秒後にボタン表示
                Invoke("Show_Button", 3);
            });
            fadeflag = true;
        }
        else
        {
            gobjImage.SetActive(true);
        }
    }

    //ボタン表示
    void Show_Button()
    {
        Buttons.SetActive(true);
    }

    //ボタン処理
    //GameVal.nextscenenameのシーンへと遷移
    public void SceneChange()
    {
        fadeout.FadeIn(1, () =>
        {
            SceneManager.LoadScene("BlackScene", LoadSceneMode.Single);
            Resources.UnloadUnusedAssets();
        });
        Buttons.SetActive(false);
    }
    //ボタン処理
    //GameVal.nextscenenameのシーンへと遷移
    public void BacktoTitle()
    {
        fadeout.FadeIn(1, () =>
        {
            SceneManager.LoadScene("Title", LoadSceneMode.Single);
            Resources.UnloadUnusedAssets();
        });        
        Buttons.SetActive(false);
    }
}
