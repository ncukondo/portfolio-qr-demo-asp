---
id: T000
title: <タスクのタイトル>
phase: 1
status: todo            # todo | in_progress | review | done
depends_on: []          # [T001, T002] のように記述
spec_refs: []           # SPECIFICATIONS.md の節番号 ["4.1", "5.1"]
layer: domain           # domain | service | repository | page | infra | docs
---

## 目的

このタスクで何を達成するか。1〜3文。

## 背景 / 元コード参照

PHP 元リポジトリ (`ncukondo/portfolio-qr-demo`) の参照箇所、関連する設計判断、依存タスクとの関係。

## 作業内容

- [ ] 具体的な手順 1
- [ ] 具体的な手順 2
- [ ] タスクファイルの status を `done` に更新する commit を含める

## テスト戦略

- **種別**: unit / integration / e2e
- **対象**: テストするクラス・関数
- **フィクスチャ**: 使用する fixture / fake / testcontainer
- **TDD 順序**: 失敗テスト追加 commit → 最小実装 commit → リファクタ commit

## 受入基準

- [ ] `dotnet build` が成功
- [ ] `dotnet test` が成功 (関連テストが全グリーン)
- [ ] (追加の受入条件)

## 想定 commit 列

```
test: T000 add failing test for <X>
feat: T000 implement <X> (minimal)
refactor: T000 extract <Y>
docs: T000 mark task done
```

## 影響範囲

- 追加/変更: <ファイル列>
- 既存への影響: <あれば>
