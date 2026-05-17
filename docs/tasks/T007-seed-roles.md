---
id: T007
title: ロール (Admin/Organizer/Participant) seeding
phase: 1
status: done
depends_on: [T006]
spec_refs: ["2.4", "8.1"]
layer: service
---

## 目的

仕様書で定義された 3 ロール (`Admin` / `Organizer` / `Participant`) をアプリ起動時に冪等に作成する seeder を実装する。初期管理者ユーザの作成も同時に行う (環境変数 or appsettings から credentials を読み込み)。

## 背景 / 元コード参照

ロールがなければ後続の認可テストが書けない。冪等性は重要 (起動の度に重複作成しないこと)。

## 作業内容

- [x] `Infrastructure/Seeding/RoleSeeder.cs` を追加 (`RoleManager<IdentityRole>` 依存、`DefaultRoles = [Admin, Organizer, Participant]`)
- [x] `Infrastructure/Seeding/AdminSeeder.cs` + `AdminSeedOptions` を追加 (`UserManager<ApplicationUser>` 依存、`Seed:Admin:Email` / `Password` / `Name` を `IConfiguration` から取得)
- [x] `Program.cs` で `IsDevelopment()` の scope 内で `RoleSeeder` → `AdminSeeder` を順に呼ぶ (本番では `dotnet run` 起動時の自動実行は無効。本番運用では手動実行コマンド or起動オプションを追加検討)
- [x] 単体テスト: `RoleSeeder.SeedAsync` が
  - ロール未作成時に 3 件作る
  - 既存ロールがあれば再作成しない (冪等)
- [x] 単体テスト: `AdminSeeder` が
  - 設定が空ならスキップ
  - 設定があれば作成し `Admin` ロールに割り当てる
  - 既に admin が存在する場合は再作成しない (冪等)
- [x] タスクファイルの status を `done` に更新する commit を含める

### 追加メモ

- 起動時自動 seeding は **Development 環境のみ**。本番では明示的に管理者作成スクリプトを走らせる運用が安全。
- 環境変数による admin seed 設定例 (devcontainer の `.env` や VS Code launch.json など):
  - `Seed__Admin__Email=admin@example.com`
  - `Seed__Admin__Password=StrongP@ss1`
  - `Seed__Admin__Name=Initial Admin`
- セッターの裏付けとして単体テスト (`SeederTestHost`) は InMemory EF + `AddIdentityCore<ApplicationUser>().AddRoles<IdentityRole>()` で構築。`AddDefaultTokenProviders` が `IDataProtectionProvider` を要求するため `services.AddDataProtection()` も登録している。
- README 作成はスキップ (システム規約上、明示的依頼が無い限り `*.md` の新規作成を避けるため)。本タスクファイルに seed 設定例を記載することで代替。

## テスト戦略

- **種別**: integration (Identity stores が必要なため)
- **対象**: `RoleSeeder` / `AdminSeeder`
- **フィクスチャ**: InMemory EF + 実 `RoleManager` / `UserManager` を組んだ ServiceProvider
- **TDD 順序**:
  1. 失敗テスト: `RoleSeeder_CreatesAllThreeRoles_WhenNonePresent`
  2. 最小実装
  3. 失敗テスト: `RoleSeeder_IsIdempotent_WhenCalledTwice`
  4. 実装に冪等チェック追加
  5. AdminSeeder のテスト追加

## 受入基準

- [x] `dotnet test` で seeder テストが全グリーン (42 passed: T006 までの 37 + Seeder 5)
- [x] アプリ起動を 2 回繰り返してもロール・admin が重複作成されない (冪等性テストで担保)
- [-] 環境変数 `Seed__Admin__Email` 等の設定例を本タスクファイルに記載 (README はシステム規約により未作成)

## 想定 commit 列

```
test: T007 add failing tests for RoleSeeder
feat: T007 implement RoleSeeder
test: T007 add idempotency test for RoleSeeder
fix: T007 make RoleSeeder idempotent
test: T007 add failing tests for AdminSeeder
feat: T007 implement AdminSeeder
chore: T007 wire seeders into Program startup
docs: T007 mark task done
```

## 影響範囲

- 追加: `Infrastructure/Seeding/*.cs`, テスト
- 変更: `Program.cs`, README (admin seed 設定の説明)
