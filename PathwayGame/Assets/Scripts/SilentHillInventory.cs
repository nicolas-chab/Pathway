using System.Collections.Generic;
using UnityEngine;
using TMPro; // Asegurate de tener TextMeshPro instalado
using UnityEngine.UI;

public class SilentHillInventory : MonoBehaviour
{
    public List<Item> items = new List<Item>(); // Nuestra lista de objetos
    private int indiceActual = 0; // ¿Qué objeto estamos mirando?
    private bool estaAbierto = false;

    [Header("UI Referencias")]
    public GameObject panelCompleto; // El panel negro de fondo
    public TextMeshProUGUI textoNombre; 
    public TextMeshProUGUI textoDescripcion;
    public RawImage visorImagen; // La imagen central
    public Transform puntoDeInspeccion; // Arrastrá el objeto vacío que creamos en el Paso 1
    public MonoBehaviour scriptControladorCamara; // Arrastrá aquí el script de movimiento de cámara
    
    void Start()
    {
        // Al empezar, el inventario está cerrado
        CerrarInventario();
    }

    void Update()
    {
        // Tecla I para abrir/cerrar
        if (Input.GetKeyDown(KeyCode.I))
        {
            ToggleInventario();
        }

        if (estaAbierto && items.Count > 0)
        {
            // Navegación con W/S (Silent Hill usaba D-Pad, aquí W/S)
            if (Input.GetKeyDown(KeyCode.W)) CambiarSeleccion(-1);
            if (Input.GetKeyDown(KeyCode.S)) CambiarSeleccion(1);

            // Tecla ENTER para 'Usar'
            if (Input.GetKeyDown(KeyCode.Return)) TentativelyUseItem();
        }
    }

    void ToggleInventario()
    {
        estaAbierto = !estaAbierto;
        panelCompleto.SetActive(estaAbierto);

        // 1. Congelar el tiempo del mundo
        Time.timeScale = estaAbierto ? 0f : 1f;

        // 2. Bloquear / Desbloquear el script de la cámara
        if (scriptControladorCamara != null)
        {
            scriptControladorCamara.enabled = !estaAbierto; 
        }

        // 3. Manejo del Cursor (Fundamental para que no se mueva la vista)
        if (estaAbierto)
        {
            Cursor.lockState = CursorLockMode.None; // Libera el mouse para moverlo en el menú
            Cursor.visible = true; // Lo hace visible
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked; // Vuelve a trabar el mouse al centro
            Cursor.visible = false;
        }

        if (estaAbierto) ActualizarVista();
    }

    void AbrirInventario()
    {
        panelCompleto.SetActive(true);
        // Opcional: Pausar el tiempo del juego aquí
        Time.timeScale = 0f; 
        ActualizarVista();
    }

    void CerrarInventario()
    {
        panelCompleto.SetActive(false);
        // Opcional: Reanudar el tiempo del juego
        Time.timeScale = 1f; 
    }

    void CambiarSeleccion(int direccion)
    {
        indiceActual = Mathf.Clamp(indiceActual + direccion, 0, items.Count - 1);
        ActualizarVista();
        // Opcional: Sonido de menú aquí
    }

    void ActualizarVista()
    {
       if (items.Count == 0)
        {
            textoNombre.text = "VACÍO";
            textoDescripcion.text = "No hay nada aquí.";
            return;
        }

        // 1. OBTENER LOS DATOS DEL ITEM ACTUAL
        Item item = items[indiceActual];
        textoNombre.text = item.nombre;
        textoDescripcion.text = item.descripcion;

        // 2. LIMPIAR EL MODELO ANTERIOR (Para que no se amontonen los objetos)
        // Esto borra lo que sea que haya quedado de la última vez que abriste el menú
        foreach (Transform hijo in puntoDeInspeccion) 
        {
            Destroy(hijo.gameObject);
        }

        // 3. HACER APARECER EL MODELO NUEVO (La "Magia")
        if (item.modelo3DPrefab != null) 
        {
            // Lo creamos justo en el PuntoDeInspeccion que definimos frente a la cámara
            GameObject clon = Instantiate(item.modelo3DPrefab, puntoDeInspeccion.position, puntoDeInspeccion.rotation);
            
            // Lo hacemos hijo del punto para que se mueva con él
            clon.transform.SetParent(puntoDeInspeccion);
            
            // OPCIONAL: Escalarlo si el modelo es muy grande o chico
            clon.transform.localScale = Vector3.one; 
        }
    }

    public void AgregarItemAlInventario(Item nuevo)
    {
        items.Add(nuevo);
        // Si es el primer objeto, lo seleccionamos automáticamente
        if (items.Count == 1) indiceActual = 0; 
        Debug.Log("Obtenido: " + nuevo.nombre);
    }

    void TentativelyUseItem()
    {
        if (items.Count == 0) return;
        Debug.Log("Intentando usar: " + items[indiceActual].nombre);
        // Aquí iría la lógica para combinar con el Raycast del mundo
    }
    public bool EstaAbierto()
    {
        return estaAbierto;
    }
}