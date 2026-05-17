---
id: T001
title: NuGet パッケージ追加
phase: 1
status: done
depends_on: []
spec_refs: ["4.1"]
layer: infra
---

## 目的

EF Core / PostgreSQL / Identity / QRCoder など、後続タスクが依存するパッケージを `DoctorPortfolioSite.csproj` に追加する。

## 背景 / 元コード参照

現状 `.csproj` はパッケージ参照ゼロ。仕様書 4.1 に列挙されている技術スタックを満たす必要がある。

## 作業内容

- [x] `Microsoft.EntityFrameworkCore` 関連を追加
  - `Microsoft.EntityFrameworkCore` 8.0.27
  - `Microsoft.EntityFrameworkCore.Design` 8.0.27
  - `Npgsql.EntityFrameworkCore.PostgreSQL` 8.0.11
- [x] `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 8.0.27 を追加
- [x] `Microsoft.AspNetCore.Identity.UI` 8.0.27 を追加 (Razor Pages 用)
- [x] `QRCoder` 1.8.0 を追加
- [x] `dotnet restore` が成功することを確認
- [x] `dotnet build` が成功することを確認 (0 Warning / 0 Error)
- [x] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: build smoke
- パッケージ追加のみのためテストコードは新規追加なし。`dotnet build` 成功で受入とする。

## 受入基準

- [x] `dotnet restore` が成功する
- [x] `dotnet build` が成功する
- [x] `dotnet list package` で上記パッケージが列挙される

## 想定 commit 列

```
chore: T001 add EF Core / Npgsql / Identity / QRCoder packages
docs: T001 mark task done
```

## 影響範囲

- 変更: `DoctorPortfolioSite.csproj`
