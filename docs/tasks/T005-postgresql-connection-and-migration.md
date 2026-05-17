---
id: T005
title: PostgreSQL 接続と初回 migration
phase: 1
status: todo
depends_on: [T004]
spec_refs: ["4.1"]
layer: infra
---

## 目的

`ApplicationDbContext` を PostgreSQL (devcontainer の `postgres` サービス) に接続し、初回 migration を生成・適用する。InMemory provider から Npgsql provider への切替を行う。

## 背景 / 元コード参照

devcontainer の compose は `postgres:15` を `portfoliodb` / `portfoliouser` / `portfoliopass` で起動済み。接続文字列を `appsettings.Development.json` に追加する。本番用接続文字列はユーザシークレットや環境変数で注入する想定 (本タスクでは方針記述のみ)。

## 作業内容

- [ ] `appsettings.Development.json` に `ConnectionStrings:DefaultConnection` を追加 (`Host=postgres;Database=portfoliodb;Username=portfoliouser;Password=portfoliopass`)
- [ ] `Program.cs` で `UseNpgsql(Configuration.GetConnectionString("DefaultConnection"))` に切替
- [ ] `dotnet ef migrations add InitialCreate` を実行し `Data/Migrations/*` を生成
- [ ] `dotnet ef database update` で実際にスキーマが作られることを確認
- [ ] アプリ起動時に開発環境のみ `db.Database.Migrate()` を呼ぶか判断し、ドキュメント化する
- [ ] 統合テスト: Testcontainers.PostgreSql で実 PostgreSQL に接続し、CRUD が動くテストを1本追加 (重い場合は `[Trait("Category", "integration")]` で分離)
- [ ] タスクファイルの status を `done` に更新する commit を含める

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

- [ ] `dotnet ef database update` がエラーなく完了する
- [ ] devcontainer 内で `dotnet run` してアプリが正常起動する
- [ ] Testcontainers ベースの統合テストがグリーン

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
