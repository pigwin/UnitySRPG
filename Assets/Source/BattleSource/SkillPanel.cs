using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/*******************************************************************/
/*スキルを表示に関する処理を行うクラス                             */
/*******************************************************************/
public class SkillPanel : MonoBehaviour
{
    public Text textSkillName;
    public Text textIsMgc;
    public Text textIsCutin;
    public Text textSkillRate;
    public Text textSkillDetail;
    public Text textSkillUse;
    public Text textIgnoreDef;
    public Image imageSkillIcon;
    public Image imageSkillPanel;

    public void SetSkill(Skill skill)
    {
        textSkillName.text = skill.skillname;
        textSkillDetail.text = skill.skilldetail;

        if (skill.is_mgc)
            textIsMgc.text = "魔法/";
        else
            textIsMgc.text = "物理/";
        if (skill.s_atk < 0)
            textIsMgc.text += "回復";
        else
            textIsMgc.text += "攻撃";

        textSkillRate.text = string.Format("威力:x{0:0.00}",Mathf.Abs(skill.s_atk));

        textSkillUse.text = string.Format("使用回数:{0}回/{1}回", skill.use, skill.maxuse);

        if (skill.is_cutscene)
            textIsCutin.gameObject.SetActive(true);
        else
            textIsCutin.gameObject.SetActive(false);

        if (skill.is_ignoredef)
            textIgnoreDef.gameObject.SetActive(true);
        else
            textIgnoreDef.gameObject.SetActive(false);

        imageSkillIcon.sprite = skill.icon;
        imageSkillPanel.color = skill.skillColor;
    }
}
