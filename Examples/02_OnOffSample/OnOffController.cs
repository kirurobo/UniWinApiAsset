using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public Dropdown transparentTypeDropdown;
        public Toggle transparentToggle;
        public Toggle topmostToggle;
        public Toggle maximizedToggle;
        public Toggle minimizedToggle;
        public Toggle enableFileDropToggle;
        public Text droppedFilesText;

        // ドロップダウンの選択肢順に合わせる
        private Dictionary<int, UniWinApi.TransparentTypes> _transparentTypes =
            new Dictionary<int, UniWinApi.TransparentTypes>()
            {
                {0, UniWinApi.TransparentTypes.None},
                {1, UniWinApi.TransparentTypes.Alpha},
                {2, UniWinApi.TransparentTypes.ColorKey},
            };
        
        // Use this for initialization
        void Start()
        {
            // 同じゲームオブジェクトに WindowController がアタッチされているとして、取得
            windowController = GetComponent<WindowController>();

            //// Allow file drop from lower privilege windows.
            //windowController.allowDropFromLowerPrivilege = true;

            // ファイルドロップ時の処理
            windowController.OnFilesDropped += (string[] files) =>
            {
                if (droppedFilesText)
                {
                    // ドロップされたファイルのパスを表示
                    droppedFilesText.text = string.Join("\n", files);
                }
            };

            // ウィンドウ状態が変化した際にはUIも一致するよう更新
            windowController.OnStateChanged += () => {
                UpdateUI();
            };

            // Toggleのチェック状態を、現在の状態に合わせる
            UpdateUI();

            // Toggleを操作された際にはウィンドウに反映されるようにする
            if (transparentTypeDropdown)
            {
                // 初期値を選択
                transparentTypeDropdown.value =
                    _transparentTypes.First(d => d.Value == windowController.transparentType).Key;
                
                transparentTypeDropdown.onValueChanged.AddListener(val => windowController.SetTransparentType(_transparentTypes[val]));
            }
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
            if (Input.GetKeyDown(KeyCode.Escape))
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
