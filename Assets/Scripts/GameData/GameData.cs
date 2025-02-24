using System.Collections;
using System.Collections.Generic;
using ExcelToObject;
using System;
using CodeStage.AntiCheat.ObscuredTypes;
using BackEnd;

namespace Lance
{
    public class GameData
    {
        public CommonData CommonData;
        public OfflineRewardCommonData OfflineRewardCommonData;
        public RankUpdateIgnoreData RankUpdateIgnoreData;
        public Dictionary<string, RaidBadData> RaidBadData;
        public Dictionary<string, MaintanceInfoData> MaintanceInfoData;

        // 플레이어 ( 유저 ) 스탯
        public PlayerStatMaxData PlayerStatMaxData;
        public PlayerBaseStatData PlayerBaseStatData;
        public List<GoldTrainAtkNHpData> GoldTrainAtkNHpData;
        public List<GoldTrainAtkNHpData> GoldTrainPowerAtkNHpData;
        public List<GoldTrainCriData> GoldTrainCriData;
        public List<GoldTrainCriData> GoldTrainSuperCriData;
        public List<GoldTrainGoldNExpData> GoldTrainGoldNExpData;
        public List<GoldTrainAmplifyData> GoldTrainAmplifyData;
        public List<GoldTrainElementalData> GoldTrainElementalData;
        public Dictionary<StatType, GoldTrainData> GoldTrainData;
        public Dictionary<int, PlayerLevelUpData> PlayerLevelUpData;

        // 몬스터
        public Dictionary<int, MonsterStatData> MonsterStatData;
        public Dictionary<int, MonsterStatData> BossStatData;
        public Dictionary<string, MonsterData> MonsterData;
        public Dictionary<string, MonsterData> BossData;
        public Dictionary<string, MonsterData> RaidBossData;
        public Dictionary<string, MonsterData> PetBossData;
        public Dictionary<string, MonsterData> AncientBossData;
        public Dictionary<string, LibraryAssetData> LibraryAssetData;
        public List<RaidBossRageLevelData> RaidBossRageLevelData;
        public Dictionary<string, RageActionData> RageActionData;
        public Dictionary<string, ShieldData> ShieldData;

        // 스테이지
        public StageCommonData StageCommonData;
        public Dictionary<string, StageData> StageData;
        public List<StageMonsterData> StageMonsterData;

        // 보상
        public Dictionary<string, MonsterRewardData> MonsterRewardData;
        public Dictionary<string, MonsterRewardData> GoldDungeonMonsterRewardData;
        public Dictionary<string, MonsterRewardData> StoneDungeonMonsterRewardData;
        public Dictionary<string, MonsterRewardData> ReforgeDungeonMonsterRewardData;
        public Dictionary<string, MonsterRewardData> PetDungeonMonsterRewardData;
        public Dictionary<string, MonsterRewardData> GrowthDungeonMonsterRewardData;
        public Dictionary<string, MonsterRewardData> AncientDungeonMonsterRewardData;
        public Dictionary<string, MonsterRewardData> DemonicRealmMonsterRewardData;
        public List<RaidRewardData> RaidRewardData;

        // 장비
        public EquipmentCommonData EquipmentCommonData;
        public Dictionary<string, EquipmentData> WeaponData;
        public Dictionary<string, EquipmentData> ArmorData;
        public Dictionary<string, EquipmentData> GlovesData;
        public Dictionary<string, EquipmentData> ShoesData;
        public Dictionary<string, OwnStatData> OwnStatData;
        public List<UpgradeData> UpgradeData;
        public Dictionary<string, RandomEquipmentRewardData> RandomEquipmentRewardData;

        public Dictionary<Grade, ReforgeData> ReforgeData;
        public Dictionary<Grade, ReforgeRequireStoneData> ReforgeRequireStoneData;
        public Dictionary<Grade, ReforgeProbData> ReforgeProbData;

        public EquipmentOptionStatGradeProbData EquipmentOptionStatGradeProbData;
        public Dictionary<Grade, EquipmentOptionStatUnlockData> EquipmentOptionStatUnlockData;
        public Dictionary<Grade, EquipmentOptionStatChangePrice> EquipmentOptionStatChangePrice;
        public Dictionary<StatType, EquipmentOptionStatValueData> EquipmentOptionStatValueData;
        public Dictionary<Grade, EquipmentOptionStatToOwnStatData> EquipmentOptionStatToOwnStatData;

        // 악세사리
        public AccessoryCommonData AccessoryCommonData;
        public Dictionary<string, AccessoryData> NecklaceData;
        public Dictionary<string, AccessoryData> EarringData;
        public Dictionary<string, AccessoryData> RingData;
        public List<UpgradeData> AccessoryUpgradeData;
        public Dictionary<string, RandomEquipmentRewardData> RandomAccessoryRewardData;

        public Dictionary<string, OwnStatData> AccessoryOwnStatData;
        public Dictionary<Grade, ReforgeData> AccessoryReforgeData;
        public Dictionary<Grade, ReforgeRequireStoneData> AccessoryReforgeRequireStoneData;
        public Dictionary<Grade, ReforgeProbData> AccessoryReforgeProbData;

        // 상점
        public ShopCommonData ShopCommonData;
        public Dictionary<string, GemShopData> GemShopData;
        public Dictionary<string, NormalShopData> NormalShopData;
        public Dictionary<string, MileageShopData> MileageShopData;
        public Dictionary<string, JoustShopData> JoustShopData;
        public List<PackageShopData> PackageShopData;

        // 소환
        public Dictionary<ItemType, SpawnData> SpawnData;
        public List<SpawnPriceData> SpawnPriceData;
        public List<SpawnGradeProbData> SpawnGradeProbData;
        public List<SpawnRewardData> SpawnRewardData;
        public List<EquipmentSubgradeProbData> EquipmentSubgradeProbData;
        public List<AccessorySubgradeProbData> AccessorySubgradeProbData;
        public SkillTypeProbData SkillTypeProbData;
        public ArtifactProbData ArtifactProbData;
        public ArtifactProbData AncientArtifactProbData;

        // 스킬
        public SkillCommonData SkillCommonData;
        public SkillSlotUnlockData SkillSlotUnlockData;
        public Dictionary<string, SkillData> ActiveSkillData;
        public Dictionary<string, SkillData> PassiveSkillData;
        public Dictionary<Grade, SkillUpgradeData> SkillUpgradeData;
        public Dictionary<Grade, SkillUpgradeData> SkillUpgradeData2;
        public Dictionary<Grade, SkillDismantleData> SkillDismantleData;
        public Dictionary<int, SkillProficiencyData> SkillProficiencyData;
        public Dictionary<string, RandomSkillRewardData> RandomSkillRewardData;

        // 패스
        public Dictionary<string, PassData> PassData;
        public List<PassStepData> PassStepData;

        // 출석부
        public Dictionary<string, AttendanceData> AttendanceData;
        public List<AttendanceDayData> AttendanceDayData;

        // 퀘스트
        public QuestCommonData QuestCommonData;
        public Dictionary<string, QuestData> DailyQuestData;
        public Dictionary<string, QuestData> WeeklyQuestData;
        public Dictionary<string, QuestData> RepeatQuestData;
        public Dictionary<string, QuestData> GuideQuestData;

        // 가이드
        public Dictionary<int, GuideData> GuideData;
        public List<GuideActionData> GuideActionData;
        public List<CharacterMessageData> CharacterMessageData;

        // 유물
        public ArtifactCommonData ArtifactCommonData;
        public List<ArtifactMakeData> ArtifactMakeData;
        public Dictionary<string, ArtifactData> ArtifactData;
        public Dictionary<string, ArtifactData> AncientArtifactData;
        public Dictionary<int, ArtifactLevelUpData> ArtifactLevelUpData;
        public Dictionary<int, ArtifactLevelUpData> AncientArtifactLevelUpData;

        // 엑스칼리버
        public Dictionary<ExcaliburForceType, ExcaliburData> ExcaliburData;
        public Dictionary<int, ExcaliburStepData> ExcaliburStepData;
        public List<ExcaliburForceStepData> ExcaliburForceStepData;
        public Dictionary<ExcaliburForceType, ExcaliburUpgradeData> ExcaliburUpgradeData;

        // 특성
        public Dictionary<string, AbilityData> AbilityData;

        // 한계 돌파
        public List<LimitBreakData> LimitBreakData;
        public List<LimitBreakData> UltimateLimitBreakData;
        public List<LimitBreakUIData> LimitBreakUIData;
        public Dictionary<string, StageData> LimitBreakStageData;
        public Dictionary<string, StageData> UltimateLimitBreakStageData;

        // 마나 하트
        public Dictionary<ManaHeartType, ManaHeartData> ManaHeartData;
        public Dictionary<int, ManaHeartStepData> ManaHeartStepData;
        public Dictionary<int, ManaHeartUpgradeStepData> ManaHeartUpgradeStepData;
        public Dictionary<ManaHeartType, ManaHeartUpgradeData> ManaHeartUpgradeData;
        public Dictionary<ManaHeartType, ManaHeartStatData> ManaHeartStatData;

        // 던전
        public RaidRankCommonData RaidRankCommonData;
        public DungeonCommonData DungeonCommonData;
        public Dictionary<StageType, DungeonData> DungeonData;
        public Dictionary<string, StageData> DungeonStageData;

        // 마계
        public DemonicRealmCommonData DemonicRealmCommonData;
        public Dictionary<StageType, DemonicRealmData> DemonicRealmData;
        public Dictionary<string, StageData> DemonicRealmStageData;
        public List<DemonicRealmSoulStoneRewardData> DemonicRealmSoulStoneRewardData;
        public List<DemonicRealmManaEssenceRewardData> DemonicRealmManaEssenceRewardData;

        // 마계 - 전리품, 호감도
        public Dictionary<string, DemonicRealmSpoilsData> DemonicRealmSpoilsData;
        public Dictionary<int, FriendShipLevelUpData> FriendShipLevelUpData;
        public Dictionary<int, FriendShipVisitExpData> FriendShipVisitExpData;
        public Dictionary<int, FalanSpeechData> FalanSpeechData;

        // 버프
        public Dictionary<string, BuffData> BuffData;
        public Dictionary<int, BuffLevelUpData> BuffLevelUpData;

        // 보상
        public Dictionary<string, RewardData> RewardData;
        public List<RankingRewardData> AdvancedRankingRewardData;
        public List<RankingRewardData> BeginnerRankingRewardData;
        public List<RankingRewardData> JoustingRankingRewardData;

        // 신수
        public PetCommonData PetCommonData;
        public Dictionary<string, PetData> PetData;
        public List<PetSkillUnlockData> PetSkillUnlockData;
        public Dictionary<int, PetStepData> PetStepData;
        public Dictionary<int, PetLevelUpData> PetLevelUpData;
        public Dictionary<int, PetEvolutionData> PetEvolutionData;
        public Dictionary<ElementalType, CompatibilityData> CompatibilityData;
        public Dictionary<string, OwnStatData> PetOwnStatData;
        public Dictionary<int, PetEquipStatData> PetEquipStatData;

        public PetEvolutionStatGradeProbData PetEvolutionStatGradeProbData;
        public PetEvolutionStatUnlockData PetEvolutionStatUnlockData;
        public PetEvolutionStatChangePrice PetEvolutionStatChangePrice;
        public Dictionary<StatType, PetEvolutionStatValueData> PetEvolutionStatValueData;
        public Dictionary<string, SkillData> PetActiveSkillData;
        public Dictionary<string, SkillData> PetPassiveSkillData;

        // 신수 장비
        public PetEquipmentCommonData PetEquipmentCommonData;
        public Dictionary<string, PetEquipmentData> PetEquipmentData;
        public List<PetEquipmentUpgradeData> PetEquipmentUpgradeData;
        public Dictionary<string, OwnStatData> PetEquipmentOwnStatData;
        public Dictionary<Grade, ReforgeData> PetEquipmentReforgeData;
        public Dictionary<Grade, ReforgeRequireStoneData> PetEquipmentReforgeRequireStoneData;
        public Dictionary<Grade, ReforgeRequireStoneData> PetEquipmentReforgeRequireElementalStoneData;
        public Dictionary<Grade, ReforgeProbData> PetEquipmentReforgeProbData;

        // 이벤트
        public WeekendDoubleData WeekendDoubleData;
        public Dictionary<string, DoubleEventData> DoubleEventData;
        public Dictionary<string, EventData> EventData;
        public Dictionary<string, QuestData> EventQuestData;
        public Dictionary<string, PassData> EventPassData;
        public List<PassStepData> EventPassStepData;
        public Dictionary<string, EventShopData> EventShopData;

        // 코스튬
        public CostumeCommonData CostumeCommonData;
        public Dictionary<string, CostumeData> BodyCostumeData;
        public Dictionary<string, CostumeData> WeaponCostumeData;
        public Dictionary<string, CostumeData> EtcCostumeData;
        public Dictionary<string, OwnStatData> CostumeOwnStatData;
        public Dictionary<Grade, CostumeUpgradeData> CostumeUpgradeRequireData;      // 필요한 실타래
        public Dictionary<Grade, CostumeUpgradeData> CostumeUpgradeRequireStoneData; // 필요한 강화석
        public Dictionary<string, CostumeShopData> CostumeShopData;

        // 현상금
        public Dictionary<string, BountyQuestData> BountyQuestData;
        public Dictionary<string, BountyQuestRewardProbData> BountyQuestRewardProbData;

        // 정수
        public EssenceCommonData EssenceCommonData;
        public Dictionary<EssenceType, EssenceData> EssenceData;
        public List<EssenceStepData> EssenceStepData;
        public Dictionary<EssenceType, EssenceStatValueData> EssenceStatValueData;
        public Dictionary<EssenceType, EssenceUpgradeData> EssenceUpgradeData;
        public Dictionary<int, CentralEssenceStepData> CentralEssenceStepData;
        public Dictionary<int, CentralEssenceActiveRequireData> CentralEssenceActiveRequireData;
        public Dictionary<int, CentralEssenceStatValueData> CentralEssenceStatValueData;

        // 빠른 전투 ( 스킵 전투 )
        public SkipBattleCommonData SkipBattleCommonData;
        public SkipBattlePriceData SkipBattlePriceData;
        public List<SkipBattleMonsterKillData> SkipBattleMonsterKillData;

        // 행운 복권 ( 로또 )
        public LottoCommonData LottoCommonData;
        public Dictionary<int, LottoRewardData> LottoRewardData;

        // 수집
        public Dictionary<string, CollectionData> CollectionData;
        public Dictionary<string, CollectionStatData> CollectionStatData;

        // 마상시합
        public JoustingCommonData JoustingCommonData;
        public Dictionary<JoustingAttackType, JoustingCompatibilityData> JoustingCompatibilityData;
        public List<JoustingTierData> JoustingTierData;
        public StageData JoustingStageData;

        // 마상 시합 영광의 보주
        public Dictionary<JoustGloryOrbType, JoustingGloryOrbData> JoustingGloryOrbData;
        public Dictionary<int, JoustingGloryOrbStepData> JoustingGloryOrbStepData;
        public Dictionary<JoustGloryOrbType, JoustingGloryOrbUpgradeData> JoustingGloryOrbUpgradeData;
        public Dictionary<JoustGloryOrbType, JoustingGloryOrbStatData> JoustingGloryOrbStatData;

        // 업적
        public AchievementCommonData AchievementCommonData;
        public Dictionary<string, AchievementData> AchievementData;
        public Dictionary<string, QuestData> AchievementQuestData;
        public Dictionary<string, AchievementStatData> AchievementStatData;

        // 프리셋
        public Dictionary<int, PresetData> PresetData;

        public List<ThanksToData> ThanksToData;

        // 스트링 테이블
        public Dictionary<string, StringTable> StringTable;
        public List<BadWord> BadWord;
    }

    public class CommonData
    {
        public double nickNameChangePrice;
        public string timeCheatingDetectUrl;
        public string inquiryMail;
        public string playStoreUrl;
        public string privacypolicyUrl;
        public string privacypolicyUrl_eng;
        public string termsofserviceUrl;
        public string termsofserviceUrl_eng;
        public string communityUrl;
        public int limitBreakMaxLevel;
        public int ultimateLimitBreakMaxLevel;
        public double bossRewardValue;
        public float speedModeDuration;
        public float speedModeValue;
        public float retroactiveElementalStoneValue;
        public int retroactiveGloryTokenCount;
        public int retroactiveGloryTokenMonhtlyFeeCount;
        public int chattingDummyPlayerCount;
    }

    public class OfflineRewardCommonData
    {
        public float monsterKillCalcMinute;
        public int maxTimeForHour;
        public float minOfflineTimeForSecond;
        public float adBonusValue;
    }

    public class RankUpdateIgnoreData
    {
        public string[] ignores;
    }

    public class MaintanceInfoData
    {
        public string id;
        public bool active;
    }

    public class PlayerStatMaxData
    {
        public float atkSpeedMax;
        public float criProbMax;
        public float superCriProbMax;
        public float moveSpeedMax;
    }

    public class PlayerBaseStatData
    {
        public double atk;
        public float atkSpeed;
        public float atkRange;
        public double hp;
        public float moveSpeed;
        public float criProb;
        public double criDmg;
        public float superCriProb;
        public double superCriDmg;
    }

    public class MonsterData
    {
        public string id;
        public MonsterType type;
        public ElementalType elementalType;
        public float atkSpeed;
        public float atkRange;
        public string prefab;
        public string pos;
        public float bodySize;
        public string libraryAsset;
    }

    public class LibraryAssetData
    {
        public string id;
        public string libraryAsset;
        public float[] attackProb;
        public float[] attackTime;
        public string rageActionId;
        public float rageTime;
        public bool sneer;
    }

    public class RaidBossRageLevelData
    {
        public int step;
        public int level;
    }

    public class RageActionData
    {
        public string id;
        public RageActionType type;
        public float value;
        public string etc;
    }

    public class ShieldData
    {
        public string id;
        public string fx;
        public float value;
    }

    public class GoldTrainAtkNHpData
    {
        public double atkValue;
        public double hpValue;
        public double requireGold;
    }

    public class GoldTrainCriData
    {
        public double criProbValue;
        public double criDmgValue;
        public double requireGold;
    }

    public class GoldTrainGoldNExpData
    {
        public double goldAmountValue;
        public double expAmountValue;
        public double requireGold;
    }

    public class GoldTrainAmplifyData
    {
        public double atkAmplifyValue;
        public double hpAmplifyValue;
        public double requireGold;
    }

    public class GoldTrainElementalData
    {
        public double fireAddDmgValue;
        public double waterAddDmgValue;
        public double grassAddDmgValue;
        public double requireGold;
    }

    public class GoldTrainData
    {
        public string id;
        public StatType type;
        public StatType requireType;
        public int requireLevel;
    }

    public class PlayerLevelUpData
    {
        public int level;
        public double requireExp;
        public int ap;
    }

    public class MonsterStatData
    {
        public int level;
        public double atk;
        public double hp;
        public double dropGold;
    }

    public class StageCommonData
    {
        public int maxStage;
        public int maxChapter;
        public int spawnMonsterCount;
        public int growthDungeonSpawnMonsterCount;
        public int demonicRealmAccessorySpawnMonsterCount;
        public int bossChallengeKillCount;
        public StageDifficulty essenceDropDiff;
        public float essenceDropProb;
        public int essenceMinDropAmount;
        public int essenceMaxDropAmount;
        public StageDifficulty threadDropDiff;
        public float threadDropProb;
        public double threadDropAmount;
        public float treasureGoblinProb;
        public float treasureGoblinRewardBonusValue;
        public string treasureGoblin;
    }

    public class StageData
    {
        public string id;
        public StageType type;
        public StageDifficulty diff;
        public int chapter;
        public int stage;
        public int monsterLevel;
        public int increaseLevel;
        public bool atOnceSpawn;
        public string monsterDropReward;
        public int monsterLimitCount;
        public int bossLevel;
        public string bossDropReward;
        public float increaseTimeout;
        public float stageTimeout;
        public string stageFirstClearReward;
    }

    public class StageMonsterData
    {
        public int chapter;
        public int stage;
        public StageType type;
        public string monsters;
        public string boss;
    }

    public class MonsterRewardData
    {
        public string id;
        public double exp;
        public double gold;
        public int petFood;
        public double stones;
        public double reforgeStone;
        public int ancientEssence;
        public int manaEssence;
        public string randomEquipment;
        public string randomAccessory;
        public float goldProb;
        public float stonesProb;
        public float reforgeStoneProb;
        public float petFoodProb;
        public float ancientEssenceProb;
        public float manaEssenceProb;
        public float equipmentProb;
        public float accessoryProb;
    }

    public class RewardData
    {
        public string id;
        public double gold;
        public double gem;
        public int ticketBundle;
        public int[] dungeonTicket;
        public int[] demonicRealmStone;
        public int[] essence;
        public int petFood;
        public int elementalStone;
        public int mileage;
        public double upgradeStone;
        public double reforgeStone;
        public int skillPiece;
        public int ancientEssence;
        public string equipment;
        public string accessory;
        public string skill;
        public string artifact;
        public string randomEquipment;
        public string randomAccessory;
        public string randomSkill;
        public string costume;
        public string eventCurrency;
        public int joustingTicket;
        public int joustingCoin;
        public int gloryToken;
        public int soulStone;
        public int manaEssence;
        public double costumeUpgrade;
        public string achievement;
        public string petEquipment;

        public bool adRemove;
        public bool speedMode;
        public bool buff;
        public bool IsRandomReward()
        {
            return randomEquipment.IsValid() || randomSkill.IsValid() || randomAccessory.IsValid();
        }
    }

    public class RaidRankCommonData
    {
        public double rankStandardDamage;   // 초과면 정예 기사 랭크 이하면 초급 기사 랭크
    }

    public class RankingRewardData
    {
        public int rankMin;
        public int rankMax;
        public string reward;
    }

    public class EquipmentCommonData
    {
        public int ownOptionMaxCount;
        public int reforgeAddMaxLevel;
        public float reforgeFailedBonusProbValue;
        public int optionStatMaxCount;
        public int optionStatMaxPreset;
        public int combineCount;
    }

    public class EquipmentData
    {
        public string id;
        public ItemType type;
        public Grade grade;
        public SubGrade subGrade;
        public StatType valueType;
        public double baseValue;
        public double levelUpValue;
        public string[] ownStats;
        public int combineCount;
        public int maxLevel;
        public string sprite;
    }

    public class PetEquipmentCommonData
    {
        public int ownOptionMaxCount;
        public int reforgeAddMaxLevel;
        public float reforgeFailedBonusProbValue;
        public int optionStatMaxCount;
        public int optionStatMaxPreset;
    }

    public class PetEquipmentData
    {
        public string id;
        public ElementalType type;
        public Grade grade;
        public SubGrade subGrade;
        public StatType valueType;
        public double baseValue;
        public double levelUpValue;
        public string[] ownStats;
        public int combineCount;
        public int maxLevel;
        public string sprite;
    }

    public class RandomEquipmentRewardData
    {
        public string id;
        public Grade grade;
        public SubGrade subGrade;
    }

    public class OwnStatData
    {
        public string id;
        public StatType valueType;
        public double baseValue;
        public double levelUpValue;
    }

    public class UpgradeData
    {
        public Grade grade;
        public SubGrade subGrade;
        public double baseRequire;
        public double[] reforge;
    }

    public class PetEquipmentUpgradeData
    {
        public Grade grade;
        public SubGrade subGrade;
        public double baseRequire;
        public double[] reforge;
    }

    public class ReforgeData
    {
        public Grade grade;
        public int maxReforge;
    }

    public class ReforgeRequireStoneData
    {
        public Grade grade;
        public int[] require;
    }

    public class ReforgeProbData
    {
        public Grade grade;
        public float[] probs;
    }

    public class EquipmentOptionStatGradeProbData
    {
        public float[] probs;
    }

    public class EquipmentOptionStatValueData
    {
        public StatType statType;
        public double[] statValue;
    }

    public class EquipmentOptionStatUnlockData
    {
        public Grade grade;
        public bool[] unlockSlot;
    }

    public class EquipmentOptionStatChangePrice
    {
        public Grade grade;
        public double[] changePrice;
    }

    public class EquipmentOptionStatToOwnStatData
    {
        public Grade grade;
        public double changeValue;
    }

    public class ShopCommonData
    {
        public int artifactSellPrice;
        public int monthlyFeeDurationDay;
        public int equipSpawnMaxLevel;
        public int accessorySpawnMaxLevel;
    }

    public class GemShopData
    {
        public string id;
        public double gemAmount;
        public double bonusAmount;
        public int bonusCount;
        public int mileage;
        public string spriteImg;
        public int price;
        public int watchAdDailyCount;
        public string productId;
    }

    public class NormalShopData
    {
        public string id;
        public string reward;
        public int price;
        public int purchaseDailyCount;
        public string spriteImg;
    }

    public class MileageShopData
    {
        public string id;
        public string reward;
        public int price;
        public int purchaseDailyCount;
        public string spriteImg;
    }

    public class JoustShopData
    {
        public string id;
        public string reward;
        public int price;
        public int purchaseDailyCount;
        public string spriteImg;
    }

    public class PackageShopData
    {
        public string id;
        public string reward;
        public string monthlyFeeDailyReward;
        public PackageCategory category;
        public PackageType type;
        public int step;
        public int price;
        public int mileage;
        public int purchaseLimitCount;
        public PackageResetType resetType;
        public int startDate;
        public int endDate;
        public bool active;
        public string background;
        public string productId;
        public int valueAmount;
    }

    public class DemonicRealmSpoilsData
    {
        public string id;
        public string reward;
        public int price;
        public int purchaseLimitCount;
        public int purchaseDailyCount;
        public int requireFriendShipLevel;
        public int friendShipExp;
        public string spriteImg;
    }

    public class FriendShipLevelUpData
    {
        public int level;
        public int requireExp;
    }

    public class FriendShipVisitExpData
    {
        public int level;
        public int exp;
    }

    public class FalanSpeechData
    {
        public int friendShipLevel;
        public string sprite;
        public string firstSpeech;
        public string speechList;
    }

    public class SpawnData
    {
        public ItemType type;
        public string probId;
        public string sprite;
        public string spriteCloseImg;
    }

    public class SpawnPriceData
    {
        public string id;
        public ItemType type;
        public SpawnType spawnType;
        public int spawnCount;
        public double price;
        public int dailyLimitCount;
        public bool firstSpawnFree;
        public bool guideButton;
        public bool autoSpawn;
        public bool onlyResult;
    }

    public class SpawnGradeProbData
    {
        public string id;
        public int level;
        public int requireStack;
        public float[] prob;
    }

    public class SpawnRewardData
    {
        public ItemType type;
        public int level;
        public string reward;
        public string repeatReward;
        public int repeatRewardRequire;
    }

    public class SpawnStackedRewardData
    {
        public string id;
        public int requireStack;
        public string reward;
        public int rewardCount;
        public RewardType type;
    }

    public class EquipmentSubgradeProbData
    {
        public int level;
        public Grade grade;
        public float[] prob;
    }

    public class AccessorySubgradeProbData
    {
        public int level;
        public Grade grade;
        public float[] prob;
    }

    public class SkillTypeProbData
    {
        public float[] prob;
    }

    public class ArtifactProbData
    {
        public float[] prob;
    }

    public class SkillCommonData
    {
        public int presetMaxCount;
        public int skillMaxSlot;
    }

    public class SkillSlotUnlockData
    {
        public int[] unlockLevel; 
    }

    public class SkillData
    {
        public string id;
        public Grade grade;
        public SkillType type;
        public StatType passiveType;
        public SkillLevelUpMaterial levelUpMaterial;
        public int maxLevel;
        public float coolTime;
        public float duration;
        public float atkRange;
        public int stackCount;
        public int atkCount;
        public float atkDelay;
        public float skillValue;
        public float levelUpValue;
        public int targetCount;
        public SkillCondition condition;
        public float conditionValue;
        public SkillValueCalcType skillValueCalcType;
        public string skillSound;
        public string skillFX;
        public string skillAnim;
        public string uiSprite;
        public int requireLimitBreak;
        public bool soundReleaseImmediately;
    }

    public class SkillUpgradeData
    {
        public Grade grade;
        public int[] requireCount;
    }

    public class SkillProficiencyData
    {
        public int step;
        public int requireCount;
        public double addSkillDmg;
    }

    public class SkillDismantleData
    {
        public Grade grade;
        public int amount;
    }

    public class RandomSkillRewardData
    {
        public string id;
        public Grade grade;
    }

    public class PassData
    {
        public string id;
        public string eventId;
        public PassType type;
        public QuestCheckType checkType;
        public string productId;
        public int price;
        public int mileage;
    }

    public class PassStepData
    {
        public string id;
        public int step;
        public double requireValue;
        public string freeReward;
        public string payReward;
    }

    public class AttendanceData
    {
        public string id;
        public AttendanceType type;
        public AttendanceDayType dayType;
        public string eventId;
        public int priority;
        public string prefab;
        public string character;
        public string characterEng;
    }
    
    public class AttendanceDayData
    {
        public string id;
        public int day;
        public string reward;
    }

    public class QuestCommonData
    {
        public int dailyQuestMaxCount;
        public int weeklyQuestMaxCount;
        public DayOfWeek weeklyQuestUpdateDayOfWeek;
        public int bountyQeustMaxCount;
    }

    public class QuestData
    {
        public string id;
        public string eventId;
        public int openDay;
        public QuestType type;
        public QuestUpdateType updateType;
        public QuestCheckType checkType;
        public int requireCount;
        public string rewardId;
        public string adBonusReward;
    }

    public class BountyQuestData
    {
        public string id;
        public MonsterType monsterType;
        public string monsterId;
        public int killCount;
        public string uiSprite;
        public int moveChapter;
        public int moveStage;
    }

    public class BountyQuestRewardProbData
    {
        public string id;
        public float prob;
    }

    public class ArtifactCommonData
    {
        public int dismantleMinAmount;
        public int dismantleMaxAmount;
        public int ancientDismantleMinAmount;
        public int ancientDismantleMaxAmount;
        public float failedBonusProbValue;
    }

    public class ArtifactMakeData
    {
        public int makeCount;
        public int makePrice;
    }

    public class ArtifactData
    {
        public string id;
        public StatType statType;
        public double baseValue;
        public double levelUpValue;
        public string sprite;
    }

    public class ArtifactLevelUpData
    {
        public int level;
        public int requireCount;
        public float prob;
    }

    public class AbilityData
    {
        public string id;
        public int step;
        public StatType statType;
        public double levelUpValue;
        public double increaseLevelUpValue;
        public int increaseStartLevel;
        public int maxLevel;
        public string requireAbilitys;
        public int requireAp;
        public string sprite;
    }

    public class DungeonCommonData
    {
        public int entranceRequireTicket;
        public int sweepRequireTicket;
        //public int spawnMonsterCount;
    }

    public class DungeonData
    {
        public StageType type;
        public int dailyWatchAdCount;
        public int maxStep;
    }

    public class DemonicRealmCommonData
    {
        public int entranceRequireTicket;
        public int sweepRequireTicket;
    }

    public class DemonicRealmData
    {
        public StageType type;
        public int dailyWatchAdCount;
        public int maxStep;
    }

    public class DemonicRealmSoulStoneRewardData
    {
        public StageType type;
        public int stage;
        public int dropAmount;
        public float dropProb;
    }

    public class DemonicRealmManaEssenceRewardData
    {
        public StageType type;
        public int stage;
        public int dropAmount;
        public float dropProb;
    }

    public class BuffData
    {
        public string id;
        public StatType type;
        public float duration;
        public double buffValue;
        public double buffLevelUpValue;
        //public int dailyCount;
        public int maxLevel;
    }

    public class BuffLevelUpData
    {
        public int level;
        public int requireMonsterKillCount;
    }

    public class GuideData
    {
        public int step;
        public string action;
        public string autoAction;
        public string quest;
    }

    public class GuideActionData
    {
        public string id;
        public int step;
        public GuideActionType type;
        public float waitTime;
        public string showMessage;
    }

    public class CharacterMessageData
    {
        public string id;
        public int step;
        public CharacterMessageActionType action;
        public string message;
        public string character;
    }

    public class LimitBreakData
    {
        public int step;
        public double atkRatio;
        public double hpRatio;
        public float atkSpeed;
        public float moveSpeed;
        public int requireLevel;
        public int requireStep;
    }

    public class LimitBreakUIData
    {
        public int step;
        public string activeLine;
    }

    public class PetCommonData
    {
        public float weakTypeAtkValue;
        public float strongTypeAtkValue;
        public int evolutionStatMaxPreset;
        public int evolutionStatMaxSlot;
    }

    public class PetData
    {
        public string id;
        public ElementalType type;
        public string[] ownStats;
        public int maxStep;
    }

    public class PetSkillUnlockData
    {
        public ElementalType type;
        public int step;
        public string active;
        public string passive;
    }

    public class PetStepData
    {
        public int step;
        public int maxLevel;
    }

    public class PetLevelUpData
    {
        public int step;
        public int require;
    }

    public class PetEvolutionData
    {
        public int step;
        public int require;
    }

    public class PetEvolutionStatGradeProbData
    {
        public float[] probs;
    }

    public class PetEvolutionStatValueData
    {
        public StatType statType;
        public double[] statValue;
    }

    public class PetEvolutionStatUnlockData
    {
        public int[] unlockSlot;
    }

    public class PetEvolutionStatChangePrice
    {
        public int[] changePrice;
    }

    public class PetEquipStatData
    {
        public int step;
        public StatType valueType;
        public double value;
    }

    public class CompatibilityData
    {
        public ElementalType type;
        public ElementalType weakType;
        public ElementalType strongType;
    }

    public class RaidRewardData
    {
        public ElementalType type;
        public Grade rankGrade;
        public double minValue;
        public double maxValue;
        public string reward;
    }

    public class WeekendDoubleData
    {
        public float expBonusValue;
        public float goldBonusValue;
        public float petFoodBonusValue;
        public float stoneBonusValue;
        public float reforgeStoneBonusValue;
        public float threadBonusValue;
        public DayOfWeek startDayOfWeek;
        public DayOfWeek endDayOfWeek;
        public int beginnerSupportLevel;
        public bool active;
    }

    public class DoubleEventData
    {
        public string id;
        public int startDate;
        public int endDate;
    }

    public class EventData
    {
        public string id;
        public EventType eventType;
        public int startDate;
        public int endDate;
        public bool attendance;
        public EventQuestUpdateType quest;
        public bool pass;
        public bool shop;
        public bool active;
        public float currencyDropProb;
        public int currencyDropAmount;
    }

    public class EventShopData
    {
        public string id;
        public string eventId;
        public string reward;
        public int price;
        public int purchaseCount;
        public string spriteImg;
    }

    public class CostumeCommonData
    {
        public string playerDefaultLibraryBodyAsset;
        public string playerDefaultLibraryHandAsset;
        public string playerDefaultJoustingBodyLibraryAsset;
        public string playerDefaultLibraryEtcAsset;
        public string playerDefaultWeapon;
        public string playerDefaultSkill;
        public string playerDefaultUISprite;
    }

    public class CostumeData
    {
        public string id;
        public bool defaultCostume;
        public Grade grade;
        public CostumeType type;
        public string[] ownStats;
        public int maxLevel;
        public string productId;
        public int price;
        public int mileage;
        public int requireLimitBreak;
        public string eventId;
        public double gemPrice;
        public string uiSprite;
        public string libraryAsset; // 기본
        public string sprite;       // 무기
        public string skillSprite;  // 무기
        public string joustingLibraryAsset;     // 마상시합 전용
        public string handLibraryAsset;         // 손
        public int orderInLayer;
    }

    public class CostumeUpgradeData
    {
        public Grade grade;
        public double baseRequire;
        public double[] require;
    }

    public class CostumeShopData
    {
        public string id;
        public CostumeType type;
        public string reward;
        public double price;
    }

    public class EssenceCommonData
    {
        public int centralEssenceDailyLimitCount;
        public float centralEssenceDurationTime;
        public float centralEssenceActiveValue;
        public string centralEssenceFx;
    }

    public class EssenceData
    {
        public EssenceType type;
        public StatType valueType;
    }

    public class EssenceStepData
    {
        public EssenceType type;
        public int step;
        public int maxLevel;
        public int requireCentralStep;
    }

    public class CentralEssenceStepData
    {
        public int step;
        public int requireAllEssenceLevel;
    }

    public class CentralEssenceActiveRequireData
    {
        public int step;
        public int requireAllEssenceAmount;
    }

    public class EssenceStatValueData
    {
        public EssenceType type;
        public float[] value;
    }

    public class CentralEssenceStatValueData
    {
        public int step;
        public float value;
    }

    public class EssenceUpgradeData
    {
        public EssenceType type;
        public int[] require;
    }

    public class SkipBattleCommonData
    {
        public int dailyMaxCount;
        public int maxTimeForMinute;
    }

    public class SkipBattlePriceData
    {
        public int[] price;
    }

    public class SkipBattleMonsterKillData
    {
        public int stageMax;
        public int stageMin;
        public int monsterKillCalcMinute;
    }

    public class LottoCommonData
    {
        public int dailyMaxCount;
        public int coolTimeForMinute;
    }

    public class LottoProbData
    {
        public float[] prob;
    }

    public class LottoRewardData
    {
        public int rewardIndex;
        public float prob;
        public string reward;
        public string sprite;
        public int priority;
    }

    public class CollectionData
    {
        public string id;
        public ItemType itemType;
        public string[] requireId;
        public int[] requireLevel;
        public string rewardStat;
    }

    public class CollectionStatData
    {
        public string id;
        public StatType valueType;
        public double statValue;
    }

    public class JoustingCommonData
    {
        public int dailyWatchAdCount;
        public int dailyFreeTicket;
        public int entranceRequireTicket;
        public int sweepRequireTicket;
        public int winRankingScore;
        public int loseRankingScore;
        public float weakTypeAtkWinRate;
        public float strongTypeAtkWinRate;
        public int resultCoinReward;
        public int resultGloryTokenReward;
        public string matchingUuid_kor;
        public string matchingUuid_glb;
    }

    public class JoustingCompatibilityData
    {
        public JoustingAttackType type;
        public JoustingAttackType weakType;
        public JoustingAttackType strongType;
    }

    public class JoustingTierData
    {
        public int rankScore;
        public JoustingTier tier;
    }
    
    public class JoustingGloryOrbData
    {
        public JoustGloryOrbType type;
        public StatType valueType;
    }

    public class JoustingGloryOrbStepData
    {
        public int step;
        public JoustGloryOrbType type;
    }

    public class JoustingGloryOrbUpgradeData
    {
        public JoustGloryOrbType type;
        public int[] require;
    }

    public class JoustingGloryOrbStatData
    {
        public JoustGloryOrbType type;
        public double[] statValue;
    }

    public class AchievementCommonData
    {
        public string defaultFrame;
        public int dailyLimitChatMessageCount;
    }

    public class AchievementData
    {
        public string id;
        public AchievementType type;
        public string rewardStat;
        public string quest;
        public string key;
        public bool hideDesc;
        public string uiSprite;
    }

    public class AchievementStatData
    {
        public string id;
        public StatType valueType;
        public double statValue;
    }

    public class ExcaliburData
    {
        public ExcaliburForceType type;
        public StatType valueType;
        public float levelUpValue;
        public string sprite;
    }

    public class ExcaliburStepData
    {
        public int step;
        public int requireUltimateLimitBreakStep;
    }

    public class ExcaliburForceStepData
    {
        public ExcaliburForceType type;
        public int step;
        public int maxLevel;
        public int requireExcalibuerStep;
    }

    public class ExcaliburUpgradeData
    {
        public ExcaliburForceType type;
        public int[] require;
    }

    public class AccessoryCommonData
    {
        public int ownOptionMaxCount;
        public int reforgeAddMaxLevel;
        public float reforgeFailedBonusProbValue;
        public int optionStatMaxCount;
        public int optionStatMaxPreset;
        public int combineCount;
        public int necklaceMaxEquipCount;
        public int earringMaxEquipCount;
        public int ringMaxEquipCount;
    }

    public class AccessoryData
    {
        public string id;
        public ItemType type;
        public Grade grade;
        public SubGrade subGrade;
        public StatType valueType;
        public double baseValue;
        public double levelUpValue;
        public string[] ownStats;
        public int combineCount;
        public int maxLevel;
        public string sprite;
    }

    public class ManaHeartData
    {
        public ManaHeartType type;
        public StatType valueType;
        public float levelUpValue;
        public string sprite;
    }

    public class ManaHeartStepData
    {
        public int step;
        public int requireLevel;
        public int maxUpgradeStep;
    }

    public class ManaHeartUpgradeStepData
    {
        public int step;
        public ManaHeartType type;
    }

    public class ManaHeartUpgradeData
    {
        public ManaHeartType type;
        public int[] require;
    }

    public class ManaHeartStatData
    {
        public ManaHeartType type;
        public double[] statValue;
    }

    public class PresetData
    {
        public int preset;
        public int unlockPrice;
        public int requireUnlockPreset;
    }

    // 데이터 마지막

    public class ThanksToData
    {
        public string name;
    }

    public class RaidBadData
    {
        public string uuid;
        public string name;
        public int count;
    }

    public class StringTable
    {
        public string key;
        public string[] text;
    }

    public class BadWord
    {
        public string word;
    }

    

    public struct Pos
    {
        public float X;
        public float Y;

        public static Pos Parse(string value)
        {
            string first = string.Empty;
            string second = string.Empty;

            if (value.IsValid() && value.SplitWord(',', out first, out second))
            {
                return new Pos() { X = first.ToFloatSafe(), Y = second.ToFloatSafe() };
            }
            else
            {
                float v = first.ToFloatSafe();
                return new Pos() { X = v, Y = v };
            }
        }
    }

    public struct RewardResult
    {
        public double exp;
        public double gold;
        public double gem;
        public double stones;
        public double reforgeStones;
        public int skillPiece;
        public int petFood;
        public int elementalStone;
        public int mileage;
        public int ticketBundle;
        public int ancientEssence;
        public int[] tickets;
        public int[] demonicRealmStones;
        public int[] essences;
        public int joustTicket;
        public int joustCoin;
        public int gloryToken;
        public int soulStone;
        public int manaEssence;
        public double costumeUpgrade;
        public MultiReward[] equipments;
        public MultiReward[] accessorys;
        public MultiReward[] skills;
        public MultiReward[] artifacts;
        public MultiReward[] costumes;
        public MultiReward[] eventCurrencys;
        public MultiReward[] achievements;
        public MultiReward[] petEquipments;

        public bool IsEmpty()
        {
            return exp == 0 &&
                gold == 0 &&
                gem == 0 &&
                stones == 0 &&
                reforgeStones == 0 &&
                skillPiece == 0 &&
                petFood == 0 &&
                elementalStone == 0 &&
                mileage == 0 &&
                ticketBundle == 0 &&
                ancientEssence == 0 &&
                joustTicket == 0 &&
                joustCoin == 0 &&
                gloryToken == 0 &&
                soulStone == 0 &&
                manaEssence == 0 &&
                costumeUpgrade == 0 &&
                tickets == null &&
                demonicRealmStones == null &&
                essences == null &&
                equipments == null &&
                accessorys == null &&
                skills == null &&
                artifacts == null &&
                costumes == null &&
                eventCurrencys == null &&
                achievements == null &&
                petEquipments == null;
        }
    }

    public struct MultiReward // 장비, 스킬 사용
    {
        public ItemType ItemType;
        public string Id;
        public int Count;

        public MultiReward(ItemType itemType, string id, int count)
        {
            ItemType = itemType;
            Id = id;
            Count = count;
        }

        public static MultiReward Parse(string value)
        {
            string first = string.Empty;
            string second = string.Empty;

            if (value.IsValid() && value.SplitWord('X', out first, out second))
            {
                return new MultiReward() { Id = first, Count = second.ToIntSafe(1) };
            }
            else
            {
                return new MultiReward() { Id = first, Count = 1 };
            }
        }
    }

    public struct MinMaxRange
    {
        public int min;
        public int max;

        public int SelectRandom()
        {
            return min == max ? min : UnityEngine.Random.Range(min, max + 1);
        }

        public bool IsInRange(int num)
        {
            return min <= num && num <= max;
        }

        public static MinMaxRange Parse(string value)
        {
            string first, second;

            bool haveRange = value.SplitWord('~', out first, out second);
            if (haveRange)
            {
                return new MinMaxRange() { min = first.ToIntSafe(), max = second.ToIntSafe() };
            }
            else
            {
                int v = first.ToIntSafe();
                return new MinMaxRange() { min = v, max = v };
            }
        }

        public override string ToString()
        {
            return $"{min}~{max}";
        }
    }

    public enum StageDifficulty
    {
        Beginner,
        Easy1, Easy2, Easy3,
        Normal1, Normal2, Normal3,
        Hard1, Hard2, Hard3,
        Expert1, Expert2, Expert3,
        Master1, Master2, Master3,
        Hyper1, Hyper2, Hyper3,
        Another1, Another2, Another3,
        Legend1, Legend2, Legend3,
        Hell1, Hell2, Hell3,
        Chaos1, Chaos2, Chaos3, Chaos4, Chaos5,
        Nightmare1, Nightmare2, Nightmare3, Nightmare4, Nightmare5,
        Inferno1, Inferno2, Inferno3, Inferno4, Inferno5,
        Void1, Void2, Void3, Void4, Void5, Void6, Void7, Void8, Void9, Void10,
        Despair1, Despair2, Despair3, Despair4, Despair5,
        Curse1, Curse2, Curse3, Curse4, Curse5,
        Oblivion1, Oblivion2, Oblivion3, Oblivion4, Oblivion5,
        Ruin1, Ruin2, Ruin3, Ruin4, Ruin5,
        Darkness1, Darkness2, Darkness3, Darkness4, Darkness5,
        Pain1, Pain2, Pain3, Pain4, Pain5,
        Terror1, Terror2, Terror3, Terror4, Terror5,            // 공포
        Abyss1, Abyss2, Abyss3, Abyss4, Abyss5,                 // 심연
        Eternal1, Eternal2, Eternal3, Eternal4, Eternal5,       // 영원
        Pandemonium1, Pandemonium2, Pandemonium3, Pandemonium4, Pandemonium5,    // 대혼란
        Cataclysm1, Cataclysm2, Cataclysm3, Cataclysm4, Cataclysm5,    // 대재앙
        Apocalypse1, Apocalypse2, Apocalypse3, Apocalypse4, Apocalypse5,    // 종말
        Fury1, Fury2, Fury3, Fury4, Fury5,    // 분노
        Wrath1, Wrath2, Wrath3, Wrath4, Wrath5,    // 격노

        Count
    }

    public enum StageType
    {
        Normal,
        Gold,
        Stone,
        Raid,
        LimitBreak,
        Pet,
        Reforge,
        Growth,
        Ancient,

        Jousting,
        UltimateLimitBreak,

        Accessory,

        Count
    }

    public enum DungeonType
    {
        Gold,
        Stone,
        Raid,
        Pet,
        Reforge,
        Growth,
        Ancient,

        Count
    }

    public enum DemonicRealmType
    {
        Accessory,

        Count,
    }

    public enum PetEvolutionStatGrade
    {
        C, B, A, S, SS, SSS, SR,

        Count
    }

    public enum EquipmentOptionStatGrade
    {
        C, B, A, S, SS, SSS, SR,

        Count
    }

    public enum Grade
    {
        D, C, B, A, S, SS, SSS,

        SR, SSR,

        Count
    }

    public enum SubGrade
    {
        One_Star, 
        Two_Star, 
        Three_Star, 
        Four_Star, 
        Five_Star,

        Count,
        Any,
    }

    public enum UISubGrade
    {
        One_Star,
        Two_Star,
        Three_Star,
        Four_Star,
        Five_Star,
        Six_Star,
        Seven_Star,
        Eight_Star,
        Nine_Star,
        Ten_Star,

        Count,
    }

    public enum SkillType
    {
        Active,
        Passive,

        Count
    }

    public enum SkillCondition
    {
        None,
        KillMonster,
        TargetHpRateUnder,
        HpRateOver,
        AttackBoss,

        Count
    }

    public enum SkillValueCalcType
    {
        None,
        CriProbOverToCriDmg,
        HpToAtk,
        AtkSpeedOverToSkillDmg,

        Count
    }

    public enum SkillLevelUpMaterial
    {
        Skill,
        SkillPiece,

        Count,
    }

    public enum MonsterType
    {
        monster,
        boss,
        raidBoss,
        ancientBoss,
        petBoss,

        Count
    }

    public enum PassType
    {
        Stage,
        Level,
        Spawn,
        PlayTime,
        EventCurrency,

        Count
    }

    public enum RewardType
    {
        OnlyOne,
        Repeat,

        Count
    }

    public enum SpawnType
    {
        Gem,
        Ad,
        AncientEssence,

        Count,
    }

    public enum EquipmentType
    {
        Weapon,
        Armor,
        Gloves,
        Shoes,

        Count
    }

    public enum AccessoryType
    {
        Necklace,
        Earring,
        Ring,

        Count
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Gloves,
        Shoes,
        Skill,
        Artifact,

        Gold,
        Gem,
        UpgradeStone,
        GoldTicket,
        StoneTicket,
        RaidTicket,
        Mileage,
        Exp,

        Random_Weapon,
        Random_Armor,
        Random_Gloves,
        Random_Shoes,
        Random_Equipment,
        Random_Skill,

        TicketBundle,
        Advertisement,
        SpeedMode,

        Pet,
        PetTicket,
        PetFood,
        ElementalStone,

        EventCurrency,
        Buff,
        Costume,
        SkillPiece,

        Essence_Chapter1,
        Essence_Chapter2,
        Essence_Chapter3,
        Essence_Chapter4,
        Essence_Chapter5,

        ReforgeStone,
        ReforgeTicket,

        GrowthTicket,
        AncientArtifact,
        AncientEssence,
        AncientTicket,

        JoustingTicket,
        JoustingCoin,
        Reward,
        Achievement,
        PetEquipment,
        GloryToken,

        Necklace,
        Earring,
        Ring,

        Random_Necklace,
        Random_Earring,
        Random_Ring,
        Random_Accessory,

        DemonicRealmStone_Accessory,

        SoulStone,
        CostumeUpgrade,
        ManaEssence,

        Count
    }

    public enum EssenceType
    {
        Chapter1,
        Chapter2,
        Chapter3,
        Chapter4,
        Chapter5,

        Count,

        Central,
    }

    public enum OptionMethodType
    {
        Ratio,
        Add,

        Count
    }

    public enum StatType
    {
        Atk,
        AtkRatio,
        AtkRange,
        Hp,
        HpRatio,
        CriProb,
        CriDmg,
        AtkSpeed,
        AtkSpeedRatio,
        MoveSpeed,
        MoveSpeedRatio,
        AddDmg,
        BossDmg,
        MonsterDmg,
        ExpAmount,
        GoldAmount,
        SkillDmg,
        ReduceSkillCoolTime,

        SuperCriProb,
        SuperCriDmg,

        FireAddDmg,
        WaterAddDmg,
        GrassAddDmg,

        PowerAtk,
        PowerHp,

        AmplifyAtk,
        AmplifyHp,

        Level,

        ManaSensitivity,

        Count,
    }

    public enum QuestCheckType
    {
        Attain,
        Stack,

        Count
    }

    public enum QuestType
    {
        None,
        DailyquestClear,    // o
        WeeklyquestClear,   // o
        RepeatquestClear,   // o
        WatchAd,            // 
        PlayTime,           // o
        Spawn,              // o
        SpawnEquipment,     // o
        SpawnWeapon,        // o
        SpawnArmor,
        SpawnGloves,
        SpawnShoes,
        SpawnSkill,         // o
        SpawnArtifact,
        KillMonster,        // o
        KillBoss,           // o
        CombineEquipment,   // o
        ClearStage,         // o
        ClearDungeon,       // o
        ClearGoldDungeon,   // o
        ClearStoneDungeon,  // o
        TryRaidDungeon,     // o
        Train,              // o
        
        UpgradeEquipment,   // o
        TryUpgradeArtifact, // o
        TrainAtk,           // o
        TrainHp,            // o
        TrainCriProb,       // o
        TrainCriDmg,        // o
        CombineWeapon,      // o
        EquipWeapon,        // o
        EquipArmor,
        EquipGloves,
        EquipShoes,
        UpgradeWeapon,      // o
        LevelUpCharacter,   // o
        LevelUpAbility,     // o
        CastSkill,
        EquipActiveSkill,         // o
        EquipPassiveSkill,         // o
        UpgradeSkill,
        UpgradeActiveSkill,       // o
        UpgradePassiveSkill,       // o
        ActiveAutoChallenge,// o
        ActiveAutoCastSkill,// o
        ActiveBuff,         // o
        ConfirmAttendance,  // o
        LimitBreak,         // o
        ConfirmRaidRank,

        EventquestClear,
        Login,
        ActiveSpeedMode,

        PurchaseTicket,                 // 일반 상점에서 티켓 구매
        EventquestClearByOpenDay,       // o openDay 같은 퀘스트 클리어 확인
        LevelUpPet,                     // o
        EvolutionPet,                   // o
        ClearBountyQuest,               // o
        LevelUpCentralEssence,          // o
        Payments,                       // o

        ConfirmEssence,                 // 정수 팝업 확인
        EquipPet,                       // 신수 장착 확인
        ClearPetDungeon,
        ClearReforgeDungeon,
        ClearGrowthDungeon,
        ClearAncientDungeon,
        ConfirmJousting,                //

        UpgradeAllArtifactMaxLevel,

        TrainPowerAtk,           // o
        TrainPowerHp,            // o
        TrainSuperCriProb,       // o
        TrainSuperCriDmg,        // o

        TryUpgradeAncientArtifact,
        SendChatMessage,        // 채팅침
        JoustingComboAtk,       // 마상시합 연속 공격 횟수


        SpawnNecklace,        
        SpawnEarring,
        SpawnRing,

        Count
    }

    public enum QuestUpdateType
    {
        Daily,
        Weekly,
        Repeat,
        None,
        Count
    }

    public enum EventQuestUpdateType
    {
        None,
        Daily,
        Normal,
        DayOpen,

        Count
    }

    public enum AttendanceType
    {
        Onlyone,
        Repeat,

        Count
    }

    public enum AttendanceDayType
    {
        D7,
        D15,

        Count
    }

    public enum GuideActionType
    {
        Move_StatureTab,                // 
        Move_Stature_TrainTab,          //
        Move_Stature_AbilityTab,        // 
        Move_Stature_ArtifactTab,       // 
        Move_Stature_LimitBreakTab,     // 

        Move_InventoryTab,              // 
        Move_Inventory_WeaponTab,       //
        Move_Inventory_ArmorTab,        //
        Move_Inventory_GlovesTab,       //
        Move_Inventory_ShoesTab,        //

        Move_SkillTab,                  // 

        Move_SpawnTab,                  //  
        Move_Spawn_EquipmentTab,        //
        Move_Spawn_SkillTab,            // 
        Move_Spawn_ArtifactTab,         //

        Move_DungeonTab,                //
        Move_ShopTab,                   //

        Highlight_TrainAtkButton,       // o
        Highlight_TrainHpButton,        // o
        Highlight_TrainCriProbButton,   // o
        Highlight_TrainCriDmgButton,    // o
                                        
        Highlight_FirstAbility,         // o
        Highlight_LevelUpAbilityButton, // o

        Highlight_ChallengeBoss,        // o
        Highlight_AutoChallengeBoss,    // o

        Highlight_SpawnWeaponButton,        // o
        Highlight_SpawnArmorButton,         // o
        Highlight_SpawnGlovesButton,        // o
        Highlight_SpawnShoesButton,         // o
        Highlight_SpawnSkillButton,         // o
        Highlight_SpawnArtifactButton,      // o

        Highlight_BestEquipment,            // o
        Highlight_CombineEquipmentButton,   // o
        Highlight_EquipEquipmentButton,     // o
        Highlight_UpgradeEquipmentButton,   // o

        Highlight_BestSkill,                // o
        Highlight_EquipActiveSkillButton,   // o
        Highlight_UpgradeActiveSkillButton, // o
        Highlight_EquipPassiveSkillButton,  // o
        Highlight_UpgradePassiveSkillButton,// o
        Highlight_AutoCastSkillButton,      // o

        Highlight_GoldDungeon,              // o
        Highlight_StoneDungeon,             // o
        Highlight_RaidDungeon,              // o

        Highlight_BuffButton,               // o
        Highlight_AtkBuffButton,            // o
        Highlight_GoldBuffButton,           // o
        Highlight_ExpBuffButton,            // o

        Highlight_SettingButton,            // o
        Highlight_AttendanceButton,         // o
        Highlight_RankButton,               // o
        Highlight_QuestButton,              // o
        Highlight_RepeatQuestButton,        // o
        
        Highlight_LimitBreakButton,         // o

        Move_Skill_ActiveTab,               // o
        Move_Skill_PassiveTab,              // o
        Highlight_CanUpgradeSkill,          // o
        Highlight_AllCombineWeaponButton,   // o
        Highlight_AllCombineArmorButton,    // o
        Highlight_AllCombineGlovesButton,   // o
        Highlight_AllCombineShoesButton,    // o

        Highlight_EquipSkillButton,   // o
        Highlight_UpgradeSkillButton, // o
        Highlight_FirstSkillCastSlot,
        Highlight_RaidRankButton,
        Highlight_CanUpgradeArtifact,

        JustWait,

        Highlight_SpawnWeaponButton_Parent,        // o
        Highlight_SpawnArmorButton_Parent,         // o
        Highlight_SpawnGlovesButton_Parent,        // o
        Highlight_SpawnShoesButton_Parent,         // o
        Highlight_TrainAtkButton_Parent,       // o
        Highlight_TrainHpButton_Parent,        // o
        Highlight_TrainCriProbButton_Parent,   // o
        Highlight_TrainCriDmgButton_Parent,    // o
        Highlight_GuideQuest,

        Highlight_PetDungeon,              // o

        Highlight_SpeedModeButton,
        OpenMenu,

        Move_PetTab,
        Highlight_BestPet,
        Highlight_EquipPetButton,
        Highlight_FeedPetButton,

        Highlight_BountyButton,
        Highlight_EssenceButton,

        Highlight_ReforgeDungeon,
        Highlight_GrowthDungeon,

        Highlight_CanEquipBestSkill,
        Highlight_AncientDungeon,
        Highlight_JoustingButton,

        Count,
    }

    public enum PackageCategory
    {
        Special,
        Daily,
        Weekly,
        Monthly,
        MonthlyFee,
        Step,

        Count
    }

    public enum PackageType
    {
        RemoveAD,
        Reward,
        StepReward,
        MonthlyFee,

        Count,
    }

    public enum PackageResetType
    {
        None,
        Daily,
        Weekly,
        Monthly,
        MonthlyFee,

        Count
    }

    public enum ShowRewardType
    {
        None,
        Popup,
        Banner,
        FirstStageClear,
    }

    public enum ArtifactLevelUpResult
    {
        Error,
        Fail,
        Success
    }

    public enum AdType
    {
        // 버프 3종
        Active_Buff_Atk,
        Active_Buff_Gold,
        Active_Buff_Exp,

        // 오프라인 보상 보너스
        Offline_BonusReward,

        // 일일 무료 잼
        DailyGem,

        // 일일 무료 티켓
        Daily_DungeonTicket_Gold,
        Daily_DungeonTicket_Stone,
        Daily_DungeonTicket_Pet,
        Daily_DungeonTicket_Raid,

        // 일일 무료 소환
        Daily_Spawn_Weapon,
        Daily_Spawn_Armor,
        Daily_Spawn_Gloves,
        Daily_Spawn_Shoes,
        Daily_Spawn_Skill,
        Daily_Spawn_Artifact,

        // 스피드 모드
        SpeedMode,

        Daily_DungeonTicket_Reforge,
        Daily_DungeonTicket_Growth,
        Daily_DungeonTicket_Ancient,

        Daily_JoustingTicket,

        Daily_QuestBonus,


        Daily_Spawn_Necklace,
        Daily_Spawn_Earring,
        Daily_Spawn_Ring,
        Daily_DemonicRealm_Accessory,
    }

    public enum ElementalType
    {
        Fire,
        Grass,
        Water,

        Count,

        Normal,
    }

    public enum RageActionType
    {
        Knockback,
        AtkSpeedUp,
        Shield,

        Count,
    }

    public enum EventType
    {
        Childrensday,
        Newbie,
        TrainSupport,
        Payback,
        Summer202406,
        LiberationDay2024,
        Chuseok2024,
        Halloween2024,
        ThanksTo10000,
        Pepero2024,
        Christmas2024,
        HappyNewYear2024,
        SnakeNewYear2025,
        NewbieSupportBuff,
        DailyPayback,
    }

    public enum CostumeType
    {
        Body,
        Weapon,
        Etc,

        Count,
    }

    public enum JoustingAttackType
    {
        Head,
        Body,
        Arm,

        Count,
        None,
    }

    public enum JoustingTier
    {
        Bronze_5, Bronze_4, Bronze_3,
        Bronze_2, Bronze_1,
        Silver_5, Silver_4, Silver_3,
        Silver_2, Silver_1,
        Gold_5, Gold_4, Gold_3,
        Gold_2, Gold_1,
        Platinum_4, Platinum_3, Platinum_2, Platinum_1,
        Diamond_3, Diamond_2, Diamond_1,
        Master_3, Master_2, Master_1,
        Challenger,

        None,
    }

    public enum AchievementType
    {
        Default,
        Reward,
        Quest,
        Count
    }

    public enum ExcaliburForceType
    {
        Force_1,
        Force_2,
        Force_3,
        Force_4,
        Force_5,
        Force_6,
        Force_7,

        Count,
    }

    public enum JoustGloryOrbType
    {
        GloryOrb_1,
        GloryOrb_2,
        GloryOrb_3,
        GloryOrb_4,
        GloryOrb_5,
        GloryOrb_6,
        GloryOrb_7,

        Count,
    }

    public enum ManaHeartType
    {
        Mana_1,
        Mana_2,
        Mana_3,
        Mana_4,
        Mana_5,
        Mana_6,

        Count,
    }


    public enum LangCode
    {
        KR, //
        US, //

        Count
    }

}