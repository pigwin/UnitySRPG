using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//クエリ
//コマンド、テキストのひとまとまりをそれぞれ、クエリと呼ぶことにする。
//クエリ毎に、Scriptionクラスでまとめる。
/************************************************************/
/*それぞれのコマンドを一括管理が可能にするための親クラス　　*/
/************************************************************/
public class Scription
{
    public string text;
    public COMMAND_TYPE type;
    public Scription(string text, COMMAND_TYPE type) { this.text = text; this.type = type; }
}