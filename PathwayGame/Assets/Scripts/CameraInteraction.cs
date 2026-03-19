using UnityEngine;

public class CameraInteraction : MonoBehaviour
{
    private new Transform camera;
    public float raydistance = 10f;

     void Awake()
    {
        camera = Camera.main.transform;
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
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
            }
        }
    }
}
