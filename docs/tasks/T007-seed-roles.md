---
id: T007
title: ロール (Admin/Organizer/Participant) seeding
phase: 1
status: todo
depends_on: [T006]
spec_refs: ["2.4", "8.1"]
layer: service
---

## 目的

仕様書で定義された 3 ロール (`Admin` / `Organizer` / `Participant`) をアプリ起動時に冪等に作成する seeder を実装する。初期管理者ユーザの作成も同時に行う (環境変数 or appsettings から credentials を読み込み)。

## 背景 / 元コード参照

ロールがなければ後続の認可テストが書けない。冪等性は重要 (起動の度に重複作成しないこと)。

## 作業内容

- [ ] `Infrastructure/Seeding/RoleSeeder.cs` を追加 (`RoleManager<IdentityRole>` 依存)
- [ ] `Infrastructure/Seeding/AdminSeeder.cs` を追加 (`UserManager<ApplicationUser>` 依存、初期 admin の email/password を `IConfiguration` から取得)
- [ ] `Program.cs` で起動時に scope を作成し seeder を呼ぶ (Dev 環境のみで実行するか判断しドキュメント化)
- [ ] 単体テスト: `RoleSeeder.SeedAsync` が
  - ロール未作成時に 3 件作る
  - 既存ロールがあれば再作成しない (冪等)
- [ ] 単体テスト: `AdminSeeder` が
  - 設定が空ならスキップ
  - 設定があれば作成し `Admin` ロールに割り当てる
- [ ] タスクファイルの status を `done` に更新する commit を含める

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

- [ ] `dotnet test` で seeder テストが全グリーン
- [ ] アプリ起動を 2 回繰り返してもロール・admin が重複作成されない
- [ ] 環境変数 `Seed__Admin__Email` 等の設定で admin が作られることを README に追記

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
