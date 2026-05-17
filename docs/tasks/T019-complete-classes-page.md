---
id: T019
title: クラス完了処理 Page (`/Complete?token=...`, Participant 権限)
phase: 2
status: todo
depends_on: [T016, T009]
spec_refs: ["2.3", "7.3", "7.4"]
layer: page
---

## 目的

PHP `public/complete-classes.php` + `src/Controllers/ClassCompletionController.php` を移植。受講者が QR をスキャンすると遷移するエンドポイント。JWT を検証し、各 `class_id` について受講完了を idempotent に登録する。

## 背景 / 元コード参照

PHP の処理フロー:
1. `?token=...` を取り出し
2. `getTokenInfo` で検証 (無効 → エラー画面)
3. expired → エラー画面
4. 未ログイン → `/login?redirect=...` へリダイレクト
5. learner ロールでない → エラー画面
6. `class_ids` 配列ごとに `ClassModel.findById` で存在確認
7. 各 class について `UserClassCompletionModel.hasUserCompletedClass` → 重複は `alreadyCompleted` に、新規は `registerCompletion`
8. 結果ページ表示 (新規 / 既存 / エラー の 3 セクション)

## 作業内容

- [ ] `Pages/Complete.cshtml` + `.cshtml.cs` を `@page "/Complete"` で作成
- [ ] PageModel:
  - `OnGetAsync(string token)`:
    - token 空 / decode 失敗 / expired → エラー結果 (`Status = Invalid|Expired`)
    - 未認証 → `Challenge()` で Login にリダイレクト、ReturnUrl 維持 (`/Complete?token=...`)
    - Participant ロールでない → 403 (or Forbidden View)
    - 認可済み: `class_ids` ごとに DB チェック → `ICourseCompletionService.RegisterAsync(userId, courseId)` を呼び、結果を分類 (NewlyCompleted / AlreadyCompleted / NotFound / Error)
- [ ] `Application/CourseCompletions/ICourseCompletionService.cs` + `CourseCompletionService.cs` を追加
  - `Task<RegisterCompletionResult> RegisterAsync(string userId, int courseId, CancellationToken)`
  - upsert: `(UserId, CourseId)` UNIQUE で重複は `AlreadyCompleted` を返す
- [ ] 結果ビュー (3 セクション):
  - 新規受講完了 (緑)
  - 既に受講済み (青)
  - エラー / 見つからなかった class_id (赤)
- [ ] タスクファイルの status を `done` に更新する commit を含める

### 設計メモ

- `RegisterCompletionResult` enum: `Created` / `AlreadyExists` / `CourseNotFound`
- 一覧の表示は `_Layout` の "受講履歴へ" リンクから T020 へ

## テスト戦略

- **種別**: e2e
- **対象**:
  - token 不正 → 結果ページに `Invalid` 表示
  - expired token → `Expired` 表示
  - 未認証 → Login リダイレクト + redirect クエリ
  - Participant ログイン + 新規 token → 完了登録、DB に行追加、結果に "新規 1 件"
  - 同 token で再 GET → "既に受講済み 1 件"
  - Organizer ログイン + token → 403
- **フィクスチャ**: T016 で実装した token サービスでテスト時に有効 JWT を発行

## 受入基準

- [ ] `dotnet test` グリーン
- [ ] 冪等: 同 user × class での `RegisterAsync` 二回目は `AlreadyExists`

## 想定 commit 列

```
test: T019 add failing test for invalid token error display
test: T019 add failing test for unauthenticated redirect with returnUrl
test: T019 add failing test for non-participant forbidden
feat: T019 scaffold Complete page
test: T019 add failing test for CourseCompletionService.RegisterAsync upsert
feat: T019 implement CourseCompletionService
feat: T019 implement Complete page handlers and result view
docs: T019 mark task done
```

## 影響範囲

- 追加: `Pages/Complete.cshtml(+.cs)`, `Application/CourseCompletions/*`, テスト
- 変更: `Program.cs` (DI 登録)
