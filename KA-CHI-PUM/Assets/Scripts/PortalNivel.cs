using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class PortalNivel : MonoBehaviour
{
    [Header("═══ CONFIGURACIÓN ═══")]
    [SerializeField] private string nombreEscenaDestino = "Nivel_1";
    [SerializeField] private float delayAntesCambiar = 0.5f; // Delay antes de cambiar escena
    [SerializeField] private bool requiereInput = false; // ¿Necesita presionar tecla?
    [SerializeField] private KeyCode teclaInteraccion = KeyCode.E; // Tecla para entrar

    [Header("═══ EFECTO VISUAL ═══")]
    [SerializeField] private bool mostrarMensaje = true;
    [SerializeField] private string mensajeInteraccion = "Presiona E para entrar";
    [SerializeField] private TextMeshProUGUI textoUI; // Texto en pantalla
    [SerializeField] private GameObject efectoPortal; // Efecto visual del portal (opcional)

    [Header("═══ AUDIO ═══")]
    [SerializeField] private AudioClip sonidoPortal; // Sonido al entrar al portal

    private bool jugadorEnZona = false;
    private bool estaActivado = false;

    private void Start()
    {
        // Ocultar texto al inicio
        if (textoUI != null)
        {
            textoUI.gameObject.SetActive(false);
        }

        Debug.Log($"Portal configurado para: {nombreEscenaDestino}");
    }

    private void Update()
    {
        // Si el jugador está en la zona y requiere input
        if (jugadorEnZona && requiereInput && !estaActivado)
        {
            if (Input.GetKeyDown(teclaInteraccion))
            {
                ActivarPortal();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detectar si es el jugador
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Jugador entró en la zona del portal");
            jugadorEnZona = true;

            // Mostrar mensaje de interacción
            if (mostrarMensaje && textoUI != null)
            {
                textoUI.gameObject.SetActive(true);
                textoUI.text = mensajeInteraccion;
            }

            // Si NO requiere input, activar automáticamente
            if (!requiereInput && !estaActivado)
            {
                ActivarPortal();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // Si el jugador sale de la zona
        if (collision.CompareTag("Player"))
        {
            Debug.Log("Jugador salió de la zona del portal");
            jugadorEnZona = false;

            // Ocultar mensaje
            if (textoUI != null)
            {
                textoUI.gameObject.SetActive(false);
            }
        }
    }

    private void ActivarPortal()
    {
        if (estaActivado) return;

        estaActivado = true;
        Debug.Log($"¡Portal activado! Transportando a: {nombreEscenaDestino}");

        // Reproducir sonido
        if (sonidoPortal != null && AudioManager.instance != null)
        {
            AudioManager.instance.ReproducirEfecto(sonidoPortal);
        }

        // Ocultar mensaje
        if (textoUI != null)
        {
            textoUI.gameObject.SetActive(false);
        }

        // Activar efecto visual (opcional)
        if (efectoPortal != null)
        {
            efectoPortal.SetActive(true);
        }

        // Cambiar de escena con delay
        StartCoroutine(CambiarEscenaConDelay());
    }

    private IEnumerator CambiarEscenaConDelay()
    {
        // Esperar el delay
        yield return new WaitForSeconds(delayAntesCambiar);

        // Verificar que la escena existe
        if (Application.CanStreamedLevelBeLoaded(nombreEscenaDestino))
        {
            Debug.Log($"Cargando escena: {nombreEscenaDestino}");
            SceneManager.LoadScene(nombreEscenaDestino);
        }
        else
        {
            Debug.LogError($"ERROR: La escena '{nombreEscenaDestino}' no existe o no está en Build Settings!");
        }
    }

    // Método público para activar desde otros scripts
    public void ActivarPortalManualmente()
    {
        ActivarPortal();
    }

    private void OnDrawGizmos()
    {
        // Visualizar el área del trigger en el editor
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            Gizmos.color = new Color(0, 1, 1, 0.3f); // Cyan transparente
            
            if (col is BoxCollider2D)
            {
                BoxCollider2D box = (BoxCollider2D)col;
                Gizmos.matrix = transform.localToWorldMatrix;
                Gizmos.DrawCube(box.offset, box.size);
                Gizmos.DrawWireCube(box.offset, box.size);
            }
            else if (col is CircleCollider2D)
            {
                CircleCollider2D circle = (CircleCollider2D)col;
                Gizmos.DrawSphere(transform.position + (Vector3)circle.offset, circle.radius);
                Gizmos.DrawWireSphere(transform.position + (Vector3)circle.offset, circle.radius);
            }
        }
    }
}
