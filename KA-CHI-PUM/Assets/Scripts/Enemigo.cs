using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Estad�sticas")] public int vidaMaxima = 50;
    private int vidaActual;
    public int danioAtaque = 5;

    [Header("Movimiento")] public float velocidadMovimiento = 2f;
    public float velocidadPersecucion = 3f;

    [Header("Detecci�n y Combate")] public float rangoDeteccion = 5f;
    public float rangoAtaque = 1.5f;
    public float tiempoEntreAtaques = 1.5f;
    public LayerMask capaJugador;

    [Header("Patrullaje (Opcional)")] public bool patrulla = true;
    public float rangoPatrullaje = 3f;
    public float tiempoEsperaPatrulla = 2f;

    // Referencias privadas
    private Transform jugador;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    // Control de combate
    private bool puedeAtacar = true;
    private bool estaAtacando = false;

    // Control de estados
    private enum EstadoEnemigo
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

    // Variables de animaci�n
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

        // Guardar posici�n inicial
        puntoInicial = transform.position;

        // Buscar al jugador
        BuscarJugador();

        // Iniciar patrullaje si est� activado
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

        // M�quina de estados
        switch (estadoActual)
        {
            case EstadoEnemigo.Idle:
                ModoIdle();
                // Verificar si detecta al jugador
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
                // Verificar si detecta al jugador
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
                    // Perdi� al jugador, volver a patrullar o idle
                    estadoActual = patrulla ? EstadoEnemigo.Patrullando : EstadoEnemigo.Idle;
                    GenerarPuntoPatrulla();
                }
                else
                {
                    PerseguirJugador();
                }

                break;

            case EstadoEnemigo.Atacando:
                if (distanciaAlJugador > rangoAtaque)
                {
                    estadoActual = EstadoEnemigo.Persiguiendo;
                    estaAtacando = false;
                }
                else
                {
                    AtacarJugador();
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
        // Si est� esperando
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

        // Si lleg� al destino, generar nuevo punto
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
            StartCoroutine(EjecutarAtaque());
        }
    }

    IEnumerator EjecutarAtaque()
    {
        estaAtacando = true;
        puedeAtacar = false;

        // Activar animaci�n de ataque
        if (animator != null)
        {
            animator.SetTrigger("Atacando");
        }

        // Peque�a espera para sincronizar con la animaci�n
        yield return new WaitForSeconds(0.2f);

        // Detectar y da�ar al jugador
        Collider2D[] objetosGolpeados = Physics2D.OverlapCircleAll(transform.position, rangoAtaque, capaJugador);
        foreach (Collider2D obj in objetosGolpeados)
        {
            PlayerController jugadorScript = obj.GetComponent<PlayerController>();
            if (jugadorScript != null)
            {
                jugadorScript.RecibirDanio(danioAtaque);
                Debug.Log("Enemigo golpea al jugador por " + danioAtaque + " de daño");
            }
        }

        // Esperar antes de poder atacar de nuevo
        yield return new WaitForSeconds(tiempoEntreAtaques);
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

        // Actualizar par�metros de velocidad para animaciones de movimiento
        animator.SetFloat("VelocidadX", ultimaDireccion.x);
        animator.SetFloat("VelocidadY", ultimaDireccion.y);

        // Puedes agregar m�s par�metros seg�n tus animaciones
        animator.SetBool("EnMovimiento", rb.velocity.magnitude > 0.1f);
    }

    public void RecibirDanio(int cantidad)
    {
        if (estadoActual == EstadoEnemigo.Muerto)
            return;

        vidaActual -= cantidad;
        Debug.Log("Enemigo recibi� " + cantidad + " de da�o. Vida restante: " + vidaActual);

        // Efecto visual de da�o
        StartCoroutine(EfectoGolpe());

        // Si recibe da�o, empezar a perseguir al jugador
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
        Debug.Log("Enemigo eliminado");

        // Detener movimiento
        rb.velocity = Vector2.zero;

        // Desactivar colisiones
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        // Animaci�n de muerte si existe
        if (animator != null)
        {
            animator.SetTrigger("Morir");
        }

        // Destruir despu�s de un tiempo (para permitir animaci�n de muerte)
        StartCoroutine(DestruirDespuesDeTiempo(1f));
    }

    IEnumerator DestruirDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);

        // Aqu� puedes agregar efectos adicionales:
        // - Spawn de items
        // - Efectos de part�culas
        // - Sonidos

        Destroy(gameObject);
    }

    // Visualizar rangos en el editor
    void OnDrawGizmosSelected()
    {
        // Rango de detecci�n (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Rango de ataque (rojo)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);

        // Rango de patrullaje (azul) - solo si est� activado
        if (patrulla)
        {
            Vector3 puntoInicio = Application.isPlaying ? puntoInicial : transform.position;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puntoInicio, rangoPatrullaje);
        }
    }

    // Getters p�blicos
    public int ObtenerVidaActual()
    {
        return vidaActual;
    }

    public int ObtenerVidaMaxima()
    {
        return vidaMaxima;
    }
}
