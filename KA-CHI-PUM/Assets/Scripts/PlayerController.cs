using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidadMovimiento = 5f;

    [Header("Combate")]
    public float rangoAtaque = 1f;
    public int dañoAtaque = 10;
    public float tiempoEntreAtaques = 0.5f;
    public Transform puntoAtaque;
    public LayerMask capasEnemigos;

    [Header("Vida")]
    public int vidaMaxima = 100;
    private int vidaActual;

    // Referencias privadas
    private Rigidbody2D rb;
    private Animator animator;
    private Vector2 movimiento;
    private Vector2 ultimaDireccion = Vector2.down;
    private bool puedeAtacar = true;
    private bool estaAtacando = false;

    // Nombres de parámetros del Animator
    private int velocidadXHash;
    private int velocidadYHash;
    private int atacandoHash;
    private int direccionAtaqueHash;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        vidaActual = vidaMaxima;

        // Cachear los hashes de los parámetros del Animator
        velocidadXHash = Animator.StringToHash("VelocidadX");
        velocidadYHash = Animator.StringToHash("VelocidadY");
        atacandoHash = Animator.StringToHash("Atacando");
        direccionAtaqueHash = Animator.StringToHash("DireccionAtaque");
    }

    void Update()
    {
        // Obtener input de movimiento
        if (!estaAtacando)
        {
            movimiento.x = Input.GetAxisRaw("Horizontal");
            movimiento.y = Input.GetAxisRaw("Vertical");

            // Guardar la última dirección si hay movimiento
            if (movimiento.magnitude > 0)
            {
                ultimaDireccion = movimiento.normalized;
            }

            // Actualizar animaciones de movimiento
            ActualizarAnimacionMovimiento();
        }

        // Input de ataque
        if (Input.GetKeyDown(KeyCode.Space) && puedeAtacar && !estaAtacando)
        {
            Atacar();
        }
    }

    void FixedUpdate()
    {
        // Aplicar movimiento
        if (!estaAtacando)
        {
            rb.MovePosition(rb.position + movimiento.normalized * velocidadMovimiento * Time.fixedDeltaTime);
        }
    }

    void ActualizarAnimacionMovimiento()
    {
        animator.SetFloat(velocidadXHash, movimiento.x);
        animator.SetFloat(velocidadYHash, movimiento.y);
    }

    void Atacar()
    {
        estaAtacando = true;
        puedeAtacar = false;

        // Determinar dirección de ataque (0=Abajo, 1=Arriba, 2=Derecha, 3=Izquierda)
        int direccion = ObtenerDireccionAtaque();
        animator.SetInteger(direccionAtaqueHash, direccion);
        animator.SetTrigger(atacandoHash);

        // Detectar enemigos en rango
        Collider2D[] enemigosGolpeados = Physics2D.OverlapCircleAll(puntoAtaque.position, rangoAtaque, capasEnemigos);

        // Aplicar daño a los enemigos
        foreach (Collider2D enemigo in enemigosGolpeados)
        {
            Enemigo enemigoScript = enemigo.GetComponent<Enemigo>();
            if (enemigoScript != null)
            {
                enemigoScript.RecibirDaño(dañoAtaque);
            }
        }

        // Reiniciar estado de ataque
        StartCoroutine(ReiniciarAtaque());
    }

    int ObtenerDireccionAtaque()
    {
        float anguloX = Mathf.Abs(ultimaDireccion.x);
        float anguloY = Mathf.Abs(ultimaDireccion.y);

        if (anguloY > anguloX)
        {
            return ultimaDireccion.y > 0 ? 1 : 0; // Arriba : Abajo
        }
        else
        {
            return ultimaDireccion.x > 0 ? 2 : 3; // Derecha : Izquierda
        }
    }

    IEnumerator ReiniciarAtaque()
    {
        yield return new WaitForSeconds(tiempoEntreAtaques);
        estaAtacando = false;
        puedeAtacar = true;
    }

    public void RecibirDaño(int cantidad)
    {
        vidaActual -= cantidad;

        if (vidaActual <= 0)
        {
            Morir();
        }
    }

    void Morir()
    {
        Debug.Log("¡El jugador ha muerto!");
        // Aquí puedes agregar lógica de game over
        // Por ejemplo: mostrar pantalla de game over, reiniciar nivel, etc.
    }

    // Visualizar el rango de ataque en el editor
    void OnDrawGizmosSelected()
    {
        if (puntoAtaque == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(puntoAtaque.position, rangoAtaque);
    }

    // Getters públicos
    public int ObtenerVidaActual() { return vidaActual; }
    public int ObtenerVidaMaxima() { return vidaMaxima; }
}
