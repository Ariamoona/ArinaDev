using UnityAdsIntegration;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(AdService))]
public class AdServiceEditor : Editor
{
    private AdService _adService;
    private bool _showTestButtons = true;

    private void OnEnable()
    {
        _adService = (AdService)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(10);

        _showTestButtons = EditorGUILayout.Foldout(_showTestButtons, "Test Controls (Editor Only)", true);

        if (_showTestButtons)
        {
            EditorGUILayout.HelpBox(
                "Тестирование в редакторе:\n" +
                "- Unity Ads отображает тестовую рекламу автоматически при включенном тестовом режиме\n" +
                "- Баннер будет показан внизу экрана\n" +
                "- На реальном устройстве нужно указать Game ID в настройках",
                MessageType.Info);

            EditorGUILayout.Space(5);

            GUILayout.Label("Rewarded Ads", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Rewarded Ad"))
            {
                _adService.LoadRewardedAd();
                Debug.Log("[Editor] Запрос на загрузку rewarded ad отправлен");
            }
            if (GUILayout.Button("Show Rewarded Ad"))
            {
                _adService.ShowRewardedAd(
                    onRewardGranted: () => Debug.Log("[Editor] Reward granted!"),
                    onError: (error) => Debug.LogError($"[Editor] Ad error: {error}")
                );
            }
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(5);

            GUILayout.Label("Banner Ads", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Show Banner"))
            {
                _adService.ShowBanner();
                Debug.Log("[Editor] Показ баннера");
            }
            if (GUILayout.Button("Hide Banner"))
            {
                _adService.HideBanner();
                Debug.Log("[Editor] Скрытие баннера");
            }
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.Space(10);

        EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.Toggle("SDK Initialized", _adService.IsInitialized);
        EditorGUILayout.Toggle("Rewarded Ad Ready", _adService.IsRewardedAdReady);
        EditorGUI.EndDisabledGroup();
    }
}