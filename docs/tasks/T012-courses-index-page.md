---
id: T012
title: Courses 一覧 Page (`/Courses`)
phase: 2
status: todo
depends_on: [T011]
spec_refs: ["2.2", "6.2", "6.3"]
layer: page
---

## 目的

PHP `public/classes.php` を Razor Page 化する。誰でも閲覧可 (未ログインでも見える)。Organizer / Admin にはクラスごとに「QR コード生成」アクションを出す。

## 背景 / 元コード参照

- PHP `classes.php`: クラスを credits 付きで一覧、ロールに応じてアクション切替
- 表示要素: クラス名 / 開催団体 / 説明 / 開催日時 / 時間(分) / 単位(label + amount のリスト)

## 作業内容

- [ ] `Pages/Courses/Index.cshtml` + `.cshtml.cs` を作成
- [ ] PageModel で `ICourseService.ListAsync()` を呼び、`CourseDto[]` をビューに渡す
- [ ] ビューで PHP と同じ情報を表示 (Bootstrap 5 のカード)
- [ ] ナビゲーション: ログイン中 Organizer/Admin の場合のみ「クラス登録」「CSV インポート」「完了URL生成」リンクを表示
- [ ] 各カードに `[Authorize(Roles="Admin,Organizer")]` で守られた「QRコード生成」リンク (T018 へ)
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: e2e (`WebApplicationFactory`)
- **対象**: 未認証で GET → 200 + サンプルクラス名が含まれる / Organizer ユーザーで GET → 「クラス登録」リンクが含まれる
- **フィクスチャ**: 既存 `TestWebApplicationFactory` + InMemory + 1 件 Course を Seeder で作成

## 受入基準

- [ ] `dotnet test` でページテストグリーン
- [ ] ブラウザ目視は不要 (devcontainer ヘッドレス)

## 想定 commit 列

```
test: T012 add failing test for /Courses anonymous access
feat: T012 add Courses/Index page
test: T012 add failing test for organizer-only nav items
refactor: T012 extract role-aware nav partial
docs: T012 mark task done
```

## 影響範囲

- 追加: `Pages/Courses/Index.cshtml(+.cs)`, テスト
- 変更: `Pages/Shared/_Layout.cshtml` (ナビゲーション分岐)
