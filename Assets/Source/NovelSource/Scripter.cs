using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Text;
using System.IO;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;
/************************************************************/
/*ノベルパートように使うセーブデータクラス                  */
/************************************************************/
public class NovelSave :Savedata
{
    public int reading_pos;
    public string right_name;
    public Sprite right_sprite;
    public Color right_color;
    public string center_name;
    public Sprite center_sprite;
    public Color center_color;
    public string left_name;
    public Sprite left_sprite;
    public Color left_color;
    public string backlog;
    public string background_name;
    public Sprite background_sprite;
    public Color background_color;
    public string bgm_name;
    public string charactervoice_name;
    public bool bgmflag;
    //Save関数
    public void Save(int reading_pos, string right_name, Sprite right_sprite, Color right_color,
                     string center_name, Sprite center_sprite, Color center_color,
                     string left_name, Sprite left_sprite, Color left_color,
                     string backlog, string background_name, Sprite background_sprite,
                     Color background_color, string stage_title,string bgm_name,string charactervoice_name,bool bgmflag)
    {
        base.Save();

        this.reading_pos = reading_pos;
        this.right_name = right_name;
        this.right_sprite = right_sprite;
        this.right_color = right_color;
        this.center_name = center_name;
        this.center_sprite = center_sprite;
        this.center_color = center_color;
        this.left_name = left_name;
        this.left_sprite = left_sprite;
        this.left_color = left_color;
        this.backlog = backlog;
        this.background_name = background_name;
        this.background_sprite = background_sprite;
        this.background_color = background_color;
        this.nameScene = SceneManager.GetActiveScene().name;
        this.nameTitle = stage_title;
        this.bgm_name = bgm_name;
        this.charactervoice_name = charactervoice_name;
        this.bgmflag = bgmflag;
    }

    public override void Load()
    {
        base.Load();
    }

}
/*****************************************************************************************/
/*背景・立ち絵のオブジェクトをまとめるクラス                                             */
/*１つの「画面」に相当する                                                               */
/*エフェクトによる立ち絵変化などでは、二つのNovelScreenをアニメーションと共に切り替える。*/
/*****************************************************************************************/
public class NovelScreen
{
    //親
    [HideInInspector]
    public GameObject gobj;

    //子
    [HideInInspector]
    public Image background;
    [HideInInspector]
    public Image left;
    [HideInInspector]
    public Image center;
    [HideInInspector]
    public Image right;
    [HideInInspector]
    public Image monocromask;

    public void Copy(NovelScreen a)
    {
        this.background.sprite = null;
        this.background.sprite = a.background.sprite;
        this.background.name = a.background.name;
        this.background.color = a.background.color;
        this.left.sprite = a.left.sprite;
        this.left.color = a.left.color;
        this.left.name = a.left.name;
        this.right.sprite = a.right.sprite;
        this.right.color = a.right.color;
        this.right.name = a.right.name;
        this.center.sprite = a.center.sprite;
        this.center.color = a.center.color;
        this.center.name = a.center.name;
        this.monocromask.sprite = a.monocromask.sprite;
        this.monocromask.name = a.monocromask.name;
        this.monocromask.color = a.monocromask.color;
        this.monocromask.gameObject.SetActive(a.monocromask.gameObject.activeSelf);
    }

    public NovelScreen(GameObject inspectorgobj)
    {
        //InspectorからGameobjectをもらってくる
        gobj = inspectorgobj;
        if (gobj.GetComponent<CanvasGroup>() == null)
        {
            gobj.AddComponent<CanvasGroup>();
        }
        //子を探索
        GameObject temp = gobj.transform.Find("left").gameObject;
        left = temp.GetComponent<Image>();
        temp = gobj.transform.Find("center").gameObject;
        center = temp.GetComponent<Image>();
        temp = gobj.transform.Find("right").gameObject;
        right = temp.GetComponent<Image>();

        temp = gobj.transform.Find("background").gameObject;
        background = temp.GetComponent<Image>();

        temp = gobj.transform.Find("monocro mask").gameObject;
        monocromask = temp.GetComponent<Image>();

    }
}
/************************************************************/
/*Novel scripter ノベル　本処理*/
/*  ノベルパートの処理を行うクラス*/
/************************************************************/
public class Scripter : MonoBehaviour {
    //ノベルパートのタイトル
    public string title_name;

    //UI制御用
    public GameObject mainui;

    //キャラクターネームを表示するテキスト
    public Text characterText;
    //キャラネームボックス
    public Image namebox;
    //本文を表示するテキスト
    //public Text text;
    //本文を表示するテキスト(MeshPro版)
    public TextMeshProUGUI text;
    //使用するスクリプトの名前
    public string scriptpath;

    //キーボードが連続押しされないようにするためのブール変数
    private bool check_key = true;
    //実行するアニメーションの番号を保存する変数
    private ANIMATION_TYPE registered_animation = ANIMATION_TYPE.ANIMATION_NO_TIME;

    //fadeinアニメーション用タイマー
    private int anim_timer;
    //バックログを表示するオブジェクト
    public Text back_log;
    public Image back_log_Stage;
    public ScrollRect VScroll;
    //キャンバスの大きさを取得するためのオブジェクト(対処法は別に考えるべき)
    public Canvas canvas;

    //フェード処理をするときのオブジェクト
    //public FadeImage fade;
    private int fade_method = 0;
    public GameObject fadeui;

    //セーブ画面用のバックグラウンド
    public Image save_background;
    //セーブデータ選択用のアイコン
    public GameObject Prefab;

    //セーブメニューテキスト
    public Text save_menu_text;
    public Text load_menu_text;

    //画面の切り替えを行うステータス
    SCREEN_STATUS screen = SCREEN_STATUS.MAIN;

    //クリック待ちアニメーション
    public Animator PageChangeAnim;
    public Animator PageWaitAnim;

    [SerializeField]
    //一文字を出力する時間
    private int wait_time = 100;
    //現在のスクリプトの位置を表す変数
    private int reading_pos = 0;
    //文字の色を管理する変数
    private Color chara_color;
    //SE用の音声ソース
    public AudioSource se;
    //BGM用の音声ソース
    public AudioSource bgm;
    //キャラクター音声ソース
    public AudioSource character_voice;
    //次の行を読み解きに音声消去をするかどうかのフラグ
    private bool isvoicestop = true;

    //delay wait 命令
    private int delaytime;
    private int delaynowtime;
    private bool skipflag;

    //フロントスクリーン・バックスクリーンのゲームオブジェクト
    public GameObject frontscreen_inspector;
    public GameObject backscreen_inspector;
    public Sprite skeltonsprite;
    //
    [HideInInspector]
    public NovelScreen FrontScreen;
    [HideInInspector]
    public NovelScreen BackScreen;

    //セーブボタン
    public Button savebutton;
    //ロードボタン
    public Button loadbutton;

    //monocro命令用マスク
    public Image monocromask;

    //登録されたアニメーションとそのオブジェクトの最高登録数
    //const int MAX_REGISTERED_ANIMATION = 10;

    //アニメーションルール番号
    int animrulenum = 0; 

    //一行に表示する文字の数
    const int MAX_CHARACTER_LINE = 28;
    //現在カウントしている文字数
    int character_count = 0;
    //現在の行数
    int line_count = 0;
    //セーブ制御用オブジェクト
    SaveManager savemanager;
    //セーブ用キャンバス
    Canvas save_canvas;
    
    //読み込むか読み込まないかの状態を表す変数
    private NOVEL_STATUS state = NOVEL_STATUS.NEXT;
    //スクリプトのクエリごとに保存する。
    List<Scription> scriptions = new List<Scription>();
    //今から実行するスクリプトを格納するScriptionクラス
    Scription now;

    bool skip_button = false;

    //次のシーン
    public string[] next_stage;
    private NovelSave[][] sd;
    private GameObject[][] save_object;
    private Texture2D picture;
    private string nextscene = "";

    enum DRAW_SCENECHANGE_STATE
    {
        FADEOUTSTART,
        NEXT
    }
    DRAW_SCENECHANGE_STATE draw_scenechange_state = DRAW_SCENECHANGE_STATE.FADEOUTSTART;
    [SerializeField] float timefadeout = 1.0f;
    [SerializeField] Fade fademask;
    
    string DeleteRubyCommand(string check)
    {
        string[] temp = check.Split('\\');
        check = temp[0];
        for (int i = 1; i < temp.Length; i++)
        {
            if (i % 2 == 0)
            {
                check += temp[i].Substring(3);
            }
            else
            {
                string[] before_slash = temp[i].Split('/');
                string[] contents = before_slash[0].Split('>');
                int num = contents.Length - 1;
                check += '(' + contents[num].Substring(0, contents[num].Length - 2) + ')';
            }
        }
        return check;
    }
    //セーブ関数
    public void Main_Save()
    {
        if (EventSystem.current == null) return;
        PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
        eventDataCurrent.position = Input.mousePosition;
        List<RaycastResult>raycast = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrent, raycast);
        if (Input.GetMouseButtonDown(1))
        {
            screen = SCREEN_STATUS.MAIN;
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
        foreach(RaycastResult tmp in raycast)
        {
            for(int i = 0; i < savemanager.save_load; i++)
            {
                if(tmp.gameObject.name == savemanager.save_object[savemanager.now_page][i].gameObject.name)
                {
                    savemanager.save_object[savemanager.now_page][i].GetComponent<Image>().color = Color.red;
                    if (Input.GetMouseButtonDown(0))
                    {

                        NovelSave SD = new NovelSave();
                        string bgmname = "";
                        string voicename = "";
                        if (bgm.clip != null)
                        {
                            bgmname = bgm.clip.name;
                        }
                        if(character_voice.clip != null)
                        {
                            voicename = character_voice.clip.name;
                        }
                        SD.Save(reading_pos, FrontScreen.right.name, FrontScreen.right.sprite, new Color(FrontScreen.right.color.r, FrontScreen.right.color.g, FrontScreen.right.color.b, 1),
                            FrontScreen.center.name, FrontScreen.center.sprite, new Color(FrontScreen.center.color.r, FrontScreen.center.color.g, FrontScreen.center.color.b, 1),
                            FrontScreen.left.name, FrontScreen.left.sprite, new Color(FrontScreen.left.color.r, FrontScreen.left.color.g, FrontScreen.left.color.b, 1),
                            back_log.text, FrontScreen.background.name, FrontScreen.background.sprite, FrontScreen.background.color, title_name, bgmname, voicename, bgm.isPlaying);

                        //--------------------------------------------------------------------------------------
                        SD.current_message = string.Format("ストーリー「{5}」\n{0}/{1}/{2}/{3}:{4}",DateTime.Now.Year,DateTime.Now.Month,DateTime.Now.Day,DateTime.Now.Hour,DateTime.Now.Minute, title_name);
                        //---------------------------------------------------------------------------------------
                        for(int j = reading_pos-1; j >= 0; j--)
                        {
                            if (scriptions[j].type == COMMAND_TYPE.NORMAL_TEXT)
                            {
                                SD.reading_pos = j;
                                break;
                            }
                        }
                        FileStream fs = new FileStream(Application.streamingAssetsPath+"/SaveData/" + tmp.gameObject.name + ".json", FileMode.Create, FileAccess.Write);
                        StreamWriter sw = new StreamWriter(fs);
                        //**注意**--------------------------------------------------------------------------------------------------------------------------------------------------------------
                        Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
                        //sw.WriteLine(ec.EncryptionSystem(JsonUtility.ToJson(SD),false));
                        sw.WriteLine(ec.EncryptionSystem(JsonUtility.ToJson(SD),true));  //debug

                        //**--------------------------------------------------------------------------------------------------------------------------------------------------------------------
                        sw.Flush();
                        sw.Close();
                        fs.Close();
                        byte[] bytes = picture.EncodeToPNG();
                        File.WriteAllBytes(Application.streamingAssetsPath + "/SaveData/" + tmp.gameObject.name + ".png", bytes);
                        screen = SCREEN_STATUS.MAIN;
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
    //ロード関数
    public void Main_Load()
    {

        if (EventSystem.current == null) return;
        if (Input.GetMouseButtonDown(1))
        {
            screen = SCREEN_STATUS.MAIN;
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
                            string source_file = ec.DecryptionSystem(sr.ReadToEnd(),true); //debug
;
                            //**--------------------------------------------------------------------------------------------------------------------------------------------------------------------
                            NovelSave sd = JsonUtility.FromJson<NovelSave>(source_file);
                            sd.Load(); //パーティメンバーのロード
                            nextscene = sd.nameScene;

                            FileStream fs2 = new FileStream(Application.streamingAssetsPath + "/SaveData/loadtemp",FileMode.Create,FileAccess.Write);
                            StreamWriter sw = new StreamWriter(fs2);
                            sw.WriteLine(source_file);
                            sw.Flush();
                            fs2.Flush();
                            sw.Close();
                            fs2.Close();
                            //reading_pos = sd.reading_pos;
                            PageChangeAnim.gameObject.SetActive(false);
                            PageWaitAnim.gameObject.SetActive(false);
                            sr.Close();
                            fs.Close();
                            state = Next_Command();
                            //back_log.text = sd.backlog;
                            screen = SCREEN_STATUS.FADE_OUT;
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
    static bool coroutine_check = false;
    //スクリーンショットを撮る関数
    IEnumerator LoadScreenshot()
    {
        if (coroutine_check) yield break;
        coroutine_check = true;
        yield return new WaitForEndOfFrame();
        picture = new Texture2D(Screen.width, Screen.height);
        picture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        picture.Apply();
    }
    //セーブボタンが押されたときの関数
    public void SaveButton()
    {
        if (screen == SCREEN_STATUS.BACK_LOG) return;
        GameObject temp_object = (GameObject)Resources.Load("Prefab/SaveCanvas");
        save_canvas = Instantiate<Canvas>(temp_object.GetComponent<Canvas>());
        savemanager = save_canvas.GetComponent<SaveManager>();
        savemanager.SaveWindowOpen();
        screen = SCREEN_STATUS.SAVE;
    }
    //ロードボタンが押されたときの関数
    public void LoadButton()
    {
        if (screen == SCREEN_STATUS.BACK_LOG) return;
        GameObject temp_object = (GameObject)Resources.Load("Prefab/SaveCanvas");
        save_canvas = Instantiate<Canvas>(temp_object.GetComponent<Canvas>());
        savemanager = save_canvas.GetComponent<SaveManager>();
        savemanager.LoadWindowOpen();
        screen = SCREEN_STATUS.LOAD;
    }
    //一桁の値を渡すと、その値を10進数に変換させる
    //異なる値を入力すると0として表示する。
    static int Hex_to_dec(char s)
    {
        if ('a' <= s && s <= 'f')
        {
            return 10 + (int)(s - 'a');
        }
        if ('0' <= s && s <= '9')
        {
            return (int)(s - '0');
        }
        return 0;
    }
    public static Color Color_Parser(string str)
    {
        float r, g, b;
        int addition = 0;
        if (str[0] == '#') addition = 1;
        str = str.ToLower();
        r = (Hex_to_dec(str[0 + addition]) * 16 + Hex_to_dec(str[1 + addition]))/255f;
        g = (Hex_to_dec(str[2 + addition]) * 16 + Hex_to_dec(str[3 + addition]))/255f;
        b = (Hex_to_dec(str[4 + addition]) * 16 + Hex_to_dec(str[5 + addition]))/255f;
        return new Color(r, g, b);
    }

    // Use this for initialization
    void Start () {
        //フロントスクリーン・バックスクリーンのロード
        FrontScreen = new NovelScreen(frontscreen_inspector);
        BackScreen = new NovelScreen(backscreen_inspector);

        FrontScreen.gobj.SetActive(true);
        BackScreen.gobj.SetActive(false);

        if(skeltonsprite == null)
            skeltonsprite = Resources.Load<Sprite>("stand/skelton");

        //ノベルスクリプトの取得
        if (Debug.isDebugBuild)
        {
            scriptpath = "/debug/" + scriptpath;
        }
        else
        {
            scriptpath = "/compiled/" + scriptpath;
        }
        StreamReader sr = new StreamReader(Application.streamingAssetsPath + scriptpath,System.Text.Encoding.GetEncoding("utf-8"));
        text.color = chara_color = Color_Parser("000000");
        characterText.color = Color_Parser("000000");
        namebox.gameObject.SetActive(false);
        PageChangeAnim.gameObject.SetActive(false);
        PageWaitAnim.gameObject.SetActive(false);
        back_log.text = "";
        back_log_Stage.gameObject.SetActive(false);
        anim_timer = 0;

        Debug.Log("Scripter Start Call");
        //**注意**--------------------------------------------------------------------------------------------------------------------------------------------------------------
        Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
        string temp;
        while(sr.Peek() != -1)
        {
            string t = sr.ReadLine();
            temp = ec.DecryptionSystem(t,false);
            Scription test = JsonUtility.FromJson<Scription>(temp);
            switch (test.type)
            {
                case COMMAND_TYPE.AUDIO_BGM:
                case COMMAND_TYPE.AUDIO_VOICE:
                case COMMAND_TYPE.AUDIO_SE:
                    {
                        AUDIO_scription audio = JsonUtility.FromJson<AUDIO_scription>(temp);
                        scriptions.Add(new AUDIO_scription(audio,Resources.Load<AudioClip>(audio.audio_name),audio.text,audio.type));
                    }
                    break;
                case COMMAND_TYPE.BACKGROUND:
                case COMMAND_TYPE.BACKGROUND_IMAGE:
                    {
                        IMAGE_scription image = JsonUtility.FromJson<IMAGE_scription>(temp);
                        scriptions.Add(new IMAGE_scription(image,Resources.Load<Sprite>(image.imagename),image.text,image.type));
                    }
                    break;
                case COMMAND_TYPE.STAND_IMAGE:
                    {
                        STAND_IMAGE_scription image = JsonUtility.FromJson<STAND_IMAGE_scription>(temp);
                        scriptions.Add(new STAND_IMAGE_scription(image.place, image, Resources.Load<Sprite>(image.imagename), image.text, image.type));
                    }
                    break;
                case COMMAND_TYPE.COLOR:
                case COMMAND_TYPE.BACKGROUND_COLOR:
                case COMMAND_TYPE.MONOCRO_ON:
                    scriptions.Add(JsonUtility.FromJson<COLOR_scription>(temp));
                    break;
                case COMMAND_TYPE.NORMAL_TEXT:
                case COMMAND_TYPE.NORMAL_TEXT_CONTINUE:
                    scriptions.Add(JsonUtility.FromJson<Text_scription>(temp));
                    break;
                case COMMAND_TYPE.WAIT_B:
                case COMMAND_TYPE.WAIT_S:
                case COMMAND_TYPE.WAIT_W:
                case COMMAND_TYPE.WAIT:
                    scriptions.Add(JsonUtility.FromJson<WAIT_scription>(temp));
                    break;
                case COMMAND_TYPE.STAND_IMAGE_CLEAR:
                    scriptions.Add(JsonUtility.FromJson<IMAGE_clear_scription>(temp));
                    break;
                case COMMAND_TYPE.AUDIO_BGM_STOP:
                case COMMAND_TYPE.AUDIO_SE_STOP:
                case COMMAND_TYPE.AUDIO_VOICE_STOP:
                case COMMAND_TYPE.TEXTOFF:
                case COMMAND_TYPE.TEXTON:
                case COMMAND_TYPE.MONOCRO_OFF:
                    scriptions.Add(JsonUtility.FromJson<Scription>(temp));
                    break;
                case COMMAND_TYPE.SETWINDOW:
                    scriptions.Add(JsonUtility.FromJson<UI_IMAGE_scription>(temp));
                    break;
                case COMMAND_TYPE.PARTY_ADD:
                case COMMAND_TYPE.PARTY_REMOVE:
                    scriptions.Add(JsonUtility.FromJson<PARTY_scription>(temp));
                    break;

            }
            
        }
        sr.Close();
        if (System.IO.File.Exists(Application.streamingAssetsPath + "/SaveData/loadtemp"))
        {
            FileStream fs = new FileStream(Application.streamingAssetsPath + "/SaveData/loadtemp",FileMode.Open, FileAccess.Read);
            sr = new StreamReader(fs, Encoding.UTF8);
            string temp_text = sr.ReadToEnd();
            NovelSave sd = JsonUtility.FromJson<NovelSave>(temp_text);
            reading_pos = sd.reading_pos;

            if (sd.left_color != null)
            {
                Debug.Log(sd.left_name);
                BackScreen.left.sprite = Resources.Load<Sprite>(sd.left_name);
                if (BackScreen.left.sprite == null) BackScreen.left.sprite = skeltonsprite;
                BackScreen.left.color = sd.left_color;
                BackScreen.left.name = sd.left_name;
            }

            if (sd.right_name != null)
            {
                BackScreen.right.sprite = Resources.Load<Sprite>(sd.right_name);
                if (BackScreen.right.sprite == null) BackScreen.right.sprite = skeltonsprite;
                BackScreen.right.color = sd.right_color;
                BackScreen.right.name = sd.right_name;
            }

            if (sd.center_name != null)
            {
                BackScreen.center.sprite = Resources.Load<Sprite>(sd.center_name);
                if (BackScreen.center.sprite == null) BackScreen.center.sprite = skeltonsprite;
                BackScreen.center.color = sd.center_color;
                BackScreen.center.name = sd.center_name;
            }

            if (sd.background_name != null)
            {
                BackScreen.background.sprite = Resources.Load<Sprite>(sd.background_name);
                if (BackScreen.background.sprite == null) BackScreen.background.sprite = skeltonsprite;
                BackScreen.background.color = sd.background_color;
                BackScreen.background.name = sd.background_name;
            }
            back_log.text = sd.backlog;
            if(sd.bgm_name != null)
            {
                bgm.clip = Resources.Load<AudioClip>("bgm/" + sd.bgm_name);
                if (bgm.clip != null && sd.bgmflag)
                {
                    bgm.loop = true;
                    bgm.Play();
                }
            }
            if(sd.charactervoice_name != null)
            {
                character_voice.clip = Resources.Load<AudioClip>(sd.charactervoice_name);
                if (character_voice.clip != null) character_voice.Play();
            }
            sr.Close();
            System.IO.File.Delete(Application.streamingAssetsPath + "/SaveData/loadtemp");
            now = scriptions[reading_pos];
        }
        else
        {
            reading_pos = 0;
        }
    }

    string log_text = "";
    //次のスクリプトを読み込む関数
    NOVEL_STATUS Next_Command()
    {
        if (scriptions.Count <= reading_pos)
        {
            SceneManager.LoadScene(next_stage[0]);
        }
        else
        {
            if (reading_pos > 0 && scriptions[reading_pos].type != COMMAND_TYPE.NORMAL_TEXT_CONTINUE)
            {
                if (now.type == COMMAND_TYPE.NORMAL_TEXT || now.type == COMMAND_TYPE.NORMAL_TEXT_CONTINUE)
                {
                    string check ="";
                    if ((now as Text_scription).character_name.Length > 0)
                    {
                        check += "[" + (now as Text_scription).character_name + "]";
                    }
                    else
                    {
                        if (characterText.text.Length > 0)
                        {
                            check += "[" + characterText.text + "]";
                        }
                        else
                        {
                            check += ' ';
                        }
                    }
                    check += log_text;
                    log_text = "";
                    Debug.Log(check);
                    string[] temp = check.Split('\\');
                    check = temp[0];
                    for(int i = 1; i < temp.Length; i++)
                    {
                        if( i % 2 == 0)
                        {
                            check += temp[i].Substring(3);
                        }
                        else
                        {
                            string[] before_slash = temp[i].Split('/');
                            string[] contents = before_slash[0].Split('>');
                            int num = contents.Length - 1;
                            check += '(' +contents[num].Substring(0, contents[num].Length - 2) + ')';
                        }
                    }
                    
                    Debug.Log(check);
                    back_log.text += check;
                    back_log.text += "\n\n";
                }
            }

            //コマンド読み込み
            now = scriptions[reading_pos];
            reading_pos++;
            switch (now.type)
            {
                //文字速度変更コマンド
                case COMMAND_TYPE.WAIT_S:
                    wait_time = (now as WAIT_scription).counter;
                    return NOVEL_STATUS.NEXT;
                // delay wait の遅延コマンド
                case COMMAND_TYPE.WAIT:
                    delaytime = (now as WAIT_scription).counter;
                    delaynowtime = 0;
                    skipflag = (now as WAIT_scription).skipflag;
                    return NOVEL_STATUS.DELAY;
                //通常テキスト
                case COMMAND_TYPE.NORMAL_TEXT:
                    {
                        characterText.text = (now as Text_scription).character_name;
                        if (characterText.text.Length > 0) namebox.gameObject.SetActive(true);
                        else namebox.gameObject.SetActive(false);
                    }
                    save_data = "";
                    text.text = "";
                    character_count = 0;
                    line_count = 0;
                    text.color = chara_color;
                    (now as Text_scription).counter = 0;
                    (now as Text_scription).timer = 0f;
                    if ((now as Text_scription).check)
                    {
                        character_voice.Play();
                    }
                    return NOVEL_STATUS.WRITING;
                case COMMAND_TYPE.NORMAL_TEXT_CONTINUE:
                    (now as Text_scription).counter = 0;
                    (now as Text_scription).timer = 0f;
                    return NOVEL_STATUS.WRITING;
                case COMMAND_TYPE.COLOR:
                    {
                        chara_color = (now as COLOR_scription).color;
                    }
                    return NOVEL_STATUS.NEXT;
                
                //bg color系命令
                case COMMAND_TYPE.BACKGROUND_COLOR:
                    {

                        //Texture2DからSpriteを作成
                        Sprite branksprite = Sprite.Create(
                          texture: Texture2D.whiteTexture,
                          rect: new Rect(0, 0, Texture2D.whiteTexture.width, Texture2D.whiteTexture.height),
                          pivot: new Vector2(0.5f, 0.5f)
                        );
                        BackScreen.background.sprite = null;
                        //BackScreen.background.sprite = branksprite;
                        BackScreen.background.sprite = Resources.Load<Sprite>("bg/white");
                        BackScreen.background.color = (now as COLOR_scription).color;

                        //アニメーション設定
                        registered_animation = (now as COLOR_scription).animation;
                        if (registered_animation == ANIMATION_TYPE.MASKRULE)
                            animrulenum = Int32.Parse((now as Scription).text);

                        //次同時描画エフェクト（エフェクト番号0番）
                        //0秒は廃止にします。（チェンジは一括にするため）
                        //仕様としては、BackScreenに登録だけして、NEXTへいく
                        if (registered_animation == ANIMATION_TYPE.ANIMATION_SYNCHRO)
                            return NOVEL_STATUS.NEXT;


                        //タイマー設定
                        if (registered_animation != ANIMATION_TYPE.ANIMATION_NO_TIME)
                        {
                            anim_timer = (now as COLOR_scription).timmer;
                        }
                        else
                        {
                            //エフェクト番号1番
                            anim_timer = -1;
                        }
                    }
                    //アニメーション準備へ
                    return NOVEL_STATUS.PRE_ANIMATION;

                //cl命令
                case COMMAND_TYPE.STAND_IMAGE_CLEAR:
                    {
                        //BackScreenに描画
                        int place = (now as IMAGE_clear_scription).place;
                        switch (place)
                        {
                            case 1:  //001
                                BackScreen.right.sprite = skeltonsprite;
                                BackScreen.right.name = null;
                                break;
                            case 2:  //010
                                BackScreen.center.sprite = skeltonsprite;
                                BackScreen.center.name = null;
                                break;
                            case 4:  //100
                                BackScreen.left.sprite = skeltonsprite;
                                BackScreen.left.name = null;
                                break;
                            case 7:  //111
                                BackScreen.right.sprite = skeltonsprite;
                                BackScreen.center.sprite = skeltonsprite;
                                BackScreen.left.sprite = skeltonsprite;
                                BackScreen.right.name = null;
                                BackScreen.center.name = null;
                                BackScreen.left.name = null;
                                break;
                        }



                        //アニメーション設定
                        registered_animation = (now as IMAGE_clear_scription).animation;
                        if (registered_animation == ANIMATION_TYPE.MASKRULE)
                            animrulenum = Int32.Parse((now as Scription).text);

                        //次同時描画エフェクト（エフェクト番号0番）
                        //0秒は廃止にします。（チェンジは一括にするため）
                        //仕様としては、BackScreenに登録だけして、NEXTへいく
                        if (registered_animation == ANIMATION_TYPE.ANIMATION_SYNCHRO)
                            return NOVEL_STATUS.NEXT;


                        //タイマー設定
                        if (registered_animation != ANIMATION_TYPE.ANIMATION_NO_TIME)
                        {
                            anim_timer = (now as IMAGE_clear_scription).timmer;
                        }
                        else
                        {
                            //エフェクト番号1番
                            anim_timer = -1;
                        }
                    }
                    //アニメーション準備へ
                    return NOVEL_STATUS.PRE_ANIMATION;
                //bg命令
                case COMMAND_TYPE.BACKGROUND_IMAGE:
                    {
                        //裏画面に描画
                        BackScreen.background.name = (now as IMAGE_scription).imagename;
                        BackScreen.background.sprite = null;
                        BackScreen.background.sprite = (now as IMAGE_scription).image;
                        BackScreen.background.color = new Color(1, 1, 1, 1);

                        //アニメーション設定
                        registered_animation = (now as IMAGE_scription).animation;
                        if (registered_animation == ANIMATION_TYPE.MASKRULE)
                            animrulenum = Int32.Parse((now as Scription).text);

                        //次同時描画エフェクト（エフェクト番号0番）
                        //0秒は廃止にします。（チェンジは一括にするため）
                        //仕様としては、BackScreenに登録だけして、NEXTへいく
                        if (registered_animation == ANIMATION_TYPE.ANIMATION_SYNCHRO)
                            return NOVEL_STATUS.NEXT;


                        //タイマー設定
                        if (registered_animation != ANIMATION_TYPE.ANIMATION_NO_TIME)
                        {
                            anim_timer = (now as IMAGE_scription).timmer;
                        }
                        else
                        {
                            //エフェクト番号1番
                            anim_timer = -1;
                        }

                    }

                    //アニメーション準備へ
                    return NOVEL_STATUS.PRE_ANIMATION;
                //ld命令
                case COMMAND_TYPE.STAND_IMAGE:
                    {
                        //BackScreenに描画
                        //l,c,rで分岐
                        switch((now as STAND_IMAGE_scription).place)
                        {
                            //left  //100
                            case 4:
                                BackScreen.left.sprite = (now as IMAGE_scription).image;
                                BackScreen.left.preserveAspect = true;
                                BackScreen.left.name = (now as IMAGE_scription).imagename;
                                break;

                            //center  //010
                            case 2:
                                BackScreen.center.sprite = (now as IMAGE_scription).image;
                                BackScreen.center.preserveAspect = true;
                                BackScreen.center.name = (now as IMAGE_scription).imagename;
                                break;

                            //right   //001
                            case 1:
                                BackScreen.right.sprite = (now as IMAGE_scription).image;
                                BackScreen.right.preserveAspect = true;
                                BackScreen.right.name = (now as IMAGE_scription).imagename;
                                break;
                        }
                        
                        //アニメーション設定
                        registered_animation = (now as IMAGE_scription).animation;
                        if (registered_animation == ANIMATION_TYPE.MASKRULE)
                            animrulenum = Int32.Parse((now as Scription).text);

                        //次同時描画エフェクト（エフェクト番号0番）
                        //0秒は廃止にします。（チェンジは一括にするため）
                        if (registered_animation == ANIMATION_TYPE.ANIMATION_SYNCHRO)
                            return NOVEL_STATUS.NEXT;

                        //タイマー設定
                        if (registered_animation != ANIMATION_TYPE.ANIMATION_NO_TIME)
                        {
                            anim_timer = (now as IMAGE_scription).timmer;
                        }
                        else
                        {
                            //エフェクト番号1番
                            anim_timer = -1;
                        }

                    }
                    return NOVEL_STATUS.PRE_ANIMATION;
                //textoff命令
                case COMMAND_TYPE.TEXTOFF:
                    //後でアニメーションとかする場合ここにその準備を追加。

                    return NOVEL_STATUS.UIHIDE;
                case COMMAND_TYPE.TEXTON:
                    //後でアニメーションとかする場合ここにその準備を追加。

                    return NOVEL_STATUS.UISHOW;
                //bgm命令
                case COMMAND_TYPE.AUDIO_BGM:
                    bgm.enabled = true;
                    bgm.clip = (now as AUDIO_scription).audio;
                    bgm.loop = true;
                    bgm.Play();

                    return NOVEL_STATUS.NEXT;
                //bgmstop命令
                case COMMAND_TYPE.AUDIO_BGM_STOP:
                    bgm.Stop();
                    bgm.enabled = false;

                    return NOVEL_STATUS.NEXT;
                //se命令
                case COMMAND_TYPE.AUDIO_SE:
                    se.enabled = true;
                    se.clip = (now as AUDIO_scription).audio;
                    if(now.text == "loop")
                        se.loop = true;
                    else
                        se.loop = false;
                    se.Play();

                    return NOVEL_STATUS.NEXT;
                //sestop命令
                case COMMAND_TYPE.AUDIO_SE_STOP:
                    se.Stop();
                    se.enabled = false;

                    return NOVEL_STATUS.NEXT;
                //タグ中のボイスを再生する、またはvoice命令
                case COMMAND_TYPE.AUDIO_VOICE:
                    character_voice.enabled = true;
                    character_voice.clip = (now as AUDIO_scription).audio;
                    character_voice.clip.name = (now as AUDIO_scription).audio_name;
                    character_voice.loop = false;
                    //voice命令の場合
                    if ((now as AUDIO_scription).text == "voice")
                    {
                        character_voice.Play();
                        isvoicestop = false;
                    }
                    return NOVEL_STATUS.NEXT;
                //voicestop命令
                //voice停止と共に、次の行を読むときにボイスを止めるフラグを復活させる意味がある
                case COMMAND_TYPE.AUDIO_VOICE_STOP:
                    character_voice.Stop();
                    character_voice.enabled = false;
                    isvoicestop = true;
                    return NOVEL_STATUS.NEXT;
                //setwindow命令
                case COMMAND_TYPE.SETWINDOW:
                    //テキストボックスを取得
                    GameObject textbox = mainui.transform.Find("TextBox").gameObject;
                    //画像を更新
                    textbox.GetComponent<Image>().sprite = Resources.Load<Sprite>((now as UI_IMAGE_scription).text);
                    //位置を更新
                    textbox.transform.localPosition = new Vector3((now as UI_IMAGE_scription).x, (now as UI_IMAGE_scription).y);

                    //TODO:セーブデータに変更を記録する
                    return NOVEL_STATUS.NEXT;
                //monocro系
                case COMMAND_TYPE.MONOCRO_ON:
                    float a = BackScreen.monocromask.color.a;
                    BackScreen.monocromask.color = new Color((now as COLOR_scription).color.r, (now as COLOR_scription).color.g, (now as COLOR_scription).color.b, a);
                    BackScreen.monocromask.gameObject.SetActive(true);
                    return NOVEL_STATUS.NEXT;
                case COMMAND_TYPE.MONOCRO_OFF:
                    BackScreen.monocromask.gameObject.SetActive(false);
                    return NOVEL_STATUS.NEXT;
                //partyadd命令
                case COMMAND_TYPE.PARTY_ADD:
                    GameVal.masterSave.UnitAdd(new UnitSaveData((now as PARTY_scription).scobjname), (now as PARTY_scription).unitlevel);
                    return NOVEL_STATUS.NEXT;
                //partyremove命令
                case COMMAND_TYPE.PARTY_REMOVE:
                    GameVal.masterSave.UnitRemove(new UnitSaveData((now as PARTY_scription).scobjname));
                    return NOVEL_STATUS.NEXT;
            }
        }
        Debug.Log(now.text);
        return NOVEL_STATUS.WRITING;
    }
    string save_data = "";
    //クリック・スペースキーが押され、次のシーンに続くときの処理
    bool Operation_next()
    {
        if (Input.GetKeyDown(KeyCode.Space) || (Input.GetMouseButtonDown(0) && !Scripter_ClickChecker.button_over ) || skip_button)
        {
            StartCoroutine(LoadScreenshot());
            if (state == NOVEL_STATUS.ANIMATION || state == NOVEL_STATUS.DELAY) return true;
            state = NOVEL_STATUS.NEXT;
            int max = now.text.Length;
            log_text += now.text;
            save_data += now.text;
 
            for (int i = (now as Text_scription).counter; i < max; i++)
            {
                if (now.text[i] == '\\' && now.text[i + 1] == 't' && now.text[i + 2] == 'a' && now.text[i + 3] == 'g')
                {
                    string cut = now.text.Substring(i + 4);
                    int main_text = Int32.Parse(cut.Substring(0, 1));                    
                    cut = cut.Substring(1, cut.Length - 1);
                    string[] temp = cut.Split('\\');
                    text.text += temp[0];
                    i += temp[0].Length + 8;
                }
                else
                {
                    character_count++;
                    text.text += now.text[i];
                }
            }
            return true;
        }
        return false;
    }
    //バックログを表示する関数<入力処理もこちらで行う>
    void Operation_BackLog()
    {
        if (screen == SCREEN_STATUS.LOAD || screen == SCREEN_STATUS.SAVE) return;
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetMouseButtonDown(1))
        {
            if (screen != SCREEN_STATUS.BACK_LOG)
            {
                back_log.rectTransform.sizeDelta = new Vector2(back_log.rectTransform.sizeDelta.x, text.preferredHeight);
                back_log.rectTransform.sizeDelta = new Vector2(back_log.rectTransform.sizeDelta.x, text.preferredHeight);
                back_log_Stage.gameObject.SetActive(true);
                VScroll.verticalNormalizedPosition = 0;
                TextBoxSize_Change.change_active = false;
                back_log.gameObject.transform.localPosition = new Vector3(0, 0, 0);
                
                screen = SCREEN_STATUS.BACK_LOG;
            }
            else
            {
                back_log_Stage.gameObject.SetActive(false);
                screen = SCREEN_STATUS.MAIN;
            }
        }
    }
    // Update is called once per frame
    void Update () {

        //バックログ処理
        Operation_BackLog();
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            skip_button = true;
        }
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            skip_button = false;
        }
        //SCREEN_STATUS分岐（バックログか、ノベルエンジンメインか、セーブ・ロードか。）
        switch(screen)
        {
            case SCREEN_STATUS.BACK_LOG:

                break;

            case SCREEN_STATUS.MAIN:
                StartCoroutine(LoadScreenshot());
                //次の行へ
                if (state == NOVEL_STATUS.NEXT)
                {
                    ResetFrontAlpha();

                    if (character_voice.isPlaying && isvoicestop)
                    {
                        character_voice.Stop();
                    }
                    Debug.Log("Scripter Update Call");

                    state = Next_Command();
                }
                //文章表示中
                else if (state == NOVEL_STATUS.WRITING)
                {
                    float time = (now as Text_scription).timer;
                    //文章表示時は強制的にtexton
                    ShowUI();
                    //強制的に0（Sync）を表示（NScripter準拠）BackScreenをFrontScreenにコピー
                    FrontScreen.Copy(BackScreen);

                    time += Time.deltaTime * 1000;
                    if (time > wait_time)
                    {
                        int count = 0;
                        count = (now as Text_scription).counter;
                        if (now.text.Length > count)
                        {
                            if (now.text[count] == '\\' && now.text[count + 1] == 't' && now.text[count + 2] == 'a' && now.text[count + 3] == 'g')
                            {
                                string cut = now.text.Substring(count + 4);
                                int main_text = Int32.Parse(cut.Substring(0, 1));
                                cut = cut.Substring(1, cut.Length - 1);
                                string[] temp = cut.Split('\\');
                                text.text += temp[0];
                                count += temp[0].Length + 8;
                            }
                            else
                            {
                                character_count++;
                                text.text += now.text[count];
                            }
                            (now as Text_scription).counter = count + 1;
                            (now as Text_scription).timer = 0f;
                            time = 0f;
                        }
                        else
                        {
                            
                            coroutine_check = false;
                            state = NOVEL_STATUS.WAITING;
                        }
                    }
                    if (check_key)
                    {
                        Operation_next();
                    }
                    (now as Text_scription).timer = time;

                }
                //クリック待ち
                else if (state == NOVEL_STATUS.WAITING)
                {
                    if(!character_voice.isPlaying)
                        PageChangeAnim.gameObject.SetActive(true);

                    if (check_key)
                    {
                        if (Operation_next())
                        {
                            PageChangeAnim.gameObject.SetActive(false);
                            PageWaitAnim.gameObject.SetActive(false);
                        }
                    }
                }
                //遅延
                else if(state == NOVEL_STATUS.DELAY)
                {
                    if ((Operation_next() && skipflag) || delaynowtime >= delaytime)
                    {
                        state = NOVEL_STATUS.NEXT;
                    }
                    delaynowtime += (int)(Time.deltaTime * 1000);
                }
                else if(state == NOVEL_STATUS.UIHIDE)
                {
                    //今は非表示にするだけ。
                    HideUI();
                    state = NOVEL_STATUS.NEXT;
                }
                else if(state == NOVEL_STATUS.UISHOW)
                {
                    //今は表示するだけ。
                    ShowUI();
                    state = NOVEL_STATUS.NEXT;
                }
                //アニメーション
                else if (state == NOVEL_STATUS.ANIMATION)
                {
                    //クリックされた
                    if (Operation_next())
                    {
                        FrontScreen.Copy(BackScreen);
                        
                        state = NOVEL_STATUS.NEXT;
                    }
                    else
                    {
                        bool check = false;
                        switch (registered_animation)
                        {
                            case ANIMATION_TYPE.ANIMATION_NO_TIME:
                                break;
                            case ANIMATION_TYPE.ANIMATION_SYNCHRO:
                                Debug.Log("Error;");
                                break;
                            case ANIMATION_TYPE.FADE_IN:
                                check = true;
                                FrontScreen.gobj.GetComponent<CanvasGroup>().alpha -= 1000 * Time.deltaTime / anim_timer;
                                //registered_object[i].color -= new Color(0, 0, 0, 1000 * Time.deltaTime / anim_timer);
                                if (FrontScreen.gobj.GetComponent<CanvasGroup>().alpha <= 0f)
                                {
                                    registered_animation = ANIMATION_TYPE.ANIMATION_NO_TIME;
                                }
                                break;

                            case ANIMATION_TYPE.MASKRULE:
                                check = true;
                                fadeui.GetComponent<FadeUI>().Range += 1000 * Time.deltaTime / anim_timer;
                                if (fadeui.GetComponent<FadeUI>().Range >= 1)
                                {
                                    //prebackground.enabled = false;
                                    fadeui.GetComponent<FadeUI>().Range = 1;
                                    registered_animation = ANIMATION_TYPE.ANIMATION_NO_TIME;
                                }
                                break;
                        }
                        if (!check)
                        {
                            FrontScreen.Copy(BackScreen);

                            ResetFrontAlpha();
                            
                            BackScreen.gobj.SetActive(false);
                            state = NOVEL_STATUS.NEXT;
                        }
                    }
                }
                else if (state == NOVEL_STATUS.PRE_ANIMATION)
                {
                    //BackScreenをSetActive
                    BackScreen.gobj.SetActive(true);
                    BackScreen.gobj.GetComponent<CanvasGroup>().alpha = 1;
                    switch (registered_animation)
                    {
                        //フェードイン 10
                        case ANIMATION_TYPE.FADE_IN:
                            FrontScreen.gobj.GetComponent<CanvasGroup>().alpha = 1;
                            state = NOVEL_STATUS.ANIMATION;
                            break;
                        //瞬間表示 1
                        case ANIMATION_TYPE.ANIMATION_NO_TIME:
                            //BackScreenをFrontScreenにコピー
                            FrontScreen.Copy(BackScreen);
                            BackScreen.gobj.SetActive(false);
                            state = NOVEL_STATUS.NEXT;

                            anim_timer = 0;
                            break;
                        //ルール画像（0,1,10以外）
                        case ANIMATION_TYPE.MASKRULE:
                            state = NOVEL_STATUS.ANIMATION;
                            //フェードマスク用ルール画像の設定
                            Texture animation_mask_texture = Resources.Load<Texture>(string.Format("Fade/{0}", animrulenum));
                            fadeui.GetComponent<FadeUI>().UpdateMaskTexture(animation_mask_texture);
                            fadeui.GetComponent<FadeUI>().Range = 0;
                            fade_method = 0;
                            break;

                    }
                }
                break;

            case SCREEN_STATUS.SAVE:
                Main_Save();
                break;

            case SCREEN_STATUS.LOAD:
                Main_Load();
                break;

            case SCREEN_STATUS.FADE_OUT:
                switch (draw_scenechange_state)
                {
                    case DRAW_SCENECHANGE_STATE.FADEOUTSTART:
                        fademask.FadeIn(timefadeout, () => {
                            GameVal.nextscenename = nextscene;
                            SceneManager.LoadScene("BlackScene", LoadSceneMode.Single);
                            Resources.UnloadUnusedAssets();
                        });
                        draw_scenechange_state = DRAW_SCENECHANGE_STATE.NEXT;
                        break;
                    case DRAW_SCENECHANGE_STATE.NEXT:

                        break;
                }
                
                break;
        }

	}

    //FrontScreenの透過度のリセット
    public void ResetFrontAlpha()
    {
        FrontScreen.gobj.GetComponent<CanvasGroup>().alpha = 1;
        fadeui.GetComponent<FadeUI>().Range = 0;
    }

    //MainUI（テキストボックスとかボタン）を非表示にする処理
    public void HideUI()
    {
        mainui.SetActive(false);
    }
    //MainUIを表示にする処理
    public void ShowUI()
    {
        mainui.SetActive(true);
    }
}
