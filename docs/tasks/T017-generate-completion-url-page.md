---
id: T017
title: 完了URL+QR 生成 Page (複数クラス対応)
phase: 2
status: done
depends_on: [T016, T011]
spec_refs: ["2.2", "7.3"]
layer: page
---

## 目的

PHP `public/generate-completion-url.php` を移植。Organizer / Admin が複数クラスをチェックして「完了 URL」を生成、その場で QR コード画像 (PNG base64) を表示・ダウンロードできるようにする。

## 背景 / 元コード参照

PHP:
- POST: `class_ids[]`, `expiration_hours` (1〜8760), `csrf_token`
- バリデーション後、`ClassCompletionTokenService->generateCompletionUrl($classIds, $baseUrl, $expirationHours)`
- QRコードラベル: 選択クラス名の最初の 2 件 + 「他 N 件」

## 作業内容

- [ ] `Pages/Courses/CompletionUrl.cshtml` + `.cshtml.cs`
- [ ] `[Authorize(Roles = "Admin,Organizer")]`
- [ ] `OnGetAsync`: 全 Course を `ICourseService.ListAsync` で取得、チェックボックスで表示
  - T015 から TempData で `PrefilledClassIds` が来ていれば初期チェック
- [ ] `OnPostAsync`:
  - `SelectedClassIds (int[])`, `ExpirationHours (int, 1〜8760)`
  - `IClassCompletionTokenService.BuildCompletionUrl` で URL 生成
  - `IQrCodeService.GenerateDataUrl(url)` で QR の data URL を生成 (`QRCoder` ライブラリ)
- [ ] `IQrCodeService` を新規追加 (`Infrastructure/Qr/QrCodeService.cs`)。`PngByteQRCode` で base64
- [ ] 結果表示: URL を読み専用 textarea + コピー JS + `<img src="data:image/png;base64,...">`
- [ ] タスクファイルの status を `done` に更新する commit を含める

### 設計メモ

- `baseUrl` は `Request.Scheme + "://" + Request.Host` で生成
- antiforgery は Razor Page デフォルトで OK

## テスト戦略

- **種別**: e2e
- **対象**:
  - Organizer で GET → 200 + course list
  - POST 正常 → URL が body に含まれ、img の src が `data:image/png;base64,` で始まる
  - `ExpirationHours = 0` → ModelState error
- **フィクスチャ**: Course を 3 件 seed

## 受入基準

- [ ] `dotnet test` グリーン
- [ ] 生成された URL を T019 (Complete) に渡すと完了登録が動く (E2E は T019 で書く)

## 想定 commit 列

```
test: T017 add failing test for /Courses/CompletionUrl authorization
feat: T017 scaffold CompletionUrl page
test: T017 add failing test for QR generation
feat: T017 implement QrCodeService (QRCoder wrapper)
feat: T017 implement OnPostAsync
test: T017 add failing test for expiration boundary
docs: T017 mark task done
```

## 影響範囲

- 追加: `Pages/Courses/CompletionUrl.cshtml(+.cs)`, `Infrastructure/Qr/IQrCodeService.cs`, `QrCodeService.cs`
- 変更: `Program.cs`
