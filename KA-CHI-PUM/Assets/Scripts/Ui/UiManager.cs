using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

// ==================== UI MANAGER ====================
public class UIManager : MonoBehaviour
{
    [Header("Barras")]
    public Image barraVida;
    public Image barraExperiencia;

    [Header("Textos")]
    public TextMeshProUGUI textoNivel;
    public TextMeshProUGUI textoTiempo;
    public TextMeshProUGUI textoEnemigos;
    public TextMeshProUGUI textoVida;

    [Header("Estadísticas en Pantalla")]
    public TextMeshProUGUI textoEstadisticasKA;
    public TextMeshProUGUI textoEstadisticasCHI;
    public TextMeshProUGUI textoEstadisticasPUM;

    private PlayerStats playerStats;
    private PlayerExperience playerExperience;
    private GameManager gameManager;

    void Start()
    {
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");

        if (jugador != null)
        {
            playerStats = jugador.GetComponent<PlayerStats>();
            playerExperience = jugador.GetComponent<PlayerExperience>();
        }

        gameManager = GameManager.Instance;

        // Suscribirse a eventos
        if (playerStats != null)
        {
            playerStats.OnVidaCambiada += ActualizarBarraVida;
        }

        if (playerExperience != null)
        {
            playerExperience.OnExperienceChanged += ActualizarBarraExperiencia;
            playerExperience.OnLevelUp += ActualizarNivel;
        }
    }

    void Update()
    {
        ActualizarTiempo();
        ActualizarEnemigos();
        ActualizarEstadisticasHabilidades();
    }

    void ActualizarBarraVida(float vidaActual, float vidaMaxima)
    {
        if (barraVida != null)
        {
            barraVida.fillAmount = vidaActual / vidaMaxima;
        }

        if (textoVida != null)
        {
            textoVida.text = $"{Mathf.CeilToInt(vidaActual)} / {Mathf.CeilToInt(vidaMaxima)}";
        }
    }

    void ActualizarBarraExperiencia(int expActual, int expRequerida)
    {
        if (barraExperiencia != null)
        {
            barraExperiencia.fillAmount = (float)expActual / expRequerida;
        }
    }

    void ActualizarNivel()
    {
        if (textoNivel != null && playerExperience != null)
        {
            textoNivel.text = $"Nivel {playerExperience.ObtenerNivel()}";
        }
    }

    void ActualizarTiempo()
    {
        if (textoTiempo != null && gameManager != null)
        {
            textoTiempo.text = gameManager.FormatearTiempo(gameManager.tiempoDeJuego);
        }
    }

    void ActualizarEnemigos()
    {
        if (textoEnemigos != null && gameManager != null)
        {
            textoEnemigos.text = $"Eliminados: {gameManager.enemigosEliminados}";
        }
    }

    void ActualizarEstadisticasHabilidades()
    {
        PlayerAbilityManager abilityManager = GameObject.FindGameObjectWithTag("Player")?.GetComponent<PlayerAbilityManager>();

        if (abilityManager == null) return;

        // KA
        HabilidadBase ka = abilityManager.ObtenerHabilidad(TipoHabilidad.KA);
        if (ka != null && textoEstadisticasKA != null)
        {
            EstadisticasHabilidad stats = ka.ObtenerEstadisticas();
            textoEstadisticasKA.text = $"KA Lv.{stats.nivelActual}\nDaño: {stats.dañoTotal:F0}";
        }

        // CHI
        HabilidadBase chi = abilityManager.ObtenerHabilidad(TipoHabilidad.CHI);
        if (chi != null && textoEstadisticasCHI != null)
        {
            EstadisticasHabilidad stats = chi.ObtenerEstadisticas();
            textoEstadisticasCHI.text = $"CHI Lv.{stats.nivelActual}\nDaño: {stats.dañoTotal:F0}";
        }

        // PUM
        HabilidadBase pum = abilityManager.ObtenerHabilidad(TipoHabilidad.PUM);
        if (pum != null && textoEstadisticasPUM != null)
        {
            EstadisticasHabilidad stats = pum.ObtenerEstadisticas();
            textoEstadisticasPUM.text = $"PUM Lv.{stats.nivelActual}\nDaño: {stats.dañoTotal:F0}";
        }
    }
}
