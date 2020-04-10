﻿using System.Collections;
using System.Diagnostics;
using System.Globalization;
using Accessables;
using DG.Tweening;
using GUI;
using Managers;
using UnityEngine;
using Debug = UnityEngine.Debug;
using Random = UnityEngine.Random;
using Vector3 = UnityEngine.Vector3;

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

    private Vector3[] path = new Vector3[3];
    private PathType pathType = PathType.CatmullRom;
    private bool _close;
    private Vector3 _gameManagerPos;
    private Camera _camera;
    public bool _updateStop;
    float randomPos;

    private static readonly int LegHit = Animator.StringToHash("LegHit");
    private static readonly int MidHit = Animator.StringToHash("MidHit");
    private static readonly int HeadHit = Animator.StringToHash("HeadHit");
    private static readonly int Laugh = Animator.StringToHash("Laugh");
    private static readonly int Shoot = Animator.StringToHash("Shoot");

    #endregion

    private void Start()
    {
        _camera = Camera.main;
    }

    private void Update()
    {
        if ((transform.position - _gameManagerPos).sqrMagnitude < 1.5f && !_updateStop)
        {
            AnimStateChanger();
        }

        if ((transform.position - _gameManagerPos).sqrMagnitude < 10 && !_updateStop)
        {
            CameraControls.main.CameraGetCloser();
        }
    }

    #region Movement

    //This is the script for ball movement, Ball will move to goalkeepers selected positions.
    public void Movement()
    {
        _gameManagerPos =
            GameManager.main
                .transformPositionToShoot; //The position for the ball to reach, it was taken via players input.
        //CameraControls.main.StartFieldOfViewChangeMainCam();
        if (ShootSystem.instance.state == PlayerState.PlayerTurn)
            randomPos = GameManager.main.ballCurveValue;
        else if (ShootSystem.instance.state == PlayerState.GoalKeeperTurn)
            randomPos = Random.Range(-.4f, .4f);
        BallParabollaMove(_gameManagerPos,
            randomPos: new Vector3(randomPos, Random.Range(.35f, 1.15f), -5));
    }

    private void BallParabollaMove(Vector3 endValue, Vector3 randomPos)
    {
        path[0] = transform.position;
        path[1] = randomPos;
        path[2] = endValue;
        transform.DOLocalPath(path, 1f, pathType).SetEase(Ease.Flash).OnComplete(CameraFollowStop);
    }


    private void ChangeState()
    {
        var p1 = GameManager.main.p1sCameraPosition.transform;
        var p2 = GameManager.main.p2sCameraPosition.transform;
        switch (ShootSystem.instance.state)
        {
            case PlayerState.PlayerTurn:

                DoTweenController.SeqMoveRotateCallBack(_camera.transform, p2.position, p2.eulerAngles, 2,
                    ChangeStateDelay, Ease.Flash);
                break;
            case PlayerState.GoalKeeperTurn:

                DoTweenController.SeqMoveRotateCallBack(_camera.transform, p1.position, p1.eulerAngles, 2,
                    ChangeStateDelay, Ease.Flash);
                break;
        }
    }

    private void ChangeStateDelay()
    {
        switch (ShootSystem.instance.state)
        {
            case PlayerState.PlayerTurn:
                transform.position = GameManager.main.p2BallsTransform.localPosition;
                GameManager.main.p1.transform.position = GameManager.main.p1Pos.position;
                GameManager.main.p1.transform.rotation = GameManager.main.p1Pos.rotation;
                break;
            case PlayerState.GoalKeeperTurn:
                transform.position = GameManager.main.p1BallsTransform.localPosition;
                GameManager.main.p2.transform.position = GameManager.main.p2Pos.position;
                GameManager.main.p2.transform.rotation = GameManager.main.p2Pos.rotation;
                break;
        }

        StartCoroutine(ChangeStateDelayCoroutine());
    }

    private IEnumerator ChangeStateDelayCoroutine()
    {
        if (ShootSystem.instance.state == PlayerState.PlayerTurn)
        {
            if (GameManager.main.ballsHitRoad != TransformPosition.Off)
            {
                GameManager.main.ballAttackValue = 0;
            }

            ShootSystem.instance.unitPlayer.damage = (int) GameManager.main.ballAttackValue;
        }
        else if (ShootSystem.instance.state == PlayerState.GoalKeeperTurn)
        {
            if (GameManager.main.ballsHitRoad != TransformPosition.Off)
            {
                GameManager.main.ballAttackValue = 0;
            }

            ShootSystem.instance.unitGoalKeeper.damage = (int) GameManager.main.ballAttackValue;
        }

        yield return new WaitForSeconds(.3f);
        switch (ShootSystem.instance.state)
        {
            case PlayerState.PlayerTurn:
                AttackCompleted();
                break;
            case PlayerState.GoalKeeperTurn:
                AttackCompleted();
                break;
        }
    }

    #endregion

    #region Attack

    private void AttackCompleted()
    {
        if (ShootSystem.instance.state == PlayerState.PlayerTurn)
        {
            if (GameManager.main.ballsHitRoad != TransformPosition.Off)
            {
                GameManager.main.ballAttackValue = 0;
            }

            ShootSystem.instance.unitPlayer.damage = (int) GameManager.main.ballAttackValue;


            ShootSystem.instance.PlayerAttack();
        }
        else if (ShootSystem.instance.state == PlayerState.GoalKeeperTurn)
        {
            {
                if (GameManager.main.ballsHitRoad != TransformPosition.Off)
                {
                    GameManager.main.ballAttackValue = 0;
                }

                ShootSystem.instance.unitGoalKeeper.damage = (int) GameManager.main.ballAttackValue;


                ShootSystem.instance.GoalKeeperAttack();
            }
        }
    }

    #endregion

    #region AnimStateChanger

    private void AnimStateChanger()
    {
        switch (ShootSystem.instance.state)
        {
            case PlayerState.PlayerTurn:
            {
                switch (GameManager.main.ballsHitRoad)
                {
                    case TransformPosition.Head:
                        GameManager.main.goalKeeperAnim.SetBool(HeadHit, true);
                        GameManager.main.playerAnim.SetLayerWeight(1, 1);
                        break;
                    case TransformPosition.Spine:
                        GameManager.main.goalKeeperAnim.SetBool(MidHit, true);
                        break;
                    case TransformPosition.Leg:
                        GameManager.main.goalKeeperAnim.SetBool(LegHit, true);
                        break;
                    case TransformPosition.Off:
                        GameManager.main.goalKeeperAnim.SetBool(Laugh, true);
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
                        GameManager.main.goalKeeperAnim.SetLayerWeight(1, 1);
                        break;
                    case TransformPosition.Spine:
                        GameManager.main.playerAnim.SetBool(MidHit, true);
                        break;
                    case TransformPosition.Leg:
                        GameManager.main.playerAnim.SetBool(LegHit, true);
                        break;
                    case TransformPosition.Off:
                        GameManager.main.playerAnim.SetBool(Laugh, true);
                        break;
                }
            }
                break;
        }

        _updateStop = true;
        ChangeState();
    }

    #endregion

    public void ChangeKeeper()
    {
        GameManager.main.ballAttackValue = Random.Range(5, 20);
        GameManager.main.transformPositionToShoot = GameManager.main.playerShootPositions[Random.Range(0, 3)].position;
        GameManager.main.goalKeeperAnim.SetBool(Shoot, true);
        GameManager.main.ActivateCam();
    }

    private void CameraFollowStop()
    {
        transform.DOKill();
        _camera.GetComponent<CameraControls>().enabled = false;
        GetComponent<Rigidbody>().AddForce((Vector3.up * 100) + (Vector3.forward * (10000 * Time.fixedDeltaTime)),
            ForceMode.Force);
    }
}