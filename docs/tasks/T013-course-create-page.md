---
id: T013
title: Course 登録 Page (`/Courses/Create`, Organizer 権限)
phase: 2
status: done
depends_on: [T011]
spec_refs: ["2.2", "6.2"]
layer: page
---

## 目的

PHP `public/register-class.php` を Razor Page 化。Organizer / Admin だけが投稿可能。

## 背景 / 元コード参照

PHP の入力フォーム:
- クラス名 (必須)
- 説明
- 開催団体 (必須)
- 開催日 + 開催時刻 (HTML date / time、必須)
- 時間(分) — 正の数
- 単位コード (チェックボックス複数)

PHP は POST 成功時にフォームをクリア、エラー時は入力を保持。

## 作業内容

- [ ] `Pages/Courses/Create.cshtml` + `.cshtml.cs`
- [ ] `[Authorize(Roles = "Admin,Organizer")]`
- [ ] `[BindProperty] public CreateCourseInputModel Input { get; set; }` 
  - フィールド: `ClassName`, `Description`, `Organizer`, `EventDate`, `EventTime`, `DurationMinutes`, `CreditCodes (string[])`
  - DataAnnotations で必須・正数バリデーション
- [ ] `OnGetAsync()`: `ICourseService` 経由ではなく `Credit` 一覧を取得 (専用 `ICreditService` か `CreditRepository` を最小実装)
- [ ] `OnPostAsync()`: バリデーション → `ICourseService.CreateAsync` → 成功時はリダイレクト + TempData success message
- [ ] antiforgery (Razor Page デフォルト) を活用
- [ ] タスクファイルの status を `done` に更新する commit を含める

### 設計メモ

- `EventDate (DateOnly)` + `EventTime (TimeOnly)` を受け取り、PageModel 内で `DateTimeOffset` に結合 → service へ
- 失敗時はフォームに入力値を残し、`ModelState` のエラーをビューで表示

## テスト戦略

- **種別**: e2e
- **対象**:
  - 未認証 GET → `/Identity/Account/Login` リダイレクト
  - Participant でログイン → 403
  - Organizer でログイン → 200, antiforgery token 取得 → POST 成功 → 302
  - 不正入力 (`DurationMinutes = 0`) → 200 + ModelState error
- **フィクスチャ**: `TestWebApplicationFactory` + Credit seed

## 受入基準

- [ ] `dotnet test` グリーン
- [ ] CreateAsync が呼ばれ DB に Course + ClassCredit が作成される

## 想定 commit 列

```
test: T013 add failing test for /Courses/Create authorization
feat: T013 scaffold Create page with [Authorize]
test: T013 add failing test for successful POST creating course
feat: T013 implement OnPostAsync
test: T013 add failing test for validation errors
refactor: T013 extract CreditOptions view component
docs: T013 mark task done
```

## 影響範囲

- 追加: `Pages/Courses/Create.cshtml(+.cs)`, `Application/Credits/ICreditService.cs` (最小)
- 変更: `Pages/Shared/_Layout.cshtml`
