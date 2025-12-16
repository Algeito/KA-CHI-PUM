using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseManager : MonoBehaviour
{
    [Header("═══ PANEL PAUSA ═══")]
    [SerializeField] private GameObject panelPausa;
    [SerializeField] private TextMeshProUGUI textoPausa; // Opcional

    [Header("═══ BOTONES ═══")]
    [SerializeField] private Button botonReanudar;
    [SerializeField] private Button botonMenu;
    [SerializeField] private Button botonSalir;

    [Header("═══ CONFIGURACIÓN ═══")]
    [SerializeField] private KeyCode teclaPausa = KeyCode.Escape;
    [SerializeField] private string nombreEscenaMenu = "MenuPrincipal";
    [SerializeField] private bool permitirPausaEnGameOver = false;

    private bool estaPausado = false;
    private GameOverManager gameOverManager;

    private void Start()
    {
        // Ocultar panel al inicio
        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        // Buscar GameOverManager
        gameOverManager = FindObjectOfType<GameOverManager>();

        // Configurar botones
        ConfigurarBotones();
    }

    private void Update()
    {
        // Detectar tecla ESC
        if (Input.GetKeyDown(teclaPausa))
        {
            if (estaPausado)
            {
                Reanudar();
            }
            else
            {
                Pausar();
            }
        }
    }

    private void ConfigurarBotones()
    {
        if (botonReanudar != null)
        {
            botonReanudar.onClick.AddListener(Reanudar);
        }

        if (botonMenu != null)
        {
            botonMenu.onClick.AddListener(IrAlMenu);
        }

        if (botonSalir != null)
        {
            botonSalir.onClick.AddListener(SalirDelJuego);
        }
    }

    public void Pausar()
    {
        // No pausar si el Game Over está activo
        if (!permitirPausaEnGameOver && Time.timeScale == 0f)
        {
            Debug.Log("No se puede pausar durante Game Over");
            return;
        }

        estaPausado = true;
        Time.timeScale = 0f; // Pausar el juego

        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }

        // Reproducir sonido
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ReproducirSonidoBoton();
        }

        Debug.Log("Juego pausado");
    }

    public void Reanudar()
    {
        estaPausado = false;
        Time.timeScale = 1f; // Reanudar el juego

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }

        // Reproducir sonido
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ReproducirSonidoBoton();
        }

        Debug.Log("Juego reanudado");
    }

    public void IrAlMenu()
    {
        Debug.Log("Volviendo al menú desde pausa...");

        // Reproducir sonido
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ReproducirSonidoBoton();
        }

        // Reanudar el tiempo
        Time.timeScale = 1f;

        // Cargar menú
        SceneManager.LoadScene(nombreEscenaMenu);
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego desde pausa...");

        // Reproducir sonido
        if (AudioManager.instance != null)
        {
            AudioManager.instance.ReproducirSonidoBoton();
        }

        // Reanudar el tiempo
        Time.timeScale = 1f;

        // Salir
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public bool EstaPausado()
    {
        return estaPausado;
    }
}
