# portfolio-qr-demo-asp

医師生涯学習ポートフォリオシステム ― **ASP.NET Core 8 実装イメージ / 機能追加検討用 PoC**

## このプロジェクトの位置づけ

医師の生涯学習を支援するポートフォリオシステムを、ASP.NET Core 8 + Razor Pages + Entity Framework Core + PostgreSQL で実装したデモです。

本プロジェクトの目的は **「既存 ASP.NET 本番システムにこの機能群を載せられるか」「追加機能としてどの方向を伸ばすのが妥当か」を関係者で議論できる動く題材を用意すること** にあります。本番運用を直接置き換えるためのコードではありません。

### こんな方に向けた資料です

- **意思決定者**: 機能カバレッジと開発粒度の感覚を掴みたい
- **技術レビュアー**: 設計の妥当性、テスト戦略、Identity / EF Core の使い方を確認したい
- **業務担当者**: 主催者・受講者・管理者それぞれの画面イメージを動かして触りたい
- **将来の追加機能オーナー**: 現状の土台の上にポートフォリオ機能・レポート機能をどう載せるか考えたい

---

## 何ができるか (現時点)

ロール別ユースケース。すべてブラウザから操作可能で、自動テスト (128 件) で挙動を担保しています。

### 共通

- ログイン / ログアウト (ASP.NET Core Identity)
- 公開クラス一覧の閲覧 (`/Courses`) ― 未ログインでも閲覧可
- 開発環境では `/Identity/Account/Login` にデモアカウント一覧を自動表示

### 主催者 (Organizer) / 管理者 (Admin)

| 機能 | URL | 用途 |
|---|---|---|
| クラス登録 | `/Courses/Create` | 単発のクラスを 1 件ずつ登録。単位コード (複数) と単位量 (decimal) を紐付け |
| CSV テンプレート DL | `/Courses/CsvTemplate` | Excel 互換 (UTF-8 BOM 付き) のテンプレ CSV |
| CSV 一括インポート | `/Courses/Import` | テンプレ準拠 CSV をアップロードして複数クラスを一括登録。行毎の成功/失敗を表示 |
| 完了 URL + QR 生成 | `/Courses/CompletionUrl` | 複数クラス選択 → 完了URL + QR コード PNG (画面表示 + DL) |
| 単一クラス QR 生成 | `/Courses/{id}/QrCode` | クラス一覧カードから 1 クリックで遷移 |

### 受講者 (Participant)

| 機能 | URL | 用途 |
|---|---|---|
| 完了登録 | `/Complete?token=...` | QR をスキャンして遷移。JWT を検証し、対応クラスを「新規受講完了」「既に受講済み」「該当なし」に分類して表示 |
| 受講履歴 | `/MyCompletions` | 自分が完了したクラスの一覧 (新しい順) |

### サンプルアカウント (Development 環境のみ)

| メール | ロール | 用途 |
|---|---|---|
| `admin@example.com` | Admin | 管理者 |
| `owner@example.com` | Organizer | 主催者 |
| `learner1@example.com` | Participant | 受講者 |
| `learner2@example.com` | Participant | 受講者 |
| `multi@example.com` | Organizer + Participant | 二重ロール検証用 |

パスワードは全アカウント共通: `Password123!` (Development 専用、`appsettings.Development.json` で上書き可)。

---

## 技術スタック

| 層 | 採用技術 | 理由 |
|---|---|---|
| Web フレームワーク | ASP.NET Core 8 (Razor Pages) | 既存 ASP.NET 本番システムとの統合を想定。ページ単位で実装でき機能ごとの粒度を保ちやすい |
| 認証 | ASP.NET Core Identity | ロール (Admin/Organizer/Participant) を標準機能で実現。パスワードハッシュ・Cookie 認証・antiforgery 標準装備 |
| データアクセス | Entity Framework Core 8 + Npgsql | コードファーストでマイグレーション管理。PostgreSQL をターゲット |
| データベース | PostgreSQL 15 | スキーマは EF マイグレーションで管理 |
| トークン | JWT (HS256, `System.IdentityModel.Tokens.Jwt`) | 完了 URL を stateless JWT で発行。DB に状態を持たないので運用負荷ゼロ |
| QR コード | QRCoder | サーバーサイドで PNG を生成し data URL として直接埋め込み |
| 単体テスト | xUnit + FluentAssertions | .NET 標準的な組み合わせ |
| 統合テスト | `WebApplicationFactory<Program>` + InMemory EF + 実 PostgreSQL | ページ E2E は HTTP 経由、Postgres 統合は dev container 内 DB |

---

## アーキテクチャ

```
[Razor Page]              Pages/Courses/*, Pages/Complete, Pages/MyCompletions
     │ DI                 ロール認可・antiforgery・モデルバインディング
     ▼
[Application Service]     Application/{Courses, Credits, CourseCompletions, Tokens}
     │                    ICourseService / ICreditService / ICourseCompletionService /
     │                    ICompletionTokenService / CourseImportRowValidator
     ▼
[Infrastructure]          Infrastructure/{Qr, Seeding}
     │                    QrCodeService / CreditSeeder / SampleUserSeeder / SampleCourseSeeder
     ▼
[Domain Model]            Domain/Models/*
     │                    Course / Credit / ClassCredit / CourseCompletion / ApplicationUser
     ▼                    + Guard で不変条件を強制 (private setter + ファクトリ)
[EF Core + PostgreSQL]    Data/ApplicationDbContext + Data/Configurations + Data/Migrations
```

層ごとの責務を分けることで、

- **Domain** は EF や ASP.NET に依存しない純粋な C# で書け、単体テストが軽い
- **Application** は `DbContext` のみ知る。Razor ページが薄くなる
- **Page** はバリデーション・ユーザー入力整形・リダイレクトに集中

を実現しています。同じ構造で Phase 3 のポートフォリオ機能やレポート機能を追加できる想定です。

---

## 動かし方 (Devcontainer)

VS Code + Docker を前提とした devcontainer で全部入りです。

### 起動

1. VS Code でこのリポジトリを開く
2. コマンドパレットから `Dev Containers: Reopen in Container`
3. コンテナ初回ビルド後、ターミナルで:

```bash
dotnet ef database update   # 初回のみ: PostgreSQL にスキーマ作成
dotnet run                  # http://localhost:5000 で待ち受け
```

初回 `dotnet run` (Development) で以下が自動投入されます:

- ロール (Admin / Organizer / Participant)
- 8 単位 (`IT001` 〜 `SK002`)
- 5 サンプルユーザー
- 5 サンプルクラス (各 ClassCredit 付き)

### テストの実行

```bash
dotnet test                                       # 全部 (unit + e2e + integration)
dotnet test --filter "Category!=integration"      # PostgreSQL 不要なものだけ
```

---

## リポジトリ構成

```
/
├── Application/                  サービス層 (純粋なビジネスロジック)
│   ├── Courses/                  CourseService / CourseDtos / CourseImportRowValidator
│   ├── Credits/                  CreditService (チェックボックス選択肢用)
│   ├── CourseCompletions/        CourseCompletionService (受講完了 upsert)
│   └── Tokens/                   ClassCompletionTokenService (JWT)
├── Areas/Identity/Pages/         Identity UI のレイアウト上書き (デモアカウント表示用)
├── Data/                         EF Core
│   ├── ApplicationDbContext.cs
│   ├── Configurations/           HasColumnName / FK / UNIQUE 設定
│   └── Migrations/               EF マイグレーション
├── Domain/                       純粋ドメイン (ASP.NET / EF 非依存)
│   ├── Guard.cs                  不変条件チェック
│   └── Models/                   Course / Credit / ClassCredit / CourseCompletion / ApplicationUser
├── Infrastructure/
│   ├── Qr/                       QrCodeService (QRCoder ラッパー)
│   └── Seeding/                  RoleSeeder / AdminSeeder / CreditSeeder / SampleUserSeeder / SampleCourseSeeder
├── Pages/                        Razor Pages (画面)
│   ├── Courses/                  一覧・登録・CSV・QR・完了URL
│   ├── Complete.cshtml           受講完了処理
│   ├── MyCompletions.cshtml      自分の受講履歴
│   ├── Index.cshtml              ロール別ダッシュボード
│   └── Shared/                   _Layout / _DemoAccounts (Dev のみ表示)
├── tests/DoctorPortfolioSite.Tests/
│   ├── Domain/                   ドメイン単体テスト
│   ├── Application/              サービス層テスト
│   ├── Infrastructure/           QrCodeService テスト
│   ├── Seeding/                  各 seeder の冪等性テスト
│   ├── Data/                     ApplicationDbContext テスト
│   ├── Integration/              実 PostgreSQL に対する統合テスト
│   └── E2E/                      WebApplicationFactory による HTTP テスト
├── docs/
│   ├── roadmap.md                Phase ごとのタスク索引
│   └── tasks/T***-*.md           タスクごとの仕様・受入基準・commit 計画
├── SPECIFICATIONS.md             ビジネス仕様 (Phase 3 以降の指針)
├── DoctorPortfolioSite.csproj
└── DoctorPortfolioSite.sln
```
---

## 参考

- 仕様書: [`SPECIFICATIONS.md`](SPECIFICATIONS.md)
- タスク索引: [`docs/roadmap.md`](docs/roadmap.md)
