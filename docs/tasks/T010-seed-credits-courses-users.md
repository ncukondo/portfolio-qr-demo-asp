---
id: T010
title: Credits / Courses / Sample users の seed 移植
phase: 2
status: done
depends_on: [T009]
spec_refs: ["5.1"]
layer: infra
---

## 目的

PHP の `database/seeds/*.sql` を ASP.NET 側に移植し、`dotnet run` (Development) 時にデモ用データが投入される状態にする。

## 背景 / 元コード参照

- `database/seeds/001_credits_seed.sql`: 8 件 (`IT001`, `IT002`, `BZ001`, `BZ002`, `LG001`, `LG002`, `SK001`, `SK002`)
- `database/seeds/002_users_seed.sql`: 5 ユーザー (`admin@example.com`, `owner@example.com`, `learner1@example.com`, `learner2@example.com`, `multi@example.com`) + ロール割り当て (`multi` は class-owner + learner の二重ロール)
- `database/seeds/003_classes_seed.sql`: 5 クラス
- `database/seeds/004_class_credits_seed.sql`: クラス×単位の関連 (amount 付き)

## 作業内容

- [ ] `Infrastructure/Seeding/CreditSeeder.cs`: `IT001` 〜 `SK002` を upsert
- [ ] `Infrastructure/Seeding/SampleUserSeeder.cs`: PHP と同じ 5 ユーザーをパスワード `Password123!` (Identity の強度ポリシーを満たす) で `UserManager` 経由で作成。ロール付与。
- [ ] `Infrastructure/Seeding/SampleCourseSeeder.cs`: 5 クラスを Course + ClassCredit で作成 (冪等: `ClassName` で existing check)
- [ ] `Program.cs` で Development の seed パイプラインに 3 つを追加 (RoleSeeder → AdminSeeder → CreditSeeder → SampleUserSeeder → SampleCourseSeeder)
- [ ] Sample seeders は `appsettings.Development.json` の `Seeding:LoadSampleData = true` で有効化
- [ ] タスクファイルの status を `done` に更新する commit を含める

### 注意

- PHP の `password_hash` は固定 (`$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi` = `password`)。ASP.NET の Identity ハッシュ形式は異なるため、`UserManager.CreateAsync` でパスワードを設定する方式に置換する。デモパスワードは `Password123!` 等の強度ポリシー準拠に変更し、ログインページ (T021) に表示する。
- `SampleCourseSeeder` は `T009` の `ClassCredit.Amount (decimal)` を使う。

## テスト戦略

- **種別**: integration (`xunit` + Postgres / InMemory)
- **対象**: 各 seeder の冪等性 (2 回実行しても重複しない)
- **フィクスチャ**: 既存 `WebApplicationFactory` ベース or 独立 DbContext

## 受入基準

- [ ] `dotnet test` で seeder テストグリーン
- [ ] `dotnet run` 後、`/Courses` (T012) でサンプル 5 件が表示される
- [ ] サンプル user でログイン可能

## 想定 commit 列

```
test: T010 add failing test for CreditSeeder idempotency
feat: T010 implement CreditSeeder
test: T010 add failing test for SampleUserSeeder
feat: T010 implement SampleUserSeeder
feat: T010 implement SampleCourseSeeder
chore: T010 wire sample seeders behind LoadSampleData flag
docs: T010 mark task done
```

## 影響範囲

- 追加: `Infrastructure/Seeding/CreditSeeder.cs`, `SampleUserSeeder.cs`, `SampleCourseSeeder.cs`, `SeedingOptions.cs`
- 変更: `Program.cs`, `appsettings.Development.json`
