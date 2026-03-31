using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SilentHillInventory : MonoBehaviour
{
    [Header("Colección de Items")]
    public List<Item> items = new List<Item>(); 
    public int indiceActual = 0; 
    public bool estaAbierto = false; // Pública para que CameraInteraction la vea
    public bool enModoInspeccion = false;

    [Header("UI: Paneles")]
    public GameObject panelPrincipal; 
    public GameObject panelVisualizador3D; 
    public GameObject subMenuMouse; 
    public MonoBehaviour scriptControladorCamara; 

    [Header("Botones Específicos")]
    public GameObject botonUsar; 
    public GameObject botonInspeccionar; 

    [Header("UI: Textos")]
    public TextMeshProUGUI textoNombreItem;
    public TextMeshProUGUI textoDescripcionItem;

    [Header("UI: Iconos")]
    public Transform contenedorFilaIconos; 
    public GameObject prefabSlotIcono; 

    [Header("Referencias Estudio 3D")]
    public Transform puntoDeSpawn_Item; 
    public InteraccionID objetoPendiente; 

    void Start()
    {
        if (panelPrincipal != null) panelPrincipal.SetActive(false);
        if (panelVisualizador3D != null) panelVisualizador3D.SetActive(false);
        if (subMenuMouse != null) subMenuMouse.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) ToggleInventario();

        if (estaAbierto && items.Count > 0)
        {
            if (enModoInspeccion)
            {
                if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)) SalirDeInspeccion();
            }
        }
    }

    // --- LA FUNCIÓN QUE FALTABA ---
    public void AbrirParaInteraccion(InteraccionID receptor)
    {
        if (items.Count == 0) 
        {
            Debug.Log("No tienes nada para usar aquí.");
            return;
        }

        objetoPendiente = receptor;
        if (!estaAbierto) ToggleInventario();
        
        // Al abrir por puerta, seleccionamos el item y mostramos opciones
        SeleccionarItemPorClick(indiceActual);
    }

    public void ToggleInventario()
    {
        estaAbierto = !estaAbierto;
        
        if (estaAbierto)
        {
            enModoInspeccion = false;
            if (panelVisualizador3D != null) panelVisualizador3D.SetActive(false);
            if (contenedorFilaIconos != null) contenedorFilaIconos.gameObject.SetActive(true);

            // Si abrimos con la "I", olvidamos cualquier puerta anterior
            if (Input.GetKeyDown(KeyCode.I)) objetoPendiente = null;

            // Forzamos selección inicial para que los botones aparezcan siempre
            if (items.Count > 0) SeleccionarItemPorClick(indiceActual);
        }
        else
        {
            objetoPendiente = null;
        }

        panelPrincipal.SetActive(estaAbierto);
        
        // Liberar cursor
        Cursor.lockState = estaAbierto ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = estaAbierto;
        
        Time.timeScale = estaAbierto ? 0f : 1f;
        if (scriptControladorCamara != null) scriptControladorCamara.enabled = !estaAbierto;

        if (estaAbierto) ActualizarVista();
    }

    public void SeleccionarItemPorClick(int indice)
    {
        if (enModoInspeccion) return;

        indiceActual = indice;
        ActualizarVista(); 

        // ACTIVAR SUBMENÚ
        if (subMenuMouse != null) subMenuMouse.SetActive(true);
        
        // Inspeccionar: SIEMPRE PRENDIDO
        if (botonInspeccionar != null) botonInspeccionar.SetActive(true);

        // Usar: SOLO si hay puerta
        if (botonUsar != null) botonUsar.SetActive(objetoPendiente != null);
    }

    public void ClickBotonUsar()
    {
        if (items.Count > 0 && objetoPendiente != null)
        {
            objetoPendiente.IntentarAccion(items[indiceActual], this);
            objetoPendiente = null;
            ToggleInventario();
        }
    }

    public void ClickBotonInspeccionar()
    {
        if (subMenuMouse != null) subMenuMouse.SetActive(false);
        EntrarAInspeccion();
    }

    void EntrarAInspeccion()
    {
        if (items.Count == 0) return;
        enModoInspeccion = true;
        if (panelVisualizador3D != null) panelVisualizador3D.SetActive(true);
        if (contenedorFilaIconos != null) contenedorFilaIconos.gameObject.SetActive(false);
    }

    void SalirDeInspeccion()
    {
        enModoInspeccion = false;
        if (panelVisualizador3D != null) panelVisualizador3D.SetActive(false);
        if (contenedorFilaIconos != null) contenedorFilaIconos.gameObject.SetActive(true);
    }

    public void ActualizarVista()
    {
        // Limpiamos los iconos viejos
        foreach (Transform t in contenedorFilaIconos) Destroy(t.gameObject);

        if (items.Count == 0) 
        {
            indiceActual = 0;
            textoNombreItem.text = "";
            textoDescripcionItem.text = "Inventario Vacío";
            foreach (Transform t in puntoDeSpawn_Item) Destroy(t.gameObject);
            return;
        }

        indiceActual = Mathf.Clamp(indiceActual, 0, items.Count - 1);
        Item item = items[indiceActual];
        textoNombreItem.text = item.nombre;
        textoDescripcionItem.text = item.descripcion;

        for (int i = 0; i < items.Count; i++)
        {
            GameObject slot = Instantiate(prefabSlotIcono, contenedorFilaIconos);
            
            // --- AQUÍ ESTÁ EL ARREGLO ---
            Image img = slot.GetComponent<Image>();
            if (img != null) 
            {
                // Asignamos el icono que tenés en el Scriptable Object
                img.sprite = items[i].icono; 
                
                // Resaltamos el seleccionado
                img.color = (i == indiceActual) ? Color.white : new Color(1, 1, 1, 0.4f);
                img.raycastTarget = true;
            }

            // Si tu prefab tiene texto (como el que vimos antes)
            TextMeshProUGUI txt = slot.GetComponentInChildren<TextMeshProUGUI>();
            if (txt != null) 
            {
                txt.text = items[i].nombre; // Ponemos el nombre del objeto
                txt.color = (i == indiceActual) ? Color.white : new Color(1, 1, 1, 0.4f);
            }

            // Aseguramos que tenga botón para el mouse
            Button btn = slot.GetComponent<Button>();
            if (btn == null) btn = slot.AddComponent<Button>();

            int indexLocal = i; 
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => SeleccionarItemPorClick(indexLocal));
        }

        // Actualizar el modelo 3D del estudio
        foreach (Transform t in puntoDeSpawn_Item) Destroy(t.gameObject);
        if (item.modelo3DPrefab != null)
        {
            GameObject clon = Instantiate(item.modelo3DPrefab, puntoDeSpawn_Item.position, puntoDeSpawn_Item.rotation);
            clon.transform.SetParent(puntoDeSpawn_Item);
        }
    }

    public void AgregarItemAlInventario(Item nuevo) { items.Add(nuevo); if (estaAbierto) ActualizarVista(); }
    public void EliminarItemActual() { if (items.Count > 0) { items.RemoveAt(indiceActual); indiceActual = Mathf.Max(0, items.Count - 1); ActualizarVista(); } }

    public bool EstaAbierto() => estaAbierto;
}
    

