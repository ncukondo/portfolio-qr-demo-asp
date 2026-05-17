---
id: T006
title: ASP.NET Core Identity セットアップ
phase: 1
status: done
depends_on: [T005]
spec_refs: ["2.4", "5.1", "8.1"]
layer: infra
---

## 目的

ASP.NET Core Identity を導入し、仕様書 5.1 の `Users` テーブルに相当する `ApplicationUser : IdentityUser` を定義する。`ApplicationDbContext` を `IdentityDbContext<ApplicationUser>` に切替え、Identity 関連テーブルの migration を追加する。

## 背景 / 元コード参照

T003 では非 Identity モデルのみ扱った。Identity は `IdentityUser` の派生として `ApplicationUser` を定義し、`Name` / `MedicalLicenseNumber` などの追加プロパティを持たせる。

## 作業内容

- [x] `Domain/Models/ApplicationUser.cs` を追加 (`IdentityUser` 派生、`Name` / `MedicalLicenseNumber` / 監査列)
- [x] `ApplicationDbContext` を `IdentityDbContext<ApplicationUser>` に変更 (`base.OnModelCreating` は既存呼び出しを継続)
- [x] `Program.cs` に `AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders()` を追加
- [x] パスワードポリシーを設定 (RequiredLength=8 / Digit / Lowercase / Uppercase / NonAlphanumeric, `RequireUniqueEmail=true`)
- [x] `dotnet ef migrations add AddIdentity` を生成・適用 (AspNetUsers / AspNetRoles 等)
- [x] migration 適用後、Identity スキーマが作成されることを確認
- [x] `Enrollment.UserId` / `Portfolio.UserId` を `ApplicationUser` (`AspNetUsers.Id`) への FK として配線、`AddUserForeignKeys` migration を追加
- [x] テスト: `UserManager<ApplicationUser>` 経由でユーザ作成・パスワード検証・取得が成功する統合テスト (Trait=integration)
- [x] タスクファイルの status を `done` に更新する commit を含める

### 追加メモ

- パスワードポリシーは仕様書 8.1 「パスワード強度チェック」に対応。ASP.NET Core Identity の既定値に `RequiredLength=8` を上書き。
- `Enrollment` / `Portfolio` の `UserId` FK は `OnDelete=Restrict` で配線 (ユーザ削除時に既存受講・ポートフォリオを誤って消さないため)。後段でユーザ削除時の振る舞いをユースケース単位で再設計する可能性あり。
- migration は 2 本に分割: `AddIdentity` (Identity テーブル) → `AddUserForeignKeys` (既存テーブル → AspNetUsers の FK)。後者を分けることで Identity テーブル作成と FK 配線を別 commit に保ち、TDD step の意図に合わせた。

## テスト戦略

- **種別**: integration
- **対象**: Identity DI 構成・ユーザ作成
- **フィクスチャ**: `WebApplicationFactory<Program>` + InMemory or Sqlite テスト DB
- **TDD 順序**:
  1. 失敗テスト: `UserManager_CanCreateAndAuthenticateUser`
  2. Identity 登録 → migration
  3. ApplicationUser へのプロパティ追加

## 受入基準

- [x] `dotnet test` で Identity 統合テストがグリーン (37 passed)
- [x] migration が冪等に適用できる (`dotnet ef database update` 再実行で `__EFMigrationsHistory` を見て既適用は no-op)
- [x] `Enrollment.UserId` / `Portfolio.UserId` の FK が `AspNetUsers.Id` を参照する (`AddUserForeignKeys` migration で確認)

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
