using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/************************************************************/
/*待機系統のコマンドの情報をもつクラス                      */
/************************************************************/
public class WAIT_scription : Scription
{
    [SerializeField]
    private int _counter;
    //このコマンドを飛ばしても大丈夫か否かを判定するフラグ
    [SerializeField]
    private bool _skipflag;

    public int counter
    {
        get { return _counter; }
    }
    public bool skipflag
    {
        get { return _skipflag; }
    }

    public WAIT_scription(int counter, bool skipflag, string str, COMMAND_TYPE t) : base(str, t)
    {
        _counter = counter;
        _skipflag = skipflag;
    }
}