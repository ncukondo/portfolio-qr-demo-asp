---
id: T000
title: .gitignore 追加と build artifact untrack
phase: 1
status: done
depends_on: []
spec_refs: []
layer: infra
---

## 目的

リポジトリに `.gitignore` がないため、`bin/` / `obj/` などの build 成果物がコミットされ、`dotnet build` のたびに git status が汚れている。標準的な .NET 用 .gitignore を追加し、既にコミットされている artifact を untrack する。

## 背景

`devcontainer-lock.json` も新たに生成されたため、コミット方針 (採用) を確定する。

## 作業内容

- [x] `.gitignore` を追加 (bin/obj、IDE、secrets、OS、test results 等)
- [x] `git rm -r --cached bin/ obj/` で既にコミットされている artifact を untrack
- [x] `.devcontainer/devcontainer-lock.json` をコミット (再現性のため)
- [x] T000 を `docs/roadmap.md` に追加
- [x] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: smoke (手動確認)
- **対象**: ビルド後に git status が汚れないこと
- 受入確認:
  1. `dotnet restore` 後に `git status` がクリーン
  2. `dotnet build` 後に `git status` がクリーン (bin/obj が ignore されている)
  3. `.gitignore` を通る項目が `git check-ignore` で確認できる

## 受入基準

- [x] `.gitignore` が存在する
- [x] `git ls-files | grep -E "^(bin|obj)/"` が何も返さない
- [x] devcontainer 内で `dotnet build` 後に `git status` が clean
- [x] `.devcontainer/devcontainer-lock.json` がコミットされている

## 想定 commit 列

```
chore: T000 add standard .NET .gitignore
chore: T000 untrack committed bin/obj build artifacts
chore: T000 commit devcontainer-lock.json
docs: T000 add task file and update roadmap
docs: T000 mark task done
```

## 影響範囲

- 追加: `.gitignore`, `docs/tasks/T000-add-gitignore.md`, `.devcontainer/devcontainer-lock.json`
- 変更: `docs/roadmap.md`
- 削除 (untrack): `bin/`, `obj/` 以下
