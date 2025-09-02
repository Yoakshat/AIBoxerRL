using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    Animator playerAnim;

    public bool canAttack = true; 

    public Bar energybar;  

    // xbox controller (for the player)
    private XboxControls controls; 
    private Dictionary<InputAction, string> actionNameMap;

    private string state; 
    private bool activated = false; 

    public bool firstTime = false; 

    public bool dead = false; 
     

    

    // Start is called before the first frame update
    void Start()
    {
        playerAnim = GetComponent<Animator>();
        energybar.SetMax(200);
    }

    void Awake(){
        controls = new XboxControls();

        // map controls to animation state
        actionNameMap = new Dictionary<InputAction, string>
        {
            { controls.Gameplay.Uppercut, "Uppercut" },
            { controls.Gameplay.RightHook, "RightHook" },
            { controls.Gameplay.LeftBlock, "LeftBlock" },
            { controls.Gameplay.BodyJab, "BodyJab" },
            { controls.Gameplay.CenterBlock, "CenterBlock" },
            { controls.Gameplay.Headbutt, "Headbutt" }, 
            { controls.Gameplay.Knee, "Knee" }, 
            { controls.Gameplay.RightBlock, "RightBlock" }, 
            { controls.Gameplay.JabCross, "JabCross" }
        };

        foreach(var kv in actionNameMap){
            kv.Key.performed += ctx => {
                state = kv.Value;
                activated = true; 
            };
        }
        


    }

    // check for collisions (then enable ragbody in enemy)

    // Update is called once per frame
    void Update()
    {
        
        
        // recharge energybar if not full
        // Time.deltaTime is seconds per frame 

        // for testing: very high (lower it later)

        // if you are not dead
        if(!dead){

            // only doing one thing at a time
            

            // only if you're idle, you can attack (e.g. cannot in middle of attack / reaction)
            if(IsIdle()){
                if(activated){
                    for(int i = 0; i < Constants.allMoves.Length; i++){
                        if(state == Constants.allMoves[i]){
                            onKeyPress(state, Constants.energies[i]);
                            activated = false; 
                            break;
                        }
                    }
                }
            }
        } 
        
        

    }

    void FixedUpdate(){
        if(!dead){
            energybar.Increase(10f * Time.fixedDeltaTime);
        }
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    public void onKeyPress(string move, float energy){
        if(energybar.slider.value >= energy){
            playerAnim.SetTrigger(move);
            energybar.Decrease(energy);
            firstTime = true; 
        }
    }

    public bool IsIdle(){
        AnimatorStateInfo attack = playerAnim.GetCurrentAnimatorStateInfo(0);
        AnimatorStateInfo reaction = playerAnim.GetCurrentAnimatorStateInfo(1);
        AnimatorStateInfo block = playerAnim.GetCurrentAnimatorStateInfo(2);

        return (attack.IsName("Idle") && reaction.IsName("Empty") && block.IsName("Empty"));
    }
}
