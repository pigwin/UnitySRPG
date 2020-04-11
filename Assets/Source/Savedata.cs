using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/***************************************************************/
/*SaveData savedata セーブデータ                               */
/*  セーブデータはこれを継承することでセーブをすることができる */
/***************************************************************/
//セーブデータの親クラス
public class Savedata : ISerializationCallbackReceiver
{

    //どこのセーブかの識別子

    public string current_message;

    //Scene名
    public string nameScene = "";

    //Title名
    public string nameTitle = "";

    //パーティリスト
    public List<UnitSaveData> playerUnitList = new List<UnitSaveData>(); //そのままセーブ可能
    //特定のキャラクタとIDの紐づけ（強制出撃や、出撃不可設定に使用する）
    //紐づいていないキャラは、汎用ユニットとして扱う。
    //汎用ユニットというのは、ユーザーが任意に雇用したりできるキャラ（将来的に実装）
    public Dictionary<int, UnitSaveData> id2unitdata = new Dictionary<int, UnitSaveData>();
    public List<int> idlist = new List<int>(); //セーブ用
    public List<UnitSaveData> unitdatawithidlist = new List<UnitSaveData>(); //セーブ用

    public PartySettings partySettings;
    public List<string> partysetting_string_serialize = new List<string>(); //セーブ用

    public bool is_clear_mugen = false; //2022用内部データ
    public bool is_story_clear = false; //ゲームオーバー時のステージセレクトへのバック用

    //キャラクタ加入
    public void UnitAdd(UnitSaveData unit, int initlevel = 0)
    {
        //仮；ユニットがすでに加入されている場合は加入処理を飛ばす
        foreach(UnitSaveData tempunit in playerUnitList)
        {
            if (unit.scobj == tempunit.scobj)
                return;
        }
        if (initlevel != 0)
            unit.level = initlevel;
        playerUnitList.Add(unit);

        //もしもパーティメンバーの設定上IDと紐づけられているキャラが加入した場合
        if(partySettings.party_charalist.ContainsValue(unit.scobj))
        {
            int key = -1;
            foreach (KeyValuePair<int, string> kvp in partySettings.party_charalist)
            {
                if (kvp.Value == unit.scobj) key = kvp.Key;
            }
            if (key != -1) JointId2Unit(key, unit);
        }
        else
        {
            //サモン・ミリタリー2020/3月用
            //パーティメンバーをIDと紐づける。
            int key = partySettings.party_charalist.Count + 1;
            partySettings.party_charalist.Add(key, unit.scobj);
            JointId2Unit(key, unit);
        }
    }

    //ステージクリア後のキャラクタのアップデート
    public void UnitUpdate(int id, UnitSaveData updateunit)
    {
        id2unitdata[id].level = updateunit.level;
        id2unitdata[id].exp = updateunit.exp;


    }

    //キャラクタ除名
    public void UnitRemove(UnitSaveData unit)
    {
        playerUnitList.Remove(unit);
        //もし紐づけされたキャラなら
        if(id2unitdata.ContainsValue(unit))
        {
            int key = -1;
            foreach(KeyValuePair<int,UnitSaveData> kvp in id2unitdata)
            {
                if (kvp.Value.scobj == unit.scobj) key = kvp.Key; 
            }
            if (key != -1) id2unitdata.Remove(key);
            partySettings.party_addmission[key] = false;
        }
    }

    //IDとユニットの紐づけ
    public void JointId2Unit(int id, UnitSaveData unit)
    {
        //念のため
        if (!playerUnitList.Contains(unit))
        {
            UnitAdd(unit);
        }
        if(!id2unitdata.ContainsKey(id)) id2unitdata.Add(id, unit);
        unit.partyid = id;
        //加入状況の更新
        partySettings.party_addmission[id] = true;
    }



    //Save
    public virtual void Save()
    {
        playerUnitList = GameVal.masterSave.playerUnitList;
        id2unitdata = GameVal.masterSave.id2unitdata;
        partySettings = GameVal.masterSave.partySettings;

        is_clear_mugen = GameVal.masterSave.is_clear_mugen;
        is_story_clear = GameVal.masterSave.is_story_clear;
    }

    //Load
    public virtual void Load()
    {
        GameVal.masterSave.playerUnitList = playerUnitList;
        //id2unitdataとplayerUnitListには紐づけが必要。
        id2unitdata = new Dictionary<int, UnitSaveData>();
        foreach (UnitSaveData unit in playerUnitList)
        {
            //もしもパーティメンバーの設定上IDと紐づけられているキャラが加入した場合
            if (partySettings.party_charalist.ContainsValue(unit.scobj))
            {
                int key = -1;
                foreach (KeyValuePair<int, string> kvp in partySettings.party_charalist)
                {
                    if (kvp.Value == unit.scobj) key = kvp.Key;
                }
                if (key != -1) JointId2Unit(key, unit);
            }
        }
        GameVal.masterSave.id2unitdata = id2unitdata;
        GameVal.masterSave.partySettings = partySettings;

        GameVal.masterSave.is_clear_mugen = is_clear_mugen;
        GameVal.masterSave.is_story_clear = is_story_clear;
    }

    //シリアライズ
    public virtual void OnBeforeSerialize()
    {
        idlist = new List<int>();
        unitdatawithidlist = new List<UnitSaveData>();

        foreach(KeyValuePair<int,UnitSaveData> kvp in id2unitdata)
        {
            idlist.Add(kvp.Key);
            unitdatawithidlist.Add(kvp.Value);
        }
        //partySettingsはstringで保存することで復元可能
        partySettings.Dictionary2string();
        partysetting_string_serialize = partySettings.party_settings_string;
    }
    //デシリアライズ
    public virtual void OnAfterDeserialize()
    {
        id2unitdata = new Dictionary<int, UnitSaveData>();
        for(int i=0; i<idlist.Count; i++)
        {
            id2unitdata.Add(idlist[i], unitdatawithidlist[i]);
        }
        partySettings = ScriptableObject.CreateInstance<PartySettings>();
        partySettings.party_settings_string = partysetting_string_serialize;
        partySettings.string2Dictionary();
    }
}
