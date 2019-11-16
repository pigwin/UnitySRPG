using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPanel : MonoBehaviour
{
    public Text CharacterName;
    public Image CharacterImage;
    public void Setup_Detail(Unitdata chara)
    {
        CharacterName.text = chara.charaname;
        CharacterImage.sprite = chara.faceimage;
    }
}
