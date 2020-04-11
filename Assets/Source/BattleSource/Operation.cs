using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.IO;
using System;
using UnityEngine.SceneManagement;

//全体制御を行うためのクラス
public class Operation : MonoBehaviour {
    //マウスの実座標
    private Vector3 MousePosition;

    //セレクトタイル
    [Header("マウス選択タイル")]
    public GameObject prefabSelectTile;
    [System.NonSerialized]public GameObject SelectTile;

    //マウスのマップ内座標(マップデータ配列の添え字）
    private int mouse_y = -1;  //マップ内にマウスがない（初期値）
    private int mouse_x = -1;  //マップ内にマウスがない（初期値）

    public static bool mouse_click = false; //ちょっと嫌だが仕方がない
    bool right_mouse_click = false;
    bool resume_button_click = false;

    [Header("Main Canvas")]
    public Canvas canvas;

    [Header("Event Canvas")]
    public Canvas event_canvas;

    [Header("Title Canvas")]
    public Canvas titleCanvas;


    //ターンチェンジ演出時間
    [Header("ターンチェンジ")]
    public float turnchange_time = 2.0f;
    float now_turnchange_time = 0;
    public Text turnChangeText;
    
    public GameObject turnChangePanel;
    public string playerTurnChangeText;
    public string enemyTurnChangeText;

    //効果音
    [Header("効果音")]
    public AudioSource seAudioSource;
    public AudioClip seButtonOK;
    public AudioClip seButtonCancel;
    public AudioClip seButtonSlide;
    public AudioClip seTurnChange;
    //他クラスから効果音再生を登録する
    private static AudioClip seSetClip;
    private static bool seSetflag = false;

    //BGM
    [Header("BGM")]
    public AudioSource bgmAudioSource;

    //このSceneで読み込むマップファイル
    [Header("このSceneで読み込むマップファイル StreamingAssets/")]
    public string mapfilename = "testmap.csv";

    //次のシーン名
    public string nextscene;

    //EnemyeCoroutine flag
    bool ecoroutine_runflag = false;

    //ステージタイトルタイトル表示周り
    //ステージタイトル
    [Header("ステージタイトル")]
    public GameObject stageTitleGobj;
    public Text stageTitleText;
    public string stageTitle;
    //勝敗条件
    [Header("勝敗条件をまとめるGameObject")]
    public GameObject orderGobj;
    //勝利条件テキスト
    [Header("勝利条件")]
    public Text victoryOrderText;
    public string victory_condition;
    public string victoryOrder;

    //敗北条件テキスト
    [Header("敗北条件")]
    public Text defeatOrderText;
    public string defeat_condition;
    public string defeatOrder;

    //FadeIN時間
    public float fadeintimer = 1.0f;
    public float timefadeout = 2.0f;
    //イベント発生条件
    [Header("イベント発生条件")]
    public List<string> event_condition = new List<string>();

    public static List<int> used_event = new List<int>();

    private bool isBattlePart = true;
    [Header("画面全体のフェードマスク")]
    public Fade fademask;

    [Header("メッセージボックス")]
    public GameObject prefabMesBox;
    [System.NonSerialized] MessageBoxHundler messageBox;
    [System.NonSerialized] bool messageBoxflag = false;

    //描画ステート
    enum DRAW_TITLE_STATE
    {
        MASKIN,
        SETTING,
        TITLE,
        ORDER,
        NEXT
    }
    enum DRAW_GAMEOVER_STATE
    {
        ANIMCHECK,
        FADEOUTSTART,
        NEXT
    }
    enum DRAW_STAGECLEAR_STATE
    {
        ANIMCHECK,
        VICTORYANIMSTART,
        VICTORYANIM,
        VICTORYEFFECT,
        NEXT
    }
    enum DRAW_SCENECHANGE_STATE
    {
        FADEOUTSTART,
        NEXT
    }

    DRAW_GAMEOVER_STATE draw_gameover_state = DRAW_GAMEOVER_STATE.ANIMCHECK;
    DRAW_SCENECHANGE_STATE draw_scenechange_state = DRAW_SCENECHANGE_STATE.FADEOUTSTART;
    DRAW_STAGECLEAR_STATE draw_stageclear_state = DRAW_STAGECLEAR_STATE.ANIMCHECK;

    //描画タイマー
    float now_title_timmer = 0.0f;
    [Header("タイトル描画時間")]
    public float draw_title_time = 1.0f;
    [Header("勝敗条件描画時間")]
    public float draw_order_time = 1.0f;

    DRAW_TITLE_STATE draw_title_state = DRAW_TITLE_STATE.MASKIN;

    //セーブ周り
    //SaveCanvas
    private Canvas save_canvas;
    //セーブ制御用オブジェクト
    private SaveManager savemanager;
    //セーブ用スクリーンショット（Title描画完了時に撮影）
    private Texture2D picture;
    //coroutine flag
    private bool screenshot_coroutine = false;
    //ロードチェックファイル名
    public const string loadtemp = "/SaveData/loadtemp";

    //シーン切り替え用
    private string nextscenename;
    [Header("キャラクター選択")]
    //出撃キャラクターの選択用ゲームオブジェクト
    public GameObject UnitSetupWindow;
    public GameObject ButtonForUnitSetup;

    //ステージクリア周り
    [Header("ステージクリア獲得経験値")]
    public int expStageClear;
    [Header("ステージクリアロゴ")]
    public GameObject pregabStageClearText;
    [Header("ステージクリア時のキャラアニメーション名(NONEでスキップ)")]
    public string victoryanim = "Victory";
    [Header("ステージクリア時のファンファーレ")]
    public AudioClip audioStageClear;
    [Header("ステージクリアロゴのフェードイン時間")]
    public float timeStageClear = 1.5f;
    private GameObject gameObjectStageClearText;


    // Use this for initialization
    void Start() {
        Resources.UnloadUnusedAssets();
        //--------------Set Active-------------------------
        titleCanvas.gameObject.SetActive(true);
        canvas.gameObject.SetActive(false);
        stageTitleGobj.SetActive(false);
        orderGobj.SetActive(false);

        turnChangeText.gameObject.SetActive(false);
        turnChangePanel.SetActive(false);
        SelectTile = Instantiate(prefabSelectTile);



        //--------------マップ読み込み-------------------------
        ScriptReader.MapReader(mapfilename);
        //--------------グローバル変数の初期化-----------------
        BattleVal.selectX = -1;
        BattleVal.selectY = -1;
        BattleVal.id2index = new Dictionary<string, Unitdata>();
        BattleVal.title_name = stageTitle;
        BattleVal.str_victoryorder = victoryOrder;
        BattleVal.str_defeatorder = defeatOrder;
        BattleVal.turn = 0;
        BattleVal.turnplayer = 1;
        BattleVal.menuflag = false;
        BattleVal.sysmenuflag = false;
        Operation.used_event = new List<int>();

        //行動スタックInstantiate
        BattleVal.actions = new Stack<Action>();

        //セーブデータをLoadされたか判定
        //Windows
        //if (System.IO.File.Exists(Application.streamingAssetsPath + loadtemp))
        //Mac (streamingAssetsPath is readonly!)
        if (System.IO.File.Exists(Application.persistentDataPath + loadtemp))
            BattleVal.status = STATUS.DRAW_STAGE_FROM_LOADDATA;
        else
            BattleVal.status = STATUS.DRAW_STAGE;
    }

    void SetupTurnChange()
    {
        //行動スタックのクリア（1手戻し不可能に）
        BattleVal.actions.Clear();

        StopCoroutine(EnemyCoroutine());

        //手番のセット
        //Debug.Log(BattleVal.turnplayer);
        BattleVal.turnplayer = (BattleVal.turnplayer + 1) % 2;
        //Debug.Log(BattleVal.turnplayer);


        //ユニットの行動可能性・攻撃可能性の更新
        foreach (Unitdata unit in BattleVal.unitlist)
        {
            if (unit.team == BattleVal.turnplayer)
            {
                unit.movable = true;
                unit.atackable = true;
                //行動不能・攻撃不能系の状態異常
                foreach(Condition condition in unit.status.conditions)
                {
                    if (condition.forbiddenAttack) unit.atackable = false;
                    if (condition.forbiddenMove) unit.movable = false;
                    if (!unit.atackable && !unit.movable) break;
                }
            }
        }

        BattleVal.turn++;
        //TURNCHANGE_SHOW用の初期化
        SetupTurnChangeShow();
    }

    void SetupTurnChangeShow()
    {
        //選択セルの初期化
        BattleVal.selectX = -1;
        BattleVal.selectY = -1;
        //TURNCHANGE_SHOW用の初期化

        now_turnchange_time = 0;
        if (BattleVal.turnplayer == 0)
        {
            //視点を移動
            foreach (Unitdata unit in BattleVal.unitlist)
            {
                if (unit.team == 0)
                {
                    Vector3 temp = new Vector3();
                    Mapclass.TranslateMapCoordToPosition(ref temp, unit.x, unit.y);
                    CameraAngle.CameraPoint(temp);
                    mouse_x = unit.x;
                    mouse_y = unit.y;
                    Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
                    break;
                }
            }
            turnChangeText.GetComponent<GradationController>().colorTop = new Color(0.7f, 0.7f, 1);
            turnChangeText.GetComponent<GradationController>().colorBottom = new Color(0, 0, 1);
            turnChangePanel.GetComponent<GradationController>().colorTop = new Color(0, 0.7f, 1, 0.7f);
            turnChangeText.text = playerTurnChangeText;

        }
        else
        {
            turnChangeText.GetComponent<GradationController>().colorTop = new Color(1, 0.7f, 0.7f);
            turnChangeText.GetComponent<GradationController>().colorBottom = new Color(1, 0, 0);
            turnChangePanel.GetComponent<GradationController>().colorTop = new Color(1, 0.7f, 0, 0.7f);
            turnChangeText.text = enemyTurnChangeText;
        }
        //サイズ調整
        turnChangeText.rectTransform.sizeDelta = new Vector2(turnChangeText.preferredWidth, turnChangeText.preferredHeight);
        turnChangeText.rectTransform.sizeDelta = new Vector2(turnChangeText.preferredWidth, turnChangeText.preferredHeight);
        //初期位置
        turnChangeText.transform.localPosition =
            new Vector3(-turnChangeText.GetComponent<RectTransform>().sizeDelta.x - canvas.GetComponent<RectTransform>().sizeDelta.x / 2, 0, 0);

        turnChangeText.gameObject.SetActive(true);
        turnChangePanel.SetActive(true);
    }

    //割込みイベントのフラグチェック
    [Header("ノベル用のオブジェクト")]
    public GameObject Novel;
    void EventCheck()
    {

        //ノベル表示の条件式チェック
        for (int i = 0; i < event_condition.Count; i++)
        {
            string str = event_condition[i];
            string[] temp = str.Split(',');
            //Debug.Log(temp[0]);
            bool check = false;
            foreach (int a in used_event)
            {
                if (a == i) check = true;
            }
            if (check) continue;
            if (Novel.GetComponent<IFCommand>().If_command(temp[0]))
            {
                //ノベルパート用のキャンバスの有効化
                //ラベルtemp[1]の場所にジャンプをする
                if (check) continue;
                canvas.gameObject.SetActive(false);
                isBattlePart = false;
                string path;
                if (Debug.isDebugBuild)
                {
                    path = "/debug/";
                }
                else
                {
                    path = "/compiled/";
                }
                FileStream fs = new FileStream(Application.streamingAssetsPath + path + "Label_Table.db", FileMode.Open, FileAccess.Read);
                StreamReader sr = new StreamReader(fs);
                int pos = 0;
                string file_name = "";
                Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine();
                    string t = ec.DecryptionSystem(s, false);
                    //Debug.Log(t);
                    string[] a = t.Split(',');
                    if (a[0].Equals(temp[1]))
                    {
                        pos = int.Parse(a[1]);
                        file_name = a[2];
                    }
                }
                used_event.Add(i);
                event_canvas.gameObject.SetActive(true);
                Novel.GetComponent<Light_Novel_Scripter>().start_Novel(file_name, pos);

                continue;
            }
        }

    }

    void DrawTitle()
    {
        switch (draw_title_state)
        {
            case DRAW_TITLE_STATE.MASKIN:
                SelectTile.SetActive(false);
                if (titleCanvas.gameObject.GetComponent<FadeUIController>().FadeIn(fadeintimer))
                    draw_title_state = DRAW_TITLE_STATE.SETTING;
                break;
            case DRAW_TITLE_STATE.SETTING:
                //右からスライドイン
                stageTitleGobj.GetComponent<RectTransform>().position += new Vector3(stageTitleGobj.GetComponent<RectTransform>().sizeDelta.x, 0, 0);
                stageTitleText.color = new Color(1, 1, 1, 0); //Gradiation Controller下なので、rgb値はダミー。aだけはfollowするように設定している。
                stageTitleText.text = stageTitle;

                stageTitleGobj.SetActive(true);
                now_title_timmer = 0.0f;
                draw_title_state = DRAW_TITLE_STATE.TITLE;
                break;
            //ステージタイトルの描画
            case DRAW_TITLE_STATE.TITLE:
                if (now_title_timmer <= draw_title_time - 0.2f + Time.deltaTime)
                {
                    float dx = -stageTitleGobj.GetComponent<RectTransform>().sizeDelta.x *
                        (Mathf.Cos(Mathf.PI / draw_title_time * now_title_timmer) + 1.0f) * Time.deltaTime / draw_title_time;
                    stageTitleGobj.GetComponent<RectTransform>().position += new Vector3(dx, 0);
                    stageTitleText.color += new Color(0, 0, 0, (Mathf.Cos(Mathf.PI / draw_title_time * now_title_timmer) + 1.0f) * Time.deltaTime / draw_title_time);
                    now_title_timmer += Time.deltaTime;
                }
                else
                {
                    //右からスライドイン
                    orderGobj.GetComponent<RectTransform>().position += new Vector3(Screen.width, 0, 0);
                    victoryOrderText.text = victoryOrder;
                    defeatOrderText.text = defeatOrder;
                    orderGobj.GetComponent<CanvasGroup>().alpha = 0;
                    //次へ
                    orderGobj.SetActive(true);
                    now_title_timmer = 0.0f;
                    draw_title_state = DRAW_TITLE_STATE.ORDER;
                }
                break;
            //勝敗条件の描画
            case DRAW_TITLE_STATE.ORDER:
                if (now_title_timmer <= draw_order_time - 0.2f + Time.deltaTime)
                {
                    float dx = -Screen.width *
                        (Mathf.Cos(Mathf.PI / draw_order_time * now_title_timmer) + 1.0f) * Time.deltaTime / draw_order_time;
                    orderGobj.GetComponent<CanvasGroup>().alpha += Time.deltaTime / draw_order_time;
                    orderGobj.GetComponent<RectTransform>().position += new Vector3(dx, 0);
                    now_title_timmer += Time.deltaTime;
                }
                else
                {
                    orderGobj.GetComponent<CanvasGroup>().alpha = 1;
                    //スクリーンショットを取る
                    StartCoroutine(LoadScreenshot());
                    draw_title_state = DRAW_TITLE_STATE.NEXT;
                }
                break;
            //次へ
            case DRAW_TITLE_STATE.NEXT:
                //クリック待ち
                if (Input.GetButtonDown("Submit"))
                {

                    //キャンバス切り替え
                    titleCanvas.gameObject.SetActive(false);
                    canvas.gameObject.SetActive(true);
                    //セットアップフェイズへ
                    BattleVal.status = STATUS.SETUP; //->セットアップ完了時にIFCommand.Initialization();
                    UnitSetupWindow.SetActive(true);
                    SelectTile.SetActive(true);

                    //Debug Mode
                    //BattleVal.status = STATUS.TURNCHANGE;
                    //一応
                    draw_title_state = DRAW_TITLE_STATE.SETTING;
                    //初期化
                    Novel.GetComponent<IFCommand>().Initialization();
                }
                break;
        }
    }

    // Update is called once per frame
    void Update() {
        //Debug.Log(BattleVal.status);
        if (BattleVal.status == STATUS.PLAYER_UNIT_SELECT 
            || BattleVal.status == STATUS.TURNCHANGE)
        {
            EventCheck();
            Victory_or_Defeat();
        }

        if (isBattlePart)
        {
            //gameover割込み
            if (BattleVal.is_gameover)
            {
                BattleVal.status = STATUS.GAMEOVER;
                //敵コルーチンの停止
                StopCoroutine(EnemyCoroutine());
                BattleVal.is_gameover = false;
            }


            //外部クラスから効果音再生が登録されていた場合
            if (seSetflag)
                playSE();

            //ステートで分岐
            switch (BattleVal.status)
            {
                //タイトル描画
                case STATUS.DRAW_TITLE:
                    DrawTitle();
                    break;
                //手番チェンジの処理
                case STATUS.TURNCHANGE:
                    //EventCheck();
                    //色々とリセット
                    enemy_show_state = 0;
                    CharaStatusPrinter.Show_Enemy_Range(enemy_show_state);
                    CharaStatusPrinter.HideStatus();
                    SelectTile.SetActive((BattleVal.turnplayer != 0));

                    SetupTurnChange();
                    //効果音再生
                    seAudioSource.PlayOneShot(seTurnChange);
                    //Status Update
                    BattleVal.status = STATUS.TURNCHANGE_SHOW;
                    break;

                //データロード時
                case STATUS.TAKE_SCREENSHOT_FROM_LOADDATA:
                    //FadeIN処理
                    if (!titleCanvas.gameObject.GetComponent<FadeUIController>().FadeIn(fadeintimer))
                        break;
                    else
                    {
                        //選択セルの初期化
                        BattleVal.selectX = -1;
                        BattleVal.selectY = -1;
                        Novel.GetComponent<IFCommand>().Initialization();
                        EventCheck();

                        //視点を移動
                        foreach (Unitdata unit in BattleVal.unitlist)
                        {
                            if (unit.team == 0)
                            {
                                Vector3 temp = new Vector3();
                                Mapclass.TranslateMapCoordToPosition(ref temp, unit.x, unit.y);
                                CameraAngle.CameraPoint(temp);
                                mouse_x = unit.x;
                                mouse_y = unit.y;
                                Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
                                break;
                            }
                        }
                        //キャンバス切り替え
                        titleCanvas.gameObject.SetActive(false);
                        canvas.gameObject.SetActive(true);
                        //スクリーンショットを取る
                        StartCoroutine(LoadScreenshot());

                        BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
                        break;
                    }

                //手番交換の表示処理
                case STATUS.TURNCHANGE_SHOW:
                    if (now_turnchange_time >= turnchange_time + 0.2f - Time.deltaTime)
                    {
                        //Status Update
                        if (BattleVal.turnplayer == 0)
                        {

                            BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
                        }
                        else
                            BattleVal.status = STATUS.ENEMY_UNIT_SELECT;
                        turnChangePanel.SetActive(false);
                        turnChangeText.gameObject.SetActive(false);
                    }
                    else
                    {
                        float dx = (Screen.width + 1.80f * turnChangeText.rectTransform.sizeDelta.x)
                            * (Mathf.Cos(2 * Mathf.PI / turnchange_time * now_turnchange_time) + 1.0f) * Time.deltaTime / turnchange_time;
                        turnChangeText.transform.localPosition += new Vector3(dx, 0, 0);
                        now_turnchange_time += Time.deltaTime;
                    }
                    break;
                //経験値獲得表示
                case STATUS.GETEXP:
                    ShowGetExp();
                    break;
                case STATUS.GAMEOVER:
                    switch(draw_gameover_state)
                    {
                        case DRAW_GAMEOVER_STATE.ANIMCHECK:
                            bgmAudioSource.Stop();
                            if (!UnitDefeat.is_defeatanim)
                            {
                                fademask.FadeIn(timefadeout, () => {
                                    draw_gameover_state = DRAW_GAMEOVER_STATE.NEXT;
                                });
                                draw_gameover_state = DRAW_GAMEOVER_STATE.FADEOUTSTART;

                            }
                            break;

                        case DRAW_GAMEOVER_STATE.FADEOUTSTART:
                            //Debug.Log("coroutine");

                            break;

                        case DRAW_GAMEOVER_STATE.NEXT:
                            GameVal.nextscenename = SceneManager.GetActiveScene().name;
                            SceneManager.LoadScene("GameOverScene", LoadSceneMode.Single);
                            Resources.UnloadUnusedAssets();
                            break;
                    }
                    break;
                case STATUS.SAVE:
                    Main_Save();
                    break;
                case STATUS.LOAD:
                    Main_Load();
                    break;
                case STATUS.STAGECLEAR:
                    switch (draw_stageclear_state)
                    {
                        case DRAW_STAGECLEAR_STATE.ANIMCHECK:
                            if (!UnitDefeat.is_defeatanim)
                            {
                                //勝利条件達成時のイベントなどをチェック
                                EventCheck();
                                //次にSTAGECLEARに来た時にはアニメーションを開始する
                                draw_stageclear_state = DRAW_STAGECLEAR_STATE.VICTORYANIMSTART;
                            }
                            break;
                        case DRAW_STAGECLEAR_STATE.VICTORYANIMSTART:
                            if (victoryanim == "NONE")
                            {
                                draw_stageclear_state = DRAW_STAGECLEAR_STATE.VICTORYEFFECT;
                            }
                            else
                            {
                                try
                                {
                                    BattleVal.selectedUnit.gobj.GetComponent<Animator>().Play(victoryanim);

                                }
                                catch (MissingReferenceException ex)
                                {
                                    //ターン終了時の状態異常などで決着し、selectedUnitがトドメを刺したキャラじゃない場合
                                    foreach (Unitdata unit in BattleVal.unitlist)
                                    {
                                        if (unit.team == 0)
                                        {
                                            BattleVal.selectedUnit = unit;
                                            BattleVal.selectedUnit.gobj.GetComponent<Animator>().Play(victoryanim);
                                            break;
                                        }
                                    }
                                }
                                catch(NullReferenceException ex)
                                {
                                    //ターン終了時の状態異常などで決着し、selectedUnitがトドメを刺したキャラじゃない場合
                                    foreach (Unitdata unit in BattleVal.unitlist)
                                    {
                                        if (unit.team == 0)
                                        {
                                            BattleVal.selectedUnit = unit;
                                            BattleVal.selectedUnit.gobj.GetComponent<Animator>().Play(victoryanim);
                                            break;
                                        }
                                    }
                                }
                                draw_stageclear_state = DRAW_STAGECLEAR_STATE.VICTORYANIM;
                            }
                            break;
                        case DRAW_STAGECLEAR_STATE.VICTORYANIM:
                            //アニメーションの終了を待つ
                            if (!BattleVal.selectedUnit.gobj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName(victoryanim)
                        && !BattleVal.selectedUnit.gobj.GetComponent<CharaAnimation>().voiceSource.isPlaying
                        && !BattleVal.selectedUnit.gobj.GetComponent<CharaAnimation>().seSource.isPlaying)
                            {
                                gameObjectStageClearText = Instantiate(pregabStageClearText, canvas.transform);
                                gameObjectStageClearText.GetComponent<CanvasGroup>().alpha = 0;
                                bgmAudioSource.clip = audioStageClear;
                                bgmAudioSource.loop = false;
                                bgmAudioSource.Play();
                                draw_stageclear_state = DRAW_STAGECLEAR_STATE.VICTORYEFFECT;
                            }
                            break;
                        case DRAW_STAGECLEAR_STATE.VICTORYEFFECT:
                            if (gameObjectStageClearText.GetComponent<CanvasGroup>().alpha >= 1)
                            {
                                gameObjectStageClearText.GetComponent<CanvasGroup>().alpha = 1;
                                if(!bgmAudioSource.isPlaying)
                                {
                                    fademask.FadeIn(timefadeout, () => {
                                        //セーブデータの更新
                                        foreach(Unitdata unit in BattleVal.unitlist)
                                        {
                                            if(unit.team == 0 && unit.partyid > 0)
                                            {
                                                GameVal.masterSave.UnitUpdate(unit.partyid, new UnitSaveData(unit));
                                            }
                                        }
                                        GameVal.nextscenename = nextscene;
                                        SceneManager.LoadScene("StageClearScene", LoadSceneMode.Single);
                                        Resources.UnloadUnusedAssets();
                                    });
                                    draw_stageclear_state = DRAW_STAGECLEAR_STATE.NEXT;
                                }
                            }
                            else
                            {
                                gameObjectStageClearText.GetComponent<CanvasGroup>().alpha += Time.deltaTime / timeStageClear;
                            }
                            break;
                        case DRAW_STAGECLEAR_STATE.NEXT:
                            
                            break;
                    }
                    EventCheck();
                    
                    break;

                //コンピュータの処理
                case STATUS.ENEMY_UNIT_SELECT:
                    //コルーチンの呼び出し
                    StartCoroutine(EnemyCoroutine());
                    break;

                //シーン遷移
                case STATUS.FADEOUT:
                    switch(draw_scenechange_state)
                    {
                        case DRAW_SCENECHANGE_STATE.FADEOUTSTART:
                            BattleVal.sysmenuflag = false;
                            fademask.FadeIn(timefadeout, () => {
                                GameVal.nextscenename = nextscenename;
                                SceneManager.LoadScene("BlackScene", LoadSceneMode.Single);
                                Resources.UnloadUnusedAssets();
                            });
                            draw_scenechange_state = DRAW_SCENECHANGE_STATE.NEXT;
                            return;
                        case DRAW_SCENECHANGE_STATE.NEXT:
                            return;
                    }
                    
                    break;
            }
            if(messageBoxflag)
            {
                if(!messageBox.destroyflag)
                {
                    return;
                }
                else
                {
                    messageBoxflag = false;
                }
            }
            //プレイヤー操作
            if (BattleVal.turnplayer == 0 || BattleVal.status == STATUS.SETUP)
            {
                //マウス操作の判定
                if (BattleVal.is_mouseinput && !EventSystem.current.IsPointerOverGameObject())
                    GetMousePosition();
                //キャンセルボタン時の処理
                if (Input.GetButtonDown("Cancel"))
                {
                    if (!right_mouse_click)
                    {
                        ActRightClick();
                        right_mouse_click = true;
                    }
                }
                else
                    right_mouse_click = false;

                //キーボード（パッド）操作の判定
                //マップ移動処理
                if(!BattleVal.is_mouseinput 
                    && (Mathf.Abs(Input.GetAxisRaw("Horizontal")) == 1 || Mathf.Abs(Input.GetAxisRaw("Vertical")) == 1))
                {
                    //セレクトマスの移動
                    if (!BattleVal.sysmenuflag && !BattleVal.menuflag && enemy_show_state != 1
                     &&(  BattleVal.status == STATUS.PLAYER_UNIT_SELECT
                       || BattleVal.status == STATUS.PLAYER_UNIT_MOVE
                       || BattleVal.status == STATUS.PLAYER_UNIT_ATTACK
                       || BattleVal.status == STATUS.PLAYER_UNIT_SKILL
                       || (BattleVal.status == STATUS.SETUP && CharaSetup.mode_Field)))
                        GetKeypadPosition();
                }
                else
                {
                    //キーが離されてたら、次回入力を直ちにできるようにする
                    delta_keypad_time = interval_keypad_time;
                }
                //決定ボタン
                if(!BattleVal.is_mouseinput && !BattleVal.sysmenuflag 
                    && !(BattleVal.status == STATUS.SETUP && !CharaSetup.mode_Field))
                {
                    ActLeftClick();
                }
                //巻き戻しボタン
                if (Input.GetButtonDown("Resume") && BattleVal.status == STATUS.PLAYER_UNIT_SELECT
                    && !BattleVal.sysmenuflag)
                {
                    if (!resume_button_click)
                    {
                        if (BattleVal.actions.Count != 0)
                        {
                            seAudioSource.PlayOneShot(seButtonCancel);

                            Resume_Action();

                            resume_button_click = true;
                        }
                    }
                }
                else
                    resume_button_click = false;

                UnitFeed();
            }
        }
        else
        {
            //ノベルパートの時
            if (!Novel.GetComponent<Light_Novel_Scripter>().Updating())
            {
                event_canvas.gameObject.SetActive(false);
                canvas.gameObject.SetActive(true);
                isBattlePart = true;
            }
        }
    }


    //セレクトタイルをマップ上に描画(マウスモード時)
    void DrawSelectTile()
    {
        //以前のを保存
        int temp_mouse_y = mouse_y;
        int temp_mouse_x = mouse_x;

        Mapclass.TranslatePositionToMapCoord(MousePosition, ref mouse_x, ref mouse_y);
        //マウス位置が変化した場合
        if (mouse_y != temp_mouse_y || mouse_x != temp_mouse_x)
        {
            Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
        }

    }

    //敵キャラクリック時のステート
    public static int enemy_show_state = 0;

    //マウスの実座標取得→マウス左クリック関連の処理
    void GetMousePosition()
    {
        RaycastHit hit = new RaycastHit();
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //当たったら
        if (Physics.Raycast(ray.origin, ray.direction, out hit, Mathf.Infinity))
        {
            if (hit.collider.gameObject.CompareTag("SelectableMapTile"))
            {
                MousePosition = hit.transform.position;
                //描画
                SelectTile.SetActive(true);

                int tempmouse_x = mouse_x;
                int tempmouse_y = mouse_y;

                //セレクトタイルの表示と、mouse_x,mouse_yの更新
                DrawSelectTile();
                
                //マウスオーバー系の処理
                ActMouseOver(tempmouse_x, tempmouse_y);

                //左クリック時の処理
                ActLeftClick();
            }
            else
            {
                //mouse_y = -1;
                //mouse_x = -1;
                //セレクトタイルを非表示に
                SelectTile.SetActive(false);
            }
        }
    }

    public void ActMouseOver(int tempmouse_x, int tempmouse_y)
    {
        CharaStatusPrinter.mapheight = BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][mouse_y][mouse_x];
       
        //マウスオーバー系の処理
        switch (BattleVal.status)
        {
            case STATUS.SETUP:
                //敵のスキル範囲を表示している場合
                if (enemy_show_state > 3)
                {
                    if (CharaSkill.Is_OverAttackable(mouse_x, mouse_y))
                    {
                        CharaSkill.Set_Attackarea(mouse_x, mouse_y);
                        CharaSkill.Show_Attackarea();
                    }
                    else
                    {
                        CharaSkill.Destroy_Areatile();
                    }
                }
                //メニューを開いていない・詳細表示をしていない場合・このフレームでクリックをしていない
                if (!BattleVal.menuflag && enemy_show_state == 0
                    && (mouse_x != tempmouse_x || mouse_y != tempmouse_y)
                    && !Input.GetButtonDown("Submit"))
                {
                    //キャラがいるなら
                    if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", mouse_x, mouse_y)))
                    {
                        CharaStatusPrinter.SetUp_DrawStatus(false, mouse_x, mouse_y);
                    }
                    else
                    {
                        CharaStatusPrinter.HideStatus();
                    }
                }
                break;
            case STATUS.PLAYER_UNIT_SELECT:
                EventCheck();
                Victory_or_Defeat();
                //敵のスキル範囲を表示している場合
                if (enemy_show_state > 3)
                {
                    if (CharaSkill.Is_OverAttackable(mouse_x, mouse_y))
                    {
                        CharaSkill.Set_Attackarea(mouse_x, mouse_y);
                        CharaSkill.Show_Attackarea();
                    }
                    else
                    {
                        CharaSkill.Destroy_Areatile();
                    }
                }
                else
                {
                    //メニューを開いていない・詳細表示をしていない場合・このフレームでクリックをしていない
                    if(!BattleVal.menuflag && enemy_show_state == 0
                        && (mouse_x != tempmouse_x || mouse_y != tempmouse_y)
                        && !Input.GetButtonDown("Submit"))
                    {

                        //キャラがいるなら
                        if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", mouse_x, mouse_y)))
                        {
                            CharaStatusPrinter.SetUp_DrawStatus(false, mouse_x, mouse_y);
                        }
                        else
                        {
                            CharaStatusPrinter.HideStatus();
                        }
                    }
                }
                break;
            case STATUS.PLAYER_UNIT_ATTACK:
                //ユニットがマウス位置にいて攻撃可能かを判定する
                if (CharaAttack.Is_attackable(mouse_x, mouse_y))
                {
                    if (!(mouse_x == tempmouse_x && mouse_y == tempmouse_y))
                    {
                        int predamage = CharaAttack.Calc_Damage(BattleVal.selectedUnit,
                            BattleVal.id2index[string.Format("{0},{1}", mouse_x, mouse_y)]);
                        float predex = CharaAttack.Calc_Dexterity(BattleVal.selectedUnit,
                            BattleVal.id2index[string.Format("{0},{1}", mouse_x, mouse_y)]);
                        float prerate = CharaAttack.Calc_CriticalRate(BattleVal.selectedUnit,
                            BattleVal.id2index[string.Format("{0},{1}", mouse_x, mouse_y)]);
                        CharaStatusPrinter.Setup_DrawTargetStatus(mouse_x, mouse_y, predamage, true, predex, prerate);
                    }
                }
                else
                    CharaStatusPrinter.HideTargetStatus();
                break;
            case STATUS.PLAYER_UNIT_SKILL:
                //範囲攻撃のエリア描画
                if (CharaSkill.Is_OverAttackable(mouse_x, mouse_y))
                {
                    CharaSkill.Set_Attackarea(mouse_x, mouse_y);
                    CharaSkill.Show_Attackarea();

                    //ユニットがマウス位置にいる
                    if (CharaSkill.Is_attackableTile(mouse_x, mouse_y))
                    {
                        //マウス位置のターゲットのみステータス表示（サモンナイト3準拠）
                        if (!(mouse_x == tempmouse_x && mouse_y == tempmouse_y))
                        {
                            int predamage = CharaSkill.Calc_Damage(BattleVal.selectedUnit,
                                BattleVal.id2index[string.Format("{0},{1}", mouse_x, mouse_y)]);
                            CharaStatusPrinter.Setup_DrawTargetStatus(mouse_x, mouse_y, predamage);
                        }
                    }
                    else
                        CharaStatusPrinter.HideTargetStatus();

                }
                else
                {
                    CharaSkill.Destroy_Areatile();
                }

                break;
        }
    }

    public void ActLeftClick()
    {
        //左クリック時の処理
        if (!mouse_click && Input.GetButtonDown("Submit") && mouse_x != -1 && mouse_y != -1)
        {

            switch (BattleVal.status)
            {
                case STATUS.PLAYER_UNIT_SELECT:
                case STATUS.SETUP:
                    //カメラの視点移動
                    if (!CameraAngle.moveFlag && CharaSkill.skillbuttonlist.Count == 0)
                    {
                        CharaSkill.Destory_SkillButtonList();
                        Vector3 mousepos = new Vector3();
                        Mapclass.TranslateMapCoordToPosition(ref mousepos, mouse_x, mouse_y);
                        
                        
                        CameraAngle.CameraPoint(mousepos);
                        /*
                        if (!BattleVal.id2index.ContainsKey(string.Format("{0},{1}", mouse_x, mouse_y)))
                            CameraAngle.CameraPoint(mousepos);
                        */
                        int tempX = BattleVal.selectX;
                        int tempY = BattleVal.selectY;

                        //selectX,selectYの設定
                        BattleVal.selectX = mouse_x;
                        BattleVal.selectY = mouse_y;



                        //セットアップ時、ユニットを動かすモードの時
                        if(BattleVal.status==STATUS.SETUP && CharaSetup.state == CharaSetup.CharaSetupStatus.MOVEDO)
                        {
                            //移動可能範囲内をクリックしたか？
                            bool selinposlist = false;
                            foreach (int[] pos in CharaSetup.unitsetposlist)
                            {
                                if (pos[0] == BattleVal.selectX && pos[1] == BattleVal.selectY)
                                {
                                    selinposlist = true;
                                    break;
                                }
                            }
                            //自分自身をクリックしたか？
                            if(tempX == BattleVal.selectX && tempY == BattleVal.selectY)
                            {
                                //移動モードを終了し、ステータス詳細表示へ進む（Switchを抜けない）
                                if(BattleVal.is_mouseinput) CharaSetup.state = CharaSetup.CharaSetupStatus.MOVEEND;
                            }
                            else
                            {
                                //そうでない場合で、移動可能範囲をクリックした場合
                                if (selinposlist)
                                {
                                    //クリック先にユニットが居る場合
                                    if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", BattleVal.selectX, BattleVal.selectY)))
                                    {
                                        Unitdata tempunit = BattleVal.id2index[string.Format("{0},{1}", BattleVal.selectX, BattleVal.selectY)];
                                        //入れ替え
                                        CharaMove.Swap_Unit_Position(BattleVal.selectedUnit, tempunit);
                                    }
                                    else
                                    {
                                        //単純な移動
                                        CharaMove.Change_Unit_Position(BattleVal.selectedUnit,
                                            new int[] { BattleVal.selectedUnit.x, BattleVal.selectedUnit.y },
                                            new int[] { BattleVal.selectX, BattleVal.selectY });
                                    }
                                    seAudioSource.clip = seButtonOK;
                                    seAudioSource.Play();
                                    CharaSetup.state = CharaSetup.CharaSetupStatus.MOVEEND;
                                    //二度クリック時の挙動を調整
                                    BattleVal.selectX = -1;
                                    BattleVal.selectY = -1;
                                }
                                else
                                {
                                    seAudioSource.clip = seButtonCancel;
                                    seAudioSource.Play();
                                    CharaSetup.state = CharaSetup.CharaSetupStatus.MOVEEND;
                                }
                                //switch文を脱出
                                mouse_click = true;
                                break;
                            }
                            
                        }
                        //ユニットが選択されている
                        if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", BattleVal.selectX, BattleVal.selectY)))
                        {
                            Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", BattleVal.selectX, BattleVal.selectY)];
                            //選択中のユニットを更新
                            BattleVal.selectedUnit = temp;
                            //敵キャラをクリックした時の処理、または移動可能範囲に居ない味方キャラをセットアップフェイズで選択した場合
                            if (temp.team == 1 || (BattleVal.status == STATUS.SETUP && CharaSetup.state == CharaSetup.CharaSetupStatus.CHECKENEMY))
                            {
                                //同じ敵キャラをクリックした時の処理
                                if(tempX == BattleVal.selectX && tempY == BattleVal.selectY)
                                    enemy_show_state += 1;
                                else
                                    enemy_show_state = 1; //詳細表示
                                enemy_show_state %= 4 + temp.skills.Count; //消去・詳細表示・移動範囲・攻撃範囲・スキル範囲

                                if (BattleVal.status == STATUS.SETUP)
                                {
                                    //セットアップ時のフラグ管理
                                    bool selinposlist = false;
                                    foreach (int[] pos in CharaSetup.unitsetposlist)
                                    {
                                        if (pos[0] == BattleVal.selectX && pos[1] == BattleVal.selectY)
                                        {
                                            selinposlist = true;
                                            break;
                                        }
                                    }
                                    if (selinposlist)
                                    {
                                        enemy_show_state = 0;
                                        CharaSetup.state = CharaSetup.CharaSetupStatus.MOVE; //移動モードフラグON
                                    } 
                                }

                                if (enemy_show_state == 0)
                                {
                                    seAudioSource.PlayOneShot(seButtonCancel);
                                    if (BattleVal.status == STATUS.SETUP && CharaSetup.state == CharaSetup.CharaSetupStatus.CHECKENEMY) CharaSetup.state = CharaSetup.CharaSetupStatus.SET;
                                }
                                else seAudioSource.PlayOneShot(seButtonOK);

                                CharaStatusPrinter.Show_Enemy_Range(enemy_show_state);

                            }
                            //味方キャラをクリック時（メニューがまだ開かれてない）
                            else if (!(tempX == BattleVal.selectX && tempY == BattleVal.selectY)
                                || (tempX == BattleVal.selectX && tempY == BattleVal.selectY && !BattleVal.menuflag && !(BattleVal.status == STATUS.SETUP) && enemy_show_state==0))
                            {
                                seAudioSource.PlayOneShot(seButtonOK);
                                enemy_show_state = 0;
                                CharaStatusPrinter.Show_Enemy_Range(enemy_show_state);
                                CharaStatusPrinter.SetUp_DrawStatus();
                                //セットアップ時のフラグ管理
                                bool selinposlist = false;
                                foreach(int[] pos in CharaSetup.unitsetposlist)
                                {
                                    if(pos[0] == BattleVal.selectX && pos[1] == BattleVal.selectY)
                                    {
                                        selinposlist = true;
                                        break;
                                    }
                                }
                                if (BattleVal.status == STATUS.SETUP
                                 && CharaSetup.state == CharaSetup.CharaSetupStatus.SET)
                                {
                                    if(selinposlist) CharaSetup.state = CharaSetup.CharaSetupStatus.MOVE; //移動モードフラグON
                                    else
                                    {
                                        //移動可能範囲に居ない味方キャラをセットアップフェイズで選択した場合
                                        enemy_show_state = 1;
                                        CharaSetup.state = CharaSetup.CharaSetupStatus.CHECKENEMY;
                                        CharaStatusPrinter.Show_Enemy_Range(enemy_show_state);
                                    }
                                }
                            }
                            //同じ味方キャラをクリックしたとき
                            else if (temp.team == 0 && tempX == BattleVal.selectX && tempY == BattleVal.selectY)
                            {
                                if(BattleVal.is_mouseinput)
                                {
                                    enemy_show_state += 1;
                                    enemy_show_state %= 2; //消去・詳細表示
                                    if (enemy_show_state == 0) seAudioSource.PlayOneShot(seButtonCancel);
                                    else seAudioSource.PlayOneShot(seButtonOK);

                                    CharaStatusPrinter.Show_Enemy_Range(enemy_show_state);
                                    BattleVal.menuflag = false;
                                }
                                //セットアップ時のフラグ管理
                                bool selinposlist = false;
                                foreach (int[] pos in CharaSetup.unitsetposlist)
                                {
                                    if (pos[0] == BattleVal.selectX && pos[1] == BattleVal.selectY)
                                    {
                                        selinposlist = true;
                                        break;
                                    }
                                }
                                if (BattleVal.status == STATUS.SETUP
                                 && CharaSetup.state == CharaSetup.CharaSetupStatus.SET
                                 && enemy_show_state == 0)
                                {
                                    //効果音重複回避
                                    if(selinposlist)
                                    {
                                        if (!BattleVal.is_mouseinput) seAudioSource.PlayOneShot(seButtonOK);
                                        CharaSetup.state = CharaSetup.CharaSetupStatus.MOVE; //移動モードフラグON
                                    }
                                    else
                                    {
                                        
                                        //移動可能範囲に居ない味方キャラをセットアップフェイズで選択した場合
                                        if (!BattleVal.is_mouseinput)
                                        {
                                            seAudioSource.PlayOneShot(seButtonOK);
                                            enemy_show_state = 1;
                                            CharaStatusPrinter.Show_Enemy_Range(enemy_show_state);
                                        }
                                        CharaSetup.state = CharaSetup.CharaSetupStatus.CHECKENEMY;
                                    }
                                }

                            }
                        }
                        else
                        {
                            enemy_show_state = 0;
                            CharaStatusPrinter.Show_Enemy_Range(enemy_show_state);
                            CharaStatusPrinter.HideStatus();
                            if (BattleVal.status == STATUS.SETUP) CharaSetup.state = CharaSetup.CharaSetupStatus.SET;
                            //スキルボタンリスト消去
                            CharaSkill.Destory_SkillButtonList();
                        }
                        mouse_click = true;
                    }
                    break;
                case STATUS.PLAYER_UNIT_MOVE:

                    //移動範囲内ならばキャラを移動する
                    if (CharaMove.Is_movable(mouse_x, mouse_y))
                    {
                        seAudioSource.PlayOneShot(seButtonOK);

                        CharaStatusPrinter.HideStatus();

                        //BattleVal.statusを移動中に変更
                        BattleVal.status = STATUS.MOVING;
                    }
                    break;
                case STATUS.PLAYER_UNIT_ATTACK:

                    //攻撃可能かを判定する
                    if (CharaAttack.Is_attackable(mouse_x, mouse_y))
                    {
                        seAudioSource.PlayOneShot(seButtonOK);

                        CharaStatusPrinter.HideStatus();

                        //BattleVal.statusを戦闘中に変更
                        CharaAttack.Destroy_Attackabletile();
                        BattleVal.status = STATUS.BATTLE;

                        //マウスセルを戻す
                        mouse_x = BattleVal.selectX;
                        mouse_y = BattleVal.selectY;
                        Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
                    }
                    break;
                case STATUS.PLAYER_UNIT_SKILL:

                    //攻撃可能かを判定する
                    if (CharaSkill.Is_attackable())
                    {
                        seAudioSource.PlayOneShot(seButtonOK);

                        CharaStatusPrinter.HideStatus();

                        //BattleVal.statusをスキル使用中に変更
                        CharaSkill.Destroy_Attackabletile();
                        CharaSkill.Destroy_Areatile();
                        BattleVal.status = STATUS.USESKILL;
                        //スキルボタンリスト消去
                        CharaSkill.Destory_SkillButtonList();

                        //マウスセルを戻す
                        mouse_x = BattleVal.selectX;
                        mouse_y = BattleVal.selectY;
                        Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);    
                    }
                    break;
            }

        }
        else
        {
            mouse_click = false;
        }
    }

    //キー入力待ち時間
    [SerializeField] const float interval_keypad_time = 0.15f;
    float delta_keypad_time = interval_keypad_time;
    //キーボード（パッド）によるマップ座標移動
    //視点のマップ座標はmouse_x,mouse_yに保管されている
    void GetKeypadPosition()
    {
        //一定時間待つ
        if(delta_keypad_time < interval_keypad_time)
        {
            delta_keypad_time += Time.deltaTime;
            return;
        }

        delta_keypad_time = 0;
        //もし前回の入力が初期値・例外なら
        if (mouse_x == -1)
        {
            //カメラの中心座標を前回入力に
            Vector3 temp = CameraAngle.NowCameraLook;
            Mapclass.TranslatePositionToMapCoord(temp, ref mouse_x, ref mouse_y);
        }

        int tempmouse_x = mouse_x;
        int tempmouse_y = mouse_y;

        //方向を加算
        /*
        if (Input.GetAxisRaw("Horizontal") > 0) mouse_x += CameraAngle.is_inverse_keyx;
        if (Input.GetAxisRaw("Horizontal") < 0) mouse_x -= CameraAngle.is_inverse_keyx;
        if (Input.GetAxisRaw("Vertical") > 0) mouse_y += CameraAngle.is_inverse_keyy;
        if (Input.GetAxisRaw("Vertical") < 0) mouse_y -= CameraAngle.is_inverse_keyy;
        */
        Vector2 keypadinput = new Vector2();
        if (Input.GetAxisRaw("Horizontal") > 0) keypadinput = CameraAngle.keypadxinput_vector2;
        if (Input.GetAxisRaw("Horizontal") < 0) keypadinput = -CameraAngle.keypadxinput_vector2;
        if (Input.GetAxisRaw("Vertical") > 0) keypadinput = CameraAngle.keypadyinput_vector2;
        if (Input.GetAxisRaw("Vertical") < 0) keypadinput = -CameraAngle.keypadyinput_vector2;

        mouse_x += (int)keypadinput.x;
        mouse_y += (int)keypadinput.y;

        //上限を超えるようなら前回のものに戻す
        if (!Mapclass.is_Inmap(mouse_x,mouse_y))
        {
            
            mouse_x = tempmouse_x;
            mouse_y = tempmouse_y;
        }
        else
        {
            //透明マスの場合
            if(BattleVal.mapdata[(int)MapdataList.MAPTEXTURE][mouse_y][mouse_x] == 0)
            {
                //mouse_x = tempmouse_x;
                //mouse_y = tempmouse_y;
            }
        }

        Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
        SelectTile.SetActive(true);

        //カメラの移動
        Vector3 newmousepos = new Vector3();
        Mapclass.TranslateMapCoordToPosition(ref newmousepos, mouse_x, mouse_y);
        CameraAngle.CameraPoint(newmousepos);

        //マウスオーバー系の処理
        ActMouseOver(tempmouse_x, tempmouse_y);
    }

    //右クリック・キャンセルボタン時の処理
    public void ActRightClick()
    {
        Vector3 newmousepos = new Vector3();

        switch (BattleVal.status)
        {
            //出撃選択時
            case STATUS.SETUP:
                //キーボードモード時
                if(!BattleVal.is_mouseinput)
                {
                    if (enemy_show_state != 0)
                    {
                        //詳細表示等消去処理
                        enemy_show_state = 0;
                        CharaStatusPrinter.Show_Enemy_Range(enemy_show_state);
                    }
                    /*
                    try
                    {
                        mouse_x = BattleVal.selectedUnit.x;
                        mouse_y = BattleVal.selectedUnit.y;
                        Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
                    }
                    catch(NullReferenceException ex)
                    {

                    }
                    */

                    if (CharaSetup.state == CharaSetup.CharaSetupStatus.MOVEDO)
                    {
                        seAudioSource.PlayOneShot(seButtonCancel);
                        CharaSetup.state = CharaSetup.CharaSetupStatus.MOVEEND;

                    }
                    else if(CharaSetup.state == CharaSetup.CharaSetupStatus.SET)
                    {
                        Mapclass.TranslatePositionToMapCoord(CameraAngle._CamParent.transform.position, ref mouse_x, ref mouse_y);
                        Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
                    }

                }
                break;
            //ユニット選択モード中ならば、「システムメニュー」を出す/消す
            case STATUS.PLAYER_UNIT_SELECT:
                seAudioSource.PlayOneShot(seButtonCancel);

                if (CharaSkill.skillbuttonlist.Count > 0)
                    CharaSkill.Destory_SkillButtonList();
                else if(enemy_show_state != 0)
                {
                    //詳細表示等消去処理
                    enemy_show_state = 0;
                    CharaStatusPrinter.Show_Enemy_Range(enemy_show_state);
                    CharaStatusPrinter.SetUp_DrawStatus();
                }
                else if(BattleVal.menuflag)
                {
                    BattleVal.menuflag = false;
                }
                else
                    BattleVal.sysmenuflag ^= true; //フラグの切り替え
                break;
            //移動モード中ならば、それを解除し、ユニット選択に戻る
            case STATUS.PLAYER_UNIT_MOVE:
                seAudioSource.PlayOneShot(seButtonCancel);

                CharaMove.Destroy_Movabletile();
                CharaStatusPrinter.SetUp_DrawStatus();
                //マウスセルを戻す
                mouse_x = BattleVal.selectX;
                mouse_y = BattleVal.selectY;
                Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
                //カメラの移動
                Mapclass.TranslateMapCoordToPosition(ref newmousepos, mouse_x, mouse_y);
                CameraAngle.CameraPoint(newmousepos);

                BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
                break;
            //攻撃モード中ならば、それを解除し、ユニット選択に戻る
            case STATUS.PLAYER_UNIT_ATTACK:
                seAudioSource.PlayOneShot(seButtonCancel);

                CharaAttack.Destroy_Attackabletile();
                CharaStatusPrinter.SetUp_DrawStatus();
                //マウスセルを戻す
                mouse_x = BattleVal.selectX;
                mouse_y = BattleVal.selectY;
                Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
                //カメラの移動
                Mapclass.TranslateMapCoordToPosition(ref newmousepos, mouse_x, mouse_y);
                CameraAngle.CameraPoint(newmousepos);

                BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
                break;
            //スキルモード中ならば、それを解除し、ユニット選択（スキルメニュー表示）に戻る
            case STATUS.PLAYER_UNIT_SKILL:
                seAudioSource.PlayOneShot(seButtonCancel);
                CharaStatusPrinter.HideTargetStatus();
                BattleVal.menuflag = true;
                CharaSkill.Destroy_Attackabletile();
                CharaSkill.Destroy_Areatile();
                CharaSkill.Setactive_SkillButtonList(true);
                //マウスセルを戻す
                mouse_x = BattleVal.selectX;
                mouse_y = BattleVal.selectY;
                Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
                //カメラの移動
                Mapclass.TranslateMapCoordToPosition(ref newmousepos, mouse_x, mouse_y);
                CameraAngle.CameraPoint(newmousepos);

                BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
                break;
        }

    }

    //エネミーのステートを繰るコルーチン
    IEnumerator EnemyCoroutine()
    {
        //コルーチンの複数生成を抑止
        if (ecoroutine_runflag)
            yield break;

        ecoroutine_runflag = true;
        //エネミーリストの作成
        List<Unitdata> enemylist = new List<Unitdata>();
        foreach (Unitdata enemy in BattleVal.unitlist)
        {

            if (enemy.team == 1)
            {
                enemylist.Add(enemy);
            }
        }

        foreach (Unitdata enemy in enemylist)
        {   

            //Selected Unitの更新
            BattleVal.selectedUnit = enemy;
            BattleVal.selectX = enemy.x;
            BattleVal.selectY = enemy.y;

            //移動可能性・攻撃可能性をリセット
            //enemy.movable = true;
            //enemy.atackable = true;

            //2回行動してたどり着ける範囲で敵を見つける
            int[] attacktarget = EnemyAI.SearchTraget(enemy);
            //Debug.Log(string.Format("TARGET:{0},{1}", attacktarget[0], attacktarget[1]));

            //ターゲットが居なければ
            if (attacktarget[0] == enemy.x && attacktarget[1] == enemy.y)  continue;
            //視点を移動
            Vector3 temp = new Vector3();
            Mapclass.TranslateMapCoordToPosition(ref temp, BattleVal.selectX, BattleVal.selectY);
            CameraAngle.CameraPoint(temp);
            CharaStatusPrinter.SetUp_DrawStatus();

            yield return new WaitForSeconds(1);

            while (enemy.atackable || enemy.movable)
            {
                //攻撃フェイズ
                if (enemy.atackable)
                {
                    //現在のキャラクターのHPの確認
                    //HPが1/8未満であれば自身に対して回復スキルの仕様を吟味
                    if (enemy.hp < enemy.status.maxhp / 8)
                    {
                        //回復スキルがあるかどうかの判定
                        Skill useone = null; //使用するスキル
                        foreach (Skill skill in enemy.skills)
                        {
                            if (skill.s_atk < 0 && skill.use > 0)
                            {
                                if (useone == null || Mathf.Abs(useone.s_atk) < Mathf.Abs(skill.s_atk))
                                    useone = skill; //使用する回復スキル(最も回復量が大きいものを使う)
                            }
                        }
                        //スキルを使うか否かの判定
                        if (useone != null)
                        {
                            //スキルを使う処理
                            CharaSkill.selectedskill = useone;
                            //敵の攻撃範囲取得処理
                            CharaSkill.Set_Attackablelist();
                            //疑似マウスオーバー処理
                            foreach(int[] pos in CharaSkill.attackablelist)
                            {
                                CharaSkill.Set_Attackarea(pos[0], pos[1]);
                                //攻撃可能範囲内の対象リストを取得
                                List<Unitdata> attackable_charalist = CharaSkill.Get_attackable_charalist();
                                //攻撃範囲内にキャラクターが存在するか否か
                                //存在
                                if (attackable_charalist.Count > 0)
                                {
                                    //自身が回復可能かを判定する
                                    if (EnemyAI.Is_Chara_in_List(attackable_charalist, enemy))
                                    {
                                        //回復対象をセット(戻り値は不要)
                                        CharaSkill.Is_attackable();
                                        CharaSkill.Is_attackableTile(pos[0], pos[1]);
                                        //攻撃タイルを描画
                                        CharaSkill.Show_Attackablelist();
                                        CharaSkill.Show_Attackarea();
                                        CharaStatusPrinter.Setup_DrawTargetStatus(enemy.x, enemy.y,
                                             CharaSkill.Calc_Damage(BattleVal.selectedUnit, enemy));

                                        yield return new WaitForSeconds(1);

                                        CharaStatusPrinter.HideStatus();
                                        CharaSkill.Destroy_Attackabletile();
                                        CharaSkill.Destroy_Areatile();
                                        //BattleVal.statusをスキル使用中に変更
                                        BattleVal.status = STATUS.USESKILL;
                                        enemy.atackable = false;

                                        //攻撃中はここで脱出
                                        while (BattleVal.status == STATUS.USESKILL)
                                        {
                                            yield return null;
                                        }
                                        //enemy.atackable = false; CharaSkill側で行っている
                                        break;
                                    }
                                    
                                }
                            }
                            
                        }

                    }
                    else
                    {
                        //回復スキルがあるかどうかの判定
                        Skill useone = null; //使用するスキル
                        foreach (Skill skill in enemy.skills)
                        {
                            if (skill.s_atk < 0 && skill.use > 0)
                            {
                                if (useone == null || Mathf.Abs(useone.s_atk) < Mathf.Abs(skill.s_atk))
                                    useone = skill; //使用する回復スキル(最も回復量が大きいものを使う)
                            }
                        }
                        //スキルを使うか否かの判定
                        if (useone != null)
                        {
                            //スキルを使う処理
                            CharaSkill.selectedskill = useone;
                            //敵の攻撃範囲取得処理
                            CharaSkill.Set_Attackablelist();
                            bool skill_used = false;
                            //疑似マウスオーバー処理
                            foreach (int[] pos in CharaSkill.attackablelist)
                            {
                                CharaSkill.Set_Attackarea(pos[0], pos[1]);
                                //攻撃可能範囲内の対象リストを取得
                                List<Unitdata> attackable_charalist = CharaSkill.Get_attackable_charalist();
                                //攻撃範囲内にキャラクターが存在するか否か
                                //存在
                                if (attackable_charalist.Count > 0)
                                {
                                    foreach(Unitdata temp_enemy in enemylist)
                                    {
                                        //敵(AI側)が回復可能かを判定する
                                        if (temp_enemy.hp >= temp_enemy.status.maxhp / 3) continue;
                                        if (EnemyAI.Is_Chara_in_List(attackable_charalist, temp_enemy))
                                        {
                                            //回復対象をセット(戻り値は不要)
                                            CharaSkill.Is_attackable();
                                            CharaSkill.Is_attackableTile(temp_enemy.x, temp_enemy.y);
                                            //攻撃タイルを描画
                                            CharaSkill.Show_Attackablelist();
                                            CharaSkill.Show_Attackarea();
                                            CharaStatusPrinter.Setup_DrawTargetStatus(temp_enemy.x, temp_enemy.y,
                                                 CharaSkill.Calc_Damage(BattleVal.selectedUnit, temp_enemy));

                                            yield return new WaitForSeconds(1);

                                            CharaStatusPrinter.HideStatus();
                                            CharaSkill.Destroy_Attackabletile();
                                            CharaSkill.Destroy_Areatile();
                                            //BattleVal.statusをスキル使用中に変更
                                            BattleVal.status = STATUS.USESKILL;
                                            enemy.atackable = false;

                                            //攻撃中はここで脱出
                                            while (BattleVal.status == STATUS.USESKILL)
                                            {
                                                yield return null;
                                            }
                                            //enemy.atackable = false; CharaSkill側で行っている
                                            skill_used = true;
                                            break;
                                        }
                                        
                                    }

                                }
                                if (skill_used) break;
                            }

                        }

                    }
                    
                    //まだ攻撃可能ならば
                    if(enemy.atackable)
                    {
                        //使用するスキルの決定：ターゲットが攻撃範囲に存在し、かつ威力のたかいものを選択する
                        Skill useone = null;
                        foreach (Skill skill in enemy.skills)
                        {
                            if (skill.use > 0 && skill.s_atk >= 0              //回復は除く
                                && !(skill.donot_moveafter && !enemy.movable)
                                && !(skill.is_mgc && !enemy.magicable)) 
                            {
                                //威力が高くて、攻撃対象が存在するものがあるか
                                if (useone == null || skill.s_atk > useone.s_atk)
                                {
                                    CharaSkill.selectedskill = skill;
                                    CharaSkill.Set_Attackablelist();
                                    //疑似マウスオーバー処理
                                    foreach (int[] pos in CharaSkill.attackablelist)
                                    {
                                        //攻撃エリアを取得
                                        CharaSkill.Set_Attackarea(pos[0], pos[1]);
                                        List<Unitdata> attackable_charalist = CharaSkill.Get_attackable_charalist();
                                        //スキル利用可能範囲にキャラクターがいる場合
                                        if (attackable_charalist.Count > 0)
                                        {
                                            //より威力の高いものが、ターゲットを含むかどうか判断（移動前）
                                            if (!enemy.movable)
                                            {
                                                //ターゲットに攻撃可能かを判定する
                                                if (EnemyAI.Is_Chara_in_List(attackable_charalist,
                                                    BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]))
                                                {
                                                    //使用スキル更新
                                                    useone = skill;
                                                }

                                            }
                                            //誰でもいいので攻撃対象がいるかどうかを判断（移動後）
                                            else
                                            {
                                                //使用スキル更新
                                                useone = skill;
                                            }
                                        }
                                    }
                                    
                                }
                            }
                        }

                        //スキルを優先する場合
                        if (enemy.routin.Priority_Of_Using_Skill)
                        {
                            //スキルが使用可能
                            if (useone != null)
                            {
                                CharaSkill.selectedskill = useone;
                                CharaSkill.Set_Attackablelist();
                                //疑似マウスオーバー処理
                                foreach(int[] pos in CharaSkill.attackablelist)
                                {
                                    //攻撃エリアを取得
                                    CharaSkill.Set_Attackarea(pos[0], pos[1]);
                                    List<Unitdata> attackable_charalist = CharaSkill.Get_attackable_charalist();
                                    //スキル利用可能範囲にキャラクターがいる場合
                                    if (attackable_charalist.Count > 0)
                                    {
                                        //移動前：ターゲットに攻撃可能なら攻撃する
                                        if (enemy.movable)
                                        {
                                            //ターゲットに攻撃可能かを判定する
                                            if (EnemyAI.Is_Chara_in_List(attackable_charalist,
                                                BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]))
                                            {
                                                //攻撃対象をセット(戻り値は不要)
                                                CharaSkill.Is_attackable();
                                                //攻撃タイルを描画
                                                CharaSkill.Show_Attackablelist();
                                                CharaStatusPrinter.Setup_DrawTargetStatus(attacktarget[0], attacktarget[1],
                                                     CharaSkill.Calc_Damage(BattleVal.selectedUnit,
                                                     BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]));

                                                yield return new WaitForSeconds(1);

                                                CharaStatusPrinter.HideStatus();
                                                CharaSkill.Destroy_Attackabletile();
                                                CharaSkill.Destroy_Areatile();
                                                //BattleVal.statusを戦闘中に変更
                                                BattleVal.status = STATUS.USESKILL;
                                                enemy.atackable = false;

                                                //攻撃中はここで脱出
                                                while (BattleVal.status == STATUS.USESKILL)
                                                {
                                                    yield return null;
                                                }
                                                // enemy.atackable = false; CharaSkill.Update側で行っている
                                                break;
                                            }
                                        }
                                        //移動後：誰でもいいので範囲にいたら攻撃する
                                        else
                                        {
                                            //攻撃対象をセット(戻り値は不要)
                                            CharaSkill.Is_attackable();
                                            //攻撃タイルを描画
                                            CharaSkill.Show_Attackablelist();
                                            if(CharaSkill.Is_attackableTile(pos[0], pos[1]))
                                                CharaStatusPrinter.Setup_DrawTargetStatus(pos[0], pos[1],
                                                     CharaSkill.Calc_Damage(BattleVal.selectedUnit,
                                                     BattleVal.id2index[string.Format("{0},{1}", pos[0], pos[1])]));

                                            yield return new WaitForSeconds(1);

                                            CharaStatusPrinter.HideStatus();
                                            CharaSkill.Destroy_Attackabletile();
                                            CharaSkill.Destroy_Areatile();
                                            //BattleVal.statusを戦闘中に変更
                                            BattleVal.status = STATUS.USESKILL;
                                            enemy.atackable = false;

                                            //攻撃中はここで脱出
                                            while (BattleVal.status == STATUS.USESKILL)
                                            {
                                                yield return null;
                                            }
                                            // enemy.atackable = false; CharaSkill.Update側で行っている

                                            break;
                                        }



                                    }
                                }
                                
                            }
                            //まだ攻撃を行ってない→スキルを使わなかった
                            //通常攻撃
                            if(enemy.atackable)
                            {
                                //敵の攻撃範囲取得処理
                                CharaAttack.Set_Attackablelist();
                                //攻撃可能範囲内の対象リストを取得
                                List<Unitdata> attackable_charalist = CharaAttack.Get_attackable_charalist();
                                //攻撃範囲内にキャラクターが存在するか否か
                                //存在
                                if (attackable_charalist.Count > 0)
                                {
                                    //ターゲットに攻撃可能かを判定する
                                    if (EnemyAI.Is_Chara_in_List(attackable_charalist,
                                        BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]))
                                    {
                                        //攻撃対象をセット
                                        CharaAttack.Set_attackedpos(attacktarget[0], attacktarget[1]);
                                        //攻撃タイルを描画
                                        CharaAttack.Show_Attackablelist();
                                        CharaStatusPrinter.Setup_DrawTargetStatus(attacktarget[0], attacktarget[1],
                                             CharaAttack.Calc_Damage(BattleVal.selectedUnit,
                                             BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]),
                                             true,
                                             CharaAttack.Calc_Dexterity(BattleVal.selectedUnit,
                                             BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]));

                                        yield return new WaitForSeconds(1);

                                        CharaStatusPrinter.HideStatus();
                                        CharaAttack.Destroy_Attackabletile();
                                        //BattleVal.statusを戦闘中に変更
                                        BattleVal.status = STATUS.BATTLE;
                                        enemy.atackable = false;

                                        //攻撃中はここで脱出
                                        while (BattleVal.status == STATUS.BATTLE)
                                        {
                                            yield return null;
                                        }
                                    }
                                    // enemy.atackable = false; Attack側で行っている
                                }
                            }
                            


                        }
                        //通常攻撃を優先する場合
                        else
                        {
                            //敵の攻撃範囲取得処理
                            CharaAttack.Set_Attackablelist();
                            //攻撃可能範囲内の対象リストを取得
                            List<Unitdata> attackable_charalist = CharaAttack.Get_attackable_charalist();
                            //攻撃範囲内にキャラクターが存在するか否か
                            //存在→攻撃可能
                            if (attackable_charalist.Count > 0)
                            {
                                //移動前：ターゲットに攻撃可能なら攻撃する
                                if(enemy.movable)
                                {
                                    //ターゲットに攻撃可能かを判定する
                                    if (EnemyAI.Is_Chara_in_List(attackable_charalist,
                                        BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]))
                                    {
                                        //攻撃対象をセット
                                        CharaAttack.Set_attackedpos(attacktarget[0], attacktarget[1]);
                                        //攻撃タイルを描画
                                        CharaAttack.Show_Attackablelist();
                                        CharaStatusPrinter.Setup_DrawTargetStatus(attacktarget[0], attacktarget[1],
                                             CharaAttack.Calc_Damage(BattleVal.selectedUnit,
                                             BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]),
                                             true,
                                             CharaAttack.Calc_Dexterity(BattleVal.selectedUnit,
                                             BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]));

                                        yield return new WaitForSeconds(1);

                                        CharaStatusPrinter.HideStatus();
                                        CharaAttack.Destroy_Attackabletile();
                                        //BattleVal.statusを戦闘中に変更
                                        BattleVal.status = STATUS.BATTLE;
                                        enemy.atackable = false;

                                        //攻撃中はここで脱出
                                        while (BattleVal.status == STATUS.BATTLE)
                                        {
                                            yield return null;
                                        }
                                    }
                                }
                                //移動後：誰でもいいので攻撃する
                                else
                                {
                                    Unitdata temptarget = attackable_charalist[(int)UnityEngine.Random.Range(0,attackable_charalist.Count)];
                                    //攻撃対象をセット
                                    CharaAttack.Set_attackedpos(temptarget.x, temptarget.y);
                                    //攻撃タイルを描画
                                    CharaAttack.Show_Attackablelist();
                                    CharaStatusPrinter.Setup_DrawTargetStatus(temptarget.x, temptarget.y,
                                             CharaAttack.Calc_Damage(BattleVal.selectedUnit,
                                             BattleVal.id2index[string.Format("{0},{1}", temptarget.x, temptarget.y)]),
                                             true,
                                             CharaAttack.Calc_Dexterity(BattleVal.selectedUnit,
                                             BattleVal.id2index[string.Format("{0},{1}", temptarget.x, temptarget.y)]));

                                    yield return new WaitForSeconds(1);

                                    CharaStatusPrinter.HideStatus();
                                    CharaAttack.Destroy_Attackabletile();
                                    //BattleVal.statusを戦闘中に変更
                                    BattleVal.status = STATUS.BATTLE;
                                    enemy.atackable = false;

                                    //攻撃中はここで脱出
                                    while (BattleVal.status == STATUS.BATTLE)
                                    {
                                        yield return null;
                                    }
                                }
                                // enemy.atackable = false; Attack側で行っている
                            }

                            //まだ攻撃を行っていない→通常攻撃をしなかった
                            //スキル攻撃
                            if(enemy.atackable)
                            {
                                //スキルが使用可能
                                if (useone != null)
                                {
                                    CharaSkill.selectedskill = useone;
                                    CharaSkill.Set_Attackablelist();
                                    //疑似マウスオーバー処理
                                    foreach (int[] pos in CharaSkill.attackablelist)
                                    {
                                        //攻撃エリアを取得
                                        CharaSkill.Set_Attackarea(pos[0], pos[1]);
                                        attackable_charalist = CharaSkill.Get_attackable_charalist();
                                        //スキル利用可能範囲にキャラクターがいる場合
                                        if (attackable_charalist.Count > 0)
                                        {
                                            //移動前：ターゲットに攻撃可能なら攻撃する
                                            if (enemy.movable)
                                            {
                                                //ターゲットに攻撃可能かを判定する
                                                if (EnemyAI.Is_Chara_in_List(attackable_charalist,
                                                    BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]))
                                                {
                                                    //攻撃対象をセット(戻り値は不要)
                                                    CharaSkill.Is_attackable();
                                                    //攻撃タイルを描画
                                                    CharaSkill.Show_Attackablelist();
                                                    CharaStatusPrinter.Setup_DrawTargetStatus(attacktarget[0], attacktarget[1],
                                                         CharaSkill.Calc_Damage(BattleVal.selectedUnit,
                                                         BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]));

                                                    yield return new WaitForSeconds(1);

                                                    CharaStatusPrinter.HideStatus();
                                                    CharaSkill.Destroy_Attackabletile();
                                                    CharaSkill.Destroy_Areatile();
                                                    //BattleVal.statusを戦闘中に変更
                                                    BattleVal.status = STATUS.USESKILL;
                                                    enemy.atackable = false;

                                                    //攻撃中はここで脱出
                                                    while (BattleVal.status == STATUS.USESKILL)
                                                    {
                                                        yield return null;
                                                    }
                                                    // enemy.atackable = false; CharaSkill.Update側で行っている
                                                    break;
                                                }
                                            }
                                            //移動後：誰でもいいので範囲にいたら攻撃する
                                            else
                                            {
                                                //攻撃対象をセット(戻り値は不要)
                                                CharaSkill.Is_attackable();
                                                //攻撃タイルを描画
                                                CharaSkill.Show_Attackablelist();
                                                CharaStatusPrinter.Setup_DrawTargetStatus(attacktarget[0], attacktarget[1],
                                                     CharaSkill.Calc_Damage(BattleVal.selectedUnit,
                                                     BattleVal.id2index[string.Format("{0},{1}", attacktarget[0], attacktarget[1])]));

                                                yield return new WaitForSeconds(1);

                                                CharaStatusPrinter.HideStatus();
                                                CharaSkill.Destroy_Attackabletile();
                                                CharaSkill.Destroy_Areatile();
                                                //BattleVal.statusを戦闘中に変更
                                                BattleVal.status = STATUS.USESKILL;
                                                enemy.atackable = false;

                                                //攻撃中はここで脱出
                                                while (BattleVal.status == STATUS.USESKILL)
                                                {
                                                    yield return null;
                                                }
                                                // enemy.atackable = false; CharaSkill.Update側で行っている

                                                break;
                                            }



                                        }
                                    }

                                }
                            }

                        }
                    }
                    
                    
                }
                Victory_or_Defeat();
                //ゲームオーバーフラグが立っている場合、動作を停止
                if (BattleVal.is_gameover)
                    yield break;
                //2度目の移動フェイズに行く前に脱出
                if (!enemy.movable) break;

                //攻撃をした場合ステータスを再表示
                if (!enemy.atackable)
                {
                    CharaStatusPrinter.SetUp_DrawStatus();
                    yield return new WaitForSeconds(1);
                }
                //移動フェイズ　
                if (enemy.movable)
                {
                    //行先を見つける(Dfs含む）
                    int[] target = EnemyAI.DecideDestination(enemy, attacktarget);
                    //ルート探索
                    List<int[]> root = new List<int[]>();
                    root = Mapclass.GetPath(BattleVal.mapdata, enemy.status.step,
                                                        enemy.x, enemy.y, target[0], target[1], enemy.status.jump);
                    CharaMove.movepath = root;
                    CharaMove.Initialize_Moving_Param();

                    //表示処理
                    CharaMove.Set_Movablelist();
                    CharaMove.Show_Movablelist();
                    yield return new WaitForSeconds(1);

                    //タイル消去
                    CharaMove.Destroy_Movabletile();
                    //Statusを移動中に
                    BattleVal.status = STATUS.MOVING;

                    enemy.movable = false;

                    //移動中はここで脱出
                    while (BattleVal.status == STATUS.MOVING)
                    {

                        yield return null;
                    }

                    //選択マスの更新
                    BattleVal.selectX = enemy.x;
                    BattleVal.selectY = enemy.y;
                }
            }


        }

        //全キャラをループしたので、ターンチェンジ
        //Debug.Log("turn change");
        BattleVal.status = STATUS.TURNEND;
        ecoroutine_runflag = false;
        yield break;
    }

    //1手戻し
    public void Resume_Action()
    {
        if(BattleVal.actions.Count != 0)
        {
            Action resume_act = BattleVal.actions.Pop();
            CharaMove.Change_Unit_Position(resume_act.unit, resume_act.follow_pos, resume_act.prev_pos);

            enemy_show_state = 0;
            CharaStatusPrinter.Show_Enemy_Range(enemy_show_state);
            CharaStatusPrinter.HideStatus();
            CharaSkill.Destory_SkillButtonList();

            //視点移動
            mouse_x = resume_act.unit.x;
            mouse_y = resume_act.unit.y;
            Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
            SelectTile.SetActive(true);
            Vector3 temp = new Vector3();
            Mapclass.TranslateMapCoordToPosition(ref temp, mouse_x, mouse_y);
            CameraAngle.CameraPoint(temp);

        }
    }

    //セーブボタンが押された時の関数
    public void SaveButton()
    {
        GameObject temp_object = (GameObject)Resources.Load("Prefab/SaveCanvas");
        save_canvas = Instantiate<Canvas>(temp_object.GetComponent<Canvas>());
        savemanager = save_canvas.GetComponent<SaveManager>();
        savemanager.SaveWindowOpen();
        EventSystem.current.SetSelectedGameObject(save_canvas.GetComponentsInChildren<Button>()[0].gameObject);
        BattleVal.status = STATUS.SAVE;
    }

    int sellect_num = 0;
    float quit_time = 0;
    bool mouse_use = false;
    //セーブを行う関数
    public void Main_Save()
    {
        //EventSystem.current.SetSelectedGameObject(null);
        if (EventSystem.current == null) return;
        
        PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
        eventDataCurrent.position = Input.mousePosition;
        List<RaycastResult> raycast = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrent, raycast);

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
            if (Input.GetAxisRaw("Vertical") > 0)
            {
                sellect_num = (sellect_num + (savemanager.save_load - 1)) % savemanager.save_load;
                mouse_use = false;
                quit_time = Time.time;
            }
            else if (Input.GetAxisRaw("Vertical") < 0)
            {
                sellect_num = (sellect_num + 1) % savemanager.save_load;
                quit_time = Time.time;
                mouse_use = false;
            }
            if (Input.GetAxisRaw("Horizontal") > 0)
            {
                quit_time = Time.time;
                savemanager.NextPageButton();
                return;
            }
            else if (Input.GetAxisRaw("Horizontal") < 0)
            {
                quit_time = Time.time;
                savemanager.PrevPageButton();
                return;
            }
        }
        //Debug.Log(savemanager.now_page);
        for (int i = 0; i < savemanager.save_load; i++)
        {
            savemanager.Show_Object[i].GetComponent<Image>().color = Color.white;
        }
        
        bool check = false;
        int temp_num = 0;
        foreach (RaycastResult tmp_result in raycast)
        {
                for (int i = 0; i < savemanager.save_load; i++)
                {
                    if (!int.TryParse(savemanager.Show_Object[i].gameObject.name, out temp_num)) break;
                    if (tmp_result.gameObject.transform.parent.gameObject.name == savemanager.Show_Object[i].gameObject.name)
                    {
                        check = true;
                        EventSystem.current.SetSelectedGameObject(savemanager.Show_Object[i].gameObject);
                        sellect_num = temp_num - 1;
                        mouse_use = true;
                        break;
                    }
                    else if (tmp_result.gameObject.name == savemanager.Show_Object[i].gameObject.name)
                    {
                        check = true;
                        EventSystem.current.SetSelectedGameObject(savemanager.Show_Object[i].gameObject);
                        sellect_num = temp_num - 1;
                        mouse_use = true;
                        break;
                    }
                }
                if (check) break;
        }
        GameObject tmp = savemanager.Show_Object[sellect_num % savemanager.save_load].gameObject;
        //savemanager.save_object[savemanager.now_page][sellect_num].GetComponent<Image>().color = Color.red;
        if (Input.GetButtonDown("Cancel"))
        {
            quit_time = 0;
            BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
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
            ActRightClick();
        }
        if (Input.GetButtonDown("Submit"))
        {
            if (EventSystem.current.currentSelectedGameObject == savemanager.prevpagebutton.gameObject || EventSystem.current.currentSelectedGameObject == savemanager.nextpagebutton.gameObject) return;
            quit_time = 0;
            BattleSave SD = new BattleSave();
            SD.Save();

            //--------------------------------------------------------------------------------------
            SD.current_message = string.Format("バトル「{5}」\n{0}/{1}/{2}/{3}:{4}", DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, SD.nameTitle);
            //---------------------------------------------------------------------------------------

            //Windows
            //FileStream fs = new FileStream(Application.streamingAssetsPath + "/SaveData/" + tmp.gameObject.name + ".json", FileMode.Create, FileAccess.Write);
            //Mac(Application.streamingAssetsPath is read only!)
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
            BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
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

    //ロードボタンが押された時の関数
    public void LoadButton()
    {
        GameObject temp_object = (GameObject)Resources.Load("Prefab/SaveCanvas");
        save_canvas = Instantiate<Canvas>(temp_object.GetComponent<Canvas>());
        savemanager = save_canvas.GetComponent<SaveManager>();
        savemanager.LoadWindowOpen();
        EventSystem.current.SetSelectedGameObject(save_canvas.GetComponentsInChildren<Button>()[0].gameObject);
        BattleVal.status = STATUS.LOAD;
    }
    //ロード関数
    public void Main_Load()
    {
        //EventSystem.current.SetSelectedGameObject(null);
        if (EventSystem.current == null) return;
        PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
        eventDataCurrent.position = Input.mousePosition;
        List<RaycastResult> raycast = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventDataCurrent, raycast);
        if (Input.GetButtonDown("Cancel"))
        {
            quit_time = 0;
            BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
            savemanager.save_BackGround.gameObject.SetActive(false);
            for (int j = 0; j < savemanager.save_load; j++)
            {
                Destroy(savemanager.Show_Object[j]);
            }
            savemanager.nextpagebutton.gameObject.SetActive(false);
            savemanager.prevpagebutton.gameObject.SetActive(false);
            savemanager.page_number.gameObject.SetActive(false);
            Destroy(save_canvas.GetComponent<CanvasScaler>());
            Destroy(save_canvas.GetComponent<GraphicRaycaster>());
            Destroy(save_canvas.gameObject);
            Destroy(save_canvas);
            ActRightClick();
        }
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
            for (int i = 0; i < savemanager.save_load; i++)
            {
                if (!int.TryParse(savemanager.Show_Object[i].gameObject.name, out temp_num)) break;
                if (tmp.gameObject.transform.parent.gameObject.name == savemanager.Show_Object[i].gameObject.name)
                {
                    check = true;
                    EventSystem.current.SetSelectedGameObject(savemanager.Show_Object[i].gameObject);
                    sellect_num = temp_num - 1;
                    mouse_use = true;
                    break;
                }
                else if (tmp.gameObject.name == savemanager.Show_Object[i].gameObject.name)
                {
                    check = true;
                    EventSystem.current.SetSelectedGameObject(savemanager.Show_Object[i].gameObject);
                    sellect_num = temp_num - 1;
                    mouse_use = true;
                    break;
                }
            }
            if (check) break;
        }
        //Debug.Log(sellect_num);
        //savemanager.save_object[savemanager.now_page][sellect_num].GetComponent<Image>().color = Color.red;
        if (Input.GetButtonDown("Submit"))
        {
            if (EventSystem.current.currentSelectedGameObject == savemanager.prevpagebutton.gameObject || EventSystem.current.currentSelectedGameObject == savemanager.nextpagebutton.gameObject) return;

            quit_time = 0;
            //Windows
            //FileStream fs = new FileStream(Application.streamingAssetsPath + "/SaveData/" + savemanager.Show_Object[sellect_num % savemanager.save_load].gameObject.name + ".json", FileMode.Open, FileAccess.Read);
            //Mac(streamingAssetsPath is readonly!)
            FileStream fs = new FileStream(Application.persistentDataPath + "/SaveData/" + savemanager.Show_Object[sellect_num % savemanager.save_load].gameObject.name + ".json", FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fs);
            string test = sr.ReadToEnd();
            //Debug.Log(test);
            //**注意**--------------------------------------------------------------------------------------------------------------------------------------------------------------
            Encryption_Config ec = Resources.Load<Encryption_Config>("Prefab/Encryption");
            //test = ec.DecryptionSystem(test,false);
            test = ec.DecryptionSystem(test, true); //serialize debug

            //**--------------------------------------------------------------------------------------------------------------------------------------------------------------------
            //Debug.Log(test);
            BattleSave sd = JsonUtility.FromJson<BattleSave>(test);

            //一時ファイルを作成
            //Windows
            //FileStream fs2 = new FileStream(Application.streamingAssetsPath + loadtemp, FileMode.Create, FileAccess.Write);
            //Mac
            FileStream fs2 = new FileStream(Application.persistentDataPath + loadtemp, FileMode.Create, FileAccess.Write);
            StreamWriter sw = new StreamWriter(fs2);
            //一時ファイルにセーブデータを書き込み
            sw.WriteLine(test);
            //sw.WriteLine(JsonUtility.ToJson(SD));
            sw.Flush();
            sw.Close();
            fs2.Close();

            nextscenename = sd.nameScene;

            //sd.OnAfterDeserialize();
            sd.Load();
            sr.Close();
            fs.Close();
            BattleVal.status = STATUS.FADEOUT;
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


    //効果音再生
    public void playSE()
    {
        seAudioSource.clip = seSetClip;
        seAudioSource.Play();
        seSetflag = false;
    }
    public static void setSE(AudioClip audioClip)
    {
        seSetClip = audioClip;
        seSetflag = true;
    }

    //特殊勝敗条件を判定　
    bool Check_Victory()
    {
        if (victory_condition != "")
        {
            if (Novel.GetComponent<IFCommand>().If_command(victory_condition))
            {
                return true;
            }
        }
        return false;
    }
    bool Check_Defeat()
    {
        if (defeat_condition != "")
        {
            if (Novel.GetComponent<IFCommand>().If_command(defeat_condition))
            {
                return true;
            }
        }
        return false;
    }

    void Victory_or_Defeat()
    {
        //相手陣営の全滅を判定
        if (UnitDefeat.Is_Totdestruct((BattleVal.turnplayer + 1) % 2))
        {
            if (BattleVal.turnplayer == 0)
                BattleVal.status = STATUS.STAGECLEAR;
            else
            {
                //シーン変化の割込みフラグをオンに。
                BattleVal.is_gameover = true;
            }
            return;

        }
        if (Check_Victory())
        {
            BattleVal.status = STATUS.STAGECLEAR;
            return;
        }
        if (Check_Defeat())
        {
            BattleVal.is_gameover = true;
            return;
        }
        return;
    }

    //獲得経験値を描画・レベルアップを表示する処理
    [Header("経験値獲得・レベルアップ演出")]
    public Text textGetExp;
    public Text textLevelup;
    public float timeShowExp;
    private float exp_popy = 50; 
    public float timeShowLevelup;
    public AudioClip seGetExp;
    public AudioClip seLevelup;
    //複数キャラに対応するようにしておく
    private static Dictionary<Unitdata, int> getExp = new Dictionary<Unitdata, int>();
    //追加する処理
    public static void AddGetExp(Unitdata unit, int exp)
    {
        getExp.Add(unit, exp);
    }
    //獲得した経験値を表示する処理
    private float exptimer = -1;
    
    private List<Text> listTextExpUI = new List<Text>(); //表示するUIのリスト
    private List<Text> listTextLevelupUI = new List<Text>();
    bool skillgetflag = false;
    string skillgetmessage = "";
    private void ShowGetExp()
    {
        //initialize
        if(exptimer == -1 && !CameraAngle.moveFlag)
        {
            listTextExpUI = new List<Text>();
            listTextLevelupUI = new List<Text>();
            skillgetflag = false;
            //経験値獲得処理と、ユニットの所にUIをInstaniate
            foreach (KeyValuePair<Unitdata,int> expPair in getExp)
            {
                //経験値を獲得する処理
                Unitdata unit = expPair.Key;
                int gainexp = expPair.Value;
                unit.status.exp += gainexp;
                //表示処理
                Vector3 pos = new Vector3();
                Mapclass.TranslateMapCoordToPosition(ref pos, unit.x, unit.y);
                pos = Camera.main.WorldToScreenPoint(pos) + new Vector3(0,1,0);
                Text temptext = Instantiate(textGetExp, pos, Quaternion.identity, canvas.transform);
                temptext.text = string.Format("Exp:{0}", gainexp);
                listTextExpUI.Add(temptext);

                //レベルアップ判定
                bool levelupflag = false;
                while (unit.status.exp >= unit.status.needexp)
                {
                    unit.status.exp -= unit.status.needexp;
                    unit.status.level++;
                    unit.hp = unit.status.maxhp; //回復

                    //スキル獲得判定
                    List<Skill> getskills = IsGetNewSkill(unit);
                    if(getskills.Count > 0)
                    {
                        skillgetmessage = string.Format("{0}が", unit.charaname);
                        foreach (Skill newskill in getskills)
                        {
                            newskill.use = newskill.maxuse;
                            unit.skills.Add(newskill);
                            skillgetmessage = string.Format("{0}「{1}」", skillgetmessage, newskill.skillname);
                        }
                        skillgetmessage = string.Format("{0}を習得した！", skillgetmessage);
                        skillgetflag = true;
                    }
                    levelupflag = true;
                }
                if (levelupflag)
                {
                    Text temptext2 = Instantiate(textLevelup, pos, Quaternion.identity, canvas.transform);
                    temptext2.gameObject.SetActive(false);
                    listTextLevelupUI.Add(temptext2);
                }
                
            }

            //効果音再生
            seAudioSource.clip = seGetExp;
            seAudioSource.Play();
            exptimer = 0;
        }
        //経験値表示を消していく処理
        else if(exptimer >= 0 && exptimer < timeShowExp)
        {
            float accelfact = 1.0f;
            //加速処理
            if (Input.GetButton("Submit")) accelfact = 2.0f;
            if (Input.GetButton("Cancel")) exptimer = timeShowExp;

            foreach (Text exptext in listTextExpUI)
            {
                exptext.color -= new Color(0, 0, 0, Time.deltaTime/timeShowExp * accelfact);
                exptext.transform.position += new Vector3(0, exp_popy * Time.deltaTime / timeShowExp * accelfact , 0);
            }
            exptimer += Time.deltaTime * accelfact;
        }
        //経験値表示処理終了
        else if(exptimer >= timeShowExp)
        {
            foreach(Text exptext in listTextExpUI)
            {
                Destroy(exptext.gameObject);
            }
            getExp.Clear();
            //レベルアップをする場合
            if(listTextLevelupUI.Count > 0)
            {
                foreach(Text leveluptext in listTextLevelupUI)
                {
                    leveluptext.gameObject.SetActive(true);
                }
                //効果音再生
                seAudioSource.clip = seLevelup;
                seAudioSource.Play();
                exptimer = -2; //-2から減算していく
            }
            else //終了
            {
                exptimer = -1;
                BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
            }
        }
        //レベルアップ表示を消していく処理
        else if(exptimer <= -2 && exptimer > -2 - timeShowLevelup)
        {
            float accelfact = 1.0f;
            //加速処理
            if (Input.GetButton("Submit")) accelfact = 2.0f;
            if (Input.GetButton("Cancel")) exptimer = -2 - timeShowLevelup;
            foreach (Text leveluptext in listTextLevelupUI)
            {
                leveluptext.color -= new Color(0, 0, 0, Time.deltaTime / timeShowLevelup * accelfact);
            }
            exptimer -= Time.deltaTime * accelfact;
        }
        //レベルアップ表示処理終了
        else if(exptimer < -2 - timeShowLevelup)
        {
            foreach (Text leveluptext in listTextLevelupUI)
            {
                Destroy(leveluptext.gameObject);
            }
            exptimer = -1;
            if(skillgetflag)
            {
                messageBox = Instantiate(prefabMesBox, canvas.transform).GetComponent<MessageBoxHundler>();
                messageBox.CreateMessageBox("スキル習得", skillgetmessage);
                skillgetflag = false;
                messageBoxflag = true;
            }
            BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
        }
    }

    List<Skill> IsGetNewSkill(Unitdata unit)
    {
        List<Skill> ans = new List<Skill>();
        foreach(SkillTree skilltree in unit.skillTrees)
        {
            //習得レベルに到達
            if (unit.status.level == skilltree.getlevel)
            {
                bool isnew = true;
                //すでに習得済みかを判定する
                foreach (Skill skill in unit.skills)
                {
                    if (skill.name == skilltree.skill.name)
                    {
                        isnew = false;
                        break;
                    }
                }
                if(isnew)
                    ans.Add(skilltree.skill);
            }
        }
        return ans;
    }

    //ユニット送り機能
    //ユニット選択モード時にNキー（Switch:Yボタン）
    //行動可能の仲間ユニットが居る場合そのキャラを選択する
    void UnitFeed()
    {
        if (BattleVal.status != STATUS.PLAYER_UNIT_SELECT) return;
        if (BattleVal.sysmenuflag) return;
        if (enemy_show_state != 0) return;

        if (Input.GetButtonDown("UnitFeed"))
        {
            //送るユニットのリスト
            List<Unitdata> feedunitlist = new List<Unitdata>();

            foreach (Unitdata unit in BattleVal.unitlist)
            {
                if (unit.team == 0 && (unit.movable || unit.atackable))
                {
                    feedunitlist.Add(unit);
                }
            }

            //
            if (feedunitlist.Count == 0)
            {
                seAudioSource.PlayOneShot(seButtonCancel);
                return;
            }

            int id = 0;
            //すでに未行動の味方ユニットを選択している
            if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", BattleVal.selectX, BattleVal.selectY)))
            {
                Unitdata tmpunit = BattleVal.id2index[string.Format("{0},{1}", BattleVal.selectX, BattleVal.selectY)];
                if (feedunitlist.Contains(tmpunit))
                {
                    id = feedunitlist.IndexOf(tmpunit); //tmp
                    //次のユニットに
                    if (id == feedunitlist.Count - 1) id = 0;
                    else id = id + 1;

                    
                }
            }

            CharaStatusPrinter.HideStatus();
            CharaSkill.Destory_SkillButtonList();

            BattleVal.selectedUnit = feedunitlist[id];
            BattleVal.selectX = mouse_x = feedunitlist[id].x;
            BattleVal.selectY = mouse_y = feedunitlist[id].y;
            Mapclass.DrawCharacter(SelectTile, mouse_x, mouse_y);
            seAudioSource.PlayOneShot(seButtonOK);
            Vector3 mousepos = new Vector3();
            Mapclass.TranslateMapCoordToPosition(ref mousepos, mouse_x, mouse_y);
            CameraAngle.CameraPoint(mousepos);
            CharaStatusPrinter.SetUp_DrawStatus();
        }
    }
}
