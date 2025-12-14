using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Estadisticas")] 
    public int vidaMaxima = 50;
    private int vidaActual;
    public int danioProyectil = 5; // Daño de cada proyectil

    [Header("Movimiento")] 
    public float velocidadMovimiento = 2f;
    public float velocidadPersecucion = 3f;
    public bool mantenerDistancia = true; // El mago mantiene distancia
    public float distanciaIdeal = 5f; // Distancia preferida del jugador

    [Header("Deteccion")] 
    public float rangoDeteccion = 8f;
    public LayerMask capaJugador;

    [Header("Ataque Mágico")]
    public GameObject prefabProyectil; // Prefab del proyectil
    public Transform puntoDisparo; // Desde donde dispara
    public float rangoAtaque = 7f; // Rango para empezar a disparar
    public float tiempoEntreRafagas = 2f; // Tiempo entre ráfagas
    public int proyectilesPorRafaga = 3; // Cantidad de proyectiles por ráfaga
    public float tiempoEntreProyectiles = 0.15f; // Delay entre cada proyectil
    public float velocidadProyectil = 5f;
    public float anguloDispersion = 15f; // Ángulo de dispersión entre proyectiles

    [Header("Patrullaje (Opcional)")] 
    public bool patrulla = true;
    public float rangoPatrullaje = 3f;
    public float tiempoEsperaPatrulla = 2f;

    // Referencias privadas
    private Transform jugador;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Control de combate
    private bool puedeDisparar = true;
    private bool estaDisparando = false;

    // Control de estados
    public enum EstadoEnemigo
    {
        Idle,
        Patrullando,
        Persiguiendo,
        Atacando,
        Muerto
    }

    private EstadoEnemigo estadoActual = EstadoEnemigo.Idle;

    // Variables de patrullaje
    private Vector2 puntoInicial;
    private Vector2 destinoPatrulla;
    private float tiempoEsperaActual;

    void Start()
    {
        // Inicializar componentes
        vidaActual = vidaMaxima;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Configurar Rigidbody2D
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Guardar posición inicial
        puntoInicial = transform.position;

        // Buscar al jugador
        BuscarJugador();

        // Validar configuración
        if (prefabProyectil == null)
        {
            Debug.LogError($"Mago '{name}': ¡Falta asignar prefab de proyectil!");
        }

        if (puntoDisparo == null)
        {
            Debug.LogWarning($"Mago '{name}': No hay punto de disparo, usando posición del mago");
            puntoDisparo = transform;
        }

        // Iniciar patrullaje si está activado
        if (patrulla)
        {
            GenerarPuntoPatrulla();
            estadoActual = EstadoEnemigo.Patrullando;
        }
        else
        {
            estadoActual = EstadoEnemigo.Idle;
        }
    }

    void Update()
    {
        if (estadoActual == EstadoEnemigo.Muerto)
            return;

        // Si no hay jugador, intentar buscarlo
        if (jugador == null)
        {
            BuscarJugador();
            if (jugador == null)
            {
                estadoActual = EstadoEnemigo.Idle;
                return;
            }
        }

        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        // Máquina de estados
        switch (estadoActual)
        {
            case EstadoEnemigo.Idle:
                ModoIdle();
                if (distanciaAlJugador <= rangoDeteccion)
                {
                    estadoActual = EstadoEnemigo.Persiguiendo;
                }
                else if (patrulla)
                {
                    estadoActual = EstadoEnemigo.Patrullando;
                }
                break;

            case EstadoEnemigo.Patrullando:
                ModoPatrulla();
                if (distanciaAlJugador <= rangoDeteccion)
                {
                    estadoActual = EstadoEnemigo.Persiguiendo;
                }
                break;

            case EstadoEnemigo.Persiguiendo:
                if (distanciaAlJugador <= rangoAtaque)
                {
                    estadoActual = EstadoEnemigo.Atacando;
                }
                else if (distanciaAlJugador > rangoDeteccion)
                {
                    estadoActual = patrulla ? EstadoEnemigo.Patrullando : EstadoEnemigo.Idle;
                    GenerarPuntoPatrulla();
                }
                else
                {
                    MoverHaciaJugador(distanciaAlJugador);
                }
                break;

            case EstadoEnemigo.Atacando:
                if (distanciaAlJugador > rangoAtaque && !estaDisparando)
                {
                    estadoActual = EstadoEnemigo.Persiguiendo;
                }
                else
                {
                    AtacarConMagia(distanciaAlJugador);
                }
                break;
        }

        // Actualizar animaciones
        ActualizarAnimaciones();
    }

    void ModoIdle()
    {
        rb.velocity = Vector2.zero;
    }

    void ModoPatrulla()
    {
        if (tiempoEsperaActual > 0)
        {
            tiempoEsperaActual -= Time.deltaTime;
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direccion = (destinoPatrulla - (Vector2)transform.position).normalized;
        rb.velocity = direccion * velocidadMovimiento;

        VoltearSprite(direccion.x);

        if (Vector2.Distance(transform.position, destinoPatrulla) < 0.5f)
        {
            tiempoEsperaActual = tiempoEsperaPatrulla;
            GenerarPuntoPatrulla();
        }
    }

    void MoverHaciaJugador(float distanciaActual)
    {
        Vector2 direccion = (jugador.position - transform.position).normalized;

        // Si debe mantener distancia (comportamiento de mago)
        if (mantenerDistancia)
        {
            if (distanciaActual < distanciaIdeal - 1f)
            {
                // Demasiado cerca, alejarse
                rb.velocity = -direccion * velocidadMovimiento;
            }
            else if (distanciaActual > distanciaIdeal + 1f)
            {
                // Demasiado lejos, acercarse
                rb.velocity = direccion * velocidadPersecucion;
            }
            else
            {
                // En distancia ideal, moverse lateralmente o quedarse quieto
                rb.velocity = Vector2.zero;
            }
        }
        else
        {
            // Moverse directamente hacia el jugador
            rb.velocity = direccion * velocidadPersecucion;
        }

        VoltearSprite(direccion.x);
    }

    void AtacarConMagia(float distanciaActual)
    {
        // Mantener distancia mientras ataca
        if (mantenerDistancia && distanciaActual < distanciaIdeal - 1f)
        {
            Vector2 direccionAlejarse = (transform.position - jugador.position).normalized;
            rb.velocity = direccionAlejarse * velocidadMovimiento * 0.5f;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

        // Mirar hacia el jugador
        float direccionX = jugador.position.x - transform.position.x;
        VoltearSprite(direccionX);

        // Disparar ráfaga de proyectiles
        if (puedeDisparar && !estaDisparando)
        {
            StartCoroutine(DispararRafaga());
        }
    }

    IEnumerator DispararRafaga()
    {
        estaDisparando = true;
        puedeDisparar = false;

        // Activar animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("Atacando");
        }

        Debug.Log($"Mago '{name}' inicia ráfaga de {proyectilesPorRafaga} proyectiles");

        // Disparar múltiples proyectiles
        for (int i = 0; i < proyectilesPorRafaga; i++)
        {
            DispararProyectil(i);
            yield return new WaitForSeconds(tiempoEntreProyectiles);
        }

        estaDisparando = false;

        // Cooldown antes de la siguiente ráfaga
        yield return new WaitForSeconds(tiempoEntreRafagas);
        puedeDisparar = true;
    }

    void DispararProyectil(int indiceProyectil)
    {
        if (prefabProyectil == null || jugador == null) return;

        // Calcular dirección base hacia el jugador
        Vector2 direccionBase = (jugador.position - puntoDisparo.position).normalized;

        // Calcular dispersión
        float anguloBase = Mathf.Atan2(direccionBase.y, direccionBase.x) * Mathf.Rad2Deg;
        
        // Distribuir los proyectiles en un arco
        float anguloOffset;
        if (proyectilesPorRafaga == 1)
        {
            anguloOffset = 0;
        }
        else
        {
            // Distribuir uniformemente alrededor del centro
            float rangoTotal = anguloDispersion * (proyectilesPorRafaga - 1);
            anguloOffset = -rangoTotal / 2 + (anguloDispersion * indiceProyectil);
        }

        float anguloFinal = anguloBase + anguloOffset;
        Vector2 direccionFinal = new Vector2(
            Mathf.Cos(anguloFinal * Mathf.Deg2Rad),
            Mathf.Sin(anguloFinal * Mathf.Deg2Rad)
        ).normalized;

        // Crear proyectil
        GameObject proyectil = Instantiate(prefabProyectil, puntoDisparo.position, Quaternion.identity);

        // Configurar velocidad (MOVIMIENTO LINEAL, NO persigue)
        Rigidbody2D rbProyectil = proyectil.GetComponent<Rigidbody2D>();
        if (rbProyectil != null)
        {
            rbProyectil.velocity = direccionFinal * velocidadProyectil;
        }

        // Rotar proyectil
        proyectil.transform.rotation = Quaternion.Euler(0, 0, anguloFinal);

        // Configurar daño
        ProyectilEnemigo proyectilScript = proyectil.GetComponent<ProyectilEnemigo>();
        if (proyectilScript != null)
        {
            proyectilScript.ConfigurarDanio(danioProyectil);
        }

        Debug.Log($"Proyectil {indiceProyectil + 1} disparado en ángulo {anguloFinal}°");
    }

    void GenerarPuntoPatrulla()
    {
        Vector2 puntoAleatorio = Random.insideUnitCircle * rangoPatrullaje;
        destinoPatrulla = puntoInicial + puntoAleatorio;
    }

    void BuscarJugador()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
    }

    void VoltearSprite(float direccionX)
    {
        if (spriteRenderer != null && direccionX != 0)
        {
            spriteRenderer.flipX = direccionX < 0;
        }
    }

    void ActualizarAnimaciones()
    {
        if (animator == null) return;
        // Aquí puedes agregar lógica de animación si tienes parámetros
    }

    public void RecibirDanio(int cantidad)
    {
        if (estadoActual == EstadoEnemigo.Muerto)
            return;

        vidaActual -= cantidad;
        Debug.Log($"Mago '{name}' recibió {cantidad} de daño. Vida restante: {vidaActual}");

        StartCoroutine(EfectoGolpe());

        if (estadoActual != EstadoEnemigo.Atacando && estadoActual != EstadoEnemigo.Persiguiendo)
        {
            estadoActual = EstadoEnemigo.Persiguiendo;
        }

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    IEnumerator EfectoGolpe()
    {
        if (spriteRenderer != null)
        {
            Color colorOriginal = spriteRenderer.color;
            spriteRenderer.color = Color.red;
            yield return new WaitForSeconds(0.15f);
            spriteRenderer.color = colorOriginal;
        }
    }

    void Morir()
    {
        estadoActual = EstadoEnemigo.Muerto;
        Debug.Log($"Mago '{name}' eliminado");

        rb.velocity = Vector2.zero;
        rb.simulated = false;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        Destroy(gameObject, 1f);
    }

    void OnDrawGizmosSelected()
    {
        // Rango de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Rango de ataque (naranja)
        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);

        // Distancia ideal (cyan)
        if (mantenerDistancia)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, distanciaIdeal);
        }

        // Punto de disparo
        if (puntoDisparo != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(puntoDisparo.position, 0.3f);
        }

        // Rango de patrullaje (azul)
        if (patrulla)
        {
            Vector3 puntoInicio = Application.isPlaying ? puntoInicial : transform.position;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puntoInicio, rangoPatrullaje);
        }
    }

    public int ObtenerVidaActual() => vidaActual;
    public int ObtenerVidaMaxima() => vidaMaxima;
    public EstadoEnemigo ObtenerEstadoActual() => estadoActual;
}
