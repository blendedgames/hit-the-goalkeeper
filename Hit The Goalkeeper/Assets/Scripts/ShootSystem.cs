﻿using System.Collections;
using System.Collections.Generic;
using GUI;
using UnityEngine;


public enum PlayerState{PlayerTurn,GoalKeeperTurn,Won,Lost}
public class ShootSystem : MonoBehaviour
{
    #region Singleton
    public static ShootSystem instance;
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    #endregion
    public PlayerState state;
     Unit unitPlayer;
     //Unit unitGoalKeeper;
    public HUDScript playerHUD;
    public HUDScript goalKeeperHUD;
    

    // Start is called before the first frame update
    void Start()
    {
        state=PlayerState.PlayerTurn;
    }
    public void SetupShoot()
    {
    // playerHUD.SetHud(unitPlayer);
   goalKeeperHUD.SetHud(unitPlayer);

  
    }
  public  IEnumerator PlayerAttack(){
    //bool isDead=  unitGoalKeeper.TakeDamage(unitPlayer.damage);

    yield return new WaitForSeconds(0.02f);

    
    // if(isDead)
    // {
    //     state=PlayerState.Won;
    //     EndShoot();
    // }
    // else{
    //     state=PlayerState.GoalKeeperTurn;
    //     StartCoroutine(GoalKeeperTurn());
    // }
    }
   
//   public IEnumerator GoalKeeperTurn(){ 
//        bool isDead= unitPlayer.TakeDamage(unitGoalKeeper.damage);
//        playerHUD.SetHp(unitGoalKeeper.currentHP);
//        yield return new WaitForSeconds(5f);
//         if(isDead)
//            {
//              state=PlayerState.Lost;
//              EndShoot();
//          }
//         else{
//               state=PlayerState.PlayerTurn;

//         }
     
//      }

       void EndShoot(){
            if(state==PlayerState.Won) Debug.Log( "you won");
            else if(state==PlayerState.Lost) Debug.Log( "you lost");
       }
}

    


