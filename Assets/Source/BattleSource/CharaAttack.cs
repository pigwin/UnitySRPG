using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

enum BATTLE_STATUS
{
    SETVECT,
    CUTIN,
    SKILLNAME,
    EFFECT,
    BATTLE,
    AFTERBATTLE
}
/*******************************************************************/
/*通常攻撃・スキル攻撃・範囲攻撃レンジ、のデータを扱うクラス       */
/*******************************************************************/
[System.Serializable]
public class AttackRange
{
    public Range BaseRange; //ベースとなる範囲
    public Range ForbiddenRange; //禁止範囲

    //RangeからAttackDfsに渡す部分マップを作成する
    public List<int[]> Attackablelist(List<List<List<int>>> maps, int startx, int starty)
    {
        List<int[]> ans = new List<int[]>();
        
        //ベース範囲の計算
        ans = Mapclass.DfsA(maps, startx, starty, BaseRange);
        
        //禁止範囲の計算
        List<int[]> forbidden = new List<int[]>();
        forbidden = Mapclass.DfsA(maps, startx, starty, ForbiddenRange);
        if(!ForbiddenRange.is_selfforbidden)
        {
            int[] temp = new int[] { startx, starty }; //dummy
            bool flag = false;
            foreach(int[] t in forbidden)
            {
                if (t[0] == startx && t[1] == starty)
                {
                    temp = t;
                    flag = true;
                    break;
                }
            }
            if(flag)
                forbidden.Remove(temp);

        }
        //Exceptを使うために文字列に変換
        List<string> ansstring = new List<string>();
        foreach(int[] tile in ans)
        {
            ansstring.Add(TileToString(tile));
        }
        List<string> forbiddenstring = new List<string>();
        foreach (int[] tile in forbidden)
        {
            forbiddenstring.Add(TileToString(tile));
        }

        //禁止範囲の除外
        ansstring = ansstring.Except<string>(forbiddenstring).ToList<string>();

        //戻す
        ans = new List<int[]>();
        foreach (string str in ansstring)
        {
            ans.Add(StringToTile(str));
        }

        return ans;
    }

    public static string TileToString(int[] tile)
    {
        return string.Format("{0},{1}", tile[0], tile[1]);
    }
    public static int[] StringToTile(string str)
    {
        string[] temp = str.Split(',');
        return new int[] { int.Parse(temp[0]), int.Parse(temp[1]) };
    }

}
[System.Serializable]
public class Range
{
    [Header("-1:単体（効果範囲用） 0:十字 1:Moveと同じ感じ 2:四角 3:網目 4:斜め十字")]
    public int type; // -1:単体（範囲攻撃レンジ用） 0:十字 1:Moveと同じ感じ 2:四角 3:網目 4:斜め十字
    public int step; // 攻撃範囲の歩数
    public int jumpup; //上高さ制限
    public int jumpdown; //下高さ制限
    [Header("Forbidden Range限定：自分自身を取り除くかどうか")]
    public bool is_selfforbidden = true;

    //縦横方向の探索
    public bool Isverthori
    {
        get
        {
            switch (type)
            {
                //十字
                case 0:
                //歩行型
                case 1:
                //四角型
                case 2:
                    return true;

                //網目
                case 3:
                //斜め十字
                case 4:
                //そもそも特殊処理
                default:
                    return false;

            }
        }
    }

    //斜め方向の探索
    public bool Iscross
    {
        get
        {
            switch (type)
            {
                //四角型
                case 2:
                //網目
                case 3:
                //斜め十字
                case 4:
                    return true;

                //十字
                case 0:
                //歩行型
                case 1:
                //そもそも特殊処理
                default:
                    return false;

            }
        }
    }

    //基本ベクトルの線形結合を許すかどうか
    public bool Is1D
    {
        get
        {
            switch (type)
            {
                //十字
                case 0:
                //斜め十字
                case 4:
                    return true;

                
                //歩行型
                case 1:
                //四角型
                case 2:
                //網目
                case 3:
                //そもそも特殊処理
                default:
                    return false;

            }
        }
    }

}


public class CharaAttack : MonoBehaviour {
    //攻撃可能範囲を表すタイル
    public static GameObject attackabletile;
    public GameObject attackabletile_inspector;
    //ボタン
    public Button unitAttackButton;

    //描画したタイルを格納する配列
    public static List<GameObject> tilelist = new List<GameObject>();
    //攻撃可能範囲
    public static List<int[]> attackablelist;

    //攻撃ボタンクリック時の処理
    //敵ターンは呼び出せない（staticでないため）
    public void AttackButton_onclick()
    {
        //スキルボタンリスト消去
        CharaSkill.Destory_SkillButtonList();

        //移動可能範囲を取得
        Set_Attackablelist();
        //表示処理
        Show_Attackablelist();

        //「移動」ボタンを消し、攻撃先選択モードへ遷移
        BattleVal.menuflag = false;
        BattleVal.status = STATUS.PLAYER_UNIT_ATTACK;
    }

    //attackablelistをセットするだけ
    public static void Set_Attackablelist()
    {
        //攻撃可能範囲の初期化
        attackablelist = new List<int[]>();

        //移動可能範囲を取得
        attackablelist = BattleVal.selectedUnit.attackrange.Attackablelist(BattleVal.mapdata, 
            BattleVal.selectedUnit.x, BattleVal.selectedUnit.y);
    }
    //attackablelistを表示するだけ
    public static void Show_Attackablelist()
    {
        //表示処理
        foreach (int[] mappos in attackablelist)
        {
            GameObject tmptile = Instantiate(attackabletile);
            tilelist.Add(tmptile);
            Mapclass.DrawCharacter(tilelist[tilelist.Count - 1], mappos[0], mappos[1]);
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

    //攻撃可能範囲に攻撃対象が存在するかどうか

    //与えたマップ内座標が、攻撃可能範囲かつ攻撃対象がいるかを判定し、攻撃可能なら攻撃処理に入る
    public static bool Is_attackable(int map_x, int map_y)
    {
        foreach (int[] mappos in attackablelist)
        {
            //攻撃可能範囲内か
            if (map_x == mappos[0] && map_y == mappos[1] && BattleVal.id2index.ContainsKey(string.Format("{0},{1}", map_x, map_y)))
            {
                Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", map_x, map_y)];
                //攻撃対象か
                if (temp.team != BattleVal.selectedUnit.team)
                {
                    //攻撃先を登録
                    Set_attackedpos(map_x, map_y);
                    return true;
                }
            }
        }
        return false;
    }

    //攻撃先を登録する関数
    public static void Set_attackedpos(int target_x, int target_y)
    {
        //攻撃先を登録
        attackedpos[0] = target_x;
        attackedpos[1] = target_y;
    }

    //与えたマップ内座標が、攻撃可能範囲かつ攻撃対象がいるかを判定し、攻撃可能なら攻撃処理に入る
    public static List<Unitdata> Get_attackable_charalist()
    {
        //返却値
        List<Unitdata> ans = new List<Unitdata>();

        foreach (int[] mappos in attackablelist)
        {
            //攻撃可能範囲内か
            if (BattleVal.id2index.ContainsKey(string.Format("{0},{1}", mappos[0], mappos[1])))
            {
                Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", mappos[0], mappos[1])];
                //攻撃対象か
                if (temp.team != BattleVal.selectedUnit.team)
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
        //ダメージ計算式
        int damage =  Attack.status.atk - Defence.status.def;
        //負の場合等の例外処理
        if (damage <= 0) damage = 1;
        
        return damage;
    }

    //命中率(0～1)を返却する関数
    public static float Calc_Dexterity(Unitdata Attack, Unitdata Defence)
    {
        float dex = 1.0f; //基本値
        foreach(Condition cond in Defence.status.conditions)
        {
            if (cond.forbiddenMove) return dex;  //移動不可系の状態異常罹患時は必中
        }
        //TEC由来
        dex += (float)(Attack.status.tec - Defence.status.tec) / 100.0f * 0.50f;
        //LUC由来
        dex += (float)(Attack.status.luc - Defence.status.luc) / 100.0f * 0.50f;
        //Debug.Log(dex);

        if (dex > 1) dex = 1.0f;
        if (dex < 0) dex = 0.0f;

        return dex;
    }
    //クリティカル率(0～1)を返却する関数
    public static float Calc_CriticalRate(Unitdata Attack, Unitdata Defence)
    {
        float rate = 0.0f; //基本値

        //TEC由来
        rate += (float)(Attack.status.tec - Defence.status.tec) / 100.0f;
        //LUC由来
        rate += (float)(Attack.status.luc - Defence.status.luc) / 100.0f * 0.50f;
        //Debug.Log(rate);

        if (rate > 1) rate = 1.0f;
        if (rate < 0) rate = 0.0f;

        return rate;
    }

    //命中・クリティカル判定
    public bool Is_Hit(float dex)
    {
        //必中
        if (dex == 1f) return true;

        float rand = Random.value;
        if (dex >= rand) return true;
        else return false;
    }


    //攻撃対象
    private static int[] attackedpos = new int[2];
    //攻撃ステート
    private BATTLE_STATUS bstate = BATTLE_STATUS.SETVECT;
    //実際に表示する数値
    public static List<Text> damagenum = new List<Text>();

    //ダメージ文字（1桁あたりorMiss、Critical）
    public Text damagetextprefab;
    //バトル演出時間
    private float battle_time = 1.0f;
    private float damage_popy = 50f;
    private static float nowtime;
    public Canvas canvas;
    //通常ヒット音
    public AudioClip seHitN;
    //回避音
    public AudioClip seDodge;
    //Start
    private void Start()
    {
        attackabletile = attackabletile_inspector;
    }

    //Update
    private Animator attackedAnimator; //防御側のアニメーター保存
    private string attackedanimstate;
    private GameObject hitEffect; //攻撃時のエフェクト
    public GameObject defeatEffect; //撃墜時のエフェクト
    private void Update()
    {
        //戦闘中
        if (BattleVal.status == STATUS.BATTLE)
        {
            bool critflag = false;
            bool dodgeflag = false;
            int damage = 0;
            switch (bstate)
            {
                case BATTLE_STATUS.SETVECT:
                    //キャラの向きの調整
                    Vector3 r0 = new Vector3(); //攻撃元
                    Vector3 r1 = new Vector3(); //攻撃先
                    Mapclass.TranslateMapCoordToPosition(ref r0, BattleVal.selectX, BattleVal.selectY);
                    Mapclass.TranslateMapCoordToPosition(ref r1, attackedpos[0], attackedpos[1]);

                    //攻撃対象の登録
                    Unitdata temp = BattleVal.id2index[string.Format("{0},{1}", attackedpos[0], attackedpos[1])];
                    damage = Calc_Damage(BattleVal.selectedUnit, temp);
                    float dex = Calc_Dexterity(BattleVal.selectedUnit, temp);
                    float rate = Calc_CriticalRate(BattleVal.selectedUnit, temp);
                    //当たった場合
                    if (Is_Hit(dex))
                    {
                        //クリティカル発生かどうか
                        if (Is_Hit(rate))
                        {
                            damage = (int)(damage * 1.5f);
                            critflag = true;
                        }

                        temp.hp -= damage; //ダメージ処理

                        string damagetext = string.Format("{0}", damage);
                        int count = 1;
                        //ダメージテキストの中心座標
                        Vector3 damage_center = Camera.main.WorldToScreenPoint(r1);

                        //テキストの登録
                        foreach (char a in damagetext)
                        {
                            int num = (int)char.GetNumericValue(a); //数を取得

                            damagenum.Add(Instantiate(damagetextprefab, r1, Quaternion.identity));
                            damagenum[count - 1].text = a.ToString();
                            //サイズ取得
                            damagenum[count - 1].rectTransform.sizeDelta =
                                new Vector2(damagenum[count - 1].preferredWidth, damagenum[count - 1].preferredHeight);
                            damagenum[count - 1].rectTransform.sizeDelta =
                                new Vector2(damagenum[count - 1].preferredWidth, damagenum[count - 1].preferredHeight);
                            damagenum[count - 1].transform.SetParent(canvas.transform, false);
                            damagenum[count - 1].transform.localPosition = damage_center - new Vector3(canvas.GetComponent<RectTransform>().sizeDelta.x / 2, canvas.GetComponent<RectTransform>().sizeDelta.y / 2, 0);
                            //1桁当たりの文字ブロック
                            Vector3 numsize = damagenum[count - 1].GetComponent<RectTransform>().sizeDelta * damagenum[count - 1].GetComponent<RectTransform>().localScale;
                            numsize.y = 0;
                            numsize.z = 0;

                            damagenum[count - 1].transform.localPosition += (count - 1) * numsize;
                            damagenum[count - 1].transform.localPosition += new Vector3(0, 30, 0);
                            damagenum[count - 1].gameObject.SetActive(false);
                            damagenum[count - 1].color = new Vector4(1, 0, 0, 1);
                            count++;
                        }
                        //クリティカルの場合、そのテキストを表示
                        if (critflag)
                        {
                            damagenum.Add(Instantiate(damagetextprefab, r1, Quaternion.identity));
                            damagenum[count - 1].text = "Critical!";
                            //サイズ取得
                            damagenum[count - 1].rectTransform.sizeDelta =
                                new Vector2(damagenum[count - 1].preferredWidth, damagenum[count - 1].preferredHeight);
                            damagenum[count - 1].rectTransform.sizeDelta =
                                new Vector2(damagenum[count - 1].preferredWidth, damagenum[count - 1].preferredHeight);
                            damagenum[count - 1].transform.SetParent(canvas.transform, false);
                            damagenum[count - 1].transform.localPosition = damage_center - new Vector3(canvas.GetComponent<RectTransform>().sizeDelta.x / 2, canvas.GetComponent<RectTransform>().sizeDelta.y / 2, 0);
                            damagenum[count - 1].transform.localPosition += new Vector3(0, 100, 0);
                            damagenum[count - 1].gameObject.SetActive(false);
                            damagenum[count - 1].color = new Color(1.0f, 1.0f, 0);
                            count++;
                        }
                    }
                    else
                    {
                        dodgeflag = true;
                        //ダメージテキストの中心座標
                        Vector3 damage_center = Camera.main.WorldToScreenPoint(r1);

                        //テキストの登録
                        damagenum.Add(Instantiate(damagetextprefab, r1, Quaternion.identity));
                        damagenum[0].text = "DODGE!";
                        //サイズ取得
                        damagenum[0].rectTransform.sizeDelta =
                            new Vector2(damagenum[0].preferredWidth, damagenum[0].preferredHeight);
                        damagenum[0].rectTransform.sizeDelta =
                            new Vector2(damagenum[0].preferredWidth, damagenum[0].preferredHeight);
                        damagenum[0].transform.SetParent(canvas.transform, false);
                        damagenum[0].transform.localPosition = damage_center - new Vector3(canvas.GetComponent<RectTransform>().sizeDelta.x / 2, canvas.GetComponent<RectTransform>().sizeDelta.y / 2, 0);
                        damagenum[0].transform.localPosition += new Vector3(0, 30, 0);
                        damagenum[0].gameObject.SetActive(false);
                        damagenum[0].color = new Vector4(1, 1, 1, 1);
                    }
                    //initialize
                    nowtime = -1;

                    //キャラ向きの調整(お互いに向き合う)
                    float tempy = r1.y;
                    r1.y = r0.y;
                    BattleVal.selectedUnit.gobj.transform.LookAt(r1);
                    r0.y = tempy;
                    temp.gobj.transform.LookAt(r0);

                    //行動スタックのクリア（1手戻し不可能に）
                    BattleVal.actions.Clear();

                    //state undate
                    if(critflag)
                    {
                        bstate = BATTLE_STATUS.EFFECT;
                        //クリティカル攻撃・構えモーション
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().Play("CriticalPre");
                    }
                    else
                    {
                        bstate = BATTLE_STATUS.BATTLE;
                        //攻撃モーション開始
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().Play("Attack");
                    }

                    attackedAnimator = temp.gobj.GetComponent<Animator>();
                    attackedanimstate = "Damage";
                    if (dodgeflag) attackedanimstate = "Dodge";

                    break;
                //クリティカル攻撃の演出
                case BATTLE_STATUS.EFFECT:
                    
                    //クリティカル攻撃・構えモーションが終了し、音声はなり終わったか？
                    if(BattleVal.selectedUnit.gobj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).normalizedTime >= 1
                        && !BattleVal.selectedUnit.gobj.GetComponent<CharaAnimation>().voiceSource.isPlaying
                        && !BattleVal.selectedUnit.gobj.GetComponent<CharaAnimation>().seSource.isPlaying)
                    {
                        //クリティカル攻撃モーションへ
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().Play("CriticalAttack");
                        bstate = BATTLE_STATUS.BATTLE;
                    }


                    break;
                case BATTLE_STATUS.BATTLE:
                    //バトルのアニメーション
                    //ヒット判定で効果音＋被ダメージアニメーション
                    if (BattleVal.selectedUnit.gobj.GetComponent<CharaAnimation>().isHit)
                    {
                        attackedAnimator.Play(attackedanimstate);
                        if (attackedanimstate == "Dodge")
                            Operation.setSE(seDodge);
                        else
                        {
                            Operation.setSE(seHitN);
                            Vector3 effectpos = new Vector3();
                            Mapclass.TranslateMapCoordToPosition(ref effectpos, attackedpos[0], attackedpos[1]);
                            hitEffect = Instantiate(BattleVal.selectedUnit.attackeffect, effectpos, BattleVal.selectedUnit.attackeffect.transform.rotation);
                        }

                        //テキスト表示
                        foreach (Text a in damagenum)
                        {
                            a.gameObject.SetActive(true);
                        }
                    }

                    //攻撃者の攻撃モーションが終わったか？
                    if(!BattleVal.selectedUnit.gobj.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("Attack"))
                    {
                        //ダメージテキストの表示
                        if(nowtime == -1)
                        {
                            
                            nowtime = 0;
                        }
                        else if (nowtime >= battle_time - Time.deltaTime)
                        {
                            bstate = BATTLE_STATUS.AFTERBATTLE;
                            
                            //ダメージ数値のテキストを消す処理
                            foreach (Text a in damagenum)
                            {
                                Destroy(a.gameObject);
                            }

                            damagenum.Clear();
                            //戦闘用のダメージ表示などの判定が終了した後に、カメラの位置の移動
                            CameraAngle.CameraPoint(BattleVal.selectedUnit.gobj.transform.position);
                            //攻撃可能フラグをオフに
                            BattleVal.selectedUnit.atackable = false;
                        }
                        else
                        {
                            float accelfact = 1.0f;
                            //加速処理
                            if (Input.GetButton("Submit")) accelfact = 2.0f;
                            if (Input.GetButton("Cancel")) nowtime = battle_time;
                            //ダメージ数値のテキストを消す処理
                            //damagetext.color -= new Color(0,0,0,Time.deltaTime);
                            foreach (Text a in damagenum)
                            {
                                a.color -= new Color(0, 0, 0, Time.deltaTime*accelfact);
                                a.transform.position += new Vector3(0, damage_popy*Time.deltaTime*accelfact, 0); 
                            }
                            nowtime += Time.deltaTime*accelfact;

                        }
                    }
                    break;
                    
                //戦闘後処理
                case BATTLE_STATUS.AFTERBATTLE:
                    //ヒットエフェクトの消去
                    Destroy(hitEffect);

                    string deffence_key = string.Format("{0},{1}", attackedpos[0], attackedpos[1]);
                    //物理攻撃で解除される状態異常の判定
                    if (!dodgeflag)
                    {
                        for(int j=BattleVal.id2index[deffence_key].status.conditions.Count - 1; j >= 0; j--)
                        {
                            if(BattleVal.id2index[deffence_key].status.conditions[j].is_curebyattack)
                            {
                                Destroy(BattleVal.id2index[deffence_key].status.conditions[j].gobjEffect);

                                BattleVal.id2index[deffence_key].status.conditions.RemoveAt(j);
                            }
                        }
                    }

                     //獲得経験値
                    int getexp = 10; //基本値

                    //撃墜処理
                    if (BattleVal.id2index[string.Format("{0},{1}", attackedpos[0], attackedpos[1])].hp <= 0)
                    {
                        Vector3 effectpos = new Vector3();
                        Mapclass.TranslateMapCoordToPosition(ref effectpos, attackedpos[0], attackedpos[1]);
                        
                        Unitdata unit = BattleVal.id2index[string.Format("{0},{1}", attackedpos[0], attackedpos[1])];
                        //撃墜エフェクト
                        StartCoroutine(UnitDefeat.DefeatHandle(unit, defeatEffect, effectpos));

                        //撃墜ボーナス
                        getexp += BattleVal.id2index[string.Format("{0},{1}", attackedpos[0], attackedpos[1])].status.needexp;

                        //ユニットデータを消去
                        ////BattleVal.unitlistから消去
                        for (int i = 0; i < BattleVal.unitlist.Count; i++)
                        {
                            if (BattleVal.unitlist[i].x == unit.x && BattleVal.unitlist[i].y == unit.y)
                            {
                                BattleVal.unitlist.RemoveAt(i);
                                break;
                            }
                        }
                        ////BattleVal.id2indexから消去
                        BattleVal.id2index.Remove(string.Format("{0},{1}", unit.x, unit.y));
                        ////mapdataからユニット情報の削除
                        BattleVal.mapdata[(int)MapdataList.MAPUNIT][unit.y][unit.x] = 0;
                            
                    }
                    
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

    }

}
