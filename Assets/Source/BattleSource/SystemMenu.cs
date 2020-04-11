using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

//右クリック時のメニューを司るクラス
public class SystemMenu : MonoBehaviour {

    public GameObject systemMenuPanel;
    public Text turnCountText;
    public Button turnChangeButton;
    public Text titleText;
    public Text victoryOrderText;
    public Text defeatOrderText;
    public Text movableNumText;
    public Text attackableNumText;
    public GameObject cautionMoveGobj;
    public GameObject cautionAttackGobj;

    // Use this for initialization
    void Start () {
        systemMenuPanel.SetActive(false);
	}

    // Update is called once per frame
    void Update() {

        if (BattleVal.sysmenuflag)
        {
            systemMenuPanel.SetActive(true);
        }
        else
        {
            systemMenuPanel.SetActive(false);
        }

        switch (BattleVal.status)
        {
            case STATUS.PLAYER_UNIT_SELECT:
                //メニュー表示中の場合
                if (BattleVal.sysmenuflag)
                {
                    if (!Operation.mouse_click)
                    {
                        try
                        {
                            bool check = false;
                            GameObject obj = EventSystem.current.currentSelectedGameObject.gameObject;
                            //システムメニュー以外のボタンを選択しているかどうか
                            foreach (Button b in systemMenuPanel.GetComponentsInChildren<Button>())
                            {
                                if (b.name == obj.name) check = true;
                            }
                            if (!check)
                            {
                                EventSystem.current.SetSelectedGameObject(systemMenuPanel.GetComponentsInChildren<Button>()[0].gameObject);
                            }
                        }
                        catch (NullReferenceException ex)
                        {
                            EventSystem.current.SetSelectedGameObject(systemMenuPanel.GetComponentsInChildren<Button>()[0].gameObject);
                        }
                    }

                    turnCountText.text = string.Format("{0}ターン目", BattleVal.turn);
                    titleText.text = BattleVal.title_name;
                    victoryOrderText.text = BattleVal.str_victoryorder;
                    defeatOrderText.text = BattleVal.str_defeatorder;
                    int num_move = 0, num_attack = 0;
                    GetAbleNum(ref num_move, ref num_attack);
                    movableNumText.text = string.Format("{0}", num_move);
                    attackableNumText.text = string.Format("{0}", num_attack);
                    cautionMoveGobj.SetActive(num_move != 0); 
                    cautionAttackGobj.SetActive(num_attack != 0);
                }
                break;
            case STATUS.SAVE:
            case STATUS.LOAD:
                
                break;
        }
        
        
        
    }

    //ターン終了ボタンを押したときの処理
    public void TurnEndButton_Onclick()
    {
        BattleVal.status = STATUS.TURNEND;
        BattleVal.sysmenuflag = false;
        systemMenuPanel.SetActive(false);
    }

    void GetAbleNum(ref int num_move, ref int num_attack)
    {
        num_move = 0;
        num_attack = 0;
        foreach(Unitdata unit in BattleVal.unitlist)
        {
            if(unit.team == 0)
            {
                if (unit.movable) num_move++;
                if (unit.atackable) num_attack++;
            }
        }
    }
}
