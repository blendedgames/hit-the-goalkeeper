﻿using System;
using DG.Tweening;
using Managers;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;
using System.Collections;
using System.Collections.Generic;
using Accessables;

public class BallMove : MonoBehaviour
{
    #region Singleton

    public static BallMove main;

    private void Awake()
    {
        if (main != null && main != this)
        {
            Destroy(gameObject);
            return;
        }

        main = this;
    }

    #endregion

    #region Variables

    private Rigidbody rb;
    private Vector3[] path = new Vector3[3];
    private PathType pathType = PathType.CatmullRom;
    private bool close;

    private static readonly int LegHit = Animator.StringToHash("LegHit");
    private static readonly int MidHit = Animator.StringToHash("MidHit");
    private static readonly int HeadHit = Animator.StringToHash("HeadHit");

    #endregion

    #region Movement

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (Time.frameCount % 3 == 0)
        {
            if (!GameManager.main.shootTheBall)
                return; //If player enters the inputs, global manager will set the trigger for movement.
            Movement();
        }
    }

    //This is the script for ball movement, Ball will move to goalkeepers selected positions.
    private void Movement()
    {
        var gameManagerPos =
            GameManager.main
                .transformPositionToShoot; //The position for the ball to reach, it was taken via players input.

        var position = transform.position; //This is for performance clearity.


        if (!GameManager.main.ballMoveStop && !close
        ) //If this trigger is not set by game manager, ball gets a force to reach the position.
        {
            //rb.AddForce((gameManagerPos - position).normalized *
            //         (GameManager.main.ballShootPowerValue * Time.fixedDeltaTime * 50)
            //   , ForceMode.Impulse); //We set rigidbody force, because without physics we have lag
            //BallShoot.main.Launch(gameManagerPos, Random.Range(5,10),-18);


            CameraControls.main.StartFieldOfViewChangeMainCam();

            BallParabollaMove(gameManagerPos,
                randomPos: new Vector3(Random.Range(-.45f, .45f), Random.Range(.35f, 1.26f), -5));
            close = true;
        }

        if ((transform.position - gameManagerPos).sqrMagnitude < .1f)
        {
//            Debug.Log("Veya Bu");
            switch (ShootSystem.instance.state)
            {
                case PlayerState.PlayerTurn:{
                    switch (GameManager.main.ballsHitRoad)
                    {
                        case TransformPosition.Head:
                            GameManager.main.goalKeeperAnim.SetBool(HeadHit, true);
                            break;
                        case TransformPosition.Spine:
                            GameManager.main.goalKeeperAnim.SetBool(MidHit, true);
                            break;
                        case TransformPosition.Leg:
                            GameManager.main.goalKeeperAnim.SetBool(LegHit, true);
                            break;
                    }
                }
                    break;
                case PlayerState.GoalKeeperTurn:
                {
                    switch (GameManager.main.ballsHitRoad)
                    {
                        case TransformPosition.Head:
                            GameManager.main.playerAnim.SetBool(HeadHit, true);
                            break;
                        case TransformPosition.Spine:
                            GameManager.main.playerAnim.SetBool(MidHit, true);
                            break;
                        case TransformPosition.Leg:
                            GameManager.main.playerAnim.SetBool(LegHit, true);
                            break;
                    } 
                }
                    break;
            }

            GameManager.main.ballMoveStop = true; //This is the trigger for balls force stop.
            Invoke("ChangeState", 2);
            AttackCompleted();
        }
    }

    private void BallParabollaMove(Vector3 endValue, Vector3 randomPos)
    {
        path[0] = transform.position;
        path[1] = randomPos;
        path[2] = endValue;
        transform.DOLocalPath(path, .75f, pathType, PathMode.Full3D).SetEase(Ease.Linear).OnComplete(ForceStop);
    }

    private void ForceStop()
    {
        transform.DOKill();
        //Camera.main.transform.DOKill();
        //rb.AddForce(Vector3.forward * 1000, ForceMode.Impulse);
    }

    private void ChangeState()
    {
        var p1 = GameManager.main.p1sCameraPosition.transform;
        var p2 = GameManager.main.p2sCameraPosition.transform;
        switch (ShootSystem.instance.state)
        {
            case PlayerState.PlayerTurn:
                DoTweenController.SequenceMoveAndRotate3D(Camera.main.transform, p1.position, p1.rotation, 1);
                transform.position = GameManager.main.p2BallsTransform.localPosition;
                break;
            case PlayerState.GoalKeeperTurn:
                DoTweenController.SequenceMoveAndRotate3D(Camera.main.transform, p2.position, p2.rotation, 1);
                transform.position = GameManager.main.p1BallsTransform.localPosition;
                break;
        }
    }

    #endregion

    #region Attack

    private void AttackCompleted()
    {
        if (ShootSystem.instance.state == PlayerState.PlayerTurn)
        {
            ShootSystem.instance.unitPlayer.currentHP = (int) GameManager.main.ballAttackValue;
            StartCoroutine(ShootSystem.instance.PlayerAttack());
        }
        else if (ShootSystem.instance.state == PlayerState.GoalKeeperTurn)
        {
            {
                ShootSystem.instance.unitGoalKeeper.currentHP = (int) GameManager.main.ballShootPowerValue;
                StartCoroutine(ShootSystem.instance.GoalKeeperAttack());
            }
            GameManager.main.shootTheBall = false;
        }
    }

    #endregion

    public IEnumerator ChangeKeeper()
    {
        GameManager.main.ballShootPowerValue = Random.Range(5, 20);
        Debug.Log(GameManager.main.ballShootPowerValue);
        GameManager.main.calculationID = 0;
        yield return new WaitForSeconds(.01f);
        GameManager.main.calculationID = 1;

        AttackCompleted();
    }
}