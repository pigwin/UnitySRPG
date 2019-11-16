using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum MOVING_STATUS
{
    SETVECT,
    MOVING
}
/*******************************************************************/
/*キャラクターの移動に関すること全般を処理するクラス               */
/*******************************************************************/
public class CharaMove : MonoBehaviour {
    //移動可能範囲を表すタイル
    public GameObject movabletile_inspector;
    //ZOCタイル
    public GameObject zoctile_inspector;

    //移動可能範囲を表すタイル
    public static GameObject movabletile;
    //ZOCタイル
    public static GameObject zoctile;

    //ボタン
    public Button unitMoveButton;

    //描画したタイルを格納する配列
    public static List<GameObject> tilelist = new List<GameObject>();

    //移動可能範囲
    public static List<int[]> movablelist;

    //キャラ移動処理時の移動経路
    public static List<int[]> movepath = new List<int[]>();

    //移動ステート
    private MOVING_STATUS mstate = MOVING_STATUS.SETVECT;

    //移動ボタンクリック時の処理
    public void MoveButton_onclick()
    {
        //スキルボタンリスト消去
        CharaSkill.Destory_SkillButtonList();

        //移動可能範囲を取得
        Set_Movablelist();

        //表示処理
        Show_Movablelist();

        //「移動」ボタンを消し、移動先選択モードへ遷移
        BattleVal.menuflag = false;
        BattleVal.status = STATUS.PLAYER_UNIT_MOVE;
    }

    public static void Set_Movablelist()
    {
        //移動可能範囲の初期化
        movablelist = new List<int[]>();
        //移動可能範囲を取得
        movablelist = Mapclass.Dfs(BattleVal.mapdata, BattleVal.selectX, BattleVal.selectY, BattleVal.selectedUnit.status.step, BattleVal.selectedUnit.status.jump);
    }

    public static void Show_Movablelist()
    {
        //表示処理
        foreach (int[] mappos in movablelist)
        {
            if (!BattleVal.zocmap[mappos[0], mappos[1]]) tilelist.Add(Instantiate(movabletile));
            if (BattleVal.zocmap[mappos[0], mappos[1]]) tilelist.Add(Instantiate(zoctile));
            Mapclass.DrawCharacter(tilelist[tilelist.Count - 1], mappos[0], mappos[1]);
        }
    }

    //移動可能タイルの消去処理
    public static void Destroy_Movabletile()
    {
        foreach(GameObject tile in tilelist)
        {
            Destroy(tile);
        }
    }

    //与えたマップ内座標が、選択したユニットの移動可能範囲(movablelist)かを判定し、移動可能なら移動準備を行う
    public static bool Is_movable(int map_x, int map_y)
    {
        //判定
        foreach (int[] mappos in movablelist)
        {
            if (map_x == mappos[0] && map_y == mappos[1])
            {
                //移動経路を登録
                movepath = Mapclass.GetPath(BattleVal.mapdata, BattleVal.selectedUnit.status.step,
                    BattleVal.selectX, BattleVal.selectY, map_x, map_y, BattleVal.selectedUnit.status.jump);
                Initialize_Moving_Param();
                Destroy_Movabletile();
                return true;
            }
        }
        return false;
    }

    //MOVING前のパラメータ初期化処理
    public static void Initialize_Moving_Param()
    {
        nowstep = 0;

    }

    //現在の移動ステップ数
    private static int nowstep = 0;
    //1ステップにかかる時間
    private static float steptime= 0.5f;

    //アニメーションを少しだけ止める
    private static float stoptime = 0;

    private static float nowtime;
    private static float now_stop_time;

    //移動速度ベクトル(/sec)
    private static Vector3 velocity = new Vector3();

    //キャラクターの座標を変更する関数（1手戻し、配置変更用）
    public static void Change_Unit_Position(Unitdata unit, int[] old_pos, int[] new_pos)
    {
        //unitのGameObjectの位置を変更
        Vector3 r1 = new Vector3();
        Mapclass.TranslateMapCoordToPosition(ref r1, new_pos[0], new_pos[1]);
        unit.gobj.transform.position = r1;
        unit.gobj.transform.LookAt(new Vector3(r1.x, unit.gobj.transform.position.y, r1.z));
        unit.x = new_pos[0];
        unit.y = new_pos[1];


        //map上のキャラクターIDの更新
        BattleVal.mapdata[(int)MapdataList.MAPUNIT][new_pos[1]][new_pos[0]]
                            = BattleVal.mapdata[(int)MapdataList.MAPUNIT][old_pos[1]][old_pos[0]];
        BattleVal.mapdata[(int)MapdataList.MAPUNIT][old_pos[1]][old_pos[0]] = 0;

        //ディクショナリのアップデート
        BattleVal.id2index.Remove(string.Format("{0},{1}", old_pos[0], old_pos[1]));
        BattleVal.id2index.Add(string.Format("{0},{1}", new_pos[0], new_pos[1]), unit);

        //unitの行動フラグを戻す
        unit.movable = true;
    }

    //キャラクターの座標を入れ替える関数（配置変更用）
    public static void Swap_Unit_Position(Unitdata unit1, Unitdata unit2)
    {
        int tmpx, tmpy;
        tmpx = unit1.x;
        tmpy = unit1.y;

        //unit1のGameObjectの位置を変更
        Vector3 r1 = new Vector3();
        Mapclass.TranslateMapCoordToPosition(ref r1, unit2.x, unit2.y);
        unit1.gobj.transform.position = r1;
        unit1.gobj.transform.LookAt(new Vector3(r1.x, unit1.gobj.transform.position.y, r1.z));
        unit1.x = unit2.x;
        unit1.y = unit2.y;

        //unit2のGameObjectの位置を変更
        Mapclass.TranslateMapCoordToPosition(ref r1, tmpx, tmpy);
        unit2.gobj.transform.position = r1;
        unit2.gobj.transform.LookAt(new Vector3(r1.x, unit2.gobj.transform.position.y, r1.z));
        unit2.x = tmpx;
        unit2.y = tmpy;

        //map上のキャラクターIDの更新
        int tmpmap1 = BattleVal.mapdata[(int)MapdataList.MAPUNIT][unit1.y][unit1.x];
        BattleVal.mapdata[(int)MapdataList.MAPUNIT][unit1.y][unit1.x]
                            = BattleVal.mapdata[(int)MapdataList.MAPUNIT][unit2.y][unit2.x];
        BattleVal.mapdata[(int)MapdataList.MAPUNIT][unit2.y][unit2.x] = tmpmap1;

        //ディクショナリのアップデート
        BattleVal.id2index.Remove(string.Format("{0},{1}", unit2.x, unit2.y));  //swap後なのでこれはunit1の消去であることに注意
        BattleVal.id2index.Remove(string.Format("{0},{1}", unit1.x, unit1.y));  //swap後なのでこれはunit2の消去であることに注意
        BattleVal.id2index.Add(string.Format("{0},{1}", unit1.x, unit1.y), unit1);
        BattleVal.id2index.Add(string.Format("{0},{1}", unit2.x, unit2.y), unit2);

    }

    //Start
    private void Start()
    {
        movabletile = movabletile_inspector;
        zoctile = zoctile_inspector;
    }

    //Update
    private void Update()
    {
        //移動中の場合
        if (BattleVal.status == STATUS.MOVING)
        {
            switch (mstate)
            {
                case MOVING_STATUS.SETVECT:

                    //移動終了の判定
                    if (nowstep == movepath.Count - 1)
                    {
                        //行動スタックを積む（1手戻し用）
                        if (BattleVal.status == STATUS.MOVING)
                        {
                            Action thisact = new Action(BattleVal.selectedUnit,
                                BattleVal.selectedUnit.x, BattleVal.selectedUnit.y, movepath[nowstep][0], movepath[nowstep][1]);
                            BattleVal.actions.Push(thisact);
                        }

                        //map上のキャラクターIDの更新
                        //座標情報のアップデート
                        BattleVal.mapdata[(int)MapdataList.MAPUNIT][movepath[nowstep][1]][movepath[nowstep][0]]
                            = BattleVal.mapdata[(int)MapdataList.MAPUNIT][movepath[0][1]][movepath[0][0]];
                        //移動した場合
                        if (nowstep != 0)
                            BattleVal.mapdata[(int)MapdataList.MAPUNIT][movepath[0][1]][movepath[0][0]] = 0;
                        //BattleVal.selectedUnit.gobj.GetComponent<QuerySDMecanimController>().ChangeAnimation(QuerySDMecanimController.QueryChanSDAnimationType.NORMAL_STAND);
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Walk", false);
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Jump", false);
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().Play("Idle");
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().Update(0);

                        BattleVal.selectedUnit.x = movepath[nowstep][0];
                        BattleVal.selectedUnit.y = movepath[nowstep][1];
                        //ディクショナリのアップデート
                        BattleVal.id2index.Remove(string.Format("{0},{1}", BattleVal.selectX, BattleVal.selectY));
                        BattleVal.id2index.Add(string.Format("{0},{1}", movepath[nowstep][0], movepath[nowstep][1]), BattleVal.selectedUnit);

                        //Battleval.statusのアップデート
                        Debug.Log(BattleVal.turnplayer);
                        if (BattleVal.turnplayer == 0)
                        {
                            BattleVal.status = STATUS.PLAYER_UNIT_SELECT;
                            BattleVal.selectX = -1;
                            BattleVal.selectY = -1;
                        }
                        else
                            BattleVal.status = STATUS.ENEMY_UNIT_SELECT;

                        //移動可能フラグをオフに
                        BattleVal.selectedUnit.movable = false;


                        break;
                    }
                    //速度ベクトル(実空間)の計算
                    Vector3 r0 = new Vector3();
                    Mapclass.TranslateMapCoordToPosition(ref r0, movepath[nowstep][0], movepath[nowstep][1]);
                    Vector3 r1 = new Vector3();
                    Mapclass.TranslateMapCoordToPosition(ref r1, movepath[nowstep + 1][0], movepath[nowstep + 1][1]);
                    velocity = (1.0f / steptime) * (r1 - r0);  //1マスの移動に0.5sec
                    nowtime = 0;
                    stoptime = 0;

                    if (Mathf.Abs(BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][movepath[nowstep + 1][1]][movepath[nowstep + 1][0]] - BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][movepath[nowstep][1]][movepath[nowstep][0]]) > 0)
                    {
                        //ジャンプモーション
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().Play("Jump");
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Walk", false);
                        stoptime = 0.3f;

                    }
                    else
                    {
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Walk", true);
                    }
                    /*
                    //もしもジャンプする場合
                    if (BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][movepath[nowstep + 1][1]][movepath[nowstep + 1][0]] - BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][movepath[nowstep][1]][movepath[nowstep][0]] > 0)
                    {
                        //ジャンプモーション
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Walk", false);
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Jump",true);
                        //BattleVal.selectedUnit.gobj.GetComponent<QuerySDMecanimController>().ChangeAnimation(QuerySDMecanimController.QueryChanSDAnimationType.NORMAL_FLY_UP);
                        stoptime = 0.3f;
                    }
                    else if (BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][movepath[nowstep + 1][1]][movepath[nowstep + 1][0]] - BattleVal.mapdata[(int)MapdataList.MAPHEIGHT][movepath[nowstep][1]][movepath[nowstep][0]] < 0)
                    {
                        //ジャンプモーション
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Walk", false);
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Jump", true);

                        //BattleVal.selectedUnit.gobj.GetComponent<QuerySDMecanimController>().ChangeAnimation(QuerySDMecanimController.QueryChanSDAnimationType.NORMAL_FLY_DOWN);
                        stoptime = 0.3f;
                    }
                    else
                    {
                        //歩行モーション
                        //BattleVal.selectedUnit.gobj.GetComponent<QuerySDMecanimController>().ChangeAnimation(QuerySDMecanimController.QueryChanSDAnimationType.NORMAL_WALK);
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Jump", false);
                        BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Walk", true);

                    }
                    */
                    //キャラの向きの調整
                    Mapclass.TranslateMapCoordToPosition(ref r1, movepath[nowstep + 1][0], movepath[nowstep + 1][1]);
                    BattleVal.selectedUnit.gobj.transform.LookAt(r1);
                    //mstateのアップデート
                    mstate = MOVING_STATUS.MOVING;

                    break;
                case MOVING_STATUS.MOVING:

                    //ステップ終了の判定
                    if (nowtime >= steptime - Time.deltaTime)
                    {
                        
                        if (now_stop_time == 0)
                        {
                            //BattleVal.selectedUnit.gobj.GetComponent<Animator>().SetBool("Jump", false);
                        }
                        //終点に表示
                        if (now_stop_time > stoptime)
                        {
                            r1 = new Vector3();
                            Mapclass.TranslateMapCoordToPosition(ref r1, movepath[nowstep + 1][0], movepath[nowstep + 1][1]);
                            BattleVal.selectedUnit.gobj.transform.position = r1;
                            BattleVal.selectedUnit.gobj.transform.LookAt(new Vector3(r1.x, BattleVal.selectedUnit.gobj.transform.position.y, r1.z));
                            CameraAngle.CameraPoint(BattleVal.selectedUnit.gobj.transform.position);
                            nowstep++;
                            now_stop_time = 0;
                            mstate = MOVING_STATUS.SETVECT;
                            break;
                        }
                        now_stop_time += Time.deltaTime;
                        nowtime += Time.deltaTime;
                        /*
                        //終点に表示

                        r1 = new Vector3();
                        Mapclass.TranslateMapCoordToPosition(ref r1, movepath[nowstep + 1][0], movepath[nowstep + 1][1]);
                        BattleVal.selectedUnit.gobj.transform.position = r1;
                        BattleVal.selectedUnit.gobj.transform.LookAt(new Vector3(r1.x, BattleVal.selectedUnit.gobj.transform.position.y, r1.z));
                        CameraAngle.CameraPoint(BattleVal.selectedUnit.gobj.transform.position);
                        nowstep++;
                        now_stop_time = 0;
                        mstate = MOVING_STATUS.SETVECT;
                        break;
                        */
                    }
                    else
                    {
                        r1 = new Vector3();
                        Mapclass.TranslateMapCoordToPosition(ref r1, movepath[nowstep + 1][0], movepath[nowstep + 1][1]);
                        BattleVal.selectedUnit.gobj.transform.LookAt(new Vector3(r1.x, BattleVal.selectedUnit.gobj.transform.position.y, r1.z));
                        //nowtime 加算処理
                        nowtime += Time.deltaTime;
                        //移動
                        BattleVal.selectedUnit.gobj.transform.position += new Vector3(velocity.x, 0, velocity.z) * Time.deltaTime;
                        //カメラの移動
                        CameraAngle.CameraPoint(BattleVal.selectedUnit.gobj.transform.position);

                        if (nowtime < steptime / 2 && velocity.y > 0)
                        {
                            BattleVal.selectedUnit.gobj.transform.position += new Vector3(0, (float)velocity.y * steptime * 60 * nowtime, 0) * Time.deltaTime;
                        }
                        else if (nowtime >= steptime / 2 && velocity.y > 0)
                        {
                            float tempvelocity = (r1.y - BattleVal.selectedUnit.gobj.transform.position.y)/(steptime - nowtime);
                            BattleVal.selectedUnit.gobj.transform.position += new Vector3(0, tempvelocity ,0) * Time.deltaTime;
                        }
                        else if (velocity.y < 0)
                        {
                            BattleVal.selectedUnit.gobj.transform.position += new Vector3(0, velocity.y ,0) * Time.deltaTime;
                        }

                        /*
                        //モーションチェンジ

                        if (nowtime < steptime / 2 && velocity.y > 0)
                        {
                            BattleVal.selectedUnit.gobj.transform.position += new Vector3(0, (float)velocity.y * steptime * 60 * nowtime, 0) * Time.deltaTime;
                        }
                        else if (nowtime >= steptime / 2 && velocity.y > 0)
                        {
                            BattleVal.selectedUnit.gobj.transform.position -= new Vector3(0, (float)(velocity.y * steptime * 16 + Physics.gravity.y) * (nowtime - steptime / 2)) * Time.deltaTime;
                        }
                        else if (velocity.y < 0)
                        {
                            BattleVal.selectedUnit.gobj.transform.position -= new Vector3(0, (float)(velocity.y * steptime * 8 - Physics.gravity.y) * nowtime - steptime) * Time.deltaTime;
                        }
                        */
                    }

                    break;
            }
        }
    }
}
