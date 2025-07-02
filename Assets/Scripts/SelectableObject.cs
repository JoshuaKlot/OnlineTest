using UnityEngine;

public class SelectableObject : MonoBehaviour
{
    public int SetNum;

    void OnMouseDown()
    {
        this.GetComponentInParent<Selection>().SetObject(SetNum);
    }
}
