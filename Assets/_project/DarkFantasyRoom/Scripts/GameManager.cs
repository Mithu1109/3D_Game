using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public string[] spellOrder = { "Resplandecer", "Viento", "Oscuro", "Blaze" };
    public int currentSpellIndex = 0;

    public int totalCrystals = 5;
    public int collectedCrystals = 0;
    public bool hasSpellBook = false;

    public int blazeTargetsRequired = 3;
    public int blazeTargetsHit = 0;
    public bool blazeActive = false;

    public GameObject[] blazeEnemies;

    public float timeLimit = 180f; // 3 minutes
    private float currentTime;
    private bool timerActive = false;

    [Header("UI References")]
    public TextMeshProUGUI timerText;
    public TextMeshProUGUI crystalText;
    public TextMeshProUGUI spellText;

    [Header("Game Objects")]
    public GameObject spellBook;
    public GameObject rubyDagger;

    [Header("Audio Settings")]
    public AudioSource audioSource;
    public AudioClip victorySound;
    public AudioClip failureSound;
    public AudioClip unlockSound;

    public Color startColor = Color.green;
    public Color endColor = Color.red;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        crystalText.text = "Collect crystals to unlock spell book";
        spellBook.SetActive(false);
        spellText.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);

        foreach (GameObject enemy in blazeEnemies)
        {
            enemy.SetActive(false);
        }

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void CollectCrystal()
    {
        collectedCrystals++;
        UpdateUI();

        if (collectedCrystals == totalCrystals)
        {
            UnlockSpellBook();
        }
    }

    void UpdateUI()
    {
        if (collectedCrystals == 0)
        {
            crystalText.text = "Collect crystals to unlock spell book";
        }
        else if (collectedCrystals < totalCrystals)
        {
            crystalText.text = "Crystals: " + collectedCrystals + " / " + totalCrystals;
        }
        else
        {
            crystalText.text = "Spell Book Unlocked!";
        }
    }

    void UnlockSpellBook()
    {
        spellBook.SetActive(true);

        if (audioSource != null && unlockSound != null)
        {
            audioSource.PlayOneShot(unlockSound);
        }
    }

    public void OnSpellBookGrabbed()
    {
        hasSpellBook = true;

        crystalText.gameObject.SetActive(false);
        spellText.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);

        currentSpellIndex = 0;
        spellText.text = "Cast spells and protect the Ruby Dagger!\nFirst: " + spellOrder[currentSpellIndex];

        currentTime = timeLimit;
        timerActive = true;
    }

    public bool TryAdvanceSpell(string spellName)
    {
        if (spellOrder[currentSpellIndex] != spellName)
        {
            Debug.Log("Wrong spell! Expected: " + spellOrder[currentSpellIndex]);
            spellText.text = "Wrong Spell!\nNext: " + spellOrder[currentSpellIndex];
            return false;
        }

        if (spellName == "Blaze")
        {
            if (!blazeActive)
            {
                blazeActive = true;
                blazeTargetsHit = 0;
                spellText.text = "Blaze: Hit " + blazeTargetsRequired + " enemies!";
            }
            return true;
        }

        if (spellName == "Oscuro")
        {
            ActivateBlazeEnemies();
        }

        currentSpellIndex++;

        if (currentSpellIndex < spellOrder.Length)
        {
            spellText.text = "Next Spell: " + spellOrder[currentSpellIndex];
        }
        else
        {
            WinGame();
        }

        return true;
    }

    public void RegisterBlazeHit()
    {
        if (!blazeActive) return;

        blazeTargetsHit++;
        spellText.text = "Blaze: " + blazeTargetsHit + " / " + blazeTargetsRequired;

        if (blazeTargetsHit >= blazeTargetsRequired)
        {
            blazeActive = false;
            currentSpellIndex++;

            if (currentSpellIndex < spellOrder.Length)
            {
                spellText.text = "Next Spell: " + spellOrder[currentSpellIndex];
            }
            else
            {
                WinGame();
            }
        }
    }

    void WinGame()
    {
        timerActive = false;
        spellText.text = "You successfully completed the challenge! :)";

        if (audioSource != null && victorySound != null)
        {
            audioSource.PlayOneShot(victorySound);
        }
    }

    void ActivateBlazeEnemies()
    {
        foreach (GameObject enemy in blazeEnemies)
        {
            enemy.SetActive(true);
        }
    }

    void Update()
    {
        if (!timerActive) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            currentTime = 0;
            timerActive = false;
            UpdateTimerUI();
            OnTimeUp();
            return;
        }

        UpdateTimerUI();
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60f);
        int seconds = Mathf.FloorToInt(currentTime % 60f);

        timerText.text = $"Time: {minutes:00}:{seconds:00}";

        float t = 1 - (currentTime / timeLimit);
        timerText.color = Color.Lerp(startColor, endColor, t);

        if (currentTime <= 10f)
        {
            float blink = Mathf.PingPong(Time.time * 5f, 1f);
            timerText.color = Color.Lerp(Color.red, Color.white, blink);
        }
    }

    void OnTimeUp()
    {
        spellText.text = "You Failed to Protect the Dagger :(\nTime Ran Out";
        hasSpellBook = false;

        if (audioSource != null && failureSound != null)
        {
            audioSource.PlayOneShot(failureSound);
        }

        if (rubyDagger != null)
        {
            rubyDagger.SetActive(false);
        }

        Time.timeScale = 0f;
    }
}