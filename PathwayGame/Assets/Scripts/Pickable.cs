using UnityEngine;

public class Pickable : InteractableObject
{
    public override void Interact()
    {
        base.Interact();
        Destroy(gameObject); // Example: Destroy the object when picked up
        
    }
}
