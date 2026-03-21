using UnityEngine;

public class InteractableObject : MonoBehaviour
{
    public Item itemData; // Arrastra aquí el ScriptableObject que creaste
    public virtual void Interact()
    {
        // Buscamos el manager y le pasamos este objeto
        Object.FindAnyObjectByType<InventoryManager>().AgregarObjeto(itemData);
        
        // Destruimos el objeto del mundo o lo desactivamos
        gameObject.SetActive(false);
    }
}
