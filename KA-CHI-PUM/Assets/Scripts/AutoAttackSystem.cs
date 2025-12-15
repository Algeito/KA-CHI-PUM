using System.Collections;
using UnityEngine;

public class AutoAttack : MonoBehaviour
{
    [Header("Configuración de Disparo")]
    public GameObject prefabProyectil; // IMPORTANTE: Arrastra aquí el prefab del proyectil
    public Transform puntoDisparo; // Punto desde donde se dispara
    public float rangoDeteccion = 5f;
    public float tiempoEntreDisparos = 1f;
    public float velocidadProyectil = 10f;
    public LayerMask capaEnemigos;
    
    private Transform enemigoObjetivo;
    private bool puedeDisparar = true;

    void Update()
    {
        // Buscar enemigos cercanos
        BuscarEnemigoMasCercano();
        
        // Disparar automáticamente si hay un enemigo en rango
        if (enemigoObjetivo != null && puedeDisparar)
        {
            DispararProyectil();
        }
    }

    void BuscarEnemigoMasCercano()
    {
        // Detectar todos los enemigos en rango
        Collider2D[] enemigosEnRango = Physics2D.OverlapCircleAll(transform.position, rangoDeteccion, capaEnemigos);
        
        if (enemigosEnRango.Length == 0)
        {
            enemigoObjetivo = null;
            return;
        }
        
        // Encontrar el enemigo más cercano
        Transform enemigoCercano = null;
        float distanciaMinima = Mathf.Infinity;
        
        foreach (Collider2D enemigo in enemigosEnRango)
        {
            float distancia = Vector2.Distance(transform.position, enemigo.transform.position);
            if (distancia < distanciaMinima)
            {
                distanciaMinima = distancia;
                enemigoCercano = enemigo.transform;
            }
        }
        
        enemigoObjetivo = enemigoCercano;
    }

    void DispararProyectil()
    {
        if (prefabProyectil == null)
        {
            Debug.LogError("AutoAttack: ¡No se asignó el prefab del proyectil en el Inspector!");
            return;
        }
        
        if (puntoDisparo == null)
        {
            Debug.LogWarning("AutoAttack: No hay punto de disparo, usando posición del jugador");
            puntoDisparo = transform;
        }
        
        // Calcular dirección hacia el enemigo
        Vector2 direccion = (enemigoObjetivo.position - puntoDisparo.position).normalized;
        
        // Crear el proyectil
        GameObject proyectil = Instantiate(prefabProyectil, puntoDisparo.position, Quaternion.identity);
        
        Debug.Log("¡Proyectil disparado hacia " + enemigoObjetivo.name + "!");
        
        // Configurar la velocidad del proyectil
        Rigidbody2D rbProyectil = proyectil.GetComponent<Rigidbody2D>();
        if (rbProyectil != null)
        {
            rbProyectil.velocity = direccion * velocidadProyectil;
        }
        else
        {
            Debug.LogError("¡El proyectil no tiene Rigidbody2D! Agrégalo al prefab.");
        }
        
        // Rotar el proyectil para que apunte en la dirección correcta
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        proyectil.transform.rotation = Quaternion.Euler(0, 0, angulo);
        
        // Iniciar cooldown
        StartCoroutine(CooldownDisparo());
    }

    IEnumerator CooldownDisparo()
    {
        puedeDisparar = false;
        yield return new WaitForSeconds(tiempoEntreDisparos);
        puedeDisparar = true;
    }

    // Visualizar rango de detección en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);
        
        // Dibujar línea hacia el enemigo objetivo si existe
        if (enemigoObjetivo != null && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, enemigoObjetivo.position);
        }
    }
}
