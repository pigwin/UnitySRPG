using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Text;
using System.IO;
using UnityEngine.EventSystems;

public class StageSelect : MonoBehaviour
{
    public GameObject gobjBackGroundImage;
    public GameObject gobjAllMenu;
    public GameObject gobjMenu;
    public GameObject gobjExtraStage;
    public GameObject gobjRetry;
    public GameObject gobjBonus;
    public GameObject gobjPartyCheck;
    public Fade fade;
    public Fade fadeout;
    bool fadeflag = false;
    [SerializeField] float logofadeintime = 0.5f;
    [SerializeField] float fadeintime = 2.0f;

    //セーブ用スクリーンショット（Title描画完了時に撮影）
    private Texture2D picture;
    //coroutine flag
    private bool screenshot_coroutine = false;
    //セーブ制御用オブジェクト
    SaveManager savemanager;
    //セーブ用キャンバス
    Canvas save_canvas;
    string nextscene;   //ロードした場合のシーン名

    public enum EXTRAMENUSTATE
    {
        MENU,
        SAVE,
        LOAD
    }
    EXTRAMENUSTATE state;


    // Start is called before the first frame update
    void Start()
    {
        if (System.IO.File.Exists(Application.streamingAssetsPath + Operation.loadtemp))
            System.IO.File.Delete(Application.streamingAssetsPath + Operation.loadtemp);
        fadeflag = false;

    }

    // Update is called once per frame
    void Update()
    {
        if (!fadeflag)
        {
            fade.FadeIn(fadeintime,()=> {
                //スクリーンショットを取る
                StartCoroutine(LoadScreenshot());
            });
            fadeflag = true;
        }
        else
        {
            gobjBackGroundImage.SetActive(true);
            
        }

        if(gobjBackGroundImage.activeSelf)
        {
            switch(state)
            {
                case EXTRAMENUSTATE.MENU:

                    break;

                case EXTRAMENUSTATE.SAVE:
                    Main_Save();
                    break;
                case EXTRAMENUSTATE.LOAD:
                    Main_Load();
                    break;
            }
        }
    }

    //ボタン処理
    //GameVal.nextscenenameのシーンへと遷移
    public void SceneChange(string scenename)
    {
        gobjAllMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
        GameVal.nextscenename = scenename;
        fadeout.FadeIn(1, () =>
        {
            SceneManager.LoadScene(GameVal.nextscenename, LoadSceneMode.Single);
            Resources.UnloadUnusedAssets();
        });

    }

    //すごく無理やり書く。
    public void StateChangeToExtraStage()
    {
        gobjExtraStage.GetComponent<StageSelectMenuAlpha>().FadeIn();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
    }
    public void StateChangeFromExtraStage()
    {
        gobjExtraStage.GetComponent<StageSelectMenuAlpha>().FadeOut();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeIn();
    }
    public void StateChangeToRetry()
    {
        gobjRetry.GetComponent<StageSelectMenuAlpha>().FadeIn();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
    }
    public void StateChangeFromRetry()
    {
        gobjRetry.GetComponent<StageSelectMenuAlpha>().FadeOut();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeIn();
    }
    public void StateChangeToBonus()
    {
        gobjBonus.GetComponent<StageSelectMenuAlpha>().FadeIn();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
    }
    public void StateChangeFromBonus()
    {
        gobjBonus.GetComponent<StageSelectMenuAlpha>().FadeOut();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeIn();
    }
    public void StateChangeToPartyCheck()
    {
        gobjPartyCheck.GetComponent<StageSelectMenuAlpha>().FadeIn();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
    }
    public void StateChangeFromPartyCheck()
    {
        gobjPartyCheck.GetComponent<StageSelectMenuAlpha>().FadeOut();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeIn();
    }

    
    //タイトルシーンへと遷移
    public void BacktoTitle()
    {
        gobjAllMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
        fadeout.FadeIn(1, () =>
        {
            SceneManager.LoadScene("Title", LoadSceneMode.Single);
            Resources.UnloadUnusedAssets();
        });
    }

    //ロードボタンが押されたときの関数
    public void LoadButton()
    {
        GameObject temp_object = (GameObject)Resources.Load("Prefab/SaveCanvas");
        save_canvas = Instantiate<Canvas>(temp_object.GetComponent<Canvas>());
        savemanager = save_canvas.GetComponent<SaveManager>();
        savemanager.LoadWindowOpen();
        state = EXTRAMENUSTATE.LOAD;
    }

    //ロード関数
    public void Main_Load()
    {
        if (EventSystem.current == null) return;
        if (Input.GetMouseButtonDown(1))
        {
            state = EXTRAMENUSTATE.MENU;
            savemanager.save_BackGround.gameObject.SetActive(false);
            for (int i = 0; i < savemanager.save_load_page; i++)
            {
                for (int j = 0; j < savemanager.save_load; j++)
                {
                    savemanager.save_object[i][j].gameObject.SetActive(true);
                    Destroy(savemanager.save_object[i][j]);
                }
            }
            savemanager.nextpagebutton.gameObject.SetActive(false);
            savemanager.prevpagebutton.gameObject.SetActive(false);
            savemanager.page_number.gameObject.SetActive(false);
            Destroy(save_canvas.GetComponent<CanvasScaler>());
            Destroy(save_canvas.GetComponent<GraphicRaycaster>());
            Destroy(save_canvas.gameObject);
            Destroy(save_canvas);
        }
        for (int page = 0; page < savemanager.save_load_page; page++)
        {
            for (int i = 0; i < savemanager.save_load; i++)
            {
                savemanager.save_object[page][i].gameObject.SetActive(page == savemanager.now_page);
            }
        }
        PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
        eventDataCurrent.position = Input.mousePosition;
        List<RaycastResult> raycast = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrent, raycast);
        for (int j = 0; j < savemanager.save_load_page; j++)
        {
            for (int i = 0; i < savemanager.save_load; i++)
            {
                savemanager.save_object[j][i].GetComponent<Image>().color = Color.white;
            }
        }
        foreach (RaycastResult tmp in raycast)
        {
            for (int page = 0; page < savemanager.save_load_page; page++)
            {
                for (int i = 0; i < savemanager.save_load; i++)
                {
                    //if ((savemanager.sd[page][i]as NovelSave).reading_pos == -1) continue;
                    if (tmp.gameObject.name == savemanager.save_object[page][i].gameObject.name)
                    {
                        savemanager.save_object[page][i].GetComponent<Image>().color = Color.red;
                        if (Input.GetMouseButtonDown(0))
                        {
                            FileStream fs = new FileStream(Application.streamingAssetsPath + "/SaveData/" + savemanager.save_object[page][i].gameObject.name + ".json", FileMode.Open, FileAccess.Read);
                            StreamReader sr = new StreamReader(fs);
                            //**注意**--------------------------------------------------------------------------------------------------------------------------------------------------------------
                            Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
                            //string source_file = ec.DecryptionSystem(sr.ReadToEnd(),false);
                            string source_file = ec.DecryptionSystem(sr.ReadToEnd(), true); //debug
                            ;
                            //**--------------------------------------------------------------------------------------------------------------------------------------------------------------------
                            NovelSave sd = JsonUtility.FromJson<NovelSave>(source_file);
                            sd.Load(); //パーティメンバーのロード
                            nextscene = sd.nameScene;

                            FileStream fs2 = new FileStream(Application.streamingAssetsPath + "/SaveData/loadtemp", FileMode.Create, FileAccess.Write);
                            StreamWriter sw = new StreamWriter(fs2);
                            sw.WriteLine(source_file);
                            sw.Flush();
                            fs2.Flush();
                            sw.Close();
                            fs2.Close();

                            sr.Close();
                            fs.Close();

                            //back_log.text = sd.backlog;
                            fadeout.FadeIn(fadeintime, () =>
                            {
                                gobjAllMenu.SetActive(false);
                                GameVal.nextscenename = nextscene;
                                SceneManager.LoadScene("BlackScene", LoadSceneMode.Single);
                                Resources.UnloadUnusedAssets();
                            });
                            savemanager.save_BackGround.gameObject.SetActive(false);
                            savemanager.nextpagebutton.gameObject.SetActive(false);
                            savemanager.prevpagebutton.gameObject.SetActive(false);
                            savemanager.page_number.gameObject.SetActive(false);
                            for (int k = 0; k < savemanager.save_load_page; k++)
                            {
                                for (int j = 0; j < savemanager.save_load; j++)
                                {
                                    savemanager.save_object[k][j].gameObject.SetActive(true);
                                    Destroy(savemanager.save_object[k][j]);
                                }
                            }
                            Destroy(save_canvas.GetComponent<CanvasScaler>());
                            Destroy(save_canvas.GetComponent<GraphicRaycaster>());
                            Destroy(save_canvas.gameObject);
                            Destroy(save_canvas);
                            gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
                            state = EXTRAMENUSTATE.MENU;
                            return;
                        }
                    }
                }
            }
        }

    }

    //セーブボタンが押されたときの関数
    public void SaveButton()
    {
        GameObject temp_object = (GameObject)Resources.Load("Prefab/SaveCanvas");
        save_canvas = Instantiate<Canvas>(temp_object.GetComponent<Canvas>());
        savemanager = save_canvas.GetComponent<SaveManager>();
        savemanager.LoadWindowOpen();
        state = EXTRAMENUSTATE.SAVE;
    }

    //セーブ関数
    public void Main_Save()
    {
        if (EventSystem.current == null) return;
        PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
        eventDataCurrent.position = Input.mousePosition;
        List<RaycastResult> raycast = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrent, raycast);
        if (Input.GetMouseButtonDown(1))
        {
            BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
            savemanager.save_BackGround.gameObject.SetActive(false);
            for (int page = 0; page < savemanager.save_load_page; page++)
            {
                for (int j = 0; j < savemanager.save_load; j++)
                {
                    savemanager.save_object[page][j].gameObject.SetActive(true);
                    Destroy(savemanager.save_object[page][j]);
                }
            }
            savemanager.nextpagebutton.gameObject.SetActive(false);
            savemanager.prevpagebutton.gameObject.SetActive(false);
            savemanager.page_number.gameObject.SetActive(false);
            Destroy(save_canvas.GetComponent<CanvasScaler>());
            Destroy(save_canvas.GetComponent<GraphicRaycaster>());
            Destroy(save_canvas.gameObject);
            Destroy(save_canvas);
        }
        for (int page = 0; page < savemanager.save_load_page; page++)
        {
            for (int i = 0; i < savemanager.save_load; i++)
            {
                savemanager.save_object[page][i].gameObject.SetActive(savemanager.now_page == page);
            }
        }
        Debug.Log(savemanager.now_page);
        for (int i = 0; i < savemanager.save_load; i++)
        {
            savemanager.save_object[savemanager.now_page][i].GetComponent<Image>().color = Color.white;
        }
        foreach (RaycastResult tmp in raycast)
        {
            for (int i = 0; i < savemanager.save_load; i++)
            {
                if (tmp.gameObject.name == savemanager.save_object[savemanager.now_page][i].gameObject.name)
                {
                    savemanager.save_object[savemanager.now_page][i].GetComponent<Image>().color = Color.red;
                    if (Input.GetMouseButtonDown(0))
                    {
                        Savedata SD = new Savedata();
                        SD.Save();
                        SD.nameScene = SceneManager.GetActiveScene().name;
                        SD.nameTitle = "夢現の旅籠";

                        //--------------------------------------------------------------------------------------
                        SD.current_message = string.Format("夢現の旅籠\n{0}/{1}/{2}/{3}:{4}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
                        //---------------------------------------------------------------------------------------

                        FileStream fs = new FileStream(Application.streamingAssetsPath + "/SaveData/" + tmp.gameObject.name + ".json", FileMode.Create, FileAccess.Write);
                        StreamWriter sw = new StreamWriter(fs);
                        //**注意**--------------------------------------------------------------------------------------------------------------------------------------------------------------
                        Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
                        //sw.WriteLine(ec.EncryptionSystem(JsonUtility.ToJson(SD),false));
                        sw.WriteLine(ec.EncryptionSystem(JsonUtility.ToJson(SD), true));

                        //**--------------------------------------------------------------------------------------------------------------------------------------------------------------------
                        //sw.WriteLine(JsonUtility.ToJson(SD));
                        sw.Flush();
                        sw.Close();
                        fs.Close();
                        byte[] bytes = picture.EncodeToPNG();
                        File.WriteAllBytes(Application.streamingAssetsPath + "/SaveData/" + tmp.gameObject.name + ".png", bytes);
                        state = EXTRAMENUSTATE.MENU;
                        savemanager.save_BackGround.gameObject.SetActive(false);
                        savemanager.nextpagebutton.gameObject.SetActive(false);
                        savemanager.prevpagebutton.gameObject.SetActive(false);
                        savemanager.page_number.gameObject.SetActive(false);

                        for (int k = 0; k < savemanager.save_load_page; k++)
                        {
                            for (int j = 0; j < savemanager.save_load; j++)
                            {
                                Destroy(savemanager.save_object[k][j]);
                            }
                        }
                        Destroy(save_canvas.GetComponent<CanvasScaler>());
                        Destroy(save_canvas.GetComponent<GraphicRaycaster>());
                        Destroy(save_canvas.gameObject);
                        Destroy(save_canvas);
                        return;
                    }
                }
            }
        }
    }

    //セーブ用スクリーンショットを撮る関数
    IEnumerator LoadScreenshot()
    {
        if (screenshot_coroutine) yield break;
        screenshot_coroutine = true;
        yield return new WaitForEndOfFrame();
        picture = new Texture2D(Screen.width, Screen.height);
        picture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        picture.Apply();
        screenshot_coroutine = false;
    }
}
