using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PartySettings : ScriptableObject
{
    //パーティメンバーの設定
    public Dictionary<int,string> party_charalist; //キャラスクリプタブルオブジェクトとIDの紐づけ（初期登用に使用する）
    public Dictionary<int, bool> party_addmission; //キャラの加入状況とキャラIDの紐づけ

    [Header("ID,ファイル名（Resource/BattleChara/以下）,1で初期加入")]
    public List<string> party_settings_string;

    //stringをDictionaryにする
    public void string2Dictionary()
    {
        party_charalist = new Dictionary<int, string>();
        party_addmission = new Dictionary<int, bool>();

        foreach(string setting in party_settings_string)
        {
            string[] temp = setting.Split(',');
            party_charalist.Add(int.Parse(temp[0]), temp[1]);
            if (int.Parse(temp[2]) == 1)
                party_addmission.Add(int.Parse(temp[0]), true);
            else
                party_addmission.Add(int.Parse(temp[0]), false);
        }
    }

    //Dictionaryをstringにする
    public void Dictionary2string()
    {
        party_settings_string = new List<string>();
        foreach(KeyValuePair<int,string> kvp in party_charalist)
        {
            if(party_addmission[kvp.Key])
                party_settings_string.Add(string.Format("{0},{1},{2}",kvp.Key,kvp.Value,1));
            else
                party_settings_string.Add(string.Format("{0},{1},{2}",kvp.Key,kvp.Value,0));
        }
    }
}
