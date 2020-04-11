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

    List<GameObject> button_list;
    GameObject top_button;

    //夢現の塔クリアフラグ
    [SerializeField] GameObject gobjStar;

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
        GameVal.masterSave.is_story_clear = true;
        //Windows
        //if (System.IO.File.Exists(Application.streamingAssetsPath + Operation.loadtemp))
        //Mac
        if (System.IO.File.Exists(Application.persistentDataPath + Operation.loadtemp))
            //Windows
            //System.IO.File.Delete(Application.streamingAssetsPath + Operation.loadtemp);
            //Mac
            System.IO.File.Delete(Application.persistentDataPath + Operation.loadtemp);
        fadeflag = false;
        gobjStar.SetActive(GameVal.masterSave.is_clear_mugen);
    }
    bool timing = true;
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
                    if(timing)Main_Save();
                    break;
                case EXTRAMENUSTATE.LOAD:
                    if(timing)Main_Load();
                    break;
            }
        }
        timing = true;
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
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);

    }
    public void StateChangeFromExtraStage()
    {
        gobjExtraStage.GetComponent<StageSelectMenuAlpha>().FadeOut();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeIn();
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);

    }
    public void StateChangeToRetry()
    {
        gobjRetry.GetComponent<StageSelectMenuAlpha>().FadeIn();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);

    }
    public void StateChangeFromRetry()
    {
        gobjRetry.GetComponent<StageSelectMenuAlpha>().FadeOut();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeIn();
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);

    }
    public void StateChangeToBonus()
    {
        gobjBonus.GetComponent<StageSelectMenuAlpha>().FadeIn();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);

    }
    public void StateChangeFromBonus()
    {
        gobjBonus.GetComponent<StageSelectMenuAlpha>().FadeOut();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeIn();
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);
    }
    public void StateChangeToPartyCheck()
    {
        gobjPartyCheck.GetComponent<StageSelectMenuAlpha>().FadeIn();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);

    }
    public void StateChangeFromPartyCheck()
    {
        gobjPartyCheck.GetComponent<StageSelectMenuAlpha>().FadeOut();
        gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeIn();
        this.gameObject.SetActive(false);
        this.gameObject.SetActive(true);
        EventSystem.current.SetSelectedGameObject(null);

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
        timing = false;
    }
    int sellect_num = 0;
    float quit_time = 0;
    bool mouse_use = false;
    //ロード関数
    public void Main_Load()
    {
        if (EventSystem.current == null) return;
        if (Input.GetButtonDown("Cancel"))
        {
            state = EXTRAMENUSTATE.MENU;
            savemanager.save_BackGround.gameObject.SetActive(false);
                for (int j = 0; j < savemanager.save_load; j++)
                {
                    savemanager.Show_Object[j].gameObject.SetActive(true);
                    Destroy(savemanager.Show_Object[j]);
                }
            savemanager.nextpagebutton.gameObject.SetActive(false);
            savemanager.prevpagebutton.gameObject.SetActive(false);
            savemanager.page_number.gameObject.SetActive(false);
            Destroy(save_canvas.GetComponent<CanvasScaler>());
            Destroy(save_canvas.GetComponent<GraphicRaycaster>());
            Destroy(save_canvas.gameObject);
            Destroy(save_canvas);
            return;
        }
        PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
        eventDataCurrent.position = Input.mousePosition;
        List<RaycastResult> raycast = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrent, raycast);
        for (int j = 0; j < savemanager.save_load_page; j++)
        {
            for (int i = 0; i < savemanager.save_load; i++)
            {
                savemanager.Show_Object[i].GetComponent<Image>().color = Color.white;
            }
        }
        if (quit_time == 0)
        {
            quit_time = Time.time - 0.2f;
            return;
        }
        if (Input.GetAxisRaw("Vertical") == 0 && Input.GetAxisRaw("Horizontal") == 0)
        {
            quit_time = Time.time - 0.2f;
        }
        if (Time.time - quit_time > 0.2f)
        {
            if (Input.GetAxisRaw("Vertical") == 1)
            {
                sellect_num = (sellect_num + (savemanager.save_load - 1)) % savemanager.save_load;
                quit_time = Time.time;
                mouse_use = false;
            }
            else if (Input.GetAxisRaw("Vertical") == -1)
            {
                sellect_num = (sellect_num + 1) % savemanager.save_load;
                quit_time = Time.time;
                mouse_use = false;
            }
            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                savemanager.NextPageButton();
                quit_time = Time.time;
                return;
            }
            else if (Input.GetAxisRaw("Horizontal") < 0)
            {
                savemanager.PrevPageButton();
                quit_time = Time.time;
                return;
            }
        }
        int temp_num;
        bool check = false;
        foreach (RaycastResult tmp in raycast)
        {
            for (int page = 0; page < savemanager.save_load_page; page++)
            {
                for (int i = 0; i < savemanager.save_load; i++)
                {
                    if (!int.TryParse(savemanager.Show_Object[i].gameObject.name, out temp_num)) break;
                    if (tmp.gameObject.transform.parent.gameObject.name == savemanager.Show_Object[i].gameObject.name)
                    {
                        check = true;
                        sellect_num = temp_num - 1;
                        mouse_use = true;
                        break;
                    }
                    else if (tmp.gameObject.name == savemanager.Show_Object[i].gameObject.name)
                    {
                        check = true;
                        sellect_num = temp_num - 1;
                        mouse_use = true;
                        break;
                    }
                }
                if (check) break;
            }
            if (check) break;
        }
        if (Input.GetButtonDown("Submit"))
        {
            if (EventSystem.current.currentSelectedGameObject == savemanager.prevpagebutton.gameObject || EventSystem.current.currentSelectedGameObject == savemanager.nextpagebutton.gameObject) return;

            //Windows
            //FileStream fs = new FileStream(Application.streamingAssetsPath + "/SaveData/" + savemanager.Show_Object[sellect_num % savemanager.save_load].gameObject.name + ".json", FileMode.Open, FileAccess.Read);
            //Mac
            FileStream fs = new FileStream(Application.persistentDataPath + "/SaveData/" + savemanager.Show_Object[sellect_num % savemanager.save_load].gameObject.name + ".json", FileMode.Open, FileAccess.Read);

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

            //Windows
            //FileStream fs2 = new FileStream(Application.streamingAssetsPath + "/SaveData/loadtemp", FileMode.Create, FileAccess.Write);
            //Mac
            FileStream fs2 = new FileStream(Application.persistentDataPath + "/SaveData/loadtemp", FileMode.Create, FileAccess.Write);
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
                for (int j = 0; j < savemanager.save_load; j++)
                {
                    savemanager.Show_Object[j].gameObject.SetActive(true);
                    Destroy(savemanager.Show_Object[j]);
                }
            Destroy(save_canvas.GetComponent<CanvasScaler>());
            Destroy(save_canvas.GetComponent<GraphicRaycaster>());
            Destroy(save_canvas.gameObject);
            Destroy(save_canvas);
            gobjMenu.GetComponent<StageSelectMenuAlpha>().FadeOut();
            state = EXTRAMENUSTATE.MENU;
            return;
        }
        if (!check && mouse_use)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        else if (!mouse_use)
        {
            EventSystem.current.SetSelectedGameObject(savemanager.Show_Object[(sellect_num % savemanager.save_load)].gameObject);
        }

    }

    //セーブボタンが押されたときの関数
    public void SaveButton()
    {
        GameObject temp_object = (GameObject)Resources.Load("Prefab/SaveCanvas");
        save_canvas = Instantiate<Canvas>(temp_object.GetComponent<Canvas>());
        savemanager = save_canvas.GetComponent<SaveManager>();
        savemanager.SaveWindowOpen();
        state = EXTRAMENUSTATE.SAVE;
        timing = false;
    }

    //セーブ関数
    public void Main_Save()
    {
        if (EventSystem.current == null) return;
        PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
        eventDataCurrent.position = Input.mousePosition;
        List<RaycastResult> raycast = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrent, raycast);
        if (Input.GetButtonDown("Cancel"))
        {
            state = EXTRAMENUSTATE.MENU;
            savemanager.save_BackGround.gameObject.SetActive(false);
                for (int j = 0; j < savemanager.save_load; j++)
                {
                    savemanager.Show_Object[j].gameObject.SetActive(true);
                    Destroy(savemanager.Show_Object[j]);
                }
            savemanager.nextpagebutton.gameObject.SetActive(false);
            savemanager.prevpagebutton.gameObject.SetActive(false);
            savemanager.page_number.gameObject.SetActive(false);
            Destroy(save_canvas.GetComponent<CanvasScaler>());
            Destroy(save_canvas.GetComponent<GraphicRaycaster>());
            Destroy(save_canvas.gameObject);
            Destroy(save_canvas);
            return;
        }
        //Debug.Log(savemanager.now_page);
        for (int i = 0; i < savemanager.save_load; i++)
        {
            savemanager.Show_Object[i].GetComponent<Image>().color = Color.white;
        }
        if (Input.GetAxisRaw("Vertical") == 0 && Input.GetAxisRaw("Horizontal") == 0)
        {
            quit_time = Time.time - 0.2f;
        }
        if (Time.time - quit_time > 0.2f)
        {
            if (Input.GetAxisRaw("Vertical") == 1)
            {
                sellect_num = (sellect_num + (savemanager.save_load - 1)) % savemanager.save_load;
                quit_time = Time.time;
                mouse_use = false;
            }
            else if (Input.GetAxisRaw("Vertical") == -1)
            {
                sellect_num = (sellect_num + 1) % savemanager.save_load;
                quit_time = Time.time;
                mouse_use = false;
            }
            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                savemanager.NextPageButton();
                quit_time = Time.time;
                return;
            }
            else if (Input.GetAxisRaw("Horizontal") < 0)
            {
                savemanager.PrevPageButton();
                quit_time = Time.time;
                return;
            }
        }
        bool check = false;
        int temp_num = 0;
        foreach (RaycastResult tmp_result in raycast)
        {
            for (int page = 0; page < savemanager.save_load_page; page++)
            {
                for (int i = 0; i < savemanager.save_load; i++)
                {
                    if (!int.TryParse(savemanager.Show_Object[i].gameObject.name, out temp_num)) break;
                    if (tmp_result.gameObject.transform.parent.gameObject.name == savemanager.Show_Object[i].gameObject.name)
                    {
                        check = true;
                        sellect_num = temp_num - 1;
                        mouse_use = true;
                        break;
                    }
                    else if (tmp_result.gameObject.name == savemanager.Show_Object[i].gameObject.name)
                    {
                        check = true;
                        sellect_num = temp_num - 1;
                        mouse_use = true;
                        break;
                    }
                }
                if (check) break;
            }
            if (check) break;
        }
        GameObject tmp = savemanager.Show_Object[sellect_num % savemanager.save_load].gameObject;
        if (Input.GetButtonDown("Submit"))
        {
            if (EventSystem.current.currentSelectedGameObject == savemanager.prevpagebutton.gameObject || EventSystem.current.currentSelectedGameObject == savemanager.nextpagebutton.gameObject) return;
            quit_time = 0;
            Savedata SD = new Savedata();
            SD.Save();
            SD.nameScene = SceneManager.GetActiveScene().name;
            SD.nameTitle = "夢現の旅籠";

            //--------------------------------------------------------------------------------------
            SD.current_message = string.Format("夢現の旅籠\n{0}/{1}/{2}/{3}:{4}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute);
            //---------------------------------------------------------------------------------------

            //Windows
            //FileStream fs = new FileStream(Application.streamingAssetsPath + "/SaveData/" + tmp.gameObject.name + ".json", FileMode.Create, FileAccess.Write);
            //Mac
            FileStream fs = new FileStream(Application.persistentDataPath + "/SaveData/" + tmp.gameObject.name + ".json", FileMode.Create, FileAccess.Write);
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
            //Windows
            //File.WriteAllBytes(Application.streamingAssetsPath + "/SaveData/" + tmp.gameObject.name + ".png", bytes);
            //Mac
            File.WriteAllBytes(Application.persistentDataPath + "/SaveData/" + tmp.gameObject.name + ".png", bytes);
            state = EXTRAMENUSTATE.MENU;
            savemanager.save_BackGround.gameObject.SetActive(false);
            savemanager.nextpagebutton.gameObject.SetActive(false);
            savemanager.prevpagebutton.gameObject.SetActive(false);
            savemanager.page_number.gameObject.SetActive(false);
            
                for (int j = 0; j < savemanager.save_load; j++)
                {
                    Destroy(savemanager.Show_Object[j]);
                }
            Destroy(save_canvas.GetComponent<CanvasScaler>());
            Destroy(save_canvas.GetComponent<GraphicRaycaster>());
            Destroy(save_canvas.gameObject);
            Destroy(save_canvas);
            return;
        }
        if (!check && mouse_use)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
        else if (!mouse_use)
        {
            EventSystem.current.SetSelectedGameObject(savemanager.Show_Object[(sellect_num % savemanager.save_load)].gameObject);
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
