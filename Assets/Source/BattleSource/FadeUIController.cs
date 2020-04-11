using UnityEngine;
using UnityEngine.UI;

/************************************************/
/*フェードイン、フェードアウトを制御するクラス  */
/************************************************/
public class FadeUIController : MonoBehaviour
{
    public GameObject gobjFadeUI;
    FadeUI fade;

    bool initflag = true; //このフラグがONの時、FadeOut,FadeInは初期化を実行する


    //終了時にtrueを返す
    public bool FadeOut(float timer)
    {
        if(initflag)
        {
            gobjFadeUI.SetActive(true);
            fade = gobjFadeUI.GetComponent<FadeUI>();
            fade.Range = 1;
            initflag = false;
            //Debug.Log(fade.Range);
            return false;
        }
        //Debug.Log(timer);
        //Debug.Log(fade.Range);
        fade.Range -= Time.deltaTime / timer;

        if(fade.Range <= 0)
        {
            initflag = true;
            return true;
        }

        return false;
    }

    //終了時にtrueを返す
    public bool FadeIn(float timer)
    {
        if (initflag)
        {
            gobjFadeUI.SetActive(true);
            fade = gobjFadeUI.GetComponent<FadeUI>();
            fade.Range = 0;
            initflag = false;
            return false;
        }

        fade.Range += Time.deltaTime / timer;

        if (fade.Range >= 1)
        {
            //gobjFadeUI.SetActive(false);
            initflag = true;
            return true;
        }

        return false;
    }
}