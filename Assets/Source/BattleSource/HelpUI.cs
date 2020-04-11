using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelpUI : MonoBehaviour
{
    [SerializeField] GameObject gobjHelpUI;
    [SerializeField] AudioClip seSlide;
    [SerializeField] GameObject gobjIntroHelpUI;

    // Update is called once per frame
    void Update()
    {
        if (!(BattleVal.status == STATUS.PLAYER_UNIT_SELECT
            || (BattleVal.status == STATUS.SETUP && CharaSetup.mode_Field)))
        {
            gobjHelpUI.SetActive(false);
            gobjIntroHelpUI.SetActive(false);
            return;
        }

        if (BattleVal.sysmenuflag)
        {
            gobjHelpUI.SetActive(false);
            gobjIntroHelpUI.SetActive(false);
            return;
        }

        if(Input.GetButtonDown("Help"))
        {
            gobjHelpUI.SetActive(!gobjHelpUI.activeSelf);
            Operation.setSE(seSlide);
        }
        gobjIntroHelpUI.SetActive(!gobjHelpUI.activeSelf);

    }
}
