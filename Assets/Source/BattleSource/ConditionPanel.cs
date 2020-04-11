using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ConditionPanel : MonoBehaviour
{
    public Text textConditionName;
    public Text textConditionDetail;
    public Text textConditionUse;
    public Image imageConditionIcon;
    public Image imageConditionPanel;

    public void SetCondition(Condition condition)
    {
        textConditionName.text = condition.condname;
        textConditionDetail.text = condition.strdetail;

        if(condition.contiturn == -1) textConditionUse.text = string.Format("永続");
        else textConditionUse.text = string.Format("{0}ターン/{1}ターン", condition.nowstate.nowturn, condition.contiturn);

        imageConditionIcon.sprite = condition.icon;
        imageConditionPanel.color = condition.panelcolor;
    }
}
