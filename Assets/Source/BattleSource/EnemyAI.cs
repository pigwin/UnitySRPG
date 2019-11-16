using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.IO;

[System.Serializable]
public class EnemyRoutin
{
    [Header("行動の志向性")]
    [Header("自分のHP減少時逃げるかどうかのフラグ")]
    public bool Cherish_life;
    [Header("スキル使用優先フラグ")]
    public bool Priority_Of_Using_Skill;

    //ターゲット選択の志向性
    [Header("ターゲット選択の志向性")]
    [Range(0f,1f)]
    [Header("ターゲットHPの優先度")]
    public float eval_hp;
    [Range(0.5f,1f)]
    [Header("ターゲットとの距離の優先度")]
    public float eval_near = 0.5f;  //0.5fは最低保障
    [Range(0f, 1f)]
    [Header("ターゲットに対する物理ダメージの優先度")]
    public float eval_def;
    [Range(0f, 1f)]
    [Header("ターゲットに対する魔法ダメージの優先度")]
    public float eval_mdf;

    [Header("ターゲット捕捉範囲：0（Default）でSTEPの2倍")]
    public int sightrange = 0;

    //評価関数
    //HPが低いほどポイント高い
    public double Eval_HP(Unitdata unit)
    {
        return eval_hp / (unit.hp / unit.status.maxhp);
    }
    //近いほどポイント高い
    public double Eval_NEAR(Unitdata unit1, Unitdata unit2)
    {
        return eval_near / (Mathf.Abs(unit1.x - unit2.x) + Mathf.Abs(unit1.y - unit2.y));
    }
    //DEFが低いほどポイント高い
    public double Eval_DEF(Unitdata unit1, Unitdata unit2)
    {
        if (unit1.status.atk - unit2.status.def > 0)
            return eval_def * (unit1.status.atk - unit2.status.def);
        else return 0; //ダメージなし
    }
    //MDFが低いほどポイント高い
    public double Eval_MDF(Unitdata unit1, Unitdata unit2)
    {
        if (unit1.status.mat - unit2.status.mdf > 0)
            return eval_mdf * (unit1.status.mat - unit2.status.mdf);
        else return 0; //ダメージなし
    }

    //全体関数：戻り値が高いtargetを目標にする。
    public double EvaluationTarget(Unitdata own, Unitdata target)
    {
        //return Eval_HP(target) + Eval_NEAR(own, target);
        return Eval_HP(target) + Eval_NEAR(own, target) + Eval_DEF(own, target) + Eval_MDF(own, target);
    }
}

/****************************************/
/*敵の思考ルーチンの設定と実現する関数  */
/****************************************/
public class EnemyAI : MonoBehaviour {

    //AIデータ
    public static Dictionary<string, double> ai_discrimination = new Dictionary<string, double>();

    //敵がターゲットを見つける関数
    public static int[] SearchTraget(Unitdata enemy)
    {
        //戻り値
        int[] target = new int[2];
        target[0] = -1;
        target[1] = -1;
        //考える範囲
        int range = enemy.routin.sightrange;
        if (range == 0) range = enemy.status.step * 2; //defaultならばstepの2倍

        //rangeでDfsを呼び出す
        List<int[]> searchlist = Mapclass.Dfs(BattleVal.mapdata, enemy.x, enemy.y, range, enemy.status.jump);
        //Dfsは自身を含まないため、加える
        searchlist.Add(new int[] { enemy.x, enemy.y });

        //発見したtargetのリスト
        HashSet<int[]> targetlist = new HashSet<int[]>();

        //searchlistをまわす
        foreach (int[] coord in searchlist)
        {
            //攻撃可能マスを取得
            //List<int[]> attacklist = Mapclass.AttackDfs(BattleVal.mapdata, coord[0], coord[1],
            //    CharaAttack.attackrange[enemy.jobid], CharaAttack.AttackMaxRange(enemy.jobid));
            List<int[]> attacklist = enemy.attackrange.Attackablelist(BattleVal.mapdata, coord[0], coord[1]);
            //攻撃範囲を探索
            foreach (int[] attackcoord in attacklist)
            {
                //そこに誰かいれば
                if (BattleVal.id2index.ContainsKey(
                    string.Format("{0},{1}", attackcoord[0], attackcoord[1])))
                {
                    //それが敵ではないならターゲットに追加
                    if(BattleVal.id2index[string.Format("{0},{1}", attackcoord[0], attackcoord[1])].team
                        != enemy.team)
                        targetlist.Add(attackcoord);
                }
            }
        }

        //ターゲットがいない場合
        if(targetlist.Count == 0)
        {
            return new int[] { enemy.x, enemy.y }; //自身の座標を返却
        }

        //AIタイプに基づいてtargetlistからターゲットを決める
        

        //ターゲットループ
        double maxeval = 0.0;
        foreach(int[] targetcoord in targetlist)
        {
            Unitdata unit = BattleVal.id2index[string.Format("{0},{1}", targetcoord[0], targetcoord[1])];

            double evaluation = enemy.routin.EvaluationTarget(enemy, unit);

            //評価値が最大なら、ターゲットに。
            if((target[0] == -1 && target[1] == -1)
                || evaluation > maxeval)
            {
                maxeval = evaluation;
                target = targetcoord;
            }
        }

        return target;
    }


    //攻撃対象に向けて移動する先を決める
    public static int[] DecideDestination(Unitdata enemy,int[] target)
    {
        int[] destination = new int[2];

        //命を大事にする傾向で、HPが1/8を切っていたら逃げる
        if (enemy.hp <= enemy.status.maxhp/8 && enemy.routin.Cherish_life)
        {
            //自身とターゲットにの間の変位ベクトルの反転を求める
            int[] v = new int[] { enemy.x - target[0], enemy.y - target[1] };
            //vのx,yの整数比を求め、疑規格化
            for (int i = 2; i <= (int)Mathf.Min(v[0], v[1]);)
            {
                if(v[0] % i == 0 && v[1] % i == 0)
                {
                    v[0] /= i;
                    v[1] /= i;
                }
                else
                {
                    i++;
                }
            }

            target = new int[] { enemy.x + v[0]*enemy.status.step, enemy.y + v[1]*enemy.status.step };
            if (target[0] >= Mapclass.mapxnum) target[0] = Mapclass.mapxnum - 1;
            if (target[0] < 0) target[0] = 0;
            if (target[1] >= Mapclass.mapynum) target[1] = Mapclass.mapynum - 1;
            if (target[1] < 0) target[1] = 0;
        }

        //歩数が十分大きいとして、行動可能範囲を探索
        List<int[]> movablelist = Mapclass.Dfs(BattleVal.mapdata, enemy.x, enemy.y,
                enemy.status.step, enemy.status.jump);
        //Dfsは自身の位置を含めないため、追加
        movablelist.Add(new int[] { enemy.x, enemy.y });

        int mindist = -1;
        //移動可能範囲の中で、ターゲットに攻撃できる位置に向かう
        //もしくはターゲットに最も近いマンハッタン距離の位置に向かう
        foreach (int[] a in movablelist)
        {
            //攻撃可能マスを取得
            //List<int[]> attacklist = Mapclass.AttackDfs(BattleVal.mapdata, coord[0], coord[1],
            //    CharaAttack.attackrange[enemy.jobid], CharaAttack.AttackMaxRange(enemy.jobid));
            List<int[]> attacklist = enemy.attackrange.Attackablelist(BattleVal.mapdata, a[0], a[1]);
            bool brakeflag = false;
            //攻撃範囲を探索
            foreach (int[] attackcoord in attacklist)
            {
                //そこにターゲットがいる
                if (attackcoord[0] == target[0] && attackcoord[1] == target[1])
                {
                    destination = a;
                    brakeflag = true;
                    break;
                }
            }

            if (brakeflag) break;

            int tempdist = Mapclass.Calc_Dist(target, a);
            if(mindist == -1 || tempdist < mindist)
            {
                mindist = tempdist;
                destination = a;
            }
        }
        
            
        return destination;

    }

    //攻撃可能キャラリストに、特定キャラがいるかどうかを判定
    public static bool Is_Chara_in_List(List<Unitdata> charalist, Unitdata target)
    {
        foreach(Unitdata chara in charalist)
        {
            if(chara.x == target.x && chara.y == target.y)
            {
                return true;
            }
        }
        return false;
    }

    //攻撃可能キャラリストの中で、攻撃対象を選び出す
    public static void Set_Attack(List<Unitdata> charalist, Unitdata target)
    {
        //そもそもターゲットが存在する場合、ここで終了
        if (Is_Chara_in_List(charalist, target))
        {
            CharaAttack.Set_attackedpos(target.x, target.y);
            return;
        }

        int randomid = Random.Range(0, charalist.Count-1);
        CharaAttack.Set_attackedpos(charalist[randomid].x, charalist[randomid].y);
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
