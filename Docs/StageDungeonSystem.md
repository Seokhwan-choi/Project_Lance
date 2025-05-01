# 🗺️ 스테이지 및 던전 시스템

본 문서는 Project_Lance의 스테이지와 던전 콘텐츠 설계 구조를 설명합니다.  
다양한 난이도와 보상 기반으로 구성된 스테이지와 특수 목적의 던전 시스템은 게임의 핵심 성장 루프를 구성합니다.

---

## ✅ 개요

### 난이도 구조

스테이지는 아래와 같은 순서로 난이도가 점점 상승합니다:

```
Beginner → Easy → Normal → Hard → Expert → Master → Hyper → Another → Legend → Hell → Chaos → Nightmare → Inferno → Void → Despair → Curse → Oblivion → Ruin → Darkness → Pain → Terror → Abyss → Eternal → Pandemonium → Cataclysm → Apocalypse → Fury → Wrath
```

- 각 난이도는 **5개의 챕터**로 구성됩니다.
- **챕터당 20개 스테이지**로 구성됩니다.
- 각 스테이지의 **보스를 처치하면 클리어**됩니다.
- 모든 챕터를 클리어해야 다음 난이도로 진입할 수 있습니다.

<p align="center">
  <img src="https://github.com/user-attachments/assets/187ffde2-2030-4a34-833d-be33308a136c" width="280"/>
</p>

---

## 🏰 던전 시스템

던전은 특수 재화나 보상을 획득할 수 있는 콘텐츠로, 일반 스테이지와 별도로 존재합니다.

| 던전명 | 최대 단계 | 주요 보상 |
|--------|-----------|------------|
| **골드 던전** | 200단계 | 대량의 골드 |
| **강화석 던전** | 200단계 | 강화석 |
| **재련석 던전** | 200단계 | 재련석 |
| **성장 던전** | 200단계 | 경험치, 장비 |
| **신수 먹이 던전** | 150단계 | 신수 먹이 |
| **레이드 던전** | - | 속성별 보스 3종 등장, 속성석 보상 (데미지 기반 분배) |
| **잊혀진 왕국** | 150단계 | 고대의 정수 |
| **제1 마계** | 80단계 | 장신구, 마나의 정수 |

<p align="center">
  <img src="https://github.com/user-attachments/assets/e52640d7-13ff-4b13-8b13-3a52c8f70f5e" width="280" style="margin-right: 16px;" />
  <img src="https://github.com/user-attachments/assets/df9a59c7-bc97-403e-bef9-177474b57171" width="280" style="margin-right: 16px;" />
  <img src="https://github.com/user-attachments/assets/5caa2b78-b26f-474c-a312-b29c1a034463" width="280"/>
</p>

---

## 💡 설계 의도

- 난이도 체계는 도전욕을 유발하며 성장 성취감을 제공합니다.
- 던전은 일일 반복 콘텐츠로, **재화 수급 루프의 중심 축** 역할을 합니다.
- **속성 기반 레이드 시스템**은 전략적인 캐릭터 육성과 장비 세팅을 유도합니다.

---

## 🧩 관련 클래스 및 데이터 구조 예시

- `StageManager.cs`
- `DungeonManager.cs`
- `StageDifficulty` (Enum)
- `DungeonType` (Enum)

---
