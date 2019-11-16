using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/*********************************************************/
/*COLOR 色処理系                                         */
/*　背景の色を変更するコマンドからの情報を処理するクラス */
/*********************************************************/
public class COLOR_scription : Scription
{
    //色情報を格納する変数
    [SerializeField]
    private Color _color;
    public Color color
    {
        get { return _color; }
    }
    //アニメーションを行う時間を格納する変数
    [SerializeField]
    private int _timmer;
    public int timmer
    {
        get { return _timmer; }
    }
    //アニメーションのタイプを格納する変数
    [SerializeField]
    private ANIMATION_TYPE _animation;
    public ANIMATION_TYPE animation
    {
        get { return _animation; }
    }
    public COLOR_scription(Color color, string str, int time, ANIMATION_TYPE animation, COMMAND_TYPE t) : base(str, t) { this._color = color; this._timmer = time; this._animation = animation; }
    public COLOR_scription(Color color, string str, COMMAND_TYPE t) : base(str, t) { this._color = color; }
}