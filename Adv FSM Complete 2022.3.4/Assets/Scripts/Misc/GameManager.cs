using UnityEngine;
using System.Collections;
using TMPro;

public class GameManager : MonoBehaviour 
{
    public float gameSpeed = 1.0f;

	private int itemsCollected = 0;
	private int totalItems = 0;
    public GameObject links;

    [SerializeField]
	private EndScreenCanvas endCanvas;
	
	public static GameManager instance {  get; private set; }

    public static int tanksRemaining { get ; private set; }

    // RW
    private void Awake()
    {
        if (FindObjectsByType<GameManager>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }


    // Use this for initialization
    void Start () 
    {
	    //Set the gravity Settings
        Physics.gravity = new Vector3(0, -500.0f, 0);

		totalItems = GameObject.FindGameObjectsWithTag("CatItem").Length;
        tanksRemaining = FindObjectsByType<NPCTankController>(FindObjectsSortMode.None).Length;
		toggleEndCanvas(false, true);
        endCanvas.showObjectiveForTime(3f);
	}
	
	// Update is called once per frame
	void Update () 
    {
        Time.timeScale = gameSpeed;
        // IY                           When there is only one tank left, turn on the shortcuts in the center. (i think this is the best place to put this?)
        if (tanksRemaining == 1)
        {
            Debug.Log("Turning on shortcuts");
            links.SetActive(true);
            return;
        }
    }

    // RW
    public void addCollectedItem()
	{
		itemsCollected++;
		if (itemsCollected == totalItems) {
			toggleEndCanvas(true, true);
		}
	}

    // RW
    public void playerCaptured()
	{
		toggleEndCanvas(true, false);
	}

    // RW
    private void toggleEndCanvas(bool shown, bool win)
	{
        if (endCanvas)
        {
			endCanvas.showEndScreen(win);
			endCanvas.toggleEndScreen(shown);
        }
    }

    public void tankReturned()
    {
        tanksRemaining++;
    }

    // RW
    public void tankDied()
    {
        tanksRemaining--;
    }
}
