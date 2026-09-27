using UnityEngine;
using CurrencySystem;

namespace Praktyki.KowadloUlepszen
{
    /// <summary>
    /// Główny kontroler kowadła w świecie gry.
    /// Implementuje IInteractable, monitoruje odległość oraz kąt patrzenia gracza,
    /// a także odtwarza efekty audiowizualne (brzdęk metalu, iskry).
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class KowadloController : MonoBehaviour, IInteractable
    {
        [Header("Zasięg i Wykrywanie")]
        [Tooltip("Maksymalna odległość gracza od kowadła umożliwiająca interakcję")]
        [SerializeField] private float interactionDistance = 3.5f;

        [Tooltip("Czułość patrzenia na kowadło (1 = prosto w punkt, 0 = 90 stopni)")]
        [SerializeField] private float lookAngleThreshold = 0.45f;

        [Header("Efekty Audiowizualne")]
        [SerializeField] private ParticleSystem sparksParticleSystem;
        [SerializeField] private AudioClip upgradeSound;
        [SerializeField] private AudioSource audioSource;

        [Header("Referencje UI")]
        [SerializeField] private KowadloPromptUI promptUI;

        private Camera playerCamera;
        private bool isPlayerFocusing = false;
        private static AudioClip proceduralAnvilClip = null;

        public string InteractionPrompt => "[E] ULEPSZ BROŃ NA KOWADLE";

        private void Awake()
        {
            EnsureCollider();
            EnsureComponents();
        }

        private void Start()
        {
            if (playerCamera == null) playerCamera = Camera.main;
            EnsurePromptUI();
            EnsureSparksParticles();
            AlignSparksToTop();
        }

        private void EnsureCollider()
        {
            MeshFilter mf = GetComponent<MeshFilter>();
            Collider col = GetComponent<Collider>();

            // Jeśli obiekt posiada model 3D (MeshFilter)
            if (mf != null && mf.sharedMesh != null)
            {
                // Jeśli jest to stary lub niedopasowany BoxCollider, zamień na precyzyjny MeshCollider
                if (col is BoxCollider)
                {
                    Destroy(col);
                    col = null;
                }

                if (col == null)
                {
                    MeshCollider mc = gameObject.AddComponent<MeshCollider>();
                    mc.convex = true;
                    mc.sharedMesh = mf.sharedMesh;
                }
            }
            else if (col == null)
            {
                gameObject.AddComponent<BoxCollider>();
            }
        }

        private void EnsureComponents()
        {
            if (audioSource == null)
            {
                audioSource = GetComponent<AudioSource>();
                if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
                audioSource.playOnAwake = false;
                audioSource.spatialBlend = 0.8f; // Dźwięk 3D
                audioSource.maxDistance = 15f;
            }
        }

        private void EnsurePromptUI()
        {
            if (promptUI == null)
            {
                promptUI = KowadloPromptUI.Instance;
                if (promptUI == null)
                {
                    promptUI = FindFirstObjectByType<KowadloPromptUI>();
                }
                if (promptUI == null)
                {
                    GameObject uiObj = new GameObject("Kowadlo_PromptManager", typeof(RectTransform));
                    promptUI = uiObj.AddComponent<KowadloPromptUI>();
                }
            }
        }

        private void EnsureSparksParticles()
        {
            if (sparksParticleSystem == null)
            {
                sparksParticleSystem = GetComponentInChildren<ParticleSystem>();
            }

            if (sparksParticleSystem == null)
            {
                GameObject sparksObj = new GameObject("Kowadlo_Sparks");
                sparksObj.transform.SetParent(transform, false);
                sparksParticleSystem = sparksObj.AddComponent<ParticleSystem>();
            }

            AlignSparksToTop();
            // Zawsze upewnij się, że cząsteczki mają materiał z gwiazdkami i nie są fioletowe
            ConfigureStarParticleSystem(sparksParticleSystem);
        }

        public void AlignSparksToTop()
        {
            if (sparksParticleSystem == null) return;

            Renderer r = GetComponentInChildren<Renderer>();
            Vector3 topPos = (r != null)
                ? new Vector3(r.bounds.center.x, r.bounds.max.y + 0.05f, r.bounds.center.z)
                : transform.position + Vector3.up * 0.55f;

            Transform sparksT = sparksParticleSystem.transform;
            sparksT.position = topPos;
            sparksT.rotation = Quaternion.Euler(-90f, 0f, 0f); // Stożek iskier skierowany pionowo w górę

            // Kompensacja skali rodzica (np. FBX przeskalowany x53), aby iskry miały naturalny rozmiar 1:1
            Vector3 parentLossy = transform.lossyScale;
            if (Mathf.Abs(parentLossy.x) > 0.001f && Mathf.Abs(parentLossy.y) > 0.001f && Mathf.Abs(parentLossy.z) > 0.001f)
            {
                sparksT.localScale = new Vector3(
                    1f / Mathf.Abs(parentLossy.x),
                    1f / Mathf.Abs(parentLossy.y),
                    1f / Mathf.Abs(parentLossy.z)
                );
            }
            else
            {
                sparksT.localScale = Vector3.one;
            }
        }

        private static Material starMaterialInstance = null;
        private static Texture2D starTextureInstance = null;

        public static Texture2D GetStarTexture()
        {
            if (starTextureInstance != null) return starTextureInstance;

#if UNITY_EDITOR
            Texture2D loaded = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Praktyki/KowadloUlepszen/StarParticle.png");
            if (loaded != null)
            {
                starTextureInstance = loaded;
                return starTextureInstance;
            }
#endif
            // Proceduralna gwiazdka jako niezawodny fallback
            int size = 64;
            starTextureInstance = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float cx = 31.5f;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x - cx) / cx;
                    float dy = (y - cx) / cx;
                    float r = Mathf.Sqrt(dx * dx + dy * dy);
                    if (r > 1f) { starTextureInstance.SetPixel(x, y, Color.clear); continue; }

                    float core = Mathf.Exp(-r * 4.5f);
                    float beamX = Mathf.Exp(-Mathf.Abs(dy) * 12f) * Mathf.Max(0f, 1f - Mathf.Abs(dx));
                    float beamY = Mathf.Exp(-Mathf.Abs(dx) * 12f) * Mathf.Max(0f, 1f - Mathf.Abs(dy));
                    float diagDist = Mathf.Abs(Mathf.Abs(dx) - Mathf.Abs(dy));
                    float diag = Mathf.Exp(-diagDist * 14f) * Mathf.Max(0f, 0.5f - r) * 0.5f;

                    float alpha = Mathf.Clamp01(core * 1.3f + beamX + beamY + diag);
                    starTextureInstance.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            starTextureInstance.Apply();
            return starTextureInstance;
        }

        public static Material GetStarMaterial()
        {
            if (starMaterialInstance != null) return starMaterialInstance;

#if UNITY_EDITOR
            Material loadedMat = UnityEditor.AssetDatabase.LoadAssetAtPath<Material>("Assets/Praktyki/KowadloUlepszen/M_KowadloStar.mat");
            if (loadedMat != null)
            {
                starMaterialInstance = loadedMat;
                return starMaterialInstance;
            }
#endif
            Shader s = Shader.Find("Sprites/Default")
                    ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                    ?? Shader.Find("Universal Render Pipeline/Particles/Unlit");

            starMaterialInstance = new Material(s);
            starMaterialInstance.name = "M_KowadloStar_Transparent";

            Texture2D starTex = GetStarTexture();
            if (starTex != null)
            {
                starMaterialInstance.mainTexture = starTex;
                if (starMaterialInstance.HasProperty("_BaseMap")) starMaterialInstance.SetTexture("_BaseMap", starTex);
            }
            return starMaterialInstance;
        }

        public static void ConfigureStarParticleSystem(ParticleSystem ps)
        {
            if (ps == null) return;

            var main = ps.main;
            main.playOnAwake = false;
            main.loop = false;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.35f, 0.65f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.0f, 4.2f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.08f); // Małe, drobne, lśniące gwiazdki
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 360f * Mathf.Deg2Rad);
            main.startColor = new ParticleSystem.MinMaxGradient(
                new Color(1f, 0.95f, 0.4f, 1f), // Lśniący złoty
                new Color(1f, 0.75f, 0.2f, 1f)  // Ciepły bursztyn
            );
            main.gravityModifier = 0.9f;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 22) });

            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = 0.15f;

            // Size over lifetime (płynne zanikanie gwiazdek)
            var sizeOverLifetime = ps.sizeOverLifetime;
            sizeOverLifetime.enabled = true;
            AnimationCurve sizeCurve = new AnimationCurve();
            sizeCurve.AddKey(0f, 1f);
            sizeCurve.AddKey(0.6f, 0.85f);
            sizeCurve.AddKey(1f, 0f);
            sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, sizeCurve);

            // Rotation over lifetime (delikatny obrót gwiazdek w locie)
            var rotationOverLifetime = ps.rotationOverLifetime;
            rotationOverLifetime.enabled = true;
            rotationOverLifetime.z = new ParticleSystem.MinMaxCurve(-180f * Mathf.Deg2Rad, 180f * Mathf.Deg2Rad);

            // Przypisanie przezroczystego materiału z teksturą gwiazdki
            ParticleSystemRenderer renderer = ps.GetComponent<ParticleSystemRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetStarMaterial();
            }
        }

        private void Update()
        {
            if (playerCamera == null)
            {
                playerCamera = Camera.main;
                if (playerCamera == null) return;
            }

            bool inRange = CheckDistance();
            bool isLooking = IsPlayerLooking();

            if (inRange && isLooking)
            {
                isPlayerFocusing = true;
                EnsurePromptUI();
                if (promptUI != null)
                {
                    Item targetItem = GetTargetUpgradeItem();
                    int caps = PlayerCurrency.Instance != null ? PlayerCurrency.Instance.CurrentCaps : 0;
                    WeaponUpgradeManager upgradeManager = EnsureUpgradeManager();

                    promptUI.UpdatePromptContent(targetItem, caps, upgradeManager);
                    promptUI.Show();
                }
            }
            else if (isPlayerFocusing)
            {
                isPlayerFocusing = false;
                if (promptUI != null)
                {
                    promptUI.Hide();
                }
            }
        }

        private void OnDisable()
        {
            if (isPlayerFocusing && promptUI != null)
            {
                promptUI.Hide();
                isPlayerFocusing = false;
            }
        }

        public Vector3 GetAnvilCenter()
        {
            Renderer r = GetComponentInChildren<Renderer>();
            if (r != null) return r.bounds.center;
            Collider c = GetComponent<Collider>();
            if (c != null) return c.bounds.center;
            return transform.position;
        }

        private bool CheckDistance()
        {
            if (playerCamera == null) return false;
            float dist = Vector3.Distance(GetAnvilCenter(), playerCamera.transform.position);
            return dist <= interactionDistance;
        }

        private bool IsPlayerLooking()
        {
            if (playerCamera == null) return false;
            Vector3 dirToAnvil = (GetAnvilCenter() - playerCamera.transform.position).normalized;
            float dot = Vector3.Dot(playerCamera.transform.forward, dirToAnvil);
            return dot >= lookAngleThreshold;
        }

        private Item GetHeldItem()
        {
            if (Inventory.instance != null)
            {
                return Inventory.instance.GetActiveItem();
            }
            PlayerHand hand = FindFirstObjectByType<PlayerHand>();
            if (hand != null)
            {
                return hand.CurrentItem;
            }
            return null;
        }

        private Item GetTargetUpgradeItem()
        {
            Item held = GetHeldItem();
            if (held != null) return held;

            // Jeśli gracz ma puste ręce, sprawdź czy ma założoną zbroję do ulepszenia
            if (Praktyki.EQ.MinecraftInventoryUI.Instance != null && Praktyki.EQ.MinecraftInventoryUI.Instance.equippedArmor != null)
            {
                var mgr = EnsureUpgradeManager();
                for (int i = 0; i < Praktyki.EQ.MinecraftInventoryUI.Instance.equippedArmor.Length; i++)
                {
                    if (Praktyki.EQ.MinecraftInventoryUI.Instance.equippedArmor[i] is Praktyki.Zbroja.ArmorItem a)
                    {
                        if (mgr == null || !mgr.IsMaxLevel(a))
                            return a;
                    }
                }
                for (int i = 0; i < Praktyki.EQ.MinecraftInventoryUI.Instance.equippedArmor.Length; i++)
                {
                    if (Praktyki.EQ.MinecraftInventoryUI.Instance.equippedArmor[i] is Praktyki.Zbroja.ArmorItem a)
                        return a;
                }
            }

            return null;
        }

        private WeaponUpgradeManager EnsureUpgradeManager()
        {
            if (WeaponUpgradeManager.Instance != null) return WeaponUpgradeManager.Instance;

            WeaponUpgradeManager mgr = FindFirstObjectByType<WeaponUpgradeManager>();
            if (mgr == null)
            {
                GameObject pObj = GameObject.FindGameObjectWithTag("Player");
                if (pObj != null)
                {
                    mgr = pObj.AddComponent<WeaponUpgradeManager>();
                }
                else
                {
                    GameObject mgrObj = new GameObject("WeaponUpgradeManager");
                    mgr = mgrObj.AddComponent<WeaponUpgradeManager>();
                }
            }
            return mgr;
        }

        /// <summary>
        /// Wywoływane przez system interakcji gracza (PlayerInteraction) po wciśnięciu [E].
        /// </summary>
        public bool Interact(GameObject interactor)
        {
            if (!CheckDistance()) return false;

            Item targetItem = GetTargetUpgradeItem();
            WeaponUpgradeManager upgradeMgr = EnsureUpgradeManager();

            if (upgradeMgr == null)
            {
                Debug.LogError("[Kowadlo] Brak WeaponUpgradeManager w scenie!");
                return false;
            }

            bool upgraded = false;
            string feedback = "";

            if (targetItem is Praktyki.Zbroja.ArmorItem armor)
            {
                upgraded = upgradeMgr.TryUpgradeArmor(armor, out feedback);
            }
            else
            {
                upgraded = upgradeMgr.TryUpgradeWeapon(targetItem, out feedback);
            }

            if (upgraded)
            {
                // Sukces ulepszenia!
                PlayUpgradeAudio();
                PlaySparks();

                if (promptUI != null)
                {
                    int caps = PlayerCurrency.Instance != null ? PlayerCurrency.Instance.CurrentCaps : 0;
                    promptUI.UpdatePromptContent(targetItem, caps, upgradeMgr);
                    promptUI.TriggerUpgradePulse();
                }

                return true;
            }
            else
            {
                // Informacja zwrotna przy niepowodzeniu
                Debug.Log($"<color=#FFAA00>[Kowadlo]</color> {feedback}");
                if (promptUI != null)
                {
                    int caps = PlayerCurrency.Instance != null ? PlayerCurrency.Instance.CurrentCaps : 0;
                    promptUI.UpdatePromptContent(targetItem, caps, upgradeMgr);
                }
                return false;
            }
        }

        private void PlaySparks()
        {
            if (sparksParticleSystem != null)
            {
                AlignSparksToTop();
                sparksParticleSystem.Play();
            }
        }

        private void PlayUpgradeAudio()
        {
            if (audioSource == null) EnsureComponents();

            if (upgradeSound != null)
            {
                audioSource.PlayOneShot(upgradeSound);
            }
            else
            {
                // Proceduralny soczysty brzdęk kowadła
                if (proceduralAnvilClip == null)
                {
                    proceduralAnvilClip = CreateProceduralAnvilClip();
                }
                if (proceduralAnvilClip != null && audioSource != null)
                {
                    audioSource.PlayOneShot(proceduralAnvilClip, 0.9f);
                }
            }
        }

        /// <summary>
        /// Generuje proceduralny metaliczny dźwięk uderzenia w kowadło (wysokie harmoniczne z szybkim tłumieniem).
        /// </summary>
        private AudioClip CreateProceduralAnvilClip()
        {
            int sampleRate = 44100;
            float duration = 0.45f;
            int samples = (int)(sampleRate * duration);
            float[] data = new float[samples];

            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-12f * t); // Szybkie wytłumienie uderzenia

                // Składowe harmoniczne metalu (dzwonienie kowadła)
                float f1 = Mathf.Sin(2f * Mathf.PI * 1280f * t);
                float f2 = Mathf.Sin(2f * Mathf.PI * 2560f * t) * 0.6f;
                float f3 = Mathf.Sin(2f * Mathf.PI * 3840f * t) * 0.35f;
                float f4 = Mathf.Sin(2f * Mathf.PI * 5120f * t) * 0.15f;

                // Szum uderzenia młota w metal (krótki atak na początku)
                float clickNoise = (t < 0.02f) ? (Random.value * 2f - 1f) * Mathf.Exp(-150f * t) : 0f;

                data[i] = (f1 + f2 + f3 + f4 + clickNoise) * envelope * 0.45f;
            }

            AudioClip clip = AudioClip.Create("ProceduralAnvilHit", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
