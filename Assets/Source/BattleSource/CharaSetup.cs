using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*******************************************************************/
/**/
/*******************************************************************/
public class CharaSetup : MonoBehaviour {

    public GameObject UnitSetTile;
    public GameObject UnitChangeTilePrefab;
    public GameObject UnitSetWindow;
    private GameObject UnitChangeTile;
    public static List<int[]> unitsetposlist = new List<int[]>();

    [System.NonSerialized] public List<GameObject> unitsetlist = new List<GameObject>();
    public GameObject prefabUnitPanel;
    public GameObject gobjUnitPanelParent;
    
    public static Dictionary<int, bool> partyForbidden = new Dictionary<int, bool>();
    public static Dictionary<int, bool> partyForce = new Dictionary<int, bool>();
    //[Header("最大出撃人数")] Mapclass.csでpublicを設定、あちらのstartでこちらに反映される。
    public static int maxpartyunitnum = 1;
    //これ以上出撃可能かのフラグ
    public static bool sortieflag = false;
    
    public Text textUnitSortie;
    private int partyunitnum = 0;

    public AudioClip seOk;
    public AudioClip seCancel;

    public Text textUnitChange;
    public Button button_start;

    public Button button_showhide;
    public Text text_showhide;

    public enum CharaSetupStatus
    {
        INIT,
        SET,
        MOVE,
        MOVEDO,
        MOVEEND,
        FIN
    }
    public static CharaSetupStatus state = CharaSetupStatus.INIT;

    public void Push_Select_Finish_Button()
    {
        UnitSetWindow.SetActive(false);
        button_showhide.gameObject.SetActive(false);
        foreach(GameObject tmp in unitsetlist)
        {
            Destroy(tmp);
        }
        state = CharaSetupStatus.FIN;
    }
    // Use this for initialization
    void Start () {
        UnitSetWindow.SetActive(false);
        textUnitChange.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update() {
        if (BattleVal.status == STATUS.SETUP)
        {
            switch(state)
            {
                case CharaSetupStatus.INIT:
                    DrawTile();
                    DrawUnitList();
                    state = CharaSetupStatus.SET;
                    break;
                case CharaSetupStatus.SET:
                    UpdatePartyUnitNum();
                    textUnitSortie.text = string.Format("出撃ユニット:{0}/{1}", partyunitnum, maxpartyunitnum);
                    break;
                case CharaSetupStatus.MOVE:
                    textUnitChange.gameObject.SetActive(true);
                    button_showhide.gameObject.SetActive(false);
                    UnitSetWindow.SetActive(false);
                    SetChangeTile();
                    state = CharaSetupStatus.MOVEDO;
                    break;
                case CharaSetupStatus.MOVEDO:

                    break;
                case CharaSetupStatus.MOVEEND:
                    textUnitChange.gameObject.SetActive(false);
                    button_showhide.gameObject.SetActive(true);
                    HideShowUnitList(false);
                    DeleteChangeTile();
                    state = CharaSetupStatus.SET;
                    break;

                case CharaSetupStatus.FIN:

                    BattleVal.status = STATUS.TURNCHANGE;
                    state = CharaSetupStatus.INIT;
                    break;
            }
        }
	}

    //ユニット設置可能なマスにタイルを描く
    void DrawTile()
    {
        unitsetposlist.Clear();
        for (int i = 0; i < Mapclass.mapxnum; i++)
        {
            for (int j = 0; j < Mapclass.mapynum; j++)
            {
                if (BattleVal.mapdata[(int)MapdataList.MAPUNITSET][j][i] == 1)
                {
                    unitsetlist.Add(Instantiate(UnitSetTile));
                    Mapclass.DrawCharacter(unitsetlist[unitsetlist.Count-1], i, j);
                    //1つ目の座標に視点移動
                    if (unitsetposlist.Count == 0)
                    {
                        Vector3 temp = new Vector3();
                        Mapclass.TranslateMapCoordToPosition(ref temp, i, j);
                        CameraAngle.CameraPoint(temp);
                    }
                    unitsetposlist.Add(new int[] { i, j });
                }
            }
        }
    }

    void DrawUnitList()
    {
        //加入しているパーティメンバーのパネルを作成する。
        foreach(UnitSaveData unitsave in GameVal.masterSave.playerUnitList)
        {
            GameObject tempunitpanel = Instantiate(prefabUnitPanel, gobjUnitPanelParent.transform);
            //もしIDの紐づけがされているキャラの場合、強制出撃かどうか・出撃不可能かを調べる
            if (GameVal.masterSave.id2unitdata.ContainsValue(unitsave))
            {
                //紐づけされている固有のパーティメンバー
                foreach(KeyValuePair<int, UnitSaveData> kvp in GameVal.masterSave.id2unitdata)
                {
                    if(kvp.Value == unitsave)
                    {
                        //マップ上に既にいる場合
                        if (partyForce.ContainsKey(kvp.Key))
                        {
                            tempunitpanel.GetComponent<UnitPanel>().SetUnit(unitsave, partyForce[kvp.Key], false); //マップに既にいるため、出撃不可ではない
                            if(!partyForce[kvp.Key]) tempunitpanel.GetComponent<UnitPanel>().sortiestate = UnitPanel.SortieState.SORTIE; //マップ上に居るが強制出撃ではないため
                        }
                        else
                        {
                            if(partyForbidden.ContainsKey(kvp.Key))
                                tempunitpanel.GetComponent<UnitPanel>().SetUnit(unitsave, false, partyForbidden[kvp.Key]); //マップ上にいないため、強制出撃ではない
                            else
                                tempunitpanel.GetComponent<UnitPanel>().SetUnit(unitsave, false, false); //設定されていなければ出撃不可ではない
                        }
                        break;
                    }
                }
            }
            else
            {
                //紐づけされていないパーティメンバー（自由登用キャラ）について
                tempunitpanel.GetComponent<UnitPanel>().SetUnit(unitsave, false, false); //強制出撃でも出撃不可能でもない
            }
        }
        UnitSetWindow.SetActive(true);
        button_showhide.gameObject.SetActive(true);

    }

    //パーティメンバー数のアップデート
    void UpdatePartyUnitNum()
    {
        partyunitnum = 0;
        foreach (Unitdata unit in BattleVal.unitlist)
        {
            if (unit.team == 0)
                partyunitnum++;
        }
        if (partyunitnum >= maxpartyunitnum)
        {
            textUnitSortie.color = new Color(1, 0, 0);
            sortieflag = false;
        }
        else
        {
            textUnitSortie.color = new Color(1, 1, 1);
            sortieflag = true;
        }
        if(partyunitnum == 0)
            button_start.interactable = false;
        else
            button_start.interactable = true;
    }

    //位置変更するパーティユニットのタイル色を変更
    void SetChangeTile()
    {
        UnitChangeTile = Instantiate(UnitChangeTilePrefab, new Vector3(), Quaternion.identity);
        Mapclass.DrawCharacter(UnitChangeTile, BattleVal.selectX, BattleVal.selectY);
    }

    //タイル色変更を戻す
    void DeleteChangeTile()
    {
        Destroy(UnitChangeTile);
    }

    //ユニットウィンドウを隠す
    public void HideShowUnitList(bool seflag = true)
    {
        if(UnitSetWindow.activeSelf)
        {
            if (seflag) Operation.setSE(seCancel);
            UnitSetWindow.SetActive(false);
            text_showhide.text = "リストを表示";
        }
        else
        {
            if (seflag) Operation.setSE(seOk);
            UnitSetWindow.SetActive(true);
            text_showhide.text = "リストを隠す";
        }
        
    }
}

[System.Serializable]
public class IdAndBool
{
    public int id;
    public bool flag;
}