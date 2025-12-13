using UnityEngine;

public class CameraSpawnPoints : MonoBehaviour
{
    [Header("Configuración de Puntos")]
    [SerializeField] private Camera camaraJuego;
    [SerializeField] private float distanciaFueraDeCamara = 2f;
    [SerializeField] private bool actualizarEnTiempoReal = true;
    
    [Header("Visualización")]
    [SerializeField] private bool mostrarGizmos = true;
    [SerializeField] private Color colorGizmos = Color.cyan;
    
    // Puntos de spawn (se actualizan automáticamente)
    private Vector2 puntoArriba;
    private Vector2 puntoAbajo;
    private Vector2 puntoIzquierda;
    private Vector2 puntoDerecha;
    private Vector2 puntoArribaIzquierda;
    private Vector2 puntoArribaDerecha;
    private Vector2 puntoAbajoIzquierda;
    private Vector2 puntoAbajoDerecha;

    private void Start()
    {
        // Si no se asignó cámara, usar la principal
        if (camaraJuego == null)
        {
            camaraJuego = Camera.main;
        }

        if (camaraJuego == null)
        {
            Debug.LogError("CameraSpawnPoints: No se encontró ninguna cámara!");
            return;
        }

        ActualizarPuntosSpawn();
    }

    private void Update()
    {
        if (actualizarEnTiempoReal)
        {
            ActualizarPuntosSpawn();
        }
    }

    private void ActualizarPuntosSpawn()
    {
        if (camaraJuego == null) return;

        // Obtener los límites de la cámara en coordenadas del mundo
        float altoCamara = camaraJuego.orthographicSize;
        float anchoCamara = altoCamara * camaraJuego.aspect;
        
        Vector3 posicionCamara = camaraJuego.transform.position;

        // Calcular puntos cardinales (arriba, abajo, izquierda, derecha)
        puntoArriba = new Vector2(
            posicionCamara.x, 
            posicionCamara.y + altoCamara + distanciaFueraDeCamara
        );
        
        puntoAbajo = new Vector2(
            posicionCamara.x, 
            posicionCamara.y - altoCamara - distanciaFueraDeCamara
        );
        
        puntoIzquierda = new Vector2(
            posicionCamara.x - anchoCamara - distanciaFueraDeCamara, 
            posicionCamara.y
        );
        
        puntoDerecha = new Vector2(
            posicionCamara.x + anchoCamara + distanciaFueraDeCamara, 
            posicionCamara.y
        );

        // Calcular puntos diagonales (esquinas)
        puntoArribaIzquierda = new Vector2(
            posicionCamara.x - anchoCamara - distanciaFueraDeCamara,
            posicionCamara.y + altoCamara + distanciaFueraDeCamara
        );
        
        puntoArribaDerecha = new Vector2(
            posicionCamara.x + anchoCamara + distanciaFueraDeCamara,
            posicionCamara.y + altoCamara + distanciaFueraDeCamara
        );
        
        puntoAbajoIzquierda = new Vector2(
            posicionCamara.x - anchoCamara - distanciaFueraDeCamara,
            posicionCamara.y - altoCamara - distanciaFueraDeCamara
        );
        
        puntoAbajoDerecha = new Vector2(
            posicionCamara.x + anchoCamara + distanciaFueraDeCamara,
            posicionCamara.y - altoCamara - distanciaFueraDeCamara
        );
    }

    // Métodos públicos para obtener puntos
    public Vector2 ObtenerPuntoAleatorio()
    {
        int puntoAleatorio = Random.Range(0, 8);
        
        switch (puntoAleatorio)
        {
            case 0: return puntoArriba;
            case 1: return puntoAbajo;
            case 2: return puntoIzquierda;
            case 3: return puntoDerecha;
            case 4: return puntoArribaIzquierda;
            case 5: return puntoArribaDerecha;
            case 6: return puntoAbajoIzquierda;
            case 7: return puntoAbajoDerecha;
            default: return puntoArriba;
        }
    }

    public Vector2 ObtenerPuntoEnDireccion(DireccionSpawn direccion)
    {
        switch (direccion)
        {
            case DireccionSpawn.Arriba: return puntoArriba;
            case DireccionSpawn.Abajo: return puntoAbajo;
            case DireccionSpawn.Izquierda: return puntoIzquierda;
            case DireccionSpawn.Derecha: return puntoDerecha;
            case DireccionSpawn.ArribaIzquierda: return puntoArribaIzquierda;
            case DireccionSpawn.ArribaDerecha: return puntoArribaDerecha;
            case DireccionSpawn.AbajoIzquierda: return puntoAbajoIzquierda;
            case DireccionSpawn.AbajoDerecha: return puntoAbajoDerecha;
            default: return puntoArriba;
        }
    }

    public Vector2 ObtenerPuntoAleatorioEnBorde()
    {
        // Solo usar puntos cardinales, no esquinas
        int puntoAleatorio = Random.Range(0, 4);
        
        switch (puntoAleatorio)
        {
            case 0: return puntoArriba;
            case 1: return puntoAbajo;
            case 2: return puntoIzquierda;
            case 3: return puntoDerecha;
            default: return puntoArriba;
        }
    }

    public Vector2 ObtenerPuntoAleatorioEnAreaFueraDeCamara(float radio)
    {
        Vector2 puntoBase = ObtenerPuntoAleatorio();
        Vector2 offset = Random.insideUnitCircle * radio;
        return puntoBase + offset;
    }

    // Visualización en el editor
    private void OnDrawGizmos()
    {
        if (!mostrarGizmos) return;
        if (camaraJuego == null) return;

        Gizmos.color = colorGizmos;

        // Dibujar los límites de la cámara
        float altoCamara = camaraJuego.orthographicSize;
        float anchoCamara = altoCamara * camaraJuego.aspect;
        Vector3 pos = camaraJuego.transform.position;

        // Rectángulo de la cámara
        Gizmos.color = Color.yellow;
        Vector3 esquinaSupIzq = new Vector3(pos.x - anchoCamara, pos.y + altoCamara, 0);
        Vector3 esquinaSupDer = new Vector3(pos.x + anchoCamara, pos.y + altoCamara, 0);
        Vector3 esquinaInfDer = new Vector3(pos.x + anchoCamara, pos.y - altoCamara, 0);
        Vector3 esquinaInfIzq = new Vector3(pos.x - anchoCamara, pos.y - altoCamara, 0);

        Gizmos.DrawLine(esquinaSupIzq, esquinaSupDer);
        Gizmos.DrawLine(esquinaSupDer, esquinaInfDer);
        Gizmos.DrawLine(esquinaInfDer, esquinaInfIzq);
        Gizmos.DrawLine(esquinaInfIzq, esquinaSupIzq);

        // Dibujar puntos de spawn
        Gizmos.color = colorGizmos;
        float tamañoPunto = 0.5f;

        Gizmos.DrawWireSphere(puntoArriba, tamañoPunto);
        Gizmos.DrawWireSphere(puntoAbajo, tamañoPunto);
        Gizmos.DrawWireSphere(puntoIzquierda, tamañoPunto);
        Gizmos.DrawWireSphere(puntoDerecha, tamañoPunto);
        Gizmos.DrawWireSphere(puntoArribaIzquierda, tamañoPunto);
        Gizmos.DrawWireSphere(puntoArribaDerecha, tamañoPunto);
        Gizmos.DrawWireSphere(puntoAbajoIzquierda, tamañoPunto);
        Gizmos.DrawWireSphere(puntoAbajoDerecha, tamañoPunto);

        // Etiquetas (solo en Scene view)
        #if UNITY_EDITOR
        UnityEditor.Handles.Label(puntoArriba, "Arriba");
        UnityEditor.Handles.Label(puntoAbajo, "Abajo");
        UnityEditor.Handles.Label(puntoIzquierda, "Izq");
        UnityEditor.Handles.Label(puntoDerecha, "Der");
        #endif
    }
}

public enum DireccionSpawn
{
    Arriba,
    Abajo,
    Izquierda,
    Derecha,
    ArribaIzquierda,
    ArribaDerecha,
    AbajoIzquierda,
    AbajoDerecha
}
