using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//右クリック時のメニューを司るクラス
public class SystemMenu : MonoBehaviour {

    public GameObject systemMenuPanel;
    public Text turnCountText;
    public Button turnChangeButton;

	// Use this for initialization
	void Start () {
        systemMenuPanel.SetActive(false);
	}

    // Update is called once per frame
    void Update() {
        //メニュー表示中の場合
        if (BattleVal.sysmenuflag)
        {
            systemMenuPanel.SetActive(true);
            turnCountText.text = string.Format("{0}ターン目", BattleVal.turn);
        }
        else
        {
            systemMenuPanel.SetActive(false);
        }
    }

    //ターン終了ボタンを押したときの処理
    public void TurnEndButton_Onclick()
    {
        BattleVal.status = STATUS.TURNCHANGE;
        BattleVal.sysmenuflag = false;
    }
}
