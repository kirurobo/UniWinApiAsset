/**
 * UniWinApi sample
 * 
 * Author: Kirurobo http://twitter.com/kirurobo
 * License: CC0 https://creativecommons.org/publicdomain/zero/1.0/
 */

using System.Collections;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
#endif

namespace Kirurobo
{

    /// <summary>
    /// Set editable the bool property
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class BoolPropertyAttribute : PropertyAttribute { }

    /// <summary>
    /// Set the attribute as readonly
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class ReadOnlyAttribute : PropertyAttribute { }


    /// <summary>
    /// ウィンドウ操作をとりまとめるクラス
    /// </summary>
    public class WindowController : MonoBehaviour
    {

        /// <summary>
        /// Window controller
        /// </summary>
        public UniWinApi uniWin;

        /// <summary>
        /// 操作を透過する状態か
        /// </summary>
        public bool isClickThrough
        {
            get { return _isClickThrough; }
        }
        private bool _isClickThrough = true;

        /// <summary>
        /// Is this window transparent
        /// </summary>
        public bool isTransparent
        {
            get { return _isTransparent; }
            set { SetTransparent(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set transparent on startup")]
        private bool _isTransparent = false;

        /// <summary>
        /// Is this window minimized
        /// </summary>
        public bool isTopmost
        {
            get { return ((uniWin == null) ? _isTopmost : _isTopmost = uniWin.IsTopmost); }
            set { SetTopmost(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set topmost on startup")]
        private bool _isTopmost = false;

        /// <summary>
        /// Is this window maximized
        /// </summary>
        public bool isMaximized
        {
            get { return ((uniWin == null) ? _isMaximized : _isMaximized = uniWin.IsMaximized); }
            set { SetMaximized(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set maximized on startup")]
        private bool _isMaximized = false;

        /// <summary>
        /// Is this window minimized
        /// </summary>
        public bool isMinimized
        {
            get { return ((uniWin == null) ? _isMinimized : _isMinimized = uniWin.IsMinimized); }
            set { SetMinimized(value); }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set minimized on startup")]
        private bool _isMinimized = false;

        /// <summary>
        /// ファイルドロップを有効にするならば最初からtrueにしておく
        /// </summary>
        public bool enableFileDrop
        {
            get { return _enableFileDrop; }
            set
            {
                if (value) { BeginFileDrop(); }
                else { EndFileDrop(); }
            }
        }
        [SerializeField, BoolProperty, Tooltip("Check to set enable file-drop on startup")]
        private bool _enableFileDrop = false;

        /// <summary>
        /// マウスドラッグでウィンドウを移動させるか
        /// </summary>
        public bool enableDragMove = true;


        // カメラの背景をアルファゼロの黒に置き換えるため、本来の背景を保存しておく変数
        private CameraClearFlags originalCameraClearFlags;
        private Color originalCameraBackground;

        /// <summary>
        /// Is the mouse pointer on an opaque pixel
        /// </summary>
        //[SerializeField, Tooltip("Is the mouse pointer on an opaque pixel? (Read only)")]
        private bool onOpaquePixel = true;

        /// <summary>
        /// The cut off threshold of alpha value.
        /// </summary>
        private float opaqueThreshold = 0.1f;

        /// <summary>
        /// Pixel color under the mouse pointer. (Read only)
        /// </summary>
        [ReadOnly, Tooltip("Pixel color under the mouse pointer. (Read only)")]
        public Color pickedColor;


        private bool isDragging = false;
        private Vector2 lastMousePosition;

        /// <summary>
        /// 最後のドラッグはマウスによるものか、タッチによるものか
        /// </summary>
        private bool wasUsingMouse;

        /// <summary>
        /// 現在対象としているウィンドウが自分自身らしいと確認できたらtrueとする
        /// </summary>
        private bool isWindowChecked = false;

        /// <summary>
        /// カメラのインスタンス
        /// </summary>
        private Camera currentCamera;


        /// <summary>
        /// ファイルドロップ時のイベントハンドラー。 UniWinApiの OnFilesDropped にそのまま渡す。
        /// </summary>
        public event UniWinApi.FilesDropped OnFilesDropped
        {
            add { if (uniWin != null) { uniWin.OnFilesDropped += value; } }
            remove { if (uniWin != null) { uniWin.OnFilesDropped -= value; } }
        }

        /// <summary>
        /// ウィンドウ状態が変化したときに発生するイベント
        /// </summary>
        public event OnStateChangedDelegate OnStateChanged;
        public delegate void OnStateChangedDelegate();

        /// <summary>
        /// 表示されたテクスチャ
        /// </summary>
        private Texture2D colorPickerTexture = null;


        // Use this for initialization
        void Awake()
        {
            if (!currentCamera)
            {
                // メインカメラを探す
                currentCamera = Camera.main;

                // もしメインカメラが見つからなければ、Findで探す
                if (!currentCamera)
                {
                    currentCamera = FindObjectOfType<Camera>();
                }
            }

            // カメラの元の背景を記憶
            if (currentCamera)
            {
                originalCameraClearFlags = currentCamera.clearFlags;
                originalCameraBackground = currentCamera.backgroundColor;

            }

            // マウス下描画色抽出用テクスチャを準備
            colorPickerTexture = new Texture2D(1, 1, TextureFormat.ARGB32, false);

#if (UNITY_WIN || UNITY_STANDALONE_WIN)
            // ウィンドウ制御用のインスタンス作成
            uniWin = new UniWinApi();

            // 自分のウィンドウを取得
            FindMyWindow();
#endif
#if UNITY_EDITOR
            // エディタのウィンドウ配置が変化した際の呼び出し
            EditorApplicationUtility.windowsReordered += () => {
                this.isWindowChecked = false;   // ウィンドウが不確かであるとする
                //Debug.Log("Editor windows reordered");
            };
#endif
        }

        void Start()
        {
            // マウスカーソル直下の色を取得するコルーチンを開始
            StartCoroutine(PickColorCoroutine());
        }

        void OnDestroy()
        {
            if (uniWin != null) {
                uniWin.Dispose();
            }
        }

        // Update is called once per frame
        void Update()
        {
            // 自ウィンドウ取得状態が不確かなら探しなおす
            //  マウス押下が取れるのはすなわちフォーカスがあるとき
            if (Input.anyKey)
            {
                UpdateWindow();
            }

            // キー、マウス操作の下ウィンドウへの透過状態を更新
            UpdateClickThrough();

            // マウスドラッグでウィンドウ移動
            DragMove();

            // ウィンドウ枠が復活している場合があるので監視するため、呼ぶ
            if (uniWin != null) {
                uniWin.Update();
            }
        }

        /// <summary>
        /// ウィンドウ状態が変わったときに呼ぶイベントを処理
        /// </summary>
        private void StateChangedEvent()
        {
            if (OnStateChanged != null)
            {
                OnStateChanged();
            }
        }

        /// <summary>
        /// 最大化時以外なら、マウスドラッグによってウィンドウを移動
        /// </summary>
        void DragMove()
        {
            if (uniWin == null) return;

            // ドラッグでの移動が無効化されていた場合
            if (!enableDragMove)
            {
                isDragging = false;
                return;
            }

            // 最大化状態ならウィンドウ移動は行わないようにする
            bool isFullScreen = uniWin.IsMaximized;

#if !UNITY_EDITOR
            // フルスクリーンならウィンドウ移動は行わない
            //  エディタだと true になってしまうようなので、エディタ以外でのみ確認
            if (Screen.fullScreen) isFullScreen = true;
#endif
            if (isFullScreen)
            {
                isDragging = false;
                return;
            }

            // マウスドラッグでウィンドウ移動
            if (Input.GetMouseButtonDown(0))
            {
                lastMousePosition = UniWinApi.GetCursorPosition();
                isDragging = true;
                wasUsingMouse = true;
            }
            bool touching = false;
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                lastMousePosition = touch.rawPosition;
                isDragging = true;
                wasUsingMouse = false;
                touching = true;
            }
            if (wasUsingMouse && !Input.GetMouseButton(0))
            {
                isDragging = false;
            }
            else if (!wasUsingMouse && !touching)
            {
                isDragging = false;
            }
            if (isDragging)
            {
                Vector2 mousePos;
                if (wasUsingMouse)
                {
                    mousePos = UniWinApi.GetCursorPosition();
                }
                else
                {
                    Touch touch = Input.GetTouch(0);
                    mousePos = touch.rawPosition;
                }
                Vector2 delta = mousePos - lastMousePosition;
                lastMousePosition = mousePos;

                Vector2 windowPosition = uniWin.GetPosition();  // 現在のウィンドウ位置を取得
                windowPosition += delta; // ウィンドウ位置に上下左右移動分を加える
                uniWin.SetPosition(windowPosition);   // ウィンドウ位置を設定
            }
        }

        /// <summary>
        /// 画素の色を基に操作受付を切り替える
        /// </summary>
        void UpdateClickThrough()
        {
            if (_isClickThrough)
            {
                if (onOpaquePixel)
                {
                    if (uniWin != null) uniWin.EnableClickThrough(false);
                    _isClickThrough = false;
                }
            }
            else
            {
                if (isTransparent && !onOpaquePixel && !isDragging)
                {
                    if (uniWin != null) uniWin.EnableClickThrough(true);
                    _isClickThrough = true;
                }
            }
        }

        /// <summary>
        /// OnPostRenderではGUI描画前になってしまうため、コルーチンを用意
        /// </summary>
        /// <returns></returns>
        private IEnumerator PickColorCoroutine()
        {
            while (Application.isPlaying)
            {
                yield return new WaitForEndOfFrame();
                MyPostRender(currentCamera);
            }
            yield return null;
        }

        /// <summary>
        /// マウス下の画素が透明かどうかを確認
        /// </summary>
        /// <param name="cam"></param>
        void MyPostRender(Camera cam)
        {
            // カメラが不明ならば何もしない
            if (!cam) return;

            Vector2 mousePos;
            if (Input.touchCount > 0)
            {
                mousePos = Input.touches[0].position;
            }
            else
            {
                mousePos = Input.mousePosition;
            }
            Rect camRect = cam.pixelRect;

            //// コルーチン & WaitForEndOfFrame ではなく、OnPostRenderで呼ぶならば、MSAAによって上下反転しないといけない？
            //if (QualitySettings.antiAliasing > 1) mousePos.y = camRect.height - mousePos.y;

            if (camRect.Contains(mousePos))
            {
                try
                {
                    // Reference http://tsubakit1.hateblo.jp/entry/20131203/1386000440
                    colorPickerTexture.ReadPixels(new Rect(mousePos, Vector2.one), 0, 0);
                    Color color = colorPickerTexture.GetPixel(0, 0);
                    pickedColor = color;
                    onOpaquePixel = (color.a >= opaqueThreshold);  // αがしきい値以上ならば不透過とする
                }
                catch (System.Exception ex)
                {
                    // 稀に範囲外になってしまうよう
                    Debug.LogError(ex.Message);
                    onOpaquePixel = false;
                }
            }
            else
            {
                onOpaquePixel = false;
            }
        }

        /// <summary>
        /// 自分のウィンドウハンドルを見つける
        /// </summary>
        private void FindMyWindow()
        {
            // ウィンドウが確かではないとしておく
            isWindowChecked = false;

            // 現在このウィンドウがアクティブでなければ、取得はやめておく
            if (!Application.isFocused) return;

            // 今アクティブなウィンドウを取得
            var window = UniWinApi.FindWindow();
            if (window == null) return;

            // 見つかったウィンドウを利用開始
            uniWin.SetWindow(window);

            // 初期状態を反映
            SetTopmost(_isTopmost);
            SetMaximized(_isMaximized);
            SetMinimized(_isMinimized);
            SetTransparent(_isTransparent);
            if (_enableFileDrop) BeginFileDrop();
        }

        /// <summary>
        /// 自分のウィンドウハンドルが不確かならば探しなおす
        /// </summary>
        private void UpdateWindow()
        {
            if (uniWin == null) return;

            // もしウィンドウハンドル取得に失敗していたら再取得
            if (!uniWin.IsActive)
            {
                //Debug.Log("Window is not active");
                FindMyWindow();
            }
            else if (!isWindowChecked)
            {
                // 自分自身のウィンドウか未確認の場合

                // 今アクティブなウィンドウが自分自身かをチェック
                if (uniWin.CheckActiveWindow())
                {
                    isWindowChecked = true; // どうやら正しくウィンドウをつかめているよう
                }
                else
                {
                    // ウィンドウが違っているようなので、もう一度アクティブウィンドウを取得
                    uniWin.Reset();
                    uniWin.Dispose();
                    uniWin = new UniWinApi();
                    FindMyWindow();
                }
            }
        }

        /// <summary>
        /// ウィンドウへのフォーカスが変化したときに呼ばれる
        /// </summary>
        /// <param name="focus"></param>
        private void OnApplicationFocus(bool focus)
        {
            //Debug.Log("Focus:" + focus);

            if (focus)
            {
                UpdateWindow();
            }

        }

        /// <summary>
        /// ウィンドウ透過状態になった際、自動的に背景を透明単色に変更する
        /// </summary>
        /// <param name="isTransparent"></param>
        void SetCameraBackground(bool isTransparent)
        {
            // カメラが特定できていなければ何もしない
            if (!currentCamera) return;

            if (isTransparent)
            {
                currentCamera.clearFlags = CameraClearFlags.SolidColor;
                currentCamera.backgroundColor = Color.clear;
            }
            else
            {
                currentCamera.clearFlags = originalCameraClearFlags;
                currentCamera.backgroundColor = originalCameraBackground;
            }
        }

        /// <summary>
        /// 透明化状態を切替
        /// </summary>
        /// <param name="transparent"></param>
        public void SetTransparent(bool transparent)
        {
            //if (_isTransparent == transparent) return;

            _isTransparent = transparent;
            SetCameraBackground(transparent);

            if (uniWin != null)
            {
                uniWin.EnableTransparent(transparent);
            }
            UpdateClickThrough();
            StateChangedEvent();
        }

        /// <summary>
        /// 最大化を切替
        /// </summary>
        public void SetMaximized(bool maximized)
        {
            //if (_isMaximized == maximized) return;
            if (uniWin == null)
            {
                _isMaximized = maximized;
            }
            else
            {

                if (maximized)
                {
                    uniWin.Maximize();
                }
                else if (uniWin.IsMaximized)
                {
                    uniWin.Restore();
                }
                _isMaximized = uniWin.IsMaximized;
            }
            StateChangedEvent();
        }

        /// <summary>
        /// 最小化を切替
        /// </summary>
        public void SetMinimized(bool minimized)
        {
            //if (_isMinimized == minimized) return;
            if (uniWin == null)
            {
                _isMinimized = minimized;
            }
            else
            {
                if (minimized)
                {
                    uniWin.Minimize();
                }
                else if (uniWin.IsMinimized)
                {
                    uniWin.Restore();
                }
                _isMinimized = uniWin.IsMinimized;
            }
            StateChangedEvent();
        }

        /// <summary>
        /// 最前面を切替
        /// </summary>
        /// <param name="topmost"></param>
        public void SetTopmost(bool topmost)
        {
            //if (_isTopmost == topmost) return;
            if (uniWin == null) return;

            uniWin.EnableTopmost(topmost);
            _isTopmost = uniWin.IsTopmost;
            StateChangedEvent();
        }

        /// <summary>
        /// Begin to accept file drop.
        /// </summary>
        public void BeginFileDrop()
        {
            if (uniWin != null)
            {
                uniWin.BeginFileDrop();
            }
            _enableFileDrop = true;
        }

        /// <summary>
        /// End to accept file drop.
        /// </summary>
        public void EndFileDrop()
        {
            if (uniWin != null)
            {
                uniWin.EndFileDrop();
            }
            _enableFileDrop = false;
        }


        /// <summary>
        /// Show open file dialog.
        /// </summary>
        /// <returns></returns>
        public string ShowOpenFileDialog(string filter = "All files|*.*")
        {
            if (uniWin != null)
            {
                return uniWin.ShowOpenFileDialog(filter);
            }
            return null;
        }

        /// <summary>
        /// 終了時にはウィンドウプロシージャを戻す処理が必要
        /// </summary>
        void OnApplicationQuit()
        {
            if (Application.isPlaying)
            {
                if (uniWin != null) {
                    uniWin.Dispose();
                }
            }
        }

        /// <summary>
        /// 自分のウィンドウにフォーカスを与える
        /// </summary>
        public void Focus()
        {
            if (uniWin != null)
            {
                uniWin.SetFocus();
            }
        }
    }


#if UNITY_EDITOR
    // エディタ実行時専用
    // ゲームビューが閉じたり開いたりされたときの対応

    // 参考 http://baba-s.hatenablog.com/entry/2017/12/04/090000#Unity-%E3%82%A8%E3%83%87%E3%82%A3%E3%82%BF%E3%81%AE%E5%90%84%E3%82%A6%E3%82%A3%E3%83%B3%E3%83%89%E3%82%A6%E3%81%AE%E4%BD%8D%E7%BD%AE%E3%81%8C%E5%A4%89%E6%9B%B4%E3%81%95%E3%82%8C%E3%81%9F%E6%99%82
    [InitializeOnLoad]
    public static class EditorApplicationUtility
    {
        private const BindingFlags BINDING_ATTR = BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic;

        private static readonly FieldInfo m_info = typeof(EditorApplication).GetField("windowsReordered", BINDING_ATTR);

        /// <summary>
        /// エディタウィンドウ配置変化時に呼ばれる
        /// </summary>
        public static EditorApplication.CallbackFunction windowsReordered
        {
            get
            {
                return m_info.GetValue(null) as EditorApplication.CallbackFunction;
            }
            set
            {
                var functions = m_info.GetValue(null) as EditorApplication.CallbackFunction;
                functions += value;
                m_info.SetValue(null, functions);
            }
        }
    }
#endif

}
