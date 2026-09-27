using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SaveSystem
{
    public class SaveMenuUI : MonoBehaviour
    {
        [Header("Mode (Check 'Is Save Mode' for Pause Menu in Game)")]
        [SerializeField] private bool isSaveMode = false;

        [Header("Slot UI Elements (Slot 1, 2, 3)")]
        [SerializeField] private Button[] saveButtons;
        [SerializeField] private Button[] loadButtons;
        [SerializeField] private Button[] deleteButtons;
        [SerializeField] private TextMeshProUGUI[] slotInfoTexts;

        private void OnEnable()
        {
            RefreshUI();
        }

        public void RefreshUI()
        {
            if (slotInfoTexts == null || slotInfoTexts.Length == 0) return;

            int slotCount = slotInfoTexts.Length;

            for (int i = 0; i < slotCount; i++)
            {
                int slotId = i + 1;
                bool exists = SaveSystem.SaveExists(slotId);

                if (exists)
                {
                    SaveData data = SaveSystem.Load(slotId);
                    if (data != null)
                    {
                        // Format jednoliniowy: Lvl X | XP / ReqXP | Data
                        int reqXp = data.playerData.requiredExperience > 0 ? data.playerData.requiredExperience : 100;
                        slotInfoTexts[i].text = $"Lvl {data.playerData.level}  |  {data.playerData.experience}/{reqXp} XP  |  {data.saveDateTime}";
                    }
                    else
                    {
                        slotInfoTexts[i].text = "<color=#FF5555>Uszkodzony</color>";
                    }

                    if (saveButtons != null && i < saveButtons.Length && saveButtons[i] != null) saveButtons[i].interactable = true;
                    if (loadButtons != null && i < loadButtons.Length && loadButtons[i] != null) loadButtons[i].interactable = true;
                    if (deleteButtons != null && i < deleteButtons.Length && deleteButtons[i] != null) deleteButtons[i].interactable = true;
                }
                else
                {
                    // Wygląd pustego slotu
                    slotInfoTexts[i].text = "<color=#777777>EMPTY</color>";
                    
                    if (saveButtons != null && i < saveButtons.Length && saveButtons[i] != null) saveButtons[i].interactable = true;
                    
                    // W trybie zapisu (w grze) pusty slot musi być klikalny!
                    bool canClick = isSaveMode;
                    if (loadButtons != null && i < loadButtons.Length && loadButtons[i] != null) loadButtons[i].interactable = canClick;
                    if (deleteButtons != null && i < deleteButtons.Length && deleteButtons[i] != null) deleteButtons[i].interactable = false;
                }
            }
        }

        public void OnClickSave(int slotId)
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame(slotId);
                RefreshUI();
            }
        }

        public void OnClickLoad(int slotId)
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.LoadGame(slotId);
            }
        }

        public void OnClickDelete(int slotId)
        {
            SaveSystem.DeleteSave(slotId);
            RefreshUI();
        }
    }
}
