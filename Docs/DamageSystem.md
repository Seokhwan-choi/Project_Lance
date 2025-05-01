# ⚔️ 전투 데미지 계산 구조

본 문서는 Project_Lance 내 전투 시스템의 데미지 계산 방식을 설명합니다.  
캐릭터의 공격력, 크리티컬, 스킬 계수, 장비 효과, 속성 상성, 마나 감응도까지 종합적으로 고려하여 데미지를 산출합니다.

---

## ✅ 데미지 계산 흐름 (플레이어 기준)

```csharp
damageInst.Damage = CalcPlayerAtk(player)
    * (isSuperCritical ? (criDmg * superCriDmg) : (isCritical ? criDmg : 1))
    * (activeSkillValue * (1 + CalcPlayerSkillDmg(player)))
    * (1 + CalcPlayerAddDmg(player))
    * (isBoss ? (1 + CalcPlayerBossDmg(player)) : (1 + CalcPlayerMonsterDmg(player)))
    * CalcElementalDmg(player, defender);

// 마나 감응도 적용
if (attacker.Stat.ManaSensitivity > 0 || defender.Stat.ManaSensitivity > 0)
{
    double sensitivity = (1.05 + (attacker.Stat.ManaSensitivity - defender.Stat.ManaSensitivity)) / 222;
    damageInst.Damage *= (1 + sensitivity);
}
```

---

## 🔍 고려 요소

| 요소             | 설명 |
|------------------|------|
| **기본 공격력**      | 캐릭터의 공격력 + 장비 + 비율 보정 |
| **치명타/슈퍼치명타**  | 확률 기반으로 데미지 배율 적용 (최대 3단계) |
| **스킬 배율**        | 스킬마다 계수 반영 (activeSkillValue) |
| **추가 데미지**      | 스탯 + 패시브 스킬로 획득 |
| **보스/일반 데미지 보정** | 대상이 Boss인지 여부에 따라 별도 배율 적용 |
| **속성 상성**        | 화/수/초 속성별 상성에 따른 배율 보정 |
| **마나 감응도**      | 공격자/수비자의 감응도 수치 차이에 따른 추가 보정 |

---

## 🧠 설계 의도

단순한 `공격력 - 방어력` 계산을 넘어서, RPG 특유의 복합적인 상황(속성, 치명타, 보정 등)을 반영한  
전략적인 전투 구조를 구현하고자 하였습니다. 이 구조는 후속 시스템(스킬 강화, 파티 조합, 장비 셋팅 등)과도 유기적으로 연계됩니다.

---

## 🧩 관련 클래스

- `DamageCalculator.cs`  
- `DamageInst.cs`  
- `CharacterStat.cs`  
- `SkillManager.cs`  
- `Util.cs` (치명타 판정, 랜덤 주사위)
