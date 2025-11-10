using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================== SISTEMA DE EXPERIENCIA DEL JUGADOR ====================
public class PlayerExperience : MonoBehaviour
{
    [Header("Experiencia en Partida")]
    public int nivelActual = 1;
    public int experienciaActual = 0;
    public int experienciaParaSiguienteNivel = 100;
    public float multiplicadorExpRequerida = 1.2f;

    [Header("Experiencia Permanente")]
    public int nivelPermanente = 1;
    public int experienciaPermanente = 0;
    public int expPermanenteParaSiguienteNivel = 1000;

    [Header("Bonificaciones por Nivel Permanente")]
    public float bonusVidaPorNivel = 5f;
    public float bonusDañoPorNivel = 2f;
    public float bonusVelocidadPorNivel = 0.05f;

    // Eventos
    public event Action OnLevelUp;
    public event Action<int> OnPermanentLevelUp;
    public event Action<int, int> OnExperienceChanged;

    private LevelUpSystem levelUpSystem;

    void Start()
    {
        levelUpSystem = FindObjectOfType<LevelUpSystem>();
        CargarProgresoPermanente();
    }

    public void AñadirExperiencia(int cantidad)
    {
        experienciaActual += cantidad;
        experienciaPermanente += cantidad;

        OnExperienceChanged?.Invoke(experienciaActual, experienciaParaSiguienteNivel);

        // Verificar level up en partida
        while (experienciaActual >= experienciaParaSiguienteNivel)
        {
            SubirNivel();
        }

        // Verificar level up permanente
        while (experienciaPermanente >= expPermanenteParaSiguienteNivel)
        {
            SubirNivelPermanente();
        }
    }

    void SubirNivel()
    {
        experienciaActual -= experienciaParaSiguienteNivel;
        nivelActual++;

        // Aumentar exp requerida
        experienciaParaSiguienteNivel = Mathf.RoundToInt(experienciaParaSiguienteNivel * multiplicadorExpRequerida);

        Debug.Log($"¡LEVEL UP! Nivel {nivelActual}");

        OnLevelUp?.Invoke();

        // Mostrar opciones de mejora
        if (levelUpSystem != null)
        {
            levelUpSystem.MostrarOpcionesMejora();
        }
    }

    void SubirNivelPermanente()
    {
        experienciaPermanente -= expPermanenteParaSiguienteNivel;
        nivelPermanente++;

        expPermanenteParaSiguienteNivel = Mathf.RoundToInt(expPermanenteParaSiguienteNivel * 1.5f);

        Debug.Log($"¡NIVEL PERMANENTE! Nivel {nivelPermanente}");

        OnPermanentLevelUp?.Invoke(nivelPermanente);

        GuardarProgresoPermanente();
        AplicarBonusPermanentes();
    }

    void AplicarBonusPermanentes()
    {
        PlayerStats stats = GetComponent<PlayerStats>();
        if (stats != null)
        {
            float bonusVida = bonusVidaPorNivel * (nivelPermanente - 1);
            float bonusDaño = bonusDañoPorNivel * (nivelPermanente - 1);
            float bonusVelocidad = bonusVelocidadPorNivel * (nivelPermanente - 1);

            stats.ModificarVidaMaxima(bonusVida);
            stats.ModificarDañoBase(bonusDaño);
            stats.ModificarVelocidad(bonusVelocidad);
        }
    }

    void GuardarProgresoPermanente()
    {
        PlayerPrefs.SetInt("NivelPermanente", nivelPermanente);
        PlayerPrefs.SetInt("ExperienciaPermanente", experienciaPermanente);
        PlayerPrefs.Save();
    }

    void CargarProgresoPermanente()
    {
        nivelPermanente = PlayerPrefs.GetInt("NivelPermanente", 1);
        experienciaPermanente = PlayerPrefs.GetInt("ExperienciaPermanente", 0);

        AplicarBonusPermanentes();
    }

    // Getters
    public int ObtenerNivel() { return nivelActual; }
    public int ObtenerNivelPermanente() { return nivelPermanente; }
    public float ObtenerProgresoExp() { return (float)experienciaActual / experienciaParaSiguienteNivel; }
}
