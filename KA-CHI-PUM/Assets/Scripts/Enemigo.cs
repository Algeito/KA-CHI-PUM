using System.Collections;
using UnityEngine;

public class Enemigo : MonoBehaviour
{
    [Header("Estadisticas")]
    public int vidaMaxima = 50;
    private int vidaActual;
    public int danioAtaque = 5;

    [Header("Recompensas")]
    public int experienciaAlMorir = 25; // EXP que otorga al morir

    [Header("Movimiento")]
    public float velocidadMovimiento = 2f;
    public float velocidadPersecucion = 3f;

    [Header("Deteccion y Combate")]
    public float rangoDeteccion = 5f;
    public float rangoAtaque = 1.5f;
    public float tiempoEntreAtaques = 1.5f;
    public LayerMask capaJugador;

    [Header("Patrullaje (Opcional)")]
    public bool patrulla = true;
    public float rangoPatrullaje = 3f;
    public float tiempoEsperaPatrulla = 2f;

    // Referencias privadas
    private Transform jugador;
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private BarraVidaEnemigo barraVida;

    // Control de combate
    private bool puedeAtacar = true;
    private bool estaAtacando = false;

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

    // Variables de animación
    private Vector2 ultimaDireccion = Vector2.down;

    void Start()
    {
        vidaActual = vidaMaxima;
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        barraVida = GetComponentInChildren<BarraVidaEnemigo>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        puntoInicial = transform.position;
        BuscarJugador();

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
        ultimaDireccion = direccion;

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

        if (animator != null)
        {
            animator.SetTrigger("Atacando");
        }

        yield return new WaitForSeconds(0.2f);

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

        yield return new WaitForSeconds(tiempoEntreAtaques);
        puedeAtacar = true;
        estaAtacando = false;
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

    void ActualizarAnimaciones()
    {
        if (animator == null) return;

        animator.SetFloat("Velocidad_X", ultimaDireccion.x);
        animator.SetFloat("Velocidad_Y", ultimaDireccion.y);
        animator.SetBool("En_Movimiento", rb.velocity.magnitude > 0.1f);
    }

    public void RecibirDanio(int cantidad)
    {
        if (estadoActual == EstadoEnemigo.Muerto)
            return;

        vidaActual -= cantidad;
        Debug.Log("Enemigo recibio " + cantidad + " de danio. Vida restante: " + vidaActual);

        // Mostrar barra de vida
        if (barraVida != null)
        {
            barraVida.MostrarBarra();
        }

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
        Debug.Log("Enemigo eliminado - Otorgando " + experienciaAlMorir + " EXP");

        // Otorgar experiencia al jugador
        if (jugador != null)
        {
            SistemaExperiencia sistemaExp = jugador.GetComponent<SistemaExperiencia>();
            if (sistemaExp != null)
            {
                sistemaExp.GanarExperiencia(experienciaAlMorir);
            }
        }

        rb.velocity = Vector2.zero;

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        if (animator != null)
        {
            animator.SetTrigger("Morir");
        }

        StartCoroutine(DestruirDespuesDeTiempo(1f));
    }

    IEnumerator DestruirDespuesDeTiempo(float tiempo)
    {
        yield return new WaitForSeconds(tiempo);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, rangoAtaque);

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