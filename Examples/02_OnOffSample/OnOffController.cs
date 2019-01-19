using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kirurobo
{
    /// <summary>
    /// WindowControllerの設定をToggleでオン／オフするサンプル
    /// </summary>
    public class OnOffController : MonoBehaviour
    {
        WindowController windowController;

        public Toggle transparentToggle;
        public Toggle topmostToggle;
        public Toggle maximizedToggle;
        public Toggle minimizedToggle;
        public Toggle enableFileDropToggle;
        public Text droppedFilesText;

        // Use this for initialization
        void Start()
        {
            // 同じゲームオブジェクトに WindowController がアタッチされているとして、取得
            windowController = GetComponent<WindowController>();

            // ファイルドロップ時の処理
            windowController.OnFilesDropped += (string[] files) =>
            {
                if (droppedFilesText)
                {
                    // ドロップされたファイルのパスを表示
                    droppedFilesText.text = string.Join("\n", files);
                }
            };

            // Toggleのチェック状態を、現在の状態に合わせる
            UpdateUI();

            // Toggleを操作された際にはウィンドウに反映されるようにする
            if (transparentToggle)
            {
                transparentToggle.onValueChanged.AddListener(val => windowController.isTransparent = val);
            }
            if (topmostToggle)
            {
                topmostToggle.onValueChanged.AddListener(val => windowController.isTopmost = val);
            }
            if (maximizedToggle)
            {
                maximizedToggle.onValueChanged.AddListener(val => windowController.isMaximized = val);
            }
            if (minimizedToggle)
            {
                minimizedToggle.onValueChanged.AddListener(val => windowController.isMinimized = val);
            }

            if (enableFileDropToggle)
            {
                enableFileDropToggle.onValueChanged.AddListener(val => windowController.enableFileDrop = val);
            }
        }

        /// <summary>
        /// 現在のWindowControllerの設定をUIに反映
        /// </summary>
        private void UpdateUI()
        {
            if (transparentToggle)
            {
                transparentToggle.isOn = windowController.isTransparent;
            }
            if (topmostToggle)
            {
                topmostToggle.isOn = windowController.isTopmost;
            }
            if (maximizedToggle)
            {
                maximizedToggle.isOn = windowController.isMaximized;
            }
            if (minimizedToggle)
            {
                minimizedToggle.isOn = windowController.isMinimized;
            }
            if (enableFileDropToggle)
            {
                enableFileDropToggle.isOn = windowController.enableFileDrop;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void Update()
        {
            // Quit or stop playing when pressed [ESC]
            if (Input.GetKey(KeyCode.Escape))
            {
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
            }
        }
    }
}
