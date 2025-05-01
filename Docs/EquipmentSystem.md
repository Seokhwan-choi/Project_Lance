# 🛠️ 장비 시스템: 강화 · 재련 · 합성

본 문서는 Project_Lance에서 구현된 장비 시스템 전반(강화, 재련, 합성)에 대한 구조, 흐름, 설계 의도를 설명합니다.

---

## ✅ 개요

### ● 장비 강화
- **강화**는 강화석을 사용하여 장비의 레벨을 올리는 기능입니다.
- 장비의 **최대 강화 레벨은 메인 등급에 따라 제한**되며, 해당 한도를 초과하여 강화할 수 없습니다.

### ● 장비 재련
- **재련**은 재련석을 소모하여 장비의 **최대 강화 가능 레벨을 확장**하는 기능입니다.
- 재련은 확률 기반으로 이루어지며, **실패 시 성공 확률 보너스가 누적**됩니다.
- 여러 번 실패하더라도 언젠가는 반드시 성공에 도달할 수 있도록 설계되었습니다.

### ● 장비 합성
- 같은 **메인 등급과 서브 등급**의 장비 여러 개를 사용하면, 상위 메인 등급의 장비를 획득할 수 있습니다.
- 장비에는 두 가지 등급이 존재합니다:
  - **메인 등급**: D → C → B → A → S → SS -> SSS-> SR → SSR
  - **서브 등급**: 1성 ⭐ ~ 5성 ⭐⭐⭐⭐⭐
- 예시: `D등급 5성 장비 3개` → `C등급 1성 장비`로 합성됨

<p align="center">
  <img src="https://github.com/user-attachments/assets/d3354a8a-faee-4317-98b4-fa002e6fad60" width="280"/>
</p>

---

## 🔁 시스템 흐름 요약

### 강화 흐름
1. 강화석 사용 → 강화 시도
2. 현재 강화 레벨이 최대치 미만일 경우 성공
3. 최대치 도달 시 강화 불가

### 재련 흐름
1. 재련석 사용 → 재련 시도 (확률 기반)
2. 실패 시 성공 확률 보너스 증가
3. 성공 시 장비의 최대 강화 레벨 확장

### 합성 흐름
1. 같은 메인/서브 등급 장비 n개 선택
2. 합성 버튼 클릭 시 장비 소모
3. 상위 메인 등급 + 1성 장비 생성

---

## 💻 핵심 코드 예시

```csharp
// 장비 강화
public void UpgradeEquipment(string id, int upgradeCount)
{
    var inventory = GetInventoryByEquipmentId(id);
    if (inventory == null)
        return;

    // 인벤토리에 있는 장비만 강화 가능
    EquipmentInst equipItem = GetEquipment(id);
    if (equipItem == null || equipItem.IsMaxLevel())
        return;

    if (equipItem.GetMaxLevel() < equipItem.GetLevel() + upgradeCount)
        return;

    double requireStones = equipItem.GetUpgradeRequireStones(upgradeCount);
    if (IsEnoughUpgradeStones(requireStones) == false)
        return;

    // 장비 레벨업
    equipItem.LevelUp(upgradeCount);

    // 강화석 사용
    UseUpgradeStones(requireStones);

    inventory.SetIsChangedData(true);
}

// 장비 재련
public int ReforgeEquipment(string id)
{
    var inventory = GetInventoryByEquipmentId(id);
    if (inventory == null)
        return -1;

    var equipData = DataUtil.GetEquipmentData(id);
    if (equipData == null)
        return -1;

    // 인벤토리에 있는 장비만 강화 가능
    EquipmentInst equipItem = GetEquipment(id);
    if (equipItem == null)
        return -1;

    if (equipItem.IsMaxLevel() == false)
        return -1;

    if (equipItem.IsMaxReforge())
        return -1;

    double requireStones = equipItem.GetReforgeRequireStone();
    if (UseReforgeStones(requireStones))
    {
        equipItem.TryReforge();

        int reforgeStep = equipItem.GetReforgeStep();
        int failCount = equipItem.GetReforgeFailedCount();
        // 장비 재련시도
        float reforgeProb = DataUtil.GetEquipmentReforgeProb(equipData.grade, reforgeStep);
        float bonusProb = DataUtil.GetEquipmentReforgeFailBonusProb(equipData.grade, reforgeStep, failCount);
        if (Util.Dice(reforgeProb + bonusProb))
        {
            equipItem.Reforge();

            inventory.SetIsChangedData(true);

            return 1;
        }
        else
        {
            inventory.SetIsChangedData(true);

            equipItem.StackFailReforge();

            return 0;
        }
    }
    else
    {
        return -1;
    }
}

// 장비 합성
public (string id, int combineCount) CombineItem(string id)
{
    // 내가 가지고 있는 장비인지 확인
    if (HaveItem(id) == false)
        return (string.Empty, 0);

    EquipmentData data = DataUtil.GetEquipmentData(id);
    if (data == null)
        return (string.Empty, 0);

    // combineCount가 없다면 합성 진행이 불가능한 것
    if (data.combineCount == 0)
        return (string.Empty, 0);

    // 합성에 필요한 장비 갯수가 충분한지 확인
    EquipmentInst equipItem = GetEquipItem(id);
    if (equipItem.IsEnoughCount(data.combineCount) == false)
        return (string.Empty, 0);

    int combineCount = equipItem.GetCount() / data.combineCount;
    int useCount = data.combineCount * combineCount;

    // 다음 등급 장비 데이터를 확인
    // 데이터가 없다면 뭔가 잘 못된 것
    EquipmentData nextData = DataUtil.GetNextGradeEquipmentData(data.type, data.grade, data.subGrade);
    if (nextData == null)
        return (string.Empty, 0);

    // 합성 횟수만큼 장비 사용
    if (equipItem.UseCount(useCount) == false)
        return (string.Empty, 0);

    mStackedCombineCount += combineCount;

    SetIsChangedData(true);

    return (nextData.id, combineCount);
}
```
---

## 💡 설계 의도

- 장비 강화와 재련을 통해 점진적인 성장을 유도하며,
- 합성 시스템을 통해 과잉 장비를 소모하고 상위 장비로 진화시키는 성장 경로를 제공합니다.
- 실패 확률 보정(재련), 반복 시도(합성), 등급 확장(강화 제한) 등을 통해 유저 경험의 스트레스를 최소화하면서도 게임 내 콘텐츠의 깊이를 강화합니다.

---

## 🧩 관련 클래스

- `Inventory.cs`
- `Equipment.cs`
- `WeaponInventory.cs` ( 랜스 - 무기 )
- `ArmorInventory.cs`  ( 갑옷 )
- `ShoesInventory.cs`  ( 신발 )
- `GlovesInventory.cs` ( 장갑 )

---
