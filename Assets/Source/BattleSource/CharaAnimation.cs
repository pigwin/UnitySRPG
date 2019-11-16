using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//キャラクターのアニメーションを制御するための抽象クラス
//効果音再生・ボイス再生の終了などを監視するために使用する。
public class CharaAnimation : MonoBehaviour
{
    //Audiosource
    public AudioSource voiceSource;
    public AudioSource seSource;

    //ピンチフラグ
    [System.NonSerialized] public bool isPinch = false;

    //被ダメージ・回避モーションの開始フラグ（Hitアニメーション等に使用）
    protected bool _isHit = false;
    public bool isHit
    {
        get
        {
            if (_isHit)
            {
                _isHit = false; //reset flag
                return true;
            }
            else
                return false;
        }
    }

}
