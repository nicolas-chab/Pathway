using UnityEngine;

[CreateAssetMenu(fileName = "Nuevo Objeto", menuName = "Inventario/Objeto")]
public class Item : ScriptableObject
{
    public string nombre;
    [TextArea] public string descripcion; // Para que el texto sea largo
    public Sprite icono;
    public string idUnico; // Ejemplo: "llave_plata", "cuchillo_caza"
}