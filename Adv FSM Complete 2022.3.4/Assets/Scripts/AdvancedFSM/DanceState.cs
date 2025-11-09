using UnityEngine;
using System.Collections;

public class DanceState : FSMState
{
    public Timer exitDanceTimer;
    float danceTime = 5f;

    public DanceState(Transform[] wp)
    {
        stateID = FSMStateID.Dancing;


        curRotSpeed = 1.0f;
        curSpeed = 100.0f;

        waypoints = wp;

    }

    public override void Reason(Transform player, Transform npc)
    {
        NPCTankController controller = npc.GetComponent<NPCTankController>(); 

        float playerDistance = Vector3.Distance(npc.position, player.position);


        if (exitDanceTimer == null)
        {
            exitDanceTimer = Timer.create(npc.gameObject);
            exitDanceTimer.startTimer(danceTime);
        }
        
        else if (exitDanceTimer.isFinished())
        {
            Debug.Log("Switch to Patrol State");
            controller.SetTransition(Transition.Timeout);
            GameObject.Destroy(exitDanceTimer.gameObject);
        }

        if (playerDistance <= 300.0f)
        {
            //2. Since the distance is near, transition to chase state
            Debug.Log("Switch to Chase State");
            transitionOut(npc, Transition.SawPlayer);
            GameObject.Destroy(exitDanceTimer.gameObject);
        }
    }

    public override void Act(Transform player, Transform npc)
    {
        NPCTankController controller = npc.GetComponent<NPCTankController>();

        
        controller.transform.Rotate(0,90*Time.deltaTime,0);
       
        controller.setTankColor(Color.magenta);
    }

    private void transitionOut(Transform npc, Transition exitTransition)
    {
        if (npc.GetComponent<NPCTankController>().SetTransition(exitTransition))
        {
            GameObject.Destroy(exitDanceTimer);
        }
    }
}