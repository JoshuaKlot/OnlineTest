using UnityEngine;

public class RetainScreenPosition: MonoBehaviour
{
    [SerializeField] private Vector2 StayHere;
    void Update()
    {
        // Get the current screen position of the object
        Vector2 worldPosition = Camera.main.ScreenToWorldPoint(StayHere);
        
        transform.position = new Vector3(worldPosition.x, worldPosition.y, transform.position.z);
    }
}
