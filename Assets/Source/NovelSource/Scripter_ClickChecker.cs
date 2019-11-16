using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
/************************************************************/
/*ノベルパート用のクリック判別用のクラス                    */
/************************************************************/
public class Scripter_ClickChecker : MonoBehaviour
{
    public List<Button> buttons = new List<Button>();
    public static bool button_over = false;
    PointerEventData eventDataCurrent = new PointerEventData(EventSystem.current);
    List<RaycastResult> raycast = new List<RaycastResult>();
    
    // Update is called once per frame
    void Update()
    {
        eventDataCurrent.position = Input.mousePosition;
        EventSystem.current.RaycastAll(eventDataCurrent, raycast);
        foreach(RaycastResult tmp in raycast)
        {
            foreach(Button button in buttons)
            {
                if(button.name == tmp.gameObject.name)
                {
                    button_over = true;
                    return;
                }
            }            
        }
        button_over = false;
    }
}
