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
    }
    float timer = 0;
    [SerializeField]float waittime = 1.0f;
    // Update is called once per frame
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
                }
                else
                    imgButton.alpha += Time.deltaTime / logofadeintime;
                break;
            case TITLESTATE.WAIT:
                break;
            case TITLESTATE.LOAD:
                Main_Load();
                break;
        }
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
    }

    //ロード関数
    public void Main_Load()
    {

        if (EventSystem.current == null) return;
        if (Input.GetMouseButtonDown(1))
        {
            titlestate = TITLESTATE.WAIT;
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
                            return;
                        }
                    }
                }
            }
        }

    }
}
