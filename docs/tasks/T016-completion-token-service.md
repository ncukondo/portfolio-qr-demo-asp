---
id: T016
title: ClassCompletionToken サービス (JWT, stateless)
phase: 2
status: done
depends_on: [T009]
spec_refs: ["2.4", "7.3", "8.1"]
layer: service
---

## 目的

PHP `src/Services/ClassCompletionTokenService.php` を移植する。完了URL は DB に保存せず、JWT (HS256) として stateless に発行・検証する。

## 背景 / 元コード参照

PHP のペイロード:

```json
{
  "iss": "portfolio-system",
  "iat": 1700000000,
  "exp": 1700086400,
  "purpose": "class_completion",
  "class_ids": [1, 2, 3],
  "created_at": "2024-..."
}
```

- アルゴリズム: HS256
- 有効期限: デフォルト 24h、1〜8760h (=1年) で指定可
- 検証: `purpose === "class_completion"`, `class_ids` 配列であること, exp 未過去

## 作業内容

- [ ] NuGet `Microsoft.IdentityModel.Tokens` + `System.IdentityModel.Tokens.Jwt` を追加
- [ ] `Application/Tokens/ClassCompletionTokenService.cs`:
  - `string Generate(int[] classIds, int expirationHours = 24)`
  - `CompletionTokenPayload? TryDecode(string token)`
  - `bool IsValid(CompletionTokenPayload payload)`
  - `string BuildCompletionUrl(int[] classIds, string baseUrl, int expirationHours = 24)`
- [ ] 設定: `appsettings.json` に `Jwt:Secret` (Development では `dotnet user-secrets` 推奨だが demo なので appsettings.Development.json で OK)、`Jwt:Issuer = "portfolio-system"`
- [ ] `Program.cs` で `Services.Configure<JwtOptions>` + `AddScoped<IClassCompletionTokenService, ClassCompletionTokenService>`
- [ ] タスクファイルの status を `done` に更新する commit を含める

### 設計メモ

- PHP 互換のペイロード形式を維持する (purpose/class_ids キー名そのまま)。これは PHP 環境からのトークンで動作確認しやすくするため。
- 鍵長は HS256 で 256bit (32 bytes) 以上推奨

## テスト戦略

- **種別**: unit
- **対象**:
  - Generate → TryDecode で round-trip 成功
  - 別 secret で署名されたトークン → TryDecode が null
  - purpose が異なる token → TryDecode が null
  - exp 過去 → TryDecode が null (or 別フィールド `IsExpired = true` で返す)
  - BuildCompletionUrl の URL エンコード

## 受入基準

- [ ] `dotnet test` グリーン
- [ ] PHP 側で発行した token を ASP.NET 側で decode できる (手動検証メモを task に追記)

## 想定 commit 列

```
chore: T016 add JWT NuGet packages
test: T016 add failing test for token round-trip
feat: T016 implement ClassCompletionTokenService.Generate/TryDecode
test: T016 add failing tests for invalid/expired tokens
refactor: T016 extract JwtOptions
chore: T016 register service in DI
docs: T016 mark task done
```

## 影響範囲

- 追加: `Application/Tokens/*`, `Application/Tokens/JwtOptions.cs`, テスト
- 変更: `DoctorPortfolioSite.csproj`, `Program.cs`, `appsettings.json`, `appsettings.Development.json`
