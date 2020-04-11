using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ニューゲームを始めた時の設定とか
public class NewGameSetting : MonoBehaviour
{
    public PartySettings initPartySettings;
    public string firstSceneName;
    public void PushButton()
    {
        //GameValの設定
        GameVal.nextscenename = firstSceneName;

        GameVal.masterSave.partySettings = initPartySettings;
        GameVal.masterSave.playerUnitList = new List<UnitSaveData>();
        GameVal.masterSave.id2unitdata = new Dictionary<int, UnitSaveData>();
        GameVal.masterSave.is_story_clear = false;
        GameVal.masterSave.is_clear_mugen = false;

        initPartySettings.string2Dictionary();
        //パーティメンバーの設定を行う
        foreach(KeyValuePair<int,string> kvp in initPartySettings.party_charalist)
        {
            //もし初期加入になっている場合
            if(initPartySettings.party_addmission[kvp.Key])
            {
                UnitSaveData temp = new UnitSaveData(kvp.Value, kvp.Key);
                GameVal.masterSave.UnitAdd(temp);
            }
        }

        //Debug.Log(GameVal.masterSave.id2unitdata[1].scobj);
    }

}
