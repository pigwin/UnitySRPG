using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
/*******************************************************************/
/*ユニットが撃墜されたときのアニメーションなどの処理クラス         */
/*******************************************************************/
public class UnitDefeat : MonoBehaviour
{
    public static bool is_defeatanim = false;
    //引数のUnitDataを撃墜させるハンドラ
    public static IEnumerator DefeatHandle(Unitdata unit, GameObject defeatEffect, Vector3 effectpos)
    {
        is_defeatanim = true;
        //撃墜モーション
        unit.gobj.GetComponent<Animator>().Play("Defeat");
        //アニメーション開始は1フレーム後
        yield return null;
        //アニメーションの終了待ち
        yield return new WaitForAnimation(unit.gobj.GetComponent<Animator>(), 0);
        //ボイス,効果音の再生終了待ち
        while(unit.gobj.GetComponent<CharaAnimation>().voiceSource.isPlaying
            || unit.gobj.GetComponent<CharaAnimation>().seSource.isPlaying)
            yield return null;
        //撃墜エフェクト再生
        //再生時間を確認
        List<ParticleSystem> particleList = new List<ParticleSystem>();
        particleList.Add(defeatEffect.GetComponent<ParticleSystem>());
        particleList.AddRange(defeatEffect.GetComponentsInChildren<ParticleSystem>());
        float time = 0.0f;
        foreach(ParticleSystem particle in particleList)
        {
            if (time < particle.main.duration) time = particle.main.duration;
        }

        Destroy(Instantiate(defeatEffect, effectpos, defeatEffect.transform.rotation), time);
        yield return new WaitForSeconds(1);
        //yield return null;

        ////表示キャラを消す（Prefab消去）
        Destroy(unit.gobj);
        is_defeatanim = false;
        yield break;
    }
	// Use this for initialization
	void Start () {
        is_defeatanim = false;

    }
	
	// Update is called once per frame
	void Update () {
		
	}

    //自軍・敵軍の全滅をチェックする関数
    public static bool Is_Totdestruct(int team)
    {
        foreach (Unitdata unit in BattleVal.unitlist)
        {
            if (unit.team == team) return false;
        }
        return true;
    }

}
