using UnityEngine;
using UnityEngine.Windows.Speech;
using System.Collections.Generic;
using System.Linq;

public class SpellCaster : MonoBehaviour
{
    public GameObject sparklePrefab;
    public Transform spawnPoint;
    public FloatingObject dagger;

    [Header("Audio Settings")]
    public AudioSource audioSource; // Drag an AudioSource here
    public AudioClip resplandecerSound;
    public AudioClip vientoSound;
    public AudioClip oscuroSound;
    public AudioClip blazeHitSound; // This plays only on enemy hit

    private KeywordRecognizer keywordRecognizer;
    private Dictionary<string, System.Action> keywords = new Dictionary<string, System.Action>();

    void Start()
    {
        // Setup AudioSource if not assigned
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        keywords.Add("Resplandecer", () => TryCastResplandecer());
        keywords.Add("Viento", () => TryCastViento());
        keywords.Add("Blaze", () => TryCastBlaze());
        keywords.Add("Oscuro", () => TryCastOscuro());

        keywordRecognizer = new KeywordRecognizer(keywords.Keys.ToArray());
        keywordRecognizer.OnPhraseRecognized += OnPhraseRecognized;
        keywordRecognizer.Start();
    }

    private void OnPhraseRecognized(PhraseRecognizedEventArgs args)
    {
        System.Action keywordAction;

        if (keywords.TryGetValue(args.text, out keywordAction))
        {
            keywordAction.Invoke();
        }
    }

    void CastBlaze()
    {
        Ray ray = new Ray(spawnPoint.position, spawnPoint.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 50f))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                // Play sound ONLY when an enemy is hit
                PlaySpellSound(blazeHitSound);

                Destroy(hit.collider.gameObject);
                Instantiate(sparklePrefab, hit.point, Quaternion.identity);
                GameManager.instance.RegisterBlazeHit();
            }
        }
    }

    void CastResplandecer()
    {
        PlaySpellSound(resplandecerSound);

        GameObject effect = Instantiate(sparklePrefab, spawnPoint.position, Quaternion.identity);

        ParticleSystem ps = effect.GetComponent<ParticleSystem>();
        ps.Play();

        Destroy(effect, ps.main.duration);
    }

    void CastViento()
    {
        PlaySpellSound(vientoSound);

        if (dagger != null)
            dagger.StartFloating();
    }

    void CastOscuro()
    {
        PlaySpellSound(oscuroSound);

        MagicCandle[] allCandles =
            FindObjectsByType<MagicCandle>(FindObjectsSortMode.None);

        foreach (MagicCandle candle in allCandles)
        {
            candle.SnuffOut(3f);
        }
    }

    // Helper method to play sounds without overlapping issues
    void PlaySpellSound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    void TryCastResplandecer()
    {
        if (GameManager.instance.hasSpellBook &&
            GameManager.instance.TryAdvanceSpell("Resplandecer"))
            CastResplandecer();
    }

    void TryCastViento()
    {
        if (GameManager.instance.hasSpellBook &&
            GameManager.instance.TryAdvanceSpell("Viento"))
            CastViento();
    }

    void TryCastOscuro()
    {
        if (GameManager.instance.hasSpellBook &&
            GameManager.instance.TryAdvanceSpell("Oscuro"))
            CastOscuro();
    }

    void TryCastBlaze()
    {
        if (GameManager.instance.hasSpellBook &&
            GameManager.instance.TryAdvanceSpell("Blaze"))
            CastBlaze();
    }

    private void OnApplicationQuit()
    {
        if (keywordRecognizer != null && keywordRecognizer.IsRunning)
        {
            keywordRecognizer.Stop();
            keywordRecognizer.Dispose();
        }
    }
}