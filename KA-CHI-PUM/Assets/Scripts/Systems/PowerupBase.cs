using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ==================== POWERUP BASE ====================
public class PowerupBase : MonoBehaviour
{
    public enum TipoPowerup
    {
        Curacion,
        VelocidadTemporal,
        DañoTemporal,
        Invulnerabilidad,
        ExperienciaExtra
    }

    public TipoPowerup tipo;
    public float valor = 50f;
    public float duracion = 5f;

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerStats stats = collision.GetComponent<PlayerStats>();
            PlayerExperience exp = collision.GetComponent<PlayerExperience>();

            switch (tipo)
            {
                case TipoPowerup.Curacion:
                    if (stats != null) stats.Curar(valor);
                    break;

                case TipoPowerup.Invulnerabilidad:
                    if (stats != null) stats.ActivarInvulnerabilidad(duracion);
                    break;

                case TipoPowerup.ExperienciaExtra:
                    if (exp != null) exp.AñadirExperiencia((int)valor);
                    break;
            }

            Destroy(gameObject);
        }
    }
}
