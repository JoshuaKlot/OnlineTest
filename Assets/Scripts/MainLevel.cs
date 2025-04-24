using System.Collections;
using System.Collections.Generic;
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
            Cursor.SetActive(false);
            Player.SetActive(true);
        }

        if (playerPhase && transform.childCount == 0)
        {
            Debug.Log("All coins collected");
            Application.Quit();
        }
    }
}
