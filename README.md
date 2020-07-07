# UniUIAgingButton

## 使用例

![2020-07-07_221512](https://user-images.githubusercontent.com/6134875/86786922-84e2a380-c09f-11ea-8187-a0f64fa3b28d.png)

エージングテストで自動でクリックしてほしいゲームオブジェクトに「UIAgingButton」をアタッチします  
そして、スクリプトから `Kogane.UIAgingButton.IsEnable = true;` することで  
エージングテストを開始し、UIAgingButton がアタッチされたゲームオブジェクトが自動でクリックされるようになります  

```cs
using Kogane;
using UnityEngine;
using UnityEngine.EventSystems;

public class Example : MonoBehaviour
{
    private void Awake()
    {
        // エージングテストを有効にします
        UIAgingButton.IsEnable = true;

        // エージングテスト中にボタンを押せないようにしたい場合は false を渡します
        UIAgingButton.CanClick = () => true;

        // エージングテストでボタンを押す時の処理を上書きしたい場合はこのデリゲートを設定します
        UIAgingButton.OnClick = ( handler, data ) => ( ( IPointerClickHandler ) handler ).OnPointerClick( data );

        // エージングテストでボタンのクリック判定を行う時に無視したいゲームオブジェクトのルールを指定できます
        UIAgingButton.IsClick = go => go.scene.name != "DebugMenuScene";
    }
}
```

## 既知の問題点

* Canvas の Render Mode が「Screen Space - Overlay」だと正常に動作しません  

## 謝辞

* このリポジトリは下記のリポジトリを参考にさせていただいております  
    * https://github.com/mob-sakai/ButtonEx