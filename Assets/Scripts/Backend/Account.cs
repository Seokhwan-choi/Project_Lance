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
        public WeaponInventory WeaponInventory = new();     // ���� �κ��丮
        public ArmorInventory ArmorInventory = new();       // ���� �κ��丮
        public GlovesInventory GlovesInventory = new();     // �尩 �κ��丮
        public ShoesInventory ShoesInventory = new();       // �Ź� �κ��丮
        public SkillInventory SkillInventory = new();       // ��ų �κ��丮
        public CurrencyInfo Currency = new();               // ��ȭ
        public ExpLevel ExpLevel = new();                   // ����ġ
        public GoldTrainLevel GoldTrain = new();            // ��� �Ʒ�
        public StageRecords StageRecords = new();           // ��������
        public Shop Shop = new();                           // ����
        public NormalShop NormalShop = new();               // �Ϲ� ����
        public MileageShop MileageShop = new();             // ���ϸ��� ����
        public PackageShop PackageShop = new();             // ��Ű�� ����
        public SpawnInfo Spawn = new();                     // ��ȯ
        public Pass Pass = new();                           // �н�

        public AccessoryNecklaceInventory NecklaceInventory = new();    // �Ǽ��縮 - ����� �κ��丮
        public AccessoryEarringInventory EarringInventory = new();      // �Ǽ��縮 - �Ͱ� �κ��丮
        public AccessoryRingInventory RingInventory = new();            // �Ǽ��縮 - ���� �κ��丮

        public PetFireInventory PetFireInventory = new();   // �Ź� - �� �κ��丮
        public PetWaterInventory PetWaterInventory = new(); // �Ź� - �� �κ��丮
        public PetGrassInventory PetGrassInventory = new(); // �Ź� - Ǯ �κ��丮

        public DailyQuest DailyQuest = new();               // ���� ����Ʈ
        public WeeklyQuest WeeklyQuest = new();             // �ְ� ����Ʈ
        public RepeatQuest RepeatQuest = new();             // �ݺ� ����Ʈ
        public GuideQuest GuideQuest = new();               // ����Ʈ ����Ʈ
        public Attendance Attendance = new();               // �⼮��
        public Artifact Artifact = new();                   // ����
        public AncientArtifact AncientArtifact = new();     // ��� ����
        public Excalibur Excalibur = new();                 // ����Į����
        public Buff Buff = new();				            // ����
        public Dungeon Dungeon = new();                     // ����
        public DemonicRealm DemonicRealm = new();               // ����
        public DemonicRealmSpoils DemonicRealmSpoils = new();   // ���� ����ǰ
        public BeginnerRaidScore BeginnerRaidScore = new();         // �߽� ���̵� ���� ����
        public NewBeginnerRaidScore NewBeginnerRaidScore = new();   // �߽� ���̵� ��ŷ ���� �ð� �ٲ�ߵǼ� �̵� �˰��� �ű�� Ŭ������ �ʿ���
        public Ability Ability = new();                     // Ư��
        public SpeedMode SpeedMode = new();                 // ���ǵ� ���
        public Pet Pet = new();                             // �ż�
        public Costume Costume = new();                     // �ڽ�Ƭ
        public BountyQuest Bounty = new();                  // �����
        public Essence Essence = new();                     // ����
        public Event Event = new();                         // �̺�Ʈ �Ѱ�
        public Lotto Lotto = new();                         // ��� ���� (�ζ�)
        public Collection Collection = new();               // ����
        public JoustBattleInfo JoustBattleInfo = new();     // ������� ���� ����
        public JoustRankInfo JoustRankInfo = new();         // ������� ��ŷ ����
        public JoustShop JoustShop = new();                 // ������� ����
        public JoustGloryOrb JoustGloryOrb = new();         // ������� ������ ����
        public Achievement Achievement = new();             // ����
        public RankProfile RankProfile = new();             // ��ŷ ������
        public ManaHeart ManaHeart = new();                 // ������Ʈ
        public Preset Preset = new();                       // ������

        // ������
        //public EventQuest EventQuest = new();               // �̺�Ʈ ����Ʈ
        //public EventPass EventPass = new();                 // �̺�Ʈ �н�
        //public EventShop EventShop = new();                 // �̺�Ʈ ����


        // AccountInfos�� ���ԵǾ� ���� ����
        // �� ���� �� ��
        //public Rank Rank = new();                         // ��ŷ
        public Leaderboard Leaderboard = new();             // �������� ( new ��ŷ )
        public Post Post = new();                           // ������
        public Coupon Coupon = new();                       // ����
        public Notice Notice = new();                       // ��������
        public ObscuredString CountryCode = string.Empty;   // �α����ϸ鼭 �����ڵ� ��ȸ�� ĳ��
        public ObscuredString GroupUuid = string.Empty;     // �α����ϸ鼭 �׷����� ��ȸ�� ĳ��
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

            // ��ȭ ó��
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

            // ����ġ
            int levelUpCount = ExpLevel.StackExp(reward.exp);
            if (levelUpCount > 0)
            {
                Lance.GameManager.OnPlayerLevelUp(levelUpCount);
            }

            // Ƽ�� ����
            if (reward.ticketBundle > 0)
            {
                for (int i = 0; i < (int)DungeonType.Count; ++i)
                {
                    DungeonType type = (DungeonType)i;

                    Currency.AddDungeonTicket(type, reward.ticketBundle);
                }
            }

            // Ƽ��
            if (reward.tickets != null)
            {
                for (int i = 0; i < reward.tickets.Length; ++i)
                {
                    DungeonType type = (DungeonType)i;

                    Currency.AddDungeonTicket(type, reward.tickets[i]);
                }
            }

            // ���輮
            if (reward.demonicRealmStones != null)
            {
                for (int i = 0; i < reward.demonicRealmStones.Length; ++i)
                {
                    DemonicRealmType type = (DemonicRealmType)i;

                    Currency.AddDemonicRealmStone(type, reward.demonicRealmStones[i]);
                }
            }

            // ����
            if (reward.essences != null)
            {
                for (int i = 0; i < reward.essences.Length; ++i)
                {
                    EssenceType type = (EssenceType)i;

                    Currency.AddEssence(type, reward.essences[i]);
                }
            }

            // ���
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

            // �Ǽ��縮
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

            // ��ų
            if (reward.skills != null)
            {
                for (int i = 0; i < reward.skills.Length; ++i)
                {
                    var skill = reward.skills[i];

                    Lance.Account.AddSkill(skill.Id, skill.Count);
                }
            }

            // ����
            if (reward.artifacts != null)
            {
                for (int i = 0; i < reward.artifacts.Length; ++i)
                {
                    var artifact = reward.artifacts[i];

                    Lance.Account.AddArtifact(artifact.Id, artifact.Count);
                }
            }

            // �ڽ�Ƭ
            if (reward.costumes != null)
            {
                for (int i = 0; i < reward.costumes.Length; ++i)
                {
                    var costume = reward.costumes[i];

                    Lance.Account.Costume.AddCostume(costume.Id);
                }
            }

            // ����
            if (reward.achievements != null)
            {
                for (int i = 0; i < reward.achievements.Length; ++i)
                {
                    var achievement = reward.achievements[i];

                    Lance.Account.Achievement.CompleteAchievement(achievement.Id);
                }
            }

            // �Ź�
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