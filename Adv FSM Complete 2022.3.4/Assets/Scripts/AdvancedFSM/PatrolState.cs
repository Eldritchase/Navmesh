using UnityEngine;
using System.Collections;

public class PatrolState : FSMState
{

    
    private float timeUntilDanceCampState;
    private float offDutyElapsedTime;
    private int timesEnteredOffDuty = 0;

    public PatrolState(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Patrolling;

        curRotSpeed = 1.0f;
        curSpeed = 100.0f;

        // sets up for reason and act states
        FindNextPoint();
        timeUntilDanceCampState = Random.Range(5f, 15f);
        offDutyElapsedTime = Random.Range(30f, 90f);
    }

    public override void Reason(Transform player, Transform npc)
    {
        NPCTankController controller = npc.GetComponent<NPCTankController>();

        // RW
        

        float playerDistance = Vector3.Distance(npc.position, player.position);

        // RW                           if the elapsed time is greater than the time for us to go offduty (and how much we have)
        if (GameManager.tanksRemaining > 1 && controller.elapsedTime >= (timesEnteredOffDuty+1) * offDutyElapsedTime)
        {
            Debug.Log("Transitioning to OffDuty");
            GameManager.instance.tankDied();
            timesEnteredOffDuty += 1;
            npc.GetComponent<NPCTankController>().SetTransition(Transition.GoToOffDuty);
            return;
        }
        

        //1. Check the distance with player tank
        if (playerDistance <= 300.0f)
        {
            //2. Since the distance is near, transition to chase state
            Debug.Log("Switch to Chase State");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
        }

    }

    public override void Act(Transform player, Transform npc)
    {
        //1. Find another random patrol point if the current point is reached
        if (Vector3.Distance(npc.position, destPos) <= 100.0f)
        {
            Debug.Log("Reached to the destination point, calculating the next point");
            FindNextPoint();
        }

        //2. Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //3. Go Forward
        npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        npc.GetComponent<NPCTankController>().setTankColor(Color.blue);
    }

    // RW
    
}