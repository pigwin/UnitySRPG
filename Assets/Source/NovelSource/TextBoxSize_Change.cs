using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextBoxSize_Change : MonoBehaviour
{
    public Canvas canvas;
    public Text text;
    [SerializeField]
    private int back_log_sizew = 964;
    public ScrollRect scrollrect;
    public static bool change_active = false;
    private void Start()
    {
        gameObject.GetComponent<RectTransform>().sizeDelta = new Vector3(back_log_sizew, 0, 0);
    }
    // Update is called once per frame
    void Update()
    {
        text.rectTransform.sizeDelta = new Vector2(back_log_sizew, text.preferredHeight);
        text.rectTransform.sizeDelta = new Vector2(back_log_sizew, text.preferredHeight);
        if(!change_active && gameObject.activeSelf)
        {
            scrollrect.verticalNormalizedPosition = 0;
            change_active = true;
        }
    }
}
