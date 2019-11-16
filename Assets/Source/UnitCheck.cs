using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitCheck : MonoBehaviour
{
    //キャラクター詳細表示用
    public GameObject detailMenuGobj;
    public Image detailFace;
    public Text detailNameText;
    public Text detailLvText;
    public Text detailJobText;
    public Text detailStepText;
    public Text detailJumpText;
    public Text detailHpText;
    public Text detailExpText;
    public Text detailAtkText;
    public Text detailDefText;
    public Text detailMatText;
    public Text detailMdfText;
    public Text detailTecText;
    public Text detailLucText;

    public GameObject detailHpBar;
    public GameObject detailExpBar;
    public GameObject detailExpBarParent;

    public Color detailNormalHPTextColor = new Color(1, 1, 1, 1);
    public Color detailPinchHPTextColor = new Color(1, 0, 0, 1);

    public int HPBARWIDTH;
    public int EXPBARWIDTH;

    public GameObject prefabSkillPanel;
    public GameObject gobjSkillListContentParent;
    public Text textNoSkill;
    private List<GameObject> skillPanelList = new List<GameObject>();

    [SerializeField] AudioSource seAudioSource;
    [SerializeField] AudioClip seOk;

    [System.NonSerialized] public List<GameObject> unitsetlist = new List<GameObject>();
    public GameObject prefabUnitPanel;
    public GameObject gobjUnitPanelParent;
    //現在選択されているユニットを格納する
    public static Unitdata unitNowSelected;
    private static bool _is_unitchange = false;
    //ユニットの変更を確認する(UnitPanelからtrueを渡すのみ)
    public static bool is_unitchange
    {
        set { _is_unitchange = true; }
    }
    // Start is called before the first frame update
    void Start()
    {
        detailMenuGobj.SetActive(false);
        _is_unitchange = false;
    }

    // Update is called once per frame
    void Update()
    {
        //ユニットが選ばれたら更新
        if(_is_unitchange)
        {
            seAudioSource.clip = seOk;
            seAudioSource.Play();
            Setup_Detail(unitNowSelected);
            _is_unitchange = false;
        }
    }

    //ユニットリストの描画
    public void DrawUnitList()
    {
        //加入しているパーティメンバーのパネルを作成する。
        foreach (UnitSaveData unitsave in GameVal.masterSave.playerUnitList)
        {
            GameObject tempunitpanel = Instantiate(prefabUnitPanel, gobjUnitPanelParent.transform);
            tempunitpanel.GetComponent<UnitPanel>().SetUnit(unitsave, false, false, true); //ユニット確認用フラグを立てる
        }

    }

    //キャラの詳細ステータス表示
    public void Setup_Detail(Unitdata chara)
    {
        //メニュー系のテキスト設定
        detailNameText.text = chara.charaname;
        detailJobText.text = chara.jobname;
        detailLvText.text = chara.status.level.ToString();
        detailHpText.text = string.Format("{0}／{1}", chara.hp, chara.status.maxhp);
        if ((float)chara.hp / (float)chara.status.maxhp < 0.125f)
            detailHpText.color = detailPinchHPTextColor;
        else
            detailHpText.color = detailNormalHPTextColor;
        detailHpBar.GetComponent<RectTransform>().sizeDelta
            = new Vector2((float)chara.hp / (float)chara.status.maxhp * HPBARWIDTH, detailHpBar.GetComponent<RectTransform>().sizeDelta.y);
        //味方のみ経験値表示
        if (chara.team != 0) detailExpBarParent.SetActive(false);
        else
        {
            detailExpBarParent.SetActive(true);
            //LvMAX判定
            if (chara.status.level >= 99)
            {
                chara.status.level = 99; //メモリ改ざんとか対策
                detailExpText.text = string.Format("ＭＡＸ");
                detailExpText.color = detailPinchHPTextColor;
                detailExpBar.GetComponent<RectTransform>().sizeDelta
                    = new Vector2(EXPBARWIDTH, detailExpBar.GetComponent<RectTransform>().sizeDelta.y);
            }
            else
            {
                detailExpText.text = string.Format("あと{0}", chara.status.needexp - chara.status.exp);
                detailExpText.color = detailNormalHPTextColor;
                detailExpBar.GetComponent<RectTransform>().sizeDelta
                    = new Vector2((float)chara.status.exp / (float)chara.status.needexp * EXPBARWIDTH, detailExpBar.GetComponent<RectTransform>().sizeDelta.y);
            }
        }
        detailStepText.text = chara.status.step.ToString();
        detailJumpText.text = chara.status.jump.ToString();
        detailAtkText.text = chara.status.atk.ToString();
        detailDefText.text = chara.status.def.ToString();
        detailMatText.text = chara.status.mat.ToString();
        detailMdfText.text = chara.status.mdf.ToString();
        detailTecText.text = chara.status.tec.ToString();
        detailLucText.text = chara.status.luc.ToString();
        detailFace.sprite = chara.faceimage;

        //スキルリストの設定
        //Initialize
        foreach (GameObject temp in skillPanelList)
        {
            Destroy(temp);
        }
        skillPanelList.Clear();
        //スキルパネルの登録
        foreach (Skill skill in chara.skills)
        {
            GameObject temp = Instantiate(prefabSkillPanel, gobjSkillListContentParent.transform);
            temp.GetComponent<SkillPanel>().SetSkill(skill);
            skillPanelList.Add(temp);
        }
        if (skillPanelList.Count == 0)
            textNoSkill.gameObject.SetActive(true);
        else
            textNoSkill.gameObject.SetActive(false);

        detailMenuGobj.SetActive(true);

    }

}
