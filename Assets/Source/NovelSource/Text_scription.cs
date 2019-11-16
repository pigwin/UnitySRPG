using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/************************************************************/
/*会話文や、地の文が記述されたときの情報を格納するクラス    */
/************************************************************/
public class Text_scription : Scription
{
    [SerializeField]
    private int _counter;
    [SerializeField]
    private float _timer;
    //キャラクター名を格納する変数
    [SerializeField]
    private string _character_name;
    [SerializeField]
    private bool _check;
    public bool check
    {
        get { return _check; }
    }
    public string character_name
    {
        get { return _character_name; }
    }
    public Text_scription(string chara, bool check, string str, COMMAND_TYPE t) : base(str, t)
    {
        this._character_name = chara;
        this._check = check;
    }
    public int counter
    {
        get { return _counter; }
        set { _counter = value; }
    }
    public float timer
    {
        get { return _timer; }
        set { _timer = value; }
    }
}