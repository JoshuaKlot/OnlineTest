using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    [SerializeField] private int SetNum;

    void OnMouseDown()
    {
        this.GetComponentInParent<Selection>().SetObject(SetNum);
    }
}
