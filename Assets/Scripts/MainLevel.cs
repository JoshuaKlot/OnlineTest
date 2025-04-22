using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;


public class MainLevel : MonoBehaviour
{
    public static bool playerPhase = false;
    [SerializeField] private GameObject Cursor;
    [SerializeField] private GameObject Player;


    // Update is called once per frame
    void Update()
    {
        if (playerPhase)
        {
            Destroy(Cursor);
            Instantiate(Player);
        }

        if (playerPhase && transform.childCount == 0)
        {
            Application.Quit();
        }
    }
}
