using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OffDutyState : FSMState
{
    
    
    public float offDutyTime = 10f; //cb - time in off duty
    public float vertHeight = 50.0f;

    public OffDutyState(Transform[] wp)
    {
        stateID = FSMStateID.OffDuty;

        curRotSpeed = 1.0f;
        curSpeed = 100.0f;
        
        waypoints = wp;
        //find next Waypoint position
    }

    public override void Reason(Transform player, Transform npc) //why exit state
    {

        //cb

        NPCTankController controller = npc.GetComponent<NPCTankController>(); 

        controller.canTakeDamage = true;
        controller.health = controller.maxHealth;
        controller.updateHealthText();
       
        
        Vector3 oldPos = npc.position;
        oldPos.y = 90f;
        npc.position = oldPos;

            
        GameManager.instance.tankReturned();
        Debug.Log("Switch to Patrol State");
        controller.SetTransition(Transition.Timeout);
        
    }

    public override void Act(Transform player, Transform npc)
    {
        NPCTankController controller = npc.GetComponent<NPCTankController>();   

        controller.setTankColor(Color.white); //cb

        controller.canTakeDamage = false; 

        npc.transform.Translate(Vector3.up * vertHeight, Space.World);
        
    }
}
