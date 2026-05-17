---
id: T006
title: ASP.NET Core Identity セットアップ
phase: 1
status: todo
depends_on: [T005]
spec_refs: ["2.4", "5.1", "8.1"]
layer: infra
---

## 目的

ASP.NET Core Identity を導入し、仕様書 5.1 の `Users` テーブルに相当する `ApplicationUser : IdentityUser` を定義する。`ApplicationDbContext` を `IdentityDbContext<ApplicationUser>` に切替え、Identity 関連テーブルの migration を追加する。

## 背景 / 元コード参照

T003 では非 Identity モデルのみ扱った。Identity は `IdentityUser` の派生として `ApplicationUser` を定義し、`Name` / `MedicalLicenseNumber` などの追加プロパティを持たせる。

## 作業内容

- [ ] `Domain/Models/ApplicationUser.cs` を追加 (`IdentityUser` 派生、`Name` / `MedicalLicenseNumber` プロパティ)
- [ ] `ApplicationDbContext` を `IdentityDbContext<ApplicationUser>` に変更し `base.OnModelCreating(builder)` を呼ぶ
- [ ] `Program.cs` に `AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders()` を追加
- [ ] `AddIdentity` のパスワードポリシーを仕様書 8.1 に合わせて設定 (強度チェック)
- [ ] `dotnet ef migrations add AddIdentity` を生成
- [ ] migration 適用後、Identity スキーマが作成されることを確認
- [ ] 既存ドメインモデルの `UserId` (string FK) と `ApplicationUser` のリレーションを `OnModelCreating` で結ぶ
- [ ] テスト: `UserManager<ApplicationUser>` を DI から解決し、ユーザ作成・パスワード検証が成功する統合テスト
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: integration
- **対象**: Identity DI 構成・ユーザ作成
- **フィクスチャ**: `WebApplicationFactory<Program>` + InMemory or Sqlite テスト DB
- **TDD 順序**:
  1. 失敗テスト: `UserManager_CanCreateAndAuthenticateUser`
  2. Identity 登録 → migration
  3. ApplicationUser へのプロパティ追加

## 受入基準

- [ ] `dotnet test` で Identity 統合テストがグリーン
- [ ] migration が冪等に適用できる
- [ ] `Enrollment.UserId` などの FK が `AspNetUsers.Id` を参照する

## 想定 commit 列

```
test: T006 add failing test for UserManager creating a user
feat: T006 add ApplicationUser with custom profile fields
feat: T006 switch DbContext to IdentityDbContext and register Identity
chore: T006 generate AddIdentity migration
refactor: T006 wire Enrollment.UserId to ApplicationUser
docs: T006 mark task done
```

## 影響範囲

- 追加: `Domain/Models/ApplicationUser.cs`, migration
- 変更: `Data/ApplicationDbContext.cs`, `Program.cs`, 既存 Entity Configuration の FK
