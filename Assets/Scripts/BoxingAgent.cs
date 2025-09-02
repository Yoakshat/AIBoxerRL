using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Policies;
using UnityEngine.UI;

using Random = UnityEngine.Random;

public class BoxingAgent : Agent {

    private Player agent; 
    private Player opponent; 

    public BoxingAgent oppBot; 

    

    public GameObject ring; 
    private Bounds b; 

    private float oppHealth; 
    private float myHealth; 

    // in the case they're turned towards camera
    private int faceFactor = 1; 

    private int steps = 0; 


    void Start(){

        
        agent = GetComponent<Player>(); 
        opponent = agent.opponent;
        b = ring.GetComponent<MeshRenderer>().bounds;

        oppHealth = opponent.healthbar.slider.value;
        myHealth = agent.healthbar.slider.value;

        // they're not facing camera (1 corresponds to a 180 degree rotation)
        if(agent.transform.rotation.y == 1f){
            faceFactor = -1; 
        } 

    }

    // reset the environment for a new episode
    // random initialization (so agent can learn in diferent conditions)
    public override void OnEpisodeBegin()
    {   
        bool isInference = GetComponent<BehaviorParameters>().BehaviorType == BehaviorType.InferenceOnly;
        
        if(!isInference){
            steps = 0; 
            // reset health + energy / other variables
            agent.Reset();
            opponent.Reset(); 

            oppHealth = opponent.healthbar.slider.value;

            // set position to (randX, 3.5, randZ)
            agent.move.setPos(new Vector3(Random.Range(b.min.x, b.max.x), 3.5f, Random.Range(b.min.z, b.max.z)));
            opponent.move.setPos(new Vector3(Random.Range(b.min.x, b.max.x), 3.5f, Random.Range(b.min.z, b.max.z)));
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 3 x, y, z variables
        // mirror me
        Vector3 diff = this.transform.position - opponent.transform.position;
        if(faceFactor == 1f){
            diff *= -1;
        }

        // what's the absolute farthest distance they can be away (normalize)
        sensor.AddObservation(diff.x/b.size.x);
        sensor.AddObservation(diff.z/b.size.z);

        // add observation of where I am (from the origin)
        Vector3 relativePos = (this.transform.position - b.center);
        // the left side for me, means the right side for the opponent
        if(faceFactor == 1f){
            relativePos *= -1;
        }
        sensor.AddObservation(relativePos.x/(b.size.x/2));
        sensor.AddObservation(relativePos.z/(b.size.z/2));

        int oppAction = 0; 
        
        if(!agent.CurrentMove().IsName("Idle")){
            oppAction = 1; 
        }  else if(!agent.CurrentBlock().IsName("Empty")){
            oppAction = 2; 
        } else if (!agent.CurrentReaction().IsName("Empty")){
            oppAction = 3; 
        }

        // discrete variable (so one-hot encode)
        sensor.AddOneHotObservation(oppAction, 4);

        // also add one energy observation (how much in energybar / total energy)
        Bar energy = agent.controller.energybar;
        sensor.AddObservation(energy.slider.value / energy.slider.maxValue);

    }

    void FixedUpdate(){
        steps++; 
    }

    // if wish, can add this as observation
    private bool closeToRope(){
        float x = transform.position.x; 
        float z = transform.position.z; 

        bool closeToRope = x <= b.min.x || 
                                x >= b.max.x;
        closeToRope = closeToRope || (z <= b.min.z ||
                                        z >= b.max.z);
        return closeToRope;
    }

    // tell me and my opponent to stop playing
    public void Finish(){
        oppBot.EndEpisode(); 
        EndEpisode(); 
    }

    // two actions: how we move, what move do we do
    // to train agent quick
    // - make revive prob 0f
    // - make death happen quicker (e.g. less health and no recovery)
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {   
        // each step is an action

        int walk = actionBuffers.DiscreteActions[0];
        int move = actionBuffers.DiscreteActions[1];

        // player movement for walking
        PlayerMovement movePlayer = agent.move; 

        // THIS is in their frame of reference (AI is FPS while our game is third-player)
        if(!agent.isDead()){
            if(walk == 1){
                movePlayer.moveHorz(0.1f);
            } else if (walk == 2){
                movePlayer.moveHorz(-0.1f);
            } else if (walk == 3){
                movePlayer.moveVert(0.1f);
            } else if (walk == 4){
                movePlayer.moveVert(-0.1f);
            }

            // player controller for move -> onKeyPress(move, Constants.energies[move])
            // move = 0 -> corresponds to do nothing
            PlayerController control = agent.controller; 
            if(move > 0 && control.IsIdle()){
                control.onKeyPress(Constants.allMoves[move-1], Constants.energies[move-1]);
            }
        }

        
        // reward shaping (if you do damage) to learn faster
        // fix bug: negatives sometimes happen when reset, so just max it out

        float damage = (oppHealth - opponent.healthbar.slider.value)/(opponent.healthbar.slider.maxValue);
        damage = Mathf.Max(0f, damage);
        // Debug.Log($"OppHealth Prev: {oppHealth:F3}, Curr: {opponent.healthbar.slider.value:F3}, RawDamage: {(oppHealth - opponent.healthbar.slider.value):F3}, Damage(norm): {damage:F3}");
        AddReward(damage);

        oppHealth = opponent.healthbar.slider.value; 

        bool isInference = GetComponent<BehaviorParameters>().BehaviorType == BehaviorType.InferenceOnly;

        if(!isInference){
            // gameover at same time, otherwise
            if(agent.isGameOver() && opponent.isGameOver()){
                Finish(); 
            } else if (agent.isGameOver()){
                AddReward(-1f);
                Finish(); 
            } else if (opponent.isGameOver()){
                // timestep penalty (how fast to beat them)
                AddReward(1f - steps/MaxStep);
                Finish(); 
            }
        }

    }

    






}