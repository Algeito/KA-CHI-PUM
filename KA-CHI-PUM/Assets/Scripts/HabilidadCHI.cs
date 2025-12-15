using System.Collections;
using UnityEngine;

public class HabilidadCHI : MonoBehaviour
{
    [Header("═══ ACTIVACIÓN ═══")]
    [SerializeField] private KeyCode teclaActivacion = KeyCode.Q;
    [SerializeField] private float cooldown = 8f;
    private bool estaEnCooldown = false;
    private bool estaActiva = false;

    [Header("═══ DAÑO Y ÁREA ═══")]
    [SerializeField] private float radioDanio = 3f;
    [SerializeField] private int danioBase = 5;
    [SerializeField] private float duracionHabilidad = 5f;
    [SerializeField] private float tiempoEntrePulsos = 0.5f;

    [Header("═══ DETECCIÓN DE ENEMIGOS ═══")]
    [SerializeField] private bool usarTag = true;
    [SerializeField] private string tagEnemigo = "Enemy";
    [SerializeField] private bool usarLayer = true;
    [SerializeField] private LayerMask capaEnemigos;

    [Header("═══ EFECTO VISUAL ═══")]
    [SerializeField] private GameObject prefabEfectoCHI; // ← Arrastra aquí tu prefab
    [SerializeField] private bool mostrarEfectoVisual = true;

    private GameObject efectoVisual;
    private float tiempoUltimoUso = -999f;

    private void Update()
    {
        // Detectar tecla Q
        if (Input.GetKeyDown(teclaActivacion))
        {
            IntentarActivar();
        }
    }

    private void IntentarActivar()
    {
        // Verificar cooldown
        if (estaEnCooldown)
        {
            float tiempoRestante = cooldown - (Time.time - tiempoUltimoUso);
            Debug.Log($"CHI en cooldown. Espera {tiempoRestante:F1} segundos");
            return;
        }

        // Verificar si ya está activa
        if (estaActiva)
        {
            Debug.Log("CHI ya está activa");
            return;
        }

        // Activar habilidad
        ActivarCHI();
    }

    private void ActivarCHI()
    {
        Debug.Log("=== HABILIDAD CHI ACTIVADA ===");
        tiempoUltimoUso = Time.time;
        estaEnCooldown = true;
        StartCoroutine(EjecutarCHI());
        StartCoroutine(CooldownTimer());
    }

    private IEnumerator EjecutarCHI()
    {
        estaActiva = true;
        float tiempoTranscurrido = 0f;
        float siguientePulso = 0f;

        // Crear efecto visual
        if (mostrarEfectoVisual)
        {
            CrearEfectoVisual();
        }

        // Bucle principal de la habilidad
        while (tiempoTranscurrido < duracionHabilidad)
        {
            // Mover efecto visual con el jugador
            if (efectoVisual != null)
            {
                efectoVisual.transform.position = transform.position;
            }

            // Aplicar daño en pulsos
            if (tiempoTranscurrido >= siguientePulso)
            {
                AplicarDanioEnArea();
                siguientePulso += tiempoEntrePulsos;
            }

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        // Limpiar
        if (efectoVisual != null)
        {
            Destroy(efectoVisual);
        }

        estaActiva = false;
        Debug.Log("=== HABILIDAD CHI TERMINADA ===");
    }

    private void AplicarDanioEnArea()
    {
        int enemigosAfectados = 0;

        // MÉTODO 1: Buscar por Tag
        if (usarTag)
        {
            GameObject[] enemigos = GameObject.FindGameObjectsWithTag(tagEnemigo);
            
            foreach (GameObject enemigo in enemigos)
            {
                float distancia = Vector2.Distance(transform.position, enemigo.transform.position);
                
                if (distancia <= radioDanio)
                {
                    if (DaniarEnemigo(enemigo))
                    {
                        enemigosAfectados++;
                    }
                }
            }
        }

        // MÉTODO 2: Buscar por Layer
        if (usarLayer)
        {
            Collider2D[] colisiones = Physics2D.OverlapCircleAll(transform.position, radioDanio, capaEnemigos);
            
            foreach (Collider2D col in colisiones)
            {
                if (DaniarEnemigo(col.gameObject))
                {
                    enemigosAfectados++;
                }
            }
        }

        if (enemigosAfectados > 0)
        {
            Debug.Log($"CHI dañó a {enemigosAfectados} enemigos por {danioBase} cada uno");
        }
    }

    private bool DaniarEnemigo(GameObject enemigo)
    {
        // Intentar dañar como Enemigo (Mago Rojo)
        Enemigo scriptEnemigo = enemigo.GetComponent<Enemigo>();
        if (scriptEnemigo != null)
        {
            if (scriptEnemigo.ObtenerEstadoActual() != Enemigo.EstadoEnemigo.Muerto)
            {
                scriptEnemigo.RecibirDanio(danioBase);
                Debug.Log($"CHI → Mago '{enemigo.name}' recibió {danioBase} de daño");
                return true;
            }
            return false;
        }

        // Intentar dañar como Minotauro
        MinotauroController scriptMinotauro = enemigo.GetComponent<MinotauroController>();
        if (scriptMinotauro != null)
        {
            if (!scriptMinotauro.EstaMuerto())
            {
                scriptMinotauro.RecibirDanio(danioBase);
                Debug.Log($"CHI → Minotauro '{enemigo.name}' recibió {danioBase} de daño");
                return true;
            }
            return false;
        }

        Debug.LogWarning($"CHI detectó '{enemigo.name}' pero no tiene script de enemigo");
        return false;
    }

    private void CrearEfectoVisual()
    {
        if (prefabEfectoCHI != null)
        {
            // Usar tu prefab personalizado
            efectoVisual = Instantiate(prefabEfectoCHI, transform.position, Quaternion.identity);
            efectoVisual.transform.SetParent(transform); // Seguir al jugador
            
            // Ajustar escala según el radio de daño
            float escalaBase = radioDanio / 1.5f; // Ajusta este divisor según tu prefab
            efectoVisual.transform.localScale = new Vector3(escalaBase, escalaBase, 1f);
            
            Debug.Log($"Efecto CHI creado con prefab en escala {escalaBase}");
        }
        else
        {
            Debug.LogWarning("No hay prefab asignado para el efecto CHI");
        }
    }

    private IEnumerator CooldownTimer()
    {
        yield return new WaitForSeconds(cooldown);
        estaEnCooldown = false;
        Debug.Log("CHI lista para usar de nuevo");
    }

    private void OnDrawGizmosSelected()
    {
        // Visualizar rango en el editor
        Gizmos.color = new Color(0, 1, 1, 0.3f);
        Gizmos.DrawSphere(transform.position, radioDanio);
        
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radioDanio);
    }
}