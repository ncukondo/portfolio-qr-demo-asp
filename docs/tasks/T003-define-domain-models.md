---
id: T003
title: ドメインモデル定義 (Course / Enrollment / Portfolio / QRCode)
phase: 1
status: todo
depends_on: [T001, T002]
spec_refs: ["5.1"]
layer: domain
---

## 目的

仕様書 5.1 で定義されている `Courses` / `Enrollments` / `Portfolios` / `QRCodes` を pure C# クラスとして `Domain/` 配下に定義する。`Users` は Identity と一体化させるため T006 で扱う。

## 背景 / 元コード参照

PHP 元リポジトリ `database/` および `src/` のモデル相当部分を参照しつつ、ASP.NET Core 流の POCO + DataAnnotations へ落とし込む。EF Core 依存をモデルに混ぜず、純粋に単体テスト可能な形に保つ。

## 作業内容

- [ ] `Domain/Models/Course.cs` を追加 (Title / Description / Credits / StartDate / EndDate / Venue / MaxParticipants / QRCodeSecret / 監査列)
- [ ] `Domain/Models/Enrollment.cs` を追加 (UserId / CourseId / EnrolledAt / CompletedAt / Status enum / QRCodeScannedAt)
- [ ] `Domain/Models/EnrollmentStatus.cs` enum を追加 (Enrolled / Completed / Cancelled)
- [ ] `Domain/Models/Portfolio.cs` を追加
- [ ] `Domain/Models/QrToken.cs` を追加 (CourseId / Token / ExpiresAt / IsUsed)
- [ ] 不変条件をモデル内のメソッドで表現 (例: `Enrollment.MarkCompleted(DateTimeOffset now)`)
- [ ] `tests/DoctorPortfolioSite.Tests/Domain/` に各モデルの単体テストを追加
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: unit
- **対象**: 各モデルの不変条件・状態遷移メソッド
- **フィクスチャ**: 不要 (pure C#)
- **TDD 順序**:
  1. 失敗テスト: `Enrollment.MarkCompleted_SetsCompletedAt`
  2. 最小実装: メソッド追加
  3. リファクタ: 重複バリデーションを抽出

## 受入基準

- [ ] `dotnet test` で新規テストが全てグリーン
- [ ] モデルクラスが EF Core / Identity への直接参照を持たない (pure C#)
- [ ] 各モデルに状態遷移 or 不変条件を表すメソッドが最低1本存在する

## 想定 commit 列

```
test: T003 add failing tests for Course/Enrollment invariants
feat: T003 add Course/Enrollment/Portfolio/QrToken models
test: T003 add tests for QrToken expiration
feat: T003 implement QrToken.IsExpired
refactor: T003 extract shared validation helpers
docs: T003 mark task done
```

## 影響範囲

- 追加: `Domain/Models/*.cs`, `tests/DoctorPortfolioSite.Tests/Domain/*.cs`
