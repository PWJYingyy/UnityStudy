using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropWeapon : MonoBehaviour
{
    public List<GameObject> weqponList;

    public void DropSwords(){
        foreach (GameObject item in weqponList)
        {
            //item.AddComponent<Rigidbody>();
            item.AddComponent<BoxCollider>();
            item.transform.parent = null;
        }
    }
}
