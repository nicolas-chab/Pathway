using UnityEngine;

public class CameraInteraction : MonoBehaviour
{
    private new Transform camera;
    public float raydistance = 3f; // Reducido a 3 para realismo (Silent Hill)
    public SilentHillInventory inventario;

    void Awake()
    {
        // Buscamos la cámara principal
        camera = Camera.main.transform;
    }

    void Start()
    {
        // Buscamos el inventario en la escena
        inventario = FindFirstObjectByType<SilentHillInventory>();
    }

    void Update()
    {
        // 1. SI EL INVENTARIO ESTÁ ABIERTO, NO HACEMOS NADA MÁS
        // Esto evita que clickees iconos y puertas al mismo tiempo
        if (inventario != null && inventario.estaAbierto) return;

        // Dibujamos el rayo en el editor para debug
        Debug.DrawRay(camera.position, camera.forward * raydistance, Color.red);

        // 2. DETECCIÓN DE INTERACCIÓN (Solo si el inventario está cerrado)
        if (Input.GetMouseButtonDown(0)) 
        {
            RaycastHit hit;
            if (Physics.Raycast(camera.position, camera.forward, out hit, raydistance))
            {
                // Si es un objeto de interacción simple (llaves, notas, etc.)
                if (hit.collider.CompareTag("Interactable"))
                {
                    var interactable = hit.transform.GetComponent<InteractableObject>();
                    if(interactable != null) interactable.Interact();
                }

                // Si es una PUERTA o CAJA que requiere el inventario
                InteraccionID objetoConID = hit.collider.GetComponent<InteraccionID>();
                if (objetoConID != null)
                {
                    // Le avisamos al inventario que esta es la puerta que queremos abrir
                    inventario.AbrirParaInteraccion(objetoConID);
                }
            }
        }
    }
}