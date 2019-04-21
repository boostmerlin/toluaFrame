using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorStateListener : StateMachineBehaviour
{
    private Dictionary<int, float> normalizedTimes = new Dictionary<int, float>();
    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //if (stateInfo.IsName("Showing"))
        //{
        //    animator.SendMessage("OnShowed", SendMessageOptions.DontRequireReceiver);
        //}
        normalizedTimes[stateInfo.fullPathHash] = 0;
    }

    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float t = stateInfo.normalizedTime;
        if (normalizedTimes[stateInfo.fullPathHash] < 1 && t >= 1)
        {
            normalizedTimes[stateInfo.fullPathHash] = 1;
            if (stateInfo.IsName("Show"))
            {
                animator.SendMessage("OnShowed", SendMessageOptions.DontRequireReceiver);
            }
            else if (stateInfo.IsName("Hide"))
            {
                if (!animator.GetBool("IsShow"))
                {
                    animator.SendMessage("OnHided", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
           }
}
