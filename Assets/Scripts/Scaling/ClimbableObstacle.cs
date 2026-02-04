using UnityEngine;

public class ClimbableObstacle : MonoBehaviour
{
    public Transform alignPoint; // Donde se alinea antes de escalar
    public Transform topPoint; // Punto superior
    public Transform exitPoint; // Salida al escalar
    public Transform hangPoint; // Donde queda colgado
    public Transform dropPoint; // Donde cae al bajar
}
