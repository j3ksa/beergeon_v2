using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Praktyki.Zbroja;

namespace Praktyki.EQ
{
    public static class MinecraftUIBuilder
    {
        public static TMP_FontAsset GetGameFont()
        {
            TMP_FontAsset font = Resources.Load<TMP_FontAsset>("IbarraRealNova-VariableFont_wght SDF");
#if UNITY_EDITOR
            if (font == null) font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Praktyki/IbarraRealNova-VariableFont_wght SDF.asset");
            if (font == null) font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Praktyki/SaveSystem/Graphics/IbarraRealNova-VariableFont_wght SDF.asset");
            if (font == null) font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/Resources/IbarraRealNova-VariableFont_wght SDF.asset");
            if (font == null)
            {
                string[] guids = UnityEditor.AssetDatabase.FindAssets("IbarraRealNova t:TMP_FontAsset");
                if (guids.Length > 0)
                {
                    font = UnityEditor.AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]));
                }
            }
#endif
            return font;
        }

        public static GameObject BuildMinecraftUI(Transform parent = null)
        {
            // Usuń ewentualne stare/zdublowane instancje (nawet nieaktywne), by nie nakładać okien
            var oldInstances = Object.FindObjectsByType<MinecraftInventoryUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (var old in oldInstances)
            {
                if (old != null) Object.DestroyImmediate(old.gameObject);
            }
            GameObject existing1 = GameObject.Find("[MinecraftEQ_System]");
            if (existing1 != null) Object.DestroyImmediate(existing1);
            GameObject existing2 = GameObject.Find("MinecraftInventory_UI");
            if (existing2 != null) Object.DestroyImmediate(existing2);

            TMP_FontAsset gameFont = GetGameFont();

            // Próba załadowania sprite'ów z Resources lub Assets
            Sprite panelSpr = Resources.Load<Sprite>("MinecraftEQ/minecraft_panel");
            Sprite slotSpr = Resources.Load<Sprite>("MinecraftEQ/minecraft_slot");
            Sprite activeSpr = Resources.Load<Sprite>("MinecraftEQ/minecraft_slot_active");
            Sprite helmetSil = Resources.Load<Sprite>("MinecraftEQ/minecraft_armor_helmet");
            Sprite chestSil = Resources.Load<Sprite>("MinecraftEQ/minecraft_armor_chestplate");
            Sprite legsSil = Resources.Load<Sprite>("MinecraftEQ/minecraft_armor_leggings");
            Sprite bootsSil = Resources.Load<Sprite>("MinecraftEQ/minecraft_armor_boots");
            Sprite playerSil = Resources.Load<Sprite>("MinecraftEQ/minecraft_player_silhouette");
            Sprite trashSpr = Resources.Load<Sprite>("MinecraftEQ/minecraft_trash");

#if UNITY_EDITOR
            if (panelSpr == null) panelSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Praktyki/EQ/Textures/minecraft_panel.png");
            if (slotSpr == null) slotSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Praktyki/EQ/Textures/minecraft_slot.png");
            if (activeSpr == null) activeSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Praktyki/EQ/Textures/minecraft_slot_active.png");
            if (helmetSil == null) helmetSil = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Praktyki/EQ/Textures/minecraft_armor_helmet.png");
            if (chestSil == null) chestSil = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Praktyki/EQ/Textures/minecraft_armor_chestplate.png");
            if (legsSil == null) legsSil = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Praktyki/EQ/Textures/minecraft_armor_leggings.png");
            if (bootsSil == null) bootsSil = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Praktyki/EQ/Textures/minecraft_armor_boots.png");
            if (playerSil == null) playerSil = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Praktyki/EQ/Textures/minecraft_player_silhouette.png");
            if (trashSpr == null) trashSpr = UnityEditor.AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Praktyki/EQ/Textures/minecraft_trash.png");
#endif

            // 1. Dedykowany Główny obiekt Root z własnym Canvasem ScreenSpaceOverlay
            // ZAWSZE stawiamy na ROOT poziomu sceny (parent = null), aby żaden wyłączony obiekt (np. PauseMenu) nie deaktywował UI!
            GameObject eqRoot = new GameObject("[MinecraftEQ_System]", 
                typeof(RectTransform), 
                typeof(Canvas), 
                typeof(CanvasScaler), 
                typeof(GraphicRaycaster), 
                typeof(CanvasGroup), 
                typeof(MinecraftInventoryUI), 
                typeof(MinecraftEQAudio), 
                typeof(AudioSource));

            eqRoot.transform.SetParent(null);
            if (Application.isPlaying)
            {
                Object.DontDestroyOnLoad(eqRoot);
            }

            Canvas eqCanvas = eqRoot.GetComponent<Canvas>();
            eqCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            eqCanvas.overrideSorting = true;
            eqCanvas.sortingOrder = 950;

            CanvasScaler scaler = eqRoot.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            RectTransform rootRect = eqRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.sizeDelta = Vector2.zero;
            rootRect.anchoredPosition = Vector2.zero;

            MinecraftInventoryUI mainUI = eqRoot.GetComponent<MinecraftInventoryUI>();
            CanvasGroup rootCg = eqRoot.GetComponent<CanvasGroup>();
            mainUI.canvasGroup = rootCg;

            // Ciemne tło przyciemniające grę (Dimmer)
            GameObject dimmer = new GameObject("BackgroundDimmer", typeof(RectTransform), typeof(Image), typeof(Button));
            dimmer.transform.SetParent(eqRoot.transform, false);
            RectTransform dimRect = dimmer.GetComponent<RectTransform>();
            dimRect.anchorMin = Vector2.zero;
            dimRect.anchorMax = Vector2.one;
            dimRect.sizeDelta = Vector2.zero;
            Image dimImg = dimmer.GetComponent<Image>();
            dimImg.color = new Color(0f, 0f, 0f, 0.6f);
            dimImg.raycastTarget = true;
            Button dimBtn = dimmer.GetComponent<Button>();
            dimBtn.transition = Selectable.Transition.None;
            dimBtn.onClick.AddListener(() => mainUI.CloseInventory());

            // 2. Główne Okno Ekwipunku (760 x 660 px) - Ciemny dąb i antyczny mosiądz (Styl Kowadła / BlackJacka)
            GameObject window = new GameObject("MinecraftWindow", typeof(RectTransform), typeof(Image), typeof(Outline));
            window.transform.SetParent(eqRoot.transform, false);
            mainUI.windowPanel = window;

            RectTransform winRect = window.GetComponent<RectTransform>();
            winRect.anchorMin = new Vector2(0.5f, 0.5f);
            winRect.anchorMax = new Vector2(0.5f, 0.5f);
            winRect.pivot = new Vector2(0.5f, 0.5f);
            winRect.sizeDelta = new Vector2(760f, 660f);
            winRect.anchoredPosition = Vector2.zero;

            Image winImg = window.GetComponent<Image>();
            winImg.sprite = null;
            winImg.color = new Color(0.06f, 0.05f, 0.06f, 0.97f); // Głęboki, klimatyczny podkład tawerny

            Outline winOutline = window.GetComponent<Outline>();
            winOutline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.9f); // Złotawo-mosiężna obwódka jak w kowadle/blackjacku
            winOutline.effectDistance = new Vector2(2.5f, -2.5f);

            // Wewnętrzna subtelna ozdobna ramka
            GameObject innerTrim = new GameObject("InnerTrim", typeof(RectTransform), typeof(Image), typeof(Outline));
            innerTrim.transform.SetParent(window.transform, false);
            RectTransform itRect = innerTrim.GetComponent<RectTransform>();
            itRect.anchorMin = Vector2.zero;
            itRect.anchorMax = Vector2.one;
            itRect.sizeDelta = new Vector2(-10f, -10f);
            Image itImg = innerTrim.GetComponent<Image>();
            itImg.color = new Color(0f, 0f, 0f, 0.15f);
            Outline itOut = innerTrim.GetComponent<Outline>();
            itOut.effectColor = new Color(0.40f, 0.32f, 0.18f, 0.5f);
            itOut.effectDistance = new Vector2(1f, -1f);

            // 3. Nagłówek okna (Tytuł + Przycisk Zamknij [X])
            GameObject header = new GameObject("Header", typeof(RectTransform));
            header.transform.SetParent(window.transform, false);
            RectTransform headerRect = header.GetComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0f, 1f);
            headerRect.anchorMax = new Vector2(1f, 1f);
            headerRect.pivot = new Vector2(0.5f, 1f);
            headerRect.sizeDelta = new Vector2(0f, 44f);
            headerRect.anchoredPosition = new Vector2(0f, -6f);

            // Tytuł
            GameObject titleObj = new GameObject("TitleText", typeof(RectTransform), typeof(TextMeshProUGUI));
            titleObj.transform.SetParent(header.transform, false);
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 0f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0f, 0.5f);
            titleRect.offsetMin = new Vector2(24f, 0f);
            titleRect.offsetMax = new Vector2(-60f, 0f);
            TextMeshProUGUI titleText = titleObj.GetComponent<TextMeshProUGUI>();
            titleText.text = "<b>EKWIPUNEK I WYPOSAŻENIE</b>";
            titleText.fontSize = 22;
            titleText.color = new Color(1f, 0.84f, 0.25f, 1f); // Szlachetne złoto IbarraRealNova
            titleText.alignment = TextAlignmentOptions.MidlineLeft;

            // Przycisk [ X ]
            GameObject closeBtnObj = new GameObject("CloseButton", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            closeBtnObj.transform.SetParent(header.transform, false);
            RectTransform closeRect = closeBtnObj.GetComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(1f, 0.5f);
            closeRect.anchorMax = new Vector2(1f, 0.5f);
            closeRect.pivot = new Vector2(1f, 0.5f);
            closeRect.sizeDelta = new Vector2(30f, 30f);
            closeRect.anchoredPosition = new Vector2(-20f, 0f);
            Image closeImg = closeBtnObj.GetComponent<Image>();
            closeImg.color = new Color(0.48f, 0.12f, 0.12f, 0.95f);
            Outline closeOut = closeBtnObj.GetComponent<Outline>();
            closeOut.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.85f);
            closeOut.effectDistance = new Vector2(1.5f, -1.5f);
            Button closeBtn = closeBtnObj.GetComponent<Button>();
            closeBtn.onClick.AddListener(mainUI.ToggleInventory);

            GameObject closeTxtObj = new GameObject("CloseX", typeof(RectTransform), typeof(TextMeshProUGUI));
            closeTxtObj.transform.SetParent(closeBtnObj.transform, false);
            RectTransform ctxRect = closeTxtObj.GetComponent<RectTransform>();
            ctxRect.anchorMin = Vector2.zero;
            ctxRect.anchorMax = Vector2.one;
            ctxRect.offsetMin = Vector2.zero;
            ctxRect.offsetMax = Vector2.zero;
            TextMeshProUGUI closeTxt = closeTxtObj.GetComponent<TextMeshProUGUI>();
            closeTxt.text = "<b>X</b>";
            closeTxt.fontSize = 18;
            closeTxt.color = new Color(1f, 0.9f, 0.9f, 1f);
            closeTxt.alignment = TextAlignmentOptions.Center;

            // Złota ozdobna linia oddzielająca nagłówek
            GameObject headDiv = new GameObject("HeaderDivider", typeof(RectTransform), typeof(Image));
            headDiv.transform.SetParent(window.transform, false);
            RectTransform hdRect = headDiv.GetComponent<RectTransform>();
            hdRect.anchorMin = new Vector2(0.5f, 1f);
            hdRect.anchorMax = new Vector2(0.5f, 1f);
            hdRect.pivot = new Vector2(0.5f, 1f);
            hdRect.anchoredPosition = new Vector2(0f, -48f);
            hdRect.sizeDelta = new Vector2(720f, 2f);
            Image hdImg = headDiv.GetComponent<Image>();
            hdImg.color = new Color(0.85f, 0.70f, 0.30f, 0.55f);

            // 4. Górna Sekcja: Zbroja (4 sloty) + Panel Statystyk Gracza (Szeroki) + Kosz (BEZ SYLWETKI)
            GameObject topSection = new GameObject("TopSection", typeof(RectTransform));
            topSection.transform.SetParent(window.transform, false);
            RectTransform topRect = topSection.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0.5f, 1f);
            topRect.anchorMax = new Vector2(0.5f, 1f);
            topRect.pivot = new Vector2(0.5f, 1f);
            topRect.sizeDelta = new Vector2(720f, 215f);
            topRect.anchoredPosition = new Vector2(0f, -54f);

            // A) Sloty Zbroi (Kolumna lewa: Hełm, Klata, Spodnie, Buty)
            GameObject armorContainer = new GameObject("ArmorContainer", typeof(RectTransform));
            armorContainer.transform.SetParent(topSection.transform, false);
            RectTransform acRect = armorContainer.GetComponent<RectTransform>();
            acRect.anchorMin = new Vector2(0f, 0.5f);
            acRect.anchorMax = new Vector2(0f, 0.5f);
            acRect.pivot = new Vector2(0f, 0.5f);
            acRect.anchoredPosition = new Vector2(16f, 0f);
            acRect.sizeDelta = new Vector2(50f, 204f);

            GameObject armorLblObj = new GameObject("ArmorLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            armorLblObj.transform.SetParent(armorContainer.transform, false);
            RectTransform alRect = armorLblObj.GetComponent<RectTransform>();
            alRect.anchorMin = new Vector2(0.5f, 1f);
            alRect.anchorMax = new Vector2(0.5f, 1f);
            alRect.pivot = new Vector2(0.5f, 1f);
            alRect.anchoredPosition = new Vector2(0f, 0f);
            alRect.sizeDelta = new Vector2(50f, 16f);
            TextMeshProUGUI alTxt = armorLblObj.GetComponent<TextMeshProUGUI>();
            alTxt.text = "<color=#D4AF37><b>ZBROJA</b></color>";
            alTxt.fontSize = 11;
            alTxt.alignment = TextAlignmentOptions.Center;

            GameObject armorCol = new GameObject("ArmorColumn", typeof(RectTransform), typeof(VerticalLayoutGroup));
            armorCol.transform.SetParent(armorContainer.transform, false);
            RectTransform aColRect = armorCol.GetComponent<RectTransform>();
            aColRect.anchorMin = new Vector2(0.5f, 0.5f);
            aColRect.anchorMax = new Vector2(0.5f, 0.5f);
            aColRect.pivot = new Vector2(0.5f, 0.5f);
            aColRect.anchoredPosition = new Vector2(0f, -8f);
            aColRect.sizeDelta = new Vector2(46f, 184f);

            VerticalLayoutGroup aLayout = armorCol.GetComponent<VerticalLayoutGroup>();
            aLayout.spacing = 3f;
            aLayout.childControlWidth = false;
            aLayout.childControlHeight = false;
            aLayout.childForceExpandWidth = false;
            aLayout.childForceExpandHeight = false;
            aLayout.childAlignment = TextAnchor.MiddleCenter;

            Sprite[] silhouettes = new Sprite[] { helmetSil, chestSil, legsSil, bootsSil };
            string[] armorNames = new string[] { "Armor_Helmet", "Armor_Chestplate", "Armor_Leggings", "Armor_Boots" };

            for (int i = 0; i < 4; i++)
            {
                GameObject aSlot = CreateSlotObject(armorCol.transform, armorNames[i], 100 + i, false, 44f, null, activeSpr, silhouettes[i]);
                MinecraftSlotUI aSlotUI = aSlot.GetComponent<MinecraftSlotUI>();
                aSlotUI.isArmorSlot = true;
                aSlotUI.requiredArmorSlot = (ArmorSlotType)i;
                mainUI.armorSlots[i] = aSlotUI;
            }

            // B) Panel Szczegółowych Statystyk Gracza (Szeroki, centralny jak tablica kowadła)
            GameObject statsBoard = new GameObject("StatsBoard", typeof(RectTransform), typeof(Image), typeof(Outline));
            statsBoard.transform.SetParent(topSection.transform, false);
            RectTransform sbRect = statsBoard.GetComponent<RectTransform>();
            sbRect.anchorMin = new Vector2(0f, 0.5f);
            sbRect.anchorMax = new Vector2(0f, 0.5f);
            sbRect.pivot = new Vector2(0f, 0.5f);
            sbRect.sizeDelta = new Vector2(540f, 204f);
            sbRect.anchoredPosition = new Vector2(76f, 0f);

            Image sbImg = statsBoard.GetComponent<Image>();
            sbImg.color = new Color(0.04f, 0.04f, 0.05f, 0.95f); // Głęboki burgund/grafit kowadła

            Outline sbOut = statsBoard.GetComponent<Outline>();
            sbOut.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.7f); // Złota obwódka
            sbOut.effectDistance = new Vector2(2f, -2f);

            GameObject statsTextObj = new GameObject("StatsDetailText", typeof(RectTransform), typeof(TextMeshProUGUI));
            statsTextObj.transform.SetParent(statsBoard.transform, false);
            RectTransform stRect = statsTextObj.GetComponent<RectTransform>();
            stRect.anchorMin = Vector2.zero;
            stRect.anchorMax = Vector2.one;
            stRect.sizeDelta = new Vector2(-20f, -16f);
            TextMeshProUGUI statsText = statsTextObj.GetComponent<TextMeshProUGUI>();
            statsText.fontSize = 15;
            statsText.lineSpacing = 16f;
            statsText.color = Color.white;
            statsText.alignment = TextAlignmentOptions.TopLeft;
            mainUI.statsDetailText = statsText;

            // C) Kosz / Zrzut [Q] (Prawa strona)
            GameObject trashObj = CreateSlotObject(topSection.transform, "TrashSlot", -1, true, 56f, null, activeSpr, null);
            RectTransform trRect = trashObj.GetComponent<RectTransform>();
            trRect.anchorMin = new Vector2(1f, 0.5f);
            trRect.anchorMax = new Vector2(1f, 0.5f);
            trRect.pivot = new Vector2(1f, 0.5f);
            trRect.anchoredPosition = new Vector2(-15f, 0f);
            mainUI.trashDropSlot = trashObj.GetComponent<MinecraftSlotUI>();

            GameObject trashLbl = new GameObject("TrashLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            trashLbl.transform.SetParent(trashObj.transform, false);
            RectTransform tLblRect = trashLbl.GetComponent<RectTransform>();
            tLblRect.anchoredPosition = new Vector2(0f, 40f);
            tLblRect.sizeDelta = new Vector2(80f, 22f);
            TextMeshProUGUI tLblTxt = trashLbl.GetComponent<TextMeshProUGUI>();
            tLblTxt.text = "<color=#FFA726><b>ZRZUT [Q]</b></color>";
            tLblTxt.fontSize = 11;
            tLblTxt.alignment = TextAlignmentOptions.Center;

            // 5. Linia oddzielająca Górną Sekcję od Plecaka
            GameObject topDiv = new GameObject("TopDivider", typeof(RectTransform), typeof(Image));
            topDiv.transform.SetParent(window.transform, false);
            RectTransform tdRect = topDiv.GetComponent<RectTransform>();
            tdRect.anchorMin = new Vector2(0.5f, 1f);
            tdRect.anchorMax = new Vector2(0.5f, 1f);
            tdRect.pivot = new Vector2(0.5f, 1f);
            tdRect.sizeDelta = new Vector2(720f, 2f);
            tdRect.anchoredPosition = new Vector2(0f, -274f);
            Image tdImg = topDiv.GetComponent<Image>();
            tdImg.color = new Color(0.85f, 0.70f, 0.30f, 0.45f);

            // Etykieta Plecaka
            GameObject storageLbl = new GameObject("StorageLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            storageLbl.transform.SetParent(window.transform, false);
            RectTransform sLblRect = storageLbl.GetComponent<RectTransform>();
            sLblRect.anchorMin = new Vector2(0.5f, 1f);
            sLblRect.anchorMax = new Vector2(0.5f, 1f);
            sLblRect.pivot = new Vector2(0f, 1f);
            sLblRect.anchoredPosition = new Vector2(-319f, -282f);
            sLblRect.sizeDelta = new Vector2(250f, 18f);
            TextMeshProUGUI sLblTxt = storageLbl.GetComponent<TextMeshProUGUI>();
            sLblTxt.text = "<color=#D4AF37><b>PLECAK (27 MIEJSC)</b></color>";
            sLblTxt.fontSize = 12;
            sLblTxt.alignment = TextAlignmentOptions.MidlineLeft;

            // 6. Sekcja Głównego Plecaka (27 slotów: 9x3)
            GameObject storageSection = new GameObject("StorageSection", typeof(RectTransform));
            storageSection.transform.SetParent(window.transform, false);
            RectTransform storageRect = storageSection.GetComponent<RectTransform>();
            storageRect.anchorMin = new Vector2(0.5f, 1f);
            storageRect.anchorMax = new Vector2(0.5f, 1f);
            storageRect.pivot = new Vector2(0.5f, 1f);
            storageRect.sizeDelta = new Vector2(720f, 195f);
            storageRect.anchoredPosition = new Vector2(0f, -302f);

            GameObject storageGrid = new GameObject("StorageGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            storageGrid.transform.SetParent(storageSection.transform, false);
            RectTransform gridRect = storageGrid.GetComponent<RectTransform>();
            gridRect.anchorMin = Vector2.zero;
            gridRect.anchorMax = Vector2.one;
            gridRect.sizeDelta = Vector2.zero;

            GridLayoutGroup sGrid = storageGrid.GetComponent<GridLayoutGroup>();
            sGrid.cellSize = new Vector2(62f, 60f);
            sGrid.spacing = new Vector2(10f, 5f);
            sGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            sGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            sGrid.childAlignment = TextAnchor.MiddleCenter;
            sGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            sGrid.constraintCount = 9;

            for (int i = 0; i < 27; i++)
            {
                int slotIdx = 9 + i;
                GameObject sObj = CreateSlotObject(storageGrid.transform, $"Slot_{slotIdx}", slotIdx, false, 60f, null, activeSpr, null);
                mainUI.storageSlots[i] = sObj.GetComponent<MinecraftSlotUI>();
            }

            // 7. Linia oddzielająca Plecak od Hotbara
            GameObject divider = new GameObject("HotbarDivider", typeof(RectTransform), typeof(Image));
            divider.transform.SetParent(window.transform, false);
            RectTransform divRect = divider.GetComponent<RectTransform>();
            divRect.anchorMin = new Vector2(0.5f, 1f);
            divRect.anchorMax = new Vector2(0.5f, 1f);
            divRect.pivot = new Vector2(0.5f, 1f);
            divRect.sizeDelta = new Vector2(720f, 2f);
            divRect.anchoredPosition = new Vector2(0f, -508f);
            Image divImg = divider.GetComponent<Image>();
            divImg.color = new Color(0.85f, 0.70f, 0.30f, 0.55f);

            // Etykieta Hotbara
            GameObject hotbarLbl = new GameObject("HotbarLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
            hotbarLbl.transform.SetParent(window.transform, false);
            RectTransform hLblRect = hotbarLbl.GetComponent<RectTransform>();
            hLblRect.anchorMin = new Vector2(0.5f, 1f);
            hLblRect.anchorMax = new Vector2(0.5f, 1f);
            hLblRect.pivot = new Vector2(0f, 1f);
            hLblRect.anchoredPosition = new Vector2(-319f, -516f);
            hLblRect.sizeDelta = new Vector2(250f, 18f);
            TextMeshProUGUI hLblTxt = hotbarLbl.GetComponent<TextMeshProUGUI>();
            hLblTxt.text = "<color=#D4AF37><b>PASEK PODRĘCZNY [1 - 9]</b></color>";
            hLblTxt.fontSize = 12;
            hLblTxt.alignment = TextAlignmentOptions.MidlineLeft;

            // 8. Sekcja Paska Podręcznego (9 slotów)
            GameObject hotbarSection = new GameObject("HotbarSection", typeof(RectTransform), typeof(GridLayoutGroup));
            hotbarSection.transform.SetParent(window.transform, false);
            RectTransform hotbarRect = hotbarSection.GetComponent<RectTransform>();
            hotbarRect.anchorMin = new Vector2(0.5f, 1f);
            hotbarRect.anchorMax = new Vector2(0.5f, 1f);
            hotbarRect.pivot = new Vector2(0.5f, 1f);
            hotbarRect.sizeDelta = new Vector2(720f, 70f);
            hotbarRect.anchoredPosition = new Vector2(0f, -538f);

            GridLayoutGroup hGrid = hotbarSection.GetComponent<GridLayoutGroup>();
            hGrid.cellSize = new Vector2(62f, 60f);
            hGrid.spacing = new Vector2(10f, 0f);
            hGrid.startCorner = GridLayoutGroup.Corner.UpperLeft;
            hGrid.startAxis = GridLayoutGroup.Axis.Horizontal;
            hGrid.childAlignment = TextAnchor.MiddleCenter;
            hGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            hGrid.constraintCount = 9;

            for (int i = 0; i < 9; i++)
            {
                GameObject hObj = CreateSlotObject(hotbarSection.transform, $"HotbarSlot_{i}", i, false, 60f, null, activeSpr, null);
                mainUI.hotbarSlots[i] = hObj.GetComponent<MinecraftSlotUI>();
            }

            // 9. Kursor trzymanego przedmiotu
            GameObject heldCursorObj = new GameObject("HeldItemCursor", typeof(RectTransform), typeof(CanvasGroup));
            heldCursorObj.transform.SetParent(eqRoot.transform, false);
            RectTransform heldRect = heldCursorObj.GetComponent<RectTransform>();
            heldRect.sizeDelta = new Vector2(60f, 60f);
            heldRect.pivot = new Vector2(0.5f, 0.5f);
            CanvasGroup heldCg = heldCursorObj.GetComponent<CanvasGroup>();
            heldCg.blocksRaycasts = false;

            GameObject heldIconObj = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            heldIconObj.transform.SetParent(heldCursorObj.transform, false);
            RectTransform heldIconRect = heldIconObj.GetComponent<RectTransform>();
            heldIconRect.anchorMin = Vector2.zero;
            heldIconRect.anchorMax = Vector2.one;
            heldIconRect.sizeDelta = new Vector2(-6f, -6f);
            Image heldIcon = heldIconObj.GetComponent<Image>();
            heldIcon.raycastTarget = false;

            GameObject heldCountObj = new GameObject("CountText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(Outline));
            heldCountObj.transform.SetParent(heldCursorObj.transform, false);
            RectTransform heldCountRect = heldCountObj.GetComponent<RectTransform>();
            heldCountRect.anchorMin = new Vector2(0.4f, 0f);
            heldCountRect.anchorMax = new Vector2(1f, 0.45f);
            heldCountRect.pivot = new Vector2(1f, 0f);
            heldCountRect.anchoredPosition = new Vector2(-4f, 4f);
            TextMeshProUGUI heldCountTxt = heldCountObj.GetComponent<TextMeshProUGUI>();
            heldCountTxt.fontSize = 20;
            heldCountTxt.fontStyle = FontStyles.Bold;
            heldCountTxt.alignment = TextAlignmentOptions.BottomRight;
            heldCountTxt.color = Color.white;
            heldCountTxt.raycastTarget = false;

            Outline heldOutline = heldCountObj.GetComponent<Outline>();
            heldOutline.effectColor = Color.black;
            heldOutline.effectDistance = new Vector2(1.5f, -1.5f);

            mainUI.heldItemCursorRect = heldRect;
            mainUI.heldItemIcon = heldIcon;
            mainUI.heldItemCountText = heldCountTxt;
            heldCursorObj.SetActive(false);

            // 10. Tooltip Ekwipunku w stylu Medieval (Stała szerokość 310px, dopasowanie wysokości)
            GameObject tooltipObj = new GameObject("MinecraftTooltip", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(CanvasGroup), typeof(MinecraftTooltipUI), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
            tooltipObj.transform.SetParent(eqRoot.transform, false);
            RectTransform ttRect = tooltipObj.GetComponent<RectTransform>();
            ttRect.pivot = new Vector2(0f, 1f);
            ttRect.sizeDelta = new Vector2(310f, 100f);

            Image ttImg = tooltipObj.GetComponent<Image>();
            ttImg.color = new Color(0.05f, 0.05f, 0.07f, 0.98f);

            Outline ttOutline = tooltipObj.GetComponent<Outline>();
            ttOutline.effectColor = new Color(0.85f, 0.70f, 0.30f, 0.85f);
            ttOutline.effectDistance = new Vector2(2f, -2f);

            VerticalLayoutGroup vLayout = tooltipObj.GetComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(14, 14, 12, 12);
            vLayout.spacing = 5f;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = true;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            ContentSizeFitter fitter = tooltipObj.GetComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            MinecraftTooltipUI ttUI = tooltipObj.GetComponent<MinecraftTooltipUI>();

            GameObject nameObj = CreateTooltipText(tooltipObj.transform, "ItemName", 20, FontStyles.Bold, Color.white);
            GameObject typeObj = CreateTooltipText(tooltipObj.transform, "ItemType", 14, FontStyles.Italic, new Color(0.7f, 0.7f, 0.7f));
            GameObject statsObj = CreateTooltipText(tooltipObj.transform, "ItemStats", 16, FontStyles.Normal, Color.white);
            GameObject descObj = CreateTooltipText(tooltipObj.transform, "ItemDesc", 14, FontStyles.Normal, new Color(0.85f, 0.85f, 0.85f));

            ttUI.InitializeReferences(
                ttRect,
                tooltipObj.GetComponent<CanvasGroup>(),
                nameObj.GetComponent<TextMeshProUGUI>(),
                typeObj.GetComponent<TextMeshProUGUI>(),
                statsObj.GetComponent<TextMeshProUGUI>(),
                descObj.GetComponent<TextMeshProUGUI>()
            );

            mainUI.tooltipUI = ttUI;

            // Zastosuj customowy średniowieczny font do wszystkich tekstów UI jeśli dostępny
            if (gameFont != null)
            {
                var allTMP = eqRoot.GetComponentsInChildren<TextMeshProUGUI>(true);
                foreach (var tmp in allTMP)
                {
                    if (tmp != null) tmp.font = gameFont;
                }
            }

            // Inicjalizacja startowego zestawu pancerza i odświeżenie UI
            mainUI.InitializeStarterArmor();
            mainUI.RecalculateArmorStats();
            mainUI.RefreshUI();

            return eqRoot;
        }

        private static GameObject CreateSlotObject(Transform parent, string name, int index, bool isTrash, float size, Sprite slotSprite, Sprite activeSprite, Sprite silhouetteSprite)
        {
            GameObject slotObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Outline), typeof(MinecraftSlotUI), typeof(LayoutElement));
            slotObj.transform.SetParent(parent, false);

            RectTransform sRect = slotObj.GetComponent<RectTransform>();
            sRect.sizeDelta = new Vector2(size, size);

            LayoutElement le = slotObj.GetComponent<LayoutElement>();
            le.minWidth = size;
            le.minHeight = size;
            le.preferredWidth = size;
            le.preferredHeight = size;
            le.flexibleWidth = 0;
            le.flexibleHeight = 0;

            Image bg = slotObj.GetComponent<Image>();
            bg.sprite = null;
            bg.color = isTrash 
                ? new Color(0.18f, 0.08f, 0.06f, 0.95f) 
                : new Color(0.08f, 0.08f, 0.10f, 0.96f);
            bg.raycastTarget = true;

            // Obwódka slotu w stylu mosiądz / kute żelazo
            Outline slotOut = slotObj.GetComponent<Outline>();
            slotOut.effectColor = isTrash 
                ? new Color(0.75f, 0.40f, 0.20f, 0.85f) 
                : new Color(0.48f, 0.40f, 0.26f, 0.75f);
            slotOut.effectDistance = new Vector2(1.5f, -1.5f);

            MinecraftSlotUI slotUI = slotObj.GetComponent<MinecraftSlotUI>();
            slotUI.slotIndex = index;
            slotUI.isTrashSlot = isTrash;
            slotUI.slotBackground = bg;

            // Sylwetka placeholder (dla slotów zbroi)
            if (silhouetteSprite != null)
            {
                GameObject silObj = new GameObject("PlaceholderSilhouette", typeof(RectTransform), typeof(Image));
                silObj.transform.SetParent(slotObj.transform, false);
                RectTransform silRect = silObj.GetComponent<RectTransform>();
                silRect.anchorMin = Vector2.zero;
                silRect.anchorMax = Vector2.one;
                silRect.sizeDelta = new Vector2(-8f, -8f);
                Image silImg = silObj.GetComponent<Image>();
                silImg.sprite = silhouetteSprite;
                silImg.color = new Color(0.85f, 0.75f, 0.55f, 0.35f); // Subtelne antyczne złoto
                silImg.preserveAspect = true;
                silImg.raycastTarget = false;
                slotUI.placeholderSilhouette = silImg;
            }

            // Hover overlay (Złocisty blask najechania)
            GameObject hoverObj = new GameObject("HoverOverlay", typeof(RectTransform), typeof(Image));
            hoverObj.transform.SetParent(slotObj.transform, false);
            RectTransform hovRect = hoverObj.GetComponent<RectTransform>();
            hovRect.anchorMin = Vector2.zero;
            hovRect.anchorMax = Vector2.one;
            hovRect.sizeDelta = Vector2.zero;
            Image hovImg = hoverObj.GetComponent<Image>();
            hovImg.color = new Color(1f, 0.88f, 0.45f, 0.22f);
            hovImg.raycastTarget = false;
            slotUI.hoverOverlay = hovImg;
            hoverObj.SetActive(false);

            // Active Hotbar Border (Wyrazista złota ramka aktywnego slotu)
            GameObject borderObj = new GameObject("ActiveBorder", typeof(RectTransform), typeof(Image), typeof(Outline));
            borderObj.transform.SetParent(slotObj.transform, false);
            RectTransform bordRect = borderObj.GetComponent<RectTransform>();
            bordRect.anchorMin = Vector2.zero;
            bordRect.anchorMax = Vector2.one;
            bordRect.sizeDelta = new Vector2(4f, 4f);
            Image bordImg = borderObj.GetComponent<Image>();
            bordImg.color = new Color(1f, 0.84f, 0.25f, 0.85f);
            bordImg.raycastTarget = false;
            Outline activeOut = borderObj.GetComponent<Outline>();
            activeOut.effectColor = new Color(0.85f, 0.65f, 0.15f, 1f);
            activeOut.effectDistance = new Vector2(1.5f, -1.5f);
            slotUI.activeBorder = borderObj;
            borderObj.SetActive(false);

            // Ikona przedmiotu
            GameObject iconObj = new GameObject("ItemIcon", typeof(RectTransform), typeof(Image));
            iconObj.transform.SetParent(slotObj.transform, false);
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.sizeDelta = new Vector2(-6f, -6f);
            Image iconImg = iconObj.GetComponent<Image>();
            iconImg.raycastTarget = false;
            slotUI.itemIcon = iconImg;
            iconObj.SetActive(false);

            // Numer slotu (dla hotbara 1-9)
            if (index >= 0 && index < 9)
            {
                GameObject numObj = new GameObject("SlotNumber", typeof(RectTransform), typeof(TextMeshProUGUI));
                numObj.transform.SetParent(slotObj.transform, false);
                RectTransform numRect = numObj.GetComponent<RectTransform>();
                numRect.anchorMin = new Vector2(0f, 1f);
                numRect.anchorMax = new Vector2(0.4f, 1f);
                numRect.pivot = new Vector2(0f, 1f);
                numRect.anchoredPosition = new Vector2(4f, -2f);
                numRect.sizeDelta = new Vector2(20f, 18f);
                TextMeshProUGUI numTxt = numObj.GetComponent<TextMeshProUGUI>();
                numTxt.text = (index + 1).ToString();
                numTxt.fontSize = 11;
                numTxt.color = new Color(0.95f, 0.85f, 0.60f, 0.85f); // Złotawy numer
                numTxt.raycastTarget = false;
                slotUI.slotNumberText = numTxt;
            }

            // Licznik ilości
            GameObject countObj = new GameObject("CountText", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(Outline));
            countObj.transform.SetParent(slotObj.transform, false);
            RectTransform countRect = countObj.GetComponent<RectTransform>();
            countRect.anchorMin = new Vector2(0.4f, 0f);
            countRect.anchorMax = new Vector2(1f, 0.45f);
            countRect.pivot = new Vector2(1f, 0f);
            countRect.anchoredPosition = new Vector2(-3f, 3f);
            countRect.sizeDelta = new Vector2(35f, 22f);
            TextMeshProUGUI countTxt = countObj.GetComponent<TextMeshProUGUI>();
            countTxt.fontSize = 17;
            countTxt.fontStyle = FontStyles.Bold;
            countTxt.alignment = TextAlignmentOptions.BottomRight;
            countTxt.color = Color.white;
            countTxt.raycastTarget = false;

            Outline countOutl = countObj.GetComponent<Outline>();
            countOutl.effectColor = new Color(0.05f, 0.05f, 0.05f, 1f);
            countOutl.effectDistance = new Vector2(1.5f, -1.5f);

            slotUI.countText = countTxt;
            countObj.SetActive(false);

            return slotObj;
        }

        private static GameObject CreateTooltipText(Transform parent, string name, float size, FontStyles style, Color color)
        {
            GameObject txtObj = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
            txtObj.transform.SetParent(parent, false);
            TextMeshProUGUI txt = txtObj.GetComponent<TextMeshProUGUI>();
            txt.fontSize = size;
            txt.fontStyle = style;
            txt.color = color;
            txt.textWrappingMode = TextWrappingModes.Normal;
            txt.overflowMode = TextOverflowModes.Overflow;
            txt.raycastTarget = false;

            LayoutElement le = txtObj.GetComponent<LayoutElement>();
            le.preferredWidth = 282f;

            return txtObj;
        }
    }
}
