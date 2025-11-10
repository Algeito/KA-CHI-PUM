// ==================== SPAWNER DE ENEMIGOS ====================
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Spawn")]
    public GameObject[] prefabsEnemigos; // 0: Normal, 1: 1 habilidad, 2: 2 habilidades, 3: 3 habilidades, 4: Inverso
    public GameObject prefabJefe;

    [Header("Spawn Continuo")]
    public float intervaloSpawnBase = 2f;
    public int enemigosSimultaneos = 20;
    public float radioSpawn = 15f;

    [Header("Oleadas")]
    public bool usarOleadas = true;
    public float intervaloEntreOleadas = 30f;
    public int enemigosBaseOleada = 50;
    public float incrementoOleada = 1.5f;

    [Header("Jefes")]
    public float intervaloJefes = 120f; // Cada 2 minutos

    // Control
    private Transform jugador;
    private List<EnemigoBase> enemigosActivos = new List<EnemigoBase>();
    private int oleadaActual = 0;
    private float tiempoUltimoSpawn;
    private float tiempoUltimaOleada;
    private float tiempoUltimoJefe;
    private int enemigosEnOleada = 0;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
        tiempoUltimaOleada = Time.time;
        tiempoUltimoJefe = Time.time;

        StartCoroutine(SpawnContinuo());

        if (usarOleadas)
        {
            StartCoroutine(SistemaOleadas());
        }

        StartCoroutine(SpawnJefes());
    }

    IEnumerator SpawnContinuo()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloSpawnBase);

            if (enemigosActivos.Count < enemigosSimultaneos)
            {
                SpawnearEnemigo(ObtenerTipoEnemigoPorTiempo());
            }
        }
    }

    IEnumerator SistemaOleadas()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloEntreOleadas);

            oleadaActual++;
            IniciarOleada();
        }
    }

    void IniciarOleada()
    {
        int cantidadEnemigos = Mathf.RoundToInt(enemigosBaseOleada * Mathf.Pow(incrementoOleada, oleadaActual - 1));
        enemigosEnOleada = cantidadEnemigos;

        Debug.Log($"¡OLEADA {oleadaActual}! - {cantidadEnemigos} enemigos");

        StartCoroutine(SpawnearOleada(cantidadEnemigos));
    }

    IEnumerator SpawnearOleada(int cantidad)
    {
        for (int i = 0; i < cantidad; i++)
        {
            SpawnearEnemigo(ObtenerTipoEnemigoPorOleada());
            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator SpawnJefes()
    {
        while (true)
        {
            yield return new WaitForSeconds(intervaloJefes);

            if (prefabJefe != null)
            {
                Debug.Log("¡JEFE APARECE!");
                SpawnearEnemigo(prefabJefe, true);
            }
        }
    }

    void SpawnearEnemigo(GameObject prefab, bool esJefe = false)
    {
        if (jugador == null || prefab == null) return;

        Vector2 posicionSpawn = ObtenerPosicionSpawnAleatoria();
        GameObject enemigo = Instantiate(prefab, posicionSpawn, Quaternion.identity);

        EnemigoBase scriptEnemigo = enemigo.GetComponent<EnemigoBase>();
        if (scriptEnemigo != null)
        {
            enemigosActivos.Add(scriptEnemigo);
        }
    }

    Vector2 ObtenerPosicionSpawnAleatoria()
    {
        Vector2 direccion = Random.insideUnitCircle.normalized;
        return (Vector2)jugador.position + direccion * radioSpawn;
    }

    GameObject ObtenerTipoEnemigoPorTiempo()
    {
        float tiempoJuego = Time.time;

        if (tiempoJuego < 60f) // Primer minuto: solo normales
        {
            return prefabsEnemigos[0];
        }
        else if (tiempoJuego < 180f) // 1-3 minutos: normales y 1 habilidad
        {
            return prefabsEnemigos[Random.Range(0, 2)];
        }
        else if (tiempoJuego < 300f) // 3-5 minutos: hasta 2 habilidades
        {
            return prefabsEnemigos[Random.Range(0, 3)];
        }
        else if (tiempoJuego < 600f) // 5-10 minutos: hasta 3 habilidades
        {
            return prefabsEnemigos[Random.Range(0, 4)];
        }
        else // 10+ minutos: incluir inversos
        {
            return prefabsEnemigos[Random.Range(0, prefabsEnemigos.Length)];
        }
    }

    GameObject ObtenerTipoEnemigoPorOleada()
    {
        if (oleadaActual <= 2)
        {
            return prefabsEnemigos[Random.Range(0, 2)];
        }
        else if (oleadaActual <= 5)
        {
            return prefabsEnemigos[Random.Range(0, 3)];
        }
        else
        {
            return prefabsEnemigos[Random.Range(0, Mathf.Min(prefabsEnemigos.Length, 4))];
        }
    }

    public void EnemigoMuerto(EnemigoBase enemigo)
    {
        enemigosActivos.Remove(enemigo);

        if (usarOleadas && enemigosEnOleada > 0)
        {
            enemigosEnOleada--;

            if (enemigosEnOleada <= 0)
            {
                Debug.Log($"¡Oleada {oleadaActual} completada!");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (jugador != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(jugador.position, radioSpawn);
        }
    }
}

