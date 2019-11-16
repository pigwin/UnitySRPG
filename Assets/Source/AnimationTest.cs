using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationTest : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.anyKey)
        {
            GetComponent<Animator>().SetBool("Walk", true);
        }
        else
        {
            GetComponent<Animator>().SetBool("Walk", false);
        }
        Debug.Log(GetComponent<Animator>().GetBool("Walk"));
    }
}
