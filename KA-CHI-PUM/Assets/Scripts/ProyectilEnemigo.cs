using UnityEngine;

/// <summary>
/// Proyectil disparado por enemigos que daña al jugador
/// </summary>
public class ProyectilEnemigo : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int danio = 10;
    [SerializeField] private float tiempoVida = 5f;
    [SerializeField] private float velocidad = 8f;

    private Vector2 direccion;
    private Rigidbody2D rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Auto-destruirse después de X segundos
        Destroy(gameObject, tiempoVida);
    }

    private void FixedUpdate()
    {
        // Mover el proyectil en la dirección establecida
        if (rb != null && direccion != Vector2.zero)
        {
            rb.velocity = direccion * velocidad;
        }
    }

    /// <summary>
    /// Establece la dirección en la que viajará el proyectil
    /// </summary>
    public void EstablecerDireccion(Vector2 nuevaDireccion)
    {
        direccion = nuevaDireccion.normalized;

        // Rotar el sprite para que apunte en la dirección correcta
        float angulo = Mathf.Atan2(direccion.y, direccion.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angulo);
    }

    /// <summary>
    /// Establece el daño que hace el proyectil
    /// </summary>
    public void EstablecerDanio(int nuevoDanio)
    {
        danio = nuevoDanio;
    }

    /// <summary>
    /// Establece la velocidad del proyectil
    /// </summary>
    public void EstablecerVelocidad(float nuevaVelocidad)
    {
        velocidad = nuevaVelocidad;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si colisiona con el jugador
        if (collision.CompareTag("Player"))
        {
            PlayerController jugador = collision.GetComponent<PlayerController>();
            if (jugador != null)
            {
                jugador.RecibirDanio(danio);
                Debug.Log($"Proyectil enemigo golpeó al jugador por {danio} de daño");
            }

            // Destruir proyectil
            Destroy(gameObject);
        }

        // Si colisiona con paredes u obstáculos
        if (collision.CompareTag("Wall") || collision.CompareTag("Obstacle"))
        {
            Destroy(gameObject);
        }
    }
}
