using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Configuración de Enemigos")]
    [SerializeField] private GameObject[] prefabsEnemigos;
    [SerializeField] private int maxEnemigosSimultaneos = 10;
    
    [Header("Configuración de Spawn")]
    [SerializeField] private float tiempoEntreSpawns = 3f;
    [SerializeField] private bool spawnerActivo = true;
    
    [Header("Sistema de Puntos de Spawn")]
    [SerializeField] private CameraSpawnPoints sistemaSpawnPoints;
    [SerializeField] private bool usarSpawnFueraDeCamara = true;
    [SerializeField] private float rangoVariacionSpawn = 2f; // Radio alrededor del punto
    
    [Header("Oleadas (Opcional)")]
    [SerializeField] private bool usarOleadas = false;
    [SerializeField] private int enemigosporOleada = 5;
    [SerializeField] private float tiempoEntreOleadas = 10f;
    
    private List<GameObject> enemigosActivos = new List<GameObject>();
    private int oleadaActual = 0;

    private void Start()
    {
        // Buscar CameraSpawnPoints si no está asignado
        if (sistemaSpawnPoints == null)
        {
            sistemaSpawnPoints = FindObjectOfType<CameraSpawnPoints>();
            
            if (sistemaSpawnPoints == null && usarSpawnFueraDeCamara)
            {
                Debug.LogError("EnemySpawner: No se encontró CameraSpawnPoints. Desactivando spawn fuera de cámara.");
                usarSpawnFueraDeCamara = false;
            }
        }

        if (prefabsEnemigos == null || prefabsEnemigos.Length == 0)
        {
            Debug.LogError("EnemySpawner: ¡No hay prefabs de enemigos asignados!");
            spawnerActivo = false;
            return;
        }

        if (spawnerActivo)
        {
            if (usarOleadas)
            {
                StartCoroutine(SistemaOleadas());
            }
            else
            {
                StartCoroutine(SpawnContinuo());
            }
        }
    }

    private void Update()
    {
        enemigosActivos.RemoveAll(enemigo => enemigo == null);
    }

    private IEnumerator SpawnContinuo()
    {
        while (spawnerActivo)
        {
            if (enemigosActivos.Count < maxEnemigosSimultaneos)
            {
                SpawnearEnemigo();
            }

            yield return new WaitForSeconds(tiempoEntreSpawns);
        }
    }

    private IEnumerator SistemaOleadas()
    {
        while (spawnerActivo)
        {
            oleadaActual++;
            Debug.Log($"¡Oleada {oleadaActual}!");

            for (int i = 0; i < enemigosporOleada; i++)
            {
                SpawnearEnemigo();
                yield return new WaitForSeconds(0.5f);
            }

            yield return new WaitForSeconds(tiempoEntreOleadas);
        }
    }

    private void SpawnearEnemigo()
    {
        Vector2 posicionSpawn = ObtenerPosicionSpawn();
        
        GameObject prefabEnemigo = prefabsEnemigos[Random.Range(0, prefabsEnemigos.Length)];
        GameObject nuevoEnemigo = Instantiate(prefabEnemigo, posicionSpawn, Quaternion.identity);
        
        enemigosActivos.Add(nuevoEnemigo);
        
        Debug.Log($"Enemigo spawneado fuera de cámara en {posicionSpawn}");
    }

    private Vector2 ObtenerPosicionSpawn()
    {
        if (usarSpawnFueraDeCamara && sistemaSpawnPoints != null)
        {
            // Obtener punto fuera de cámara con variación
            return sistemaSpawnPoints.ObtenerPuntoAleatorioEnAreaFueraDeCamara(rangoVariacionSpawn);
        }
        else
        {
            // Fallback: spawn alrededor del spawner
            return (Vector2)transform.position + Random.insideUnitCircle * 10f;
        }
    }

    // Métodos públicos
    public void ActivarSpawner() { spawnerActivo = true; }
    public void DesactivarSpawner() { spawnerActivo = false; }
    public int ObtenerCantidadEnemigosActivos() { return enemigosActivos.Count; }
}
