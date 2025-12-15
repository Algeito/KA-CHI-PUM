using UnityEngine;

public class ProyectilEnemigo : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int danio = 5;
    [SerializeField] private float tiempoVida = 5f;
    [SerializeField] private bool destruirAlChocar = true;

    private void Start()
    {
        // Destruir proyectil después del tiempo de vida
        Destroy(gameObject, tiempoVida);
    }

    public void ConfigurarDanio(int nuevoDanio)
    {
        danio = nuevoDanio;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Detectar al jugador
        if (collision.CompareTag("Player"))
        {
            PlayerController jugador = collision.GetComponent<PlayerController>();
            if (jugador != null)
            {
                jugador.RecibirDanio(danio);
                Debug.Log($"Proyectil enemigo golpeó al jugador por {danio} de daño");
            }

            // Destruir proyectil
            if (destruirAlChocar)
            {
                Destroy(gameObject);
            }
        }
        // Destruir si choca con paredes u obstáculos
        else if (collision.CompareTag("Wall") || collision.gameObject.layer == LayerMask.NameToLayer("Obstacles"))
        {
            Debug.Log("Proyectil chocó con obstáculo");
            Destroy(gameObject);
        }
    }

    private void OnBecameInvisible()
    {
        // Destruir si sale de la cámara (opcional, para optimizar)
        Destroy(gameObject, 1f);
    }
}
