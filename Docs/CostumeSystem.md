# 🧥 코스튬 시스템 (Sprite Resolver 기반)

이 문서는 Project_Lance의 **코스튬 시스템 구현 방식**과 **SpriteResolver를 활용한 구조**에 대해 설명합니다.  
외형 다양성과 유지보수 편의성을 확보하며, 게임의 커스터마이징 요소를 자연스럽게 확장할 수 있도록 설계되었습니다.

---

## 🎨 시스템 개요

- 캐릭터는 다음 부위별로 코스튬을 착용할 수 있습니다:
  - 무기 (Weapon)
  - 몸통 (Armor)
  - 악세사리 (Etc)
- 각 코스튬은 전용 스프라이트로 구성되어 있으며, **Sprite Resolver**를 통해 교체됩니다.

<p align="center">
  <img src="https://github.com/user-attachments/assets/ef5a010e-c2b4-4cc3-98a4-42b5e3d8d871" width="280" style="margin-right: 16px;" />
  <img src="https://github.com/user-attachments/assets/da12dad6-34ba-4e83-9794-f56b4c49b900" width="280" style="margin-right: 16px;" />
  <img src="https://github.com/user-attachments/assets/3672a81b-073e-43f4-af0e-b16378e702ee" width="280"/>
</p>

---

## 🧩 Sprite Resolver란?

**Unity 2D Animation Package**에서 제공하는 기능으로,  
`Sprite Library`와 연동하여 코드나 데이터에서 **스프라이트 교체를 쉽게 처리**할 수 있는 시스템입니다.

<p align="center">
  <img src="https://github.com/user-attachments/assets/17d3860b-87e7-4f79-9952-8c51b68aa4f1" width="280"/>
</p>

---

## ✅ Sprite Resolver의 장점

| 항목 | 내용 |
|------|------|
| 🔄 **유지보수 편의** | 코드로 직접 Sprite를 교체하지 않고, 이름만 바꾸면 자동 적용 |
| 🚀 **확장성** | 새로운 코스튬 추가 시 Sprite Library에만 등록하면 적용 가능 |
| 🧩 **애니메이션 연계** | 동일한 애니메이션 상태에서도 Sprite만 교체 가능 |
| 📁 **구조화** | 부위별 카테고리 관리가 용이 (ex: Helmet / Armor / Shoes 등) |

---

## 💡 설계 의도

- SpriteResolver를 통해 **개별 장비별 스프라이트 적용 로직을 통합**
- 다양한 코스튬을 적용해도 **애니메이션, 코드, UI 수정 없이 대응 가능**
- 추후 유료 코스튬, 이벤트 코스튬 등으로 손쉽게 확장 가능

---

## 🧪 활용 예시

```csharp
mAnim.SetBodyLibraryAsset(mSpriteLibraryAssetData.libraryAsset);
mAnim.SetHandLibraryAsset(handLibraryAsset);
mAnim.SetEtcLibraryAsset(etcLibrearyAsset, orderInLayer);
mWeaponRenderer.sprite = Lance.Atlas.GetPlayerSprite(weaponSprite);
```

---

## 📁 관련 클래스

- `Costume.cs`  
- `LibraryAssetData.cs`  
- `CharacterAnim.cs`
