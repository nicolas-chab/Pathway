using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public virtual void Interact()
    {
        Debug.Log("Interacted with " + gameObject.name);
        // Add your interaction logic here
    }
}
