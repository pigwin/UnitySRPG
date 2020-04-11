using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

//状態異常の設定データ
public class Condition : ScriptableObject
{
    [Header("ファイル名（ロードに必要）")]
    public string filename;
    [Header("状態異常名")]
    public string condname;
    [Header("スリップダメージ　最大HPに対する倍率（実数）：負で回復 ／絶対値1以上で固定値")]
    public float slipdamage;
    [Header("状態異常にかかる確率")]
    [Range(0,1)]
    public float percent;

    [Header("ステータス変化倍率（実数）")]
    public float maxhpsup = 1.0f;
    public float atksup = 1.0f;
    public float defsup = 1.0f;
    public float matsup = 1.0f;
    public float mdfsup = 1.0f;
    public float tecsup = 1.0f;
    public float lucsup = 1.0f;

    [Header("歩数・ジャンプ変化（整数）")]
    public int stepsup = 0;
    public int jumpsup = 0;

    [Header("移動不能化フラグ")]
    public bool forbiddenMove = false;

    [Header("攻撃不能化フラグ")]
    public bool forbiddenAttack = false;

    [Header("魔法使用不可フラグ")]
    public bool forbiddenMagic = false;

    [Header("持続ターン数 -1で永続")]
    public int contiturn;

    [Header("バッドステータスフラグ（状態異常回復スキル判定）")]
    public bool isbad = true;

    [Header("強化解除無効フラグ")]
    public bool isresistdisarm = false;

    //ToDo 毒無効、などのレジスト系の状態異常も対応できるように
    [Header("発動条件となる状態異常名")]
    public string need_condition = "";

    [Header("両立しない状態異常名")]
    public List<string> resist_condition;

    [Header("物理攻撃による解除フラグ")]
    public bool is_curebyattack = false;

    [Header("バトルで状態異常にかかった時にポップアップするGameObject")]
    public GameObject prefabPopup;
    [Header("バトルで状態異常にかかった時のSE")]
    public AudioClip se;

    [Header("状態異常中のエフェクト")]
    public GameObject prefabEffect;
    [System.NonSerialized]
    public GameObject gobjEffect;

    [Header("アイコン")]
    public Sprite icon;
    [Header("状態異常パネルカラー")]
    public Color panelcolor;
    [Header("状態異常説明文")]
    public string strdetail;

    [System.NonSerialized]
    public ConditionSaveData nowstate;

    //状態異常の追加
    //付与確率も計算する
    public void SetCondition(Unitdata unit, bool instantiateflag = true)
    {
        nowstate = new ConditionSaveData();
        nowstate.nowturn = contiturn;
        nowstate.scobj = filename;
        if (!instantiateflag) return;
        InstantiateEffect(unit);
    }
    //エフェクト描画
    public void InstantiateEffect(Unitdata unit)
    {
        Vector3 reffect = new Vector3();
        Mapclass.TranslateMapCoordToPosition(ref reffect, unit.x, unit.y);
        try
        {
            gobjEffect = Instantiate(prefabEffect, reffect, prefabEffect.transform.rotation);
            gobjEffect.transform.parent = unit.gobj.transform;
        }
        catch(UnassignedReferenceException)
        {
            //エフェクトなしの場合
        }
        catch(NullReferenceException)
        {

        }
        
    }
    //ターン経過
    //引数：経過数（デフォルトは1ターン、2倍の速さで消費するなどにも対応可能）
    //戻り値：trueで状態異常回復
    public bool Consume(int count = 1)
    {
        nowstate.nowturn -= count;
        if (nowstate.nowturn <= 0 && contiturn != -1)
        {
            Destroy(gobjEffect);
            return true;
        }
        return false;
    }

    public int SlipDamage(int maxhp)
    {
        if (slipdamage < 1)
            return (int)((float)maxhp * slipdamage);
        else
            return (int)slipdamage;
    }

    public bool is_Effective(Unitdata temp, ref int removekey)
    {
        //耐性があるか？
        if (temp.status.list_resistconditions.Contains(condname))
        {
            return false;
        }
        //もう既に罹患していたらスキップ
        //両立しない状態異常に罹患していたらスキップ
        foreach (Condition cond in temp.status.conditions)
        {
            if (cond.condname == condname
                || resist_condition.Contains(cond.condname))
            {
                return false;
            }
        }
        removekey = -1;
        //条件となる状態異常に罹患していなかったらスキップ
        if (need_condition != "")
        {
            for (int i = temp.status.conditions.Count - 1; i >= 0; i--)
            {
                if (need_condition == temp.status.conditions[i].condname)
                {
                    //条件の状態異常を解除
                    removekey = i;

                    return true;
                }
            }
            return false;
        }

        return true;
    }

}

//セーブ用
[System.Serializable]
public class ConditionSaveData
{
    public int nowturn;
    public string scobj;
    
}