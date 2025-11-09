using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using TMPro;

public class SimpleFSM : FSM 
{
    public enum FSMState
    {
        None,
        Patrol,
        Chase,
        Attack,
        Dance, // dance state for assignment
        Camp,
        Rest,
        Dead,
    }

    //Current state that the NPC is reaching
    public FSMState curState;

    //Speed of the tank
    private float curSpeed;

    //Tank Rotation Speed
    private float curRotSpeed;

    //Bullet
    public GameObject Bullet;

    //Damage boolean
    private bool canTakeDamage = true;

    //Health Text
    public TMP_Text healthText;

    //Whether the NPC is destroyed or not
    private bool bDead;
    private float health;
    private int maxHealth;

    // The timer for entering and exiting the dance state (Custom Class)
    private Timer enterDanceTimer;
    private Timer exitDanceTimer;
    private float idleUntilDanceTimeMinimum = 5f;
    private float idleUntilDanceTimeMaximum = 15f;
    private float danceTime = 5f;

    private Timer exitCampTimer;
    private float campTime = 5f;

    private float restHealRate = 30f;

    [SerializeField]
    private GameObject restPoint;

    private MeshRenderer tankMesh;

    //Initialize the Finite state machine for the NPC tank
	protected override void Initialize () 
    {
        curState = FSMState.Patrol;
        curSpeed = 150.0f;
        curRotSpeed = 2.0f;
        bDead = false;
        elapsedTime = 0.0f;
        shootRate = 3.0f;
        maxHealth = 150;
        health = maxHealth;

        //Get the list of points
        pointList = GameObject.FindGameObjectsWithTag("WandarPoint");
        
        GameObject restPoint = GameObject.FindGameObjectWithTag("RestPoint");
        print(restPoint);

        // get the tank mesh
        tankMesh = GetComponent<MeshRenderer>();

        // get health text
        updateHealthText();

        //Set Random destination point first
        FindNextPoint();

        //Get the target enemy(Player)
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;

        if(!playerTransform)
            print("Player doesn't exist.. Please add one with Tag named 'Player'");

        //Get the turret of the tank
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;
	}

    //Update each frame
    protected override void FSMUpdate()
    {
        switch (curState)
        {
            case FSMState.Patrol: UpdatePatrolState(); break;
            case FSMState.Chase: UpdateChaseState(); break;
            case FSMState.Attack: UpdateAttackState(); break;
            case FSMState.Dance: UpdateDanceState(); break;
            case FSMState.Dead: UpdateDeadState(); break;
            case FSMState.Rest: UpdateRestState(); break;
            case FSMState.Camp: UpdateCampState(); break;
        }

        //Update the time
        elapsedTime += Time.deltaTime;

        // update the text rotation so the player can see it, regardless of state
        updateTextRotation();

        if (health <= 50)
        {
            destroyTimers();
            curState = FSMState.Rest;
        }

        //Go to dead state is no health left
        if (health <= 0)
            curState = FSMState.Dead;
    }

    /// <summary>
    /// Patrol state
    /// </summary>
    protected void UpdatePatrolState()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        if (enterDanceTimer == null)
        {
            enterDanceTimer = Timer.create(gameObject);
            enterDanceTimer.startTimer(Random.Range(idleUntilDanceTimeMinimum, idleUntilDanceTimeMaximum));
        }
        else if (enterDanceTimer.isFinished())
        {
            if (distanceToPlayer >= 300f && distanceToPlayer <= 500f)
            {
                curState = FSMState.Camp;
            }
            else
            {
                Destroy(enterDanceTimer);
                enterDanceTimer = null;
                print("Switching from Idle to Dance state");
                curState = FSMState.Dance;
            }
        }

        //Find another random patrol point if the current point is reached
        if (Vector3.Distance(transform.position, destPos) <= 100.0f)
        {
            print("Reached to the destination point\ncalculating the next point");
            FindNextPoint();
        }
        //Check the distance with player tank
        //When the distance is near, transition to chase state
        else if (distanceToPlayer <= 300.0f)
        {
            print("Switch to Chase Position");
            Destroy(enterDanceTimer);
            enterDanceTimer = null;
            curState = FSMState.Chase;
        }

        setTankColor(Color.blue);

        //Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
        
    }

    /// <summary>
    /// Chase state
    /// </summary>
    protected void UpdateChaseState()
    {
        //Set the target position as the player position
        destPos = playerTransform.position;

        //Check the distance with player tank
        //When the distance is near, transition to attack state
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist <= 200.0f)
        {
            curState = FSMState.Attack;
        }
        //Go back to patrol is it become too far
        else if (dist >= 300.0f)
        {
            curState = FSMState.Patrol;
        }

        setTankColor(Color.yellow);

        //Go Forward
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    /// <summary>
    /// Attack state
    /// </summary>
    protected void UpdateAttackState()
    {
        //Set the target position as the player position
        destPos = playerTransform.position;

        //Check the distance with the player tank
        float dist = Vector3.Distance(transform.position, playerTransform.position);
        if (dist >= 200.0f && dist < 300.0f)
        {
            //Rotate to the target point
            Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);  

            //Go Forward
            transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);

            curState = FSMState.Attack;
        }
        //Transition to patrol is the tank become too far
        else if (dist >= 300.0f)
        {
            curState = FSMState.Patrol;
        }

        setTankColor(Color.red);

        //Always Turn the turret towards the player
        Quaternion turretRotation = Quaternion.LookRotation(destPos - turret.position);
        turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * curRotSpeed); 

        //Shoot the bullets
        ShootBullet();
    }

    /// <summary>
    /// Dance state
    /// </summary>
    protected void UpdateDanceState()
    {
        if (exitDanceTimer == null)
        {
            exitDanceTimer = Timer.create(gameObject);
            exitDanceTimer.startTimer(danceTime);
        }
        else if (exitDanceTimer.isFinished())
        {
            destroyTimers();
            print("Switching from Dance to Patrol state");
            curState = FSMState.Patrol;
            return;
        }
        
        if (Vector3.Distance(transform.position, playerTransform.position) <= 300.0f)
        {
            destroyTimers();
            print("Switch to Chase Position");
            curState = FSMState.Chase;
            return;
        }

        setTankColor(Color.magenta);

        // TODO dance code
        if (exitDanceTimer.isFinished() == false){
            turret.transform.Rotate(0,90*Time.deltaTime,0);
            //transform.Rotate(0,90*Time.deltaTime,0); // if you want the tank to rotate too
        }

    }

    /// <summary>
    /// Camp state
    /// </summary>
    protected void UpdateCampState()
    {
        if (exitCampTimer == null)
        {
            exitCampTimer = Timer.create(gameObject);
            exitCampTimer.startTimer(campTime);
        }
        else if (exitCampTimer.isFinished())
        {
            destroyTimers();
            print("Switching from Camp to Patrol state");
            curState = FSMState.Patrol;
            return;
        }

        if (Vector3.Distance(transform.position, playerTransform.position) <= 300.0f)
        {
            destroyTimers();
            print("Switch to Chase Position");
            curState = FSMState.Chase;
            return;
        }

        setTankColor(Color.black);
    }

    private void destroyTimers()
    {
        if (exitCampTimer != null)
        {
            Destroy(exitCampTimer);
            exitCampTimer = null;
        }
        if (exitDanceTimer != null)
        {
            Destroy(exitDanceTimer);
            exitDanceTimer = null;
        }
    }

    /// <summary>
    /// Rest state
    /// </summary>
    protected void UpdateRestState()
    {

        Vector3 restPos = restPoint.transform.position;
        float dist = Vector3.Distance(transform.position, restPos);
        float moveCloserDistance = 40f;

        if (dist > moveCloserDistance)
        {
            //Rotate to the target point
            Quaternion targetRotation = Quaternion.LookRotation(restPos - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);  

            //Go Forward
            transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
        }

        if (dist <= moveCloserDistance)
        {
            //heal tank
            canTakeDamage = false;
            health += restHealRate * Time.deltaTime;
            updateHealthText();
        }

        //tank cannot take damage
       
        //set pseudo timer
        

        setTankColor(Color.green);

        if (health >= maxHealth)
        {
            health = maxHealth;
            canTakeDamage = true;
            curState = FSMState.Patrol;
        }
    }

    /// <summary>
    /// Dead state
    /// </summary>
    protected void UpdateDeadState()
    {
        //Show the dead animation with some physics effects
        if (!bDead)
        {
            bDead = true;
            Explode();
        }
    }

    private void setTankColor(Color color)
    {
        if (tankMesh != null)
        {
            tankMesh.material.color = color;
        }
    }

    /// <summary>
    /// Shoot the bullet from the turret
    /// </summary>
    private void ShootBullet()
    {
        if (elapsedTime >= shootRate)
        {
            //Shoot the bullet
            Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            elapsedTime = 0.0f;
        }
    }

    /// <summary>
    /// Check the collision with the bullet
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if(collision.gameObject.tag == "Bullet" && canTakeDamage){
            health -= collision.gameObject.GetComponent<Bullet>().damage;
            updateHealthText();

        }

    }   

    private void updateTextRotation()
    {
        healthText.transform.rotation = Quaternion.Euler(healthText.transform.rotation.x, (gameObject.transform.rotation.y * -1.0f) + 180f, 0.0f);
    }

    /// <summary>
    /// Find the next semi-random patrol point
    /// </summary>
    protected void FindNextPoint()
    {
        print("Finding next point");
        int rndIndex = Random.Range(0, pointList.Length);
        float rndRadius = 10.0f;
        
        Vector3 rndPosition = Vector3.zero;
        destPos = pointList[rndIndex].transform.position + rndPosition;

        //Check Range
        //Prevent to decide the random point as the same as before
        if (IsInCurrentRange(destPos))
        {
            rndPosition = new Vector3(Random.Range(-rndRadius, rndRadius), 0.0f, Random.Range(-rndRadius, rndRadius));
            destPos = pointList[rndIndex].transform.position + rndPosition;
        }
    }

    /// <summary>
    /// Check whether the next random position is the same as current tank position
    /// </summary>
    /// <param name="pos">position to check</param>
    protected bool IsInCurrentRange(Vector3 pos)
    {
        float xPos = Mathf.Abs(pos.x - transform.position.x);
        float zPos = Mathf.Abs(pos.z - transform.position.z);

        if (xPos <= 50 && zPos <= 50)
            return true;

        return false;
    }

    protected void Explode()
    {
        float rndX = Random.Range(10.0f, 30.0f);
        float rndZ = Random.Range(10.0f, 30.0f);
        for (int i = 0; i < 3; i++)
        {
            GetComponent<Rigidbody>().AddExplosionForce(10000.0f, transform.position - new Vector3(rndX, 10.0f, rndZ), 40.0f, 10.0f);
            GetComponent<Rigidbody>().velocity = transform.TransformDirection(new Vector3(rndX, 20.0f, rndZ));
        }

        Destroy(gameObject, 1.5f);
    }

    protected void updateHealthText()
    {
        healthText.text = Mathf.Floor(health).ToString();
    }

}
