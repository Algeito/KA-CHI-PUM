using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemigo especializado en combate cuerpo a cuerpo
/// Características: Rápido, agresivo, ataques en cadena
/// </summary>
public class EnemigoMelee : MonoBehaviour
{
    [Header("Estadisticas")]
    public int vidaMaxima = 75;
    private int vidaActual;
    public int danioAtaqueBasico = 8;
    public int danioAtaqueFuerte = 15;

    [Header("Movimiento")]
    public float velocidadMovimiento = 2.5f;
    public float velocidadPersecucion = 4f;
    public float velocidadCarga = 7f; // Velocidad durante carga especial

    [Header("Deteccion y Combate")]
    public float rangoDeteccion = 6f;
    public float rangoAtaque = 1.8f;
    public float tiempoEntreAtaques = 1.2f;
    public LayerMask capaJugador;

    [Header("Habilidades Especiales")]
    public bool puedeCargar = true; // Ataque de carga
    public float rangoCarga = 5f; // Distancia mínima para iniciar carga
    public float cooldownCarga = 5f;
    public int probabilidadAtaqueFuerte = 30; // 30% de probabilidad

    [Header("Patrullaje")]
    public bool patrulla = true;
    public float rangoPatrullaje = 4f;
    public float tiempoEsperaPatrulla = 1.5f;

    // Referencias privadas
    private Transform jugador;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Control de combate
    private bool puedeAtacar = true;
    private bool estaAtacando = false;
    private bool estaCargando = false;
    private bool puedeCargaNuevamente = true;

    // Combo de ataques
    private int contadorCombo = 0;
    private float tiempoUltimoAtaque = 0f;
    private float tiempoResetCombo = 2f;

    // Control de estados
    public enum EstadoEnemigo
    {
        Idle,
        Patrullando,
        Persiguiendo,
        Atacando,
        Cargando,
        Muerto
    }

    private EstadoEnemigo estadoActual = EstadoEnemigo.Idle;

    // Variables de patrullaje
    private Vector2 puntoInicial;
    private Vector2 destinoPatrulla;
    private float tiempoEsperaActual;

    // Variables de animación
    private Vector2 ultimaDireccion = Vector2.down;

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

        // Reset combo si ha pasado mucho tiempo
        if (Time.time - tiempoUltimoAtaque > tiempoResetCombo)
        {
            contadorCombo = 0;
        }

        // Verificar si puede hacer carga especial
        if (puedeCargar && puedeCargaNuevamente &&
            distanciaAlJugador <= rangoCarga && distanciaAlJugador > rangoAtaque &&
            estadoActual == EstadoEnemigo.Persiguiendo)
        {
            // 40% de probabilidad de hacer carga
            if (Random.Range(0, 100) < 40)
            {
                IniciarCarga();
                return;
            }
        }

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
                else if (distanciaAlJugador > rangoDeteccion * 1.5f)
                {
                    // Perdió al jugador, volver a patrullar o idle
                    estadoActual = patrulla ? EstadoEnemigo.Patrullando : EstadoEnemigo.Idle;
                    GenerarPuntoPatrulla();
                }
                else
                {
                    PerseguirJugador();
                }
                break;

            case EstadoEnemigo.Atacando:
                if (distanciaAlJugador > rangoAtaque * 1.3f)
                {
                    estadoActual = EstadoEnemigo.Persiguiendo;
                    estaAtacando = false;
                }
                else
                {
                    AtacarJugador();
                }
                break;

            case EstadoEnemigo.Cargando:
                // La carga se maneja en la corrutina
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
        // Si está esperando
        if (tiempoEsperaActual > 0)
        {
            tiempoEsperaActual -= Time.deltaTime;
            rb.velocity = Vector2.zero;
            return;
        }

        // Moverse hacia el destino
        Vector2 direccion = (destinoPatrulla - (Vector2)transform.position).normalized;
        rb.velocity = direccion * velocidadMovimiento;
        ultimaDireccion = direccion;

        // Si llegó al destino, generar nuevo punto
        if (Vector2.Distance(transform.position, destinoPatrulla) < 0.5f)
        {
            tiempoEsperaActual = tiempoEsperaPatrulla;
            GenerarPuntoPatrulla();
        }
    }

    void PerseguirJugador()
    {
        if (jugador == null) return;

        Vector2 direccion = (jugador.position - transform.position).normalized;
        rb.velocity = direccion * velocidadPersecucion;
        ultimaDireccion = direccion;
    }

    void AtacarJugador()
    {
        rb.velocity = Vector2.zero;

        // Mirar hacia el jugador
        if (jugador != null)
        {
            ultimaDireccion = (jugador.position - transform.position).normalized;
        }

        if (puedeAtacar && !estaAtacando)
        {
            // Decidir tipo de ataque
            bool esAtaqueFuerte = Random.Range(0, 100) < probabilidadAtaqueFuerte;
            StartCoroutine(EjecutarAtaque(esAtaqueFuerte));
        }
    }

    void IniciarCarga()
    {
        if (jugador == null || estaCargando) return;

        estadoActual = EstadoEnemigo.Cargando;
        StartCoroutine(EjecutarCarga());
    }

    IEnumerator EjecutarCarga()
    {
        estaCargando = true;
        puedeCargaNuevamente = false;

        // Calcular dirección de carga
        Vector2 direccionCarga = (jugador.position - transform.position).normalized;
        ultimaDireccion = direccionCarga;

        // Pequeña pausa antes de cargar
        yield return new WaitForSeconds(0.3f);

        // Activar animación de carga si existe
        if (animator != null)
        {
            animator.SetTrigger("Cargando");
        }

        // Realizar la carga
        float tiempoCarga = 0.8f;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoCarga)
        {
            rb.velocity = direccionCarga * velocidadCarga;

            // Detectar colisión durante la carga
            Collider2D[] objetosGolpeados = Physics2D.OverlapCircleAll(transform.position, rangoAtaque * 0.8f, capaJugador);
            foreach (Collider2D obj in objetosGolpeados)
            {
                PlayerController jugadorScript = obj.GetComponent<PlayerController>();
                if (jugadorScript != null)
                {
                    jugadorScript.RecibirDanio(danioAtaqueFuerte);
                    Debug.Log("Enemigo Melee CARGA al jugador por " + danioAtaqueFuerte + " de daño");
                    break; // Solo golpear una vez por carga
                }
            }

            tiempoTranscurrido += Time.deltaTime;
            yield return null;
        }

        // Detener carga
        rb.velocity = Vector2.zero;
        estaCargando = false;
        estadoActual = EstadoEnemigo.Persiguiendo;

        // Cooldown de carga
        yield return new WaitForSeconds(cooldownCarga);
        puedeCargaNuevamente = true;
    }

    IEnumerator EjecutarAtaque(bool esAtaqueFuerte)
    {
        estaAtacando = true;
        puedeAtacar = false;
        contadorCombo++;
        tiempoUltimoAtaque = Time.time;

        int danio = esAtaqueFuerte ? danioAtaqueFuerte : danioAtaqueBasico;

        // Activar animación de ataque
        if (animator != null)
        {
            animator.SetTrigger("Atacando");
            if (esAtaqueFuerte)
            {
                animator.SetTrigger("AtaqueFuerte");
            }
        }

        // Pequeña espera para sincronizar con la animación
        yield return new WaitForSeconds(0.25f);

        // Detectar y dañar al jugador
        Collider2D[] objetosGolpeados = Physics2D.OverlapCircleAll(transform.position, rangoAtaque, capaJugador);
        foreach (Collider2D obj in objetosGolpeados)
        {
            PlayerController jugadorScript = obj.GetComponent<PlayerController>();
            if (jugadorScript != null)
            {
                // Bonus de daño por combo
                int danioTotal = danio + (contadorCombo - 1);
                jugadorScript.RecibirDanio(danioTotal);

                string tipoAtaque = esAtaqueFuerte ? "FUERTE" : "BASICO";
                Debug.Log($"Enemigo Melee {tipoAtaque} golpea por {danioTotal} (combo x{contadorCombo})");
            }
        }

        // Ataques más rápidos en combo
        float cooldown = tiempoEntreAtaques * (1f - (contadorCombo * 0.1f));
        cooldown = Mathf.Max(cooldown, 0.5f); // Mínimo 0.5 segundos

        yield return new WaitForSeconds(cooldown);
        puedeAtacar = true;
        estaAtacando = false;
    }

    void GenerarPuntoPatrulla()
    {
        // Generar un punto aleatorio dentro del rango de patrullaje
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

    void ActualizarAnimaciones()
    {
        if (animator == null) return;

        // Actualizar parámetros de velocidad para animaciones de movimiento
        animator.SetFloat("Velocidad_X", ultimaDireccion.x);
        animator.SetFloat("Velocidad_Y", ultimaDireccion.y);
        animator.SetBool("En_Movimiento", rb.velocity.magnitude > 0.1f);
        animator.SetInteger("Combo", contadorCombo);
    }

    public void RecibirDanio(int cantidad)
    {
        if (estadoActual == EstadoEnemigo.Muerto)
            return;

        vidaActual -= cantidad;
        Debug.Log("Enemigo Melee recibió " + cantidad + " de daño. Vida restante: " + vidaActual);

        // Efecto visual de daño
        StartCoroutine(EfectoGolpe());

        // Si recibe daño, empezar a perseguir al jugador agresivamente
        if (estadoActual != EstadoEnemigo.Atacando && estadoActual != EstadoEnemigo.Persiguiendo)
        {
            estadoActual = EstadoEnemigo.Persiguiendo;
        }

        // Cuando recibe daño, incrementar velocidad brevemente (enrage)
        StartCoroutine(Enrage());

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    IEnumerator Enrage()
    {
        float velocidadOriginal = velocidadPersecucion;
        velocidadPersecucion *= 1.3f;
        yield return new WaitForSeconds(2f);
        velocidadPersecucion = velocidadOriginal;
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
        Debug.Log("Enemigo Melee eliminado");

        // Detener movimiento
        rb.velocity = Vector2.zero;

        // Desactivar colisiones
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Animación de muerte si existe
        if (animator != null)
        {
            animator.SetTrigger("Morir");
        }

        // Destruir después de un tiempo
        StartCoroutine(DestruirDespuesDeTiempo(1f));
    }

    IEnumerator DestruirDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);

        // Aquí puedes agregar efectos adicionales:
        // - Spawn de items (monedas, pociones, etc.)
        // - Efectos de partículas
        // - Sonidos

        Destroy(gameObject);
    }

    // Visualizar rangos en el editor
    void OnDrawGizmosSelected()
    {
        // Rango de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Rango de ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);

        // Rango de carga (naranja)
        if (puedeCargar)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f);
            Gizmos.DrawWireSphere(transform.position, rangoCarga);
        }

        // Rango de patrullaje (azul)
        if (patrulla)
        {
            Vector3 puntoInicio = Application.isPlaying ? puntoInicial : transform.position;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puntoInicio, rangoPatrullaje);
        }
    }

    // Getters públicos
    public int ObtenerVidaActual() => vidaActual;
    public int ObtenerVidaMaxima() => vidaMaxima;
    public EstadoEnemigo ObtenerEstadoActual() => estadoActual;
    public int ObtenerCombo() => contadorCombo;
}
