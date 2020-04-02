﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
   // public float comerValue;
   // public float a;
    public float rise;
    public GameObject goalkeeper;
    public Rigidbody rb;
    public bool direction = false;
    private bool speed = false, final = false;
    [SerializeField] private float shootPower; //Lerp için time değişkeni inputtan gelen değer.

    [SerializeField] public Animator cameraAnim;
   Vector3 position;
   private static readonly int Shoot = Animator.StringToHash("Shoot");

   private void Start()
    {
        rb = GetComponent<Rigidbody>();
        //a = Random.Range(0, 3);
    }

    // Update is called once per frame
    private void Update()
    {
        if (!GameManager.main.ballMoveStart) return;
        //If ball is kicked by the player, then we will the ball to corresponding position.
        shootPower = GameManager.main.ballShootPowerValue;
       
        Movement();
    }

    private void Movement()
    {
        //Vector3 direction = (goalkeeper.transform.position - this.gameObject.transform.position).normalized;
        //  Vector3 firstPos = transform.position;      
        //   rb.AddForce(direction * comerValue,ForceMode.Force);

    position = GameManager.main.transformPositionToShoot.position;
       
          Debug.Log(position.y +" "+ position.z);
        transform.position =
            Vector3.Lerp(transform.position, goalkeeper.transform.position, shootPower * Time.deltaTime);
        //   transform.position= Vector3.MoveTowards(transform.localPosition,goalkeeper.transform.localPosition,comerValue*Time.deltaTime);
        if (this.gameObject.transform.position.z < goalkeeper.transform.localPosition.z/2 )
        {
            this.gameObject.transform.position -= new Vector3(position.x, position.y,position.z) * Time.deltaTime;
            //  this.gameObject.transform.localPosition += new Vector3 (transform.localPosition.x, rise, 0)*Time.deltaTime;
        }
        // else if (this.gameObject.transform.position.z >= goalkeeper.transform.localPosition.z / 2)
        // {
        //     this.gameObject.transform.localPosition -= new Vector3(position.x,0, position.z) * Time.deltaTime;
        //     //  transform.position= Vector3.Lerp( transform.position,goalkeeper.transform.position,Time.deltaTime*rise);
        // }
    }

    // private void OnTriggerEnter(Collider other)
    // {
    //     if (!other.CompareTag("goalkeeper")) return;
    //     cameraAnim.SetBool("goalKeeperTouch", true);

    //     final = true;
    //     direction = false;
    //     speed = false;
    // }
}