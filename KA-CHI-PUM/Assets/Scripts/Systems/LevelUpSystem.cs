using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================== SISTEMA DE LEVEL UP ====================
public class LevelUpSystem : MonoBehaviour
{
    [System.Serializable]
    public class OpcionMejora
    {
        public string nombre;
        public string descripcion;
        public Sprite icono;
        public TipoMejora tipo;
        public TipoHabilidad habilidadObjetivo;
        public TipoElemento elementoObjetivo;
        public int valorMejora;
    }

    public enum TipoMejora
    {
        SubirNivelHabilidad,
        AñadirElemento,
        MejorarEstadistica,
        NuevaHabilidad,
        DesbloquearInverso
    }

    [Header("Opciones Disponibles")]
    public List<OpcionMejora> todasLasMejoras = new List<OpcionMejora>();
    public int opcionesPorLevelUp = 3;

    [Header("Referencias")]
    public GameObject panelLevelUp;
    public GameObject prefabOpcionUI;
    public Transform contenedorOpciones;

    private PlayerStats playerStats;
    private List<HabilidadBase> habilidadesActivas = new List<HabilidadBase>();
    private bool juegoEnPausa = false;

    void Start()
    {
        playerStats = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStats>();

        if (panelLevelUp != null)
        {
            panelLevelUp.SetActive(false);
        }

        InicializarHabilidades();
    }

    void InicializarHabilidades()
    {
        // Buscar todas las habilidades activas del jugador
        HabilidadBase[] habilidades = FindObjectsOfType<HabilidadBase>();
        habilidadesActivas.AddRange(habilidades);
    }

    public void MostrarOpcionesMejora()
    {
        if (panelLevelUp == null) return;

        // Pausar el juego
        Time.timeScale = 0f;
        juegoEnPausa = true;
        panelLevelUp.SetActive(true);

        // Limpiar opciones anteriores
        foreach (Transform child in contenedorOpciones)
        {
            Destroy(child.gameObject);
        }

        // Generar opciones aleatorias
        List<OpcionMejora> opcionesDisponibles = ObtenerOpcionesAleatorias();

        foreach (OpcionMejora opcion in opcionesDisponibles)
        {
            CrearBotonOpcion(opcion);
        }
    }

    List<OpcionMejora> ObtenerOpcionesAleatorias()
    {
        List<OpcionMejora> opciones = new List<OpcionMejora>();
        List<OpcionMejora> pool = new List<OpcionMejora>(todasLasMejoras);

        // Filtrar opciones inválidas
        pool.RemoveAll(o => !EsOpcionValida(o));

        for (int i = 0; i < opcionesPorLevelUp && pool.Count > 0; i++)
        {
            int indice = UnityEngine.Random.Range(0, pool.Count);
            opciones.Add(pool[indice]);
            pool.RemoveAt(indice);
        }

        return opciones;
    }

    bool EsOpcionValida(OpcionMejora opcion)
    {
        switch (opcion.tipo)
        {
            case TipoMejora.SubirNivelHabilidad:
                HabilidadBase habilidad = EncontrarHabilidad(opcion.habilidadObjetivo);
                return habilidad != null && habilidad.ObtenerEstadisticas().nivelActual < habilidad.ObtenerDatos().nivelMaximo;

            case TipoMejora.DesbloquearInverso:
                HabilidadBase hab = EncontrarHabilidad(opcion.habilidadObjetivo);
                return hab != null && hab.ObtenerEstadisticas().nivelActual >= hab.ObtenerDatos().nivelMaximo && !hab.ObtenerEstadisticas().inversoDesbloqueado;

            default:
                return true;
        }
    }

    void CrearBotonOpcion(OpcionMejora opcion)
    {
        // Aquí crearías la UI del botón
        // Por simplicidad, solo registro la lógica
        Debug.Log($"Opción: {opcion.nombre} - {opcion.descripcion}");
    }

    public void SeleccionarMejora(OpcionMejora opcion)
    {
        switch (opcion.tipo)
        {
            case TipoMejora.SubirNivelHabilidad:
                HabilidadBase habilidad = EncontrarHabilidad(opcion.habilidadObjetivo);
                if (habilidad != null) habilidad.SubirNivel();
                break;

            case TipoMejora.AñadirElemento:
                HabilidadBase hab = EncontrarHabilidad(opcion.habilidadObjetivo);
                if (hab != null) hab.AñadirElemento(opcion.elementoObjetivo, opcion.valorMejora);
                break;

            case TipoMejora.MejorarEstadistica:
                if (playerStats != null)
                {
                    // Mejorar estadísticas del jugador
                    playerStats.ModificarVidaMaxima(opcion.valorMejora);
                }
                break;

            case TipoMejora.NuevaHabilidad:
                // Activar nueva habilidad
                break;

            case TipoMejora.DesbloquearInverso:
                HabilidadBase habInv = EncontrarHabilidad(opcion.habilidadObjetivo);
                if (habInv != null) habInv.SubirNivel(); // Esto desbloqueará el inverso
                break;
        }

        CerrarPanelLevelUp();
    }

    void CerrarPanelLevelUp()
    {
        if (panelLevelUp != null)
        {
            panelLevelUp.SetActive(false);
        }

        Time.timeScale = 1f;
        juegoEnPausa = false;
    }

    HabilidadBase EncontrarHabilidad(TipoHabilidad tipo)
    {
        foreach (HabilidadBase hab in habilidadesActivas)
        {
            if (hab.ObtenerDatos().tipo == tipo)
            {
                return hab;
            }
        }
        return null;
    }
}
