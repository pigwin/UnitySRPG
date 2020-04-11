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
/*Novel ノベル処理　　　　　　　　　　　　　　　　　　　　　*/
/*  バトルパート用のノベル処理を行うクラス　　　　　　　　　*/
/*  基本的な処理は、Scripterとほぼ変わらない　　　　　　　　*/
/************************************************************/
//戦闘パートのイベント処理
public class Light_Novel_Scripter : MonoBehaviour
{
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
    //キャンバスの大きさを取得するためのオブジェクト(対処法は別に考えるべき)
    public Canvas canvas;

    //フェード処理をするときのオブジェクト
    //public FadeImage fade;
    private int fade_method = 0;
    public GameObject fadeui;
    //フェードマスクのルール画像リスト
    Dictionary<ANIMATION_TYPE, Texture> animation_mask_texture;


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
    //次の行を読むときに音声消去をするかどうかのフラグ
    private bool isvoicestop = true;

    //delay wait 命令
    private int delaytime;
    private int delaynowtime;
    private bool skipflag;

    //フロントスクリーン・バックスクリーンのゲームオブジェクト
    public GameObject frontscreen_inspector;
    public GameObject backscreen_inspector;
    public Sprite skeltonsprite;
    //イベント時の背景（初回呼び出し時のスクリーンショット）
    private Texture2D eventInitBackgroud;
    private bool screenshot_coroutine = false;

    [HideInInspector]
    public NovelScreen FrontScreen;
    [HideInInspector]
    public NovelScreen BackScreen;


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
    //ルビに埋め込まれているコマンドを削除する関数
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
    //色情報を16進数から0~1までの実数で表す関数
    public static Color Color_Parser(string str)
    {
        float r, g, b;
        int addition = 0;
        if (str[0] == '#') addition = 1;
        str = str.ToLower();
        r = (Hex_to_dec(str[0 + addition]) * 16 + Hex_to_dec(str[1 + addition])) / 255f;
        g = (Hex_to_dec(str[2 + addition]) * 16 + Hex_to_dec(str[3 + addition])) / 255f;
        b = (Hex_to_dec(str[4 + addition]) * 16 + Hex_to_dec(str[5 + addition])) / 255f;
        return new Color(r, g, b);
    }
    public void start_Novel(string novel_name, int pos)
    {

        //スクリーンショット取得
        StartCoroutine(LoadScreenshot());
        //フロントスクリーン・バックスクリーンのロード
        FrontScreen = new NovelScreen(frontscreen_inspector);
        BackScreen = new NovelScreen(backscreen_inspector);
        FrontScreen.gobj.SetActive(true);
        BackScreen.gobj.SetActive(false);

        if (skeltonsprite == null)
            skeltonsprite = Resources.Load<Sprite>("stand/skelton");

        //ノベルスクリプトの取得
        string path = "";
        if (Debug.isDebugBuild)
        {
            path += "/debug/";
        }
        else
        {
            path += "/compiled/";
        }
        StreamReader sr = new StreamReader(Application.streamingAssetsPath + path + novel_name, System.Text.Encoding.GetEncoding("utf-8"));
        text.color = chara_color = Color_Parser("000000");
        characterText.color = Color_Parser("000000");
        namebox.gameObject.SetActive(false);
        PageChangeAnim.gameObject.SetActive(false);
        PageWaitAnim.gameObject.SetActive(false);
        back_log_Stage.gameObject.SetActive(false);
        back_log.text = "";
        anim_timer = 0;

        ////Debug.Log("Scripter Start Call");

        string temp;
        Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
        int count = 0;
        while (sr.Peek() != -1)
        {
            temp = ec.DecryptionSystem(sr.ReadLine(),false);
            count++;
            Scription test = JsonUtility.FromJson<Scription>(temp);
            switch (test.type)
            {
                case COMMAND_TYPE.AUDIO_BGM:
                case COMMAND_TYPE.AUDIO_VOICE:
                case COMMAND_TYPE.AUDIO_SE:
                    {
                        AUDIO_scription audio = JsonUtility.FromJson<AUDIO_scription>(temp);
                        scriptions.Add(new AUDIO_scription(audio, Resources.Load<AudioClip>(audio.audio_name), audio.text, audio.type));
                    }
                    break;
                case COMMAND_TYPE.BACKGROUND:
                case COMMAND_TYPE.BACKGROUND_IMAGE:
                    {
                        IMAGE_scription image = JsonUtility.FromJson<IMAGE_scription>(temp);
                        scriptions.Add(new IMAGE_scription(image, Resources.Load<Sprite>(image.imagename), image.text, image.type));
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
                    scriptions.Add(JsonUtility.FromJson<Scription>(temp));
                    break;
                case COMMAND_TYPE.SETWINDOW:
                    scriptions.Add(JsonUtility.FromJson<UI_IMAGE_scription>(temp));
                    break;
                case COMMAND_TYPE.RETURN:
                    scriptions.Add(JsonUtility.FromJson<Scription>(temp));
                    break;
            }

        }
        reading_pos = pos;
        now = scriptions[reading_pos];
        state = NOVEL_STATUS.NEXT;
    }
    string log_text = "";
    //次のスクリプトを読み込む関数
    NOVEL_STATUS Next_Command()
    {
        if (scriptions.Count <= reading_pos)
        {
            return NOVEL_STATUS.FINISH;
        }
        else
        {
            if (reading_pos > 0 && scriptions[reading_pos].type != COMMAND_TYPE.NORMAL_TEXT_CONTINUE)
            {
                if (now.type == COMMAND_TYPE.NORMAL_TEXT || now.type == COMMAND_TYPE.NORMAL_TEXT_CONTINUE)
                {
                    string check = "";
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
                    ////Debug.Log(check);
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

                    //Debug.Log(check);
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
                        switch ((now as STAND_IMAGE_scription).place)
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
                    if (now.text == "loop")
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
                case COMMAND_TYPE.RETURN:
                    return NOVEL_STATUS.FINISH;
            }
        }
        //Debug.Log(now.text);
        return NOVEL_STATUS.WRITING;
    }
    string save_data = "";
    //クリック・スペースキーが押され、次のシーンに続くときの処理
    bool Operation_next()
    {
        if ((Input.GetButtonDown("Submit")&& !Scripter_ClickChecker.button_over))
        {
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
    public bool Updating()
    {
        if (screenshot_coroutine) return true;
        //バックログ処理
        //Operation_BackLog();

        //SCREEN_STATUS分岐（バックログか、ノベルエンジンメインか、セーブ・ロードか。）
        switch (screen)
        {
            case SCREEN_STATUS.BACK_LOG:

                break;

            case SCREEN_STATUS.MAIN:
                //次の行へ
                if (state == NOVEL_STATUS.NEXT)
                {
                    ResetFrontAlpha();

                    if (character_voice.isPlaying && isvoicestop)
                    {
                        character_voice.Stop();
                    }
                    ////Debug.Log("Scripter Update Call");

                    state = Next_Command();
                    if (state == NOVEL_STATUS.FINISH)
                    {
                        FrontScreen.left.name = "left";
                        FrontScreen.right.name = "right";
                        FrontScreen.center.name = "center";
                        BackScreen.left.name = "left";
                        BackScreen.center.name = "center";
                        BackScreen.right.name = "right";
                        scriptions.Clear();
                        reading_pos = 0;
                        FrontScreen.background.sprite = skeltonsprite;
                        FrontScreen.center.sprite = FrontScreen.right.sprite = FrontScreen.left.sprite = skeltonsprite;
                        BackScreen.center.sprite = BackScreen.right.sprite = BackScreen.left.sprite = skeltonsprite;

                        BackScreen.background.sprite = skeltonsprite;
                        mainui.SetActive(false);
                        screenshot_coroutine = false;
                        return false;
                    }
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
                    if (!character_voice.isPlaying)
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
                else if (state == NOVEL_STATUS.DELAY)
                {
                    if ((Operation_next() && skipflag) || delaynowtime >= delaytime)
                    {
                        state = NOVEL_STATUS.NEXT;
                    }
                    delaynowtime += (int)(Time.deltaTime * 1000);
                }
                else if (state == NOVEL_STATUS.UIHIDE)
                {
                    //今は非表示にするだけ。
                    HideUI();
                    state = NOVEL_STATUS.NEXT;
                }
                else if (state == NOVEL_STATUS.UISHOW)
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
                                ////Debug.Log("Error;");
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
                    switch (registered_animation)
                    {
                        //フェードイン
                        case ANIMATION_TYPE.FADE_IN:
                            FrontScreen.gobj.GetComponent<CanvasGroup>().alpha = 1;
                            state = NOVEL_STATUS.ANIMATION;
                            break;
                        //瞬間表示
                        case ANIMATION_TYPE.ANIMATION_NO_TIME:
                            //BackScreenをFrontScreenにコピー
                            FrontScreen.Copy(BackScreen);
                            BackScreen.gobj.SetActive(false);
                            state = NOVEL_STATUS.NEXT;

                            anim_timer = 0;
                            break;
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
            case SCREEN_STATUS.FADE_OUT:
                ////Debug.Log("Error");
                break;
        }
        return true;
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

    //セーブ用スクリーンショットを撮る関数
    IEnumerator LoadScreenshot()
    {
        if (screenshot_coroutine) yield break;
        screenshot_coroutine = true;
        yield return new WaitForEndOfFrame();
        eventInitBackgroud = new Texture2D(Screen.width, Screen.height);
        eventInitBackgroud.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        eventInitBackgroud.Apply();
        FrontScreen.background.sprite = Sprite.Create(eventInitBackgroud,new Rect(0,0, Screen.width, Screen.height),Vector2.zero);
        FrontScreen.background.color = new Color(1, 1, 1, 1);
        BackScreen.background.sprite = Sprite.Create(eventInitBackgroud,new Rect(0,0, Screen.width, Screen.height),Vector2.zero);
        BackScreen.background.color = new Color(1, 1, 1, 1);
        screenshot_coroutine = false;
    }
}
