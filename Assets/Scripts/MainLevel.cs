using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class MainLevel : NetworkBehaviour
{
    public static bool playerPhase = false;
    [SerializeField] private GameObject Cursor;
    [SerializeField] private GameObject Player;

    void Start()
    {
        Instantiate(Cursor);
    }

    // Update is called once per frame
    void Update()
    {
        if (playerPhase && transform.childCount == 0)
        {
            Debug.Log("All coins collected");
            Application.Quit();
        }
    }
}
