# 『Unity JobSystemで大群を動かす！実用Boidsシミュレーション』サンプルリポジトリ

技術書典 商品ページ : https://techbookfest.org/product/eQPriK07yiyuDYm5TWXKGk

 <img src="https://github.com/user-attachments/assets/ff89c9b8-f1ce-4aa6-9553-c63ac64f9203" width="40%" />

## フォルダ構成

```
Techbook-PracticalBoids/
├── PracticalBoids-unity/ // サンプルUnityプロジェクト
├── unitypackage/         // お試し用unity package
├── README.md
└── LICENSE
```

## PracticalBoids-unity
本のサンプルプログラムを章ごとにまとめたUnityプロジェクトです。URPで作成しています。

### バージョン

* Unity 6000.0.36f1
* URP 17.0.3
* Burst 1.8.18
* Collections 2.5.1
* Mathematics 1.3.2

### フォルダ構成
章ごとにフォルダを分けて、各サンプルシーンやプログラムを保存しています。

```
PracticalBoids-unity/
├── Assets/
|     ├── Common/    // サンプル全体で使用するプログラム
|     ├── FBX/       // シーン上に含まれる.fbxデータ
|     ├── Materials/ // シーン上で使用するマテリアル
|     ├── Part1/     // 1章 Graphics.RenderMeshInstancedとJobSystemの基礎
|     ├── Part2/     // 2章 群れを作るBoidsシミュレーション
|     ├── Part3/     // 3章 Boidsシミュレーションの最適化
|     ├── Part4/     // 4章 Colliderの衝突回避と退避行動の実装
|     ├── Part5/     // 5章 Boidsシミュレーションを使ったシューティングゲーム開発
|     └── Settings/  // URPの設定
├── Packages/
├── ProjectSettings/
└── .gitignore
```

### サンプルの紹介
**❗❗注意 ❗❗ サンプルを実行するときは、UnityのSceneビューとGameビューを同時に開かないよう気をつけてください。**

レンダリング負荷が高くなり、動作が重くなる原因になります。

#### 1章 Graphics.RenderMeshInstancedとJobSystemの基礎
Graphics.RenderMeshInstancedとJobSystemを使って、大量のCubeが落ちるサンプル。

↓ 100,000個で動作確認。

https://github.com/user-attachments/assets/7c2dc2dc-dfe7-4a03-846f-c0b7c872bb56


#### 2章 群れを作るBoidsシミュレーション
Boidsシミュレーションを作成したサンプル。

↓ 4,000体で動作確認。

https://github.com/user-attachments/assets/1f9b2862-d9a8-49c0-bd5d-70c502138e61

#### 3章 Boidsシミュレーションの最適化
Boidsシミュレーションを最適化して個体数を増やし、シミュレーション空間を広げたサンプル。

↓ 30,000体で動作確認。

https://github.com/user-attachments/assets/07c06c1d-2cea-432b-b4cf-c047c660a9e7

#### 4章 Colliderの衝突回避と退避行動の実装
Boidsシミュレーションに衝突回避と退避行動を追加したサンプル。シーン上のオブジェクトを回避したり、プレイヤーから退避する。

↓ 30,000体で動作確認。

https://github.com/user-attachments/assets/3639c8e9-dd32-495a-857c-f20e7b057e17

#### 5章 Boidsシミュレーションを使ったシューティングゲーム開発
Boidsシミュレーションに生存管理や衝突判定を実装し、シューティングゲームに拡張したサンプル。

↓ 30,000体で動作確認。

https://github.com/user-attachments/assets/f5d1caab-53a5-4122-b14a-c03d58e4927b

### 本からの修正箇所

「4章 Colliderの衝突回避と退避行動の実装」にて、`ApplySteerForceJob` の速度制限を修正しました。

```cs
if (escapeForce is not { x: 0, y: 0, z: 0 })
{
    escapeForce *= _escapeObstaclesWeight;
    boidsData.Velocity = MathematicsUtility.Limit(boidsData.Velocity + escapeForce * _deltaTime, _escapeMaxSpeed);
}
else
{
    // 原稿から修正：動的オブジェクトから逃げない場合、通常の最高速度に制限する
    // 逃げない場合にのみ制限しないと、次フレームで逃げる速度が消えるため
    boidsData.Velocity = MathematicsUtility.Limit(boidsData.Velocity + force * _deltaTime, _maxSpeed);
}
```

<br>

## お試し用Boidsシミュレーション unity package
読者の作成済みUnityプロジェクトでも簡単に導入できるよう、Boidsシミュレーションのunity packageを公開しています。
「4章 Colliderの衝突回避と退避行動の実装」の実装とサンプルが含まれています。

### バージョン
unity packageは汎用性を高めるため、Unity 2022.3.0f1で作成しています。

* Unity 2022.3.0f1
* Burst 1.8.18
* Collections 2.5.1
* Mathematics 1.3.2

### 導入方法

#### Package Managerからインポートする場合

1. UnityのPackage Managerを開く
2. `+`ボタンを押して、Git URLから追加を選択
3. 以下のURLを入力
```
https://github.com/Shitakami/Techbook-PracticalBoids.git?path=unitypackage/com.shitakami.practicalboids#v1.0.0
```

#### manifest.jsonを直接編集してインポートする場合

1. プロジェクトの`Packages/manifest.json`ファイルを開く
2. `dependencies` 内に以下を追加
```
"com.shitakami.practicalboids": "https://github.com/Shitakami/Techbook-PracticalBoids.git?path=unitypackage/com.shitakami.practicalboids#v1.0.0"
```

3. Unity画面をリフレッシュ後、自動的にパッケージがインストールされる

### サンプルのインポート
**サンプルはURPで作成しているため、Built-inなどで使用する場合は自前でマテリアルを変更してください。**

1. unity packageインポート後に、UnityのPackage Managerを開く
2. `Packages - Shitakami` の `Practical Boids Sample` を選択
3. Sampleタブを選択し、`Boids Sample`をダウンロード
4. `Assets/Samples/Practical Boids Smaple`が追加されたことを確認

![image](https://github.com/user-attachments/assets/c849e33f-3a5e-4b43-8a34-1c5db0ecf07b)
