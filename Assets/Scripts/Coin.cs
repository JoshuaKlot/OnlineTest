using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Coin : MonoBehaviour
{
    void onTriggerEnter2D(Collider2D col)
    {
        Destroy(this.gameObject);
    }
}
