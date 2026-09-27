using UnityEngine;

namespace Praktyki.Kusza
{
    public class ArrowProjectile : MonoBehaviour
    {
        [Header("Konfiguracja Lotu")]
        [SerializeField] private float speed = 40f;
        [SerializeField] private float gravityMultiplier = 0.5f;
        [SerializeField] private float lifetime = 15f;

        [Header("Obrażenia i Kolizje")]
        [SerializeField] private int damage = 20;
        [SerializeField] private LayerMask hitLayers = ~0;

        [Header("Wizualia i Model")]
        [Tooltip("Główny transform modelu 3D (dziecko).")]
        [SerializeField] private Transform visualRoot;
        [Tooltip("Prefab modelu 3D strzały (np. strzala.prefab).")]
        [SerializeField] private GameObject arrowModelPrefab;
        [SerializeField] private GameObject impactEffectPrefab;
        [SerializeField] private AudioClip impactSound;
        [SerializeField] private AudioClip fleshHitSound;

        [Header("Opcje po trafieniu w ścianę")]
        [SerializeField] private bool stickInSurface = true;
        [SerializeField] private float stickLifetime = 20f;
        [SerializeField] private Item arrowItemPickup;

        private Vector3 velocity;
        private bool hasHit = false;
        private GameObject shooter;
        private float spawnTime;

        private void Awake()
        {
            SetupVisualModel();
        }

        private void SetupVisualModel()
        {
            if (visualRoot == null)
            {
                Transform found = transform.Find("Visual_ArrowModel");
                if (found != null) visualRoot = found;
            }

            if (visualRoot != null)
            {
                // Usuń ewentualny stary placeholderowy cylinder (MeshFilter / MeshRenderer)
                MeshRenderer mr = visualRoot.GetComponent<MeshRenderer>();
                if (mr != null) Destroy(mr);
                MeshFilter mf = visualRoot.GetComponent<MeshFilter>();
                if (mf != null) Destroy(mf);

                // Jeśli visualRoot nie ma jeszcze modelu dziecka, a podano prefab, zinstancjonuj go
                if (visualRoot.childCount == 0 && arrowModelPrefab != null)
                {
                    GameObject model = Instantiate(arrowModelPrefab, visualRoot);
                    model.name = "strzala";
                    model.transform.localPosition = Vector3.zero;
                    AlignArrowTipForward(model.transform);
                }
                else if (visualRoot.childCount > 0)
                {
                    Transform child = visualRoot.GetChild(0);
                    child.localPosition = Vector3.zero;
                    AlignArrowTipForward(child);
                }
            }
        }

        private void AlignArrowTipForward(Transform modelTransform)
        {
            if (modelTransform == null) return;

            // Sprawdź czy siatka pozwala na odczyt wierzchołków
            MeshFilter mf = modelTransform.GetComponentInChildren<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && mf.sharedMesh.isReadable)
            {
                Mesh mesh = mf.sharedMesh;
                Vector3[] verts = mesh.vertices;
                if (verts != null && verts.Length > 0)
                {
                    // Wykryj główną oś podłużną strzały (najdłuższy wymiar)
                    Vector3 size = mesh.bounds.size;
                    int axis = 2; // Z
                    if (size.x > size.y && size.x > size.z) axis = 0;
                    else if (size.y > size.x && size.y > size.z) axis = 1;
                    else axis = 2;

                    // Znajdź stronę z grotem (grot ma 1 zbiegający się czubek, lotki mają ich dużo)
                    float minVal = float.MaxValue, maxVal = float.MinValue;
                    for (int i = 0; i < verts.Length; i++)
                    {
                        float v = axis == 0 ? verts[i].x : (axis == 1 ? verts[i].y : verts[i].z);
                        if (v < minVal) minVal = v;
                        if (v > maxVal) maxVal = v;
                    }

                    float range = maxVal - minVal;
                    int countMin = 0, countMax = 0;
                    for (int i = 0; i < verts.Length; i++)
                    {
                        float v = axis == 0 ? verts[i].x : (axis == 1 ? verts[i].y : verts[i].z);
                        if (v < minVal + range * 0.25f) countMin++;
                        if (v > maxVal - range * 0.25f) countMax++;
                    }

                    bool tipIsPositive = countMax <= countMin;
                    Vector3 meshTipDir = Vector3.forward;
                    if (axis == 0) meshTipDir = tipIsPositive ? Vector3.right : Vector3.left;
                    else if (axis == 1) meshTipDir = tipIsPositive ? Vector3.up : Vector3.down;
                    else if (axis == 2) meshTipDir = tipIsPositive ? Vector3.forward : Vector3.back;

                    // Dopasuj obrót tak, aby grot był skierowany wzdłuż toru lotu pocisku (+Z)
                    Vector3 currentTipWorld = modelTransform.TransformDirection(meshTipDir);
                    Quaternion correction = Quaternion.FromToRotation(currentTipWorld, transform.forward);
                    modelTransform.rotation = correction * modelTransform.rotation;
                    return;
                }
            }

            // Bezpieczny domyślny obrót dla importu strzały z Blendera (oś Z modelu skierowana w przód)
            modelTransform.localRotation = Quaternion.Euler(0f, -90f, 0f);
        }

        public void Initialize(Vector3 direction, float initialSpeed, int calculatedDamage, GameObject shooterSource)
        {
            speed = initialSpeed > 0 ? initialSpeed : speed;
            velocity = direction.normalized * speed;
            damage = calculatedDamage;
            shooter = shooterSource;
            spawnTime = Time.time;
            transform.forward = direction.normalized;
        }

        private void Start()
        {
            if (spawnTime == 0) spawnTime = Time.time;
            if (velocity == Vector3.zero) velocity = transform.forward * speed;
        }

        private void Update()
        {
            if (hasHit) return;

            // Sprawdź czas życia pocisku w locie
            if (Time.time - spawnTime > lifetime)
            {
                Destroy(gameObject);
                return;
            }

            // Grawitacja
            velocity += Physics.gravity * gravityMultiplier * Time.deltaTime;

            // Krok ruchu
            Vector3 step = velocity * Time.deltaTime;
            float stepDistance = step.magnitude;
            Ray ray = new Ray(transform.position, velocity.normalized);

            // Wykrywanie kolizji za pomocą RaycastSweep (zapobiega przenikaniu przez ściany/wrogów)
            if (Physics.Raycast(ray, out RaycastHit hit, stepDistance, hitLayers, QueryTriggerInteraction.Ignore))
            {
                // Ignoruj strzelca
                if (shooter != null && (hit.collider.gameObject == shooter || hit.collider.transform.IsChildOf(shooter.transform)))
                {
                    transform.position += step;
                    return;
                }

                OnHit(hit);
            }
            else
            {
                transform.position += step;
                if (velocity.sqrMagnitude > 0.01f)
                {
                    transform.forward = velocity.normalized;
                }
            }
        }

        private void OnHit(RaycastHit hit)
        {
            hasHit = true;
            transform.position = hit.point;

            // Sprawdź czy cel przyjmuje obrażenia (IDamageable)
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable == null) damageable = hit.collider.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(damage);
                Debug.Log($"<color=#FFAA00>[Kusza] Strzała trafiła {hit.collider.name} zadając {damage} obrażeń!</color>");

                if (fleshHitSound != null)
                {
                    AudioSource.PlayClipAtPoint(fleshHitSound, hit.point);
                }

                // Wbij w cel lub zniszcz
                transform.SetParent(hit.collider.transform);
                Destroy(gameObject, 3f);
            }
            else
            {
                // Trafienie w przeszkodę terenową / ścianę
                if (impactSound != null)
                {
                    AudioSource.PlayClipAtPoint(impactSound, hit.point);
                }

                // Fizyczne popchnięcie obiektu (jeśli posiada Rigidbody)
                Rigidbody rb = hit.collider.attachedRigidbody;
                if (rb != null && !rb.isKinematic)
                {
                    rb.AddForceAtPosition(velocity.normalized * 5f, hit.point, ForceMode.Impulse);
                }

                // Obsługa ICollidable
                ICollidable collidable = hit.collider.GetComponent<ICollidable>();
                if (collidable != null)
                {
                    collidable.OnCollide(gameObject);
                }

                if (stickInSurface)
                {
                    // Wbicie strzały w ścianę
                    transform.position = hit.point - transform.forward * 0.15f;
                    transform.SetParent(hit.collider.transform);
                    Destroy(gameObject, stickLifetime);
                }
                else
                {
                    Destroy(gameObject);
                }
            }

            // Efekt cząsteczkowy uderzenia
            if (impactEffectPrefab != null)
            {
                Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            }
        }
    }
}
