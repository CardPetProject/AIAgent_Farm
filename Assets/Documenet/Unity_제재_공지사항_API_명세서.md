# Unity 제재 및 공지사항 API 명세서

작성일: 2026-05-12
대상: Unity 클라이언트 담당자

## 1. 서버 주소

- 서버 IP: `54.116.59.82`
- 기본 주소: `http://54.116.59.82/api/v1`
- 프로토콜: HTTP

## 2. 사용자 제재 정보

제재 정보는 별도 API로 요청하지 않습니다.
Steam 로그인 API 응답에 `canPlay`와 `sanction` 객체가 함께 내려갑니다.
제재 단계값(`ONE_DAY`, `SEVEN_DAYS` 등)은 클라이언트로 내려가지 않습니다.

### 요청 주소

```http
POST http://54.116.59.82/api/v1/game/auth/steam
Content-Type: application/json
```

### 요청 바디

```json
{
  "ticket": "HEX_ENCODED_STEAM_TICKET",
  "identity": "farmverse",
  "personaName": "SteamNickname"
}
```

### 요청 필드

| 필드명 | 타입 | 필수 | 설명 |
|---|---|---:|---|
| `ticket` | string | O | Steam Web API 티켓을 hex string으로 변환한 값 |
| `identity` | string | O | Steam 티켓 identity. 현재 값은 `farmverse` |
| `personaName` | string | X | Steam 표시 이름. DB 표시용이며 인증에는 사용하지 않음 |

### 정상 유저 응답 예시

```json
{
  "message": "Steam 로그인에 성공했습니다.",
  "accessToken": "BACKEND_JWT",
  "appUserId": "USR-xxxx",
  "steamId": "7656119xxxxxxxxxx",
  "displayName": "SteamNickname",
  "sessionId": "SESSION-xxxx",
  "canPlay": true,
  "sanction": {
    "reason": "-",
    "endsAt": null
  }
}
```

### 제재 유저 응답 예시

```json
{
  "message": "Steam 로그인에 성공했습니다.",
  "accessToken": "BACKEND_JWT",
  "appUserId": "USR-xxxx",
  "steamId": "7656119xxxxxxxxxx",
  "displayName": "SteamNickname",
  "sessionId": "SESSION-xxxx",
  "canPlay": false,
  "sanction": {
    "reason": "비정상 재화 복제",
    "endsAt": "2026-05-19T12:00:00Z"
  }
}
```

### 응답 필드

| 필드명 | 타입 | 설명 |
|---|---|---|
| `accessToken` | string | 이후 게임 API 요청에 사용할 Bearer 토큰 |
| `appUserId` | string | 백엔드 사용자 ID |
| `steamId` | string | Steam 사용자 ID |
| `displayName` | string | 표시 이름 |
| `sessionId` | string | 서버 발급 세션 ID |
| `canPlay` | boolean | 게임 진입 가능 여부. 클라이언트는 이 값을 기준으로 진입 차단 처리 |
| `sanction.reason` | string | 제재 사유 |
| `sanction.endsAt` | string \| null | 제재 종료 시각, ISO-8601 UTC. 영구 제재는 `null` |

기간제 제재는 로그인 시점에 서버가 만료 여부를 확인합니다.
만료된 제재는 서버에서 자동으로 해제된 뒤 `canPlay: true`로 응답됩니다.

### Unity 처리 기준

1. Steam 로그인 요청을 보냅니다.
2. 응답의 최상위 `canPlay`를 확인합니다.
3. `canPlay == false`이면 게임 진입을 막고, `sanction.reason`, `sanction.endsAt`을 팝업에 표시합니다.
4. `canPlay == true`이면 `accessToken`을 저장하고 게임에 진입합니다.

## 3. 공지사항

공지사항은 인증 없이 조회할 수 있습니다.
현재 공지는 하나만 사용하므로 응답도 제목과 내용만 내려갑니다.

### 요청 주소

```http
GET http://54.116.59.82/api/v1/game/notices
```

### 요청 헤더

별도 인증 헤더가 필요 없습니다.

### 응답 예시

```json
{
  "title": "얼리억세스",
  "content": "0.9.0"
}
```

### 공지가 없을 때

```json
{
  "title": "",
  "content": ""
}
```

### 응답 필드

| 필드명 | 타입 | 설명 |
|---|---|---|
| `title` | string | 공지 제목 |
| `content` | string | 공지 내용 |

### Unity 처리 기준

1. 게임 시작 시점이나 공지 팝업을 열 때 `GET /game/notices`를 호출합니다.
2. `title` 또는 `content`가 비어 있지 않으면 공지 팝업을 표시합니다.
3. 둘 다 빈 문자열이면 표시하지 않습니다.
