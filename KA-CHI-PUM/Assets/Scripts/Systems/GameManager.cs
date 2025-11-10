using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// ==================== GAME MANAGER ==================== //
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Estado del Juego")]
    public bool juegoActivo = false;
    public bool juegoPausado = false;
    public float tiempoDeJuego = 0f;

    [Header("Estadísticas")]
    public int enemigosEliminados = 0;
    public int dañoTotalInfligido = 0;
    public int dañoTotalRecibido = 0;

    [Header("Paneles UI")]
    public GameObject panelPausa;
    public GameObject panelGameOver;
    public GameObject panelVictoria;

    [Header("Referencias")]
    private PlayerStats playerStats;
    private PlayerExperience playerExperience;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();
        playerExperience = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerExperience>();

        if (playerStats != null)
        {
            playerStats.OnMuerte += GameOver;
        }

        IniciarJuego();
    }

    void Update()
    {
        if (juegoActivo && !juegoPausado)
        {
            tiempoDeJuego += Time.deltaTime;
        }

        // Input de pausa
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
                ReanudarJuego();
            else
                PausarJuego();
        }
    }

    public void IniciarJuego()
    {
        juegoActivo = true;
        juegoPausado = false;
        Time.timeScale = 1f;
        tiempoDeJuego = 0f;

        if (panelPausa != null) panelPausa.SetActive(false);
        if (panelGameOver != null) panelGameOver.SetActive(false);
        if (panelVictoria != null) panelVictoria.SetActive(false);
    }

    public void PausarJuego()
    {
        juegoPausado = true;
        Time.timeScale = 0f;

        if (panelPausa != null)
        {
            panelPausa.SetActive(true);
        }
    }

    public void ReanudarJuego()
    {
        juegoPausado = false;
        Time.timeScale = 1f;

        if (panelPausa != null)
        {
            panelPausa.SetActive(false);
        }
    }

    public void GameOver()
    {
        juegoActivo = false;
        Time.timeScale = 0f;

        if (panelGameOver != null)
        {
            panelGameOver.SetActive(true);
            ActualizarEstadisticasFinales();
        }
    }

    public void Victoria()
    {
        juegoActivo = false;
        Time.timeScale = 0f;

        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
            ActualizarEstadisticasFinales();
        }
    }

    void ActualizarEstadisticasFinales()
    {
        // Actualizar UI con estadísticas finales
        Debug.Log($"Tiempo: {FormatearTiempo(tiempoDeJuego)}");
        Debug.Log($"Enemigos eliminados: {enemigosEliminados}");
        Debug.Log($"Nivel alcanzado: {playerExperience?.ObtenerNivel()}");
    }

    public void ReiniciarNivel()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MenuPrincipal"); // Cambia por el nombre de tu escena
    }

    public void RegistrarEnemigoEliminado()
    {
        enemigosEliminados++;
    }

    public string FormatearTiempo(float tiempo)
    {
        int minutos = Mathf.FloorToInt(tiempo / 60f);
        int segundos = Mathf.FloorToInt(tiempo % 60f);
        return string.Format("{0:00}:{1:00}", minutos, segundos);
    }
}
