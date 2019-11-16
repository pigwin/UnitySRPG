//コマンド
//******************************************//
//Command command コマンド 命令             //
//  COMMAND_TYPEはノベルゲームのNScripter   //
//  を実現するためのコマンドを列挙定数で    //
//  定義をしている。                        //
//　                                        //
//  これを追加することで、ノベルゲーム用の  //
//  コマンドを追加することができる。        //
//******************************************//
public enum COMMAND_TYPE
{
    NORMAL_TEXT,            //通常テキスト表示
    NORMAL_TEXT_CONTINUE,   //@で止まる時
    BACKGROUND,             //
    WAIT_B,
    WAIT_W,
    WAIT_S,                 //文字速度変更命令
    WAIT,                   //delay命令 wait命令
    TEXTOFF,                //textoff命令　UIを非表示にする。
    TEXTON,                 //texton命令　UIを表示する。
    COLOR,                  //文字色変更命令
    BACKGROUND_COLOR,       //bg命令　カラーコード
    BACKGROUND_IMAGE,       //bg命令　画像
    STAND_IMAGE,            //ld命令
    AUDIO_BGM,              //bgm命令
    AUDIO_BGM_STOP,         //bgmstop命令
    AUDIO_VOICE,            //タグ[name,"voice"]中のボイス（引数2）再生 voice命令
    AUDIO_VOICE_STOP,         //voicestop命令
    AUDIO_SE,               //se命令
    AUDIO_SE_STOP,         //sestop命令
    STAND_IMAGE_CLEAR,       //cl命令
    SETWINDOW,               //setwindow命令
    JUMP,                    //jump命令 ラベル名
    MONOCRO_ON,              //monocro命令
    MONOCRO_OFF,            //monocro off命令
    PARTY_ADD,               //パーティにメンバーを加入する
    PARTY_REMOVE,           //パーティのメンバーを離脱させる
    CALCULA_VARY,            //変数宣言と代入
    CALCULA_ADD,             //変数の加算
    CALCULA_SUB,            //変数の減算
    CALCULA_DIV,            //変数の除算
    CALCULA_MUL,            //変数の乗算
    CALCULA_MOD,            //変数のモジュロ計算
    RETURN                  //終了命令
};
//**********************************************************//
//Animation animation アニメーション                        //
//  エフェクトアニメーションをいじるための列挙定数          //
//  アニメーションを追加する場合はここで定数を追加後        //
//  アニメーション用のプログラムを作成すること              //
//**********************************************************//
//エフェクト番号
public enum ANIMATION_TYPE
{
    ANIMATION_SYNCHRO = 0,    //次のアニメと同時
    ANIMATION_NO_TIME = 1,    //瞬間表示
    MASKRULE = 2,
    FADE_IN=10
};
//**********************************************************//
//Drawing drawing 描画                                      //
//  ここでは、アニメーション描画時のステート遷移を定義する  //
//  基本的にはいじることはないと思うが、ノベルゲームの      //
//  描画時の様々な状態遷移を必要とする場合、ここに状態を    //
//  追加する。                                              //
//**********************************************************//
//ノベルエンジン（Scripter.cs）MAIN描画部分のステート
public enum NOVEL_STATUS
{
    NEXT,                   //次の行へ
    DELAY,                  //タイマーによる遅延
    UIHIDE,                 //テキストボックスを隠す
    UISHOW,                 //テキストボックスを表示する
    WRITING,                //文字を描いている
    ANIMATION,              //アニメーション（エフェクトによる画面切り替え）中
    PRE_ANIMATION,          //アニメーション準備
    WAITING,                 //クリック待ち
    FINISH
};
//**********************************************************//
//Menu menu メニュー                                        //
//　ノベルゲーム中のメニューに関する状態遷移用の列挙定数    //
//  他のメニューをいじる場合はここを追加、もしくはこれに    //
//  関係する部分を変更することでメニューの内容を変更する    //
//  ことができる                                            //
//**********************************************************//
//ノベルエンジン（Scripter.cs）描画部分のステート
public enum SCREEN_STATUS
{
    MAIN,                   //ノベルエンジンメイン
    BACK_LOG,               //バックログ表示モード
    SAVE,                   //セーブ画面
    LOAD,                   //ロード画面
    FADE_OUT                //ロード等のシーンチェンジ
};
    