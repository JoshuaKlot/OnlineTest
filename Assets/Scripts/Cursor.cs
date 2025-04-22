using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] private GameObject coins;
    [SerializeField] private int numOfCoins;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 cursorWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        this.transform.position = new Vector3(cursorWorldPosition.x, cursorWorldPosition.y,0);

        if (Input.GetMouseButtonDown(0))
        {
            if (numOfCoins > 0)
            {
                GameObject placedCoins;
                placedCoins=Instantiate(coins);
                placedCoins.transform.parent = GameObject.Find("MainLevel").transform;
                placedCoins.transform.position=this.transform.position;
                numOfCoins--;
            }
        }

        if (numOfCoins > 0)
        {
            MainLevel.playerPhase = true;
        }
    }
}
