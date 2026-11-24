using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;
using TheTycoon.UI.Bootstrap;
using TheTycoon.UI.Dashboard;
using TheTycoon.UI.Events;
using TheTycoon.UI.Finance;

namespace TheTycoon.Editor
{
    public static class GameSceneBuilder
    {
        [MenuItem("TheTycoon/Build Game Scene")]
        public static void BuildScene()
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 1. Camera
            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.067f, 0.094f, 0.153f); // Dark navy #111827
            camera.orthographic = true;
            camera.orthographicSize = 5;
            cameraGo.transform.position = new Vector3(0, 0, -10);

            // 2. EventSystem with InputSystemUIInputModule
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<InputSystemUIInputModule>();

            // 3. Canvas
            var canvasGo = new GameObject("Canvas");
            var canvas = canvasGo.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            canvasGo.AddComponent<GraphicRaycaster>();

            // 4. Background
            var bgGo = CreateUiObject("Background", canvasGo.transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);
            var bgImage = bgGo.AddComponent<Image>();
            bgImage.color = new Color(0.07f, 0.09f, 0.14f); // #121724

            // 5. Dashboard Root
            var dashboardGo = CreateUiObject("Dashboard", canvasGo.transform, new Vector2(0, 0), new Vector2(1, 1), Vector2.zero, Vector2.zero);

            // 5a. TopBar
            var topBarGo = CreateUiObject("TopBar", dashboardGo.transform, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -90), new Vector2(0, 0));
            var topBarImage = topBarGo.AddComponent<Image>();
            topBarImage.color = new Color(0.12f, 0.16f, 0.23f); // #1F293B
            var topBarView = topBarGo.AddComponent<TopBarView>();

            var companyLabelGo = CreateUiObject("CompanyNameLabel", topBarGo.transform, new Vector2(0, 0), new Vector2(0.35f, 1), new Vector2(30, 0), new Vector2(0, 0));
            var companyLabel = companyLabelGo.AddComponent<TextMeshProUGUI>();
            companyLabel.text = "Nexus Corporation";
            companyLabel.fontSize = 26;
            companyLabel.fontStyle = FontStyles.Bold;
            companyLabel.color = new Color(0.95f, 0.96f, 0.98f);
            companyLabel.alignment = TextAlignmentOptions.MidlineLeft;

            var dateLabelGo = CreateUiObject("DateLabel", topBarGo.transform, new Vector2(0.35f, 0), new Vector2(0.6f, 1), Vector2.zero, Vector2.zero);
            var dateLabel = dateLabelGo.AddComponent<TextMeshProUGUI>();
            dateLabel.text = "January 2026 (Q1)";
            dateLabel.fontSize = 20;
            dateLabel.color = new Color(0.85f, 0.88f, 0.92f);
            dateLabel.alignment = TextAlignmentOptions.Center;

            var repLabelGo = CreateUiObject("ReputationLabel", topBarGo.transform, new Vector2(0.6f, 0), new Vector2(0.8f, 1), Vector2.zero, Vector2.zero);
            var repLabel = repLabelGo.AddComponent<TextMeshProUGUI>();
            repLabel.text = "Reputation: 50%";
            repLabel.fontSize = 18;
            repLabel.color = new Color(0.96f, 0.62f, 0.04f); // Gold #F59E0B
            repLabel.alignment = TextAlignmentOptions.Center;

            // Advance Month Button
            var advanceBtnGo = CreateUiObject("AdvanceMonthButton", topBarGo.transform, new Vector2(0.82f, 0.15f), new Vector2(0.98f, 0.85f), Vector2.zero, Vector2.zero);
            var advanceBtnImage = advanceBtnGo.AddComponent<Image>();
            advanceBtnImage.color = new Color(0.85f, 0.47f, 0.02f); // #D97706
            var advanceBtn = advanceBtnGo.AddComponent<Button>();
            var btnTextGo = CreateUiObject("ButtonText", advanceBtnGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var btnText = btnTextGo.AddComponent<TextMeshProUGUI>();
            btnText.text = "ADVANCE MONTH";
            btnText.fontSize = 18;
            btnText.fontStyle = FontStyles.Bold;
            btnText.color = Color.white;
            btnText.alignment = TextAlignmentOptions.Center;

            // Wire TopBarView serialized fields
            var topBarSerialized = new SerializedObject(topBarView);
            topBarSerialized.FindProperty("_companyNameLabel").objectReferenceValue = companyLabel;
            topBarSerialized.FindProperty("_dateLabel").objectReferenceValue = dateLabel;
            topBarSerialized.FindProperty("_reputationLabel").objectReferenceValue = repLabel;
            topBarSerialized.FindProperty("_advanceMonthButton").objectReferenceValue = advanceBtn;
            topBarSerialized.ApplyModifiedPropertiesWithoutUndo();

            // 5b. Financial Overview Card
            var finCardGo = CreateUiObject("FinancialOverviewCard", dashboardGo.transform, new Vector2(0.05f, 0.45f), new Vector2(0.45f, 0.85f), Vector2.zero, Vector2.zero);
            var finCardImage = finCardGo.AddComponent<Image>();
            finCardImage.color = new Color(0.12f, 0.16f, 0.23f);
            var finOverviewView = finCardGo.AddComponent<FinancialOverviewView>();

            var treasuryTitleGo = CreateUiObject("TreasuryTitle", finCardGo.transform, new Vector2(0.08f, 0.78f), new Vector2(0.92f, 0.92f), Vector2.zero, Vector2.zero);
            var treasuryTitle = treasuryTitleGo.AddComponent<TextMeshProUGUI>();
            treasuryTitle.text = "CORPORATE TREASURY";
            treasuryTitle.fontSize = 16;
            treasuryTitle.fontStyle = FontStyles.Bold;
            treasuryTitle.color = new Color(0.6f, 0.65f, 0.75f);
            treasuryTitle.alignment = TextAlignmentOptions.MidlineLeft;

            var cashValGo = CreateUiObject("CashLabel", finCardGo.transform, new Vector2(0.08f, 0.52f), new Vector2(0.92f, 0.78f), Vector2.zero, Vector2.zero);
            var cashVal = cashValGo.AddComponent<TextMeshProUGUI>();
            cashVal.text = "RM250,000.00";
            cashVal.fontSize = 36;
            cashVal.fontStyle = FontStyles.Bold;
            cashVal.color = new Color(0.1f, 0.75f, 0.45f); // Emerald
            cashVal.alignment = TextAlignmentOptions.MidlineLeft;

            var perfTitleGo = CreateUiObject("PerformanceTitle", finCardGo.transform, new Vector2(0.08f, 0.38f), new Vector2(0.92f, 0.48f), Vector2.zero, Vector2.zero);
            var perfTitle = perfTitleGo.AddComponent<TextMeshProUGUI>();
            perfTitle.text = "PREVIOUS MONTH PERFORMANCE";
            perfTitle.fontSize = 14;
            perfTitle.color = new Color(0.6f, 0.65f, 0.75f);
            perfTitle.alignment = TextAlignmentOptions.MidlineLeft;

            var revLabelGo = CreateUiObject("RevenueLabel", finCardGo.transform, new Vector2(0.08f, 0.26f), new Vector2(0.92f, 0.36f), Vector2.zero, Vector2.zero);
            var revLabel = revLabelGo.AddComponent<TextMeshProUGUI>();
            revLabel.text = "Revenue: RM0.00";
            revLabel.fontSize = 16;
            revLabel.color = Color.white;

            var expLabelGo = CreateUiObject("ExpensesLabel", finCardGo.transform, new Vector2(0.08f, 0.15f), new Vector2(0.92f, 0.25f), Vector2.zero, Vector2.zero);
            var expLabel = expLabelGo.AddComponent<TextMeshProUGUI>();
            expLabel.text = "Expenses: RM0.00";
            expLabel.fontSize = 16;
            expLabel.color = Color.white;

            var netLabelGo = CreateUiObject("NetProfitLabel", finCardGo.transform, new Vector2(0.08f, 0.04f), new Vector2(0.92f, 0.14f), Vector2.zero, Vector2.zero);
            var netLabel = netLabelGo.AddComponent<TextMeshProUGUI>();
            netLabel.text = "Net: Not yet available";
            netLabel.fontSize = 16;
            netLabel.fontStyle = FontStyles.Bold;
            netLabel.color = Color.gray;

            // Wire FinancialOverviewView serialized fields
            var finSerialized = new SerializedObject(finOverviewView);
            finSerialized.FindProperty("_cashLabel").objectReferenceValue = cashVal;
            finSerialized.FindProperty("_revenueLabel").objectReferenceValue = revLabel;
            finSerialized.FindProperty("_expensesLabel").objectReferenceValue = expLabel;
            finSerialized.FindProperty("_netProfitLabel").objectReferenceValue = netLabel;
            finSerialized.ApplyModifiedPropertiesWithoutUndo();

            // 5c. Monthly Financial Report Modal
            var reportModalGo = CreateUiObject("MonthlyReportModal", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var reportBackdrop = reportModalGo.AddComponent<Image>();
            reportBackdrop.color = new Color(0, 0, 0, 0.75f); // Dimmed background
            var monthlyReportView = reportModalGo.AddComponent<MonthlyReportView>();

            var modalCardGo = CreateUiObject("ReportCard", reportModalGo.transform, new Vector2(0.3f, 0.25f), new Vector2(0.7f, 0.75f), Vector2.zero, Vector2.zero);
            var modalCardImage = modalCardGo.AddComponent<Image>();
            modalCardImage.color = new Color(0.1f, 0.13f, 0.19f);

            var reportTitleGo = CreateUiObject("ReportTitle", modalCardGo.transform, new Vector2(0.08f, 0.82f), new Vector2(0.92f, 0.95f), Vector2.zero, Vector2.zero);
            var reportTitle = reportTitleGo.AddComponent<TextMeshProUGUI>();
            reportTitle.text = "Monthly Financial Report";
            reportTitle.fontSize = 24;
            reportTitle.fontStyle = FontStyles.Bold;
            reportTitle.color = new Color(0.96f, 0.62f, 0.04f); // Gold
            reportTitle.alignment = TextAlignmentOptions.Center;

            var rptOpenGo = CreateUiObject("OpeningCash", modalCardGo.transform, new Vector2(0.1f, 0.68f), new Vector2(0.9f, 0.78f), Vector2.zero, Vector2.zero);
            var rptOpen = rptOpenGo.AddComponent<TextMeshProUGUI>();
            rptOpen.fontSize = 18;
            rptOpen.color = Color.white;

            var rptRevGo = CreateUiObject("Revenue", modalCardGo.transform, new Vector2(0.1f, 0.56f), new Vector2(0.9f, 0.66f), Vector2.zero, Vector2.zero);
            var rptRev = rptRevGo.AddComponent<TextMeshProUGUI>();
            rptRev.fontSize = 18;
            rptRev.color = Color.white;

            var rptExpGo = CreateUiObject("Expenses", modalCardGo.transform, new Vector2(0.1f, 0.44f), new Vector2(0.9f, 0.54f), Vector2.zero, Vector2.zero);
            var rptExp = rptExpGo.AddComponent<TextMeshProUGUI>();
            rptExp.fontSize = 18;
            rptExp.color = Color.white;

            var rptNetGo = CreateUiObject("NetProfit", modalCardGo.transform, new Vector2(0.1f, 0.32f), new Vector2(0.9f, 0.42f), Vector2.zero, Vector2.zero);
            var rptNet = rptNetGo.AddComponent<TextMeshProUGUI>();
            rptNet.fontSize = 20;
            rptNet.fontStyle = FontStyles.Bold;
            rptNet.color = Color.white;

            var rptCloseGo = CreateUiObject("ClosingCash", modalCardGo.transform, new Vector2(0.1f, 0.20f), new Vector2(0.9f, 0.30f), Vector2.zero, Vector2.zero);
            var rptClose = rptCloseGo.AddComponent<TextMeshProUGUI>();
            rptClose.fontSize = 18;
            rptClose.fontStyle = FontStyles.Bold;
            rptClose.color = new Color(0.1f, 0.75f, 0.45f);

            var dismissBtnGo = CreateUiObject("DismissButton", modalCardGo.transform, new Vector2(0.35f, 0.05f), new Vector2(0.65f, 0.15f), Vector2.zero, Vector2.zero);
            var dismissBtnImage = dismissBtnGo.AddComponent<Image>();
            dismissBtnImage.color = new Color(0.2f, 0.25f, 0.35f);
            var dismissBtn = dismissBtnGo.AddComponent<Button>();
            var dismissTextGo = CreateUiObject("DismissText", dismissBtnGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var dismissText = dismissTextGo.AddComponent<TextMeshProUGUI>();
            dismissText.text = "DISMISS";
            dismissText.fontSize = 16;
            dismissText.fontStyle = FontStyles.Bold;
            dismissText.color = Color.white;
            dismissText.alignment = TextAlignmentOptions.Center;

            // Wire MonthlyReportView
            var rptSerialized = new SerializedObject(monthlyReportView);
            rptSerialized.FindProperty("_panelRoot").objectReferenceValue = reportModalGo;
            rptSerialized.FindProperty("_titleLabel").objectReferenceValue = reportTitle;
            rptSerialized.FindProperty("_openingCashLabel").objectReferenceValue = rptOpen;
            rptSerialized.FindProperty("_revenueLabel").objectReferenceValue = rptRev;
            rptSerialized.FindProperty("_expensesLabel").objectReferenceValue = rptExp;
            rptSerialized.FindProperty("_netProfitLabel").objectReferenceValue = rptNet;
            rptSerialized.FindProperty("_closingCashLabel").objectReferenceValue = rptClose;
            rptSerialized.FindProperty("_dismissButton").objectReferenceValue = dismissBtn;
            rptSerialized.ApplyModifiedPropertiesWithoutUndo();

            // Set monthly report modal initially inactive
            reportModalGo.SetActive(false);

            // 5d. Event Modal
            var eventModalGo = CreateUiObject("EventModal", canvasGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var eventModalBackdrop = eventModalGo.AddComponent<Image>();
            eventModalBackdrop.color = new Color(0, 0, 0, 0.82f); // Deep dim overlay
            var eventModalView = eventModalGo.AddComponent<EventModalView>();

            // Event card panel
            var eventCardGo = CreateUiObject("EventCard", eventModalGo.transform, new Vector2(0.2f, 0.1f), new Vector2(0.8f, 0.9f), Vector2.zero, Vector2.zero);
            var eventCardImage = eventCardGo.AddComponent<Image>();
            eventCardImage.color = new Color(0.067f, 0.098f, 0.157f); // Dark navy #111827

            // Category tag
            var categoryLabelGo = CreateUiObject("CategoryLabel", eventCardGo.transform, new Vector2(0.05f, 0.88f), new Vector2(0.95f, 0.95f), Vector2.zero, Vector2.zero);
            var categoryLabel = categoryLabelGo.AddComponent<TextMeshProUGUI>();
            categoryLabel.text = "CORPORATE";
            categoryLabel.fontSize = 13;
            categoryLabel.fontStyle = FontStyles.Bold;
            categoryLabel.color = new Color(0.96f, 0.62f, 0.04f); // Gold #F59E0B
            categoryLabel.alignment = TextAlignmentOptions.MidlineLeft;

            // Event title
            var eventTitleGo = CreateUiObject("EventTitleLabel", eventCardGo.transform, new Vector2(0.05f, 0.76f), new Vector2(0.95f, 0.88f), Vector2.zero, Vector2.zero);
            var eventTitle = eventTitleGo.AddComponent<TextMeshProUGUI>();
            eventTitle.text = "Event Title";
            eventTitle.fontSize = 28;
            eventTitle.fontStyle = FontStyles.Bold;
            eventTitle.color = new Color(0.95f, 0.96f, 0.98f);
            eventTitle.alignment = TextAlignmentOptions.MidlineLeft;

            // Event description
            var eventDescGo = CreateUiObject("EventDescriptionLabel", eventCardGo.transform, new Vector2(0.05f, 0.52f), new Vector2(0.95f, 0.76f), Vector2.zero, Vector2.zero);
            var eventDesc = eventDescGo.AddComponent<TextMeshProUGUI>();
            eventDesc.text = "Event narrative description goes here.";
            eventDesc.fontSize = 16;
            eventDesc.color = new Color(0.75f, 0.80f, 0.88f);
            eventDesc.alignment = TextAlignmentOptions.TopLeft;

            // Divider line (cosmetic)
            var dividerGo = CreateUiObject("Divider", eventCardGo.transform, new Vector2(0.05f, 0.50f), new Vector2(0.95f, 0.515f), Vector2.zero, Vector2.zero);
            var dividerImg = dividerGo.AddComponent<Image>();
            dividerImg.color = new Color(0.96f, 0.62f, 0.04f, 0.4f);

            // Choices container (vertical stack layout)
            var choicesContainerGo = CreateUiObject("ChoicesContainer", eventCardGo.transform, new Vector2(0.03f, 0.10f), new Vector2(0.97f, 0.50f), Vector2.zero, Vector2.zero);
            var choicesContainerRect = choicesContainerGo.GetComponent<RectTransform>();
            var vertLayout = choicesContainerGo.AddComponent<VerticalLayoutGroup>();
            vertLayout.spacing = 8;
            vertLayout.childAlignment = TextAnchor.UpperCenter;
            vertLayout.childControlWidth = true;
            vertLayout.childControlHeight = false;
            vertLayout.childForceExpandWidth = true;
            vertLayout.childForceExpandHeight = false;
            vertLayout.padding = new RectOffset(0, 0, 0, 0);

            // Status label (error / info messages)
            var statusLabelGo = CreateUiObject("StatusLabel", eventCardGo.transform, new Vector2(0.05f, 0.02f), new Vector2(0.95f, 0.09f), Vector2.zero, Vector2.zero);
            var statusLabel = statusLabelGo.AddComponent<TextMeshProUGUI>();
            statusLabel.fontSize = 14;
            statusLabel.color = new Color(0.96f, 0.26f, 0.21f); // Red
            statusLabel.alignment = TextAlignmentOptions.Center;
            statusLabelGo.SetActive(false);

            // Build the ChoiceButton prefab as a plain GameObject (not a real prefab asset — just wired inline)
            // The EventModalView will pool buttons manually; we create a template object here and disable it
            var choiceButtonTemplate = CreateUiObject("ChoiceButtonTemplate", eventCardGo.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var choiceButtonImg = choiceButtonTemplate.AddComponent<Image>();
            choiceButtonImg.color = new Color(0.14f, 0.19f, 0.28f); // Slate card
            var choiceButtonComp = choiceButtonTemplate.AddComponent<Button>();

            var choiceTitleGo = CreateUiObject("ChoiceTitle", choiceButtonTemplate.transform, new Vector2(0.03f, 0.55f), new Vector2(0.97f, 0.95f), Vector2.zero, Vector2.zero);
            var choiceTitle = choiceTitleGo.AddComponent<TextMeshProUGUI>();
            choiceTitle.fontSize = 17;
            choiceTitle.fontStyle = FontStyles.Bold;
            choiceTitle.color = new Color(0.95f, 0.96f, 0.98f);
            choiceTitle.alignment = TextAlignmentOptions.MidlineLeft;

            var choiceDescGo = CreateUiObject("ChoiceDescription", choiceButtonTemplate.transform, new Vector2(0.03f, 0.35f), new Vector2(0.97f, 0.55f), Vector2.zero, Vector2.zero);
            var choiceDesc = choiceDescGo.AddComponent<TextMeshProUGUI>();
            choiceDesc.fontSize = 14;
            choiceDesc.color = new Color(0.75f, 0.80f, 0.88f);
            choiceDesc.alignment = TextAlignmentOptions.MidlineLeft;

            var choiceReqGo = CreateUiObject("RequirementsText", choiceButtonTemplate.transform, new Vector2(0.03f, 0.18f), new Vector2(0.97f, 0.35f), Vector2.zero, Vector2.zero);
            var choiceReq = choiceReqGo.AddComponent<TextMeshProUGUI>();
            choiceReq.fontSize = 13;
            choiceReq.color = new Color(0.96f, 0.26f, 0.21f);
            choiceReq.alignment = TextAlignmentOptions.MidlineLeft;

            var choiceConsGo = CreateUiObject("ConsequencesText", choiceButtonTemplate.transform, new Vector2(0.03f, 0.02f), new Vector2(0.97f, 0.18f), Vector2.zero, Vector2.zero);
            var choiceCons = choiceConsGo.AddComponent<TextMeshProUGUI>();
            choiceCons.fontSize = 13;
            choiceCons.color = new Color(0.75f, 0.80f, 0.88f);
            choiceCons.alignment = TextAlignmentOptions.MidlineLeft;

            // Wire ChoiceButtonView on template
            var choiceButtonView = choiceButtonTemplate.AddComponent<ChoiceButtonView>();
            var choiceViewSer = new SerializedObject(choiceButtonView);
            choiceViewSer.FindProperty("_titleText").objectReferenceValue = choiceTitle;
            choiceViewSer.FindProperty("_descriptionText").objectReferenceValue = choiceDesc;
            choiceViewSer.FindProperty("_requirementsText").objectReferenceValue = choiceReq;
            choiceViewSer.FindProperty("_consequencesText").objectReferenceValue = choiceCons;
            choiceViewSer.FindProperty("_button").objectReferenceValue = choiceButtonComp;
            choiceViewSer.ApplyModifiedPropertiesWithoutUndo();

            // Disable template — EventModalView spawns copies of this
            choiceButtonTemplate.SetActive(false);

            // Wire EventModalView serialized fields
            var eventModalSer = new SerializedObject(eventModalView);
            eventModalSer.FindProperty("_panelRoot").objectReferenceValue = eventModalGo;
            eventModalSer.FindProperty("_titleLabel").objectReferenceValue = eventTitle;
            eventModalSer.FindProperty("_categoryLabel").objectReferenceValue = categoryLabel;
            eventModalSer.FindProperty("_descriptionLabel").objectReferenceValue = eventDesc;
            eventModalSer.FindProperty("_choicesContainer").objectReferenceValue = choicesContainerGo.transform;
            eventModalSer.FindProperty("_choiceButtonPrefab").objectReferenceValue = choiceButtonTemplate;
            eventModalSer.FindProperty("_statusLabel").objectReferenceValue = statusLabel;
            eventModalSer.ApplyModifiedPropertiesWithoutUndo();

            // Set event modal initially inactive
            eventModalGo.SetActive(false);

            // 6. SimulationBootstrapper
            var bootstrapperGo = new GameObject("SimulationBootstrapper");
            var bootstrapper = bootstrapperGo.AddComponent<SimulationBootstrapper>();
            var bootSerialized = new SerializedObject(bootstrapper);
            bootSerialized.FindProperty("_topBarView").objectReferenceValue = topBarView;
            bootSerialized.FindProperty("_financialOverviewView").objectReferenceValue = finOverviewView;
            bootSerialized.FindProperty("_monthlyReportView").objectReferenceValue = monthlyReportView;
            bootSerialized.FindProperty("_eventModalView").objectReferenceValue = eventModalView;
            bootSerialized.ApplyModifiedPropertiesWithoutUndo();

            // 7. Save Scene
            string dir = "Assets/_Project/Scenes/Game";
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            string scenePath = Path.Combine(dir, "Game.unity").Replace('\\', '/');
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"[GameSceneBuilder] Successfully created and saved scene at: {scenePath}");

            // 8. Register in EditorBuildSettings
            var scenes = EditorBuildSettings.scenes;
            bool alreadyRegistered = false;
            foreach (var s in scenes)
            {
                if (s.path == scenePath)
                {
                    alreadyRegistered = true;
                    break;
                }
            }

            if (!alreadyRegistered)
            {
                var newScenes = new EditorBuildSettingsScene[scenes.Length + 1];
                scenes.CopyTo(newScenes, 0);
                newScenes[scenes.Length] = new EditorBuildSettingsScene(scenePath, true);
                EditorBuildSettings.scenes = newScenes;
                Debug.Log($"[GameSceneBuilder] Registered scene in EditorBuildSettings: {scenePath}");
            }
        }

        private static GameObject CreateUiObject(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var rect = go.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            return go;
        }
    }
}
