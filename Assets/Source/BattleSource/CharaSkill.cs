using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/************************************************/
/*キャラクターのスキル利用時の処理を行うクラス  */
/************************************************/
public class CharaSkill : MonoBehaviour {
    //攻撃可能範囲を表すタイル
    public static GameObject attackabletile;
    public GameObject attackabletile_inspector;
    //攻撃エリアを表すタイル
    public static GameObject attackareatile;
    public GameObject attackareatile_inspector;
    //回復可能範囲を表すタイル
    public static GameObject healabletile;
    public GameObject healabletile_inspector;
    //回復エリアを表すタイル
    public static GameObject healareatile;
    public GameObject healareatile_inspector;

    //スキル名表示
    public GameObject skillnamePanel;
    public Text skilltext;

    //ボタン
    public Button unitSkillButton; //Instantiate元
    public static List<Button> unitSkillButtons;
    public AudioClip seButtonOK;

    //描画したタイルを格納する配列
    public static List<GameObject> tilelist = new List<GameObject>();
    //範囲攻撃時の攻撃エリアを表すタイル
    public static List<GameObject> areatilelist = new List<GameObject>();
    //攻撃可能範囲
    public static List<int[]> attackablelist;
    //攻撃エリア（範囲攻撃)
    public static List<int[]> attackkingarea;
    //スキルボタンリスト
    public static List<Button> skillbuttonlist = new List<Button>();

    //スキルボタンリストを表示（ボタン作成）
    public void ShowSkill()
    {
        if (skillbuttonlist.Count > 0) return;
        const int BUTTONX0 = 130;
        const int BUTTONY0 = -150;
        const int BUTTONW0 = 200;
        const int BUTTONW = 400;
        const int BUTTONH = 50;
        const int DX = 0;
        const int DY = 0;

        int i = 0;
        foreach(Skill skill in BattleVal.selectedUnit.skills)
        {
            Button tmp = Instantiate(unitSkillButton);
            tmp.transform.SetParent(canvas.transform, false);
            //座標計算
            int tmpx = BUTTONX0 + ((BUTTONW0+BUTTONW)/2 + DX);
            int tmpy = BUTTONY0 - i * (BUTTONH + DY);
            tmp.transform.localPosition = new Vector3(-canvas.GetComponent<RectTransform>().sizeDelta.x / 2, canvas.GetComponent<RectTransform>().sizeDelta.y / 2, 0);
            tmp.transform.localPosition += new Vector3(tmpx,tmpy);
            tmp.transform.Find("SkillText").GetComponent<Text>().text
               = string.Format("{0}",skill.skillname);
            tmp.transform.Find("CountText").GetComponent<Text>().text
                = string.Format("{0}回／{1}回", skill.use, skill.maxuse);
            if (skill.use == 0) tmp.interactable = false;
            tmp.onClick.AddListener(() => SkillHundle(skill));
            skillbuttonlist.Add(tmp);
            i++;
        }
    }

    //スキルボタンリストを消去する関数
    public static void Destory_SkillButtonList()
    {
        foreach(Button tmp in skillbuttonlist)
        {
            Destroy(tmp.gameObject);
        }
        skillbuttonlist.Clear();
    }
    //スキルボタンリストを非表示・表示する関数
    public static void Setactive_SkillButtonList(bool flag)
    {
        foreach (Button tmp in skillbuttonlist)
        {
            tmp.gameObject.SetActive(flag);
        }
    }

    //選択されたスキル
    public static Skill selectedskill;

    //スキルの実体(onclick)
    public void SkillHundle(Skill unitskill)
    {
        selectedskill = unitskill;
        //攻撃可能範囲取得
        Set_Attackablelist();
        //攻撃可能範囲の描画
        Show_Attackablelist();
        //効果範囲の初期化
        attackkingarea = new List<int[]>();
        //Battleval.statusの更新
        BattleVal.status = STATUS.PLAYER_UNIT_SKILL;
        //スキルリストを一時的に隠す
        Setactive_SkillButtonList(false);
        //メニューボタン非表示に
        BattleVal.menuflag = false;
        Operation.setSE(seButtonOK);
    }

    //attackablelistをセットするだけ
    public static void Set_Attackablelist()
    {
        //移動可能範囲の初期化
        attackablelist = new List<int[]>();
        //移動可能範囲を取得
        attackablelist = selectedskill.attackrange.Attackablelist(BattleVal.mapdata,
            BattleVal.selectedUnit.x, BattleVal.selectedUnit.y);
        //attackablelist = Mapclass.AttackDfs(BattleVal.mapdata, BattleVal.selectX, BattleVal.selectY, attackrange[selectedskill.rangeid], AttackMaxRange(attackrange[selectedskill.rangeid]));
    }
    //attackablelistを表示するだけ
    public static void Show_Attackablelist()
    {
        //表示処理
        foreach (int[] mappos in attackablelist)
        {
            GameObject tmptile;
            //回復かどうか
            if (selectedskill.s_atk < 0)
            {
                tmptile = Instantiate(healabletile);
            }
            else
            {
                tmptile = Instantiate(attackabletile);
            }
            tilelist.Add(tmptile);
            Mapclass.DrawCharacter(tilelist[tilelist.Count - 1], mappos[0], mappos[1]);
        }
    }

    //マウスオーバー判定
    public static bool Is_OverAttackable(int mousex, int mousey)
    {
        foreach (int[] mappos in attackablelist)
        {
            if (mappos[0] == mousex && mappos[1] == mousey) return true;
        }

        return false;
    }

    //スキルタイルマウスオーバー時の処理 
    //スキル効果範囲のセット
    public static void Set_Attackarea(int mousex, int mousey)
    {
        //効果範囲の初期化
        attackkingarea = new List<int[]>();
        //効果範囲を取得

        if (selectedskill.arearange.BaseRange.type != -1)
            attackkingarea = selectedskill.arearange.Attackablelist(BattleVal.mapdata,
                mousex, mousey);
        attackkingarea.Add(new int[] { mousex, mousey });

        skillpos = new int[] { mousex, mousey };
    }
    //攻撃エリアの描画
    public static void Show_Attackarea()
    {
        //消去処理
        Destroy_Areatile();
        //表示処理
        foreach (int[] mappos in attackkingarea)
        {
            GameObject tmptile;
            //回復かどうか
            if (selectedskill.s_atk < 0)
            {
                tmptile = Instantiate(healareatile);
            }
            else
            {
                tmptile = Instantiate(attackareatile);
            }
            areatilelist.Add(tmptile);
            Mapclass.DrawCharacter(areatilelist[areatilelist.Count - 1], mappos[0], mappos[1]);
        }
    }

    //攻撃可能タイルの消去処理
    public static void Destroy_Attackabletile()
    {
        foreach (GameObject tile in tilelist)
        {
            Destroy(tile);
        }
        tilelist.Clear();
    }

    //攻撃エリアタイルの消去処理
    public static void Destroy_Areatile()
    {
        foreach (GameObject tile in areatilelist)
        {
            Destroy(tile);
        }
        areatilelist.Clear();
    }

    //AttackingAreaからAttackedPosへと攻撃先を登録する
    //戻り値：呼び出し時点での攻撃エリアに敵が1体以上いるか
    public static bool Is_attackable()
    {
        attackedposlist.Clear();
        foreach (int[] mappos in attackkingarea)
        {
            //攻撃可能範囲内か
            if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", mappos[0], mappos[1])))
            {
                Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", mappos[0], mappos[1])];
                //攻撃対象・回復対象か
                if ((temp.team != BattleVal.selectedUnit.team && selectedskill.s_atk > 0)
                    || (temp.team == BattleVal.selectedUnit.team && selectedskill.s_atk < 0))
                {
                    //攻撃先を登録
                    Set_attackedpos(mappos[0], mappos[1]);
                }
            }
        }

        if (attackedposlist.Count > 0) return true;
        return false;
    }

    /// <summary>
    /// 攻撃対象かを判定
    /// 
    /// </summary>
    /// <param name="mapx">skillpos[0]</param>
    /// <param name="mapy">skillpos[]</param>
    /// <returns>キャラが存在するかどうか</returns>
    public static bool Is_attackableTile(int mapx, int mapy)
    {
        //キャラが存在しているか
        if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", mapx, mapy)))
        {
            Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", mapx, mapy)];
            //攻撃対象・回復対象か
            if ((temp.team != BattleVal.selectedUnit.team && selectedskill.s_atk > 0)
                    || (temp.team == BattleVal.selectedUnit.team && selectedskill.s_atk < 0))
            {
                skillpos = new int[] {mapx, mapy};
                return true;
            }
        }

        return false;
        
    }

    //攻撃先を登録する関数
    public static void Set_attackedpos(int target_x, int target_y)
    {
        //攻撃先を登録
        attackedposlist.Add(new int[] { target_x, target_y });
    }

    //攻撃可能エリアの返却
    public static List<Unitdata> Get_attackable_charalist()
    {
        //返却値
        List<Unitdata> ans = new List<Unitdata>();

        foreach (int[] mappos in attackkingarea)
        {
            //攻撃可能範囲内か
            if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", mappos[0], mappos[1])))
            {
                Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", mappos[0], mappos[1])];
                //攻撃対象・回復対象か
                if ((temp.team != BattleVal.selectedUnit.team && selectedskill.s_atk > 0)
                    || (temp.team == BattleVal.selectedUnit.team && selectedskill.s_atk < 0))
                {
                    ans.Add(temp);
                }
            }
        }
        return ans;
    }

    //ダメージを返却する関数
    public static int Calc_Damage(Unitdata Attack, Unitdata Defence)
    {
        int damage = 0;
        //ダメージ計算式
        //MATフラグで使用するパラメータを変更すること。
        if (selectedskill.s_atk > 0)
        {
            //魔法扱い
            if(selectedskill.is_mgc)
            {
                //防御無視
                if(selectedskill.is_ignoredef)
                    damage = Attack.status.mat;
                else
                    damage = Attack.status.mat - Defence.status.mdf;
            }
            else
            {
                //防御無視
                if (selectedskill.is_ignoredef)
                    damage = Attack.status.atk;
                else
                    damage = Attack.status.atk - Defence.status.def;
            }
            //スキル倍率
            damage = (int)(damage * selectedskill.s_atk);
            //負の場合等の例外処理
            if (damage < 0) damage = 1;
        }
        else
        {
            //回復
            if(selectedskill.is_mgc)
                damage = (int)(selectedskill.s_atk * Attack.status.mat/2);
            else
                damage = (int)(selectedskill.s_atk * Attack.status.atk/2);
        }

        return damage;
    }

    //攻撃対象
    private static List<int[]> attackedposlist = new List<int[]>();
    //スキルエフェクト発生場所
    private static int[] skillpos = new int[2];
    //攻撃ステート
    private BATTLE_STATUS bstate = BATTLE_STATUS.SETVECT;
    //ダメージ文字（1桁あたり）
    public Text damagetextprefab;
    //実際に表示する数値
    public static List<List<Text>> damagenum = new List<List<Text>>();
    //バトル演出時間
    private float battle_time = 1.0f;
    private static float nowtime;
    public Canvas canvas;

    //Start
    private void Start()
    {
        attackabletile = attackabletile_inspector;
        attackareatile = attackareatile_inspector;
        healabletile = healabletile_inspector;
        healareatile = healareatile_inspector;
        skillnamePanel.SetActive(false);
    }

    //撃墜処理のエフェクト
    public GameObject defeatEffect;
    private List<Animator> attackedAnimator = new List<Animator>(); //防御側のアニメーター保存
    private List<string> attackedanimstate = new List<string>();
    private List<CharaAnimation> attackedCharaAnimation = new List<CharaAnimation>(); //効果音再生音源をキャラクター分確保する
    public AudioClip seDamage;
    public AudioClip seHeal;
    private GameObject hitEffect;
    public string hitAnimName = "Damage";
    public string healAnimName = "Idle";

    private GameObject gobjCutin;
    //Update
    private void Update()
    {
        //戦闘中
        if (BattleVal.status == STATUS.USESKILL)
        {
            int damage = 0;
            switch (bstate)
            {
                case BATTLE_STATUS.SETVECT:
                    //キャラの向き調整；攻撃元が発動場所を向く
                    Vector3 r0 = new Vector3(); //攻撃元
                    Vector3 rSkill = new Vector3();
                    Mapclass.TranslateMapCoordToPosition(ref r0, BattleVal.selectX, BattleVal.selectY);
                    Mapclass.TranslateMapCoordToPosition(ref rSkill, skillpos[0], skillpos[1]);
                    rSkill.y = r0.y;
                    BattleVal.selectedUnit.gobj.transform.LookAt(rSkill);

                    /*
                    //攻撃対象
                    foreach (int[] attackedpos in attackedposlist)
                    {
                        Vector3 r1 = new Vector3(); //攻撃先
                        Mapclass.TranslateMapCoordToPosition(ref r1, attackedpos[0], attackedpos[1]);
                        Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", attackedpos[0], attackedpos[1])];
                        damage = Calc_Damage(BattleVal.selectedUnit, temp);

                        temp.hp -= damage; //ダメージ処理

                        string damagetext = string.Format("{0}", (int)Mathf.Abs(damage));
                        int count = 1;
                        //ダメージテキストの中心座標
                        Vector3 damage_center = Camera.main.WorldToScreenPoint(r1);

                        //テキストの登録
                        List<Text> damagenumtmp = new List<Text>();
                        foreach (char a in damagetext)
                        {
                            int num = (int)char.GetNumericValue(a); //数を取得

                            damagenumtmp.Add(Instantiate(damagetextprefab, r1, Quaternion.identity));
                            damagenumtmp[count - 1].text = a.ToString();
                            //サイズ取得
                            damagenumtmp[count - 1].rectTransform.sizeDelta = 
                                new Vector2(damagenumtmp[count - 1].preferredWidth, damagenumtmp[count - 1].preferredHeight);
                            damagenumtmp[count - 1].rectTransform.sizeDelta = 
                                new Vector2(damagenumtmp[count - 1].preferredWidth, damagenumtmp[count - 1].preferredHeight);
                            damagenumtmp[count - 1].transform.SetParent(canvas.transform, false);
                            damagenumtmp[count - 1].transform.localPosition = damage_center - new Vector3(canvas.GetComponent<RectTransform>().sizeDelta.x / 2, canvas.GetComponent<RectTransform>().sizeDelta.y / 2, 0);

                            //1桁当たりの文字ブロック
                            Vector3 numsize = damagenumtmp[count - 1].GetComponent<RectTransform>().sizeDelta * damagenumtmp[count - 1].GetComponent<RectTransform>().localScale;
                            numsize.y = 0;
                            numsize.z = 0;

                            damagenumtmp[count - 1].transform.localPosition += (count - 1) * numsize;
                            damagenumtmp[count - 1].transform.localPosition += new Vector3(0, 30, 0);
                            damagenumtmp[count - 1].color = new Vector4(1, 0, 0, 1);
                            if (damage < 0) damagenumtmp[count - 1].color = new Vector4(0.7f,1,0.7f,1);
                            damagenumtmp[count - 1].gameObject.SetActive(false);
                            count++;

                        }
                        damagenum.Add(damagenumtmp);

                        //アニメーション設定
                        attackedAnimator.Add(temp.gobj.GetComponent<Animator>());
                        if(damage > 0)
                            attackedanimstate.Add(hitAnimName);    //共通モーションだと思うが、一部キャラで回復・ダメージが混ざる将来性も加味する
                        else
                            attackedanimstate.Add(healAnimName); //暫定　Healモーションも作る
                        attackedCharaAnimation.Add(temp.gobj.GetComponent<CharaAnimation>());
                    }
                    */
                    nowtime = -1; //initialize

                    //スキルの消費
                    selectedskill.Consume(1);
                    //行動スタックのクリア（1手戻し不可能に）
                    BattleVal.actions.Clear();

                    //カットイン演出の場合
                    if(selectedskill.is_cutscene)
                    {
                        gobjCutin = Instantiate(selectedskill.prefab_cutin);
                        bstate = BATTLE_STATUS.CUTIN;

                    }
                    else
                    {
                        //通常スキルの場合
                        //スキル使用モーション再生
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().Play(selectedskill.animname);
                        //state update
                        bstate = BATTLE_STATUS.EFFECT;
                        skilltext.text = selectedskill.skillname;
                        skillnamePanel.SetActive(true);
                    }
                    

                    break;
                case BATTLE_STATUS.CUTIN:
                    if(gobjCutin.GetComponent<CutinChecker>().cutinFinish)
                    {
                        Destroy(gobjCutin);
                        //スキル使用モーション再生
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().Play(selectedskill.animname);
                        //state update
                        bstate = BATTLE_STATUS.EFFECT;
                        skilltext.text = selectedskill.skillname;
                        skillnamePanel.SetActive(true);
                    }
                    break;
                case BATTLE_STATUS.EFFECT:
                    //スキル使用モーションが終了し、音声はなり終わったか？
                    /*
                    if (BattleVal.selectedUnit.gobj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1
                        && !BattleVal.selectedUnit.gobj.GetComponent<CharaAnimation>().voiceSource.isPlaying
                        && !BattleVal.selectedUnit.gobj.GetComponent<CharaAnimation>().seSource.isPlaying)
                        */
                    if (!BattleVal.selectedUnit.gobj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).shortNameHash.Equals(Animator.StringToHash(selectedskill.animname))
                        && !BattleVal.selectedUnit.gobj.GetComponent<CharaAnimation>().voiceSource.isPlaying
                        && !BattleVal.selectedUnit.gobj.GetComponent<CharaAnimation>().seSource.isPlaying)
                    {

                        Vector3 effectcampoint = new Vector3();
                        Mapclass.TranslateMapCoordToPosition(ref effectcampoint, skillpos[0], skillpos[1]);
                        CameraAngle.CameraPoint(effectcampoint);
                        
                        //エフェクト再生へ
                        Vector3 reffect = new Vector3();
                        Mapclass.TranslateMapCoordToPosition(ref reffect, skillpos[0], skillpos[1]);
                        hitEffect = Instantiate(selectedskill.skillefect, reffect, selectedskill.skillefect.transform.rotation);
                        bstate = BATTLE_STATUS.BATTLE;
                    }
                    break;
                case BATTLE_STATUS.BATTLE:
                    //バトルのアニメーション

                    //エフェクトの再生が終了したか？
                    if(hitEffect == null)
                    {
                        //被弾モーション再生
                        if(nowtime == -1)
                        {
                            //攻撃対象
                            foreach (int[] attackedpos in attackedposlist)
                            {
                                Vector3 r1 = new Vector3(); //攻撃先
                                Mapclass.TranslateMapCoordToPosition(ref r1, attackedpos[0], attackedpos[1]);
                                Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", attackedpos[0], attackedpos[1])];
                                damage = Calc_Damage(BattleVal.selectedUnit, temp);

                                temp.hp -= damage; //ダメージ処理

                                string damagetext = string.Format("{0}", (int)Mathf.Abs(damage));
                                int count = 1;
                                //ダメージテキストの中心座標
                                Vector3 damage_center = Camera.main.WorldToScreenPoint(r1);

                                //テキストの登録
                                List<Text> damagenumtmp = new List<Text>();
                                foreach (char a in damagetext)
                                {
                                    int num = (int)char.GetNumericValue(a); //数を取得

                                    damagenumtmp.Add(Instantiate(damagetextprefab, r1, Quaternion.identity));
                                    damagenumtmp[count - 1].text = a.ToString();
                                    //サイズ取得
                                    damagenumtmp[count - 1].rectTransform.sizeDelta =
                                        new Vector2(damagenumtmp[count - 1].preferredWidth, damagenumtmp[count - 1].preferredHeight);
                                    damagenumtmp[count - 1].rectTransform.sizeDelta =
                                        new Vector2(damagenumtmp[count - 1].preferredWidth, damagenumtmp[count - 1].preferredHeight);
                                    damagenumtmp[count - 1].transform.SetParent(canvas.transform, false);
                                    damagenumtmp[count - 1].transform.localPosition = damage_center - new Vector3(canvas.GetComponent<RectTransform>().sizeDelta.x / 2, canvas.GetComponent<RectTransform>().sizeDelta.y / 2, 0);

                                    //1桁当たりの文字ブロック
                                    Vector3 numsize = damagenumtmp[count - 1].GetComponent<RectTransform>().sizeDelta * damagenumtmp[count - 1].GetComponent<RectTransform>().localScale;
                                    numsize.y = 0;
                                    numsize.z = 0;

                                    damagenumtmp[count - 1].transform.localPosition += (count - 1) * numsize;
                                    damagenumtmp[count - 1].transform.localPosition += new Vector3(0, 30, 0);
                                    damagenumtmp[count - 1].color = new Vector4(1, 0, 0, 1);
                                    if (damage < 0) damagenumtmp[count - 1].color = new Vector4(0.7f, 1, 0.7f, 1);
                                    damagenumtmp[count - 1].gameObject.SetActive(false);
                                    count++;

                                }
                                damagenum.Add(damagenumtmp);

                                //アニメーション設定
                                attackedAnimator.Add(temp.gobj.GetComponent<Animator>());
                                if (damage > 0)
                                    attackedanimstate.Add(hitAnimName);    //共通モーションだと思うが、一部キャラで回復・ダメージが混ざる将来性も加味する
                                else
                                    attackedanimstate.Add(healAnimName); //暫定　Healモーションも作る
                                attackedCharaAnimation.Add(temp.gobj.GetComponent<CharaAnimation>());
                            }
                            for (int i=0; i<attackedAnimator.Count; i++)
                            {
                                //Count不一致するかも？
                                attackedAnimator[i].Play(attackedanimstate[i]);
                                if(attackedanimstate[i] == hitAnimName)
                                {
                                    attackedCharaAnimation[i].seSource.clip = seDamage;
                                }
                                else
                                {
                                    attackedCharaAnimation[i].seSource.clip = seHeal;
                                }
                                attackedCharaAnimation[i].seSource.Play();
                            }
                            //テキスト表示
                            foreach (List<Text> damagenumtmp in damagenum)
                            {
                                foreach (Text a in damagenumtmp)
                                {
                                    a.gameObject.SetActive(true);
                                }
                                //CameraAngle.CameraPoint(BattleVal.selectedUnit.gobj.transform.position);
                            }
                            nowtime = 0;
                        }
                        else if (nowtime >= battle_time - Time.deltaTime)
                        {
                            //戦闘後処理
                            bstate = BATTLE_STATUS.AFTERBATTLE;
                           
                            //ダメージ数値のテキストを消す処理
                            foreach (List<Text> damagenumtmp in damagenum)
                            {
                                foreach (Text a in damagenumtmp)
                                {
                                    Destroy(a.gameObject);
                                }
                            }
                            //戦闘用のダメージ表示などの判定が終了した後に、カメラの位置の移動
                            CameraAngle.CameraPoint(BattleVal.selectedUnit.gobj.transform.position);
                            //アニメーション関連の初期化
                            attackedAnimator.Clear();
                            attackedanimstate.Clear();
                            attackedCharaAnimation.Clear();
                            damagenum.Clear();
                            skillnamePanel.SetActive(false);
                            //攻撃可能フラグをオフに
                            BattleVal.selectedUnit.atackable = false;

                        }
                        else
                        {
                            //ダメージ数値のテキストを消す処理
                            //damagetext.color -= new Color(0,0,0,Time.deltaTime);
                            foreach (List<Text> damagenumtmp in damagenum)
                            {
                                foreach (Text a in damagenumtmp)
                                {
                                    a.color -= new Color(0, 0, 0, Time.deltaTime);
                                    a.transform.position += new Vector3(0, 1, 0);
                                }
                            }
                            nowtime += Time.deltaTime;

                        }
                    }

                    
                    break;
                //戦闘後処理
                case BATTLE_STATUS.AFTERBATTLE:
                    //獲得経験値
                    int getexp = 10; //基本値
                    //撃墜処理
                    foreach (int[] attackedpos in attackedposlist)
                    {
                        Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", attackedpos[0], attackedpos[1])];
                        //撃墜処理
                        if (temp.hp <= 0)
                        {
                            Vector3 defeatpos = new Vector3();
                            Mapclass.TranslateMapCoordToPosition(ref defeatpos, attackedpos[0], attackedpos[1]);
                            StartCoroutine(UnitDefeat.DefeatHandle(temp, defeatEffect, defeatpos));

                            //撃墜ボーナス
                            getexp += BattleVal.id2index[string.Format("{0},{1}", attackedpos[0], attackedpos[1])].status.needexp;

                            //ユニットデータを消去
                            ////BattleVal.unitlistから消去
                            for (int i = 0; i < BattleVal.unitlist.Count; i++)
                            {
                                if (BattleVal.unitlist[i].x == temp.x && BattleVal.unitlist[i].y == temp.y)
                                {
                                    BattleVal.unitlist.RemoveAt(i);
                                    break;
                                }
                            }
                            ////BattleVal.id2indexから消去
                            BattleVal.id2index.Remove(string.Format("{0},{1}", temp.x, temp.y));
                            ////mapdataからユニット情報の削除
                            BattleVal.mapdata[(int)MapdataList.MAPUNIT][temp.y][temp.x] = 0;
                        }
                        if (temp.hp > temp.status.maxhp)
                        {
                            temp.hp = temp.status.maxhp;
                        }
                    }

                    //必殺技の場合、獲得経験値が1.5倍ボーナス
                    if(selectedskill.is_cutscene) getexp = (int)((float)getexp*1.5f);
                    //ユニットセレクトに戻る
                    bstate = BATTLE_STATUS.SETVECT;
                    if (BattleVal.selectedUnit.team == 0)
                    {
                        //経験値を獲得するキャラの場合
                        if (BattleVal.selectedUnit.status.needexp != 0 
                            && BattleVal.selectedUnit.status.level < 99 && BattleVal.selectedUnit.status.level >= 1)
                        {
                            Operation.AddGetExp(BattleVal.selectedUnit, getexp);
                            BattleVal.status = STATUS.GETEXP;
                        }
                        else BattleVal.status = STATUS.PLAYER_UNIT_SELECT;

                    }
                    else
                    {
                        BattleVal.status = STATUS.ENEMY_UNIT_SELECT;
                    }
                    break;
            }
        }
        else
        {

        }
    }
}
