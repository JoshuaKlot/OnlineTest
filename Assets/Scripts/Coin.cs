using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
public class Coin : NetworkBehaviour
{
    void Start()
    {
        
        this.transform.parent = GameObject.Find("MainLevel").transform;
    }
    private void OnTriggerEnter2D(Collider2D col)
    {
        Debug.Log(col.gameObject.name + " : " + gameObject.name + " : " + Time.time);
        Destroy(this.gameObject);
    }
}
