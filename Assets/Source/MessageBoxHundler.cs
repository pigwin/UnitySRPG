using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.EventSystems;

public class MessageBoxHundler : MonoBehaviour
{
    public Text textMessageTitle;
    public Text textMessage;
    public float fadeouttime = 0.2f;
    public CanvasGroup canvas;
    public Button button;

    [System.NonSerialized]public bool destroyflag = false;
    enum STATUS
    {
        START,
        FADEIN,
        SHOW,
        FADEOUT
    }
    STATUS state = STATUS.START;
    public void CreateMessageBox(string messagetitle, string message)
    {
        canvas = GetComponent<CanvasGroup>();
        canvas.alpha = 0;
        textMessageTitle.text = messagetitle;
        textMessage.text = message;
        state = STATUS.FADEIN;
    }

    public void onClick()
    {
        state = STATUS.FADEOUT;
    }

    private void Start()
    {
        destroyflag = false;
    }
    private void Update()
    {
        switch(state)
        {
            case STATUS.FADEIN:
                if(canvas.alpha >= 1)
                {
                    state = STATUS.SHOW;
                    button.Select();
                }
                else
                {
                    canvas.alpha += Time.deltaTime / fadeouttime;
                }
                break;
            case STATUS.SHOW:

                break;
            case STATUS.FADEOUT:
                if (canvas.alpha <= 0)
                {
                    destroyflag = true;
                    Destroy(this.gameObject);
                    EventSystem.current.SetSelectedGameObject(null);
                }
                else
                {
                    canvas.alpha -= Time.deltaTime / fadeouttime;
                }
                break;
        }
    }

}
