---
id: T020
title: 自分の受講履歴 Page (`/MyCompletions`)
phase: 2
status: done
depends_on: [T019]
spec_refs: ["2.3", "6.3"]
layer: page
---

## 目的

PHP `UserClassCompletionModel.getUserCompletions` 相当の表示ページ。ログイン中の Participant が自分の受講完了履歴 (クラス名・開催団体・開催日時・受講完了日時) を新しい順で見られる。

## 背景 / 元コード参照

PHP には専用ページが切り出されてはおらず、`ClassCompletionController->showCompletionResults` の最後で `/my-completions` への導線が示されている。本タスクでこのページを新規に Razor Page で作る。

## 作業内容

- [ ] `Pages/MyCompletions.cshtml` + `.cshtml.cs` を `@page "/MyCompletions"`
- [ ] `[Authorize]` (ロール問わず本人の履歴を見るだけなので Participant/Organizer/Admin すべて可)
- [ ] `ICourseCompletionService.GetUserCompletionsAsync(userId)` を追加:
  - `CourseCompletions` を Course と JOIN し、`completed_at DESC` で返す
- [ ] ビュー: テーブル (クラス名 / 開催団体 / 開催日時 / 受講完了日時 / 関連 Credits)
- [ ] 件数 0 のときは "まだ受講完了したクラスはありません" 表示
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: e2e
- **対象**:
  - 未認証 → ログインリダイレクト
  - Participant で 0 件 → "まだ受講…" 文言
  - Participant で 2 件登録済み → テーブルに 2 行
  - 他ユーザーの行は混入しない
- **フィクスチャ**: 2 ユーザー seed + 各々 completion を作る

## 受入基準

- [ ] `dotnet test` グリーン

## 想定 commit 列

```
test: T020 add failing test for empty-state display
test: T020 add failing test for listing only current user's completions
feat: T020 add GetUserCompletionsAsync to service
feat: T020 implement MyCompletions page
docs: T020 mark task done
```

## 影響範囲

- 追加: `Pages/MyCompletions.cshtml(+.cs)`, テスト
- 変更: `Application/CourseCompletions/ICourseCompletionService.cs`
