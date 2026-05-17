---
id: T002
title: テストプロジェクト作成
phase: 1
status: todo
depends_on: [T001]
spec_refs: []
layer: infra
---

## 目的

xUnit ベースのテストプロジェクト `DoctorPortfolioSite.Tests` を作成し、以降のタスクで TDD が即座に始められる状態にする。WebApplicationFactory を使った統合/E2E テスト基盤も準備する。

## 背景 / 元コード参照

このリポジトリにテストプロジェクトはまだ存在しない。TDD で進めるための前提整備。

## 作業内容

- [ ] ソリューションファイル `DoctorPortfolioSite.sln` を作成し、本体プロジェクトを追加
- [ ] `tests/DoctorPortfolioSite.Tests/` に xUnit プロジェクトを作成
  - `xunit`
  - `xunit.runner.visualstudio`
  - `Microsoft.NET.Test.Sdk`
  - `Microsoft.AspNetCore.Mvc.Testing` (WebApplicationFactory)
  - `Microsoft.EntityFrameworkCore.InMemory` (Service層のテスト用)
  - `FluentAssertions` (任意。アサーション可読性のため)
- [ ] テストプロジェクトから本体プロジェクトへの参照を追加
- [ ] サンプル smoke test `SmokeTests.cs` を1本書き、`WebApplicationFactory<Program>` で `/` が 200 を返すことを確認
- [ ] `Program.cs` を `partial class` 化 (テストから参照可能にする)
- [ ] `dotnet test` が成功することを確認
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: integration (smoke)
- **対象**: アプリケーション起動と `/` 応答
- **フィクスチャ**: `WebApplicationFactory<Program>`
- **TDD 順序**: smoke test を最初に書き、起動できることを確認 → 以降のタスクの足場とする

## 受入基準

- [ ] `dotnet test` でテストが1件以上実行され全てグリーン
- [ ] CI なしの段階でもローカルで再現可能

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
