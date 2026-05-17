# 移植ロードマップ

`ncukondo/portfolio-qr-demo` (PHP/Laravel風) を ASP.NET Core 8 + Razor Pages + EF Core + PostgreSQL へ素直に移植するためのロードマップ。

仕様の出典は [`../SPECIFICATIONS.md`](../SPECIFICATIONS.md)。本ファイルは索引であり、詳細は各タスクファイル (`tasks/T***-*.md`) を参照する。

## 運用ルール

- **1タスク = 1ブランチ = 1PR** が基本。膨らんだら PR を分けるのではなくタスクを `T001a` / `T001b` に分割する。
- **TDD**: タスクごとに「失敗テスト → 最小実装 → リファクタ」のサイクルで commit を残す。`squash merge` ではなく `rebase merge`。
- **branch 名**: `task/T<番号>-<slug>` (タスクファイル名と一致)
- **PR タイトル**: `T<番号>: <タイトル>`
- **commit prefix**: `test:` / `feat:` / `fix:` / `refactor:` / `chore:` / `docs:`
- **commit 1行目**: タスクIDを含める (例: `test: T003 add failing test for ...`)
- **status 遷移**: `todo` → `in_progress` (ブランチ作成時) → `review` (PR ready) → `done` (merge時)
- **マージ commit** にタスクファイルの `status: done` 更新を含める。
- 例外:
  - 調査のみのタスクはPRなし。結果をタスクファイルに追記して `done`。
  - migration とその利用コードは同PRに含める。
  - 横断的リファクタは独立タスクとし、機能PRに混ぜない。

## Phase 1: 基盤構築

| ID | タイトル | Status | Depends | PR | Merged |
|---|---|---|---|---|---|
| T000 | [.gitignore 追加と build artifact untrack](tasks/T000-add-gitignore.md) | done | - | - | - |
| T001 | [NuGet パッケージ追加](tasks/T001-add-nuget-packages.md) | done | T000 | - | - |
| T002 | [テストプロジェクト作成](tasks/T002-setup-test-project.md) | todo | T001 | - | - |
| T003 | [ドメインモデル定義 (Course/Enrollment/Portfolio/QRCode)](tasks/T003-define-domain-models.md) | todo | T001 | - | - |
| T004 | [ApplicationDbContext 構成](tasks/T004-configure-dbcontext.md) | todo | T003 | - | - |
| T005 | [PostgreSQL 接続と初回 migration](tasks/T005-postgresql-connection-and-migration.md) | todo | T004 | - | - |
| T006 | [ASP.NET Core Identity セットアップ](tasks/T006-setup-identity.md) | todo | T005 | - | - |
| T007 | [ロール (Admin/Organizer/Participant) seeding](tasks/T007-seed-roles.md) | todo | T006 | - | - |
| T008 | [ログイン/ログアウト Page](tasks/T008-login-logout-pages.md) | todo | T007 | - | - |

## Phase 2: 基本機能開発

PHP元コードの読み込み後に詳細タスクを切る。現時点では以下の粒度を想定:

- ユーザー管理 (CRUD / プロフィール編集)
- コース管理 (CRUD / 検索 / 一覧)
- 受講管理 (申込 / 履歴)
- QRコード生成・検証・スキャン

## Phase 3: 応用機能開発

- ポートフォリオ機能 (学習履歴 / 目標管理 / 学習記録)
- レポート機能 (受講統計 / CSV/Excel エクスポート)
- UI/UX 改善

## Phase 4: テスト・リリース

- 統合テスト整備
- セキュリティテスト (CSRF / XSS / 認可)
- 本番環境デプロイ手順

## レイヤーとテスト戦略

```
[Domain Model]     pure C# / 単体テスト最優先
     ↓
[Service]          DbContext は interface or InMemory provider で差替
     ↓
[Repository / EF]  PostgreSQL Testcontainer or SQLite で統合テスト
     ↓
[Razor Page]       WebApplicationFactory で E2E
```

タスクはこのレイヤー方向に分割する。1タスクで複数レイヤーを跨ぐ場合は分割を検討する。
