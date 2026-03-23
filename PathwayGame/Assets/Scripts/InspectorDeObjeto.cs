using UnityEngine;

public class Inspector : MonoBehaviour
{
   [Header("Configuración")]
    public Transform puntoDeInspeccion; // El objeto vacío frente a la cámara
    public float sensibilidad = 5f;
    public float friccion = 0.95f; // Para que siga girando un poquito al soltar

    [Header("Referencias")]
    public SilentHillInventory inventario;

    private Vector3 velocidadRotacion;
    private Vector2 ultimaPosicionMouse;
    private bool arrastrando = false; // Estado interno para controlar el arrastre

    void Update()
    {
        // Solo funciona si el inventario está abierto
        if (inventario != null && inventario.EstaAbierto())
        {
            // --- DETECTAR EL CLIC INICIAL (GetMouseButtonDown) ---
            if (Input.GetMouseButtonDown(0))
            {
                arrastrando = true;
                ultimaPosicionMouse = Input.mousePosition;
                velocidadRotacion = Vector3.zero; // Reseteamos la inercia anterior
            }

            // --- MANANTENER EL CLIC (ARRASTRE - GetMouseButton) ---
            // Solo procesamos si el script "sabe" que empezamos a arrastrar
            if (Input.GetMouseButton(0) && arrastrando)
            {
                Vector2 deltaMouse = (Vector2)Input.mousePosition - ultimaPosicionMouse;
                
                // La magia: calculamos el eje de rotación basado en el movimiento del mouse
                velocidadRotacion = new Vector3(deltaMouse.y, -deltaMouse.x, 0);
                
                ultimaPosicionMouse = Input.mousePosition;
            }

            // --- SOLTAR EL CLIC (GetMouseButtonUp) ---
            if (Input.GetMouseButtonUp(0))
            {
                arrastrando = false;
            }

            // --- APLICAR FRICCIÓN (Inercia) ---
            if (!arrastrando)
            {
                velocidadRotacion *= friccion;
            }

            // --- APLICAR LA ROTACIÓN ---
            if (velocidadRotacion.magnitude > 0.01f)
            {
                puntoDeInspeccion.Rotate(Vector3.up, velocidadRotacion.y * sensibilidad * Time.unscaledDeltaTime, Space.World);
                puntoDeInspeccion.Rotate(Vector3.right, velocidadRotacion.x * sensibilidad * Time.unscaledDeltaTime, Space.World);
            }
        }
        else
        {
            // Si el inventario está cerrado, nos aseguramos de no estar arrastrando
            arrastrando = false;
        }
    }

    // --- FUNCIÓN CLAVE: SE EJECUTA CUANDO VUELVES A LA VENTANA ---
    void OnApplicationFocus(bool hasFocus)
    {
        // Si la ventana recupera el foco, forzamos el reseteo del estado de arrastre
        if (hasFocus)
        {
            arrastrando = false;
            // Opcional: Podés forzar el bloqueo del cursor momentáneamente si tu inventario lo requiere
            // Cursor.lockState = CursorLockMode.None;
        }
    }
}