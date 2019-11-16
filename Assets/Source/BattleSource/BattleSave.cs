using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//戦闘パートに関するセーブを司るクラス
public class BattleSave : Savedata, ISerializationCallbackReceiver
{

    //-----BattleVal セーブ用------
    //マップ用のデータ
    public List<List<List<int>>> mapdata;
    public List<int> mapsavedata; //セーブ用

    //ユニットリスト
    [System.NonSerialized]
    public List<Unitdata> unitlist;
    public List<UnitSaveData> unitsavelist; //セーブ用

    //ユニットの向きのセーブ
    public Dictionary<Unitdata, Vector3> keyValuePairsUnitdirection;
    public List<Vector3> unitdirection;//セーブ用

    //作成されたユニットのIDと配列の添え字の連想記憶
    public Dictionary<string, Unitdata> id2index;
    public List<string> id2indexkeys;  //セーブ用
    public List<UnitSaveData> id2indexvalues;  //セーブ用

    //ターン数
    public int turn;

    //どちらの手番か（0→プレイヤー、1→コンピュータ）
    public int turnplayer = 1; //SETUPで足してMod 2するので、初期値は1

    //行動スタック
    public Stack<Action> actions;
    public List<ActionSaveData> actionslist;  //セーブ用
    public List<int> used_event;

    //-----MapClass セーブ用-------
    public float maporiginx;
    public float maporiginy;

    public int mapxnum;
    public int mapynum;
    //--------------------------------


    //Save
    public override void Save()
    {
        base.Save();
        //戦闘パートの盤面でセーブ必要なものをJSONに（シリアライズは下で行う）。

        //BattleValから読み込み
        mapdata = BattleVal.mapdata;
        unitlist = BattleVal.unitlist;
        id2index = BattleVal.id2index;
        turn = BattleVal.turn;
        turnplayer = BattleVal.turnplayer;
        actions = BattleVal.actions;
        nameTitle = BattleVal.title_name;
        nameScene = SceneManager.GetActiveScene().name;  //これはNovelSaveと共通化した方がいい？
        used_event = Operation.used_event;

        //MapClassから読み込み
        maporiginx = Mapclass.maporiginx;
        maporiginy = Mapclass.maporiginy;
        mapxnum = Mapclass.mapxnum;
        mapynum = Mapclass.mapynum;



    }
    //Load
    public override void Load()
    {
        base.Load();
        //シーンネームのLoadは呼び出し元（Operation）で先に行っていることに注意

        //戦闘パートの盤面でロード必要なものをJSONから引っ張り、各static変数に代入
        //戦闘パートの盤面でセーブ必要なものをJSONに。（デシリアライズは下で行う）

        //MapClassへ書き込み
        Mapclass.maporiginx = maporiginx;
        Mapclass.maporiginy = maporiginy;
        Mapclass.mapxnum = mapxnum;
        Mapclass.mapynum = mapynum;

        //BattleValへ書き込み
        //mapdataのデシリアライズ
        mapdata = ScriptReader.LoadMapSaveData(mapsavedata);
        BattleVal.mapdata = mapdata;
        BattleVal.unitlist = unitlist;
        BattleVal.id2index = id2index;
        BattleVal.turn = turn;
        BattleVal.turnplayer = turnplayer;
        BattleVal.actions = actions;
        Operation.used_event = used_event;
        

    }
    //シリアライズ
    public override void OnBeforeSerialize()
    {
        //パーティメンバーとかのシリアライズは親が行う
        base.OnBeforeSerialize();

        //mapdataのシリアライズ
        mapsavedata = ScriptReader.CreateMapSaveData(mapdata);

        //unitlistのシリアライズ
        unitsavelist = new List<UnitSaveData>();
        unitdirection = new List<Vector3>();
        foreach (Unitdata unit in unitlist)
        {
            unitsavelist.Add(new UnitSaveData(unit));
            unitdirection.Add(unit.gobj.transform.forward);
        }

        //id2indexのシリアライズ
        id2indexkeys = new List<string>();
        id2indexvalues = new List<UnitSaveData>();
        foreach (KeyValuePair<string, Unitdata> kvp in id2index)
        {
            id2indexkeys.Add(kvp.Key);
            id2indexvalues.Add(new UnitSaveData(kvp.Value));
        }

        //Action Stackのシリアライズ
        Stack<Action> temp = new Stack<Action>();

        while(actions.Count > 0)
        {
            temp.Push(actions.Pop());
        }

        actionslist = new List<ActionSaveData>();
        while(temp.Count > 0)
        {
            actionslist.Add(new ActionSaveData(temp.Pop()));
        }

        //actionsを元に戻す
        foreach (ActionSaveData act in actionslist)
        {
            actions.Push(new Action(act, id2index));
        }
    }
    //デシリアライズ
    public override void OnAfterDeserialize()
    {
        //パーティメンバーとかのデシリアライズは親が行う
        base.OnAfterDeserialize();

        //unitlistのデシリアライズ
        unitlist = new List<Unitdata>();
        //keyValuePairUnitDirectionのデシリアライズ
        keyValuePairsUnitdirection = new Dictionary<Unitdata, Vector3>();
        //id2indexのデシリアライズ
        id2index = new Dictionary<string, Unitdata>();
        for(int i = 0; i < id2indexkeys.Count; i++)
        {
            Unitdata temp = new Unitdata(id2indexvalues[i], false);

            id2index.Add(id2indexkeys[i], temp);
            unitlist.Add(temp);
            keyValuePairsUnitdirection.Add(temp, unitdirection[i]);
        }

        //Action Stackのデシリアライズ
        actions = new Stack<Action>();
        foreach (ActionSaveData act in actionslist)
        {
            actions.Push(new Action(act, id2index));
        }
    }

}
