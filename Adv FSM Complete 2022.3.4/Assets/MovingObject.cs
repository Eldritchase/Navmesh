using System.Collections;
using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;

public class MovingObject : MonoBehaviour
{

    public float speed;
    public int startingPoint;
    public Transform[] points;
    private int i;




    // Start is called before the first frame update
    void Start()
    {
        
        i = startingPoint;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = Vector3.MoveTowards(transform.position, points[i].position, speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, points[i].position) < 0.02f)
        {
            i++;

            if(i == points.Length)
            {
                i = 0;
            }

            
        }

    
    }
}
