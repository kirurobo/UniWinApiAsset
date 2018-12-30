using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Kirurobo
{
    /// <summary>
    /// WindowControllerの設定を単純にオン／オフするサンプル
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
            // 同じゲームオブジェクトに WindowController をアタッチしておくこと
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

            if (transparentToggle)
            {
                transparentToggle.isOn = windowController.isTransparent;
                transparentToggle.onValueChanged.AddListener(val => windowController.isTransparent = val);
            }
            if (topmostToggle)
            {
                topmostToggle.isOn = windowController.isTopmost;
                topmostToggle.onValueChanged.AddListener(val => windowController.isTopmost = val);
            }
            if (maximizedToggle)
            {
                maximizedToggle.isOn = windowController.isMaximized;
                maximizedToggle.onValueChanged.AddListener(val => windowController.isMaximized = val);
            }
            if (minimizedToggle)
            {
                minimizedToggle.isOn = windowController.isMinimized;
                minimizedToggle.onValueChanged.AddListener(val => windowController.isMinimized = val);
            }

            if (enableFileDropToggle)
            {
                enableFileDropToggle.isOn = windowController.enableFileDrop;
                enableFileDropToggle.onValueChanged.AddListener(val => windowController.enableFileDrop = val);
            }
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}
