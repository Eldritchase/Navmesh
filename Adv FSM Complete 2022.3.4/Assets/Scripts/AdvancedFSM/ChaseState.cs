using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class ChaseState : FSMState
{

    private NavMeshPath path = new NavMeshPath();
    
    public ChaseState(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Chasing;

        curRotSpeed = 5.0f;
        curSpeed = 150.0f;

        //find next Waypoint position
        FindNextPoint();
    }

    public override void Reason(Transform player, Transform npc)
    {

        //Set the target position as the player position
        destPos = player.position;

        //Check the distance with player tank
        //When the distance is near, transition to attack state
        float dist = Vector3.Distance(npc.position, destPos);
        if (dist <= 200.0f)
        {
            Debug.Log("Switch to Attack state");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.ReachPlayer);
        }
        //Go back to patrol is it become too far
        else if (dist >= 600.0f)
        {
            Debug.Log("Switch to Patrol state");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.LostPlayer);
        }
    }

    public override void Act(Transform player, Transform npc)
    {
        NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
        agent.destination = player.position;

        NPCTankController controller = npc.GetComponent<NPCTankController>();
        //Rotate to the target point

        Quaternion targetRotation = Quaternion.LookRotation(destPos - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        controller.setTankColor(Color.yellow);
        controller.ShootBullet();
    }

}
