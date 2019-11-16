using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***************************************************************/
/*IMAGE_clear 画像消去                                         */
/*　画像を消去するコマンドの情報を処理するためのクラス         */
/***************************************************************/
public class IMAGE_clear_scription : Scription
{
    //削除する画像の場所を格納する変数
    [SerializeField]
    private int _place;//ビット位置で場所を決定 1:l 2:c 4:r 7:all clear
    //アニメーションを行う時間を格納する変数
    [SerializeField]
    private int _timmer;
    //アニメーションの種類を格納する変数
    [SerializeField]
    private ANIMATION_TYPE _animation;
    public IMAGE_clear_scription(int place, ANIMATION_TYPE animation_type, int animtime, string str, COMMAND_TYPE type) : base(str, type)
    {
        _place = place;
        _timmer = animtime;
        _animation = animation_type;
    }
    public int place
    {
        get { return _place; }
    }
    public int timmer
    {
        get { return _timmer; }
    }
    public ANIMATION_TYPE animation
    {
        get { return _animation; }
    }
}
