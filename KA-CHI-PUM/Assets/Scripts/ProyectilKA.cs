using UnityEngine;

public class ProyectilKA : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private float danio = 10f;
    [SerializeField] private float tiempoVida = 3f;

    private void Start()
    {
        Destroy(gameObject, tiempoVida);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si colisiona con enemigo
        if (collision.CompareTag("Enemy"))
        {
            int danioEntero = Mathf.RoundToInt(danio);
            bool dañoAplicado = false;

            // Intentar dañar Enemigo normal
            Enemigo enemigo = collision.GetComponent<Enemigo>();
            if (enemigo != null)
            {
                enemigo.RecibirDanio(danioEntero);
                dañoAplicado = true;
                Debug.Log($"Proyectil golpeó a {collision.name} por {danioEntero} de daño");
            }

            // Intentar dañar Minotauro
            MinotauroController minotauro = collision.GetComponent<MinotauroController>();
            if (minotauro != null)
            {
                minotauro.RecibirDanio(danioEntero);
                dañoAplicado = true;
                Debug.Log($"Proyectil golpeó al Minotauro por {danioEntero} de daño");
            }

            // Si se aplicó daño, destruir proyectil
            if (dañoAplicado)
            {
                Destroy(gameObject);
            }
        }
    }
}
