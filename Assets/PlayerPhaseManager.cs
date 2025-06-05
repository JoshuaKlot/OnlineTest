using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class PlayerPhaseManager : NetworkBehaviour
{
    public Button resetButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        resetButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.IsHost)
            {
                GameManager.Instance.ResetGame(); // Call host-side start logic
                //startButton.gameObject.SetActive(false); // Hide the button after pressing
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
