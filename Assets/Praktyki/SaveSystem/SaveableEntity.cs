using System;
using UnityEngine;

namespace SaveSystem
{
    [DisallowMultipleComponent]
    public class SaveableEntity : MonoBehaviour
    {
        [SerializeField] private string saveID = "";

        public string SaveID => saveID;

        private void Awake()
        {
            // Zawsze nadpisujemy saveID na wypadek gdyby zduplikowane prefaby miały to samo zahardkodowane ID
            saveID = $"{gameObject.name}_{Mathf.RoundToInt(transform.position.x * 10f)}_{Mathf.RoundToInt(transform.position.y * 10f)}_{Mathf.RoundToInt(transform.position.z * 10f)}";
        }

        private void OnValidate()
        {
            if (string.IsNullOrEmpty(saveID))
            {
                GenerateID();
            }
        }

        [ContextMenu("Generuj Nowy Save ID")]
        public void GenerateID()
        {
            saveID = Guid.NewGuid().ToString();
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
        }
    }
}
