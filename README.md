# Kogane UI Aging Button

ボタンを自動で押すエージングテストを実装できるコンポーネント

## 使用例

![2020-07-20_120914](https://user-images.githubusercontent.com/6134875/87896183-75d9f900-ca82-11ea-9ce7-b2f9118e0548.png)

自動でタップしてほしいボタンに「UIAgingButton」をアタッチして

```cs
// タグが設定されていないボタンのタップ開始
UIAgingButton.SetEnable( "Untagged", true );
```

上記のようなコードを記述することで使用できます

```cs
[InitializeOnLoad]
public static class Example
{
    static Example()
    {
        UIAgingButton.Tags = new[]
        {
            "TAG_1",
            "TAG_2",
            "TAG_3",
        };
    }
}
```

このようなエディタ拡張を記述することで

![2020-07-20_121118](https://user-images.githubusercontent.com/6134875/87896185-770b2600-ca82-11ea-9b08-2fa0964192f5.png)

タグを追加することができます

```cs
// TAG_1 タグが設定されているボタンのタップ開始
UIAgingButton.SetEnable( "TAG_1", true );
// TAG_2 タグが設定されているボタンのタップ開始
UIAgingButton.SetEnable( "TAG_2", true );
```

タグを分けることで複数のエージングテストを実装できます

![01](https://user-images.githubusercontent.com/6134875/87896186-770b2600-ca82-11ea-9eb5-752122c8662d.png)

1回だけタップしてほしい場合は「Is Once」をオンにします

![02](https://user-images.githubusercontent.com/6134875/87896188-77a3bc80-ca82-11ea-8965-a94ed280a1b5.png)

ときどきタップしてほしい場合は「Random Weight」に 0.0 から 100.0 の数値を指定します  
例えば 0.1 を指定すると毎フレーム 0.1% の確率でタップされるようになります  
0.0 か 100.0 なら必ずタップします

例えばエージングテスト中に装備選択画面で装備はランダムに選択したい場合などに活用できます

![03](https://user-images.githubusercontent.com/6134875/87896193-796d8000-ca82-11ea-9055-a393617872eb.png)

タップを遅延させたい場合は「Delay Frame」に遅延させるフレーム数を指定します  
例えば 60 を指定すると 60 フレーム後からタップされるようになります

例えばエージングテスト中に装備選択画面で装備を選択してから決定ボタンを押してほしい時などに活用できます

```cs
// シーン遷移中はエージングテストでタップできないようにする
UIAgingButton.CanClick = () => !SceneLoader.IsLoading;
```

もしもエージングテストは継続したいが特定の状況でタップを無効化したいときがあれば上記のように記述できます

```cs
UIAgingButton.OnClick = ( handler, data ) =>
{
    ( handler as IPointerDownHandler )?.OnPointerDown( data );
    ( handler as IPointerUpHandler )?.OnPointerUp( data );
};
```

UIAgingButton はデフォルトでは OnPointerClick を呼び出しますが  
IPointerDownHandler や IPointerUpHandler などを使用してボタンを実装している場合は  
上記のようにタップ処理を上書きすることでエージングテストできるようになります

```cs
// デバッグメニューが表示されていてもエージングテストを進行
UIAgingButton.IsClick = go => go.scene.name != "DebugMenuScene";
```

UIAgingButton は対象のボタンの前面に他の UI が存在する場合はタップしないようになっています  
もしも「デバッグメニューは前面に表示されていても無視したい」などの場合は上記のようなコードを記述できます

リリースビルドから UIAgingButton の機能を除外したい場合は  
`DISABLE_UNI_AGING_BUTTON` シンボルを定義します

## 既知の問題点

* Canvas の Render Mode が「Screen Space - Overlay」だと正常に動作しません

## 謝辞

* このリポジトリは下記のリポジトリを参考にさせていただいております
    * https://github.com/mob-sakai/ButtonEx
