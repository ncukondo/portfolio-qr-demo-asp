---
id: T009
title: ドメイン整合 — Credit/ClassCredit 追加・Course 列再設計・Completion 簡略化・QrToken 廃止
phase: 2
status: todo
depends_on: [T008]
spec_refs: ["5.1"]
layer: domain
---

## 目的

PHP 元 (`portfolio-qr-demo`) の `classes` / `credits` / `class_credits` / `user_class_completions` スキーマに合わせて、Phase 1 で先に作ったドメインモデル (`Course` / `Enrollment` / `QrToken`) を作り直す。Phase 2 以降の機能はこの整合したモデルの上に乗せる。

## 背景 / 元コード参照

- `database/migrations/002_create_classes_table.sql`: classes は `class_name / description / organizer(string) / event_datetime / duration_minutes` のみで、SPECIFICATIONS.md にあった `Venue` / `MaxParticipants` / `Credits(int)` / `QrCodeSecret` は無い。
- `database/migrations/003_create_credits_table.sql` + `004_create_class_credits_table.sql`: クラスと単位は多対多。`class_credits.credit_amount` は `DECIMAL(3,1)`。
- `database/migrations/005_create_user_class_completions_table.sql`: `user_id + class_id` が UNIQUE で「申込中」状態は存在しない。受講完了の登録のみ。
- `src/Services/ClassCompletionTokenService.php`: 完了URL は JWT (`class_ids[]` を payload に持つ stateless トークン)。DB テーブルとしての QrToken は不要。

## 作業内容

- [ ] `Course` を以下の形に再設計
  - `Id (int)` / `ClassName` / `Description` / `Organizer (string)` / `EventDateTime (DateTimeOffset)` / `DurationMinutes (int)` / `CreatedAt` / `UpdatedAt`
  - 列名: PHP に合わせて `class_name` / `organizer` / `event_datetime` / `duration_minutes` を `HasColumnName` で割当
- [ ] `Credit` エンティティ追加
  - `Id` / `Code (unique)` / `Label` / `Category` / `Description` / `CreatedAt` / `UpdatedAt`
- [ ] `ClassCredit` 中間エンティティ追加
  - `CourseId` + `CreditId` を複合キーまたは `Id` + 複合 UNIQUE、`Amount (decimal(3,1), default 1.0)`
  - `Course.Credits` (`ICollection<ClassCredit>`) ナビゲーションを追加
- [ ] `Enrollment` → `CourseCompletion` にリネーム & 簡略化
  - `Id` / `UserId` / `CourseId` / `CompletedAt` / `CreatedAt` / `UpdatedAt`
  - `(UserId, CourseId)` UNIQUE
  - `Enrolled` / `Cancelled` 状態は **削除** (PHP に無いため)
  - `EnrollmentStatus` enum は削除
- [ ] `QrToken` エンティティとテーブルを **廃止** (T016 で stateless JWT に置換)
- [ ] `Portfolio` エンティティは Phase 3 まで凍結 (テーブルは残す or 一旦削除して Phase 3 で再導入 — 削除を推奨)
- [ ] EF Core migration を 1 つ生成 (`AlignDomainWithPhp`): 既存テーブルを DROP & 新スキーマ作成
- [ ] `ApplicationDbContext` の `DbSet` を更新 (`Courses` / `Credits` / `ClassCredits` / `CourseCompletions`)
- [ ] 既存ユニットテスト (`Domain/CourseTests` 等) を新仕様に書き直す
- [ ] タスクファイルの status を `done` に更新する commit を含める

### 設計メモ

- `Course` ファクトリ `Create(className, description, organizer, eventDateTime, durationMinutes, now?)`. PHP に揃え `MaxParticipants` 等は持たない。
- `Credit.Amount` は per-class なので `Credit` ではなく `ClassCredit` 側に持つ。
- `CourseCompletion` は冪等登録 (`Register(userId, courseId, completedAt)`) を upsert で扱えるように service 層 (T011/T019) で対応。エンティティ自体は単純な値保持。
- ロール名は ASP.NET 流 (`Admin` / `Organizer` / `Participant`) を継続。マッピングは `RoleSeeder` の DocComment に追記する。

## テスト戦略

- **種別**: unit
- **対象**: `Course.Create`, `Credit.Create`, `ClassCredit.AssignTo(course, credit, amount)`, `CourseCompletion.Create`
- **フィクスチャ**: 純粋 C# (DB なし)
- **TDD 順序**: 既存テスト削除 → 新テスト追加 (失敗) → 新エンティティ実装 (グリーン) → migration 生成 → DbContext テスト追加

## 受入基準

- [ ] `dotnet build` 成功
- [ ] `dotnet test` 成功 (旧テスト削除、新テストグリーン)
- [ ] `Add-Migration AlignDomainWithPhp` 後、`Update-Database` が dev 環境で成功
- [ ] `_ViewImports` 等で旧 `Enrollment` を参照していた箇所がないこと

## 想定 commit 列

```
test: T009 add failing tests for new Course/Credit/ClassCredit/CourseCompletion
refactor: T009 redesign Course, drop Enrollment status machine
feat: T009 add Credit and ClassCredit entities
feat: T009 add CourseCompletion entity
chore: T009 drop QrToken and Portfolio entities/tables
chore: T009 generate AlignDomainWithPhp migration
docs: T009 mark task done
```

## 影響範囲

- 変更: `Domain/Models/Course.cs`, `Data/ApplicationDbContext.cs`, `Data/Configurations/*`, テスト
- 追加: `Domain/Models/Credit.cs`, `Domain/Models/ClassCredit.cs`, `Domain/Models/CourseCompletion.cs`, 新 migration
- 削除: `Domain/Models/Enrollment.cs`, `Domain/Models/EnrollmentStatus.cs`, `Domain/Models/QrToken.cs`, `Domain/Models/Portfolio.cs` (Phase 3 で再導入), 旧 `EnrollmentConfiguration.cs` / `QrTokenConfiguration.cs` / `PortfolioConfiguration.cs`
