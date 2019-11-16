using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/************************************************************/
/*JUMP 読み込み位置変更                                     */
/*  読む場所をジャンプするコマンドを利用したときに情報を    */
/*  しょりするクラス                                        */
/************************************************************/
public class JUMP_scription : Scription
{
    [SerializeField]
    private string label_name;
    private int _jump_line;
    private string _file_name;
    public JUMP_scription(string label_name, int jump_line, string file_name, string str, COMMAND_TYPE type) : base(str, type)
    {
        this.label_name = label_name;
        this._jump_line = jump_line;
        this._file_name = file_name;
    }
    public int jump_line{
        get { return _jump_line; }
    }
    public string file_name
    {
        get { return _file_name; }
    }
}
