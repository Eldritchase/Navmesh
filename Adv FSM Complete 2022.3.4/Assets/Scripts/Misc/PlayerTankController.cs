using UnityEngine;
using System.Collections;

public class PlayerTankController : MonoBehaviour
{
    public GameObject Bullet;
	
    private Transform Turret;
    private Transform bulletSpawnPoint;    
    private float curForwardSpeed, forwardSpeed, curRightSpeed, rightSpeed;
    private float turretRotSpeed = 10.0f;
    [SerializeField]
    private float regularSpeed = 300f;
    private float currentSpeed = 300.0f;

    // slowing on hit
    [SerializeField]
    private float slowedSpeedPercent = .85f;
    [SerializeField]
    private float slowTime = 2f;

    [SerializeField]
    private GameObject jailLocation;
    [SerializeField]
    private string enemyTagName = "EnemyTank";



    //Bullet shooting rate
    protected float shootRate;
    protected float elapsedTime;

    //health
    public float health = 150.0f;

    private Rigidbody playerBody;

    void Start()
    {
        //Tank Settings
        currentSpeed = regularSpeed;

        //Get the turret of the tank
        Turret = gameObject.transform.GetChild(0).transform;
        bulletSpawnPoint = Turret.GetChild(0).transform;
        playerBody = GetComponent<Rigidbody>();
    }

    void OnEndGame()
    {
        // Don't allow any more control changes when the game ends
        this.enabled = false;
    }

    void Update()
    {
        UpdateControl();
        UpdateWeapon();
    }

    void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if(collision.gameObject.tag == "Bullet"){
            slowSelf();
        }
        if (collision.gameObject.tag == enemyTagName) {
            jailSelf();
        }
        
        if(health <= 0){
            Destroy(gameObject);
        }
    }

    // RW
    private void jailSelf()
    {
        if (jailLocation != null)
        {
            transform.position = jailLocation.transform.position;
            GameManager.instance.playerCaptured();
        }
    }

    // RW
    private void slowSelf()
    {
        

        currentSpeed = regularSpeed * slowedSpeedPercent;
    }

    private void speedBackUp()
    {
        print("Speeding back up");
        currentSpeed = regularSpeed;
    }
    
    void UpdateControl()
    {
        //AIMING WITH THE MOUSE
        // Generate a plane that intersects the transform's position with an upwards normal.
        Plane playerPlane = new Plane(Vector3.up, transform.position + new Vector3(0, 0, 0));

        // Generate a ray from the cursor position
        Ray RayCast = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Determine the point where the cursor ray intersects the plane.
        float HitDist = 0;

        // If the ray is parallel to the plane, Raycast will return false.
        if (playerPlane.Raycast(RayCast, out HitDist))
        {
            // Get the point along the ray that hits the calculated distance.
            Vector3 RayHitPoint = RayCast.GetPoint(HitDist);

            Quaternion targetRotation = Quaternion.LookRotation(RayHitPoint - transform.position);
            Turret.transform.rotation = Quaternion.Slerp(Turret.transform.rotation, targetRotation, Time.deltaTime * turretRotSpeed);
        }

        // RW

        if (Input.GetKey(KeyCode.W))
        {
            forwardSpeed = -currentSpeed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            forwardSpeed = currentSpeed;
        }
        else
        {
            forwardSpeed = 0;
        }

        if (Input.GetKey(KeyCode.D))
        {
            rightSpeed = -currentSpeed;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            rightSpeed = currentSpeed;
        }
        else
        {
            rightSpeed = 0;
        }

        //if (Input.GetKey(KeyCode.A))
        //{
        //    transform.Rotate(0, -rotSpeed * Time.deltaTime, 0.0f);
        //}
        //else if (Input.GetKey(KeyCode.D))
        //{
        //    transform.Rotate(0, rotSpeed * Time.deltaTime, 0.0f);
        //}

        //Determine current speed
        curForwardSpeed = Mathf.Lerp(curForwardSpeed, forwardSpeed, 7.0f * Time.deltaTime);
        curRightSpeed = Mathf.Lerp(curRightSpeed, rightSpeed, 7.0f * Time.deltaTime);
        Vector3 translation = Vector3.forward * curForwardSpeed + Vector3.right * curRightSpeed;
        //transform.Translate(translation * Time.deltaTime);
        setVelocity(translation);
    }

    // RW
    private void setVelocity(Vector3 velocity)
    {
        float mag = velocity.magnitude;
        playerBody.velocity = velocity.normalized * mag;
    }

    void UpdateWeapon()
    {
        if(Input.GetMouseButtonDown(0))
        {
            if (elapsedTime >= shootRate)
            {
                //Reset the time
                elapsedTime = 0.0f;

                //Also Instantiate over the PhotonNetwork
                Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            }
        }
    }
}