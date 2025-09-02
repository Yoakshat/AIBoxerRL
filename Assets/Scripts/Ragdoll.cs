using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// use this script for FULL KNOCKOUT
public class Ragdoll : MonoBehaviour{
    private Player player; 
    // get all rigidbodies and colliders
    private Rigidbody[] bodies; 
    private  Collider[] colliders;
    private Animator anim; 
    public Ragdoll oppDoll; 




    // initialize all rigidbodies and colliders
    void Start(){
        bodies = GetComponentsInChildren<Rigidbody>();
        colliders = GetComponentsInChildren<Collider>();
        anim = GetComponent<Animator>();
        player = GetComponent<Player>();

        

        // no one starts as a ragdoll
        enableRagdoll(false);
    }

    // only useful for knockout
    public void enableRagdoll(bool enabled){
        // we need the parent to be able to move 
        // this rule never applies to first body

        foreach(Rigidbody rb in bodies){
            rb.isKinematic = !enabled;
        }

        // the overall rigidbody can have physics 
        bodies[0].isKinematic = false;

        anim.enabled = !enabled;
    }

    public Collider[] getColliders(){
        return this.colliders;
    }

    public Rigidbody[] getBodies(){
        return this.bodies;
    }

    void Update(){


        // if they don't face each other, attack shouldn't work
        // OR if they're dead (DUH!)
        if(player.isDead() || !player.faceEachOther()){
            return; 
        }

        // for each loop in the states

        string currState = "";

        
        AnimatorStateInfo state = player.CurrentMove();
        foreach(string aState in Constants.attackStates){
            if(state.IsName(aState)){
                // then continue
                currState = aState; 
            }
        }


        if(currState == ""){
            return; 
        } 

        // if we called animateReaction once, don't do it again until we have been in idle for long enough
        // otherwise will keep on calling it, and player will die very quickly!
        
        for(int j = 1; j < colliders.Length; j++){
            Collider collMe = colliders[j];
            // for every collider there is a rigidbody
            Collider[] oppColls = oppDoll.getColliders();
            
            // Yep done!
            // you can only hit, when you're in an idle state
            // also animate reaction once only during collision

            for(int i = 1; i < oppColls.Length; i++){
                if(collMe.bounds.Intersects(oppColls[i].bounds)){
                    Debug.Log("Move: " + currState);
                    // curr state will be in uppecut for some time 
                    if(currState == "Uppercut"){
                        player.AnimateReaction("Uppercut", "HeadBack", 500, 6f);
                    } else if (currState == "Headbutt"){
                        player.AnimateReaction("Headbutt", "HeadBack", 400, 4f);
                    } else if (currState == "BodyJab"){
                        player.AnimateReaction("BodyJab", "RibHit", 200, 3f);
                    } else if (currState == "RightHook"){
                        player.AnimateReaction("RightHook", "SideHit", 350, 3f);
                    } else if (currState == "Knee"){
                        player.AnimateReaction("Knee", "BodyForward", 200, 3f);
                    } else if (currState == "JabCross"){
                        player.AnimateReaction("JabCross", "RibHit", 350, 3f);
                    }
                }
            }
        }

    
    }

    


}