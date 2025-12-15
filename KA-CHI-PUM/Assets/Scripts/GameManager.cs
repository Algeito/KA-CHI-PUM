using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelGameOver;
    
    [Header("Configuración")]
    [SerializeField] private bool pausarAlMorir = true;
    [SerializeField] private float tiempoAntesDeGameOver = 1f;
    
    // Singleton para acceso global
    public static GameManager Instance { get; private set; }

    private bool juegoTerminado = false;

    private void Awake()
    {
        // Patrón Singleton
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Ocultar panel de game over al inicio
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(false);
        }
        
        // Asegurar que el tiempo esté corriendo
        Time.timeScale = 1f;
    }

    public void OnJugadorMurio()
    {
        if (juegoTerminado) return;
        
        juegoTerminado = true;
        Debug.Log("GameManager: Jugador ha muerto");
        
        // Pausar el juego si está configurado
        if (pausarAlMorir)
        {
            Invoke(nameof(PausarJuego), tiempoAntesDeGameOver);
        }
        
        // Mostrar Game Over
        Invoke(nameof(MostrarGameOver), tiempoAntesDeGameOver);
    }

    private void PausarJuego()
    {
        Time.timeScale = 0f;
    }

    private void MostrarGameOver()
    {
        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
        }
    }

    // Métodos públicos para botones
    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        juegoTerminado = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        juegoTerminado = false;
        SceneManager.LoadScene("Lobby"); // Cambia al nombre de tu escena de menú
    }

    public void SalirDelJuego()
    {
        Debug.Log("Saliendo del juego...");
        Application.Quit();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    // Getters
    public bool JuegoTerminado() => juegoTerminado;
}
