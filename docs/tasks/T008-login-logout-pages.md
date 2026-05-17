---
id: T008
title: ログイン / ログアウト Page
phase: 1
status: done
depends_on: [T007]
spec_refs: ["2.3", "6.4", "7.1"]
layer: page
---

## 目的

Identity Default UI を導入するか手書きの Razor Page を用意するかを決め、ログイン・ログアウト・現在ユーザ情報の最小フローを実装する。E2E テストで「未認証ユーザは保護ページにアクセスすると `/Account/Login` にリダイレクトされる」「ログイン後はアクセスできる」ことを確認する。

## 背景 / 元コード参照

Phase 2 以降のコース管理・受講管理ページは認証必須となる。本タスクで認証エンドポイントの土台を固める。

## 作業内容

- [x] 方針: Identity Default UI を有効化 (`AddIdentity<>...AddDefaultUI()`)。override せずに既存 Razor Pages をそのまま使う
- [-] `Areas/Identity/Pages/Account/Login.cshtml` のスキャフォルドは見送り (Default UI 既定で十分)
- [x] レイアウトのナビゲーションに `<partial name="_LoginPartial" />` を組込 (`Pages/Shared/_LoginPartial.cshtml`)
- [x] `[Authorize]` を要する `Pages/Protected/Sample.cshtml` を追加
- [x] E2E テスト:
  - 未認証 → 保護ページ GET で 302 + Location に `/Identity/Account/Login` を含む
  - `UserManager` でテストユーザを作成 → `/Identity/Account/Login` GET で antiforgery token 取得 → POST → 302 リダイレクト + Cookie 払い出し
  - Cookie 付き HttpClient で保護ページ GET が 200 + 期待文字列を含む
- [x] タスクファイルの status を `done` に更新する commit を含める

### 追加メモ

- **Default UI が要求する `_LoginPartial.cshtml` を `Pages/Shared/_LoginPartial.cshtml` に手書きで追加**。Default UI のレイアウトは標準ロケーションを探すため、無いと Login ページ自体が 500 を返す。
- **`UseAuthentication()` middleware の追加** が必要だった。`AddIdentity` は middleware を自動追加しないため `UseRouting` と `UseAuthorization` の間に挿入。
- E2E テスト基盤 `TestWebApplicationFactory` を追加。DbContext を InMemory にすげ替えることで Postgres 非依存で E2E が回る (Identity stores も InMemory で動作)。
- antiforgery token は HTML から regex で抽出。Default UI が `name="__RequestVerificationToken" ... value="..."` の hidden input を出力する前提。レイアウト変更で順序が変わると壊れる可能性あり (許容範囲)。

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

- [x] `dotnet test` で認証フローの E2E が全グリーン (44 passed: T007 までの 42 + E2E 2)
- [-] ブラウザでの目視確認は未実施 (devcontainer ヘッドレス環境のため。`dotnet run` で起動可能なことはアプリ起動 + Migrate + Seeder で担保)
- [x] 認証状態に応じてヘッダ表示が切替わる (`_LoginPartial` の `SignInManager.IsSignedIn(User)` 分岐)

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
