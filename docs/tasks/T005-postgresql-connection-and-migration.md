---
id: T005
title: PostgreSQL 接続と初回 migration
phase: 1
status: done
depends_on: [T004]
spec_refs: ["4.1"]
layer: infra
---

## 目的

`ApplicationDbContext` を PostgreSQL (devcontainer の `postgres` サービス) に接続し、初回 migration を生成・適用する。InMemory provider から Npgsql provider への切替を行う。

## 背景 / 元コード参照

devcontainer の compose は `postgres:15` を `portfoliodb` / `portfoliouser` / `portfoliopass` で起動済み。接続文字列を `appsettings.Development.json` に追加する。本番用接続文字列はユーザシークレットや環境変数で注入する想定 (本タスクでは方針記述のみ)。

## 作業内容

- [x] `appsettings.Development.json` に `ConnectionStrings:DefaultConnection` を追加
- [x] `Program.cs` で `UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))` に切替
- [x] `dotnet ef migrations add InitialCreate` を実行し `Data/Migrations/*` を生成
- [x] `dotnet ef database update` で実際にスキーマ作成を確認
- [x] アプリ起動時 (`IsDevelopment()` かつ `Database.IsRelational()` 時) のみ `db.Database.Migrate()` を実行
- [x] 統合テスト: 実 PostgreSQL に接続して CRUD と `QrToken.Token` ユニーク制約違反 (`DbUpdateException`) を確認、`[Trait("Category", "integration")]` で分離
- [x] タスクファイルの status を `done` に更新する commit を含める

### 追加メモ

- **Testcontainers 不採用**: devcontainer に Docker-in-Docker が無いため `Testcontainers.PostgreSql` を導入できなかった。代わりに devcontainer compose の `postgres` サービスへ直接接続する形で integration test を実装。CI 環境で Docker が利用可能になった時点で Testcontainers 化すべき (`PostgresIntegrationTests.cs` 冒頭の TODO コメント参照)。
- `dotnet-ef` 8.0.27 をグローバルツールとして install (ローカル環境のみ。`.dotnet/tools` を PATH に追加する必要あり)。
- T004 でスキップしていた `QrToken.Token` ユニーク制約違反 → `DbUpdateException` の検証は本タスクの integration test で対応済み。
- 接続先は環境変数 `POSTGRES_TEST_CONNECTION` でも上書き可能。CI で別 DB を指したい場合に利用。
- 本番用接続文字列はユーザシークレット / 環境変数 (`ConnectionStrings__DefaultConnection`) で注入する想定。本タスクでは導入なし。

## テスト戦略

- **種別**: integration (real PostgreSQL via Testcontainers)
- **対象**: Npgsql provider 経由での CRUD と migration 適用
- **フィクスチャ**: `Testcontainers.PostgreSql` で使い捨てインスタンスを起動
- **TDD 順序**:
  1. 失敗テスト: `CanInsertCourseIntoPostgres`
  2. 接続文字列・provider 切替
  3. migration 生成・適用
  4. グリーン後、テスト並列実行で衝突しないか確認

## 受入基準

- [x] `dotnet ef database update` がエラーなく完了
- [x] devcontainer 内で `dotnet run` してアプリが正常起動 (Migrate() が成功し、smoke test も実 Postgres 経由でグリーン)
- [-] Testcontainers ベースの統合テストがグリーン → **代替**: 実 PostgreSQL に対する integration test (Trait 分離) がグリーン (36 passed)

## 想定 commit 列

```
chore: T005 add PostgreSQL connection string to dev appsettings
feat: T005 switch DbContext provider to Npgsql
chore: T005 generate InitialCreate migration
test: T005 add Testcontainers-based integration test
docs: T005 mark task done
```

## 影響範囲

- 変更: `appsettings.Development.json`, `Program.cs`
- 追加: `Data/Migrations/*`, `tests/DoctorPortfolioSite.Tests/Integration/*`
