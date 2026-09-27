using UnityEngine;

namespace SaveSystem
{
    [RequireComponent(typeof(SaveableEntity))]
    public class EnemySaveable : MonoBehaviour
    {
        [SerializeField] private string enemyType = "DefaultEnemy";
        [SerializeField] private int maxHealth = 100;
        private int currentHealth = 100;
        private bool isDead = false;

        private SaveableEntity saveableEntity;

        private void Awake()
        {
            saveableEntity = GetComponent<SaveableEntity>();
            currentHealth = maxHealth;
        }

        public string SaveID => saveableEntity != null ? saveableEntity.SaveID : "";

        public EnemySaveData SaveState()
        {
            EnemySaveData data = new EnemySaveData();
            data.saveID = SaveID;
            data.enemyName = enemyType;
            data.position = new Vector3Serializable(transform.position);
            data.rotation = new Vector3Serializable(transform.eulerAngles);
            
            EnemyHealth healthComponent = GetComponent<EnemyHealth>();
            data.currentHealth = healthComponent != null ? healthComponent.CurrentHealth : 100;
            
            data.isDead = healthComponent != null && healthComponent.CurrentHealth <= 0 || !gameObject.activeSelf;

            return data;
        }

        public void LoadState(EnemySaveData data)
        {
            if (data == null) return;

            isDead = data.isDead;
            
            EnemyHealth healthComponent = GetComponent<EnemyHealth>();

            if (isDead)
            {
                if (healthComponent != null) healthComponent.SetHealthDirectly(0);
                gameObject.SetActive(false);
            }
            else
            {
                gameObject.SetActive(true);
                
                if (healthComponent != null) healthComponent.SetHealthDirectly(data.currentHealth);

                // Wyłącz komponenty fizyki/ruchu na ułamek sekundy przed teleportacją
                UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
                CharacterController cc = GetComponent<CharacterController>();
                
                if (agent != null)
                {
                    agent.Warp(data.position.ToVector3());
                    transform.eulerAngles = data.rotation.ToVector3();
                }
                else
                {
                    if (cc != null) cc.enabled = false;
                    transform.position = data.position.ToVector3();
                    transform.eulerAngles = data.rotation.ToVector3();
                    if (cc != null) cc.enabled = true;
                }
            }
        }

        public void SetHealth(int health)
        {
            currentHealth = Mathf.Clamp(health, 0, maxHealth);
            if (currentHealth <= 0)
            {
                Die();
            }
        }

        public void Die()
        {
            isDead = true;
            gameObject.SetActive(false);
        }
    }
}
