using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System;

//エキストラメニューでボタンオーバーした際の処理
public class OnMouseButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [TextArea(1,3)]
    [SerializeField] string descriptstring;
    [SerializeField] Text textDescriptTextField;
    
    public GameObject top;
    public Button Return_Button;
    public void OnPointerEnter(PointerEventData eventData)
    {
        textDescriptTextField.text = descriptstring;
        EventSystem.current.SetSelectedGameObject(this.gameObject);
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        textDescriptTextField.text = "";
        if (EventSystem.current.currentSelectedGameObject == this.gameObject && BattleVal.is_mouseinput)
        {
            EventSystem.current.SetSelectedGameObject(null);
        }
    }
    private void Update()
    {
        if (Input.GetAxis("Cancel") > 0)
        {
            try
            {
                Return_Button.onClick.Invoke();
            }
            catch (NullReferenceException)
            {
                //Debug.Log("OnMouseButton.cs Return_Buttonにオブジェクトが入れられていません");
            }
        }
        if (!BattleVal.is_mouseinput)
        {
            if (EventSystem.current.currentSelectedGameObject == null)
            {
                if (Math.Abs(Input.GetAxis("Vertical")) > 0 || Math.Abs(Input.GetAxis("Horizontal")) > 0)
                {
                    try
                    {
                        EventSystem.current.SetSelectedGameObject(top);
                    }
                    catch (NullReferenceException)
                    {
                        //Debug.Log("OnMouseButton.cs topにオブジェクトが入れられていません");
                    }
                }
            }
            else if (EventSystem.current.currentSelectedGameObject == this.gameObject)
            {
                textDescriptTextField.text = descriptstring;
            }
        }
    }

    public void OnSelect()
    {
        textDescriptTextField.text = descriptstring;
    }

    private void Start()
    {
        gameObject.AddComponent<EventTrigger>();

        //UnitPanelのキーパッド時の処理を追加
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.Select;
        entry.callback.AddListener((data) => OnSelect());

        GetComponent<EventTrigger>().triggers.Add(entry);
    }
}
