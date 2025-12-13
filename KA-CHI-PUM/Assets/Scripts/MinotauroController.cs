using System.Collections;
using UnityEngine;

public class MinotauroController : MonoBehaviour
{
    [Header("═══ ESTADÍSTICAS ═══")]
    [SerializeField] private int vidaMaxima = 100;
    [SerializeField] private int vidaActual;
    [SerializeField] private int danioAtaque = 15;
    [SerializeField] private float defensaBase = 0f; // Porcentaje de reducción de daño (0-1)

    [Header("═══ MOVIMIENTO ═══")]
    [SerializeField] private float velocidadCaminar = 2f;
    [SerializeField] private float velocidadCorrer = 4f;
    [SerializeField] private bool puedeCorrer = true;

    [Header("═══ COMBATE ═══")]
    [SerializeField] private float rangoDeteccion = 7f;
    [SerializeField] private float rangoAtaque = 2f;
    [SerializeField] private float tiempoEntreAtaques = 2f;
    [SerializeField] private float duracionAtaque = 0.8f; // Duración de la animación de ataque
    [SerializeField] private Transform puntoAtaque;
    [SerializeField] private LayerMask capaJugador;

    [Header("═══ COMPORTAMIENTO ═══")]
    [SerializeField] private bool patrullar = true;
    [SerializeField] private float rangoPatrullaje = 5f;
    [SerializeField] private float tiempoEsperaPatrulla = 3f;

    [Header("═══ EFECTOS VISUALES ═══")]
    [SerializeField] private Color colorDanio = Color.red;
    [SerializeField] private float duracionEfectoDanio = 0.2f;
    [SerializeField] private GameObject efectoMuerte; // Prefab de partículas opcional
    [SerializeField] private float tiempoAntesDeDestruir = 2f;

    [Header("═══ AUDIO (Opcional) ═══")]
    [SerializeField] private AudioClip sonidoAtaque;
    [SerializeField] private AudioClip sonidoDanio;
    [SerializeField] private AudioClip sonidoMuerte;

    // Referencias de componentes
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private Transform jugador;

    // Estados del Minotauro
    public enum EstadoMinotauro
    {
        Idle,
        Patrullando,
        Persiguiendo,
        Atacando,
        Muriendo,
        Muerto
    }

    private EstadoMinotauro estadoActual = EstadoMinotauro.Idle;

    // Variables de control
    private bool puedeAtacar = true;
    private bool estaAtacando = false;
    private bool estaMuerto = false;
    private Vector2 puntoInicial;
    private Vector2 destinoPatrulla;
    private float tiempoEsperaActual;
    private Color colorOriginal;

    // Hashes de animación (optimización)
    private int hashCaminar;
    private int hashAtacar;
    private int hashMorir;
    private int hashVelocidad;

    private void Awake()
    {
        // Obtener componentes
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Configurar Rigidbody2D
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        // Guardar color original
        if (spriteRenderer != null)
        {
            colorOriginal = spriteRenderer.color;
        }
    }

    private void Start()
    {
        // Inicializar vida
        vidaActual = vidaMaxima;

        // Guardar posición inicial
        puntoInicial = transform.position;

        // Buscar al jugador
        BuscarJugador();

        // Inicializar hashes de animación
        InicializarHashesAnimacion();

        // Configurar estado inicial
        if (patrullar)
        {
            GenerarPuntoPatrulla();
            estadoActual = EstadoMinotauro.Patrullando;
        }
        else
        {
            estadoActual = EstadoMinotauro.Idle;
        }

        Debug.Log($"Minotauro inicializado - Vida: {vidaActual}/{vidaMaxima}");
    }

    private void Update()
    {
        if (estaMuerto) return;

        // Buscar jugador si no existe
        if (jugador == null)
        {
            BuscarJugador();
            if (jugador == null)
            {
                estadoActual = EstadoMinotauro.Idle;
                return;
            }
        }

        // Ejecutar máquina de estados
        EjecutarMaquinaEstados();

        // Actualizar animaciones
        ActualizarAnimaciones();
    }

    private void EjecutarMaquinaEstados()
    {
        float distanciaAlJugador = Vector2.Distance(transform.position, jugador.position);

        switch (estadoActual)
        {
            case EstadoMinotauro.Idle:
                Idle();
                if (distanciaAlJugador <= rangoDeteccion)
                {
                    CambiarEstado(EstadoMinotauro.Persiguiendo);
                }
                else if (patrullar)
                {
                    CambiarEstado(EstadoMinotauro.Patrullando);
                }
                break;

            case EstadoMinotauro.Patrullando:
                Patrullar();
                if (distanciaAlJugador <= rangoDeteccion)
                {
                    CambiarEstado(EstadoMinotauro.Persiguiendo);
                }
                break;

            case EstadoMinotauro.Persiguiendo:
                if (distanciaAlJugador <= rangoAtaque)
                {
                    CambiarEstado(EstadoMinotauro.Atacando);
                }
                else if (distanciaAlJugador > rangoDeteccion)
                {
                    CambiarEstado(patrullar ? EstadoMinotauro.Patrullando : EstadoMinotauro.Idle);
                    GenerarPuntoPatrulla();
                }
                else
                {
                    PerseguirJugador();
                }
                break;

            case EstadoMinotauro.Atacando:
                if (distanciaAlJugador > rangoAtaque && !estaAtacando)
                {
                    CambiarEstado(EstadoMinotauro.Persiguiendo);
                }
                else
                {
                    AtacarJugador();
                }
                break;
        }
    }

    private void Idle()
    {
        rb.velocity = Vector2.zero;
    }

    private void Patrullar()
    {
        if (tiempoEsperaActual > 0)
        {
            tiempoEsperaActual -= Time.deltaTime;
            rb.velocity = Vector2.zero;
            return;
        }

        Vector2 direccion = (destinoPatrulla - (Vector2)transform.position).normalized;
        rb.velocity = direccion * velocidadCaminar;

        VoltearSprite(direccion.x);

        if (Vector2.Distance(transform.position, destinoPatrulla) < 0.5f)
        {
            tiempoEsperaActual = tiempoEsperaPatrulla;
            GenerarPuntoPatrulla();
        }
    }

    private void PerseguirJugador()
    {
        Vector2 direccion = (jugador.position - transform.position).normalized;
        float velocidad = puedeCorrer && Vector2.Distance(transform.position, jugador.position) > rangoAtaque * 1.5f 
            ? velocidadCorrer 
            : velocidadCaminar;
        
        rb.velocity = direccion * velocidad;

        VoltearSprite(direccion.x);
    }

    private void AtacarJugador()
    {
        rb.velocity = Vector2.zero;

        // Mirar hacia el jugador
        float direccionX = jugador.position.x - transform.position.x;
        VoltearSprite(direccionX);

        if (puedeAtacar && !estaAtacando)
        {
            StartCoroutine(EjecutarAtaque());
        }
    }

    private IEnumerator EjecutarAtaque()
    {
        estaAtacando = true;
        puedeAtacar = false;

        // Activar animación de ataque
        if (animator != null)
        {
            animator.SetTrigger(hashAtacar);
        }

        // Reproducir sonido de ataque
        ReproducirSonido(sonidoAtaque);

        // Esperar a que la animación llegue al punto de impacto (aprox. 40% de la animación)
        yield return new WaitForSeconds(duracionAtaque * 0.4f);

        // Detectar y dañar al jugador
        if (puntoAtaque != null)
        {
            Collider2D[] objetosGolpeados = Physics2D.OverlapCircleAll(puntoAtaque.position, rangoAtaque * 0.5f, capaJugador);
            
            foreach (Collider2D obj in objetosGolpeados)
            {
                PlayerController jugadorScript = obj.GetComponent<PlayerController>();
                if (jugadorScript != null)
                {
                    jugadorScript.RecibirDanio(danioAtaque);
                    Debug.Log($"¡Minotauro golpeó al jugador por {danioAtaque} de daño!");
                }
            }
        }

        // Esperar a que termine la animación completa
        yield return new WaitForSeconds(duracionAtaque * 0.6f);

        estaAtacando = false;

        // Cooldown antes del siguiente ataque
        yield return new WaitForSeconds(tiempoEntreAtaques);
        puedeAtacar = true;
    }

    public void RecibirDanio(int cantidad)
    {
        if (estaMuerto) return;

        // Aplicar defensa
        int danioFinal = Mathf.RoundToInt(cantidad * (1f - defensaBase));
        danioFinal = Mathf.Max(1, danioFinal); // Mínimo 1 de daño

        vidaActual -= danioFinal;
        vidaActual = Mathf.Max(0, vidaActual);

        Debug.Log($"Minotauro recibió {danioFinal} de daño ({cantidad} - {defensaBase * 100}% defensa). Vida: {vidaActual}/{vidaMaxima}");

        // Efectos visuales y de audio
        StartCoroutine(EfectoGolpe());
        ReproducirSonido(sonidoDanio);

        // Si recibe daño, agredir al jugador
        if (estadoActual == EstadoMinotauro.Idle || estadoActual == EstadoMinotauro.Patrullando)
        {
            CambiarEstado(EstadoMinotauro.Persiguiendo);
        }

        // Verificar muerte
        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    private IEnumerator EfectoGolpe()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = colorDanio;
            yield return new WaitForSeconds(duracionEfectoDanio);
            spriteRenderer.color = colorOriginal;
        }
    }

    private void Morir()
    {
        if (estaMuerto) return;

        estaMuerto = true;
        CambiarEstado(EstadoMinotauro.Muerto);

        Debug.Log("¡Minotauro eliminado!");

        // Detener movimiento
        rb.velocity = Vector2.zero;
        rb.simulated = false;

        // Desactivar colisión
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // Animación de muerte
        if (animator != null)
        {
            animator.SetTrigger(hashMorir);
        }

        // Sonido de muerte
        ReproducirSonido(sonidoMuerte);

        // Efecto de partículas
        if (efectoMuerte != null)
        {
            Instantiate(efectoMuerte, transform.position, Quaternion.identity);
        }

        // Destruir después de un tiempo
        Destroy(gameObject, tiempoAntesDeDestruir);
    }

    private void GenerarPuntoPatrulla()
    {
        Vector2 puntoAleatorio = Random.insideUnitCircle * rangoPatrullaje;
        destinoPatrulla = puntoInicial + puntoAleatorio;
    }

    private void BuscarJugador()
    {
        GameObject jugadorObj = GameObject.FindGameObjectWithTag("Player");
        if (jugadorObj != null)
        {
            jugador = jugadorObj.transform;
        }
    }

    private void VoltearSprite(float direccionX)
    {
        if (spriteRenderer != null && direccionX != 0)
        {
            spriteRenderer.flipX = direccionX < 0;
        }
    }

    private void CambiarEstado(EstadoMinotauro nuevoEstado)
    {
        if (estadoActual != nuevoEstado)
        {
            estadoActual = nuevoEstado;
            Debug.Log($"Minotauro -> {nuevoEstado}");
        }
    }

    private void ActualizarAnimaciones()
    {
        if (animator == null) return;

        // Velocidad para blend tree de caminar
        float velocidadActual = rb.velocity.magnitude;
        animator.SetFloat(hashVelocidad, velocidadActual);
    }

    private void InicializarHashesAnimacion()
    {
        hashCaminar = Animator.StringToHash("Idle");
        hashAtacar = Animator.StringToHash("Atacar");
        hashMorir = Animator.StringToHash("Morir");
        hashVelocidad = Animator.StringToHash("Velocidad");
    }

    private void ReproducirSonido(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    // Visualización en editor
    private void OnDrawGizmosSelected()
    {
        // Rango de detección (amarillo)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rangoDeteccion);

        // Rango de ataque (rojo)
        Gizmos.color = Color.red;
        if (puntoAtaque != null)
        {
            Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque * 0.5f);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, rangoAtaque);
        }

        // Rango de patrullaje (azul)
        if (patrullar)
        {
            Vector3 puntoInicio = Application.isPlaying ? puntoInicial : transform.position;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(puntoInicio, rangoPatrullaje);
        }
    }

    // Getters públicos
    public int ObtenerVidaActual() => vidaActual;
    public int ObtenerVidaMaxima() => vidaMaxima;
    public bool EstaMuerto() => estaMuerto;
    public EstadoMinotauro ObtenerEstado() => estadoActual;
}
