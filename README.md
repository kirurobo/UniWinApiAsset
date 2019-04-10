# UniWinApi

**UniWinApi** is a Windows API utility with a window controller class for Unity. This allows several window controls that Unity originally does not provide as followings.

* Move window
* Resize window
* Maximize and Minimize
* **Transparent window** (the window hasn't any borders, so it looks non-rectangular) 
* **Accept file dropping**
* **Open File open dialog (Experimental - file only now)**
* Move mouse pointer
* Send mouse button operation (up / down)

This asset is designed to provide functions which enable your Unity application to be some kind of Desktop Mascot.

[![UniWinApi VRM viewer](http://i.ytimg.com/vi/cq2g-hIGlAs/mqdefault.jpg)](https://youtu.be/cq2g-hIGlAs "UniWinApi VRM viewer v0.4.0 beta")


## Confirmed environment

* Unity 5.6.6f2, Unity 2018.2.20f1
* Windows 10 Pro x64
* GeForce GTX980, GeForce GTX 1070

|Scripting backend|x86_64|x86|
|:----------------|:----:|:-:|
|Mono             |  x   | x |
|IL2CPP           |  x   |   |

## Usage

1. Download .unitypackage file from the [Releases](https://github.com/kirurobo/UniWinApi/releases) page.
2. Import the package to your project.
3. Attach "WindowController.cs" to the object which you want to add the functions.
    * Recommended: Create an empty object named "WindowController".
4. Build the project as "PC Standalone" for "Platform"


## License

[![CC0](http://i.creativecommons.org/p/zero/1.0/88x31.png "CC0")](http://creativecommons.org/publicdomain/zero/1.0/deed.ja)


UniWinApi.cs is main. In addition, WindowsController.cs is provided which enables controls of self window.

You can change the state of the window. To enable that, change following values in Inspector View or write script which sets these values.


- IsTransparent - Let borders of the window transparent
- IsTopmost - Fixed to the front
- IsMaximized - Maximize the window
- IsMinimized - Minimize the window
- EnableFileDrop - Enable file dropping
- EnableDragMove - Enable window dragging with mouse left button

![image](https://user-images.githubusercontent.com/1019117/51594588-1dcb4080-1f38-11e9-9a93-910f59632fc2.png)

If you use file dropping, there is the event named `OnFilesDropped(string[] files). Thus, add some procedures to this.

I'm sorry that the document hasn't prepared. Please read the sample project (^-^;

## Attention

- Since I haven't known how to get window handle certainly, rarely you might operate with another window.
  - (On the other hand, you can designate and operate other windows)
- You might be forced to down the application if the window lost the "Close" button or if you cannot see the window since the window goes to somewhere.

## FAQ
- The transparent application looks false on Editor
  - This is because of the specification. You have to build to confirm the transparent appearance.
  - You cannot let the Game window transparent. I suppose it's because something paints the background.
  - Normally the operation object is Game View. If the view is docked with other views, the object is the whole window.

## License

[![CC0](http://i.creativecommons.org/p/zero/1.0/88x31.png "CC0")](http://creativecommons.org/publicdomain/zero/1.0/deed.ja)

UniWinApi is CC0 public domain.
You can modify, duplicate or redistribute without copyright notice.
However, there is no operation guarantee.

*I recognize this should be able to be CC0 in the scope of copyright protection, though I have used other information such as blogs as references. Because when I finish implementation of whole function set, the deliverable must be similar.

## Contact & Source

* [Twitter: @kirurobo](http://twitter.com/kirurobo)
* [GitHub](http://github.com/kirurobo)

## Change log

* 2019/01/23 Add file open dialog. Return the way to acquire self window to be active window standard
* 2018/12/28 Add namespace, Modify the way to acquire self window to be PID standard
* 2018/12/07 Separate the asset part of UniWinApi
* 2018/09/09 Publish package since this looks to work mostly as expected
* 2018/08/24 Arrange again as UniWinApi
* 2015/03/03 Set keep the status just before when the window is maximized or minimized
* 2014/04/26 First publish

---

# UniWinApi (日本語説明)

**UniWinApi** は Unityでは本来行えない操作を Windows API 経由で行うものです。  
自ウィンドウに対しての処理を行うためのコントローラーが付属します。  
以下のようなことができます。  

* ウィンドウの移動
* ウィンドウサイズ変更
* ウィンドウの最大化、最小化
* **ウィンドウの透過** （枠なしで、四角形でないウィンドウにします） 
* **ファイルのドロップを受け付ける**
* **Windowsのダイアログでファイルを開く（試験実装で単一ファイルのみ）**
* マウスポインタを移動させる
* マウスのボタン操作を送出する

主にデスクトップマスコット的な用途で利用しそうな機能を取り込んでいます。
[![UniWinApi VRM viewer](http://i.ytimg.com/vi/cq2g-hIGlAs/mqdefault.jpg)](https://youtu.be/cq2g-hIGlAs "UniWinApi VRM viewer v0.4.0 beta")


## 確認済み動作環境

* Unity 5.6.6f2, Unity 2018.2.20f1
* Windows 10 Pro x64
* GeForce GTX980, GeForce GTX 1070

|Scripting backend|x86_64|x86|
|:----------------|:----:|:-:|
|Mono             |  ○   | ○ |
|IL2CPP           |  ○   |   |

## 利用方法

1. [Releases](https://github.com/kirurobo/UniWinApi/releases) ページから .unitypackage ファイルをダウンロードします。
2. それをプロジェクトにインポートします。
3. "WindowController.cs" スクリプトを適当なオブジェクトにアタッチしてください。
    * "WindowController" といった名前の空オブジェクト作成をお勧めします。
4. Platform として PC Standalone を選び、ビルドしてください。

UniWinApi が本体ですが、自ウィンドウを操作するものとして WindowController を用意しています。


インスペクタで下記を変更するか、スクリプトから変更するとウィンドウの状態を変えられます。
- IsTransparent … ウィンドウ枠を透明化
- IsTopmost … 常に最前面
- IsMaximized … ウィンドウ最大化
- IsMinimized … ウィンドウ最小化
- EnableFileDrop … ファイルドロップを受け付ける
- EnableDragMove … マウス左ボタンでウィンドウを移動できる

![image](https://user-images.githubusercontent.com/1019117/51594588-1dcb4080-1f38-11e9-9a93-910f59632fc2.png)

ファイルドロップを利用する場合、`OnFilesDropped(string[] files)` というイベントがありますので、そちらに処理を追加してください。
files にはドロップされたファイルのパスが入ります。

ドキュメントは整えられていないため、利用例はサンプルをご覧ください (^-^;


## 注意点
- ウィンドウハンドルを確実に取る方法が分ってないので、別のウィンドウが操作対象になったりするかも知れません。
  - （むしろ別のウィンドウもタイトルやクラス名で指定して操作できます。）
- 閉じるボタンがなくなったり、ウィンドウを見失った場合、タスクマネージャから終了する必要が出るかも知れません。

## FAQ
- エディタ上で透明にすると表示がおかしいのですが。
	- すみません。仕様です。透明化はビルドしたものでご確認ください。
	- Game ウィンドウはどうも背景が塗りつぶされてしまっているようで、透明化されません。
	- ゲームビューが操作対象となりますが、他のビューとドッキングしていた場合はそのウィンドウごと対象となります。


## ライセンス

[![CC0](http://i.creativecommons.org/p/zero/1.0/88x31.png "CC0")](http://creativecommons.org/publicdomain/zero/1.0/deed.ja)

UniWinApi は、CC0（パブリックドメイン）としています。  
著作権表示なしで修正、複製、再配布も可能です。好きに使っていただけますが無保証です。

※他の方のブログなど参考にしている部分が大いにありますが、一通り作成した結果、同様の機能を実装した場合は似た内容になるとして、著作権で保護される範囲においては基本的に独自のものとしてCC0にできると判断しています。

## 連絡先・配布元

* [Twitter: @kirurobo](http://twitter.com/kirurobo)
* [GitHub](http://github.com/kirurobo)

## 更新履歴

* 2019/01/23 ファイルオープンダイアログ追加。自ウィンドウ取得方法をアクティブウィンドウ基準に戻した。
* 2018/12/28 namespaceを設定、自ウィンドウ取得をPIDを基準とする方法に修正
* 2018/12/07 UniWinApiのアセット部分を分離
* 2018/09/09 おおよそ想定通りに動作しそうなため、パッケージを公開
* 2018/08/24 UniWinApiとして整理し直した
* 2015/03/03 最小化、最大化時はその直前の状態を保存するようにした
* 2014/04/26 公開用初版