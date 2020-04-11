using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurnEnd : MonoBehaviour
{

    //ターン終了時の処理
    //状態異常のターン経過・治癒系処理
    public enum TURNEND_STATE
    {
        SETUP,
        DAMAGE_SHOW,
        END
    }
    TURNEND_STATE turnend_state = TURNEND_STATE.SETUP;

    //ダメージ文字（1桁あたり）
    public Text damagetextprefab;
    //実際に表示する数値
    public static List<List<Text>> damagenum = new List<List<Text>>();
    private List<GameObject> gobjPopupConditions = new List<GameObject>();

    public GameObject defeatEffect;

    [SerializeField] Canvas canvas;

    float nowtime = 0;
    //ダメージ演出時間
    [SerializeField] float damage_time = 1.0f;
    const float damage_posy = 50f;
    // Update is called once per frame
    void Update()
    {
        if (BattleVal.status != STATUS.TURNEND) return;
        
        switch(turnend_state)
        {
            case TURNEND_STATE.SETUP:
                damagenum = new List<List<Text>>();
                gobjPopupConditions = new List<GameObject>();

                foreach(Unitdata unit in BattleVal.unitlist)
                {
                    int conditioncount = 0;
                    foreach(Condition condition in unit.status.conditions)
                    {
                        int damage = condition.SlipDamage(unit.status.maxhpcurve.value(unit.status.level));
                        if (damage == 0) continue;

                        unit.hp -= damage;
                        Vector3 r1 = new Vector3(); //ダメージ発生座標
                        Mapclass.TranslateMapCoordToPosition(ref r1, unit.x, unit.y);
                        string damagetext = string.Format("{0}", (int)Mathf.Abs(damage));
                        int count = 1;
                        //ダメージテキストの中心座標
                        Vector3 damage_center = Camera.main.WorldToScreenPoint(r1);
                        //テキストの登録
                        List<Text> damagenumtmp = new List<Text>();
                        foreach (char a in damagetext)
                        {
                            int num = (int)char.GetNumericValue(a); //数を取得

                            damagenumtmp.Add(Instantiate(damagetextprefab, r1, Quaternion.identity));
                            damagenumtmp[count - 1].text = a.ToString();
                            //サイズ取得
                            damagenumtmp[count - 1].rectTransform.sizeDelta =
                                new Vector2(damagenumtmp[count - 1].preferredWidth, damagenumtmp[count - 1].preferredHeight);
                            damagenumtmp[count - 1].rectTransform.sizeDelta =
                                new Vector2(damagenumtmp[count - 1].preferredWidth, damagenumtmp[count - 1].preferredHeight);
                            damagenumtmp[count - 1].transform.SetParent(canvas.transform, false);
                            damagenumtmp[count - 1].transform.localPosition = damage_center - new Vector3(canvas.GetComponent<RectTransform>().sizeDelta.x / 2, canvas.GetComponent<RectTransform>().sizeDelta.y / 2, 0);

                            //1桁当たりの文字ブロック
                            Vector3 numsize = damagenumtmp[count - 1].GetComponent<RectTransform>().sizeDelta * damagenumtmp[count - 1].GetComponent<RectTransform>().localScale;
                            numsize.y = 0;
                            numsize.z = 0;

                            damagenumtmp[count - 1].transform.localPosition += (count - 1) * numsize;
                            damagenumtmp[count - 1].transform.localPosition += new Vector3(0, 30 + 65* conditioncount, 0);
                            damagenumtmp[count - 1].color = new Vector4(1, 0, 0, 1);
                            if (damage < 0) damagenumtmp[count - 1].color = new Vector4(0.7f, 1, 0.7f, 1);
                            damagenumtmp[count - 1].gameObject.SetActive(false);
                            count++;

                        }
                        damagenum.Add(damagenumtmp);

                        //state popup
                        GameObject tempPopup = Instantiate(condition.prefabPopup, r1, Quaternion.identity);
                        tempPopup.AddComponent<CanvasGroup>();
                        tempPopup.GetComponent<CanvasGroup>().alpha = 1;
                        tempPopup.transform.SetParent(canvas.transform, false);
                        tempPopup.transform.localPosition = damage_center
                            - new Vector3(canvas.GetComponent<RectTransform>().sizeDelta.x / 2, canvas.GetComponent<RectTransform>().sizeDelta.y / 2, 0)
                            + new Vector3(0, 65 * (conditioncount+1), 0);
                        tempPopup.SetActive(false);
                        gobjPopupConditions.Add(tempPopup);
                        unit.gobj.GetComponent<CharaAnimation>().seSource.PlayOneShot(condition.se);

                        conditioncount++;//ユニットのダメージ・回復状態異常数

                    }
                }

                //テキスト表示
                foreach (List<Text> damagenumtmp in damagenum)
                {
                    foreach (Text a in damagenumtmp)
                    {
                        a.gameObject.SetActive(true);
                    }
                    //CameraAngle.CameraPoint(BattleVal.selectedUnit.gobj.transform.position);
                }
                foreach (GameObject gobjCondition in gobjPopupConditions)
                {
                    gobjCondition.SetActive(true);
                }
                nowtime = 0;

                if (damagenum.Count == 0) turnend_state = TURNEND_STATE.END;
                else turnend_state = TURNEND_STATE.DAMAGE_SHOW;

                break;

            case TURNEND_STATE.DAMAGE_SHOW:
                if (nowtime >= damage_time - Time.deltaTime)
                {
                    //戦闘後処理
                    turnend_state = TURNEND_STATE.END;

                    //ダメージ数値のテキストを消す処理
                    foreach (List<Text> damagenumtmp in damagenum)
                    {
                        foreach (Text a in damagenumtmp)
                        {
                            Destroy(a.gameObject);
                        }
                    }
                    foreach (GameObject gobjCondition in gobjPopupConditions)
                    {
                        Destroy(gobjCondition);
                    }
                    
                    //初期化
                    gobjPopupConditions.Clear();
                    damagenum.Clear();

                }
                else
                {
                    float accelfact = 1.0f;
                    //加速処理
                    if (Input.GetButton("Submit")) accelfact = 2.0f;
                    if (Input.GetButton("Cancel")) nowtime = damage_time;

                    //ダメージ数値のテキストを消す処理
                    //damagetext.color -= new Color(0,0,0,Time.deltaTime);
                    foreach (List<Text> damagenumtmp in damagenum)
                    {
                        foreach (Text a in damagenumtmp)
                        {
                            a.color -= new Color(0, 0, 0, Time.deltaTime * accelfact);
                            a.transform.position += new Vector3(0, damage_posy * Time.deltaTime * accelfact, 0);
                        }
                    }
                    foreach (GameObject gobjCondition in gobjPopupConditions)
                    {
                        gobjCondition.GetComponent<CanvasGroup>().alpha -= Time.deltaTime * accelfact;
                        gobjCondition.transform.position += new Vector3(0, damage_posy * Time.deltaTime * accelfact, 0);
                    }
                    nowtime += Time.deltaTime;

                }
                break;

            case TURNEND_STATE.END:
                for (int j = BattleVal.unitlist.Count - 1; j >= 0; j--)
                {
                    
                    for(int i = BattleVal.unitlist[j].status.conditions.Count - 1; i >= 0; i--)
                    {
                        if(BattleVal.unitlist[j].status.conditions[i].Consume())
                        {
                            BattleVal.unitlist[j].status.conditions.Remove(
                                BattleVal.unitlist[j].status.conditions[i]);
                        }
                    }

                    if (BattleVal.unitlist[j].hp > BattleVal.unitlist[j].status.maxhp)
                    {
                        BattleVal.unitlist[j].hp = BattleVal.unitlist[j].status.maxhp;
                    }

                    //撃墜処理
                    if (BattleVal.unitlist[j].hp <= 0)
                    {
                        Vector3 defeatpos = new Vector3();
                        Mapclass.TranslateMapCoordToPosition(ref defeatpos, BattleVal.unitlist[j].x, BattleVal.unitlist[j].y);
                        StartCoroutine(UnitDefeat.DefeatHandle(BattleVal.unitlist[j], defeatEffect, defeatpos));

                        //ユニットデータを消去
                        ////BattleVal.id2indexから消去
                        BattleVal.id2index.Remove(string.Format("{0},{1}", BattleVal.unitlist[j].x, BattleVal.unitlist[j].y));
                        ////mapdataからユニット情報の削除
                        BattleVal.mapdata[(int)MapdataList.MAPUNIT][BattleVal.unitlist[j].y][BattleVal.unitlist[j].x] = 0;

                        ////BattleVal.unitlistから消去
                        BattleVal.unitlist.RemoveAt(j);
                    }

                }
                BattleVal.status = STATUS.TURNCHANGE;
                turnend_state = TURNEND_STATE.SETUP;
                break;
        }
    }
}
