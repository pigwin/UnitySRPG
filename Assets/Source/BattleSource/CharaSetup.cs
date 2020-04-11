using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
    private List<GameObject> unitpanellist = new List<GameObject>();

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


    bool cancelButtonClick = false;

    public enum CharaSetupStatus
    {
        INIT,
        SET,
        MOVE,
        MOVEDO,
        MOVEEND,
        CHECKENEMY,
        FIN
    }
    public static CharaSetupStatus state = CharaSetupStatus.INIT;

    public void Push_Select_Finish_Button()
    {
        UnitSetWindow.SetActive(false);
        button_showhide.gameObject.SetActive(false);
        foreach (GameObject tmp in unitsetlist)
        {
            Destroy(tmp);
        }
        state = CharaSetupStatus.FIN;
    }
    // Use this for initialization
    void Start() {
        UnitSetWindow.SetActive(false);
        textUnitChange.gameObject.SetActive(false);
        mode_Field = false;
    }

    // Update is called once per frame
    void Update() {
        if (BattleVal.status == STATUS.SETUP)
        {
            switch (state)
            {
                case CharaSetupStatus.INIT:
                    DrawTile();
                    DrawUnitList();
                    state = CharaSetupStatus.SET;
                    break;
                case CharaSetupStatus.SET:
                    UpdatePartyUnitNum();
                    textUnitSortie.text = string.Format("出撃ユニット:{0}/{1}", partyunitnum, maxpartyunitnum);
                    //キーボードモード
                    if (!BattleVal.is_mouseinput)
                    {
                        //表示非表示切り替え
                        if (Input.GetButtonDown("Cancel") && !cancelButtonClick)
                        {
                            HideShowUnitList();
                            cancelButtonClick = true;
                        }
                        else
                            cancelButtonClick = false;
                        if (!mode_Field) GetKeypadInput_SortieWindow();
                        if (Input.GetButtonDown("Enter") && button_start.interactable) Push_Select_Finish_Button();
                    }
                    HelpUI();
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
                    if (BattleVal.is_mouseinput) HideShowUnitList(false);
                    DeleteChangeTile();
                    state = CharaSetupStatus.SET;
                    break;
                case CharaSetupStatus.CHECKENEMY:
                    //Debug.Log(state);
                    if (Input.GetButtonDown("Cancel") && !cancelButtonClick)
                    {
                        Operation.setSE(seCancel);
                        state = CharaSetupStatus.SET;
                        cancelButtonClick = true;
                    }
                    else
                    {
                        cancelButtonClick = false;
                    }
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
                    Mapclass.DrawCharacter(unitsetlist[unitsetlist.Count - 1], i, j);
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
        foreach (UnitSaveData unitsave in GameVal.masterSave.playerUnitList)
        {
            GameObject tempunitpanel = Instantiate(prefabUnitPanel, gobjUnitPanelParent.transform);
            //もしIDの紐づけがされているキャラの場合、強制出撃かどうか・出撃不可能かを調べる
            if (GameVal.masterSave.id2unitdata.ContainsValue(unitsave))
            {
                //紐づけされている固有のパーティメンバー
                foreach (KeyValuePair<int, UnitSaveData> kvp in GameVal.masterSave.id2unitdata)
                {
                    if (kvp.Value == unitsave)
                    {
                        //マップ上に既にいる場合
                        if (partyForce.ContainsKey(kvp.Key))
                        {
                            tempunitpanel.GetComponent<UnitPanel>().SetUnit(unitsave, partyForce[kvp.Key], false); //マップに既にいるため、出撃不可ではない
                            if (!partyForce[kvp.Key]) tempunitpanel.GetComponent<UnitPanel>().sortiestate = UnitPanel.SortieState.SORTIE; //マップ上に居るが強制出撃ではないため
                        }
                        else
                        {
                            if (partyForbidden.ContainsKey(kvp.Key))
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

            unitpanellist.Add(tempunitpanel);
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
        if (partyunitnum == 0)
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

    //キーパッドモード時にマップ上を十字キーで操作できるか
    public static bool mode_Field = false;

    //ユニットウィンドウを隠す
    public void HideShowUnitList(bool seflag = true)
    {
        if (UnitSetWindow.activeSelf)
        {
            if (seflag) Operation.setSE(seCancel);
            UnitSetWindow.SetActive(false);
            text_showhide.text = "リストを表示";
            mode_Field = true;
            //ボタンをセレクトを外す
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            if (seflag) Operation.setSE(seOk);
            UnitSetWindow.SetActive(true);
            text_showhide.text = "リストを隠す";
            mode_Field = false;
            /*
            //キーパッドモード時ならば戦闘開始ボタンをセレクト
            if(!BattleVal.is_mouseinput)
                button_start.Select();
            */
        }

    }

    //キー入力待ち時間
    [SerializeField] const float interval_keypad_time = 0.13f;
    float delta_keypad_time = interval_keypad_time;
    public ScrollRect scrollviewUnitSetup;

    //キーパッドモード時の出撃メニュー操作
    public void GetKeypadInput_SortieWindow()
    {
        //選ばれてないとき
        try
        {
            GameObject obj = EventSystem.current.currentSelectedGameObject;
            if (obj == null)
            {
                EventSystem.current.SetSelectedGameObject(unitpanellist[0]);
                scrollviewUnitSetup.verticalNormalizedPosition = 1.0f;
            }
        }
        catch (NullReferenceException ex)
        {
            EventSystem.current.SetSelectedGameObject(unitpanellist[0]);
            scrollviewUnitSetup.verticalNormalizedPosition = 1.0f;
        }

        //一定時間待つ
        if (delta_keypad_time < interval_keypad_time)
        {
            delta_keypad_time += Time.deltaTime;
            return;
        }

        delta_keypad_time = 0;

        //左入力（スキル選択判定）
        if (Input.GetAxisRaw("Horizontal") < 0 && !CharaStatusPrinter.is_selectSkillList)
        {
            CharaStatusPrinter.is_selectSkillList = true;
        }
        if (Input.GetAxisRaw("Horizontal") > 0 && CharaStatusPrinter.is_selectSkillList)
        {
            CharaStatusPrinter.is_selectSkillList = false;
            int nowselect = (int)((1 - scrollviewUnitSetup.verticalNormalizedPosition) * (unitpanellist.Count - 1));
            EventSystem.current.SetSelectedGameObject(unitpanellist[nowselect]);
        }
        if (CharaStatusPrinter.is_selectSkillList) return;
        //以下スキルリストがセレクトされてない場合

        //上入力
        if (Input.GetAxisRaw("Vertical") > 0)
        {
            int nowselect = unitpanellist.IndexOf(EventSystem.current.currentSelectedGameObject);
            if (nowselect > 0)
            {
                //表示領域調整
                try
                {
                    scrollviewUnitSetup.verticalNormalizedPosition = Mathf.Clamp(1.0f - (float)(nowselect - 1) / (unitpanellist.Count - 1), 0, 1);
                } catch (ArithmeticException ex)
                {
                    scrollviewUnitSetup.verticalNormalizedPosition = 1.0f;
                }
                EventSystem.current.SetSelectedGameObject(unitpanellist[nowselect - 1]);
            }
        }
        //下入力
        if (Input.GetAxisRaw("Vertical") < 0)
        {
            int nowselect = unitpanellist.IndexOf(EventSystem.current.currentSelectedGameObject);
            if (nowselect < unitpanellist.Count - 1)
            {
                //表示領域調整
                try
                {
                    scrollviewUnitSetup.verticalNormalizedPosition = Mathf.Clamp(1.0f - (float)(nowselect + 1) / (unitpanellist.Count - 1), 0, 1);
                } catch (ArithmeticException ex)
                {
                    scrollviewUnitSetup.verticalNormalizedPosition = 1.0f;
                }

                EventSystem.current.SetSelectedGameObject(unitpanellist[nowselect + 1]);
            }
        }

    }

    [SerializeField] GameObject gobjHelpUI;
    [SerializeField] GameObject gobjSortieModeUI;
    [SerializeField] GameObject gobjSkillListModeUI;

    void HelpUI()
    {
        gobjHelpUI.SetActive(!BattleVal.is_mouseinput);
        gobjSkillListModeUI.SetActive(CharaStatusPrinter.is_selectSkillList);
        gobjSortieModeUI.SetActive(!CharaStatusPrinter.is_selectSkillList);
    }
}

[System.Serializable]
public class IdAndBool
{
    public int id;
    public bool flag;
}