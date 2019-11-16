using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//カットイン演出の終了判定
public class CutinChecker : MonoBehaviour
{
    private bool _cutinFinish = false;
    [SerializeField] private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        _cutinFinish = false;    
    }

    public bool cutinFinish
    {
        get
        {
            if (animator.GetCurrentAnimatorStateInfo(0).IsTag("END"))
                return true;
            else
                return false;
        }
    }
}
