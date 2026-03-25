using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SilentHillInventory : MonoBehaviour
{
    [Header("Colección de Items")]
    public List<Item> items = new List<Item>(); 
    private int indiceActual = 0; 
    private bool estaAbierto = false;
    private bool enModoInspeccion = false;

    [Header("UI: Paneles")]
    public GameObject panelPrincipal; 
    public GameObject panelVisualizador3D; 
    public GameObject subMenuMouse; // El panel que tiene los 2 botones
    public MonoBehaviour scriptControladorCamara; 

    [Header("UI: Textos")]
    public TextMeshProUGUI textoNombreItem;
    public TextMeshProUGUI textoDescripcionItem;

    [Header("UI: Iconos")]
    public Transform contenedorFilaIconos; 
    public GameObject prefabSlotIcono; 

    [Header("Referencias Estudio 3D")]
    public Transform puntoDeSpawn_Item; 
    private InteraccionID objetoPendiente; // Para saber qué puerta queremos abrir

    void Start()
    {
        // 1. Nos aseguramos de que el panel principal esté apagado
        if (panelPrincipal != null) panelPrincipal.SetActive(false);

        // 2. IMPORTANTÍSIMO: El visualizador 3D debe empezar apagado
        // para que no veas el modelo apenas inicias el juego o abres el inventario
        if (panelVisualizador3D != null) panelVisualizador3D.SetActive(false);

        // 3. El submenú de botones (Usar/Inspeccionar) también apagado
        if (subMenuMouse != null) subMenuMouse.SetActive(false);

        // Si ya tienes items al empezar, actualizamos la lógica interna pero sin mostrar el 3D
        if (items.Count > 0)
        {
            indiceActual = 0;
            // No llamamos a ActualizarVista() aquí si no quieres que se genere el modelo 
            // hasta que el jugador abra el inventario.
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I)) ToggleInventario();

        if (estaAbierto && items.Count > 0 && !enModoInspeccion)
        {
            // Navegación rápida con teclado opcional
            if (Input.GetKeyDown(KeyCode.D)) CambiarSeleccion(1);
            if (Input.GetKeyDown(KeyCode.A)) CambiarSeleccion(-1);

            // Si apretás Enter, también se abre el submenú de botones
            if (Input.GetKeyDown(KeyCode.Return)) AbrirOpciones();
        }
        else if (enModoInspeccion)
        {
            if (Input.GetKeyDown(KeyCode.Q) || Input.GetKeyDown(KeyCode.Escape)) SalirDeInspeccion();
        }
    }

    public void AbrirParaInteraccion(InteraccionID receptor)
    {
        // Si no tenemos items, no deberíamos ni intentar interactuar
        if (items.Count == 0)
        {
            Debug.Log("No tienes nada en el inventario para usar con esto.");
            objetoPendiente = null; // Limpiamos por las dudas
            return; 
        }

        objetoPendiente = receptor;
        if (!estaAbierto) ToggleInventario();
        
        // Solo abrimos las opciones si realmente hay algo que mostrar
        AbrirOpciones();
    }

    public void ToggleInventario()
    {
        estaAbierto = !estaAbierto;
        panelPrincipal.SetActive(estaAbierto);

        // Bloquear/Desbloquear el cursor
        Cursor.lockState = estaAbierto ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = estaAbierto;

        // Pausar el tiempo del juego (opcional, pero recomendado para terror)
        Time.timeScale = estaAbierto ? 0f : 1f;

        // --- AQUÍ ESTÁ EL TRUCO ---
        if (scriptControladorCamara != null)
        {
            // Si el inventario está abierto, el script de la cámara se DESACTIVA
            scriptControladorCamara.enabled = !estaAbierto;
        }

        if (estaAbierto) ActualizarVista();
    }

    // --- FUNCIONES PARA LOS BOTONES DEL MOUSE ---
    public void ClickBotonUsar()
    {
        if (items.Count == 0 || objetoPendiente == null) return;

        // Calculamos la distancia entre el jugador y la puerta
        float distancia = Vector3.Distance(transform.position, objetoPendiente.transform.position);

        // Si el jugador se alejó más de 4 metros, cancelamos el uso
        if (distancia > 4f) 
        {
            Debug.Log("Estás demasiado lejos de la puerta.");
            objetoPendiente = null; // Limpiamos la referencia
            ToggleInventario();
            return;
        }

        // Si está cerca, procedemos...
        objetoPendiente.IntentarAccion(items[indiceActual], this);
        objetoPendiente = null;
        ToggleInventario();
    }

    public void ClickBotonInspeccionar()
    {
        subMenuMouse.SetActive(false);
        EntrarAInspeccion();
    }

    public void AbrirOpciones()
    {
        if (enModoInspeccion) return;

        subMenuMouse.SetActive(true);
    }

    // --- LÓGICA INTERNA ---
    void CambiarSeleccion(int dir)
    {
        indiceActual = (indiceActual + dir + items.Count) % items.Count;
        subMenuMouse.SetActive(false); // Ocultar botones al cambiar de ítem
        ActualizarVista();
    }

    void EntrarAInspeccion()
    {
        // Si por algún error la lista está vacía, abortamos
        if (items.Count == 0) return;

        enModoInspeccion = true;
        if (panelVisualizador3D != null) panelVisualizador3D.SetActive(true);
        if (contenedorFilaIconos != null) contenedorFilaIconos.gameObject.SetActive(false);
    }

    void SalirDeInspeccion()
    {
        enModoInspeccion = false;
        panelVisualizador3D.SetActive(false);
        contenedorFilaIconos.gameObject.SetActive(true);
    }

    void ActualizarVista()
    {
        // Limpiamos los iconos de la fila siempre
        foreach (Transform t in contenedorFilaIconos) Destroy(t.gameObject);

        // CASO A: EL INVENTARIO ESTÁ VACÍO
        if (items.Count == 0) 
        {
            // 1. Ponemos el índice en 0 por seguridad
            indiceActual = 0;

            // 2. Limpiamos los textos de la UI
            textoNombreItem.text = ""; // O podrías poner "Inventario Vacío"
            textoDescripcionItem.text = "";

            // 3. Borramos el modelo 3D que estaba en el estudio
            foreach (Transform t in puntoDeSpawn_Item) Destroy(t.gameObject);

            // Opcional: Si tienes el cartel de "Items" arriba, podrías ocultarlo
            // textoTituloItems.gameObject.SetActive(false); 

            
            return; // IMPORTANTE: Cortamos la ejecución aquí
        }

        // CASO B: EL INVENTARIO TIENE OBJETOS (Lógica que ya tenías)
        
        // Blindaje extra: aseguramos que el índice nunca se escape
        indiceActual = Mathf.Clamp(indiceActual, 0, items.Count - 1);
        
        Item item = items[indiceActual];
        textoNombreItem.text = item.nombre;
        textoDescripcionItem.text = item.descripcion;

        // Redibujar iconos
        for (int i = 0; i < items.Count; i++)
        {
            GameObject slot = Instantiate(prefabSlotIcono, contenedorFilaIconos);
            Image img = slot.GetComponent<Image>();
            img.sprite = items[i].icono;
            img.color = (i == indiceActual) ? Color.white : new Color(1,1,1,0.4f);
        }

        // Actualizar 3D
        foreach (Transform t in puntoDeSpawn_Item) Destroy(t.gameObject);
        if (item.modelo3DPrefab != null)
        {
            GameObject clon = Instantiate(item.modelo3DPrefab, puntoDeSpawn_Item.position, puntoDeSpawn_Item.rotation);
            clon.transform.SetParent(puntoDeSpawn_Item);
            clon.transform.localScale = Vector3.one;
        }
    }

    public void EliminarItemActual()
    {
        if (items.Count > 0)
        {
            // Borramos el objeto de la lista
            items.RemoveAt(indiceActual);

            // Re-ajustamos el índice para que no apunte a la nada
            if (indiceActual >= items.Count)
            {
                indiceActual = Mathf.Max(0, items.Count - 1);
            }

            // ¡IMPORTANTE! Llamar a esto para que los iconos se borren de la UI
            ActualizarVista(); 
            Debug.Log("Item eliminado del inventario.");
        }
    }
    public void AgregarItemAlInventario(Item nuevo)
    {
        items.Add(nuevo);
        if (items.Count == 1) indiceActual = 0; 
    }
    public bool PuedeRotar() => enModoInspeccion;
}