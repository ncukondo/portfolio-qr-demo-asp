# 医師生涯学習ポートフォリオサイト 仕様書

## 1. システム概要

### 1.1 目的
医師の生涯学習を支援するポートフォリオサイト。管理者が学習コースを登録し、医師がQRコードスキャンで受講完了を記録できるシステム。

### 1.2 対象ユーザー
- **管理者**: 学習コースの管理、受講状況の確認
- **医師**: 学習履歴の記録、ポートフォリオの作成・管理

## 2. 機能要件

### 2.1 管理者機能
- **コース管理**
  - 学習コースの登録・編集・削除
  - コース情報（タイトル、説明、単位数、開催日時、会場）
  - QRコードの生成・管理
- **ユーザー管理**
  - 医師アカウントの作成・編集・削除
  - 権限管理（管理者・医師）
- **受講管理**
  - 受講状況の確認・一覧表示
  - 受講履歴の検索・フィルタリング
  - 受講証明書の発行
- **レポート機能**
  - 受講統計の表示
  - CSV/Excel形式でのデータエクスポート

### 2.2 医師機能
- **認証・プロフィール**
  - ログイン・ログアウト
  - プロフィール情報の編集
- **受講管理**
  - QRコードスキャンによる受講完了登録
  - 受講履歴の確認
  - 受講証明書・修了証の表示・ダウンロード
- **ポートフォリオ**
  - 学習履歴の整理・表示
  - 学習目標の設定・管理
  - 学習記録の追加・編集

### 2.3 システム機能
- **認証・認可**
  - ASP.NET Core Identity
  - 役割ベースのアクセス制御
- **QRコード**
  - QRコード生成・検証
  - ワンタイムトークン機能
- **セキュリティ**
  - HTTPS通信
  - CSRF対策
  - 入力値検証

## 3. 非機能要件

### 3.1 パフォーマンス
- レスポンス時間: 3秒以内
- 同時接続数: 100ユーザー

### 3.2 可用性
- 稼働率: 99%以上
- メンテナンス時間: 月1回、2時間以内

### 3.3 セキュリティ
- パスワード暗号化
- セッション管理
- 監査ログ

## 4. 技術仕様

### 4.1 技術スタック
- **フレームワーク**: ASP.NET Core 8.0
- **データベース**: PostgreSQL 15
- **ORM**: Entity Framework Core
- **認証**: ASP.NET Core Identity
- **フロントエンド**: Razor Pages + Bootstrap 5
- **QRコード**: QRCoder ライブラリ

### 4.2 開発環境
- **コンテナ**: Docker + devcontainer
- **IDE**: Visual Studio Code
- **バージョン管理**: Git

## 5. データベース設計

### 5.1 テーブル設計

#### Users（ユーザー）
- Id (Primary Key)
- Email
- PasswordHash
- Name
- MedicalLicenseNumber
- Role (Admin/Doctor)
- CreatedAt
- UpdatedAt

#### Courses（コース）
- Id (Primary Key)
- Title
- Description
- Credits
- StartDate
- EndDate
- Venue
- MaxParticipants
- QRCodeSecret
- CreatedAt
- UpdatedAt

#### Enrollments（受講記録）
- Id (Primary Key)
- UserId (Foreign Key)
- CourseId (Foreign Key)
- EnrolledAt
- CompletedAt
- Status (Enrolled/Completed/Cancelled)
- QRCodeScannedAt

#### Portfolios（ポートフォリオ）
- Id (Primary Key)
- UserId (Foreign Key)
- Title
- Description
- LearningGoals
- CreatedAt
- UpdatedAt

#### QRCodes（QRコード管理）
- Id (Primary Key)
- CourseId (Foreign Key)
- Token
- ExpiresAt
- IsUsed
- CreatedAt

## 6. 画面設計

### 6.1 管理者画面
- ダッシュボード
- コース一覧・登録・編集
- ユーザー一覧・管理
- 受講状況一覧
- レポート画面

### 6.2 医師画面
- ダッシュボード
- プロフィール編集
- 受講履歴
- QRコードスキャン
- ポートフォリオ編集

### 6.3 共通画面
- ログイン・ログアウト
- エラー画面
- アクセス拒否画面

## 7. API設計

### 7.1 認証API
- POST /api/auth/login
- POST /api/auth/logout
- GET /api/auth/profile

### 7.2 コース管理API
- GET /api/courses
- POST /api/courses
- GET /api/courses/{id}
- PUT /api/courses/{id}
- DELETE /api/courses/{id}

### 7.3 QRコードAPI
- POST /api/qr/generate
- POST /api/qr/verify
- POST /api/qr/scan

### 7.4 受講管理API
- GET /api/enrollments
- POST /api/enrollments
- GET /api/enrollments/{id}
- PUT /api/enrollments/{id}

## 8. セキュリティ要件

### 8.1 認証・認可
- JWT トークンによる認証
- 役割ベースのアクセス制御
- パスワード強度チェック

### 8.2 データ保護
- 個人情報の暗号化
- データベース接続の暗号化
- 監査ログの記録

### 8.3 通信セキュリティ
- HTTPS必須
- CSRF対策
- XSS対策

## 9. 運用要件

### 9.1 監視
- アプリケーションログ
- パフォーマンス監視
- エラー通知

### 9.2 バックアップ
- データベース日次バックアップ
- ファイル週次バックアップ
- 復旧手順の文書化

### 9.3 メンテナンス
- 定期的なセキュリティ更新
- パフォーマンス最適化
- 機能追加・改善

## 10. 開発スケジュール

### Phase 1: 基盤構築（2週間）
- 開発環境構築
- データベース設計・構築
- 認証システム構築

### Phase 2: 基本機能開発（4週間）
- ユーザー管理機能
- コース管理機能
- QRコード機能

### Phase 3: 応用機能開発（3週間）
- ポートフォリオ機能
- レポート機能
- UI/UX改善

### Phase 4: テスト・リリース（1週間）
- 統合テスト
- セキュリティテスト
- 本番環境デプロイ