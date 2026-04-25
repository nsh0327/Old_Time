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
- 타임필드 진입 시 중력 적용되어 낙하 시작
- 타임필드 이탈 시 그 자리에서 정지
- Ground 레이어에 닿으면 설정된 딜레이 후 삭제 또는 유지
- 기울어진 땅에서 물리 기반으로 자연스럽게 이동 가능

| 인스펙터 | 설명 |
|------|------|
| GravityScale | 낙하 속도 |
| FallDelay | 타임필드 진입 후 낙하 시작까지 대기 시간 |
| DestroyOnArrival | true: Ground 닿으면 삭제 / false: 그 자리에 유지 |
| DestroyDelay | 삭제까지 걸리는 시간 |

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
- 

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
