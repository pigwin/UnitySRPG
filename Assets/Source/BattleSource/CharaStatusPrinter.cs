using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//ステータス表示の状態
enum StatusPrinterState
{
    IDLE,
    SETUP,
    TARGETSETUP,
    TARGETHIDE,
    ANIMATION,
    HIDE,
    STATUSUPDATE,
    DETAILSETUP,
    DETAILHIDE
}


//UIの表示周りの制御
public class CharaStatusPrinter : MonoBehaviour {
    public GameObject unitStatusPanel;
    public Text unitStatusText;
    public Text unitNameText;
    public GameObject targetStatusPanel;
    public Text targetStatusText;
    public Text targetNameText;
    public Button unitMoveButton;
    public Button unitAttackButton;
    public Button unitSkillButton;
    public Button ResumeButton;
    public Sprite playerStatusPanelSprite;
    public Sprite enemyStatusPanelSprite;
    public GameObject commandPanel_Inspector;
    public Text commandText_Inspector;
    public static GameObject commandPanel;
    public static Text commandText;
    public GameObject DamageArrow;
    public Text DamagePredictText;
    public Text DexPredictText;
    public Text RatePredictText;
    public Text MapHeightText;

    public GameObject detailMenuGobj;
    public Image detailFace;
    public Text detailNameText;
    public Text detailLvText;
    public Text detailJobText;
    public Text detailStepText;
    public Text detailJumpText;
    public Text detailHpText;
    public Text detailExpText;
    public Text detailAtkText;
    public Text detailDefText;
    public Text detailMatText;
    public Text detailMdfText;
    public Text detailTecText;
    public Text detailLucText;

    public GameObject detailHpBar;
    public GameObject detailExpBar;
    public GameObject detailExpBarParent;

    public Color detailNormalHPTextColor = new Color(1, 1, 1, 1);
    public Color detailPinchHPTextColor = new Color(1, 0, 0, 1);

    public int HPBARWIDTH;
    public int EXPBARWIDTH;

    public GameObject prefabSkillPanel;
    public GameObject gobjSkillListContentParent;
    public Text textNoSkill;
    private List<GameObject> skillPanelList = new List<GameObject>();

    public static int predictdamage;
    public static float predictdex;
    public static float predictrate;
    public static bool predictdexflag;

    private static int targetx, targety;
    private Dictionary<GameObject,int> animvector = new Dictionary<GameObject, int>(); //進行方向係数（1……右向き　-1……左向き）
    //表示状態
    private static StatusPrinterState state= StatusPrinterState.HIDE;

    //表示アニメにかかる時間
    public float animationtime = 0.1f;

    //表示アニメ時にずらす距離
    public float animationwidth = 50;

    //アニメするオブジェクト
    private List<GameObject> AnimObjList = new List<GameObject>();
    //アニメした先のゴール座標保存
    private Dictionary<GameObject,Vector3> GoalPosList = new Dictionary<GameObject, Vector3>();

    //アニメーション現在時間
    float animnowtime = 0;

    //外部から呼び出し、IDLE状態からSETUP状態へ
    public static void SetUp_DrawStatus()
    {
        state = StatusPrinterState.SETUP;
    }

    //外部から呼び出し、メニュー系統を消す処理
    public static void HideStatus()
    {
        state = StatusPrinterState.HIDE;
    }
    //実体
    public void Hide_Status(bool is_buttononly=false)
    {
        BattleVal.menuflag = false;
        if(!is_buttononly) unitStatusPanel.SetActive(false);
        Hide_TaretStatus();
        detailMenuGobj.SetActive(false);

        Hide_Button();
        
    }
    public void Hide_Button()
    {
        unitMoveButton.gameObject.SetActive(false);
        unitAttackButton.gameObject.SetActive(false);
        unitSkillButton.gameObject.SetActive(false);
        CharaSkill.Setactive_SkillButtonList(false);
    }
    //外部から呼び出し、ターゲットステータス表示モードへ
    public static void Setup_DrawTargetStatus(int mousex, int mousey, int damage, bool dexflag = false, float dex = 0, float rate = 0)
    {
        targetx = mousex;
        targety = mousey;
        predictdamage = damage;
        if(dexflag) predictdex = dex;
        if(dexflag) predictrate = rate;
        predictdexflag = dexflag;
        state = StatusPrinterState.TARGETSETUP;
    }
    //外部から呼び出し、ターゲットステータス消去モードへ
    public static void HideTargetStatus()
    {
        state = StatusPrinterState.TARGETHIDE;
    }
    //実体
    public void Hide_TaretStatus()
    {
        targetStatusPanel.SetActive(false);
        DamageArrow.SetActive(false);
        DamagePredictText.gameObject.SetActive(false);
        DexPredictText.gameObject.SetActive(false);
        RatePredictText.gameObject.SetActive(false);

    }

    private void Start()
    {
        commandPanel = commandPanel_Inspector;
        commandText = commandText_Inspector;
    }

    // Update is called once per frame
    private void Update()
    {
        switch (state)
        {
            //非表示モード
            case StatusPrinterState.HIDE:
                //ステータス関連非表示処理
                Hide_Status();

                state = StatusPrinterState.IDLE;
                break;
            //通常のステータス表示・セットアップ
            case StatusPrinterState.SETUP:
                //初期化
                animnowtime = 0;
                Hide_Status(true);
                //表示内容のセットアップ
                Setup_CharaStatusPrinter();

                state = StatusPrinterState.ANIMATION;
                break;
            //攻撃・スキル使用時、対象のステータスを表示するセットアップ
            case StatusPrinterState.TARGETSETUP:
                //初期化
                animnowtime = 0;
                Setup_TargetStatusPrinter();

                state = StatusPrinterState.ANIMATION;
                break;
            //攻撃・スキル使用時の解除
            case StatusPrinterState.TARGETHIDE:
                Hide_TaretStatus();

                state = StatusPrinterState.IDLE;
                break;
            //キャラの詳細ステータス表示セットアップ
            case StatusPrinterState.DETAILSETUP:
                //初期化
                animnowtime = 0;
                Setup_Detail(BattleVal.selectedUnit);

                state = StatusPrinterState.ANIMATION;
                break;
            //キャラの詳細ステータス表示セットアップ
            case StatusPrinterState.DETAILHIDE:
                Hide_Detail();

                state = StatusPrinterState.IDLE;
                break;
            //アニメーションモード
            //AnimObjListに登録したGameObjectをアニメーションさせる
            case StatusPrinterState.ANIMATION:
                if (animnowtime >= animationtime)
                {
                    Reset_AnimPos();

                    state = StatusPrinterState.IDLE;
                }
                else
                {
                    foreach (GameObject obj in AnimObjList)
                    {
                        //obj.GetComponent<Image>().color += new Color(0, 0, 0, Time.deltaTime / animationtime);
                        obj.GetComponent<CanvasGroup>().alpha += Time.deltaTime / animationtime;
                        obj.transform.position += new Vector3(animvector[obj] * animationwidth * Time.deltaTime / animationtime, 0);
                    }
                    animnowtime += Time.deltaTime;
                }

                break;
            //待機状態
            //BattleVal.menuflagを監視、メニューの瞬間表示を行う
            case StatusPrinterState.IDLE:
                //瞬間表示
                if (BattleVal.menuflag)
                {
                    Show_Menu();
                }
                else
                {
                    Hide_Button();
                }
                break;
            //選択キャラステータスのアップデート
            //敵攻撃によって体力が変動した場合等
            //StatusUpdate()呼出しでこの状態になる
            case StatusPrinterState.STATUSUPDATE:
                if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", BattleVal.selectX, BattleVal.selectY)))
                {
                    Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", BattleVal.selectX, BattleVal.selectY)];
                    unitNameText.text = string.Format("{0}", temp.charaname);
                    unitStatusText.text = string.Format("Lv : {0}\nHP : {1}／{2}\nSTEP : {3} JUMP : {4}",
                                                     temp.status.level, temp.hp, temp.status.maxhp, temp.status.step, temp.status.jump);
                }
                state = StatusPrinterState.IDLE;

                break;
        }

        //1手戻し可能か
        if (BattleVal.actions.Count != 0 && BattleVal.status==STATUS.PLAYER_UNIT_SELECT)
        {
            ResumeButton.gameObject.SetActive(true);
        }
        else
        {
            ResumeButton.gameObject.SetActive(false);
        }

        //選択マスの高さを表示
        if(BattleVal.status != STATUS.GAMEOVER && BattleVal.status != STATUS.STAGECLEAR && BattleVal.status != STATUS.FADEOUT)
            MapHeightPrinter();
    }

    //Setup処理
    public void Setup_CharaStatusPrinter()
    {
        //初期化
        if(AnimObjList.Count > 0)
        {
            Reset_AnimPos();
        }
        AnimObjList.Clear();
        GoalPosList.Clear();
        animvector.Clear();

        //ステータス表示処理
        if(!unitStatusPanel.activeSelf)
        {
            unitStatusPanel.SetActive(true);
            AnimObjList.Add(unitStatusPanel);
            animvector.Add(unitStatusPanel, 1);
        }

        Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", BattleVal.selectX, BattleVal.selectY)];
        unitNameText.text = string.Format("{0}", temp.charaname);
        unitStatusText.text = string.Format("Lv : {0}\nHP : {1}／{2}\nSTEP : {3} JUMP : {4}",
                                         temp.status.level, temp.hp, temp.status.maxhp, temp.status.step, temp.status.jump);

        //ユニットパネルの画像・色選択
        if (temp.team == 0)
        {
            unitStatusPanel.GetComponent<Image>().sprite = playerStatusPanelSprite;
            unitNameText.GetComponent<GradationController>().colorTop = new Color(0.5f, 1, 1);
        }
        else
        {
            unitStatusPanel.GetComponent<Image>().sprite = enemyStatusPanelSprite;
            unitNameText.GetComponent<GradationController>().colorTop = new Color(1, 0.5f, 0.5f);
        }

        if (BattleVal.status == STATUS.PLAYER_UNIT_SELECT && temp.team == BattleVal.turnplayer)
        {
            //メニューボタン表示
            BattleVal.menuflag = true;

        }

        //メニューの表示非表示
        if (BattleVal.menuflag)
        {
            Show_Menu(true);
        }

        //アニメーションセットアップ
        Setup_Animation();
    }

    //Setup処理
    public void Setup_TargetStatusPrinter()
    {
        //初期化
        if (AnimObjList.Count > 0)
        {
            Reset_AnimPos();
        }
        AnimObjList.Clear();
        GoalPosList.Clear();
        animvector.Clear();


        //ステータス表示処理
        if (!targetStatusPanel.activeSelf)
        {
            targetStatusPanel.SetActive(true);
            AnimObjList.Add(targetStatusPanel);
            animvector.Add(targetStatusPanel, -1);

        }
        //ダメージ予想表示
        DamageArrow.SetActive(true);
        DamagePredictText.gameObject.SetActive(true);

        AnimObjList.Add(DamageArrow);
        animvector.Add(DamageArrow, 1);

        Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", targetx, targety)];
        targetNameText.text = string.Format("{0}", temp.charaname);
        targetStatusText.text = string.Format("Lv : {0}\nHP : {1}／{2}\nSTEP : {3} JUMP : {4}",
                                         temp.status.level, temp.hp, temp.status.maxhp, temp.status.step, temp.status.jump);

        //ヒット率・クリティカル率表示
        if(predictdexflag)
        {
            DexPredictText.text = string.Format("Hit:{0}%", (int)(predictdex * 100));
            RatePredictText.text = string.Format("Critical:{0}%", (int)(predictrate * 100));
            DexPredictText.gameObject.SetActive(true);
            RatePredictText.gameObject.SetActive(true);
        }

        //ユニットパネルの画像・色選択
        if (temp.team == 0)
        {
            targetStatusPanel.GetComponent<Image>().sprite = playerStatusPanelSprite;
            targetNameText.GetComponent<GradationController>().colorTop = new Color(0.5f, 1, 1);
        }
        else
        {
            targetStatusPanel.GetComponent<Image>().sprite = enemyStatusPanelSprite;
            targetNameText.GetComponent<GradationController>().colorTop = new Color(1, 0.5f, 0.5f);
        }

        //ダメージ予想の色
        if(predictdamage >= 0)
        {
            DamageArrow.GetComponent<GradationController>().colorTop = new Color(1, 0, 0, 0.7f);
            DamageArrow.GetComponent<GradationController>().colorBottom = new Color(1, 1, 0, 0.7f);
            DamagePredictText.GetComponent<GradationController>().colorTop = new Color(1, 0, 0);
            DamagePredictText.GetComponent<GradationController>().colorBottom = new Color(1, 0.6549f, 0.2235f);
            DamagePredictText.text = string.Format("Damage:{0}", Mathf.Abs(predictdamage));
        }
        else  //回復
        {
            DamageArrow.GetComponent<GradationController>().colorTop = new Color(0, 1, 0, 0.7f);
            DamageArrow.GetComponent<GradationController>().colorBottom = new Color(0, 1, 1, 0.7f);
            DamagePredictText.GetComponent<GradationController>().colorTop = new Color(0, 1, 0);
            DamagePredictText.GetComponent<GradationController>().colorBottom = new Color(0.7843f, 1, 0.2235f);
            DamagePredictText.text = string.Format("Heal:{0}", Mathf.Abs(predictdamage));
        }
        //アニメーションセットアップ
        Setup_Animation();
    }

    //ポジションリセット
    public void Reset_AnimPos()
    {
        foreach (GameObject obj in AnimObjList)
        {
            //Color temp = obj.GetComponent<Image>().color;
            //temp.a = 1;
            //obj.GetComponent<Image>().color = temp;
            obj.GetComponent<CanvasGroup>().alpha = 1;
            if (GoalPosList.ContainsKey(obj))
                obj.transform.position = GoalPosList[obj];
        }
    }

    //アニメーションのセットアップ
    public void Setup_Animation()
    {

        foreach (GameObject obj in AnimObjList)
        {
            //フェードイン準備
            //Color temp = obj.GetComponent<Image>().color;
            //temp.a = 0;
            //obj.GetComponent<Image>().color = temp;
            obj.GetComponent<CanvasGroup>().alpha = 0;
            //スライドイン準備
            GoalPosList.Add(obj,obj.transform.position);
            obj.transform.position -= new Vector3(animvector[obj] * animationwidth,0);
        }
        
    }

    //メニューボタン表示処理
    public void Show_Menu(bool isanim=false)
    {
        //行動可能か
        if (BattleVal.selectedUnit.movable)
        {
            unitMoveButton.gameObject.SetActive(true);
            if (isanim) AnimObjList.Add(unitMoveButton.gameObject);
            if (isanim) animvector.Add(unitMoveButton.gameObject, 1);
        }

        //攻撃可能か
        if (BattleVal.selectedUnit.atackable)
        {
            unitAttackButton.gameObject.SetActive(true);
            if (isanim) AnimObjList.Add(unitAttackButton.gameObject);
            if (isanim) animvector.Add(unitAttackButton.gameObject, 1);
            //スキルを持っているか
            if (BattleVal.selectedUnit.skills.Count > 0)
            {
                unitSkillButton.gameObject.SetActive(true);
                if (isanim) AnimObjList.Add(unitSkillButton.gameObject);
                if (isanim) animvector.Add(unitSkillButton.gameObject, 1);
            }
        }
    }

    //キャラの詳細ステータス表示
    public void Setup_Detail(Unitdata chara)
    {
        //初期化
        if (AnimObjList.Count > 0)
        {
            Reset_AnimPos();
        }
        AnimObjList.Clear();
        GoalPosList.Clear();
        animvector.Clear();

        //メニュー系のテキスト設定
        detailNameText.text = chara.charaname;
        detailJobText.text = chara.jobname;
        detailLvText.text = chara.status.level.ToString();
        detailHpText.text = string.Format("{0}／{1}", chara.hp, chara.status.maxhp);
        if ((float)chara.hp / (float)chara.status.maxhp < 0.125f)
            detailHpText.color = detailPinchHPTextColor;
        else
            detailHpText.color = detailNormalHPTextColor;
        detailHpBar.GetComponent<RectTransform>().sizeDelta
            = new Vector2((float)chara.hp / (float)chara.status.maxhp * HPBARWIDTH, detailHpBar.GetComponent<RectTransform>().sizeDelta.y);
        //味方のみ経験値表示
        if (chara.team != 0) detailExpBarParent.SetActive(false);
        else
        {
            detailExpBarParent.SetActive(true);
            //LvMAX判定
            if(chara.status.level >= 99)
            {
                chara.status.level = 99; //メモリ改ざんとか対策
                detailExpText.text = string.Format("ＭＡＸ");
                detailExpText.color = detailPinchHPTextColor;
                detailExpBar.GetComponent<RectTransform>().sizeDelta
                    = new Vector2(EXPBARWIDTH, detailExpBar.GetComponent<RectTransform>().sizeDelta.y);
            }
            else
            {
                detailExpText.text = string.Format("あと{0}", chara.status.needexp - chara.status.exp);
                detailExpText.color = detailNormalHPTextColor;
                detailExpBar.GetComponent<RectTransform>().sizeDelta
                    = new Vector2((float)chara.status.exp / (float)chara.status.needexp * EXPBARWIDTH, detailExpBar.GetComponent<RectTransform>().sizeDelta.y);
            }
        }
        detailStepText.text = chara.status.step.ToString();
        detailJumpText.text = chara.status.jump.ToString();
        detailAtkText.text = chara.status.atk.ToString();
        detailDefText.text = chara.status.def.ToString();
        detailMatText.text = chara.status.mat.ToString();
        detailMdfText.text = chara.status.mdf.ToString();
        detailTecText.text = chara.status.tec.ToString();
        detailLucText.text = chara.status.luc.ToString();
        detailFace.sprite = chara.faceimage;

        //スキルリストの設定
        //Initialize
        foreach(GameObject temp in skillPanelList)
        {
            Destroy(temp);
        }
        skillPanelList.Clear();
        //スキルパネルの登録
        foreach(Skill skill in chara.skills)
        {
            GameObject temp = Instantiate(prefabSkillPanel, gobjSkillListContentParent.transform);
            temp.GetComponent<SkillPanel>().SetSkill(skill);
            skillPanelList.Add(temp);
        }
        if (skillPanelList.Count == 0)
            textNoSkill.gameObject.SetActive(true);
        else
            textNoSkill.gameObject.SetActive(false);

        detailMenuGobj.SetActive(true);

        AnimObjList.Add(detailMenuGobj);
        animvector.Add(detailMenuGobj, 1);

        //アニメーションセットアップ
        Setup_Animation();
    }

    //敵キャラの移動範囲や攻撃範囲・スキル範囲を表示する、あるいは消去する
    //state : 0->消去　1->詳細表示 2->移動範囲　3->攻撃範囲
    public static void Show_Enemy_Range(int showstate)
    {
        Delete_Enemy_Range();
        switch(showstate)
        {
            case 0:
                state = StatusPrinterState.DETAILHIDE;
                break;
            case 1:
                state = StatusPrinterState.DETAILSETUP;
                break;
            case 2:
                state = StatusPrinterState.DETAILHIDE;
                commandText.text = "移動";
                commandPanel.SetActive(true);
                CharaMove.Set_Movablelist();
                CharaMove.Show_Movablelist();
                break;
            case 3:
                commandText.text = "攻撃";
                commandPanel.SetActive(true);
                CharaAttack.Set_Attackablelist();
                CharaAttack.Show_Attackablelist();
                break;
            default:    //スキル
                Skill tempskill = BattleVal.selectedUnit.skills[showstate - 4]; //stateが3で0番目スキル
                commandText.text = tempskill.skillname;
                commandPanel.SetActive(true);
                CharaSkill.selectedskill = tempskill;
                CharaSkill.Set_Attackablelist();
                CharaSkill.Show_Attackablelist();
                break;
        }
    }

    public static void Delete_Enemy_Range()
    {
        commandPanel.SetActive(false);
        CharaMove.Destroy_Movabletile();
        CharaAttack.Destroy_Attackabletile();
        CharaSkill.Destroy_Attackabletile();
        CharaSkill.Destroy_Areatile();
    }

    public void Hide_Detail()
    {
        detailMenuGobj.SetActive(false);
    }

    //selectX, selectYの高さを表示
    public void MapHeightPrinter()
    {
        if(BattleVal.selectX != -1)
            MapHeightText.text = string.Format("{0}h",BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][BattleVal.selectY][BattleVal.selectX]);
    }

    //ステータスアップデート登録
    public static void StatusUpdate()
    {
        state = StatusPrinterState.STATUSUPDATE;
    }
}
