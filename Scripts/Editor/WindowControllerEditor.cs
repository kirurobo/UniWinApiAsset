using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;

namespace Kirurobo
{
    /// <summary>
    /// WindowControllerのためのUnityエディタカスタマイズ
    /// </summary>
    [CustomEditor(typeof(WindowController))]
    public class WindowControllerEditor : Editor
    {
        private EditorWindow gameViewWindow;

        private bool isWarningDismissed = false;

        void OnEnable()
        {
            LoadSettings();
        }

        void OnDisable()
        {
            SaveSettings();
        }

        private void LoadSettings()
        {
            isWarningDismissed = EditorUserSettings.GetConfigValue("WindowController_IS_WARNING DISMISSED") == "1";
        }

        private void SaveSettings()
        {
            EditorUserSettings.SetConfigValue("WindowController_IS_WARNING DISMISSED", isWarningDismissed ? "1" : "0");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            EditorGUILayout.Space();

            bool enableValidation = EditorGUILayout.Foldout(!isWarningDismissed, "Player Settings validation");

            // チェックするかどうかを記憶
            if (enableValidation == isWarningDismissed)
            {
                isWarningDismissed = !enableValidation;
            }

            // 推奨設定のチェック
            //if (!isWarningDismissed)
            if (enableValidation)
            {
                // Player Settings をチェックし、非推奨があれば警告メッセージを得る
                string[] warnings = ValidatePlayerSettings();

                //  チェックに引っかかればボタンを表示
                if (warnings.Length > 0)
                {

                    // 枠を作成
                    //EditorGUILayout.BeginVertical(GUI.skin.box);
                    //GUILayout.Label("Player Settings validation");

                    // 警告メッセージを表示
                    foreach (var message in warnings)
                    {
                        EditorGUILayout.HelpBox(message, MessageType.Warning);
                    }

                    // 推奨設定をすべて適用するボタン
                    GUI.backgroundColor = Color.green;
                    if (
                        GUILayout.Button(
                            "✔ Apply all recommended settings",
                            GUILayout.MinHeight(20f)
                        ))
                    {
                        ApplyRecommendedSettings();
                    }

                    // チェックを今後無視するボタン
                    GUI.backgroundColor = Color.red;
                    if (
                        GUILayout.Button(
                            "✘ Mute this validation",
                            GUILayout.MinHeight(20f)
                        ))
                    {
                        isWarningDismissed = true;
                        //SaveSettings();
                    }

                    //EditorGUILayout.EndVertical();
                }
                else
                {
                    GUI.color = Color.green;
                    GUILayout.Label("OK!");
                }
            }
        }

        /// <summary>
        /// Player設定を確認し、推奨設定になっていない項目のメッセージ一覧を得る
        /// </summary>
        /// <returns></returns>
        private string[] ValidatePlayerSettings()
        {
            // 警告メッセージのリスト
            List<string> warnings = new List<string>();

            if (!PlayerSettings.runInBackground)
            {
                warnings.Add("'Run in background' is highly recommended.");
            }

            if (!PlayerSettings.resizableWindow)
            {
                warnings.Add("'Resizable window' is recommended.");
            }

#if UNITY_2018_1_OR_NEWER
            // Unity 2018 からはフルスクリーン指定の仕様が変わった
            if (PlayerSettings.fullScreenMode != FullScreenMode.Windowed)
            {
                warnings.Add("Chose 'Windowed' in 'Fullscreen Mode'.");
            }
#else
            if (PlayerSettings.defaultIsFullScreen)
            {
                warnings.Add("'Default is full screen' is not recommended.");
            }
#endif

            // ↓Unity 2019.1.6未満だと useFlipModelSwapchain は無いはず
            //    なので除外のため書き連ねてあるが、ここまでサポートしなくて良い気もする。
#if UNITY_2019_1_6
#elif UNITY_2019_1_5
#elif UNITY_2019_1_4
#elif UNITY_2019_1_3
#elif UNITY_2019_1_2
#elif UNITY_2019_1_1
#elif UNITY_2019_1_0
#elif UNITY_2019_1_OR_NEWER
            // Unity 2019.1.7 以降であれば、Player 設定 の Use DXGI Flip... 無効化を推奨
            if (PlayerSettings.useFlipModelSwapchain)
            {
                warnings.Add("Disable 'Use DXGI Flip Mode Swapchain' to make the window transparent.");
            }
#endif

            return warnings.ToArray();
        }

        /// <summary>
        /// 推奨設定を一括で適用
        /// </summary>
        private void ApplyRecommendedSettings()
        {
#if UNITY_2018_1_OR_NEWER
            PlayerSettings.fullScreenMode = FullScreenMode.Windowed;
#else
            PlayerSettings.defaultIsFullScreen = false;
#endif
            PlayerSettings.runInBackground = true;
            PlayerSettings.resizableWindow = true;

#if UNITY_2019_1_6
#elif UNITY_2019_1_5
#elif UNITY_2019_1_4
#elif UNITY_2019_1_3
#elif UNITY_2019_1_2
#elif UNITY_2019_1_1
#elif UNITY_2019_1_0
#elif UNITY_2019_1_OR_NEWER
            PlayerSettings.useFlipModelSwapchain = false;
#endif

        }

        // 参考 http://baba-s.hatenablog.com/entry/2017/09/17/135018
        /// <summary>
        /// ゲームビューのEditorWindowを取得
        /// </summary>
        /// <returns></returns>
        public static EditorWindow GetGameView()
        {
            var assembly = typeof(EditorWindow).Assembly;
            var type = assembly.GetType("UnityEditor.GameView");
            var gameView = EditorWindow.GetWindow(type);
            return gameView;
        }
    }

    [CustomPropertyDrawer(typeof(ReadOnlyAttribute))]
    public class WindowControllerReadOnlyDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = true;
        }
    }

    /// <summary>
    /// Set to readonly during playing
    /// Reference: http://ponkotsu-hiyorin.hateblo.jp/entry/2015/10/20/003042
    /// Reference: https://forum.unity.com/threads/c-class-property-with-reflection-in-propertydrawer-not-saving-to-prefab.473942/
    /// </summary>
    [CustomPropertyDrawer(typeof(BoolPropertyAttribute))]
    public class WindowControllerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            //base.OnGUI(position, property, label);

            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if ((property.type == "bool") && (property.name[0] == '_'))
                {
                    Object obj = property.serializedObject.targetObject;
                    string propertyName = property.name.Substring(1);
                    PropertyInfo info = obj.GetType().GetProperty(propertyName);
                    MethodInfo getMethod = default(MethodInfo);
                    MethodInfo setMethod = default(MethodInfo);
                    if (info.CanRead) { getMethod = info.GetGetMethod(); }
                    if (info.CanWrite) { setMethod = info.GetSetMethod(); }

                    bool oldValue = property.boolValue;
                    if (getMethod != null)
                    {
                        oldValue = (bool)getMethod.Invoke(obj, null);
                    }
                    GUI.enabled = (setMethod != null);
                    EditorGUI.PropertyField(position, property, label, true);
                    GUI.enabled = true;
                    bool newValue = property.boolValue;
                    if ((setMethod != null) && (oldValue != newValue))
                    {
                        setMethod.Invoke(obj, new[] { (object)newValue });
                    }
                }
                else
                {
                    // Readonly
                    GUI.enabled = false;
                    EditorGUI.PropertyField(position, property, label, true);
                    GUI.enabled = true;
                }
            }
            else
            {
                // Default value
                EditorGUI.PropertyField(position, property, label, true);
            }

        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }
    }
}