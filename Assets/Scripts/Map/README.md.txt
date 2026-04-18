# 맵 오브젝트 시스템

담당자: 노승현

## TimeFieldObject 타입
| 타입 | 설명 |
|------|------|
| ActivateAndLatch | 타임필드 최초 진입 시 1회 실행, 이후 상태 유지 |
| ActiveInsideField | 타임필드 안에서만 작동, 밖에서는 정지 |

## 오브젝트 목록

### Enemy
타입: ActiveInsideField
- 타임필드 진입 시 플레이어 방향으로 이동
- 타임필드 이탈 시 그 자리에서 정지
- (공격 구현 X, 체력 정해지면 구현 예정)
| 인스펙터 | 설명 |
|------|------|
| MoveSpeed | 이동 속도 |

---

### FallingObject
타입: ActiveInsideField
- 타임필드 진입 시 waypoint 경로대로 이동
- 타임필드 이탈 시 그 자리에서 정지
- 마지막 waypoint 도착 시 삭제 또는 정지

| 인스펙터 | 설명 |
|------|------|
| Waypoints | 이동 경로 (순서대로 등록) |
| MoveSpeed | 이동 속도 |
| DestroyOnArrival | true: 도착 시 삭제 / false: 그 자리에 유지 |

---

### Door
타입: ActiveInsideField
- 타임필드 진입 시 열림
- 타임필드 이탈 시 닫힘
- 버튼 연동 시 버튼이 눌린 상태에서만 열림

| 인스펙터 | 설명 |
|------|------|
| RequireButton | true: 버튼 필요 / false: 타임필드만으로 열림 |
| LinkedButton | 연결할 Button 오브젝트 |
| Animator | 열림/닫힘 애니메이션 연결 필요 |

---

### Button
타입: ActiveInsideField
- Rigidbody2D 있는 오브젝트가 올라오면 눌림
- 타임필드 안에서 눌린 상태면 연결된 Door 열림

---

### BreakableFloor
- FallingObject 충돌 시 부서짐
- 1초 후 삭제

| 인스펙터 | 설명 |
|------|------|
| Animator | 부서지는 애니메이션 연결 필요 |

---

### TilemapManager
- 타일맵 레이어 관리

| 레이어 | 설명 |
|------|------|
| Ground | 바닥, 벽 (콜라이더 있음) |