using UnityEngine;
using UnityEngine.Events;

public class InteraccionID : MonoBehaviour
{
    public string idRequerido; 
    public bool seConsumeAlUsar = true;
    public UnityEvent alFuncionar;

    public void IntentarAccion(Item itemEnMano, SilentHillInventory inv)
    {
        if (itemEnMano != null && itemEnMano.idUnico == idRequerido)
        {
            Debug.Log("ID Correcto!");
            alFuncionar.Invoke(); // Aquí es donde abrís la caja

            // ESTO ES LO QUE FALTA:
            if (seConsumeAlUsar) 
            {
                inv.EliminarItemActual();
            }
        }
        else
        {
            Debug.Log("Este objeto no sirve aquí.");
        }
    }
}



