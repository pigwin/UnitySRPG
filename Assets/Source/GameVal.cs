using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*****************************************/
/*GameVal gameval ゲーム変数             */
/* ゲーム全体で共有する際に用いる変数群  */
/*****************************************/
//ノベルパート・バトルパート・それ以外のパートすべてで共有されるpublic staticな変数
//シーンを跨いで使う変数などを格納する
public class GameVal : MonoBehaviour {

    //次のシーンに行くためのシーン名保存変数（セーブ・次のステージ用）
    public static string nextscenename;

    //パーティメンバー等保存のためのもの
    public static Savedata masterSave = new Savedata();

}
