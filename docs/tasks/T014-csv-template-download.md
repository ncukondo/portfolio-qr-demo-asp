---
id: T014
title: CSV テンプレダウンロード (`/Courses/CsvTemplate`)
phase: 2
status: todo
depends_on: [T013]
spec_refs: ["2.2"]
layer: page
---

## 目的

PHP `public/download-csv-template.php` を移植。Organizer / Admin がクリックすると、UTF-8 BOM 付き CSV テンプレ (ヘッダ + サンプル 2 行) がダウンロードされる。

## 背景 / 元コード参照

PHP テンプレ:

```
クラス名,説明,開催団体,開催日,開催時刻,時間（分）,単位コード（カンマ区切り）
Web開発入門サンプル,HTML、CSS、JavaScriptの基礎を学ぶクラスのサンプル,技術研修センター,2024-12-01,10:00,120,IT001,IT002
データベース設計サンプル,PostgreSQLを使用したデータベース設計とSQL基礎のサンプル,データベース研究室,2024-12-05,14:00,90,IT002,BZ002
```

UTF-8 BOM (`\xEF\xBB\xBF`) を先頭に付与し、Excel で正しく開ける。

## 作業内容

- [ ] `Pages/Courses/CsvTemplate.cshtml.cs` で `OnGet` → `FileContentResult`
- [ ] `[Authorize(Roles = "Admin,Organizer")]`
- [ ] ファイル名: `class_template_{yyyy-MM-dd}.csv`
- [ ] Content-Type: `text/csv; charset=utf-8`
- [ ] BOM `﻿` を先頭に
- [ ] CSV 生成は `string.Join` で十分 (依存追加しない)。エスケープが必要な値は今のテンプレに無いが、念のため `"` 含む値はクォート + エスケープするヘルパを用意
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: e2e
- **対象**:
  - Organizer GET → 200, `Content-Type` が `text/csv`, body 先頭が BOM、行数が 3 行
  - Participant GET → 403
- **フィクスチャ**: `TestWebApplicationFactory`

## 受入基準

- [ ] `dotnet test` グリーン
- [ ] Excel / LibreOffice で文字化けせずに開けることを目視 (devcontainer 内では curl + xxd で BOM 確認)

## 想定 commit 列

```
test: T014 add failing test for CSV template authorization
test: T014 add failing test for BOM and headers
feat: T014 implement CsvTemplate handler
docs: T014 mark task done
```

## 影響範囲

- 追加: `Pages/Courses/CsvTemplate.cshtml.cs`, テスト
