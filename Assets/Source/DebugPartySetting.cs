using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPartySetting : MonoBehaviour
{
    [Header("他ステージと通しプレイする時は無効化すること")]
    public PartySettings debugPartySettings;
    [Header("デバッグ用レベル設定；要素数に対応するキャラのレベルを変更 id,level")]
    public List<string> debuglevel = new List<string>(); 
    // Start is called before the first frame update
    void Start()
    {
        if (!Debug.isDebugBuild) return; //リリース時は以下を読み込まない
        //GameValの設定
        GameVal.masterSave.playerUnitList = new List<UnitSaveData>();
        GameVal.masterSave.id2unitdata = new Dictionary<int, UnitSaveData>();
        //debugPartySettings.string2Dictionary();
        GameVal.masterSave.partySettings = ScriptableObject.CreateInstance<PartySettings>(); //パーティメンバーの設定を行う
        GameVal.masterSave.partySettings.party_settings_string = new List<string>(debugPartySettings.party_settings_string);
        GameVal.masterSave.partySettings.string2Dictionary();
        foreach (KeyValuePair<int, string> kvp in GameVal.masterSave.partySettings.party_charalist)
        {
            //もし初期加入になっている場合
            if (GameVal.masterSave.partySettings.party_addmission[kvp.Key])
            {
                UnitSaveData temp = new UnitSaveData(kvp.Value,kvp.Key);
                GameVal.masterSave.UnitAdd(temp);
            }
        }

        for(int i=0; i<debuglevel.Count; i++)
        {
            string[] temp = debuglevel[i].Split(',');

            if(int.Parse(temp[1]) != 0 && GameVal.masterSave.id2unitdata.ContainsKey(int.Parse(temp[0])))
            {
                int templevel = int.Parse(temp[1]);
                int tempid = int.Parse(temp[0]);
                GameVal.masterSave.id2unitdata[tempid].level = templevel;
            }
        }
        
    }

}
