using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class RestState : FSMState
{
    
    public float restHealRate = 30f;
    public RestState(Transform[] wp)
    {
        stateID = FSMStateID.Resting;

        curRotSpeed = 1.0f;
        curSpeed = 100.0f;
       

        waypoints = wp;
    }

    public override void Reason(Transform player, Transform npc)
    {
        NPCTankController controller = npc.GetComponent<NPCTankController>();   
        NavMeshAgent agent = controller.GetComponent<NavMeshAgent>();
        Vector3 restPos = controller.restPoint.transform.position;
        if (controller.health >= controller.maxHealth)
        {
            Debug.Log("Exiting rest state");
            controller.health = controller.maxHealth;
            controller.canTakeDamage = true;
            controller.SetTransition(Transition.Healed);
            controller.GetComponent<NavMeshAgent>().destination = Vector3.zero;
        }
        else if (Vector3.Distance(agent.destination, restPos) > 120f)
        {
            //Debug.Log("going to rest point " + restPos);
            agent.SetDestination(restPos);
        }
    }

    public override void Act(Transform player, Transform npc)
    {
        NPCTankController controller = npc.GetComponent<NPCTankController>();
        Vector3 restPos = controller.restPoint.transform.position;
        //Debug.Log("Agent Destination: " + agent.destination);
        //Debug.Log("Rest point position: " + restPos);
        //Debug.Log("Distance to rest location: " + Vector3.Distance(agent.destination, restPos));

        float dist = Vector3.Distance(npc.transform.position, restPos);
        float moveCloserDistance = 200f;

        // This entire block is unnecessary
        //if (dist > moveCloserDistance)
        //{
        //    //Rotate to the target point
        //    Quaternion targetRotation = Quaternion.LookRotation(restPos - npc.transform.position);
        //    npc.transform.rotation = Quaternion.Slerp(npc.transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //    //Go Forward
        //    npc.transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
        //}

        if (dist <= moveCloserDistance)
        {
            //heal tank
            controller.canTakeDamage = false;
            controller.health += restHealRate * Time.deltaTime;
            controller.updateHealthText();
        }

        //tank cannot take damage
        //set pseudo timer
        
        controller.setTankColor(Color.green);
    }
}