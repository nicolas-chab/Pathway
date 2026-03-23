using UnityEngine;

[CreateAssetMenu(fileName = "Nuevo Objeto", menuName = "Inventario/Objeto")]
public class Item : ScriptableObject
{
    public string nombre;
    [TextArea] public string descripcion; // Para que el texto sea largo
    public GameObject modelo3DPrefab; // Arrastrá aquí el FBX del cuchillo
    public string idUnico; // Ejemplo: "llave_plata", "cuchillo_caza"
}