using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

//マウスモードとキーパッドモードの変更・そのUIを司る
public class InputModeChange : MonoBehaviour
{
    //
    [SerializeField] private Sprite img_mousemode_on;    
    [SerializeField] private Sprite img_mousemode_off;    
    [SerializeField] private Sprite img_keypadmode_on;    
    [SerializeField] private Sprite img_keypadmode_off;

    [SerializeField] private Button btn_mousemode;
    [SerializeField] private Button btn_keypadmode;

    [SerializeField] private GameObject gobj_inputmodeparent;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(BattleVal.is_mouseinput)
        {
            //マウスモード
            btn_mousemode.image.sprite = img_mousemode_on;
            btn_keypadmode.image.sprite = img_keypadmode_off;

            btn_mousemode.interactable = false;
            btn_keypadmode.interactable = true;

            //セレクトを外す処理
            EventSystem.current.SetSelectedGameObject(null);
        }
        else
        {
            //キーボード・パッドモード
            btn_mousemode.image.sprite = img_mousemode_off;
            btn_keypadmode.image.sprite = img_keypadmode_on;

            btn_mousemode.interactable = true;
            btn_keypadmode.interactable = false;
        }

        //表示したくない場合の分岐
        if((BattleVal.status == STATUS.SETUP 
             && !(CharaSetup.state == CharaSetup.CharaSetupStatus.SET || CharaSetup.state == CharaSetup.CharaSetupStatus.CHECKENEMY))
            || BattleVal.status == STATUS.BATTLE
            || BattleVal.status == STATUS.USESKILL
            || BattleVal.status == STATUS.GETEXP
            || BattleVal.status == STATUS.TURNEND
            || BattleVal.status == STATUS.TURNCHANGE
            || BattleVal.status == STATUS.TURNCHANGE_SHOW
            || BattleVal.status == STATUS.STAGECLEAR
            || BattleVal.status == STATUS.GAMEOVER)
        {
            gobj_inputmodeparent.SetActive(false);
            return; //変更キー判定回避
        }
        else
        {
            gobj_inputmodeparent.SetActive(true);
        }
        

        if(Input.GetButtonDown("KeyMouseChange"))
        {
            if (BattleVal.is_mouseinput) OnClick_KeypadmodeButton();
            else OnClick_MousemodeButton();
        }
    }

    //モード切替
    public void OnClick_MousemodeButton()
    {
        BattleVal.is_mouseinput = true;
    }

    public void OnClick_KeypadmodeButton()
    {
        BattleVal.is_mouseinput = false;
    }

}
