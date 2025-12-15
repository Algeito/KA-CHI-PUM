using UnityEngine;
using System;

public class SistemaExperiencia : MonoBehaviour
{
    [Header("Configuración de Experiencia")]
    public int nivelActual = 1;
    public int experienciaActual = 0;
    public int experienciaParaSiguienteNivel = 100;
    public float multiplicadorExpPorNivel = 1.5f; // Cuánto aumenta la exp necesaria por nivel

    [Header("Estadísticas por Nivel")]
    public int vidaBasePorNivel = 10; // Cuánta vida aumenta por nivel
    public int danioBasePorNivel = 2; // Cuánto daño aumenta por nivel

    // Eventos para notificar cambios
    public event Action<int, int, int> OnExperienciaGanada; // exp actual, exp necesaria, nivel
    public event Action<int> OnSubidaNivel; // nuevo nivel

    private PlayerController playerController;
    private int experienciaTotal = 0;

    void Start()
    {
        playerController = GetComponent<PlayerController>();

        // Notificar estado inicial
        NotificarCambioExperiencia();
    }

    public void GanarExperiencia(int cantidad)
    {
        experienciaActual += cantidad;
        experienciaTotal += cantidad;

        Debug.Log($"¡Ganaste {cantidad} EXP! Total: {experienciaActual}/{experienciaParaSiguienteNivel}");

        // Verificar si subió de nivel
        while (experienciaActual >= experienciaParaSiguienteNivel)
        {
            SubirNivel();
        }

        // Notificar cambios
        NotificarCambioExperiencia();
    }

    void SubirNivel()
    {
        // Restar la experiencia necesaria
        experienciaActual -= experienciaParaSiguienteNivel;

        // Aumentar nivel
        nivelActual++;

        // Calcular nueva experiencia necesaria
        experienciaParaSiguienteNivel = Mathf.RoundToInt(experienciaParaSiguienteNivel * multiplicadorExpPorNivel);

        Debug.Log($"¡SUBISTE DE NIVEL! Ahora eres nivel {nivelActual}");

        // Mejorar estadísticas del jugador
        MejorarEstadisticas();

        // Notificar subida de nivel
        OnSubidaNivel?.Invoke(nivelActual);
    }

    void MejorarEstadisticas()
    {
        if (playerController != null)
        {
            // Aumentar vida máxima
            int nuevaVidaMaxima = playerController.vidaMaxima + vidaBasePorNivel;
            int diferenciaVida = nuevaVidaMaxima - playerController.vidaMaxima;

            playerController.vidaMaxima = nuevaVidaMaxima;

            // Curar al jugador al subir de nivel
            playerController.Curar(diferenciaVida);

            // Aumentar daño de ataque
            playerController.danioAtaque += danioBasePorNivel;

            Debug.Log($"Estadísticas mejoradas: Vida={playerController.vidaMaxima}, Daño={playerController.danioAtaque}");
        }
    }

    void NotificarCambioExperiencia()
    {
        OnExperienciaGanada?.Invoke(experienciaActual, experienciaParaSiguienteNivel, nivelActual);
    }

    // Métodos públicos para obtener información
    public int ObtenerNivelActual() => nivelActual;
    public int ObtenerExperienciaActual() => experienciaActual;
    public int ObtenerExperienciaNecesaria() => experienciaParaSiguienteNivel;
    public int ObtenerExperienciaTotal() => experienciaTotal;
    public float ObtenerProgresoNivel() => (float)experienciaActual / experienciaParaSiguienteNivel;
}