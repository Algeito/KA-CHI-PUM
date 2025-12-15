using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VidaJugadorUI : MonoBehaviour
{
    [Header("Referencias UI")]
    [SerializeField] private Image barraVida;
    [SerializeField] private Image barraFondo;
    [SerializeField] private TextMeshProUGUI textoVida;
    
    [Header("Colores")]
    [SerializeField] private Color colorVidaAlta = Color.green;
    [SerializeField] private Color colorVidaMedia = Color.yellow;
    [SerializeField] private Color colorVidaBaja = Color.red;
    [SerializeField] private float umbralVidaMedia = 0.5f;
    [SerializeField] private float umbralVidaBaja = 0.25f;
    
    [Header("Animación")]
    [SerializeField] private float velocidadAnimacion = 5f;
    [SerializeField] private bool animarCambios = true;
    
    [Header("Game Over")]
    [SerializeField] private GameObject panelGameOver;
    
    private float vidaObjetivo;
    private float vidaActualAnimada;

    private void Start()
    {
        // Buscar al jugador y obtener su vida inicial
        PlayerController jugador = FindObjectOfType<PlayerController>();
        if (jugador != null)
        {
            ActualizarVida(jugador.ObtenerVidaActual(), jugador.ObtenerVidaMaxima());
        }
        
        // Ocultar panel de game over
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }
    }

    private void Update()
    {
        // Animar cambios de vida suavemente
        if (animarCambios && barraVida != null)
        {
            vidaActualAnimada = Mathf.Lerp(vidaActualAnimada, vidaObjetivo, Time.deltaTime * velocidadAnimacion);
            barraVida.fillAmount = vidaActualAnimada;
        }
    }

    public void ActualizarVida(int vidaActual, int vidaMaxima)
    {
        if (barraVida == null)
        {
            Debug.LogWarning("VidaJugadorUI: Falta asignar la barra de vida");
            return;
        }

        // Calcular porcentaje
        float porcentaje = (float)vidaActual / vidaMaxima;
        vidaObjetivo = porcentaje;
        
        if (!animarCambios)
        {
            barraVida.fillAmount = porcentaje;
            vidaActualAnimada = porcentaje;
        }

        // Actualizar color según vida restante
        ActualizarColor(porcentaje);

        // Actualizar texto si existe
        if (textoVida != null)
        {
            textoVida.text = $"{vidaActual} / {vidaMaxima}";
        }

        Debug.Log($"UI Vida actualizada: {vidaActual}/{vidaMaxima} ({porcentaje * 100}%)");
    }

    private void ActualizarColor(float porcentaje)
    {
        if (barraVida == null) return;

        Color nuevoColor;
        
        if (porcentaje <= umbralVidaBaja)
        {
            nuevoColor = colorVidaBaja;
        }
        else if (porcentaje <= umbralVidaMedia)
        {
            nuevoColor = colorVidaMedia;
        }
        else
        {
            nuevoColor = colorVidaAlta;
        }

        barraVida.color = nuevoColor;
    }

    public void OnJugadorMuerto()
    {
        Debug.Log("Jugador ha muerto - Mostrando Game Over");
        
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
    }
}
