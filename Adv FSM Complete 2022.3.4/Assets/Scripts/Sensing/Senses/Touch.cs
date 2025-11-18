using UnityEngine;
using System.Collections;

public class Touch : Sense
{
    //check if collides withs something with an entity component
    void OnTriggerEnter(Collider other)
    {
        Entity entityType = other.GetComponent<Entity>();
        if (entityType != null)
        {
            //Check the entity
            if (entityType.entityName == entityName)
            {
                Debug.Log("Enemy Touch Detected");
            }
        }
    }


}
