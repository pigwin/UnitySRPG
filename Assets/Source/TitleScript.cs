using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Text;
using System.IO;
using UnityEngine.EventSystems;

/*************************************/
/*Title title タイトル               */
/*  タイトルのシーンを制御するクラス */
/*************************************/

[RequireComponent(typeof(NewGameSetting))]
public class TitleScript : MonoBehaviour
{
    public AudioSource bgmAudioSource;
    public AudioClip bgmAudioClip;
    public Button newgamebutton;
    public Button loadgamebutton;
    [SerializeField] GameObject titleGraph;
    enum TITLESTATE
    {
        LOGO1IN,
        LOGO1WAIT,
        LOGO1OUT,
        INTERVAL,
        LOGO2IN,
        LOGO2WAIT,
        LOGO2OUT,
        FADEIN,
        BUTTONSHOWSTART,
        WAIT,
        LOAD,
        FADEOUT
    }
    TITLESTATE titlestate = TITLESTATE.LOGO1IN;
    [SerializeField] CanvasGroup imgLogo1;
    [SerializeField] CanvasGroup imgLogo2;
    [SerializeField] CanvasGroup imgButton;
    [SerializeField] float logofadeintime = 0.5f;
    [SerializeField] float fadeintime = 2.0f;
    [SerializeField] Fade fadein;
    [SerializeField] Fade fadeout;
    NewGameSetting newGameSetting;

    //セーブ制御用オブジェクト
    SaveManager savemanager;
    //セーブ用キャンバス
    Canvas save_canvas;
    string nextscene;   //ロードした場合のシーン名

    // Start is called before the first frame update
    void Start()
    {
        newGameSetting = GetComponent<NewGameSetting>();

        //Mac Savedataフォルダの作成
        if(!System.IO.Directory.Exists(Application.persistentDataPath + "/Savedata/"))
        {
            System.IO.Directory.CreateDirectory(Application.persistentDataPath + "/Savedata/");
        }
    }
    float timer = 0;
    [SerializeField]float waittime = 1.0f;
    // Update is called once per frame
    bool timing = true;
    void Update()
    {
        switch(titlestate)
        {
            case TITLESTATE.LOGO1IN:
                if (imgLogo1.alpha >= 1)
                {
                    titlestate = TITLESTATE.LOGO1WAIT;
                    timer = 0;
                }
                else
                    imgLogo1.alpha += Time.deltaTime/logofadeintime;
                break;
            case TITLESTATE.LOGO1WAIT:
                if (timer >= waittime)
                    titlestate = TITLESTATE.LOGO1OUT;
                else
                    timer += Time.deltaTime;
                break;
            case TITLESTATE.LOGO1OUT:
                if (imgLogo1.alpha <= 0)
                {
                    titlestate = TITLESTATE.INTERVAL;
                    timer = 0;
                }
                else
                    imgLogo1.alpha -= Time.deltaTime / logofadeintime;
                break;
            case TITLESTATE.INTERVAL:
                if (timer >= waittime)
                    titlestate = TITLESTATE.LOGO2IN;
                else
                    timer += Time.deltaTime;
                break;
            case TITLESTATE.LOGO2IN:
                if (imgLogo2.alpha >= 1)
                {
                    titlestate = TITLESTATE.LOGO2WAIT;
                    timer = 0;
                }
                else
                    imgLogo2.alpha += Time.deltaTime / logofadeintime;
                break;
            case TITLESTATE.LOGO2WAIT:
                if (timer >= waittime)
                    titlestate = TITLESTATE.LOGO2OUT;
                else
                    timer += Time.deltaTime;
                break;
            case TITLESTATE.LOGO2OUT:
                if (imgLogo2.alpha <= 0)
                {
                    bgmAudioSource.clip = bgmAudioClip;
                    bgmAudioSource.Play();
                    fadein.FadeIn(fadeintime, () =>
                    {
                        titlestate = TITLESTATE.BUTTONSHOWSTART;
                    });
                    titlestate = TITLESTATE.FADEIN;
                }
                else
                    imgLogo2.alpha -= Time.deltaTime / logofadeintime;
                break;
            case TITLESTATE.FADEIN:
                titleGraph.SetActive(true);
                break;
            case TITLESTATE.BUTTONSHOWSTART:
                if (imgButton.alpha >= 1)
                {
                    titlestate = TITLESTATE.WAIT;
                    EventSystem.current.SetSelectedGameObject(newgamebutton.gameObject);
                }
                else
                    imgButton.alpha += Time.deltaTime / logofadeintime;
                break;
            case TITLESTATE.WAIT:
                
                if (EventSystem.current.IsPointerOverGameObject())
                {
                    {
                        PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
                        eventDataCurrent.position = Input.mousePosition;
                        List<RaycastResult> raycast = new List<RaycastResult>();
                        EventSystem.current.RaycastAll(eventDataCurrent, raycast);
                        foreach(RaycastResult temp in raycast)
                        {
                            if(temp.gameObject == newgamebutton.gameObject || temp.gameObject == loadgamebutton.gameObject)
                            {
                                EventSystem.current.SetSelectedGameObject(temp.gameObject);
                            }
                        }
                    }
                }
                else
                {
                    try
                    {
                        if (EventSystem.current.currentSelectedGameObject == null)
                        {
                            EventSystem.current.SetSelectedGameObject(newgamebutton.gameObject);
                        }
                    }
                    catch
                    {
                        EventSystem.current.SetSelectedGameObject(newgamebutton.gameObject);
                    }
                    if (Input.GetButtonDown("Submit"))
                    {
                        if(EventSystem.current.currentSelectedGameObject == newgamebutton.gameObject)
                        {
                            SceneChange();
                        }else if(EventSystem.current.currentSelectedGameObject == loadgamebutton.gameObject)
                        {
                            titlestate = TITLESTATE.LOAD;
                        }
                    }
                }
                break;
            case TITLESTATE.LOAD:
                if(timing)Main_Load();
                break;
        }
        if (!timing) timing = true;
    }

    //ボタン処理
    //GameVal.nextscenenameのシーンへと遷移
    public void SceneChange()
    {
        newgamebutton.interactable = false;
        loadgamebutton.interactable = false;
        newGameSetting.PushButton();
        fadeout.FadeIn(fadeintime, () =>
         {
             SceneManager.LoadScene("BlackScene", LoadSceneMode.Single);
             Resources.UnloadUnusedAssets();
         });
        
    }

    //ロードボタンが押されたときの関数
    public void LoadButton()
    {
        if (titlestate != TITLESTATE.WAIT) return;
        GameObject temp_object = (GameObject)Resources.Load("Prefab/SaveCanvas");
        save_canvas = Instantiate<Canvas>(temp_object.GetComponent<Canvas>());
        savemanager = save_canvas.GetComponent<SaveManager>();
        savemanager.LoadWindowOpen();
        titlestate = TITLESTATE.LOAD;
        timing = false;
    }
    float quit_time = 0.0f;
    int sellect_num = 0;
    bool mouse_use;
    //ロード関数
    public void Main_Load()
    {

        if (EventSystem.current == null) return;
        if (Input.GetButtonDown("Cancel"))
        {
            titlestate = TITLESTATE.WAIT;
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
        }
        PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
        eventDataCurrent.position = Input.mousePosition;
        List<RaycastResult> raycast = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrent, raycast);
            for (int i = 0; i < savemanager.save_load; i++)
            {
                savemanager.Show_Object[i].GetComponent<Image>().color = Color.white;
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
            if (Input.GetAxisRaw("Vertical") >0)
            {
                sellect_num = (sellect_num + (savemanager.save_load - 1)) % savemanager.save_load;
                quit_time = Time.time;
                mouse_use = false;
            }
            else if (Input.GetAxisRaw("Vertical") < 0)
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
        //savemanager.save_object[savemanager.now_page][sellect_num].GetComponent<Image>().color = Color.red;
        if (Input.GetButtonDown("Submit"))
        {
            if (EventSystem.current.currentSelectedGameObject == savemanager.prevpagebutton.gameObject || EventSystem.current.currentSelectedGameObject == savemanager.nextpagebutton.gameObject) return;

            //Windows
            //FileStream fs = new FileStream(Application.streamingAssetsPath + "/SaveData/" + savemanager.Show_Object[sellect_num%savemanager.save_load].gameObject.name + ".json", FileMode.Open, FileAccess.Read);
            //Mac
            FileStream fs = new FileStream(Application.persistentDataPath + "/SaveData/" + savemanager.Show_Object[sellect_num%savemanager.save_load].gameObject.name + ".json", FileMode.Open, FileAccess.Read);
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
                loadgamebutton.interactable = false;
                newgamebutton.interactable = false;
                GameVal.nextscenename = nextscene;
                SceneManager.LoadScene("BlackScene", LoadSceneMode.Single);
                Resources.UnloadUnusedAssets();
            });
            titlestate = TITLESTATE.FADEOUT;
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
}
