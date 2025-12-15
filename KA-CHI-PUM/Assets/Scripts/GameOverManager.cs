using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameOverManager : MonoBehaviour
{
    [Header("═══ PANEL GAME OVER ═══")]
    [SerializeField] private GameObject panelGameOver;
    [SerializeField] public TextMeshProUGUI textoGameOver; // Opcional: para mostrar estadísticas
    
    [Header("═══ BOTONES ═══")]
    [SerializeField] private Button botonReiniciar;
    [SerializeField] private Button botonMenu;
    [SerializeField] private Button botonSalir;

    [Header("═══ CONFIGURACIÓN ═══")]
    [SerializeField] private string nombreEscenaActual = "Nivel_1";
    [SerializeField] private string nombreEscenaMenu = "Lobby";
    [SerializeField] private float delayAntesMostrar = 1f; // Delay antes de mostrar el panel
    [SerializeField] private bool pausarJuego = true;

    [Header("═══ AUDIO (Opcional) ═══")]
    [SerializeField] private AudioClip sonidoGameOver;

    private bool gameOverActivo = false;

    private void Start()
    {
        // Asegurarse de que el panel esté oculto al inicio
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }

        // Configurar botones
        ConfigurarBotones();
    }

    private void ConfigurarBotones()
    {
        if (botonReiniciar != null)
        {
            botonReiniciar.onClick.AddListener(Reiniciar);
        }

        if (botonMenu != null)
        {
            botonMenu.onClick.AddListener(VolverAlMenu);
        }

        if (botonSalir != null)
        {
            botonSalir.onClick.AddListener(SalirDelJuego);
        }
    }

    public void MostrarGameOver()
    {
        if (gameOverActivo) return;

        StartCoroutine(MostrarGameOverCoroutine());
    }

    private IEnumerator MostrarGameOverCoroutine()
    {
        gameOverActivo = true;

        // Esperar un poco antes de mostrar
        yield return new WaitForSeconds(delayAntesMostrar);

        // Reproducir sonido
        if (sonidoGameOver != null && AudioManager.instance != null)
        {
            AudioManager.instance.ReproducirEfecto(sonidoGameOver);
        }

        // Mostrar panel
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }

        // Pausar el juego
        if (pausarJuego)
        {
            Time.timeScale = 0f;
        }

        Debug.Log("Game Over mostrado");
    }

    public void Reiniciar()
    {
        Debug.Log("Reiniciando nivel...");
        
        // Reproducir sonido de botón
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ReproducirSonidoBoton();
        }

        // Reanudar el tiempo
        Time.timeScale = 1f;

        // Recargar la escena actual
        SceneManager.LoadScene(nombreEscenaActual);
    }

    public void VolverAlMenu()
    {
        Debug.Log("Volviendo al menú...");
        
        // Reproducir sonido de botón
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ReproducirSonidoBoton();
        }

        // Reanudar el tiempo
        Time.timeScale = 1f;

        // Cargar escena del menú
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        
        // Reproducir sonido de botón
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ReproducirSonidoBoton();
        }

        // Reanudar el tiempo (por si acaso)
        Time.timeScale = 1f;

        // Salir del juego
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    // Método opcional para actualizar texto con estadísticas
    public void ActualizarTextoGameOver(string texto)
    {
        if (textoGameOver != null)
        {
            textoGameOver.text = texto;
        }
    }
}
