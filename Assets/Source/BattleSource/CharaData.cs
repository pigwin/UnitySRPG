using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//ステータス管理用のキャラデータ
public class CharaData : ScriptableObject
{
    [Header("基本設定")]
    [Header("キャラクター名")]
    public string charaname;
    [Header("ジョブ名")]
    public string jobname;
    [Header("マップ上の３Ｄモデル")]
    public GameObject gobj;
    [Header("顔グラフィック")]
    public Sprite faceimage;

    [Header("攻撃範囲")]
    public AttackRange attackrange;
    [Header("攻撃エフェクト")]
    public GameObject attackeffect;

    [Header("ステータス")]
    public Status init_status;
    [Header("陣営")]
    [Tooltip("0:味方　1:敵　2:障害物(未実装)")]
    public int init_team; //0:味方　1:敵　2:障害物
    [Header("パッシブスキル")]
    [Tooltip("ZOCフラグ")]
    public bool zocflag = false;
    [Range(1,2)]
    [Tooltip("ZOCレベル；1で四方、2で八方")]
    public int zoclevel=1;
    [Header("保有スキル")] //Skill.cs参照
    public List<SkillTree> skillTrees;

    [Header("エネミーの思考設定")]
    public EnemyRoutin routin; //EnemyAI.cs参照
}

//戦闘に関わるステータス
[System.Serializable]
public class Status
{
    [Tooltip("レベル")]
    [Range(1,99)]
    public int level;
    [System.NonSerialized]
    public int exp;
    [Tooltip("歩数")]
    public int step=1;
    [Tooltip("跳躍")]
    public int jump=1;

    //成長曲線
    [Header("最大ＨＰ")]
    public Growcurve maxhpcurve;
    [Header("攻撃力")]
    public Growcurve atkcurve;
    [Header("防御力")]
    public Growcurve defcurve;
    [Header("魔法攻撃力")]
    public Growcurve matcurve;
    [Header("魔法防御力")]
    public Growcurve mdfcurve;
    [Header("技量")]
    public Growcurve teccurve;
    [Header("運")]
    public Growcurve luccurve;

    //必要経験値
    [Header("必要経験値／撃破時取得経験値")]
    public Growcurve needexpcurve;

    public int maxhp { get { return maxhpcurve.value(level); } }
    public int atk { get { return atkcurve.value(level); } }
    public int def { get { return defcurve.value(level); } }
    public int mat { get { return matcurve.value(level); } }
    public int mdf { get { return mdfcurve.value(level); } }
    public int tec { get { return teccurve.value(level); } }
    public int luc { get { return luccurve.value(level); } }
    public int needexp { get { return needexpcurve.value(level); } }
}

//戦闘に関わるステータス
//キャラクタの成長曲線
//https://blogs.yahoo.co.jp/fermiumbay2/41015350.html  参照
[System.Serializable]
public class Growcurve
{
//    [Header("早熟かどうか")]
    private bool is_precocious = true;

    [Header("指数（大きいほど急成長, 0で一定）")]
    [Range(0,2)]
    public float exponent = 1.0f;

    [Header("Lv1の時の値")]
    public int minval;
    [Header("Lv99の時の値")]
    public int maxval;

    public int value(int level)
    {
        //早熟
        if (is_precocious) return (int)(-(float)(maxval - minval) 
                * Mathf.Pow(((float)(level - 99) / (float)(1 - 99)), exponent) + (float)maxval);
        else return (int)((float)(maxval - maxval)
                * Mathf.Pow(((float)(level - 1) / (float)(99 - 1)), exponent) + (float)minval);
    }
}

//ゲーム中で実際に使用されるキャラデータ
public class Unitdata
{
    //データから読み込むもの
    public int id; //ロード順に決定される
    public string charaname;
    public AttackRange attackrange;
    public string jobname;
    public Status status;
    public Sprite faceimage;
    public int team;
    public bool zocflag;
    public int zoclevel=1;
    public List<SkillTree> skillTrees; //レベルアップ時に参照
    public List<Skill> skills;
    public EnemyRoutin routin;

    public GameObject gobj_prefab;
    public GameObject gobj; //キャラのGameObject
    public GameObject attackeffect; 

    //ゲーム中変わるもの
    public int x, y; //マップ内座標
    public int hp;
    //public int exp; //経験値
    public bool movable; //行動可能か（ターンチェンジ時にtrueに、移動するとfalseに）
    public bool atackable; //攻撃可能か

    //セーブ用
    public string savescobj;
    //パーティメンバーの場合1以上の値を持つ
    public int partyid = 0;

    private static int idcount = 0;
    //Scriptable Objectからのキャラ読み込み（コンストラクタ）
    public Unitdata(string scobj, int level = -1, int partyid = 0)
    {
        CharaData loaddata = ScriptableObject.Instantiate(Resources.Load(string.Format("BattleChara/{0}",scobj)) as CharaData);
        id = 0;
        //idcount中に未使用があるかを探索する
        for (int tempid = 1; tempid < idcount; tempid++)
        {
            bool contflag = false;
            foreach (Unitdata tempunit in BattleVal.unitlist)
            {
                if (tempunit.id == tempid)
                {
                    contflag = true;
                    break;
                }
            }
            if (contflag) continue;
            //ここに到達したという事は、そのtempidは使用されていない
            id = tempid;
        }
        if (id == 0)
        {
            id = idcount + 1;
            idcount++;
        }
        charaname = loaddata.charaname;
        jobname = loaddata.jobname;
        attackrange = loaddata.attackrange;
        status = loaddata.init_status;
        if (level > 0) status.level = level;
        faceimage = loaddata.faceimage;
        team = loaddata.init_team;
        gobj_prefab = loaddata.gobj;
        gobj = MonoBehaviour.Instantiate(gobj_prefab, new Vector3(), Quaternion.identity);
        attackeffect = loaddata.attackeffect;
        zocflag = loaddata.zocflag;
        zoclevel = loaddata.zoclevel;
        skillTrees = loaddata.skillTrees;
        skills = new List<Skill>();
        foreach(SkillTree skilltree in loaddata.skillTrees)
        {
            if (loaddata.init_status.level >= skilltree.getlevel)
                skills.Add(ScriptableObject.Instantiate<Skill>(skilltree.skill));
        }
        //skills = loaddata.skills;
        foreach (Skill skill in skills)
            skill.use = skill.maxuse;
        routin = loaddata.routin;
        savescobj = scobj;
        this.partyid = partyid;
    }

    //ロード時のコンストラクタ
    public Unitdata(UnitSaveData save, bool instantiateflag = true, bool is_unitcheck = false)
    {
        CharaData loaddata = ScriptableObject.Instantiate(Resources.Load(string.Format("BattleChara/{0}", save.scobj)) as CharaData);
        id = 0;
        //戦闘パートの場合
        if(!is_unitcheck)
        {
            //idcount中に未使用があるかを探索する
            for (int tempid = 1; tempid < idcount; tempid++)
            {
                bool contflag = false;
                foreach (Unitdata tempunit in BattleVal.unitlist)
                {
                    if (tempunit.id == tempid)
                    {
                        contflag = true;
                        break;
                    }
                }
                if (contflag) continue;
                //ここに到達したという事は、そのtempidは使用されていない
                id = tempid;
            }
            if (id == 0)
            {
                id = idcount + 1;
                idcount++;
            }
        }
        charaname = loaddata.charaname;
        jobname = loaddata.jobname;
        attackrange = loaddata.attackrange;
        attackeffect = loaddata.attackeffect;
        status = loaddata.init_status;
        faceimage = loaddata.faceimage;
        team = loaddata.init_team;
        gobj_prefab = loaddata.gobj;
        if(instantiateflag)
            gobj = MonoBehaviour.Instantiate(gobj_prefab, new Vector3(), Quaternion.identity);
        zocflag = loaddata.zocflag;
        zoclevel = loaddata.zoclevel;
        skillTrees = loaddata.skillTrees;
        
        //skills = loaddata.skills;
        /*
        foreach (Skill skill in skills)
            skill.use = skill.maxuse;
        */
        routin = loaddata.routin;

        //初期設定ファイルからの変更を獲得
        savescobj = save.scobj;
        status.level = save.level;
        x = save.x;
        y = save.y;
        hp = save.hp;
        status.exp = save.exp;

        skills = new List<Skill>();
        foreach (SkillTree skilltree in loaddata.skillTrees)
        {
            if (status.level >= skilltree.getlevel)
                skills.Add(ScriptableObject.Instantiate<Skill>(skilltree.skill));
        }

        //スキル使用回数
        if (skills.Count > 0)
        {
            for (int numskill = 0; numskill < save.skilluses.Count; numskill++)
            {
                //スキル名の一致するスキルを探索し、使用回数をロードする。
                foreach(Skill skill in skills)
                {
                    if (skill.skillname == save.skillname[numskill])
                    {
                        skill.use = save.skilluses[numskill];
                        break;
                    }
                }
            }
        }
        movable = save.movable;
        atackable = save.atackable;

        partyid = save.partyid;
    }

}

//セーブする必要のあるUnitData
[System.Serializable]
public class UnitSaveData
{
    public int x, y;
    public int hp;
    public int exp;
    public int level;
    public List<int> skilluses;
    public List<string> skillname;
    public string scobj;
    public bool movable;
    public bool atackable;
    public int partyid;

    //セーブデータ抽出
    public UnitSaveData(Unitdata unit)
    {
        x = unit.x;
        y = unit.y;
        hp = unit.hp;
        exp = unit.status.exp;
        scobj = unit.savescobj;
        level = unit.status.level;
        movable = unit.movable;
        atackable = unit.atackable;

        skilluses = new List<int>();
        skillname = new List<string>();
        if (unit.skills.Count > 0)
        {
            foreach (Skill skill in unit.skills)
            {
                skilluses.Add(skill.use);
                skillname.Add(skill.skillname);
            }
        }
        partyid = unit.partyid;
        
    }

    //CharaData(scobj) から CharaSaveDataの作成
    //初期パーティメンバーの設定に使用する
    public UnitSaveData(string scobj, int partyid=0)
    {
        x = 0;
        y = 0;
        exp = 0;
        CharaData chara = ScriptableObject.Instantiate(Resources.Load(string.Format("BattleChara/{0}", scobj)) as CharaData);
        this.scobj = scobj;
        level = chara.init_status.level;
        hp = chara.init_status.maxhp;
        movable = true;
        atackable = true;
        skilluses = new List<int>();
        skillname = new List<string>();
        if (chara.skillTrees.Count > 0)
        {
            foreach (SkillTree skilltree in chara.skillTrees)
            {
                skilluses.Add(skilltree.skill.maxuse);
                skillname.Add(skilltree.skill.skillname);
            }
        }
        this.partyid = partyid; 
    }

}

