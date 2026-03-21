using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // Asegúrate de tener TextMeshPro instalado

public class InventoryManager : MonoBehaviour
{
    public List<Item> objetosEnInventario = new List<Item>();
    public Item objetoSeleccionado;

    [Header("UI Referencias")]
    public GameObject panelInventario;
    public TextMeshProUGUI textoNombre;
    public TextMeshProUGUI textoDescripcion;

    void Update()
    {
        // Abrir/Cerrar inventario con la tecla I
        if (Input.GetKeyDown(KeyCode.I))
        {
            panelInventario.SetActive(!panelInventario.activeSelf);
            ActualizarUI();
        }
    }

    public void AgregarObjeto(Item nuevoItem)
    {
        objetosEnInventario.Add(nuevoItem);
        Debug.Log("Obtenido: " + nuevoItem.nombre);

        // NUEVO: Si es el primer objeto o el único, mostrarlo de una vez
        if (objetosEnInventario.Count == 1) 
        {
            SeleccionarObjeto(0); 
        }
        
        // Opcional: Si querés que el inventario se abra solo al agarrar algo
        // panelInventario.SetActive(true); 
        
        ActualizarUI();
    }

    public void SeleccionarObjeto(int indice)
    {
        objetoSeleccionado = objetosEnInventario[indice];
        textoNombre.text = objetoSeleccionado.nombre;
        textoDescripcion.text = objetoSeleccionado.descripcion;
    }

    void ActualizarUI()
    {
        // Aquí podrías crear botones dinámicamente para cada objeto
        // Por ahora, solo limpia el texto si no hay nada seleccionado
        if (objetoSeleccionado == null && objetosEnInventario.Count > 0)
        {
            SeleccionarObjeto(0);
        }
    }
}