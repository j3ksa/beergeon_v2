using UnityEngine;
using UnityEngine.Events;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth;

    public UnityEvent OnDeath;
    public UnityEvent<int> OnDamageTaken;

    [Header("Drops")]
    [SerializeField] private GameObject healthPotionPrefab;
    [SerializeField] private float dropChance = 0.3f;
    [SerializeField] private GameObject monkeyPotionPrefab;
    [SerializeField] private float monkeyDropChance = 0.15f;
    [SerializeField] private GameObject speedPotionPrefab;
    [SerializeField] private float speedPotionDropChance = 0.22f;
    [SerializeField] private GameObject bullFuelPotionPrefab;
    [SerializeField] private float bullFuelDropChance = 0.30f;
    [SerializeField] private GameObject ognistaWodaPrefab;
    [SerializeField] private float ognistaWodaDropChance = 0.20f;
    [SerializeField] private GameObject pitnaWiedzaPrefab;
    [SerializeField] private float pitnaWiedzaDropChance = 0.13f;
    [SerializeField] private GameObject slepySzalPrefab;
    [SerializeField] private float slepySzalDropChance = 0.03f;
    [SerializeField] private GameObject swiatloWidzeniaPrefab;
    [SerializeField] private float swiatloWidzeniaDropChance = 0.02f;
    [SerializeField] private GameObject kuszaPrefab;
    [SerializeField] private float kuszaDropChance = 0.05f;
    [SerializeField] private GameObject strzalaPrefab;
    [SerializeField] private float strzalaDropChance = 0.15f;
    [SerializeField] private GameObject zatrutyMieczPrefab;
    [SerializeField] private float zatrutyMieczDropChance = 0.02f;
    [SerializeField] private GameObject mlotekDoKotletowPrefab;
    [SerializeField] private float mlotekDoKotletowDropChance = 0.02f;
    [SerializeField] private GameObject mlekoStaregoPrefab;
    [SerializeField] private float mlekoStaregoDropChance = 0.10f;
    [SerializeField] private GameObject sztyletPrefab;
    [SerializeField] private float sztyletDropChance = 0.10f;
    [SerializeField] private GameObject wloczniaPrefab;
    [SerializeField] private float wloczniaDropChance = 0.04f;
    [SerializeField] private GameObject sierpPrefab;
    [SerializeField] private float sierpDropChance = 0.03f;
    [SerializeField] private GameObject wytrychPrefab;
    [SerializeField] private float wytrychDropChance = 0.08f;
    [SerializeField] private GameObject helmZelaznyPrefab;
    [SerializeField] private float helmZelaznyDropChance = 0.03f;
    [SerializeField] private GameObject klataZelaznaPrefab;
    [SerializeField] private float klataZelaznaDropChance = 0.015f;
    [SerializeField] private GameObject spodnieZelaznePrefab;
    [SerializeField] private float spodnieZelazneDropChance = 0.02f;
    [SerializeField] private GameObject butyZelaznePrefab;
    [SerializeField] private float butyZelazneDropChance = 0.03f;
    [SerializeField] private GameObject czapkaSkorzanaPrefab;
    [SerializeField] private float czapkaSkorzanaDropChance = 0.05f;
    [SerializeField] private GameObject kurtkaSkorzanaPrefab;
    [SerializeField] private float kurtkaSkorzanaDropChance = 0.035f;
    [SerializeField] private GameObject spodnieSkorzanePrefab;
    [SerializeField] private float spodnieSkorzaneDropChance = 0.04f;
    [SerializeField] private GameObject butySkorzanePrefab;
    [SerializeField] private float butySkorzaneDropChance = 0.05f;
    [SerializeField] private GameObject helmKapslowyPrefab;
    [SerializeField] private float helmKapslowyDropChance = 0.006f; // 0.6% szansy (Ultra Rare)
    [SerializeField] private GameObject pancerzKapslowyPrefab;
    [SerializeField] private float pancerzKapslowyDropChance = 0.003f; // 0.3% szansy (Legendary Jackpot)
    [SerializeField] private GameObject spodnieKapslowePrefab;
    [SerializeField] private float spodnieKapsloweDropChance = 0.005f; // 0.5% szansy (Ultra Rare)
    [SerializeField] private GameObject butyKapslowePrefab;
    [SerializeField] private float butyKapsloweDropChance = 0.006f; // 0.6% szansy (Ultra Rare)
#pragma warning disable 0414
    [SerializeField] private int xpReward = 25; // Przestarzałe: EXP przyznawany jest wyłącznie z kulek (EnemyExpDropper -> ExpItem)
#pragma warning restore 0414

    private SpriteRenderer[] renderers;
    private Color[] originalColors;
    private float flashEndTime;
    private bool isFlashing = false;

    private void Awake()
    {
        currentHealth = maxHealth;
        renderers = GetComponentsInChildren<SpriteRenderer>();
        
        if (renderers != null && renderers.Length > 0)
        {
            originalColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++)
            {
                originalColors[i] = renderers[i].color;
            }
        }
    }

    private void LateUpdate()
    {
        if (renderers == null || renderers.Length == 0) return;

        // Check if we are currently in the flash duration
        if (Time.time < flashEndTime)
        {
            isFlashing = true;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].color = Color.red;
            }
        }
        else if (isFlashing)
        {
            // We were flashing, but time is up. Restore colors once.
            isFlashing = false;
            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] != null) renderers[i].color = originalColors[i];
            }
        }
    }

    public int CurrentHealth => currentHealth;

    public void SetHealthDirectly(int newHealth)
    {
        currentHealth = newHealth;
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log($"{gameObject.name} took {damage} damage. Current Health: {currentHealth}");
        OnDamageTaken?.Invoke(damage);

        // Wyświetlenie latającej cyferki obrażeń
        DamageNumbers.DamagePopupManager.Show(transform.position + Vector3.up * 1.5f, damage);

        // Trigger Flash
        flashEndTime = Time.time + 0.1f;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        Debug.Log($"{gameObject.name} died.");
        OnDeath?.Invoke();
        
        // Drop Chance
        if (healthPotionPrefab != null && Random.value <= dropChance)
        {
            Instantiate(healthPotionPrefab, transform.position, Quaternion.identity);
        }

        if (monkeyPotionPrefab != null && Random.value <= monkeyDropChance)
        {
            Instantiate(monkeyPotionPrefab, transform.position, Quaternion.identity);
        }

        if (speedPotionPrefab != null && Random.value <= speedPotionDropChance)
        {
            Instantiate(speedPotionPrefab, transform.position, Quaternion.identity);
        }

        if (bullFuelPotionPrefab != null && Random.value <= bullFuelDropChance)
        {
            Instantiate(bullFuelPotionPrefab, transform.position, Quaternion.identity);
        }

        if (ognistaWodaPrefab != null && Random.value <= ognistaWodaDropChance)
        {
            Instantiate(ognistaWodaPrefab, transform.position, Quaternion.identity);
        }

        // XP jest przyznawany wyłącznie z wciąganych kulek doświadczenia (EnemyExpDropper -> ExpItem)
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();

        if (pitnaWiedzaPrefab != null)
        {
            float dynamicPitnaDropChance = pitnaWiedzaDropChance;
            if (playerStats != null && playerStats.Level > 1)
            {
                // Dynamiczne skalowanie rzadkości dropu z poziomem gracza (np. LVL 1: 13%, LVL 2: ~11.3%, LVL 3: ~10%...)
                dynamicPitnaDropChance = Mathf.Max(0.03f, pitnaWiedzaDropChance / (1f + (playerStats.Level - 1) * 0.15f));
            }

            if (Random.value <= dynamicPitnaDropChance)
            {
                Instantiate(pitnaWiedzaPrefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Ślepego Szału (3% szansy)
        if (slepySzalPrefab != null && Random.value <= slepySzalDropChance)
        {
            Instantiate(slepySzalPrefab, transform.position, Quaternion.identity);
        }

        // Drop Światła Widzenia (2% szansy)
        if (swiatloWidzeniaPrefab != null && Random.value <= swiatloWidzeniaDropChance)
        {
            Instantiate(swiatloWidzeniaPrefab, transform.position, Quaternion.identity);
        }

        // Drop Kuszy (5% szansy)
        if (Random.value <= kuszaDropChance)
        {
            GameObject prefab = kuszaPrefab != null ? kuszaPrefab : Resources.Load<GameObject>("Items/KuszaPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Strzał (15% szansy)
        if (Random.value <= strzalaDropChance)
        {
            GameObject prefab = strzalaPrefab != null ? strzalaPrefab : Resources.Load<GameObject>("Items/StrzalaPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Zatrutego Miecza (2% szansy)
        if (Random.value <= zatrutyMieczDropChance)
        {
            GameObject prefab = zatrutyMieczPrefab != null ? zatrutyMieczPrefab : Resources.Load<GameObject>("Items/ZatrutyMieczPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("ZatrutyMieczPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Młotka Do Kotletów (2% szansy)
        if (Random.value <= mlotekDoKotletowDropChance)
        {
            GameObject prefab = mlotekDoKotletowPrefab != null ? mlotekDoKotletowPrefab : Resources.Load<GameObject>("Items/MlotekDoKotletowPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("MlotekDoKotletowPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Mleka Starego (10% szansy)
        if (Random.value <= mlekoStaregoDropChance)
        {
            GameObject prefab = mlekoStaregoPrefab != null ? mlekoStaregoPrefab : Resources.Load<GameObject>("Items/MlekoStaregoPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("MlekoStaregoPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Sztyletu (10% szansy)
        if (Random.value <= sztyletDropChance)
        {
            GameObject prefab = sztyletPrefab != null ? sztyletPrefab : Resources.Load<GameObject>("Items/SztyletPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("SztyletPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Włóczni (4% szansy)
        if (Random.value <= wloczniaDropChance)
        {
            GameObject prefab = wloczniaPrefab != null ? wloczniaPrefab : Resources.Load<GameObject>("Items/WloczniaPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("WloczniaPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Sierpa (3% szansy)
        if (Random.value <= sierpDropChance)
        {
            GameObject prefab = sierpPrefab != null ? sierpPrefab : Resources.Load<GameObject>("Items/SierpPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("SierpPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Wytrychu (8% szansy)
        if (Random.value <= wytrychDropChance)
        {
            GameObject prefab = wytrychPrefab != null ? wytrychPrefab : Resources.Load<GameObject>("Items/WytrychPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("WytrychPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Żelaznego Hełmu (3% szansy)
        if (Random.value <= helmZelaznyDropChance)
        {
            GameObject prefab = helmZelaznyPrefab != null ? helmZelaznyPrefab : Resources.Load<GameObject>("Items/HelmZelaznyPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("HelmZelaznyPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Żelaznego Napierśnika (1.5% szansy)
        if (Random.value <= klataZelaznaDropChance)
        {
            GameObject prefab = klataZelaznaPrefab != null ? klataZelaznaPrefab : Resources.Load<GameObject>("Items/KlataZelaznaPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("KlataZelaznaPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Żelaznych Nogawek (2% szansy)
        if (Random.value <= spodnieZelazneDropChance)
        {
            GameObject prefab = spodnieZelaznePrefab != null ? spodnieZelaznePrefab : Resources.Load<GameObject>("Items/SpodnieZelaznePickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("SpodnieZelaznePickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Żelaznych Butów (3% szansy)
        if (Random.value <= butyZelazneDropChance)
        {
            GameObject prefab = butyZelaznePrefab != null ? butyZelaznePrefab : Resources.Load<GameObject>("Items/ButyZelaznePickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("ButyZelaznePickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Skórzanej Czapki (5% szansy)
        if (Random.value <= czapkaSkorzanaDropChance)
        {
            GameObject prefab = czapkaSkorzanaPrefab != null ? czapkaSkorzanaPrefab : Resources.Load<GameObject>("Items/CzapkaSkorzanaPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("CzapkaSkorzanaPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Skórzanej Kurtki (3.5% szansy)
        if (Random.value <= kurtkaSkorzanaDropChance)
        {
            GameObject prefab = kurtkaSkorzanaPrefab != null ? kurtkaSkorzanaPrefab : Resources.Load<GameObject>("Items/KurtkaSkorzanaPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("KurtkaSkorzanaPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Skórzanych Spodni (4% szansy)
        if (Random.value <= spodnieSkorzaneDropChance)
        {
            GameObject prefab = spodnieSkorzanePrefab != null ? spodnieSkorzanePrefab : Resources.Load<GameObject>("Items/SpodnieSkorzanePickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("SpodnieSkorzanePickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Skórzanych Butów (5% szansy)
        if (Random.value <= butySkorzaneDropChance)
        {
            GameObject prefab = butySkorzanePrefab != null ? butySkorzanePrefab : Resources.Load<GameObject>("Items/ButySkorzanePickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("ButySkorzanePickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Kapslowego Hełmu (1.5% szansy)
        if (Random.value <= helmKapslowyDropChance)
        {
            GameObject prefab = helmKapslowyPrefab != null ? helmKapslowyPrefab : Resources.Load<GameObject>("Items/HelmKapslowyPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("HelmKapslowyPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Kapslowego Pancerza (0.8% szansy)
        if (Random.value <= pancerzKapslowyDropChance)
        {
            GameObject prefab = pancerzKapslowyPrefab != null ? pancerzKapslowyPrefab : Resources.Load<GameObject>("Items/PancerzKapslowyPickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("PancerzKapslowyPickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Kapslowych Nogawek (1.2% szansy)
        if (Random.value <= spodnieKapsloweDropChance)
        {
            GameObject prefab = spodnieKapslowePrefab != null ? spodnieKapslowePrefab : Resources.Load<GameObject>("Items/SpodnieKapslowePickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("SpodnieKapslowePickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Drop Kapslowych Butów (1.5% szansy)
        if (Random.value <= butyKapsloweDropChance)
        {
            GameObject prefab = butyKapslowePrefab != null ? butyKapslowePrefab : Resources.Load<GameObject>("Items/ButyKapslowePickup");
            if (prefab == null) prefab = Resources.Load<GameObject>("ButyKapslowePickup");
            if (prefab != null)
            {
                Instantiate(prefab, transform.position, Quaternion.identity);
            }
        }

        // Zamiast niszczyć obiekt, deaktywujemy go, by SaveSystem mógł zapisać jego stan jako martwy
        gameObject.SetActive(false);
    }
}
