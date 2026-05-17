---
id: T008
title: ログイン / ログアウト Page
phase: 1
status: todo
depends_on: [T007]
spec_refs: ["2.3", "6.4", "7.1"]
layer: page
---

## 目的

Identity Default UI を導入するか手書きの Razor Page を用意するかを決め、ログイン・ログアウト・現在ユーザ情報の最小フローを実装する。E2E テストで「未認証ユーザは保護ページにアクセスすると `/Account/Login` にリダイレクトされる」「ログイン後はアクセスできる」ことを確認する。

## 背景 / 元コード参照

Phase 2 以降のコース管理・受講管理ページは認証必須となる。本タスクで認証エンドポイントの土台を固める。

## 作業内容

- [ ] 方針決定: Identity Default UI (`AddDefaultIdentity` / `AddRazorPages` のスキャフォルド) vs 自前 Razor Pages
  - 推奨: 最初は Default UI で立ち上げ、必要に応じて部分的に override
- [ ] `Areas/Identity/Pages/Account/Login.cshtml` などをスキャフォルド (or override 不要なら何もしない)
- [ ] レイアウトのナビゲーションに「ログイン」「ログアウト」「ユーザ名」を表示
- [ ] `[Authorize]` を要する `Pages/Protected/Sample.cshtml` を1枚追加 (E2E 検証用、後続タスクで削除可)
- [ ] E2E テスト:
  - 未認証 → 保護ページ GET で 302 + Location が `/Identity/Account/Login`
  - `UserManager` でテストユーザを直接作り、ログイン Post 後に Cookie が払い出されること
  - Cookie 付与で保護ページ GET が 200
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: e2e (`WebApplicationFactory<Program>`)
- **対象**: 認証フローのリダイレクトと Cookie 発行
- **フィクスチャ**: 専用の TestServer + InMemory DB + テスト用 seed user
- **TDD 順序**:
  1. 失敗テスト: 未認証 → 302 リダイレクト確認
  2. `[Authorize]` ページと Identity DI 設定
  3. 失敗テスト: ログイン Post → Cookie 検証
  4. 実装 (Default UI ならスキャフォルドのみ)
  5. 失敗テスト: ログイン後 → 保護ページ 200

## 受入基準

- [ ] `dotnet test` で認証フローの E2E が全グリーン
- [ ] ブラウザで `/Identity/Account/Login` が表示され、T007 で作った admin でログインできる
- [ ] 認証状態に応じてヘッダ表示が切替わる

## 想定 commit 列

```
test: T008 add failing test for unauthenticated redirect
feat: T008 add protected sample page
chore: T008 scaffold Identity Default UI
test: T008 add login flow e2e test
feat: T008 wire nav header with login/logout links
docs: T008 mark task done
```

## 影響範囲

- 追加: `Areas/Identity/*` (override する場合), `Pages/Protected/Sample.cshtml`, テスト
- 変更: `Pages/Shared/_Layout.cshtml`
