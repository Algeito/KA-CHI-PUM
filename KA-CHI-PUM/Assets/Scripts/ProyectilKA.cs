using UnityEngine;

public class ProyectilKA : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float danio = 10f;
    [SerializeField] private float tiempoVida = 3f;

    private void Start()
    {
        // Auto-destruirse después de X segundos
        Destroy(gameObject, tiempoVida);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si colisiona con enemigo
        if (collision.CompareTag("Enemy"))
        {
            // Intentar hacer daño usando el método correcto de tu script
            Enemigo enemigo = collision.GetComponent<Enemigo>();
            if (enemigo != null)
            {
                // Convertir float a int porque tu método recibe int
                int danioEntero = Mathf.RoundToInt(danio);
                enemigo.RecibirDanio(danioEntero);
                Debug.Log($"Proyectil golpeó a {collision.name} por {danioEntero} de daño");
            }
            
            // Destruir proyectil
            Destroy(gameObject);
        }
    }
}
