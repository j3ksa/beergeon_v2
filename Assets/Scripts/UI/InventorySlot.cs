using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public Image icon;
    public Image activeBorder;
    public TextMeshProUGUI countText;

    Item item;

    private void Awake()
    {
        EnsureCountText();
    }

    private void EnsureCountText()
    {
        if (countText == null)
        {
            countText = GetComponentInChildren<TextMeshProUGUI>(true);
        }

        if (countText == null)
        {
            GameObject textObj = new GameObject("CountText", typeof(RectTransform));
            textObj.transform.SetParent(transform, false);
            
            RectTransform rect = textObj.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.4f, 0f);
            rect.anchorMax = new Vector2(1f, 0.45f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = new Vector2(-4f, 4f);
            rect.sizeDelta = new Vector2(40f, 25f);

            countText = textObj.AddComponent<TextMeshProUGUI>();
            countText.fontSize = 20;
            countText.fontStyle = FontStyles.Bold;
            countText.alignment = TextAlignmentOptions.BottomRight;
            countText.color = Color.white;
            countText.raycastTarget = false;

            // Add outline for contrast
            Outline outline = textObj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
        }
    }

    public void AddItem(Item newItem, int count = 1)
    {
        if (newItem == null)
        {
            ClearSlot();
            return;
        }

        item = newItem;

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.preserveAspect = true; // Zachowuje naturalne proporcje grafiki przedmiotu bez rozciągania/rozjeżdżania
            
            // Dopasowanie rozmiaru z zachowaniem proporcji (preserveAspect = true)
            RectTransform iconRect = icon.rectTransform;
            if (iconRect != null)
            {
                iconRect.anchorMin = new Vector2(0.5f, 0.5f);
                iconRect.anchorMax = new Vector2(0.5f, 0.5f);
                iconRect.pivot = new Vector2(0.5f, 0.5f);
                iconRect.anchoredPosition = Vector2.zero;

                // Sprawdzamy czy dany sprite posiada duże puste marginesy (np. Jagerek, Ognista Woda itp.)
                float customScale = 1.0f;
                if (item != null)
                {
                    string nameLower = (item.itemName ?? "").ToLowerInvariant();
                    string assetNameLower = (item.name ?? "").ToLowerInvariant();

                    if (nameLower.Contains("jager") || assetNameLower.Contains("jager") || nameLower.Contains("przysp") || assetNameLower.Contains("speed"))
                    {
                        customScale = 2.2f; // Jagerek miał aż 61% pustej przestrzeni w pionie
                    }
                    else if (nameLower.Contains("ognist") || assetNameLower.Contains("ognist"))
                    {
                        customScale = 2.0f; // Ognista woda miała 63% pustego tła w pionie
                    }
                    else if (nameLower.Contains("kusza") || assetNameLower.Contains("crossbow"))
                    {
                        customScale = 1.4f;
                    }
                    else if (item.icon != null)
                    {
                        // Jeśli tekstura ma proporcje 1:2 (704x1511), a rysunek jest w środku
                        float aspect = (float)item.icon.rect.width / Mathf.Max(1f, item.icon.rect.height);
                        if (aspect < 0.55f)
                        {
                            customScale = 1.35f;
                        }
                    }
                }

                float baseSize = 90f * customScale;
                iconRect.sizeDelta = new Vector2(baseSize, baseSize);
            }

            icon.gameObject.SetActive(true);
            icon.enabled = true; // Just in case
        }
        else
        {
            Debug.LogError("InventorySlot: Icon Image is not assigned in the Inspector!", this);
        }

        EnsureCountText();
        if (countText != null)
        {
            if (count > 1)
            {
                countText.text = count.ToString();
                countText.gameObject.SetActive(true);
            }
            else
            {
                countText.text = "";
                countText.gameObject.SetActive(false);
            }
        }
    }

    public void ClearSlot()
    {
        item = null;

        if (icon != null)
        {
            icon.sprite = null;
            icon.gameObject.SetActive(false);
        }

        if (countText != null)
        {
            countText.text = "";
            countText.gameObject.SetActive(false);
        }
    }

    public void SetActive(bool isActive)
    {
        if (activeBorder != null)
        {
            activeBorder.gameObject.SetActive(isActive);
        }
    }
}

