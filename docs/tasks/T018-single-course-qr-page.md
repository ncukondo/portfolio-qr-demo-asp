---
id: T018
title: 単一クラス QR Page (`/Courses/{id}/QrCode`)
phase: 2
status: done
depends_on: [T016, T011]
spec_refs: ["2.2", "7.3"]
layer: page
---

## 目的

PHP `public/generate-qr-code.php?class_id=X` を移植。Course 一覧 (T012) のカードから 1 クリックで遷移できる、特定 1 クラスの完了 URL + QR 専用ページ。

## 背景 / 元コード参照

PHP は `?class_id` クエリ受取、有効期限 24h 固定で `[classId]` を入れた JWT を生成、QR を表示。

## 作業内容

- [ ] `Pages/Courses/QrCode.cshtml` + `.cshtml.cs` を `@page "{id:int}/QrCode"` で
- [ ] `[Authorize(Roles = "Admin,Organizer")]`
- [ ] `OnGetAsync(int id)`:
  - `ICourseService.GetByIdAsync(id)` → null なら 404
  - `IClassCompletionTokenService.BuildCompletionUrl(new[]{id}, baseUrl, 24)`
  - `IQrCodeService.GenerateDataUrl(url)`
  - ViewModel に `Course`, `CompletionUrl`, `QrDataUrl` を詰める
- [ ] 「印刷」「コピー」ボタン (JS, インライン script で OK)
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: e2e
- **対象**:
  - 存在しない id → 404
  - Organizer で GET → 200 + QR の data URL + クラス名表示
  - Participant で GET → 403
- **フィクスチャ**: Course 1 件 seed

## 受入基準

- [ ] `dotnet test` グリーン
- [ ] T012 のカードからリンクして到達可能

## 想定 commit 列

```
test: T018 add failing test for /Courses/{id}/QrCode authorization
test: T018 add failing test for 404 on missing course
feat: T018 implement QrCode page
docs: T018 mark task done
```

## 影響範囲

- 追加: `Pages/Courses/QrCode.cshtml(+.cs)`, テスト
- 変更: `Pages/Courses/Index.cshtml` (リンク追加)
