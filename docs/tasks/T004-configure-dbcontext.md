---
id: T004
title: ApplicationDbContext 構成
phase: 1
status: todo
depends_on: [T003]
spec_refs: ["4.1", "5.1"]
layer: repository
---

## 目的

`ApplicationDbContext` を作成し、T003 で定義したドメインモデルを `DbSet<T>` として公開する。Fluent API でリレーション・インデックス・制約を設定する。DI 登録は本タスクで済ませるが、実際の PostgreSQL 接続は T005 で行う。

## 背景 / 元コード参照

T003 で作ったモデルは pure C# のため、永続化境界をここで明示する。Identity 用の `ApplicationUser` は T006 で追加し、`IdentityDbContext<ApplicationUser>` への切替もそこで行う (本タスクでは一旦 `DbContext` を継承)。

## 作業内容

- [ ] `Data/ApplicationDbContext.cs` を追加 (`DbContext` 継承)
- [ ] `DbSet<Course>` / `DbSet<Enrollment>` / `DbSet<Portfolio>` / `DbSet<QrToken>` を公開
- [ ] `OnModelCreating` でリレーション・必須項目・ユニークインデックス (例: `QrToken.Token`) を設定
- [ ] `Program.cs` に `AddDbContext<ApplicationDbContext>` を登録 (一旦 InMemory provider でOK、接続文字列切替は T005)
- [ ] テスト: InMemory provider で各 `DbSet` の add/save/query が動くことを確認
- [ ] テスト: `QrToken.Token` のユニーク制約違反が `DbUpdateException` を発生させること (Sqlite テストプロバイダ or 後続 T005 で確認するなら本タスクではスキップ可)
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: integration (InMemory)
- **対象**: `ApplicationDbContext` の構成と基本 CRUD
- **フィクスチャ**: `DbContextOptionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString())`
- **TDD 順序**:
  1. 失敗テスト: `CanAddAndQueryCourse`
  2. 最小実装: `ApplicationDbContext` と `DbSet<Course>` のみ追加
  3. 同様に他モデルを追加
  4. リファクタ: `OnModelCreating` を Entity Configuration クラスへ抽出

## 受入基準

- [ ] `dotnet test` で新規テストが全てグリーン
- [ ] `Program.cs` でアプリ起動時に DI コンテナが `ApplicationDbContext` を解決できる (smoke test がグリーン)
- [ ] Entity Configuration が `Data/Configurations/` に分離されている (リファクタ後)

## 想定 commit 列

```
test: T004 add failing test for ApplicationDbContext basic CRUD
feat: T004 add ApplicationDbContext with DbSets
feat: T004 register DbContext with InMemory provider
refactor: T004 extract entity configurations
docs: T004 mark task done
```

## 影響範囲

- 追加: `Data/ApplicationDbContext.cs`, `Data/Configurations/*.cs`, テスト
- 変更: `Program.cs`
