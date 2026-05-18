---
id: T011
title: CourseService (CRUD + 一覧 + クレジット関連付け)
phase: 2
status: done
depends_on: [T009]
spec_refs: ["7.2"]
layer: service
---

## 目的

PHP `src/Models/ClassModel.php` のクエリと、`register-class.php` / `bulk-import-classes.php` の中に散在する INSERT ロジックを、サービスクラス `CourseService` として 1 か所に集約する。Razor Page から DI 経由で呼ぶ。

## 背景 / 元コード参照

PHP `ClassModel`:
- `create(array $data)`: classes に 1 行追加
- `findById(int $id)`, `findAll()`, `findByOrganizer(string)`, `findByDateRange(start, end)`: classes と class_credits / credits を LEFT JOIN して credits 配列付きで返す
- `update(int $id, array $data)`, `delete(int $id)`

PHP は class_credits を `register-class.php` 内で個別 INSERT しているが、ASP.NET 側ではサービスのトランザクション内で完結させる。

## 作業内容

- [ ] `Application/Courses/ICourseService.cs` + `CourseService.cs` を追加
- [ ] DTO `CourseDto` / `CourseDetailsDto` (Course + 関連 Credits)
- [ ] メソッド:
  - `Task<int> CreateAsync(CreateCourseInput input, CancellationToken)`: Course + ClassCredit を 1 トランザクションで作成。`creditCodes` (string[]) を受け取り内部で `Credit.Id` 解決
  - `Task<CourseDetailsDto?> GetByIdAsync(int id, CancellationToken)`
  - `Task<IReadOnlyList<CourseDto>> ListAsync(CourseQuery query, CancellationToken)`: organizer / 期間でフィルタ可
  - `Task<bool> UpdateAsync(int id, UpdateCourseInput input, CancellationToken)`
  - `Task<bool> DeleteAsync(int id, CancellationToken)` — class_credits は cascade で削除
- [ ] `Program.cs` で `AddScoped<ICourseService, CourseService>()`
- [ ] タスクファイルの status を `done` に更新する commit を含める

### 設計メモ

- `CreateCourseInput` で受け取る `CreditCodes` (string[]) は service 内で `Credit.Code` → `Credit.Id` に解決。未知の code は黙って無視 (PHP と同じ挙動)。
- ソート: `event_datetime ASC` (PHP `findAll` と同じ)

## テスト戦略

- **種別**: integration (`InMemory` DbContext で十分。decimal や JOIN は本筋でないため Postgres まで持っていかない)
- **対象**: 6 メソッドそれぞれ
- **フィクスチャ**: `DbContextOptionsBuilder<ApplicationDbContext>().UseInMemoryDatabase(...)` ベースの test helper
- **TDD 順序**: 失敗テスト追加 → 実装 → 別メソッドの失敗テスト → 実装 …

## 受入基準

- [ ] `dotnet test` でサービステストグリーン
- [ ] `CreateAsync` 失敗時 (バリデーション) は DB 変更が発生しない (トランザクション)

## 想定 commit 列

```
test: T011 add failing test for CourseService.CreateAsync with credit codes
feat: T011 implement CourseService.CreateAsync
test: T011 add failing test for CourseService.GetByIdAsync
feat: T011 implement CourseService.GetByIdAsync
test: T011 add failing tests for List/Update/Delete
feat: T011 implement remaining methods
chore: T011 register CourseService in DI
docs: T011 mark task done
```

## 影響範囲

- 追加: `Application/Courses/*` (新フォルダ), テスト
- 変更: `Program.cs`
