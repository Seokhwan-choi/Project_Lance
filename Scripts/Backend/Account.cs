using CodeStage.AntiCheat.ObscuredTypes;
using System;
using System.Linq;
using System.Collections.Generic;
using BackEnd;
using BackEnd.Group;
using UnityEngine;

namespace Lance
{
    public partial class Account
    {
        public UserInfo UserInfo = new();
        public WeaponInventory WeaponInventory = new();     // 무기 인벤토리
        public ArmorInventory ArmorInventory = new();       // 갑옷 인벤토리
        public GlovesInventory GlovesInventory = new();     // 장갑 인벤토리
        public ShoesInventory ShoesInventory = new();       // 신발 인벤토리
        public SkillInventory SkillInventory = new();       // 스킬 인벤토리
        public CurrencyInfo Currency = new();               // 재화
        public ExpLevel ExpLevel = new();                   // 경험치
        public GoldTrainLevel GoldTrain = new();            // 골드 훈련
        public StageRecords StageRecords = new();           // 스테이지
        public Shop Shop = new();                           // 상점
        public NormalShop NormalShop = new();               // 일반 상점
        public MileageShop MileageShop = new();             // 마일리지 상점
        public PackageShop PackageShop = new();             // 패키지 상점
        public SpawnInfo Spawn = new();                     // 소환
        public Pass Pass = new();                           // 패스

        public AccessoryNecklaceInventory NecklaceInventory = new();    // 악세사리 - 목걸이 인벤토리
        public AccessoryEarringInventory EarringInventory = new();      // 악세사리 - 귀고리 인벤토리
        public AccessoryRingInventory RingInventory = new();            // 악세사리 - 반지 인벤토리

        public PetFireInventory PetFireInventory = new();   // 신물 - 불 인벤토리
        public PetWaterInventory PetWaterInventory = new(); // 신물 - 물 인벤토리
        public PetGrassInventory PetGrassInventory = new(); // 신물 - 풀 인벤토리

        public DailyQuest DailyQuest = new();               // 일일 퀘스트
        public WeeklyQuest WeeklyQuest = new();             // 주간 퀘스트
        public RepeatQuest RepeatQuest = new();             // 반복 퀘스트
        public GuideQuest GuideQuest = new();               // 가이트 퀘스트
        public Attendance Attendance = new();               // 출석부
        public Artifact Artifact = new();                   // 유물
        public AncientArtifact AncientArtifact = new();     // 고대 유물
        public Excalibur Excalibur = new();                 // 엑스칼리버
        public Buff Buff = new();				            // 버프
        public Dungeon Dungeon = new();                     // 던전
        public DemonicRealm DemonicRealm = new();               // 마계
        public DemonicRealmSpoils DemonicRealmSpoils = new();   // 마계 전리품
        public BeginnerRaidScore BeginnerRaidScore = new();         // 견습 레이드 점수 저장
        public NewBeginnerRaidScore NewBeginnerRaidScore = new();   // 견습 레이드 랭킹 정산 시간 바꿔야되서 이딴 똥같은 옮기는 클래스가 필요함
        public Ability Ability = new();                     // 특성
        public SpeedMode SpeedMode = new();                 // 스피드 모드
        public Pet Pet = new();                             // 신수
        public Costume Costume = new();                     // 코스튬
        public BountyQuest Bounty = new();                  // 현상금
        public Essence Essence = new();                     // 정수
        public Event Event = new();                         // 이벤트 총괄
        public Lotto Lotto = new();                         // 행운 복권 (로또)
        public Collection Collection = new();               // 도감
        public JoustBattleInfo JoustBattleInfo = new();     // 마상시합 전투 정보
        public JoustRankInfo JoustRankInfo = new();         // 마상시합 랭킹 정보
        public JoustShop JoustShop = new();                 // 마상시합 상점
        public JoustGloryOrb JoustGloryOrb = new();         // 마상시합 영광의 보주
        public Achievement Achievement = new();             // 업적
        public RankProfile RankProfile = new();             // 랭킹 프로필
        public ManaHeart ManaHeart = new();                 // 마나하트
        public Preset Preset = new();                       // 프리셋

        // 사용안함
        //public EventQuest EventQuest = new();               // 이벤트 퀘스트
        //public EventPass EventPass = new();                 // 이벤트 패스
        //public EventShop EventShop = new();                 // 이벤트 상점


        // AccountInfos에 포함되어 있지 않음
        // 잘 관리 할 것
        //public Rank Rank = new();                         // 랭킹
        public Leaderboard Leaderboard = new();             // 리더보드 ( new 랭킹 )
        public Post Post = new();                           // 우편함
        public Coupon Coupon = new();                       // 쿠폰
        public Notice Notice = new();                       // 공지사항
        public ObscuredString CountryCode = string.Empty;   // 로그인하면서 국가코드 조회후 캐싱
        public ObscuredString GroupUuid = string.Empty;     // 로그인하면서 그룹정보 조회후 캐싱
        public ObscuredString GroupName = string.Empty;

        public readonly Dictionary<string, AccountBase>
            AccountInfos = new Dictionary<string, AccountBase>();

        public readonly List<Inventory> Inventorys = new List<Inventory>();
        public readonly List<AccessoryInventory> AccessoryInventorys = new List<AccessoryInventory>();
        public readonly List<PetInventory> PetInventorys = new List<PetInventory>();
        public readonly List<Quest> Quests = new List<Quest>();
        public bool IsKorean => CountryCode == "KR";
        public Account()
        {
            AccountInfos.Add("user", UserInfo);
            AccountInfos.Add("w_Inven", WeaponInventory);
            AccountInfos.Add("a_Inven", ArmorInventory);
            AccountInfos.Add("g_Inven", GlovesInventory);
            AccountInfos.Add("s_Inven", ShoesInventory);
            AccountInfos.Add("currency", Currency);
            AccountInfos.Add("goldTrain", GoldTrain);
            AccountInfos.Add("stage", StageRecords);
            AccountInfos.Add("expLevel", ExpLevel);
            AccountInfos.Add("shop", Shop);
            AccountInfos.Add("normalShop", NormalShop);
            AccountInfos.Add("mileageShop", MileageShop);
            AccountInfos.Add("packageShop", PackageShop);
            AccountInfos.Add("spawn", Spawn);
            AccountInfos.Add("skill_Inven", SkillInventory);
            AccountInfos.Add("pass", Pass);

            AccountInfos.Add("attendance", Attendance);
            AccountInfos.Add("artifact", Artifact);
            AccountInfos.Add("ancientArtifact", AncientArtifact);
            AccountInfos.Add("excalibur", Excalibur);
            AccountInfos.Add("buff", Buff);
            AccountInfos.Add("dungeon", Dungeon);
            AccountInfos.Add("demonicRealm", DemonicRealm);
            AccountInfos.Add("demonicRealmSpoils", DemonicRealmSpoils);
            AccountInfos.Add("beginnerRaidScore", BeginnerRaidScore);
            AccountInfos.Add("newBeginnerRaidScore", NewBeginnerRaidScore);
            AccountInfos.Add("ability", Ability);
            AccountInfos.Add("speedMode", SpeedMode);
            AccountInfos.Add("pet", Pet);

            AccountInfos.Add("costume", Costume);
            AccountInfos.Add("bounty", Bounty);
            AccountInfos.Add("essence", Essence);

            AccountInfos.Add("lotto", Lotto);
            AccountInfos.Add("collection", Collection);
            AccountInfos.Add("joust_battleInfo", JoustBattleInfo);
            AccountInfos.Add("joust_rankInfo", JoustRankInfo);
            AccountInfos.Add("joustShop", JoustShop);
            AccountInfos.Add("joustGloryOrb", JoustGloryOrb);

            AccountInfos.Add("ac_n_inven", NecklaceInventory);
            AccountInfos.Add("ac_e_inven", EarringInventory);
            AccountInfos.Add("ac_r_inven", RingInventory);

            AccountInfos.Add("p_f_inven", PetFireInventory);
            AccountInfos.Add("p_w_inven", PetWaterInventory);
            AccountInfos.Add("p_g_inven", PetGrassInventory);

            AccountInfos.Add("dailyQuest", DailyQuest);
            AccountInfos.Add("weeklyQuest", WeeklyQuest);
            AccountInfos.Add("repeatQuest", RepeatQuest);
            AccountInfos.Add("guideQuest", GuideQuest);

            AccountInfos.Add("event", Event);

            AccountInfos.Add("achievement", Achievement);
            AccountInfos.Add("rankProfil", RankProfile);
            AccountInfos.Add("manaHeart", ManaHeart);
            AccountInfos.Add("preset", Preset);

            Inventorys.Add(WeaponInventory);
            Inventorys.Add(ArmorInventory);
            Inventorys.Add(GlovesInventory);
            Inventorys.Add(ShoesInventory);

            AccessoryInventorys.Add(NecklaceInventory);
            AccessoryInventorys.Add(EarringInventory);
            AccessoryInventorys.Add(RingInventory);

            PetInventorys.Add(PetFireInventory);
            PetInventorys.Add(PetWaterInventory);
            PetInventorys.Add(PetGrassInventory);

            Quests.Add(DailyQuest);
            Quests.Add(WeeklyQuest);
            Quests.Add(RepeatQuest);
        }

        public void RandomizeKey()
        {
            foreach(var accountBase in AccountInfos.Values)
            {
                accountBase.RandomizeKey();
            }

            Leaderboard.RandomizeKey();
            Post.RandomizeKey();
        }

        public int StackExp(double exp)
        {
            return ExpLevel.StackExp(exp);
        }

        public bool UpdateStageRecord(int totalStage)
        {
            return StageRecords.UpdateRecord(totalStage);
        }

        public void GiveReward(RewardResult reward)
        {
            double prevGem = Currency.GetGem();

            // 재화 처리
            Currency.AddGold(reward.gold);
            Currency.AddGem(reward.gem);
            Currency.AddUpgradeStones(reward.stones);
            Currency.AddReforgeStone(reward.reforgeStones);
            Currency.AddSkillPiece(reward.skillPiece);
            Currency.AddPetFood(reward.petFood);
            Currency.AddElementalStone(reward.elementalStone);
            Currency.AddAncientEssence(reward.ancientEssence);
            Currency.AddJoustingTicket(reward.joustTicket);
            Currency.AddJoustingCoin(reward.joustCoin);
            Currency.AddGloryToken(reward.gloryToken);
            Currency.AddSoulStone(reward.soulStone);
            Currency.AddManaEssence(reward.manaEssence);
            Currency.AddCostumeUpgrade(reward.costumeUpgrade);
            MileageShop.AddMileage(reward.mileage);
            
            if(reward.eventCurrencys != null)
            {
                for(int i = 0; i < reward.eventCurrencys.Length; ++i)
                {
                    var eventReward = reward.eventCurrencys[i];

                    string eventId = eventReward.Id;
                    int count = eventReward.Count;

                    Event.AddEventCurrency(eventId, count);
                }
            }

            // 경험치
            int levelUpCount = ExpLevel.StackExp(reward.exp);
            if (levelUpCount > 0)
            {
                Lance.GameManager.OnPlayerLevelUp(levelUpCount);
            }

            // 티켓 묶음
            if (reward.ticketBundle > 0)
            {
                for (int i = 0; i < (int)DungeonType.Count; ++i)
                {
                    DungeonType type = (DungeonType)i;

                    Currency.AddDungeonTicket(type, reward.ticketBundle);
                }
            }

            // 티켓
            if (reward.tickets != null)
            {
                for (int i = 0; i < reward.tickets.Length; ++i)
                {
                    DungeonType type = (DungeonType)i;

                    Currency.AddDungeonTicket(type, reward.tickets[i]);
                }
            }

            // 마계석
            if (reward.demonicRealmStones != null)
            {
                for (int i = 0; i < reward.demonicRealmStones.Length; ++i)
                {
                    DemonicRealmType type = (DemonicRealmType)i;

                    Currency.AddDemonicRealmStone(type, reward.demonicRealmStones[i]);
                }
            }

            // 정수
            if (reward.essences != null)
            {
                for (int i = 0; i < reward.essences.Length; ++i)
                {
                    EssenceType type = (EssenceType)i;

                    Currency.AddEssence(type, reward.essences[i]);
                }
            }

            // 장비
            if (reward.equipments != null)
            {
                for (int i = 0; i < reward.equipments.Length; ++i)
                {
                    var equipment = reward.equipments[i];

                    Inventory inventory = GetInventoryByEquipmentId(equipment.Id);
                    if (inventory == null)
                        continue;

                    inventory.AddItem(equipment.Id, equipment.Count);
                }
            }

            // 악세사리
            if (reward.accessorys != null)
            {
                for (int i = 0; i < reward.accessorys.Length; ++i)
                {
                    var accessory = reward.accessorys[i];

                    AccessoryInventory inventory = GetAccessoryInventoryByAccessoryId(accessory.Id);
                    if (inventory == null)
                        continue;

                    inventory.AddItem(accessory.Id, accessory.Count);
                }
            }

            // 스킬
            if (reward.skills != null)
            {
                for (int i = 0; i < reward.skills.Length; ++i)
                {
                    var skill = reward.skills[i];

                    Lance.Account.AddSkill(skill.Id, skill.Count);
                }
            }

            // 유물
            if (reward.artifacts != null)
            {
                for (int i = 0; i < reward.artifacts.Length; ++i)
                {
                    var artifact = reward.artifacts[i];

                    Lance.Account.AddArtifact(artifact.Id, artifact.Count);
                }
            }

            // 코스튬
            if (reward.costumes != null)
            {
                for (int i = 0; i < reward.costumes.Length; ++i)
                {
                    var costume = reward.costumes[i];

                    Lance.Account.Costume.AddCostume(costume.Id);
                }
            }

            // 업적
            if (reward.achievements != null)
            {
                for (int i = 0; i < reward.achievements.Length; ++i)
                {
                    var achievement = reward.achievements[i];

                    Lance.Account.Achievement.CompleteAchievement(achievement.Id);
                }
            }

            // 신물
            if (reward.petEquipments != null)
            {
                for(int i = 0; i < reward.petEquipments.Length; ++i)
                {
                    var equipment = reward.petEquipments[i];

                    PetInventory inventory = GetPetInventoryByEquipmentId(equipment.Id);
                    if (inventory == null)
                        continue;

                    inventory.AddItem(equipment.Id, equipment.Count);
                }
            }

            if (reward.gem > 0)
            {
                Param param = new Param();
                param.Add("prevGem", prevGem);
                param.Add("getGemAmount", reward.gem);
                param.Add("curGem", Currency.GetGem());

                Lance.BackEnd.InsertLog("GetGem", param, 7);
            }
        }
    }
}