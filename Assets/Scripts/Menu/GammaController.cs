using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GammaController : MonoBehaviour
{
    public static GammaController Instance { get; private set; }

    [Header("Configuration")]
    [Tooltip("The canvas sort order. Must be very high to be on top of everything.")]
    public int sortOrder = 32767; 
    
    [Header("References")]
    public Volume globalVolume;

    // Internal
    private GameObject overlayCanvasObj;
    private Image gammaImage;
    private Image contrastImage;
    private Canvas canvas;
    
    // State
    private float currentGamma = 0f;
    private float currentContrast = 0f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        CreateOverlay();
    }
    
    private void CreateOverlay()
    {
        // 1. Create Canvas GameObject (if not exists)
        if (overlayCanvasObj != null) Destroy(overlayCanvasObj);
        
        overlayCanvasObj = new GameObject("GlobalGammaOverlay");
        overlayCanvasObj.transform.SetParent(this.transform);
        
        // 2. Add Canvas
        canvas = overlayCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortOrder;
        
        // 3. Scale
        CanvasScaler scaler = overlayCanvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);

        // 4. Contrast Image (Bottom Layer of overlay)
        contrastImage = CreateImage("ContrastImage", overlayCanvasObj.transform);
        
        // 5. Gamma Image (Top Layer of overlay)
        gammaImage = CreateImage("GammaImage", overlayCanvasObj.transform);
    }
    
    private Image CreateImage(string name, Transform parent)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Image img = go.AddComponent<Image>();
        img.color = Color.clear;
        img.raycastTarget = false;
        
        // Fill screen
        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        
        return img;
    }

    public void SetGamma(float val)
    {
        currentGamma = val;
        
        // 1. Gameplay Gamma (Post-Process)
        if (globalVolume == null) globalVolume = FindFirstObjectByType<Volume>();
        
        if (globalVolume != null && globalVolume.profile != null)
        {
             LiftGammaGain lgg;
             if (!globalVolume.profile.TryGet(out lgg))
             {
                 lgg = globalVolume.profile.Add<LiftGammaGain>(true);
             }
             
             if (lgg != null)
             {
                 lgg.gamma.overrideState = true;
                 lgg.gamma.value = new Vector4(1f, 1f, 1f, val);
             }
        }
        else
        {
            // Debug.LogWarning("[GammaController] Could not find Global Volume!");
        }

        UpdateGammaOverlay();
    }

    public void SetContrast(float val)
    {
        currentContrast = val;

        // 1. Gameplay Contrast
        if (globalVolume == null) globalVolume = FindFirstObjectByType<Volume>();
        
        if (globalVolume != null && globalVolume.profile != null)
        {
             ColorAdjustments ca;
             if (!globalVolume.profile.TryGet(out ca))
             {
                 ca = globalVolume.profile.Add<ColorAdjustments>(true);
             }
             
             if (ca != null)
             {
                ca.contrast.overrideState = true;
                ca.contrast.value = val;
             }
        }
        
        UpdateContrastOverlay();
    }
    
    private void UpdateGammaOverlay()
    {
        if (gammaImage == null) return; // Should try to recover but for now simple
        
        Color targetColor = Color.clear; 
        float alpha = 0f;

        // Darken
        if (currentGamma < 0)
        {
            targetColor = Color.black;
            alpha = Mathf.Abs(currentGamma); 
        }
        // Lighten
        else if (currentGamma > 0)
        {
            targetColor = Color.white;
            alpha = currentGamma * 0.5f; 
        }
        
        targetColor.a = Mathf.Clamp01(alpha);
        gammaImage.color = targetColor;
    }

    private void UpdateContrastOverlay()
    {
        if (contrastImage == null) return;
        
        // DISABLE UI CONTRAST SIMULATION
        // User reported it looks like inverted Gamma/Brightness.
        // It's better to have no effect on UI than a confusing one.
        // Contrast will only affect the 3D World (Post-Processing).
        
        contrastImage.color = Color.clear;
    }
}
