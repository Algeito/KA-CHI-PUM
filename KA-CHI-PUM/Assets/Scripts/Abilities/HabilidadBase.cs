using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================== ENUMS Y TIPOS ====================
public enum TipoHabilidad { KA, CHI, PUM }
public enum TipoElemento { Ninguno, Fuego, Agua, Tierra, Viento, Rayo }
public enum TipoAtaque { Proyectil, Area, Orbital }

// ==================== DATOS DE HABILIDAD ====================
[System.Serializable]
public class DatosHabilidad
{
    public TipoHabilidad tipo;
    public string nombre;
    public int nivelBase = 1;
    public int nivelMaximo = 5;
    public int nivelInverso = 10;
    public bool tieneInverso = false;

    [Header("Estadísticas Base")]
    public float dañoBase = 10f;
    public float velocidadAtaque = 1f; // Ataques por segundo
    public float alcance = 5f;
    public int proyectilesPorDisparo = 1;

    [Header("Mejora por Nivel")]
    public float incrementoDañoPorNivel = 2f;
    public float incrementoVelocidadPorNivel = 0.1f;
    public int incrementoProyectilesPorNivel = 0;

    [Header("Configuración Visual")]
    public Color colorHabilidad = Color.white;
    public Sprite spriteProyectil;
    public GameObject efectoImpacto;
}

// ==================== COMBINACIÓN ELEMENTAL ====================
[System.Serializable]
public class CombinacionElemental
{
    public TipoElemento elemento;
    public int nivelElemento = 0;
    public int nivelMaximoElemento = 3;

    [Header("Bonus Elemental")]
    public float multiplicadorDaño = 1.0f;
    public float probabilidadEfecto = 0f;
    public GameObject efectoVisual;

    // Efectos especiales por elemento
    public bool aplicaQuemadura = false;  // Fuego
    public bool aplicaCongelamiento = false; // Agua
    public bool aplicaAturdimiento = false; // Rayo
    public bool aplicaSangrado = false; // Tierra
    public bool aplicaEmpuje = false; // Viento
}

// ==================== ESTADÍSTICAS DE HABILIDAD ====================
public class EstadisticasHabilidad
{
    public TipoHabilidad tipo;
    public int nivelActual = 1;
    public bool inversoDesbloqueado = false;

    public float dañoTotal;
    public float velocidadAtaqueTotal;
    public float alcanceTotal;
    public int proyectilesTotales;

    public CombinacionElemental elementoActual;

    public void CalcularEstadisticas(DatosHabilidad datos)
    {
        int nivel = inversoDesbloqueado ? datos.nivelInverso : nivelActual;

        dañoTotal = datos.dañoBase + (datos.incrementoDañoPorNivel * (nivel - 1));
        velocidadAtaqueTotal = datos.velocidadAtaque + (datos.incrementoVelocidadPorNivel * (nivel - 1));
        alcanceTotal = datos.alcance;
        proyectilesTotales = datos.proyectilesPorDisparo + (datos.incrementoProyectilesPorNivel * (nivel - 1));

        // Aplicar bonus elemental
        if (elementoActual != null && elementoActual.nivelElemento > 0)
        {
            dañoTotal *= elementoActual.multiplicadorDaño;
        }
    }
}

// ==================== HABILIDAD BASE ====================
public abstract class HabilidadBase : MonoBehaviour
{
    [Header("Configuración")]
    public DatosHabilidad datosHabilidad;
    public EstadisticasHabilidad estadisticas;

    [Header("Referencias")]
    protected Transform jugador;
    protected Transform contenedorProyectiles;

    protected float tiempoUltimoAtaque;
    protected bool puedeAtacar = true;

    protected virtual void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;

        // Crear contenedor para proyectiles
        GameObject contenedor = new GameObject(datosHabilidad.nombre + "_Proyectiles");
        contenedorProyectiles = contenedor.transform;

        // Inicializar estadísticas
        estadisticas = new EstadisticasHabilidad();
        estadisticas.tipo = datosHabilidad.tipo;
        estadisticas.nivelActual = datosHabilidad.nivelBase;
        estadisticas.CalcularEstadisticas(datosHabilidad);

        StartCoroutine(AtaqueAutomatico());
    }

    protected IEnumerator AtaqueAutomatico()
    {
        while (true)
        {
            float intervalo = 1f / estadisticas.velocidadAtaqueTotal;
            yield return new WaitForSeconds(intervalo);

            if (puedeAtacar)
            {
                EjecutarAtaque();
            }
        }
    }

    protected abstract void EjecutarAtaque();

    public virtual void SubirNivel()
    {
        if (estadisticas.nivelActual < datosHabilidad.nivelMaximo)
        {
            estadisticas.nivelActual++;
            estadisticas.CalcularEstadisticas(datosHabilidad);
            Debug.Log($"{datosHabilidad.nombre} subió a nivel {estadisticas.nivelActual}");
        }
        else if (!estadisticas.inversoDesbloqueado && datosHabilidad.tieneInverso)
        {
            DesbloquearInverso();
        }
    }

    protected virtual void DesbloquearInverso()
    {
        estadisticas.inversoDesbloqueado = true;
        estadisticas.nivelActual = datosHabilidad.nivelInverso;
        estadisticas.CalcularEstadisticas(datosHabilidad);
        Debug.Log($"¡{datosHabilidad.nombre} INVERSO desbloqueado!");
    }

    public void AñadirElemento(TipoElemento elemento, int nivel)
    {
        if (estadisticas.elementoActual == null)
        {
            estadisticas.elementoActual = new CombinacionElemental();
        }

        estadisticas.elementoActual.elemento = elemento;
        estadisticas.elementoActual.nivelElemento = nivel;
        ConfigurarBonusElemental(elemento, nivel);
        estadisticas.CalcularEstadisticas(datosHabilidad);
    }

    protected virtual void ConfigurarBonusElemental(TipoElemento elemento, int nivel)
    {
        float bonus = 1.0f + (0.2f * nivel); // 20% por nivel
        estadisticas.elementoActual.multiplicadorDaño = bonus;
        estadisticas.elementoActual.probabilidadEfecto = 0.15f * nivel;

        switch (elemento)
        {
            case TipoElemento.Fuego:
                estadisticas.elementoActual.aplicaQuemadura = true;
                break;
            case TipoElemento.Agua:
                estadisticas.elementoActual.aplicaCongelamiento = true;
                break;
            case TipoElemento.Rayo:
                estadisticas.elementoActual.aplicaAturdimiento = true;
                break;
            case TipoElemento.Tierra:
                estadisticas.elementoActual.aplicaSangrado = true;
                break;
            case TipoElemento.Viento:
                estadisticas.elementoActual.aplicaEmpuje = true;
                break;
        }
    }

    protected Transform EncontrarEnemigoMasCercano()
    {
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        Transform objetivo = null;
        float distanciaMinima = estadisticas.alcanceTotal;

        foreach (GameObject enemigo in enemigos)
        {
            float distancia = Vector2.Distance(jugador.position, enemigo.transform.position);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                objetivo = enemigo.transform;
            }
        }

        return objetivo;
    }

    public DatosHabilidad ObtenerDatos() { return datosHabilidad; }
    public EstadisticasHabilidad ObtenerEstadisticas() { return estadisticas; }
}
