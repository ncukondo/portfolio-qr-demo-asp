---
id: T021
title: トップページとナビゲーションのロール別整理
phase: 2
status: in_progress
depends_on: [T012, T020]
spec_refs: ["6.1", "6.2", "6.3", "6.4"]
layer: page
---

## 目的

PHP `public/index.php` のロール別ダッシュボードと `_Layout` 全体のナビゲーションを整理する。Phase 2 で追加した各ページへの導線を一通り張る。

## 背景 / 元コード参照

PHP `index.php`:
- ヘッダーロゴ + ナビ
- 共通: ホーム / クラス一覧
- class-owner | administrator: クラス登録 / CSV一括 / 完了URL生成
- ログイン中ユーザー名 + ロール表示
- システム情報 (DB 接続 / PHP version) — Razor 版では削っても良い

## 作業内容

- [ ] `Pages/Index.cshtml` を作り直し:
  - 未ログイン → アプリ紹介 + 「ログイン」ボタン + 「クラス一覧 (公開)」ボタン
  - Participant → 「クラス一覧」「受講履歴 (T020)」
  - Organizer / Admin → 「クラス一覧」「クラス登録 (T013)」「CSV インポート (T015)」「完了URL生成 (T017)」
  - 上部にユーザー名 + ロールバッジ
- [ ] `Pages/Shared/_Layout.cshtml` をリファクタ:
  - ナビゲーションをロール別パーシャル (`_NavItems.cshtml`) に切り出し
  - `SignInManager` + `UserManager` の Razor Page injection でロール判定
- [ ] `wwwroot/css` に最小スタイル (Bootstrap 5 既定 + 軽いカスタム)
- [ ] PHP の "テスト用アカウント" 一覧 (`/login`) は T010 サンプル user に合わせて Login ページに表示する開発専用ヘルパ partial を追加 (Environment が Development のときだけ表示)
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: e2e
- **対象**:
  - 未認証で `/` → "ログイン" リンク存在、Organizer リンク非存在
  - Participant で `/` → "受講履歴" 存在、"クラス登録" 非存在
  - Organizer で `/` → "クラス登録" / "CSV インポート" 存在
- **フィクスチャ**: 既存 `TestWebApplicationFactory` + 3 ロールの seed user

## 受入基準

- [ ] `dotnet test` グリーン
- [ ] Phase 1 で書いた既存 E2E (`/Protected/Sample` リダイレクト等) が引き続き通る

## 想定 commit 列

```
test: T021 add failing tests for role-based nav items
refactor: T021 extract _NavItems partial
feat: T021 rebuild Index dashboard with role widgets
feat: T021 show demo accounts on login page in Development
docs: T021 mark task done
```

## 影響範囲

- 変更: `Pages/Index.cshtml(+.cs)`, `Pages/Shared/_Layout.cshtml`
- 追加: `Pages/Shared/_NavItems.cshtml`, テスト
