using UnityEngine;

public class CameraInteraction : MonoBehaviour
{
    private new Transform camera;
    public float raydistance = 10f;
    public SilentHillInventory inventario;
     void Awake()
    {
        camera = Camera.main.transform;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Buscamos el objeto que tiene el script del inventario
        inventario = FindAnyObjectByType<SilentHillInventory>();
    }

    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(camera.position, camera.forward * raydistance, Color.red);
        if (Input.GetMouseButtonDown(0)) // Check for left mouse button click
        {
            
        
            RaycastHit hit;
            if (Physics.Raycast(camera.position, camera.forward, out hit, raydistance))
            {
                
                if (hit.collider.CompareTag("Interactable"))
                {
                    hit.transform.GetComponent<InteractableObject>().Interact(); // Call the Interact method on the hit object
                }
                InteraccionID objetoConID = hit.collider.GetComponent<InteraccionID>();

                if (objetoConID != null)
                {
                    // En lugar de intentar la acción acá, abrimos el inventario 
                    // y le avisamos que esta es la puerta que queremos abrir.
                    inventario.AbrirParaInteraccion(objetoConID);
                }
            }
            
        }
    }
}
