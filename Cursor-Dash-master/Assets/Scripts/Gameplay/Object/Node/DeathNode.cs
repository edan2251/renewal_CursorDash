using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathNode : Node
{
    public override void Execute(){
        gameObject.SetActive(true);
        ExecuteCoroutine().Start(this);
    }

    private IEnumerator ExecuteCoroutine(){
        do{
            yield return YieldInstructionCache.WaitFrame;
        }while(animator.GetCurrentAnimatorStateInfo(0).normalizedTime % 1 < 0.99f);
        
        Attack();
    }
    

    public void Attack(){
        var randomPosiion = Random.Range(0,8);
        
    }

    public void Attack(int position){
        
    }

    public override void ResetObject()
    {
        base.ResetObject();
    }
}
