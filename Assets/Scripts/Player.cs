using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player: MonoBehaviour {

    // handles movement, physics, animation, health ALL IN ONE
    [HideInInspector]
    public PlayerMovement move; 

    [HideInInspector]
    public Animator anim; 

    [HideInInspector]
    public PlayerController controller; 

    public Player opponent; 

    public Bar healthbar; 

    private bool bot; 

    private bool gameOver = false;
    private bool dead = false; 
    private bool getUp = false; 
    private float deadTime = 0f; 

    private float reviveProb = 1f; 



    void Start(){
        move = GetComponent<PlayerMovement>(); 
        anim = GetComponent<Animator>(); 
        controller = GetComponent<PlayerController>(); 

        healthbar.SetMax(1000);

        bot = move.bot; 
    }

    void Update(){
        
        // update other dead variables
        controller.dead = dead; 
        move.dead = dead; 

        // if dead, wait 5 seconds (need some drama)
        if(getUp){
            // wait till current move is idle 
            if(CurrentMove().IsName("Idle")){
                dead = false; 
                getUp = false; 
                deadTime = 0f; 
            }

            // we're back and ready to fight (back to 1/10 of max health)
            healthbar.Set(200);
        } else if(dead && !gameOver){
            deadTime += Time.deltaTime;
            if(deadTime >= 5){
                // then with some probability, revive
                if(Random.Range(0f, 1f) < reviveProb){
                    anim.SetTrigger("GetUp");
                    getUp = true; 
                    // next time of reviving is halfed; 
                    reviveProb *= 0.5f; 
                } else {
                    gameOver = true; 
                }
            }
        }

        


        // check if dead
        if(!dead && healthbar.slider.value <= 0){
            // set trigger for KO
            anim.SetTrigger("KO");
            dead = true;
        }

        // AnimatorStateInfo m = CurrentMove();

        // if they are different, update 

        // idle, uppercut
        // if dead, don't just magically revive
        

    }

    public void Reset(){
        // reset any necessary variables
        dead = false; 
        getUp = false; 
        gameOver = false; 

        deadTime = 0f; 
        reviveProb = 1f;

        // go back to idle state (if KOed)
        // otherwise will already return
        anim.SetTrigger("Reset");
        // fill healthbar
        healthbar.Fill();
        // fill energybar
        controller.energybar.Fill();

    }

    void FixedUpdate(){
        if(!dead){
            // 30 - 35f seems good
            healthbar.Increase(15f * Time.fixedDeltaTime);
        }
    }

    public bool isGameOver(){
        return gameOver;
    }

    public bool isDead(){
        return dead; 
    }

    public bool faceEachOther(){
        // check their orientation to check if they're facing each other
        if(transform.rotation.y == 1f){
            return transform.position.z >= opponent.transform.position.z;
        }

        return opponent.transform.position.z >= transform.position.z; 
    }

    public void Push(float pushForce){
        opponent.move.MoveBackwards(pushForce);
    }
    
    public void Damage(int damage){
        opponent.healthbar.Set((int)opponent.healthbar.slider.value - damage);
    }

    
    // do damage, push, and set move up for recharging
    public void AnimateReaction(string attack, string animation, int damage, float pushFactor){
        // only want to animate reaction once for one hit
        if(!opponent.isDead() && controller.firstTime){

            bool blocked = OpponentBlockSuccess(attack);
            
            if(!blocked){
                opponent.anim.SetTrigger(animation);   
                Damage(damage);
                // Push(pushFactor);
            } else {
                // if blocked -> less damage, less push, no reaction
                Damage((int)(damage * 0.2));
            }

            controller.firstTime = false; 
        } 

    }

    // goal: check opponent block successsful
    private bool OpponentBlockSuccess(string attack){
        // is block successful 
        // if block state and right hook, uppercut, headbutt, or jabcross (will always block these)

        bool unblockable = (attack == "Knee" || attack == "BodyJab");
        if(unblockable){
            return false; 
        }

        AnimatorStateInfo state = opponent.CurrentBlock();
            
        foreach(string bState in Constants.blockStates){
            if(state.IsName(bState)){
                return true; 
            }
        }

        return false; 
    }

    // gets the animation in the base layer (which is the attack)

    public AnimatorStateInfo CurrentMove(){
        return anim.GetCurrentAnimatorStateInfo(0);
    }

    public AnimatorStateInfo CurrentReaction(){
        return anim.GetCurrentAnimatorStateInfo(1);
    }

    public AnimatorStateInfo CurrentBlock(){
        return anim.GetCurrentAnimatorStateInfo(2);
    }




}