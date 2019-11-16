using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//スキルの設定データ
public class Skill : ScriptableObject
{
    [Header("スキル名")]
    public string skillname;   //スキル名
    [Header("スキル威力倍率（実数）；負で回復スキルに")]
    public float s_atk;          //威力（負の場合回復）
    [Header("魔法攻撃フラグ")]
    public bool is_mgc;        //魔力依存かどうか
    [Header("防御無視フラグ")]
    public bool is_ignoredef;   //防御無視かどうか
    [Header("攻撃範囲")]
    public AttackRange attackrange;
    [Header("効果範囲")]
    public AttackRange arearange;
    [Header("カットインフラグ")]
    public bool is_cutscene;
    [Header("カットインPrefab")]
    public GameObject prefab_cutin;
    [Header("モーション名")]
    public string animname;
    [Header("スキルエフェクト")]
    public GameObject skillefect;

    [Header("最大使用回数")]
    public int maxuse;          //最大使用回数

    [Header("スキルアイコン")]
    public Sprite icon;
    [Header("スキルカラー")]
    public Color skillColor;
    [Header("スキル説明文")]
    public string skilldetail;

    [System.NonSerialized]
    public int use;             //のこり使用回数保存用

    public void Consume(int count)
    {
        use -= count;
    }
    public void Recover(int count)
    {
        use += count;
    }
}

[System.Serializable]
public class SkillTree
{
    [Header("スキル")]
    public Skill skill;
    [Header("習得レベル")]
    public int getlevel;
}
