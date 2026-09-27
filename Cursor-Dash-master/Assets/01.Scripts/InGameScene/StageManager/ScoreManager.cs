using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class ScoreManager : MonoBehaviour
{
    private float defaultHp = 3;
    private float hp;

    private float timeScore;

    private Action<float> hpEditFunction;
    private Action deathFunction;
    private Action<float> timeScoreFunction;

    public float Hp{
        get{
            return hp;
        }

        set{
            hp = value;

            if(hp <= 0){
                hp = 0;
                deathFunction();
            }
            else if(hp > defaultHp){
                hp = defaultHp;
            }

            HoldAchivement.IsMaxHp = false;
            GameManager.instance.soundManager.PlaySFX(ClipTag.coreDamaged);  
            StageManager.instance.CameraDamagedFlash();          
            hpEditFunction(hp / defaultHp);
        }
    }

    public float TimeScore{
        get{
            return timeScore;
        }

        set{
            timeScore = value;
            timeScoreFunction(timeScore);
        }
    }

    private void Start(){
        hp = defaultHp;
    }

    public void SetHpEditAction(Action<float> action){
        hpEditFunction = action;
    }

    public void SetDeathAction(Action action){
        deathFunction = action;
    }

    public void SetScoreAction(Action<float> action){
        timeScoreFunction = action;
    }

}
