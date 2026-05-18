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
| T002 | [テストプロジェクト作成](tasks/T002-setup-test-project.md) | done | T001 | - | - |
| T003 | [ドメインモデル定義 (Course/Enrollment/Portfolio/QRCode)](tasks/T003-define-domain-models.md) | done | T001 | - | - |
| T004 | [ApplicationDbContext 構成](tasks/T004-configure-dbcontext.md) | done | T003 | - | - |
| T005 | [PostgreSQL 接続と初回 migration](tasks/T005-postgresql-connection-and-migration.md) | done | T004 | - | - |
| T006 | [ASP.NET Core Identity セットアップ](tasks/T006-setup-identity.md) | done | T005 | - | - |
| T007 | [ロール (Admin/Organizer/Participant) seeding](tasks/T007-seed-roles.md) | done | T006 | - | - |
| T008 | [ログイン/ログアウト Page](tasks/T008-login-logout-pages.md) | done | T007 | - | - |

## Phase 2: ドメイン整合 + 基本機能

PHP 元コード (`ncukondo/portfolio-qr-demo`) のスキーマと機能をベースに移植する。元PHP には Portfolio 機能と QrToken テーブルが無く、Credit (多対多) と stateless JWT 完了URL が中心。Phase 1 で先に作った Course/Enrollment/QrToken は PHP の実体から乖離しているため、まず T009 でドメインを整合させてから機能を積む。

ロール名は SPECIFICATIONS.md に従い ASP.NET 流 (Admin/Organizer/Participant) を保持。PHP の `administrator` ⇔ Admin、`class-owner` ⇔ Organizer、`learner` ⇔ Participant とマッピング。

| ID | タイトル | Status | Depends | PR | Merged |
|---|---|---|---|---|---|
| T009 | [ドメイン整合: Credit/ClassCredit 追加・Course 列再設計・Completion 簡略化・QrToken 廃止](tasks/T009-realign-domain-with-php.md) | done | T008 | - | - |
| T010 | [Credits / Courses / Sample users の seed 移植](tasks/T010-seed-credits-courses-users.md) | done | T009 | - | - |
| T011 | [CourseService (CRUD + 一覧 + クレジット関連付け)](tasks/T011-course-service.md) | done | T009 | - | - |
| T012 | [Courses 一覧 Page (`/Courses`)](tasks/T012-courses-index-page.md) | done | T011 | - | - |
| T013 | [Course 登録 Page (`/Courses/Create`, Organizer 権限)](tasks/T013-course-create-page.md) | done | T011 | - | - |
| T014 | [CSV テンプレダウンロード (`/Courses/CsvTemplate`)](tasks/T014-csv-template-download.md) | done | T013 | - | - |
| T015 | [CSV 一括インポート Page (`/Courses/Import`)](tasks/T015-csv-bulk-import.md) | done | T013, T014 | - | - |
| T016 | [ClassCompletionToken サービス (JWT, stateless)](tasks/T016-completion-token-service.md) | done | T009 | - | - |
| T017 | [完了URL+QR 生成 Page (複数クラス対応)](tasks/T017-generate-completion-url-page.md) | done | T016, T011 | - | - |
| T018 | [単一クラス QR Page (`/Courses/{id}/QrCode`)](tasks/T018-single-course-qr-page.md) | done | T016, T011 | - | - |
| T019 | [クラス完了処理 Page (`/Complete?token=...`, Participant 権限)](tasks/T019-complete-classes-page.md) | done | T016, T009 | - | - |
| T020 | [自分の受講履歴 Page (`/MyCompletions`)](tasks/T020-my-completions-page.md) | done | T019 | - | - |
| T021 | [トップページとナビゲーションのロール別整理](tasks/T021-dashboard-and-nav.md) | done | T012, T020 | - | - |

## Phase 3: 応用機能開発

SPECIFICATIONS.md には記載されているが PHP 元実装に無い機能群。Phase 2 完了後に着手判断。

- ポートフォリオ機能 (学習履歴 / 目標管理 / 学習記録) — `Portfolio` エンティティを復活させて実装
- レポート機能 (受講統計 / CSV/Excel エクスポート)
- ユーザー管理 (管理者用 CRUD / プロフィール編集)
- UI/UX 改善 (Bootstrap 5 整備)

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
