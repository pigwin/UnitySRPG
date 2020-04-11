using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public class StageSelectMenuAlpha : MonoBehaviour
{
    CanvasGroup canvas;
    [SerializeField] float fadeintime = 1.0f;
    enum STATE
    {
        FADEIN,
        FADEOUT,
        NORMAL
    }
    STATE state = STATE.NORMAL;

    private void Start()
    {
        canvas = GetComponent<CanvasGroup>();    
    }

    public void FadeIn()
    {
        canvas.gameObject.SetActive(true);
        canvas.alpha = 0;
        canvas.interactable = true;
        state = STATE.FADEIN;
    }
    public void FadeOut()
    {
        canvas.gameObject.SetActive(false);
        canvas.alpha = 1;
        canvas.interactable = false;
        canvas.blocksRaycasts = false;
        state = STATE.FADEOUT;
    }
    private void Update()
    {
        switch(state)
        {
            case STATE.FADEIN:
                canvas.alpha += Time.deltaTime / fadeintime;
                if(canvas.alpha >= 1)
                {
                    canvas.alpha = 1;
                    canvas.blocksRaycasts = true;
                    state = STATE.NORMAL;
                }
                break;
            case STATE.FADEOUT:
                canvas.alpha -= Time.deltaTime / fadeintime;
                if (canvas.alpha <= 0)
                {
                    canvas.alpha = 0;
                    state = STATE.NORMAL;
                }
                break;
        }
        if(canvas.alpha == 0.0f)
        {
            canvas.gameObject.SetActive(false);
        }
    }
}
