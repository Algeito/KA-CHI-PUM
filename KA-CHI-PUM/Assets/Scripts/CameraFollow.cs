using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Configuración de Seguimiento")]
    [SerializeField] private Transform objetivo;
    [SerializeField] private float suavizado = 0.125f;
    [SerializeField] private Vector3 offset = new Vector3(0f, 0f, -10f);
    
    [Header("Límites de Cámara (Opcional)")]
    [SerializeField] private bool usarLimites = false;
    [SerializeField] private float limiteMinX = -50f;
    [SerializeField] private float limiteMaxX = 50f;
    [SerializeField] private float limiteMinY = -50f;
    [SerializeField] private float limiteMaxY = 50f;

    private void Start()
    {
        // Si no se asignó objetivo en el Inspector, buscar al jugador
        if (objetivo == null)
        {
            GameObject jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null)
            {
                objetivo = jugador.transform;
            }
            else
            {
                Debug.LogError("CameraFollow: No se encontró el jugador. Asegúrate de que tenga el tag 'Player'");
            }
        }
    }

    private void LateUpdate()
    {
        if (objetivo == null) return;

        // Calcular posición deseada
        Vector3 posicionDeseada = objetivo.position + offset;

        // Aplicar límites si están activados
        if (usarLimites)
        {
            posicionDeseada.x = Mathf.Clamp(posicionDeseada.x, limiteMinX, limiteMaxX);
            posicionDeseada.y = Mathf.Clamp(posicionDeseada.y, limiteMinY, limiteMaxY);
        }

        // Interpolar suavemente hacia la posición deseada
        Vector3 posicionSuavizada = Vector3.Lerp(transform.position, posicionDeseada, suavizado);
        
        // Aplicar la nueva posición
        transform.position = posicionSuavizada;
    }

    // Método para cambiar el objetivo en tiempo de ejecución
    public void CambiarObjetivo(Transform nuevoObjetivo)
    {
        objetivo = nuevoObjetivo;
    }

    // Método para ajustar el suavizado en tiempo de ejecución
    public void AjustarSuavizado(float nuevoSuavizado)
    {
        suavizado = Mathf.Clamp01(nuevoSuavizado);
    }

    // Visualizar el área de la cámara en el editor
    private void OnDrawGizmosSelected()
    {
        if (usarLimites)
        {
            Gizmos.color = Color.yellow;
            
            // Dibujar los límites como un rectángulo
            Vector3 esquinaInferiorIzq = new Vector3(limiteMinX, limiteMinY, 0);
            Vector3 esquinaInferiorDer = new Vector3(limiteMaxX, limiteMinY, 0);
            Vector3 esquinaSuperiorDer = new Vector3(limiteMaxX, limiteMaxY, 0);
            Vector3 esquinaSuperiorIzq = new Vector3(limiteMinX, limiteMaxY, 0);
            
            Gizmos.DrawLine(esquinaInferiorIzq, esquinaInferiorDer);
            Gizmos.DrawLine(esquinaInferiorDer, esquinaSuperiorDer);
            Gizmos.DrawLine(esquinaSuperiorDer, esquinaSuperiorIzq);
            Gizmos.DrawLine(esquinaSuperiorIzq, esquinaInferiorIzq);
        }
    }
}
