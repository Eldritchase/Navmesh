using UnityEngine;
using UnityEngine.Events;

// RW
/// <summary>
/// A class that acts as a timer. Must be instantiated into the heirarchy in order to work.
/// </summary>
public class Timer : MonoBehaviour
{
    public UnityEvent timeout = new UnityEvent();
    public float timeLeft { get; private set; } = 0f;
    public float timerLength { get; private set; } = 0f;
    /// <summary>
    /// Whether this timer is affected by the game being paused. Defaults to true.
    /// </summary>
    public bool affectedByGamePause = true;
    /// <summary>
    /// If this timer runs once and then stops, or if false will automatically restart. Defaults to true.
    /// </summary>
    public bool oneShot = true;
    /// <summary>
    /// If true, pauses this timer. Defaults to false.
    /// </summary>
    public bool paused = false;
    // If the timer is running
    private bool running = false;
    public bool countsUp = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (isRunning())
        {
            timeLeft -= Time.deltaTime;
            checkTime();
        }
    }

    /// <summary>
    /// Checks the time of the timer.
    /// </summary>
    private void checkTime()
    {
        if (isFinished())
        {
            timeout.Invoke();
            running = false;

            if (!oneShot)
            {
                startTimer(timerLength);
            }
        }
    }

    public bool isFinished()
    {
        return timeLeft <= 0;
    }

    /// <summary>
    /// Starts the timer at the given time. Does not override being paused.
    /// </summary>
    /// <param name="length">The amount of time this timer runs for.</param>
    public void startTimer(float length)
    {
        timerLength = length;
        running = true;
        timeLeft = timerLength;
    }

    /// <summary>
    /// Stops the timer.
    /// </summary>
    public void stopTimer()
    {
        running = false;
    }

    /// <summary>
    /// Whether the timer is running or not.
    /// </summary>
    /// <returns>If the timer is running.</returns>
    public bool isRunning()
    {
        return (!affectedByGamePause || true) && !paused && running && !isFinished();
    }

    /// <summary>
    /// Creates a new Timer gameobject and returns it. In order to run, it must be in the tree.
    /// </summary>
    /// <returns>A new Timer Gameobject.</returns>
    public static Timer create()
    {
        GameObject newObject = new GameObject();
        Timer newTimer = newObject.AddComponent<Timer>();
        return newTimer;
    }

    /// <summary>
    /// Creates a new Timer gameobject and returns it. Will be child-ed to the inputted parent object.
    /// </summary>
    /// <param name="parent">The object the new timer will be parented to.</param>
    /// <returns>A new Timer Gameobject.</returns>
    public static Timer create(GameObject parent)
    {
        Timer newTimer = create();
        newTimer.transform.parent = parent.transform;
        return newTimer;
    }

    /// <summary>
    /// Creates a new Timer gameobject and returns it. In order to run, it must be in the tree.
    /// </summary>
    /// <param name="objectName">The name of the new object.</param>
    /// <returns>A new Timer Gameobject.</returns>
    public static Timer create(string objectName)
    {
        Timer newTimer = create();
        newTimer.gameObject.name = objectName;
        return newTimer;
    }

    /// <summary>
    /// Creates a new Timer gameobject and returns it. Will be child-ed to the inputted parent object.
    /// </summary>
    /// <param name="parent">The object the new timer will be parented to.</param>
    /// <param name="objectName">The name of the new object.</param>
    /// <returns>A new Timer Gameobject.</returns>
    public static Timer create(GameObject parent, string objectName)
    {
        Timer newTimer = create(parent);
        newTimer.gameObject.name = objectName;
        return newTimer;
    }

    public void OnDestroy()
    {
        timeout.RemoveAllListeners();
    }

    /// <summary>
    /// Destroys this timer and handles listeners.
    /// </summary>
    public void killTimer()
    {
        Destroy(gameObject);
    }

}
