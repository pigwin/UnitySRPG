using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*******************************************************************/
/*ユニット出撃用のパネルの処理                                     */
/*******************************************************************/
public class UnitPanel : MonoBehaviour
{
    public Text textName;
    public Text textLevel;
    public GameObject expGauge;
    public GameObject expGaugeParent;
    public Text textExp;

    public Button buttonUnitPanel;
    public Button buttonIsSortie;
    public Image imageIsSortie;

    [SerializeField] private Sprite sprite_ForceSortie;
    [SerializeField] private Sprite sprite_Sortie;
    [SerializeField] private Sprite sprite_NoneSortie;
    [SerializeField] private Sprite sprite_ForbiddenSortie;

    public GameObject prefabPlayerTile;

    public AudioClip seOk;
    public AudioClip seCancel;

    private Unitdata unit;

    private bool unitcheckflag = false; //パーティ確認用フラグ

    //出撃可能かどうか
    public enum SortieState
    {
        NONE,
        SORTIE,
        FORCE,
        FORBIDDEN
    }
    public SortieState sortiestate = SortieState.NONE;


    public void SetUnit(UnitSaveData unitSave, bool isForceSortie, bool isForbiddenSortie, bool is_unitcheck = false)
    {
        unit = new Unitdata(unitSave, false, is_unitcheck);
        //HPとスキル使用回数の回復
        unit.hp = unit.status.maxhp;
        foreach (Skill skill in unit.skills)
            skill.use = skill.maxuse;
        textName.text = unit.charaname;
        textLevel.text = string.Format("Lv:{0}",unit.status.level);
        //LvMAX判定
        if (unit.status.level >= 99)
        {
            unit.status.level = 99; //メモリ改ざんとか対策
            textExp.text = string.Format("ＭＡＸ");
            textExp.color = Color.red;
            expGauge.GetComponent<RectTransform>().sizeDelta
                = new Vector2(expGaugeParent.GetComponent<RectTransform>().sizeDelta.x,
                                expGaugeParent.GetComponent<RectTransform>().sizeDelta.y);
        }
        else
        {
            textExp.text = string.Format("あと{0}", unit.status.needexp - unit.status.exp);
            textExp.color = Color.white;
            expGauge.GetComponent<RectTransform>().sizeDelta
                = new Vector2((float)unit.status.exp / (float)unit.status.needexp * expGaugeParent.GetComponent<RectTransform>().sizeDelta.x,
                                expGaugeParent.GetComponent<RectTransform>().sizeDelta.y);
        }
        
        if (isForceSortie)
        {
            sortiestate = SortieState.FORCE;
            //強制出撃の場合、すでにマップ上に存在する該当キャラクターとunitの指し示す先が一致する必要がある。
            foreach(Unitdata tempunit in BattleVal.unitlist)
            {
                if(tempunit.savescobj == unit.savescobj)
                {
                    unit = tempunit;
                }
            }
        }
        else if (isForbiddenSortie) sortiestate = SortieState.FORBIDDEN;
        else sortiestate = SortieState.NONE;
        if (is_unitcheck) buttonIsSortie.gameObject.SetActive(false);
        unitcheckflag = is_unitcheck;
    }

    private void Update()
    {
        switch(sortiestate)
        {
            case SortieState.NONE:
                imageIsSortie.sprite = sprite_NoneSortie;
                buttonIsSortie.interactable = true;
                break;
            case SortieState.SORTIE:
                imageIsSortie.sprite = sprite_Sortie;
                buttonIsSortie.interactable = true;
                break;
            case SortieState.FORCE:
                imageIsSortie.sprite = sprite_ForceSortie;
                buttonIsSortie.interactable = false;
                break;
            case SortieState.FORBIDDEN:
                imageIsSortie.sprite = sprite_ForbiddenSortie;
                buttonIsSortie.interactable = false;
                break;
        }
    }

    //UnitPanelクリック時に詳細表示する
    public void OnClickPanel()
    {
        //ユニット確認の場合（システムメニューなど）
        if (unitcheckflag)
        {
            UnitCheck.unitNowSelected = unit;
            UnitCheck.is_unitchange = true;
        }
        else //戦闘パートの場合
        {
            Operation.setSE(seOk);
            BattleVal.selectedUnit = unit;
            CharaStatusPrinter.Show_Enemy_Range(1);
            if (sortiestate == SortieState.SORTIE || sortiestate == SortieState.FORCE)
            {
                //カメラの移動
                CameraAngle.CameraPoint(BattleVal.selectedUnit.gobj.transform.position);
            }
        }
        
    }

    //出撃フラグボタンがクリックされた時
    public void OnClickSortieButton()
    {
        switch (sortiestate)
        {
            case SortieState.NONE:
                //出撃可能な場合
                if(CharaSetup.sortieflag)
                {
                    Operation.setSE(seOk);
                    sortiestate = SortieState.SORTIE;
                    DrawUnit();
                }
                else
                {
                    Operation.setSE(seCancel);
                }
                break;
            case SortieState.SORTIE:
                sortiestate = SortieState.NONE;
                HideUnit();
                break;
        }
    }

    //出撃可能エリアにユニットを描画
    public void DrawUnit()
    {
        CharaData loaddata = ScriptableObject.Instantiate(Resources.Load(string.Format("BattleChara/{0}", unit.savescobj)) as CharaData);
        unit.gobj = MonoBehaviour.Instantiate(loaddata.gobj, new Vector3(), Quaternion.identity);
        foreach(int[] pos in CharaSetup.unitsetposlist)
        {
            //キャラが居なければ生成
            if (!BattleVal.id2index.ContainsKey(string.Format("{0},{1}", pos[0], pos[1])))
            {
                //描画
                Mapclass.DrawCharacter(unit.gobj, pos[0], pos[1]);
                unit.gobj.layer = 8;

                unit.x = pos[0];
                unit.y = pos[1];
                //unitlistへの登録
                BattleVal.unitlist.Add(unit);
                //id2indexへの登録
                BattleVal.id2index.Add(string.Format("{0},{1}", pos[0], pos[1]), unit);
                ////mapdataにユニット情報を登録
                BattleVal.mapdata[(int)MapdataList.MAPUNIT][unit.y][unit.x] = 99; //ユニット番号はcsvから読み取るときしか使わないため、1から99なら何でもよい

                //足元にチーム識別タイルを
                GameObject unittile;
                unittile = Instantiate(prefabPlayerTile, unit.gobj.transform);
                unittile.transform.localScale = new Vector3(unittile.transform.localScale.x / unittile.transform.lossyScale.x,
                                                            0.01f,
                                                            unittile.transform.localScale.z / unittile.transform.lossyScale.z);
                Mapclass.DrawCharacter(unittile, unit.x, unit.y);

                break;
            }
        }
    }

    //すでにマップに出ているユニットを隠す
    public void HideUnit()
    {
        //ゲームオブジェクト消去
        Destroy(unit.gobj);
        //ユニットデータを消去
        ////BattleVal.unitlistから消去
        for (int i = 0; i < BattleVal.unitlist.Count; i++)
        {
            if (BattleVal.unitlist[i].x == unit.x && BattleVal.unitlist[i].y == unit.y)
            {
                BattleVal.unitlist.RemoveAt(i);
                break;
            }
        }
        ////BattleVal.id2indexから消去
        BattleVal.id2index.Remove(string.Format("{0},{1}", unit.x, unit.y));
        ////mapdataからユニット情報の削除
        BattleVal.mapdata[(int)MapdataList.MAPUNIT][unit.y][unit.x] = 0;

    }

}
