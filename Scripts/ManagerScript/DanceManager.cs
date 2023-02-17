using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DanceManager : MonoBehaviour
{
   
    Animator thisAnimator;

    // Start is called before the first frame update
    void Awake()
    {
        thisAnimator = GetComponent<Animator>();
    }

   public void SetAnimTrigger(int _idx)
    {
        if(_idx==0)
        {
            thisAnimator.SetTrigger("SoHappy");
        }
        else
        {
            thisAnimator.SetTrigger("HeroesTonight");
        }
    }
    public void StopDance()
    {
        thisAnimator.StopPlayback();
    }
}
