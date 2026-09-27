#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BlackJack.Editor
{
    public static class BlackJackUIBuilder
    {
        private static TMP_FontAsset GetMedievalFont()
        {
            string[] possiblePaths = new string[]
            {
                "Assets/Praktyki/IbarraRealNova-VariableFont_wght SDF.asset",
                "Assets/Praktyki/SaveSystem/Graphics/IbarraRealNova-VariableFont_wght SDF.asset"
            };

            foreach (string path in possiblePaths)
            {
                TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
                if (font != null) return font;
            }

            string[] guids = AssetDatabase.FindAssets("IbarraRealNova t:TMP_FontAsset");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                return AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(path);
            }

            return null;
        }

        [MenuItem("Tools/BlackJack/1. Podepnij Hazardziste pod Zaznaczony Obiekt")]
        public static void SetupSelectedNPC()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                EditorUtility.DisplayDialog("Brak zaznaczenia", "Zaznacz w Hierarchy obiekt swojej postaci hazardzisty!", "OK");
                return;
            }

            Collider col = selected.GetComponent<Collider>();
            if (col == null)
            {
                CapsuleCollider capsule = selected.AddComponent<CapsuleCollider>();
                capsule.height = 2f;
                capsule.radius = 0.5f;
                capsule.center = new Vector3(0, 1f, 0);
            }

            GamblerNPC npc = selected.GetComponent<GamblerNPC>();
            if (npc == null)
            {
                npc = selected.AddComponent<GamblerNPC>();
            }

            EditorUtility.SetDirty(selected);
            EditorUtility.DisplayDialog("Sukces!", $"Postać '{selected.name}' ma teraz poprawnie podpięty skrypt GamblerNPC i Collider!", "Super!");
        }

        [MenuItem("Tools/BlackJack/2. Stworz Kompletne Okno BlackJack w Canvas")]
        public static void CreateBlackJackUIInCanvas()
        {
            TMP_FontAsset medievalFont = GetMedievalFont();
            Canvas canvas = null;

            if (Selection.activeGameObject != null)
            {
                canvas = Selection.activeGameObject.GetComponentInParent<Canvas>();
            }

            if (canvas == null)
            {
                Canvas[] allCanvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
                foreach (Canvas c in allCanvases)
                {
                    if (c.renderMode != RenderMode.WorldSpace)
                    {
                        canvas = c;
                        break;
                    }
                }
            }

            if (canvas == null)
            {
                GameObject canvasObj = new GameObject("BlackJackCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObj.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
            }

            // Usuwamy stare okno, jeśli istnieje
            Transform existing = canvas.transform.Find("BlackJackWindow");
            if (existing != null)
            {
                Object.DestroyImmediate(existing.gameObject);
            }

            // GŁÓWNY PODKŁAD (Mroczna tawerna / pełnoekranowe przyciemnienie)
            GameObject windowRoot = CreateUIObject("BlackJackWindow", canvas.transform);
            RectTransform winRT = windowRoot.GetComponent<RectTransform>();
            winRT.anchorMin = Vector2.zero;
            winRT.anchorMax = Vector2.one;
            winRT.sizeDelta = Vector2.zero;
            winRT.anchoredPosition = Vector2.zero;

            Image bgImg = windowRoot.AddComponent<Image>();
            bgImg.color = new Color(0.04f, 0.03f, 0.02f, 0.95f); // Bardzo ciemny, klimatyczny brąz tawerny

            BlackJackGame game = windowRoot.AddComponent<BlackJackGame>();
            BlackJackUI ui = windowRoot.AddComponent<BlackJackUI>();

            // ZEWNĘTRZNA DREWNIANA RAMA STOŁU (Aged Oak Table Border)
            GameObject tableBorder = CreateUIObject("TableFrame", windowRoot.transform);
            RectTransform tbRT = tableBorder.GetComponent<RectTransform>();
            tbRT.anchorMin = new Vector2(0.5f, 0.5f);
            tbRT.anchorMax = new Vector2(0.5f, 0.5f);
            tbRT.pivot = new Vector2(0.5f, 0.5f);
            tbRT.sizeDelta = new Vector2(1200, 760);
            tbRT.anchoredPosition = Vector2.zero;
            Image frameImg = tableBorder.AddComponent<Image>();
            frameImg.color = new Color(0.16f, 0.10f, 0.06f, 1f); // Ciemny dąb tawerniany

            // ZŁOTA/MOSIĘŻNA OBWÓDKA STOŁU
            GameObject brassTrim = CreateUIObject("BrassTrim", tableBorder.transform);
            RectTransform btRT = brassTrim.GetComponent<RectTransform>();
            btRT.anchorMin = new Vector2(0.5f, 0.5f);
            btRT.anchorMax = new Vector2(0.5f, 0.5f);
            btRT.pivot = new Vector2(0.5f, 0.5f);
            btRT.sizeDelta = new Vector2(1176, 736);
            Image brassImg = brassTrim.AddComponent<Image>();
            brassImg.color = new Color(0.58f, 0.44f, 0.18f, 0.9f); // Antyczny mosiądz / stare złoto

            // WEWNĘTRZNE SUKNO STOŁU (Szmaragdowy filc karciany)
            GameObject tableFelt = CreateUIObject("TableFelt", brassTrim.transform);
            RectTransform feltRT = tableFelt.GetComponent<RectTransform>();
            feltRT.anchorMin = new Vector2(0.5f, 0.5f);
            feltRT.anchorMax = new Vector2(0.5f, 0.5f);
            feltRT.pivot = new Vector2(0.5f, 0.5f);
            feltRT.sizeDelta = new Vector2(1168, 728);
            Image feltImg = tableFelt.AddComponent<Image>();
            feltImg.color = new Color(0.06f, 0.16f, 0.10f, 1f); // Głęboki leśny filc tawerny

            // ==================== 1. NAGŁÓWEK (HEADER) ====================
            GameObject header = CreateUIObject("Header", tableFelt.transform);
            RectTransform headerRT = header.GetComponent<RectTransform>();
            headerRT.anchorMin = new Vector2(0.5f, 1f);
            headerRT.anchorMax = new Vector2(0.5f, 1f);
            headerRT.pivot = new Vector2(0.5f, 1f);
            headerRT.anchoredPosition = new Vector2(0, -14);
            headerRT.sizeDelta = new Vector2(1120, 50);

            // Tytuł - Średniowieczny napis w złocie
            GameObject titleObj = CreateUIObject("TitleText", header.transform);
            RectTransform titleRT = titleObj.GetComponent<RectTransform>();
            titleRT.anchorMin = Vector2.zero;
            titleRT.anchorMax = Vector2.one;
            titleRT.offsetMin = new Vector2(60, 0);
            titleRT.offsetMax = new Vector2(-60, 0);

            TextMeshProUGUI titleTMP = titleObj.AddComponent<TextMeshProUGUI>();
            if (medievalFont != null) titleTMP.font = medievalFont;
            titleTMP.text = "OCZKO U HAZARDZISTY";
            titleTMP.fontSize = 28;
            titleTMP.fontStyle = FontStyles.Bold;
            titleTMP.alignment = TextAlignmentOptions.Center;
            titleTMP.color = new Color(0.95f, 0.82f, 0.35f, 1f); // Złoto karczemne
            titleTMP.enableWordWrapping = false;

            // Przycisk wyjścia (X) - Pieczęć woskowa / ciemna stal
            Button closeBtn = CreateButton(header.transform, "CloseButton", "X", new Vector2(44, 44), new Color(0.65f, 0.14f, 0.14f, 1f), medievalFont);
            RectTransform closeRT = closeBtn.GetComponent<RectTransform>();
            closeRT.anchorMin = new Vector2(1, 0.5f);
            closeRT.anchorMax = new Vector2(1, 0.5f);
            closeRT.pivot = new Vector2(1, 0.5f);
            closeRT.anchoredPosition = Vector2.zero;
            TextMeshProUGUI closeTMP = closeBtn.GetComponentInChildren<TextMeshProUGUI>();
            if (closeTMP != null)
            {
                closeTMP.fontSize = 24;
                closeTMP.fontStyle = FontStyles.Bold;
                closeTMP.color = new Color(0.95f, 0.88f, 0.75f, 1f);
            }

            // ==================== 2. DYMEK BOTA (TABLICZKA DIALOGOWA) ====================
            GameObject dialogueBox = CreateUIObject("DealerDialogueBox", tableFelt.transform);
            RectTransform dlgRT = dialogueBox.GetComponent<RectTransform>();
            dlgRT.anchorMin = new Vector2(0.5f, 1f);
            dlgRT.anchorMax = new Vector2(0.5f, 1f);
            dlgRT.pivot = new Vector2(0.5f, 1f);
            dlgRT.anchoredPosition = new Vector2(0, -70);
            dlgRT.sizeDelta = new Vector2(880, 48);

            Image dlgBg = dialogueBox.AddComponent<Image>();
            dlgBg.color = new Color(0.08f, 0.06f, 0.04f, 0.85f); // Ciemne drewno / żelazo

            GameObject dlgTextObj = CreateUIObject("DialogueText", dialogueBox.transform);
            RectTransform dlgTextRT = dlgTextObj.GetComponent<RectTransform>();
            dlgTextRT.anchorMin = Vector2.zero;
            dlgTextRT.anchorMax = Vector2.one;
            dlgTextRT.offsetMin = new Vector2(15, 2);
            dlgTextRT.offsetMax = new Vector2(-15, -2);

            TextMeshProUGUI dialogueTMP = dlgTextObj.AddComponent<TextMeshProUGUI>();
            if (medievalFont != null) dialogueTMP.font = medievalFont;
            dialogueTMP.text = "<b>HAZARDZISTA:</b> \"STAWIAJ KAPSLE, FORTUNA SPRZYJA ODWAŻNYM!\"";
            dialogueTMP.fontSize = 18;
            dialogueTMP.alignment = TextAlignmentOptions.Center;
            dialogueTMP.color = new Color(0.94f, 0.90f, 0.80f, 1f); // Pergaminowy
            dialogueTMP.enableWordWrapping = true;

            // ==================== 3. SEKCJA KRUPIERA ====================
            GameObject dsObj = CreateUIObject("DealerScoreText", tableFelt.transform);
            RectTransform dsRT = dsObj.GetComponent<RectTransform>();
            dsRT.anchorMin = new Vector2(0.5f, 1f);
            dsRT.anchorMax = new Vector2(0.5f, 1f);
            dsRT.pivot = new Vector2(0.5f, 1f);
            dsRT.anchoredPosition = new Vector2(0, -128);
            dsRT.sizeDelta = new Vector2(500, 28);

            TextMeshProUGUI dealerScoreTMP = dsObj.AddComponent<TextMeshProUGUI>();
            if (medievalFont != null) dealerScoreTMP.font = medievalFont;
            dealerScoreTMP.text = "KRUPIER: 0";
            dealerScoreTMP.fontSize = 21;
            dealerScoreTMP.fontStyle = FontStyles.Bold;
            dealerScoreTMP.alignment = TextAlignmentOptions.Center;
            dealerScoreTMP.color = new Color(0.90f, 0.84f, 0.72f, 1f); // Stara kość słoniowa
            dealerScoreTMP.enableWordWrapping = false;

            // Kontener kart krupiera
            GameObject dealerCards = CreateUIObject("DealerCards", tableFelt.transform);
            RectTransform dcRT = dealerCards.GetComponent<RectTransform>();
            dcRT.anchorMin = new Vector2(0.5f, 1f);
            dcRT.anchorMax = new Vector2(0.5f, 1f);
            dcRT.pivot = new Vector2(0.5f, 1f);
            dcRT.anchoredPosition = new Vector2(0, -164);
            dcRT.sizeDelta = new Vector2(750, 135);

            HorizontalLayoutGroup dcLayout = dealerCards.AddComponent<HorizontalLayoutGroup>();
            dcLayout.childAlignment = TextAnchor.MiddleCenter;
            dcLayout.spacing = 16;
            dcLayout.childControlWidth = false;
            dcLayout.childControlHeight = false;

            // ==================== 4. BANER WYNIKU ====================
            GameObject resultBanner = CreateUIObject("ResultBanner", tableFelt.transform);
            RectTransform rbRT = resultBanner.GetComponent<RectTransform>();
            rbRT.anchorMin = new Vector2(0.5f, 0.5f);
            rbRT.anchorMax = new Vector2(0.5f, 0.5f);
            rbRT.pivot = new Vector2(0.5f, 0.5f);
            rbRT.anchoredPosition = new Vector2(0, 10);
            rbRT.sizeDelta = new Vector2(650, 85);

            Image rbBg = resultBanner.AddComponent<Image>();
            rbBg.color = new Color(0.06f, 0.04f, 0.02f, 0.95f);

            GameObject rtObj = CreateUIObject("ResultTitleText", resultBanner.transform);
            RectTransform rtRT = rtObj.GetComponent<RectTransform>();
            rtRT.anchorMin = new Vector2(0, 0.45f);
            rtRT.anchorMax = new Vector2(1, 1);
            rtRT.offsetMin = rtRT.offsetMax = Vector2.zero;
            TextMeshProUGUI resultTitleTMP = rtObj.AddComponent<TextMeshProUGUI>();
            if (medievalFont != null) resultTitleTMP.font = medievalFont;
            resultTitleTMP.text = "WYGRANA!";
            resultTitleTMP.fontSize = 28;
            resultTitleTMP.fontStyle = FontStyles.Bold;
            resultTitleTMP.alignment = TextAlignmentOptions.Center;
            resultTitleTMP.color = new Color(0.95f, 0.82f, 0.35f, 1f);

            GameObject rpObj = CreateUIObject("ResultPayoutText", resultBanner.transform);
            RectTransform rpRT = rpObj.GetComponent<RectTransform>();
            rpRT.anchorMin = new Vector2(0, 0);
            rpRT.anchorMax = new Vector2(1, 0.45f);
            rpRT.offsetMin = rpRT.offsetMax = Vector2.zero;
            TextMeshProUGUI resultPayoutTMP = rpObj.AddComponent<TextMeshProUGUI>();
            if (medievalFont != null) resultPayoutTMP.font = medievalFont;
            resultPayoutTMP.text = "+20 KAPSLI!";
            resultPayoutTMP.fontSize = 20;
            resultPayoutTMP.alignment = TextAlignmentOptions.Center;
            resultPayoutTMP.color = new Color(0.35f, 0.85f, 0.45f, 1f);

            resultBanner.SetActive(false);

            // ==================== 5. SEKCJA GRACZA ====================
            GameObject playerCards = CreateUIObject("PlayerCards", tableFelt.transform);
            RectTransform pcRT = playerCards.GetComponent<RectTransform>();
            pcRT.anchorMin = new Vector2(0.5f, 0f);
            pcRT.anchorMax = new Vector2(0.5f, 0f);
            pcRT.pivot = new Vector2(0.5f, 0f);
            pcRT.anchoredPosition = new Vector2(0, 220);
            pcRT.sizeDelta = new Vector2(750, 135);

            HorizontalLayoutGroup pcLayout = playerCards.AddComponent<HorizontalLayoutGroup>();
            pcLayout.childAlignment = TextAnchor.MiddleCenter;
            pcLayout.spacing = 16;
            pcLayout.childControlWidth = false;
            pcLayout.childControlHeight = false;

            GameObject psObj = CreateUIObject("PlayerScoreText", tableFelt.transform);
            RectTransform psRT = psObj.GetComponent<RectTransform>();
            psRT.anchorMin = new Vector2(0.5f, 0f);
            psRT.anchorMax = new Vector2(0.5f, 0f);
            psRT.pivot = new Vector2(0.5f, 0f);
            psRT.anchoredPosition = new Vector2(0, 180);
            psRT.sizeDelta = new Vector2(500, 28);

            TextMeshProUGUI playerScoreTMP = psObj.AddComponent<TextMeshProUGUI>();
            if (medievalFont != null) playerScoreTMP.font = medievalFont;
            playerScoreTMP.text = "TWOJE PUNKTY: 0";
            playerScoreTMP.fontSize = 21;
            playerScoreTMP.fontStyle = FontStyles.Bold;
            playerScoreTMP.alignment = TextAlignmentOptions.Center;
            playerScoreTMP.color = new Color(0.90f, 0.84f, 0.72f, 1f);
            playerScoreTMP.enableWordWrapping = false;

            // ==================== 6. TABLICA STATUSU (KAPSLE, ZAKŁAD, PULA) ====================
            GameObject statusBar = CreateUIObject("StatusBar", tableFelt.transform);
            RectTransform sbRT = statusBar.GetComponent<RectTransform>();
            sbRT.anchorMin = new Vector2(0.5f, 0f);
            sbRT.anchorMax = new Vector2(0.5f, 0f);
            sbRT.pivot = new Vector2(0.5f, 0f);
            sbRT.anchoredPosition = new Vector2(0, 120);
            sbRT.sizeDelta = new Vector2(950, 42);

            Image sbBg = statusBar.AddComponent<Image>();
            sbBg.color = new Color(0.08f, 0.06f, 0.04f, 0.85f); // Okuta deska

            // Kapsle
            GameObject capsObj = CreateUIObject("PlayerCapsText", statusBar.transform);
            RectTransform capsRT = capsObj.GetComponent<RectTransform>();
            capsRT.anchorMin = new Vector2(0, 0);
            capsRT.anchorMax = new Vector2(0.33f, 1);
            capsRT.offsetMin = new Vector2(20, 0);
            capsRT.offsetMax = Vector2.zero;
            TextMeshProUGUI playerCapsTMP = capsObj.AddComponent<TextMeshProUGUI>();
            if (medievalFont != null) playerCapsTMP.font = medievalFont;
            playerCapsTMP.text = "POSIADANE KAPSLE: 0";
            playerCapsTMP.fontSize = 19;
            playerCapsTMP.fontStyle = FontStyles.Bold;
            playerCapsTMP.alignment = TextAlignmentOptions.Left;
            playerCapsTMP.color = new Color(0.95f, 0.82f, 0.35f, 1f); // Złoty
            playerCapsTMP.enableWordWrapping = false;

            // Zakład
            GameObject betObj = CreateUIObject("CurrentBetText", statusBar.transform);
            RectTransform betRT = betObj.GetComponent<RectTransform>();
            betRT.anchorMin = new Vector2(0.33f, 0);
            betRT.anchorMax = new Vector2(0.66f, 1);
            betRT.offsetMin = betRT.offsetMax = Vector2.zero;
            TextMeshProUGUI currentBetTMP = betObj.AddComponent<TextMeshProUGUI>();
            if (medievalFont != null) currentBetTMP.font = medievalFont;
            currentBetTMP.text = "ZAKŁAD: 10 KAPSLI";
            currentBetTMP.fontSize = 19;
            currentBetTMP.fontStyle = FontStyles.Bold;
            currentBetTMP.alignment = TextAlignmentOptions.Center;
            currentBetTMP.color = new Color(0.90f, 0.88f, 0.82f, 1f); // Pergamin
            currentBetTMP.enableWordWrapping = false;

            // Pula
            GameObject potObj = CreateUIObject("PotText", statusBar.transform);
            RectTransform potRT = potObj.GetComponent<RectTransform>();
            potRT.anchorMin = new Vector2(0.66f, 0);
            potRT.anchorMax = new Vector2(1, 1);
            potRT.offsetMin = Vector2.zero;
            potRT.offsetMax = new Vector2(-20, 0);
            TextMeshProUGUI potTMP = potObj.AddComponent<TextMeshProUGUI>();
            if (medievalFont != null) potTMP.font = medievalFont;
            potTMP.text = "PULA: 20 KAPSLI";
            potTMP.fontSize = 19;
            potTMP.fontStyle = FontStyles.Bold;
            potTMP.alignment = TextAlignmentOptions.Right;
            potTMP.color = new Color(0.35f, 0.88f, 0.45f, 1f); // Szmaragd
            potTMP.enableWordWrapping = false;

            // ==================== 7. PRZYCISKI ZAKŁADÓW (ŚREDNIOWIECZNE) ====================
            GameObject bettingControls = CreateUIObject("BettingControlsGroup", tableFelt.transform);
            RectTransform bcRT = bettingControls.GetComponent<RectTransform>();
            bcRT.anchorMin = new Vector2(0.5f, 0f);
            bcRT.anchorMax = new Vector2(0.5f, 0f);
            bcRT.pivot = new Vector2(0.5f, 0f);
            bcRT.anchoredPosition = new Vector2(0, 25);
            bcRT.sizeDelta = new Vector2(1080, 60);

            Color chipColor = new Color(0.20f, 0.22f, 0.25f, 1f); // Kute żelazo
            Button b5 = CreateButton(bettingControls.transform, "Btn+5", "+5", new Vector2(70, 46), chipColor, medievalFont);
            Button b10 = CreateButton(bettingControls.transform, "Btn+10", "+10", new Vector2(70, 46), chipColor, medievalFont);
            Button b25 = CreateButton(bettingControls.transform, "Btn+25", "+25", new Vector2(70, 46), chipColor, medievalFont);
            Button b50 = CreateButton(bettingControls.transform, "Btn+50", "+50", new Vector2(70, 46), chipColor, medievalFont);
            Button b100 = CreateButton(bettingControls.transform, "Btn+100", "+100", new Vector2(80, 46), chipColor, medievalFont);
            Button bClear = CreateButton(bettingControls.transform, "BtnClear", "WYCZYŚĆ", new Vector2(95, 46), new Color(0.55f, 0.16f, 0.16f, 1f), medievalFont);
            Button bAllIn = CreateButton(bettingControls.transform, "BtnAllIn", "ALL-IN!", new Vector2(85, 46), new Color(0.68f, 0.38f, 0.08f, 1f), medievalFont);
            Button bDeal = CreateButton(bettingControls.transform, "BtnDeal", "ROZDAJ KARTY", new Vector2(210, 52), new Color(0.12f, 0.48f, 0.22f, 1f), medievalFont);

            HorizontalLayoutGroup bcLayout = bettingControls.AddComponent<HorizontalLayoutGroup>();
            bcLayout.childAlignment = TextAnchor.MiddleCenter;
            bcLayout.spacing = 10;
            bcLayout.childControlWidth = false;
            bcLayout.childControlHeight = false;

            // ==================== 8. PRZYCISKI GRY ====================
            GameObject gameplayControls = CreateUIObject("GameplayControlsGroup", tableFelt.transform);
            RectTransform gcRT = gameplayControls.GetComponent<RectTransform>();
            gcRT.anchorMin = new Vector2(0.5f, 0f);
            gcRT.anchorMax = new Vector2(0.5f, 0f);
            gcRT.pivot = new Vector2(0.5f, 0f);
            gcRT.anchoredPosition = new Vector2(0, 25);
            gcRT.sizeDelta = new Vector2(800, 60);

            Button bHit = CreateButton(gameplayControls.transform, "BtnHit", "DOBIERZ (H)", new Vector2(200, 50), new Color(0.12f, 0.48f, 0.22f, 1f), medievalFont);
            Button bStand = CreateButton(gameplayControls.transform, "BtnStand", "PASUJ (S)", new Vector2(200, 50), new Color(0.60f, 0.16f, 0.16f, 1f), medievalFont);
            Button bDouble = CreateButton(gameplayControls.transform, "BtnDouble", "PODWÓJ (D)", new Vector2(200, 50), new Color(0.70f, 0.44f, 0.10f, 1f), medievalFont);

            HorizontalLayoutGroup gcLayout = gameplayControls.AddComponent<HorizontalLayoutGroup>();
            gcLayout.childAlignment = TextAnchor.MiddleCenter;
            gcLayout.spacing = 25;
            gcLayout.childControlWidth = false;
            gcLayout.childControlHeight = false;
            gameplayControls.SetActive(false);

            // ==================== 9. PRZYCISKI KOŃCA RUNDY ====================
            GameObject endRoundControls = CreateUIObject("EndRoundControlsGroup", tableFelt.transform);
            RectTransform erRT = endRoundControls.GetComponent<RectTransform>();
            erRT.anchorMin = new Vector2(0.5f, 0f);
            erRT.anchorMax = new Vector2(0.5f, 0f);
            erRT.pivot = new Vector2(0.5f, 0f);
            erRT.anchoredPosition = new Vector2(0, 25);
            erRT.sizeDelta = new Vector2(700, 60);

            Button bNext = CreateButton(endRoundControls.transform, "BtnNextRound", "NOWA PARTIA (SPACJA)", new Vector2(250, 50), new Color(0.12f, 0.48f, 0.22f, 1f), medievalFont);
            Button bRebet = CreateButton(endRoundControls.transform, "BtnRebet", "POWTÓRZ ZAKŁAD I GRAJ", new Vector2(260, 50), new Color(0.18f, 0.35f, 0.55f, 1f), medievalFont);

            HorizontalLayoutGroup erLayout = endRoundControls.AddComponent<HorizontalLayoutGroup>();
            erLayout.childAlignment = TextAnchor.MiddleCenter;
            erLayout.spacing = 25;
            erLayout.childControlWidth = false;
            erLayout.childControlHeight = false;
            endRoundControls.SetActive(false);

            // ==================== PODPIĘCIE REFERENCJI ====================
            SerializedObject soUI = new SerializedObject(ui);
            soUI.FindProperty("windowRoot").objectReferenceValue = windowRoot;
            soUI.FindProperty("closeButton").objectReferenceValue = closeBtn;
            soUI.FindProperty("game").objectReferenceValue = game;
            soUI.FindProperty("playerCardContainer").objectReferenceValue = playerCards.transform;
            soUI.FindProperty("dealerCardContainer").objectReferenceValue = dealerCards.transform;
            soUI.FindProperty("playerScoreText").objectReferenceValue = playerScoreTMP;
            soUI.FindProperty("dealerScoreText").objectReferenceValue = dealerScoreTMP;
            soUI.FindProperty("playerCapsText").objectReferenceValue = playerCapsTMP;
            soUI.FindProperty("currentBetText").objectReferenceValue = currentBetTMP;
            soUI.FindProperty("potText").objectReferenceValue = potTMP;
            soUI.FindProperty("dealerDialogueText").objectReferenceValue = dialogueTMP;
            soUI.FindProperty("resultBanner").objectReferenceValue = resultBanner;
            soUI.FindProperty("resultTitleText").objectReferenceValue = resultTitleTMP;
            soUI.FindProperty("resultPayoutText").objectReferenceValue = resultPayoutTMP;
            soUI.FindProperty("bettingControlsGroup").objectReferenceValue = bettingControls;
            soUI.FindProperty("gameplayControlsGroup").objectReferenceValue = gameplayControls;
            soUI.FindProperty("endRoundControlsGroup").objectReferenceValue = endRoundControls;
            soUI.FindProperty("dealButton").objectReferenceValue = bDeal;
            soUI.FindProperty("clearBetButton").objectReferenceValue = bClear;
            soUI.FindProperty("allInButton").objectReferenceValue = bAllIn;
            soUI.FindProperty("betAdd5Button").objectReferenceValue = b5;
            soUI.FindProperty("betAdd10Button").objectReferenceValue = b10;
            soUI.FindProperty("betAdd25Button").objectReferenceValue = b25;
            soUI.FindProperty("betAdd50Button").objectReferenceValue = b50;
            soUI.FindProperty("betAdd100Button").objectReferenceValue = b100;
            soUI.FindProperty("hitButton").objectReferenceValue = bHit;
            soUI.FindProperty("standButton").objectReferenceValue = bStand;
            soUI.FindProperty("doubleButton").objectReferenceValue = bDouble;
            soUI.FindProperty("nextRoundButton").objectReferenceValue = bNext;
            soUI.FindProperty("rebetButton").objectReferenceValue = bRebet;
            soUI.ApplyModifiedProperties();

            windowRoot.SetActive(false);

            EditorUtility.SetDirty(windowRoot);
            EditorUtility.DisplayDialog("Sukces!", "Średniowieczny stół BlackJacka z czcionką Ibarra Real Nova (ALL CAPS) został utworzony!", "Znakomicie!");
        }

        private static GameObject CreateUIObject(string name, Transform parent)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            return obj;
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 size, Color bgColor, TMP_FontAsset font = null)
        {
            GameObject btnObj = CreateUIObject(name, parent);
            RectTransform rt = btnObj.GetComponent<RectTransform>();
            rt.sizeDelta = size;

            Image img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.highlightedColor = bgColor * 1.25f;
            colors.pressedColor = bgColor * 0.75f;
            btn.colors = colors;

            GameObject textObj = CreateUIObject("Text", btnObj.transform);
            RectTransform textRT = textObj.GetComponent<RectTransform>();
            textRT.anchorMin = Vector2.zero;
            textRT.anchorMax = Vector2.one;
            textRT.offsetMin = textRT.offsetMax = Vector2.zero;

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            if (font != null) tmp.font = font;
            tmp.text = label.ToUpper();
            tmp.fontSize = 17;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = new Color(0.95f, 0.90f, 0.82f, 1f); // Stary pergamin
            tmp.fontStyle = FontStyles.Bold;
            tmp.enableWordWrapping = false;

            return btn;
        }
    }
}
#endif
