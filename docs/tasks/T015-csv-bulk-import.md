---
id: T015
title: CSV 一括インポート Page (`/Courses/Import`)
phase: 2
status: done
depends_on: [T013, T014]
spec_refs: ["2.2"]
layer: page
---

## 目的

PHP `public/bulk-import-classes.php` を移植。Organizer / Admin が UTF-8 BOM 付き CSV をアップロードし、Course と ClassCredit を一括登録する。

## 背景 / 元コード参照

PHP の挙動:
- ファイルサイズ上限 5MB、拡張子 `.csv` のみ
- BOM をスキップ
- ヘッダ行検証 (7 列固定)
- 1 行ずつバリデーション:
  - class_name 必須 / organizer 必須
  - event_date は `strtotime` パース可
  - event_time は `HH:MM` 正規表現
  - duration_minutes は正の整数
- バリデーション失敗行はスキップして他は登録、結果一覧を表示
- 成功した class_ids を集めて、続けて「完了URL 生成へ」リンクを出す (T017 への遷移)

## 作業内容

- [ ] `Pages/Courses/Import.cshtml` + `.cshtml.cs`
- [ ] `[Authorize(Roles = "Admin,Organizer")]`
- [ ] `[BindProperty] public IFormFile CsvFile { get; set; }`
- [ ] 5MB 制限 (`builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 5 * 1024 * 1024)` or page-level `[RequestFormLimits]`)
- [ ] CSV パース: `System.IO.StreamReader` + `string.Split` で十分 (CsvHelper 等の追加依存は避ける。ただしクォート escape を扱う場合は最小ヘルパを書く)
- [ ] BOM (`﻿`) を読み飛ばす
- [ ] バリデーションは `CourseImportRowValidator` クラスに切り出し
- [ ] サービス層 `ICourseService.BulkCreateAsync(IEnumerable<CreateCourseInput>)` を追加 (or 単に foreach `CreateAsync`)
- [ ] 結果 ViewModel に `Row[]` (status: success/error, message, classId) を持たせる
- [ ] 成功 ClassId の配列を `TempData` に格納し、結果ページで「これらの完了URL を生成」リンクを表示 (T017 と連携)
- [ ] タスクファイルの status を `done` に更新する commit を含める

### 設計メモ

- 7 列のヘッダ名は PHP と同じ (`クラス名,説明,開催団体,開催日,開催時刻,時間（分）,単位コード（カンマ区切り）`)
- ヘッダ不一致なら 400 で即終了

## テスト戦略

- **種別**: unit + e2e
- **対象**:
  - unit: `CourseImportRowValidator` の各種失敗パターン
  - e2e: 正しい CSV を multipart で POST → 結果ページに success 行
  - e2e: ヘッダが違う CSV → エラー表示
- **フィクスチャ**: `MultipartFormDataContent` でテスト CSV を組み立てる

## 受入基準

- [ ] `dotnet test` グリーン
- [ ] PHP `test_import.csv` 相当のファイルがインポートできる

## 想定 commit 列

```
test: T015 add failing unit tests for CourseImportRowValidator
feat: T015 implement CourseImportRowValidator
test: T015 add failing e2e test for /Courses/Import happy path
feat: T015 implement Import page
test: T015 add failing test for header mismatch
refactor: T015 extract result view model
docs: T015 mark task done
```

## 影響範囲

- 追加: `Pages/Courses/Import.cshtml(+.cs)`, `Application/Courses/CourseImportRowValidator.cs`, テスト
- 変更: `Application/Courses/ICourseService.cs` (BulkCreate メソッド)
