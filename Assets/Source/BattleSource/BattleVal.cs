using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//現在のステータスを管理する列挙型
public enum STATUS
{
    LOADING,
    DRAW_STAGE,
    DRAW_TITLE,
    SETUP,
    TURNEND,
    TURNCHANGE,
    TURNCHANGE_SHOW,
    PLAYER_UNIT_SELECT,
    PLAYER_UNIT_MOVE,
    PLAYER_UNIT_ATTACK,
    PLAYER_UNIT_SKILL,
    ENEMY_UNIT_SELECT,
    MOVING,
    BATTLE,
    USESKILL,
    GAMEOVER,
    STAGECLEAR,
    SAVE,
    LOAD,
    DRAW_STAGE_FROM_LOADDATA,
    TAKE_SCREENSHOT_FROM_LOADDATA,
    FADEOUT,
    IFCMD_DIC_SET,   //マップ描画後（ユニットがそろったあと）IF文用に変数を登録する
    GETEXP
};

//行動情報の構造体
public struct Action
{
    public Unitdata unit;
    public int[] prev_pos;
    public int[] follow_pos;

    public Action(Unitdata unit,int prev_x, int prev_y, int follow_x, int follow_y)
    {
        this.unit = unit;
        this.prev_pos = new int[] {prev_x, prev_y};
        this.follow_pos = new int[] {follow_x, follow_y};
    }

    //セーブデータからの再構築
    public Action(ActionSaveData save, Dictionary<string, Unitdata> dic)
    {
        unit = dic[string.Format("{0},{1}", save.unit.x, save.unit.y)];
        prev_pos = save.prev_pos;
        follow_pos = save.follow_pos;
    }
}
//セーブ用行動情報構造体
[System.Serializable]
public struct ActionSaveData
{
    public UnitSaveData unit;
    public int[] prev_pos;
    public int[] follow_pos;

    public ActionSaveData(Action action)
    {
        unit = new UnitSaveData(action.unit);
        prev_pos = action.prev_pos;
        follow_pos = action.follow_pos;
    }
}

//戦闘パートで共有されるpublic staticな変数をまとめるクラス
public class BattleVal{
    //マップ用のデータ
    private static List<List<List<int>>> _mapdata;

    //マップのゲームオブジェクト
    public static Dictionary<string, GameObject> mapgobj;

    //ZOC用データ
    public static bool[,] zocmap;

    //現在の状態を表すデータ
    private static STATUS _status = STATUS.LOADING;
    //ユニットリスト
    public static List<Unitdata> unitlist;

    //作成されたユニットのIDと配列の添え字の連想記憶
    public static Dictionary<string, Unitdata> id2index;

    //選択されているユニットのID取得
    public static int selectX;
    public static int selectY;

    //選択されているユニットの記憶
    public static Unitdata selectedUnit;

    //マップデータの操作
    public static List<List<List<int>>> mapdata
    {
        get { return _mapdata; }
        set { _mapdata = value; }
    }
    //ステータスの操作
    public static STATUS status
    {
        get { return _status; }
        set { _status = value; }
    }

    //メニューボタンの表示フラグ
    public static bool menuflag;

    //システムメニューボタンの表示フラグ
    public static bool sysmenuflag;
    //ターン数
    public static int turn;

    //どちらの手番か（0→プレイヤー、1→コンピュータ）
    public static int turnplayer; //TURNCHANGEで足してMod 2するので、初期値は1

    //行動スタック
    public static Stack<Action> actions;

    //セーブデータ用ステージタイトル名
    public static string title_name;
    public static string str_victoryorder;
    public static string str_defeatorder;

    //GameOver割込み用フラグ
    public static bool is_gameover = false;

    //InputModeフラグ
    public static bool is_mouseinput = true;
}
