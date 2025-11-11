using UnityEngine;
using System.Collections;
using System.IO;
using UnityEngine.AI;

public class PatrolState : FSMState
{

    private NavMeshPath path = new NavMeshPath();

    private float timeUntilDanceCampState;
    private float offDutyElapsedTime;
    private int timesEnteredOffDuty = 0;
    private Vector3 nextDestination = Vector3.zero;

    public PatrolState(Transform[] wp)
    {
        waypoints = wp;
        stateID = FSMStateID.Patrolling;

        curRotSpeed = 1.0f;
        curSpeed = 100.0f;

        // sets up for reason and act states
        //FindNextPoint();
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
            nextDestination = Vector3.zero;
            return;
        }


        //1. Check the distance with player tank
        if (playerDistance <= 300.0f)
        {
            //2. Since the distance is near, transition to chase state
            Debug.Log("Switch to Chase State");
            npc.GetComponent<NPCTankController>().SetTransition(Transition.SawPlayer);
            nextDestination = Vector3.zero;
        }

    }

    public override void Act(Transform player, Transform npc)
    {
        //Debug.Log(destPos);
        //1. Find another random patrol point if the current point is reached
        if (nextDestination == Vector3.zero || Vector3.Distance(npc.position, nextDestination) <= 200.0f)//(pathWaypoints == null || pathWaypoints.Length < 1) ||  || pathWaypoints.Length - 3 < currentPathIndex)
        {
            NavMeshAgent agent = npc.GetComponent<NavMeshAgent>();
            Debug.Log("Reached to the destination point, calculating the next point");
            recalculatePath(agent, getRandomNextPositionOnMesh(npc));
        }

        //Debug.Log(pathWaypoints.Length);
        //var nextPoint = pathWaypoints[currentPathIndex];

        //2. Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(nextDestination - npc.position);
        npc.rotation = Quaternion.Slerp(npc.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //3. Go Forward
        //npc.Translate(Vector3.forward * Time.deltaTime * curSpeed);

        npc.GetComponent<NPCTankController>().setTankColor(Color.blue);
        //if (Vector3.Distance(npc.position, nextPoint) <= 200.0f)
        //{
        //    currentPathIndex++;
        //}
    }

    // RW
    private void recalculatePath(NavMeshAgent agent, Vector3 targetPosition)
    {
        nextDestination = targetPosition;
        agent.destination = targetPosition;
        //path = new NavMeshPath();
        //currentPathIndex = 0;
        //agent.CalculatePath(targetPosition, path);
        //pathWaypoints = path.corners;
        //Debug.Log("calculated new path: " + pathWaypoints.Length);
    }

    // RW - Thank you to this post for this code and understanding https://discussions.unity.com/t/how-to-get-a-random-point-on-navmesh/73440
    private Vector3 getRandomNextPositionOnMesh(Transform tank)
    {
        Vector3 randomDirection = Random.insideUnitCircle * 900f;
        randomDirection += tank.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, 900f, 1);
        return hit.position;
    }

}