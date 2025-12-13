using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Vector2 ultimaDireccionMostrada = Vector2.zero;
    private bool ultimoEstadoMovimiento = false;
    [Header("Movimiento")]
    public float velocidadMovimiento = 5f;
    public float suavizadoMovimiento = 0.1f;
    
    [Header("Combate")]
    public float rangoAtaque = 1f;
    public int danioAtaque = 10;
    public float tiempoEntreAtaques = 0.5f;
    public Transform puntoAtaque;
    public LayerMask capasEnemigos;

    [Header("Vida")]
    public int vidaMaxima = 100;
    private int vidaActual;

    // Referencias privadas
    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    // Variables de movimiento
    private Vector2 movimiento;
    private Vector2 movimientoSuavizado;
    private Vector2 velocidadActual;
    private Vector2 ultimaDireccion = Vector2.down;
    
    // Estados
    private bool estaAtacando = false;
    private bool puedeAtacar = true;

    // Hashes del Animator
    private int velocidadXHash;
    private int velocidadYHash;
    private int atacandoHash;
    private int enMovimientoHash;
    private int direccionAtaqueHash;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        vidaActual = vidaMaxima;

        
        velocidadXHash = Animator.StringToHash("Velocidad_X");
        velocidadYHash = Animator.StringToHash("Velocidad_Y");
        atacandoHash = Animator.StringToHash("Atacando");
        enMovimientoHash = Animator.StringToHash("En_Movimiento");
        direccionAtaqueHash = Animator.StringToHash("Direccion_Ataque");
    }

    void Update()
    {
        if (!estaAtacando)
        {
            movimiento.x = Input.GetAxisRaw("Horizontal");
            movimiento.y = Input.GetAxisRaw("Vertical");

            if (movimiento.magnitude > 0)
            {
                ultimaDireccion = movimiento.normalized;
            }
        }
        else
        {
            movimiento = Vector2.zero;
        }

        // Input de ataque
        if (Input.GetKeyDown(KeyCode.Space) && puedeAtacar && !estaAtacando)
        {
            StartCoroutine(EjecutarAtaque());
        }

        ActualizarAnimaciones();
    }

    void FixedUpdate()
    {
        if (!estaAtacando)
        {
            Vector2 movimientoObjetivo = movimiento.normalized * velocidadMovimiento;
            movimientoSuavizado = Vector2.SmoothDamp(
                movimientoSuavizado, 
                movimientoObjetivo, 
                ref velocidadActual, 
                suavizadoMovimiento
            );
            
            rb.velocity = movimientoSuavizado;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
    }

    void ActualizarAnimaciones()
    {
        if (animator == null) return;

        Vector2 direccionAnimacion = movimiento.magnitude > 0 ? movimiento.normalized : ultimaDireccion;
        bool enMovimientoActual = movimiento.magnitude > 0 && !estaAtacando;

        // SOLO mostrar debug cuando algo CAMBIE
        if (direccionAnimacion != ultimaDireccionMostrada || enMovimientoActual != ultimoEstadoMovimiento)
        {
            
            ultimaDireccionMostrada = direccionAnimacion;
            ultimoEstadoMovimiento = enMovimientoActual;
        }

        animator.SetFloat(velocidadXHash, direccionAnimacion.x);
        animator.SetFloat(velocidadYHash, direccionAnimacion.y);
        animator.SetBool(enMovimientoHash, enMovimientoActual);
    }

    IEnumerator EjecutarAtaque()
    {
        estaAtacando = true;
        puedeAtacar = false;

        movimiento = Vector2.zero;
        rb.velocity = Vector2.zero;
        movimientoSuavizado = Vector2.zero;

        if (animator != null)
        {
            int direccion = ObtenerDireccionAtaque();
            animator.SetInteger(direccionAtaqueHash, direccion);
            animator.SetTrigger(atacandoHash);
        }

        yield return new WaitForSeconds(0.2f);

        // DETECCIÓN DE ENEMIGOS MEJORADA
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoAtaque.position, rangoAtaque, capasEnemigos);
    
        foreach (Collider2D enemigo in enemigosGolpeados)
        {
            // Intentar dañar Enemigo normal
            Enemigo enemigoScript = enemigo.GetComponent<Enemigo>();
            if (enemigoScript != null)
            {
                enemigoScript.RecibirDanio(danioAtaque);
                Debug.Log($"Jugador golpeó a {enemigo.name} por {danioAtaque} de daño");
                continue; // Pasar al siguiente enemigo
            }

            // Intentar dañar Minotauro
            MinotauroController minotauroScript = enemigo.GetComponent<MinotauroController>();
            if (minotauroScript != null)
            {
                minotauroScript.RecibirDanio(danioAtaque);
                Debug.Log($"Jugador golpeó al Minotauro por {danioAtaque} de daño");
            }
        }

        yield return new WaitForSeconds(tiempoEntreAtaques - 0.2f);
    
        estaAtacando = false;
        puedeAtacar = true;
    }

    int ObtenerDireccionAtaque()
    {
        float anguloX = Mathf.Abs(ultimaDireccion.x);
        float anguloY = Mathf.Abs(ultimaDireccion.y);

        if (anguloY > anguloX)
        {
            return ultimaDireccion.y > 0 ? 1 : 0;
        }
        else
        {
            return ultimaDireccion.x > 0 ? 2 : 3;
        }
    }

    public void RecibirDanio(int cantidad)
    {
        vidaActual -= cantidad;
        Debug.Log("Jugador recibió " + cantidad + " de daño. Vida restante: " + vidaActual);

        StartCoroutine(EfectoGolpe());

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
        Debug.Log("El jugador ha muerto!");
    }

    void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
    }

    public int ObtenerVidaActual() { return vidaActual; }
    public int ObtenerVidaMaxima() { return vidaMaxima; }
    
    
}
