using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/***********************************************/
/*AUDIO 音響系                                 */
/*  音響系のコマンドからの情報を処理するクラス */
/***********************************************/
public class AUDIO_scription : Scription
{
    //音響ファイルを保存する変数
    [SerializeField]
    private AudioClip _audio;
    //音響ファイルの名前を保存する変数
    [SerializeField]
    private string _audio_name;
    public AudioClip audio
    {
        get { return _audio; }
    }
    public string audio_name
    {
        get { return _audio_name; }
    }
    public AUDIO_scription(AudioClip audio, string str,string audioname, COMMAND_TYPE t) : base(str, t)
    {
        _audio_name = audioname;
        this._audio = audio;
    }
    public AUDIO_scription(AUDIO_scription main, AudioClip audio, string str, COMMAND_TYPE t) : base(str, t)
    {
        this._audio_name = main.audio_name;
        _audio = audio;
    }
}