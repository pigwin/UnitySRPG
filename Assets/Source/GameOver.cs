using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
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
    public GameObject gobjBackStageSelect;
    public Fade fade;
    public Fade fadeout;
    bool fadeflag = false;
    int buttoncount = 0;
    // Start is called before the first frame update
    void Start()
    {
        gobjBackStageSelect.SetActive(GameVal.masterSave.is_story_clear);
        fadeflag = false;
        buttoncount = 0;
    }
    //キー入力待ち時間
    [SerializeField] const float interval_keypad_time = 0.30f;
    float delta_keypad_time = interval_keypad_time;
    // Update is called once per frame
    void Update()
    {
        PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
        eventDataCurrent.position = Input.mousePosition;
        List<RaycastResult> raycast = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrent, raycast);
        if(Input.GetAxisRaw("Vertical") != 0)
        {
            if (delta_keypad_time < interval_keypad_time) delta_keypad_time += Time.deltaTime;
            else
            {
                delta_keypad_time = 0;
                if (Input.GetAxisRaw("Vertical") > 0)
                {
                    if (GameVal.masterSave.is_story_clear) buttoncount = (buttoncount + 2) % 3;
                    else buttoncount = (buttoncount + 1) % 2;
                    EventSystem.current.SetSelectedGameObject(Buttons.transform.GetChild(buttoncount).gameObject);
                }
                else if (Input.GetAxisRaw("Vertical") < 0)
                {
                    if (GameVal.masterSave.is_story_clear) buttoncount = (buttoncount + 1) % 3;
                    else buttoncount = (buttoncount + 1) % 2;
                    EventSystem.current.SetSelectedGameObject(Buttons.transform.GetChild(buttoncount).gameObject);
                }
            }
            
        }
        else
        {
            delta_keypad_time += Time.deltaTime;
            foreach (RaycastResult tmp_result in raycast)
            {
                if (tmp_result.gameObject.name == Buttons.transform.GetChild(0).name)
                {
                    Buttons.transform.GetChild(1).gameObject.SetActive(false);
                    Buttons.transform.GetChild(1).gameObject.SetActive(true);
                    if(GameVal.masterSave.is_story_clear)
                    {
                        Buttons.transform.GetChild(2).gameObject.SetActive(false);
                        Buttons.transform.GetChild(2).gameObject.SetActive(true);
                    }
                    buttoncount = 0;
                    EventSystem.current.SetSelectedGameObject(Buttons.transform.GetChild(0).gameObject);
                }
                else if(tmp_result.gameObject.name == Buttons.transform.GetChild(1).name)
                {
                    Buttons.transform.GetChild(0).gameObject.SetActive(false);
                    Buttons.transform.GetChild(0).gameObject.SetActive(true);
                    if (GameVal.masterSave.is_story_clear)
                    {
                        Buttons.transform.GetChild(2).gameObject.SetActive(false);
                        Buttons.transform.GetChild(2).gameObject.SetActive(true);
                    }
                    buttoncount = 1;
                    EventSystem.current.SetSelectedGameObject(Buttons.transform.GetChild(1).gameObject);

                }
                else if (tmp_result.gameObject.name == Buttons.transform.GetChild(2).name && GameVal.masterSave.is_story_clear)
                {
                    Buttons.transform.GetChild(0).gameObject.SetActive(false);
                    Buttons.transform.GetChild(0).gameObject.SetActive(true);
                    Buttons.transform.GetChild(1).gameObject.SetActive(false);
                    Buttons.transform.GetChild(1).gameObject.SetActive(true);
                    buttoncount = 2;
                    EventSystem.current.SetSelectedGameObject(Buttons.transform.GetChild(2).gameObject);

                }

            }
        }
        if (!fadeflag)
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
        EventSystem.current.SetSelectedGameObject(Buttons.transform.GetChild(0).gameObject);
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
    public void BacktoTitle()
    {
        fadeout.FadeIn(1, () =>
        {
            SceneManager.LoadScene("Title", LoadSceneMode.Single);
            Resources.UnloadUnusedAssets();
        });        
        Buttons.SetActive(false);
    }

    //
    public void BacktoStageSelect()
    {
        BattleVal.status = STATUS.DRAW_STAGE;
        fadeout.FadeIn(1, () =>
        {
            SceneManager.LoadScene("StageSelect", LoadSceneMode.Single);
            Resources.UnloadUnusedAssets();
        });
        Buttons.SetActive(false);
    }
}
