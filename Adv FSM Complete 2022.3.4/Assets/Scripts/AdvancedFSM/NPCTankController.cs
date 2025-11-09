using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;
using TMPro;


public class NPCTankController : AdvancedFSM
{
    public GameObject Bullet;

    public MeshRenderer tankMesh;
    public float health;
    public int maxHealth;

    // We overwrite the deprecated built-in `rigidbody` variable.
    new private Rigidbody rigidbody;

    //Damage boolean
    public bool canTakeDamage = true;

    [SerializeField]
    public GameObject restPoint;

    private const float PHYSICS_SCALE = 10f;

    [SerializeField]
    private float textXRotation = 75f;

    //Health Text
    [SerializeField]
    public TMP_Text healthText;

    //OffDuty Timer  - Depricated
    //public Timer enterOffDutyTimer; //cb - creates a timer to enter game
    //public float enterOffDutyTime;
    

    //Initialize the Finite state machine for the NPC tank
    protected override void Initialize()
    {
        health = 150.0f;
        maxHealth = 150;
        elapsedTime = 0.0f;
        shootRate = 2.0f;
        //enterOffDutyTime = 5f; //Random.Range(30f, 90f); //cb - time in off duty

        //Get the target enemy(Player)
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;

        //Get the rigidbody
        rigidbody = GetComponent<Rigidbody>();

        if (!playerTransform)
            print("Player doesn't exist.. Please add one with Tag named 'Player'");

        //Get the turret of the tank
        turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = turret.GetChild(0).transform;

  

        //if (enterOffDutyTimer == null)
        //{
        //    enterOffDutyTimer = Timer.create(gameObject);
        //    enterOffDutyTimer.startTimer(enterOffDutyTime);
        //}

        //Start Doing the Finite State Machine
        ConstructFSM();

    }

    //Update each frame
    protected override void FSMUpdate()
    {
        //Check for health
        elapsedTime += Time.deltaTime;

        // get health text
        updateHealthText();

        // update the text rotation so the player can see it, regardless of state
        updateTextRotation();

        // Depricated
        //if (enterOffDutyTimer.isFinished() && CurrentStateID == FSMStateID.Patrolling)
        //{
        //    Debug.Log("Switch to OffDuty State");
        //    SetTransition(Transition.GoToOffDuty);
        //    GameObject.Destroy(enterOffDutyTimer); 
        //}
    }

    protected override void FSMFixedUpdate()
    {
        CurrentState.Reason(playerTransform, transform);
        CurrentState.Act(playerTransform, transform);
    }

    public bool SetTransition(Transition t)
    {
        return PerformTransition(t);
    }

    private void ConstructFSM()
    {
        //Get the list of points
        pointList = GameObject.FindGameObjectsWithTag("WandarPoint");

        Transform[] waypoints = new Transform[pointList.Length];
        int i = 0;
        foreach (GameObject obj in pointList)
        {
            waypoints[i] = obj.transform;
            i++;
        }

        PatrolState patrol = new PatrolState(waypoints);
        patrol.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        patrol.AddTransition(Transition.Bored, FSMStateID.Dancing);
        patrol.AddTransition(Transition.BoredSensePlayer, FSMStateID.Camping);
        patrol.AddTransition(Transition.Damaged, FSMStateID.Resting);
        patrol.AddTransition(Transition.GoToOffDuty, FSMStateID.OffDuty);
        patrol.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        ChaseState chase = new ChaseState(waypoints);
        chase.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        chase.AddTransition(Transition.ReachPlayer, FSMStateID.Attacking);
        chase.AddTransition(Transition.Damaged, FSMStateID.Resting);
        chase.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        AttackState attack = new AttackState(waypoints);
        attack.AddTransition(Transition.LostPlayer, FSMStateID.Patrolling);
        attack.AddTransition(Transition.SawPlayer, FSMStateID.Chasing);
        attack.AddTransition(Transition.Damaged, FSMStateID.Resting);
        attack.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        DanceState dance = new DanceState(waypoints);
        dance.AddTransition(Transition.Damaged, FSMStateID.Resting);
        dance.AddTransition(Transition.Timeout, FSMStateID.Patrolling);
        dance.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        CampState camp = new CampState(waypoints);
        camp.AddTransition(Transition.Damaged, FSMStateID.Resting);
        camp.AddTransition(Transition.Timeout, FSMStateID.Patrolling);
        camp.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        RestState rest = new RestState(waypoints);
        rest.AddTransition(Transition.NoHealth, FSMStateID.Dead);
        rest.AddTransition(Transition.Healed, FSMStateID.Patrolling);

        OffDutyState offDuty = new OffDutyState(waypoints);
        offDuty.AddTransition(Transition.Timeout, FSMStateID.Patrolling);
        offDuty.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        DeadState dead = new DeadState();
        dead.AddTransition(Transition.NoHealth, FSMStateID.Dead);

        AddFSMState(patrol);
        AddFSMState(chase);
        //AddFSMState(attack);
        AddFSMState(dance);
        AddFSMState(camp);
        AddFSMState(rest);
        AddFSMState(offDuty);
        AddFSMState(dead);
    }

    /// <summary>
    /// Check the collision with the bullet
    /// </summary>
    /// <param name="collision"></param>
    void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if (collision.gameObject.tag == "Bullet" && canTakeDamage)
        {
            health -= 50;

            if (health <= 50 && health > 0)
            {
                Debug.Log("Switch to Rest State");
                SetTransition(Transition.Damaged);
            }

            if (health <= 0)
            {
                Debug.Log("Switch to Dead State");
                SetTransition(Transition.NoHealth);
                GameManager.instance.tankDied();
                Explode();
            }

            updateHealthText();
        }
    }

    public void setVelocity(Vector3 velocity)
    {
        float mag = velocity.magnitude;
        float prevY = rigidbody.velocity.y;
        Vector3 normalizedVelocity = velocity.normalized * mag * PHYSICS_SCALE;
        normalizedVelocity.y = prevY;
        rigidbody.velocity = normalizedVelocity;
    }

    protected void Explode()
    {
        float rndX = Random.Range(10.0f, 30.0f);
        float rndZ = Random.Range(10.0f, 30.0f);
        for (int i = 0; i < 3; i++)
        {
            rigidbody.AddExplosionForce(10000.0f, transform.position - new Vector3(rndX, 10.0f, rndZ), 40.0f, 10.0f);
            rigidbody.velocity = transform.TransformDirection(new Vector3(rndX, 20.0f, rndZ));
        }

        Destroy(gameObject, 1.5f);
    }

    /// <summary>
    /// Shoot the bullet from the turret
    /// </summary>
    public void ShootBullet()
    {
        if (elapsedTime >= shootRate)
        {
            Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            elapsedTime = 0.0f;
        }
    }

    public void updateHealthText()
    {
        healthText.text = Mathf.Floor(health).ToString();
    }

    public void updateTextRotation()
    {
        healthText.transform.rotation = Quaternion.Euler(healthText.transform.rotation.x + textXRotation, (gameObject.transform.rotation.y * -1.0f) + 180f, healthText.transform.rotation.z);
    }
    
     public void setTankColor(Color color)
    {
        if (tankMesh != null)
        {
            tankMesh.material.color = color;
        }
    }
}
