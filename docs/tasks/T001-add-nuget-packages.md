---
id: T001
title: NuGet パッケージ追加
phase: 1
status: todo
depends_on: []
spec_refs: ["4.1"]
layer: infra
---

## 目的

EF Core / PostgreSQL / Identity / QRCoder など、後続タスクが依存するパッケージを `DoctorPortfolioSite.csproj` に追加する。

## 背景 / 元コード参照

現状 `.csproj` はパッケージ参照ゼロ。仕様書 4.1 に列挙されている技術スタックを満たす必要がある。

## 作業内容

- [ ] `Microsoft.EntityFrameworkCore` 関連を追加
  - `Microsoft.EntityFrameworkCore`
  - `Microsoft.EntityFrameworkCore.Design`
  - `Npgsql.EntityFrameworkCore.PostgreSQL`
- [ ] `Microsoft.AspNetCore.Identity.EntityFrameworkCore` を追加
- [ ] `Microsoft.AspNetCore.Identity.UI` を追加 (Razor Pages 用)
- [ ] `QRCoder` を追加
- [ ] `dotnet restore` が成功することを確認
- [ ] `dotnet build` が成功することを確認
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: build smoke
- パッケージ追加のみのためテストコードは新規追加なし。`dotnet build` 成功で受入とする。

## 受入基準

- [ ] `dotnet restore` が成功する
- [ ] `dotnet build` が成功する
- [ ] `dotnet list package` で上記パッケージが列挙される

## 想定 commit 列

```
chore: T001 add EF Core / Npgsql / Identity / QRCoder packages
docs: T001 mark task done
```

## 影響範囲

- 変更: `DoctorPortfolioSite.csproj`
