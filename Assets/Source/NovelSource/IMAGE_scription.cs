using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/************************************************************/
/*IMAGE 画像系コマンドの処理                                */
/*　画像を表示するコマンドの情報を格納するクラス            */
/************************************************************/
//画像関連のScriptionクラス

public class IMAGE_scription : Scription
{
    //画像データ
    [SerializeField]
    private Sprite _image;
    //画像ファイルの名前
    [SerializeField]
    private string _imagename;
    public Sprite image
    {
        get { return _image; }
    }
    public string imagename
    {
        get { return _imagename; }
    }
    //アニメーションの種類
    [SerializeField]
    private ANIMATION_TYPE _animation;
    public ANIMATION_TYPE animation
    {
        get { return _animation; }
    }
    //アニメーションの時間
    [SerializeField]
    private int _timmer;
    public int timmer
    {
        get { return _timmer; }
    }

    public IMAGE_scription(Sprite spr, ANIMATION_TYPE animation, string str, int time, string image_name, COMMAND_TYPE t) : base(str, t)
    {
        this._image = spr;
        this._animation = animation;
        this._timmer = time;
        this._imagename = image_name;
    }
    public IMAGE_scription(IMAGE_scription main,Sprite spr,string str,COMMAND_TYPE t) : base(str,t)
    {
        this._animation = main.animation;
        this._timmer = main.timmer;
        this._imagename = main.imagename;
        this._image = spr;
    }
}
/****************************************************************/
/*このクラスはIMAGE_scriptionの中でも特に、立ち絵を描画する際に */
/*用いるクラス                                                  */
/****************************************************************/
public class STAND_IMAGE_scription : IMAGE_scription
{
    //立ち絵を描画する場所を表す変数
    [SerializeField]
    private int _place;
    public int place
    {
        get { return _place; }
    }

    public STAND_IMAGE_scription(int place, Sprite spr, ANIMATION_TYPE animation, string str,
        int time, string image_name, COMMAND_TYPE t) : base (spr,animation, str, time, image_name, t)
    {
        _place = place;
    }
    public STAND_IMAGE_scription(int place, IMAGE_scription main, Sprite spr, string str, COMMAND_TYPE t) : base(main, spr, str, t)
    {
        _place = place;
    }
}

//UI変更等で画像の名前と座標だけ分かっていればよい場合
public class UI_IMAGE_scription : Scription
{
    [SerializeField]
    private float _x;
    [SerializeField]
    private float _y;

    public float x
    {
        get { return _x; }
    }
    public float y
    {
        get { return _y; }
    }

    public UI_IMAGE_scription(float x, float y, string imagename, COMMAND_TYPE t) : base(imagename,t)
    {
        _x = x;
        _y = y;
    }
}