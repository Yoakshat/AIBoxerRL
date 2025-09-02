using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement: MonoBehaviour {
    public float moveSpeed; 
    public float bounceSpeed = 20f; 
    public Transform orientation; 
    // are you a bot
    public bool bot = false; 
    private bool bounceRope = false; 

    float horizontalInput; 
    float verticalInput;

    private bool firstFrame = true; 
    private Vector3 startPosition; 

    Vector3 moveDirection; 
    Rigidbody rb;

    public bool dead = false; 
    private int faceFactor = 1; 

    private XboxControls controls; 

    private Vector2 moveInput = Vector2.zero; 
    
    private bool isMoving = false; 
    private int movingTicks = 0; 
    private int movingFrames = 20; 

    void Awake(){
        controls = new XboxControls();

        controls.Gameplay.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Move.canceled += ctx => moveInput = Vector2.zero;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    void Start(){
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if(transform.rotation.y == 1f){
            faceFactor = -1; 
        } 
    }


    void Update(){
        UpdateMoving(); 

        if(firstFrame){
            startPosition = orientation.position; 
            firstFrame = false; 
        }

        // quick hack to fix bug
        // if outside of ring, would have fallen past ground
        // -> reset him to original position
        if(orientation.position.y < -20){
            // Debug.Log("yep we outside!");
            orientation.position = startPosition; 
        }

        MyInput();


        if(!bounceRope){
            // if it's not bouncing, control the speed
            SpeedControl(moveSpeed);
        } else {
            // wait till speed slows defore before you can regain control 
            if(!isMoving){
                // our bounce is done!
                bounceRope = false; 
            }
        }
    }

    private void UpdateMoving(){
        bool notMoving = (Mathf.Abs(rb.velocity[0]) <= 0.2f 
                            && Mathf.Abs(rb.velocity[1]) <= 0.2f 
                                && Mathf.Abs(rb.velocity[2]) <= 0.2f);

        if(notMoving){
            movingTicks++; 
            if(movingTicks > movingFrames){
                // you are actually not moving
                isMoving = false; 
            }
        } else {
            isMoving = true; 
            movingTicks = 0; 
        }
    }

    void FixedUpdate(){
        // only move player if not dead
        if(!dead){
            if(!bounceRope){
                MovePlayer();
            }
        }
    }


    // add rebound effect on ropes; 
    void OnCollisionStay(Collision collision){
        float reboundFactor = 6f; 

        string name = collision.gameObject.name;
        Debug.Log("name: " + name);
        if(name.Contains("ropes")){
            Vector3 move = new Vector3(0f, 0f, 0f); 

            if(name.Contains("ropes_A")){
                move.x = -1 * reboundFactor;
            } else if (name.Contains("ropes_B")){
                move.z = reboundFactor; 
            } else if (name.Contains("ropes_C")){
                move.x = reboundFactor; 
            } else if(name.Contains("ropes_D")){
                move.z = -1 * reboundFactor; 
            }
            
            rb.AddForce(move, ForceMode.Impulse);
            bounceRope = true; 
            
        }
    }

    void MyInput(){
        // get input from controller
        if(!bot){
            horizontalInput += moveInput[0] * faceFactor; 
            verticalInput += moveInput[1] * faceFactor; 
        }
    }

    public void setPos(Vector3 pos){
        orientation.position = pos;
    }

    public void moveHorz(float horz){
        horizontalInput += horz; 
    }

    public void moveVert(float vert){
        verticalInput += vert;  
    }

    private void MovePlayer(){
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // reset horizontal and vertical input (after moved)
        horizontalInput = 0;
        verticalInput = 0; 
    }

    public void MoveBackwards(float pushForce){
        // when moving backwards also disable control 
        rb.AddForce((-1 * orientation.forward).normalized * pushForce, ForceMode.Impulse);
        // same mechanics
        bounceRope = true;
    }



     private void SpeedControl(float limit)
     {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // limit velocity if needed
        if(flatVel.magnitude > limit)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

}