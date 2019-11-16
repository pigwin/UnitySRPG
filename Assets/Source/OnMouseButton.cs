using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//エキストラメニューでボタンオーバーした際の処理
public class OnMouseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(1,3)]
    [SerializeField] string descriptstring;
    [SerializeField] Text textDescriptTextField;

    public void OnPointerEnter(PointerEventData eventData)
    {
        textDescriptTextField.text = descriptstring;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        textDescriptTextField.text = "";
    }
}
