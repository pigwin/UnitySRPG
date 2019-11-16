using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Staffroll : MonoBehaviour
{
    [SerializeField] GameObject gobjStaffroll;
    [SerializeField] int time = 2 * 60 + 53;
    [SerializeField] int sizeY = 4000;
    [SerializeField] AudioSource bgmAudioSource;
    [SerializeField] Fade fadeout;
   
    enum STATE
    {
        STAFFROLL,
        WAIT
    }
    STATE state = STATE.STAFFROLL;


    // Use this for initialization
    void Start()
    {
        state = STATE.STAFFROLL;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            case STATE.STAFFROLL:
                if (gobjStaffroll.GetComponent<RectTransform>().position.y >= sizeY+768)
                {
                    bgmAudioSource.Stop();
                    gobjStaffroll.transform.GetComponent<RectTransform>().position = new Vector3(0, sizeY + 768, 0);
                    fadeout.FadeIn(1, () =>
                    {
                        SceneManager.LoadScene("WelcomeToStageSelect", LoadSceneMode.Single);
                        Resources.UnloadUnusedAssets();
                    });
                    state = STATE.WAIT;
                }
                else
                {
                    gobjStaffroll.GetComponent<RectTransform>().position += new Vector3(0, sizeY / time * Time.deltaTime, 0);
                }
                break;

            case STATE.WAIT:

                break;
        }
    }
}
