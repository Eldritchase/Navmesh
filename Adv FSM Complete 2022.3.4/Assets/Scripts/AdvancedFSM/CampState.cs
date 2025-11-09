using UnityEngine;
using System.Collections;

public class CampState : FSMState
{
    
    public Timer exitCampTimer; //cb
    public float campTime = 5f; //cb

    public CampState(Transform[] wp)
    {
        stateID = FSMStateID.Camping;

        curRotSpeed = 1.0f;
        curSpeed = 100.0f;
        
        waypoints = wp;
        //find next Waypoint position
    }

    public override void Reason(Transform player, Transform npc)
    {

        //cb

        NPCTankController controller = npc.GetComponent<NPCTankController>(); 
        float playerDistance = Vector3.Distance(npc.position, player.position);

        if (exitCampTimer == null)
        {
            exitCampTimer = Timer.create(npc.gameObject);
            exitCampTimer.startTimer(campTime);
        }

        if (exitCampTimer.isFinished())
        {
            Debug.Log("Switch to Patrol State");
            controller.SetTransition(Transition.Timeout);
        }

        if (playerDistance <= 300.0f)
        {
            //2. Since the distance is near, transition to chase state
            Debug.Log("Switch to Chase State");
            transitionOut(npc, Transition.SawPlayer);
        }
    }

    public override void Act(Transform player, Transform npc)
    
    {
        NPCTankController controller = npc.GetComponent<NPCTankController>();

        controller.setTankColor(Color.black); //cb
    }

    private void transitionOut(Transform npc, Transition exitTransition)
    {

        //rw
        if (npc.GetComponent<NPCTankController>().SetTransition(exitTransition))
        {
            GameObject.Destroy(exitCampTimer.gameObject); 
        }
    }
}