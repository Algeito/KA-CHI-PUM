using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    // Singleton
    public static AudioManager instance;

    [Header("═══ MÚSICA ═══")]
    [SerializeField] private AudioClip musicaLobby;
    [SerializeField] private AudioClip musicaNivel1;
    [SerializeField] private AudioClip musicaGameOver;
    
    [Header("═══ CONFIGURACIÓN ═══")]
    [Range(0f, 1f)]
    [SerializeField] private float volumenMusica = 0.5f;
    [SerializeField] private bool fadeEntreCanciones = true;
    [SerializeField] private float tiempoFade = 1f;

    [Header("═══ EFECTOS DE SONIDO ═══")]
    [SerializeField] private AudioClip sonidoBoton;
    [SerializeField] private AudioClip sonidoInicio;
    [Range(0f, 1f)]
    [SerializeField] private float volumenEfectos = 0.7f;

    private AudioSource audioSourceMusica;
    private AudioSource audioSourceEfectos;
    private string escenaActual;
    private bool estaCambiandoMusica = false;

    private void Awake()
    {
        // Implementar Singleton
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            InicializarAudioSources();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        // Subscribirse al evento de cambio de escena
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // Reproducir música de la escena inicial
        escenaActual = SceneManager.GetActiveScene().name;
        Debug.Log($"AudioManager iniciado en escena: {escenaActual}");
        ReproducirMusicaDeEscena(escenaActual);
    }

    private void OnDestroy()
    {
        // Desubscribirse del evento
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    private void InicializarAudioSources()
    {
        // AudioSource para música
        audioSourceMusica = gameObject.AddComponent<AudioSource>();
        audioSourceMusica.loop = true;
        audioSourceMusica.playOnAwake = false;
        audioSourceMusica.volume = volumenMusica;

        // AudioSource para efectos
        audioSourceEfectos = gameObject.AddComponent<AudioSource>();
        audioSourceEfectos.loop = false;
        audioSourceEfectos.playOnAwake = false;
        audioSourceEfectos.volume = volumenEfectos;

        Debug.Log("AudioManager inicializado correctamente");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string nombreEscena = scene.name;
        Debug.Log($"=== ESCENA CARGADA: {nombreEscena} ===");
        Debug.Log($"Escena anterior: {escenaActual}");
        
        // SIEMPRE cambiar música al cargar una nueva escena
        escenaActual = nombreEscena;
        ReproducirMusicaDeEscena(nombreEscena);
    }

    private void ReproducirMusicaDeEscena(string nombreEscena)
    {
        AudioClip musicaNueva = null;

        Debug.Log($"Buscando música para escena: {nombreEscena}");

        // Determinar qué música reproducir según la escena
        // Convertir a minúsculas para evitar problemas de mayúsculas
        string escenaLower = nombreEscena.ToLower();

        if (escenaLower.Contains("menu") || escenaLower.Contains("lobby") || escenaLower.Contains("principal"))
        {
            musicaNueva = musicaLobby;
            Debug.Log("Música seleccionada: Lobby");
        }
        else if (escenaLower.Contains("nivel") || escenaLower.Contains("game") || escenaLower.Contains("level"))
        {
            musicaNueva = musicaNivel1;
            Debug.Log("Música seleccionada: Nivel 1");
        }
        else if (escenaLower.Contains("gameover") || escenaLower.Contains("over"))
        {
            musicaNueva = musicaGameOver;
            Debug.Log("Música seleccionada: Game Over");
        }
        else
        {
            Debug.LogWarning($"No hay música asignada para la escena: {nombreEscena}");
            return;
        }

        // Reproducir la música nueva
        if (musicaNueva != null)
        {
            // Verificar si ya está sonando esta música
            if (audioSourceMusica.clip == musicaNueva && audioSourceMusica.isPlaying)
            {
                Debug.Log($"La música '{musicaNueva.name}' ya está sonando");
                return;
            }

            if (fadeEntreCanciones)
            {
                if (!estaCambiandoMusica)
                {
                    StartCoroutine(CambiarMusicaConFade(musicaNueva));
                }
            }
            else
            {
                CambiarMusicaDirecta(musicaNueva);
            }
        }
        else
        {
            Debug.LogError($"No hay clip de audio asignado para la escena: {nombreEscena}");
        }
    }

    private void CambiarMusicaDirecta(AudioClip nuevaMusica)
    {
        audioSourceMusica.Stop();
        audioSourceMusica.clip = nuevaMusica;
        audioSourceMusica.volume = volumenMusica;
        audioSourceMusica.Play();

        Debug.Log($"Reproduciendo: {nuevaMusica.name}");
    }

    private IEnumerator CambiarMusicaConFade(AudioClip nuevaMusica)
    {
        estaCambiandoMusica = true;

        Debug.Log($"Iniciando fade out de: {(audioSourceMusica.clip != null ? audioSourceMusica.clip.name : "ninguna")}");

        // Fade Out (bajar volumen)
        float volumenInicial = audioSourceMusica.volume;
        float tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoFade)
        {
            tiempoTranscurrido += Time.unscaledDeltaTime; // Usar unscaledDeltaTime para que funcione incluso si Time.timeScale = 0
            audioSourceMusica.volume = Mathf.Lerp(volumenInicial, 0f, tiempoTranscurrido / tiempoFade);
            yield return null;
        }

        audioSourceMusica.volume = 0f;

        // Cambiar la canción
        audioSourceMusica.Stop();
        audioSourceMusica.clip = nuevaMusica;
        audioSourceMusica.Play();

        Debug.Log($"Reproduciendo con fade: {nuevaMusica.name}");

        // Fade In (subir volumen)
        tiempoTranscurrido = 0f;

        while (tiempoTranscurrido < tiempoFade)
        {
            tiempoTranscurrido += Time.unscaledDeltaTime;
            audioSourceMusica.volume = Mathf.Lerp(0f, volumenMusica, tiempoTranscurrido / tiempoFade);
            yield return null;
        }

        audioSourceMusica.volume = volumenMusica;
        estaCambiandoMusica = false;

        Debug.Log($"Fade completado. Volumen final: {audioSourceMusica.volume}");
    }

    // ═══ MÉTODOS PÚBLICOS ═══

    public void ReproducirEfecto(AudioClip efecto)
    {
        if (efecto != null && audioSourceEfectos != null)
        {
            audioSourceEfectos.PlayOneShot(efecto, volumenEfectos);
        }
    }

    public void ReproducirSonidoBoton()
    {
        ReproducirEfecto(sonidoBoton);
    }

    public void ReproducirSonidoInicio()
    {
        ReproducirEfecto(sonidoInicio);
    }

    public void CambiarVolumenMusica(float nuevoVolumen)
    {
        volumenMusica = Mathf.Clamp01(nuevoVolumen);
        audioSourceMusica.volume = volumenMusica;
    }

    public void CambiarVolumenEfectos(float nuevoVolumen)
    {
        volumenEfectos = Mathf.Clamp01(nuevoVolumen);
        audioSourceEfectos.volume = volumenEfectos;
    }

    public void MutearMusica(bool mutear)
    {
        audioSourceMusica.mute = mutear;
    }

    public void MutearEfectos(bool mutear)
    {
        audioSourceEfectos.mute = mutear;
    }

    public void PausarMusica()
    {
        audioSourceMusica.Pause();
    }

    public void ReanudarMusica()
    {
        audioSourceMusica.UnPause();
    }

    public void DetenerMusica()
    {
        audioSourceMusica.Stop();
    }

    // Método público para forzar cambio de música (por si acaso)
    public void CambiarMusicaManualmente(string nombreEscena)
    {
        Debug.Log($"Cambio manual de musica solicitado para: {nombreEscena}");
        ReproducirMusicaDeEscena(nombreEscena);
    }
}
