---
id: T002
title: テストプロジェクト作成
phase: 1
status: done
depends_on: [T001]
spec_refs: []
layer: infra
---

## 目的

xUnit ベースのテストプロジェクト `DoctorPortfolioSite.Tests` を作成し、以降のタスクで TDD が即座に始められる状態にする。WebApplicationFactory を使った統合/E2E テスト基盤も準備する。

## 背景 / 元コード参照

このリポジトリにテストプロジェクトはまだ存在しない。TDD で進めるための前提整備。

## 作業内容

- [x] ソリューションファイル `DoctorPortfolioSite.sln` を作成し、本体プロジェクトを追加
- [x] `tests/DoctorPortfolioSite.Tests/` に xUnit プロジェクトを作成
  - `xunit` 2.5.3 (テンプレート既定)
  - `xunit.runner.visualstudio` 2.5.3
  - `Microsoft.NET.Test.Sdk` 17.8.0
  - `Microsoft.AspNetCore.Mvc.Testing` 8.0.27 (WebApplicationFactory)
  - `Microsoft.EntityFrameworkCore.InMemory` 8.0.27 (Service層のテスト用)
  - `FluentAssertions` 6.12.2 (MIT 最終バージョンを採用。7.x 以降はライセンス変更のため見送り)
- [x] テストプロジェクトから本体プロジェクトへの参照を追加
- [x] サンプル smoke test `SmokeTests.cs` を1本書き、`WebApplicationFactory<Program>` で `/` が 200 を返すことを確認
- [x] `Program.cs` を `partial class` 化 (テストから参照可能にする)
- [x] `dotnet test` が成功することを確認
- [x] タスクファイルの status を `done` に更新する commit を含める

### 追加メモ

- 本体 `DoctorPortfolioSite.csproj` (Microsoft.NET.Sdk.Web) がリポジトリ直下にあるため、デフォルトの compile glob が `tests/**/*.cs` まで拾ってしまい、`SmokeTests.cs` を本体側でもコンパイルしようとしてビルドエラーになった。対処として本体 csproj に `<DefaultItemExcludes>$(DefaultItemExcludes);tests/**</DefaultItemExcludes>` を追加。

## テスト戦略

- **種別**: integration (smoke)
- **対象**: アプリケーション起動と `/` 応答
- **フィクスチャ**: `WebApplicationFactory<Program>`
- **TDD 順序**: smoke test を最初に書き、起動できることを確認 → 以降のタスクの足場とする

## 受入基準

- [x] `dotnet test` でテストが1件以上実行され全てグリーン (1 passed)
- [x] CI なしの段階でもローカルで再現可能

## 想定 commit 列

```
chore: T002 add solution file and test project skeleton
test: T002 add smoke test for application bootstrap
feat: T002 make Program partial for test access
docs: T002 mark task done
```

## 影響範囲

- 追加: `DoctorPortfolioSite.sln`, `tests/DoctorPortfolioSite.Tests/*`
- 変更: `Program.cs` (末尾に `public partial class Program { }`)
