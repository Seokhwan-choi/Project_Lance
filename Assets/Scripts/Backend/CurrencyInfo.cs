using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CodeStage.AntiCheat.ObscuredTypes;
using BackEnd;
using LitJson;
using System.Linq;

namespace Lance
{
    public class CurrencyInfo : AccountBase
    {
        ObscuredDouble mGold;               // 골드
        ObscuredDouble mGem;                // 잼
        ObscuredDouble mUpgradeStones;      // 장비 강화석
        ObscuredDouble mSkillPiece;         // 스킬 조각
        ObscuredDouble mMileage;            // 마일리지
        ObscuredInt[] mDungeonTickets;      // 던전 입장권
        ObscuredInt[] mDemonicRealmStones;  // 마계 입장석
        ObscuredInt[] mEssences;            // 정수 재화
        ObscuredInt mElementalStone;        // 속성석
        ObscuredInt mPetFood;               // 신수 먹이 ( 경험치 )
        ObscuredDouble mReforgeStone;       // 재련석
        ObscuredDouble mAncientEssence;     // 고대의 정수
        ObscuredInt mJoustingTicket;        // 마상시합 입장권
        ObscuredInt mJoustingCoin;          // 마상시합 코인 ( 상점 전용 재화 )
        ObscuredInt mGloryToken;            // 영광의 증표 ( 영광의 보주 강화 재료 )
        ObscuredInt mSoulStone;             // 영혼석 ( 마계 몬스터 처치 드랍 )
        ObscuredInt mManaEssence;           // 마나의 정수 ( 마나 하트 강화 재료 )
        ObscuredDouble mCostumeUpgrade;     // 실타래 ( 코스튬 강화 재화 )
        DailyCounter mDailyTicket;          // 골드, 강화석, 레이드 던전 입장권
        DailyCounter mDailyPetTicket;       // 신수 던전 입장권
        DailyCounter mDailyReforgeTicket;   // 재련 던전 입장권
        DailyCounter mDailyGrowthTicket;    // 성장의 던전 입장권
        DailyCounter mDailyAncientTicket;   // 잊혀진 왕국 입장권
        DailyCounter mDailyDemonicRealmStone;  // 마계 입장권

        DailyCounter mDailyJoustingTicket;          // 일일마상시합 입장권
        RankDailyCounter mRankDailyJoustingTicket;  // 일일마상시합 입장권 ( 오전 6시에 지급 )

        ObscuredBool mRetroactiveElementalStone;
        ObscuredBool mRetroactiveDailyJoustingTicket;    // 일일 마상시합 입장권 이미 받았냐?
        ObscuredBool mRetroactiveGloryToken;             // 마상 시합 토큰 소급
        public override string GetTableName()
        {
            return "Currency";
        }
        public override Param GetParam()
        {
            var param = new Param();
            param.Add("Gold", (double)mGold);
            param.Add("Gem", (double)mGem);
            param.Add("UpgradeStone", (double)mUpgradeStones);
            param.Add("SkillPiece", (double)mSkillPiece);
            param.Add("Mileage", (double)mMileage);
            param.Add("Essences", mEssences.Select(x => (int)x).ToArray());
            param.Add("DungeonTickets", mDungeonTickets.Select(x => (int)x).ToArray());
            param.Add("DemonicRealmStones", mDemonicRealmStones.Select(x => (int)x).ToArray());
            param.Add("ElementalStone", (int)mElementalStone);
            param.Add("PetFood", (int)mPetFood);
            param.Add("ReforgeStone", (double)mReforgeStone);
            param.Add("RetroactiveElementalStone", (bool)mRetroactiveElementalStone);
            param.Add("RetroactiveDailyJoustingTicket", (bool)mRetroactiveDailyJoustingTicket);
            param.Add("RetroactiveGloryToken",(bool)mRetroactiveGloryToken);
            param.Add("AncientEssence", (double)mAncientEssence);
            param.Add("JoustingTicket", (int)mJoustingTicket);
            param.Add("JoustingCoin", (int)mJoustingCoin);
            param.Add("GloryToken", (int)mGloryToken);
            param.Add("SoulStone", (int)mSoulStone);
            param.Add("ManaEssence", (int)mManaEssence);
            param.Add("CostumeUpgrade", (double)mCostumeUpgrade);

            mDailyTicket.ReadyToSave();
            param.Add("DailyTicket", mDailyTicket);

            mDailyPetTicket.ReadyToSave();
            param.Add("PetDailyTicket", mDailyPetTicket);

            mDailyReforgeTicket.ReadyToSave();
            param.Add("ReforgeDailyTicket", mDailyReforgeTicket);

            mDailyGrowthTicket.ReadyToSave();
            param.Add("GrowthDailyTicket", mDailyGrowthTicket);

            mDailyAncientTicket.ReadyToSave();
            param.Add("AncientDailyTicket", mDailyAncientTicket);

            mDailyJoustingTicket.ReadyToSave();
            param.Add("JoustingDailyTicket", mDailyJoustingTicket);

            mRankDailyJoustingTicket.ReadyToSave();
            param.Add("JoustingRankDailyTicket", mRankDailyJoustingTicket);

            mDailyDemonicRealmStone.ReadyToSave();
            param.Add("DemonicRealmDailyStone", mDailyDemonicRealmStone);

            return param;
        }

        public override bool CanUpdateRankScore()
        {
            return mJoustingTicket >= 0;
        }

        protected override void InitializeData()
        {
            mDungeonTickets = Enumerable.Repeat<ObscuredInt>(0, (int)DungeonType.Count).ToArray();
            mDemonicRealmStones = Enumerable.Repeat<ObscuredInt>(0, (int)DemonicRealmType.Count).ToArray();
            mEssences = Enumerable.Repeat<ObscuredInt>(0, (int)EssenceType.Count).ToArray();
            mDailyTicket = new DailyCounter();
            mDailyPetTicket = new DailyCounter();
            mDailyReforgeTicket = new DailyCounter();
            mDailyGrowthTicket = new DailyCounter();
            mDailyAncientTicket = new DailyCounter();
            mDailyJoustingTicket = new DailyCounter();
            mRankDailyJoustingTicket = new RankDailyCounter();
            mDailyDemonicRealmStone = new DailyCounter();

            mRetroactiveElementalStone = true;  // 새로 생성된 계정은 보정 받을 필요가 없다.
            mRetroactiveDailyJoustingTicket = true;
            mRetroactiveGloryToken = true;

            UpdateFreeTicket();
        }

        protected override void SetServerDataToLocal(JsonData gameDataJson)
        {
            double goldTemp = 0;
            double.TryParse(gameDataJson["Gold"].ToString(), out goldTemp);
            mGold = goldTemp;

            double gemTemp = 0;
            double.TryParse(gameDataJson["Gem"].ToString(), out gemTemp);
            mGem = gemTemp;

            double upgradeStoneTemp = 0;
            double.TryParse(gameDataJson["UpgradeStone"].ToString(), out upgradeStoneTemp);
            mUpgradeStones = upgradeStoneTemp;

            if (gameDataJson.ContainsKey("SkillPiece"))
            {
                double skillPieceTemp = 0;
                double.TryParse(gameDataJson["SkillPiece"].ToString(), out skillPieceTemp);
                mSkillPiece = skillPieceTemp;
            }

            if (gameDataJson.ContainsKey("Mileage"))
            {
                double mileageTemp = 0;
                double.TryParse(gameDataJson["Mileage"].ToString(), out mileageTemp);
                mMileage = mileageTemp;
            }

            if (gameDataJson.ContainsKey("PetFood"))
            {
                int petFoodTemp = 0;
                int.TryParse(gameDataJson["PetFood"].ToString(), out petFoodTemp);
                mPetFood = petFoodTemp;
            }

            if (gameDataJson.ContainsKey("ElementalStone"))
            {
                int elementalStoneTemp = 0;
                int.TryParse(gameDataJson["ElementalStone"].ToString(), out elementalStoneTemp);
                mElementalStone = elementalStoneTemp;
            }

            mEssences = Enumerable.Repeat<ObscuredInt>(0, (int)EssenceType.Count).ToArray();

            if (gameDataJson.ContainsKey("Essences"))
            {
                for (int i = 0; i < gameDataJson["Essences"].Count; ++i)
                {
                    if (mEssences.Length <= i)
                        break;

                    int essenceTemp = 0;

                    int.TryParse(gameDataJson["Essences"][i].ToString(), out essenceTemp);

                    mEssences[i] = essenceTemp;
                }
            }

            mDungeonTickets = Enumerable.Repeat<ObscuredInt>(0, (int)DungeonType.Count).ToArray();

            for(int i = 0; i < gameDataJson["DungeonTickets"].Count; ++i)
            {
                if (mDungeonTickets.Length <= i)
                    break;

                int ticketTemp = 0;

                int.TryParse(gameDataJson["DungeonTickets"][i].ToString(), out ticketTemp);

                mDungeonTickets[i] = ticketTemp;
            }

            mDemonicRealmStones = Enumerable.Repeat<ObscuredInt>(0, (int)DemonicRealmType.Count).ToArray();
            if (gameDataJson.ContainsKey("DemonicRealmStones"))
            {
                for (int i = 0; i < gameDataJson["DemonicRealmStones"].Count; ++i)
                {
                    if (mDemonicRealmStones.Length <= i)
                        break;

                    int stoneTemp = 0;

                    int.TryParse(gameDataJson["DemonicRealmStones"][i].ToString(), out stoneTemp);

                    mDemonicRealmStones[i] = stoneTemp;
                }
            }

            mDailyTicket = new();
            if (gameDataJson.ContainsKey("DailyTicket"))
            {
                var dailyTicket = new DailyCounter();

                dailyTicket.SetServerDataToLocal(gameDataJson["DailyTicket"]);

                mDailyTicket = dailyTicket;
            }

            mDailyPetTicket = new();
            if (gameDataJson.ContainsKey("PetDailyTicket"))
            {
                var petDailyTicket = new DailyCounter();

                petDailyTicket.SetServerDataToLocal(gameDataJson["PetDailyTicket"]);

                mDailyPetTicket = petDailyTicket;
            }

            if (gameDataJson.ContainsKey("RetroactiveElementalStone"))
            {
                bool retroactiveElementalStoneTemp = false;

                bool.TryParse(gameDataJson["RetroactiveElementalStone"].ToString(), out retroactiveElementalStoneTemp);

                mRetroactiveElementalStone = retroactiveElementalStoneTemp;
            }

            mDailyReforgeTicket = new();
            if (gameDataJson.ContainsKey("ReforgeDailyTicket"))
            {
                var reforgeDailyTicket = new DailyCounter();

                reforgeDailyTicket.SetServerDataToLocal(gameDataJson["ReforgeDailyTicket"]);

                mDailyReforgeTicket = reforgeDailyTicket;
            }

            if (gameDataJson.ContainsKey("ReforgeStone"))
            {
                double reforgeStoneTemp = 0;
                double.TryParse(gameDataJson["ReforgeStone"].ToString(), out reforgeStoneTemp);
                mReforgeStone = reforgeStoneTemp;
            }

            mDailyGrowthTicket = new();
            if (gameDataJson.ContainsKey("GrowthDailyTicket"))
            {
                var growthDailyTicket = new DailyCounter();

                growthDailyTicket.SetServerDataToLocal(gameDataJson["GrowthDailyTicket"]);

                mDailyGrowthTicket = growthDailyTicket;
            }

            if (gameDataJson.ContainsKey("AncientEssence"))
            {
                double ancientEssenceTemp = 0;

                double.TryParse(gameDataJson["AncientEssence"].ToString(), out ancientEssenceTemp);

                mAncientEssence = ancientEssenceTemp;
            }

            mDailyAncientTicket = new();
            if (gameDataJson.ContainsKey("AncientDailyTicket"))
            {
                var ancientDailyTicket = new DailyCounter();

                ancientDailyTicket.SetServerDataToLocal(gameDataJson["AncientDailyTicket"]);

                mDailyAncientTicket = ancientDailyTicket;
            }

            if (gameDataJson.ContainsKey("JoustingTicket"))
            {
                int joustingTicketTemp = 0;
                int.TryParse(gameDataJson["JoustingTicket"].ToString(), out joustingTicketTemp);
                mJoustingTicket = joustingTicketTemp;
            }

            // 업데이트이 후 확인 한번하고 다시 안씀
            mDailyJoustingTicket = new();
            if (gameDataJson.ContainsKey("JoustingDailyTicket"))
            {
                var joustingDailyTicket = new DailyCounter();

                joustingDailyTicket.SetServerDataToLocal(gameDataJson["JoustingDailyTicket"]);

                mDailyJoustingTicket = joustingDailyTicket;
            }

            mRetroactiveDailyJoustingTicket = false;

            if (gameDataJson.ContainsKey("RetroactiveDailyJoustingTicket"))
            {
                bool retroactiveDailyJoustingTicketTemp = false;

                bool.TryParse(gameDataJson["RetroactiveDailyJoustingTicket"].ToString(), out retroactiveDailyJoustingTicketTemp);

                mRetroactiveDailyJoustingTicket = retroactiveDailyJoustingTicketTemp;
            }

            mRankDailyJoustingTicket = new();
            if (gameDataJson.ContainsKey("JoustingRankDailyTicket"))
            {
                var joustingRankDailyTicket = new RankDailyCounter();

                joustingRankDailyTicket.SetServerDataToLocal(gameDataJson["JoustingRankDailyTicket"]);

                mRankDailyJoustingTicket = joustingRankDailyTicket;
            }

            if (gameDataJson.ContainsKey("JoustingCoin"))
            {
                int joustingCoinTemp = 0;

                int.TryParse(gameDataJson["JoustingCoin"].ToString(), out joustingCoinTemp);

                mJoustingCoin = joustingCoinTemp;
            }

            if (gameDataJson.ContainsKey("GloryToken"))
            {
                int gloryTokenTemp = 0;

                int.TryParse(gameDataJson["GloryToken"].ToString(), out gloryTokenTemp);

                mGloryToken = gloryTokenTemp;
            }

            mRetroactiveGloryToken = false;

            if (gameDataJson.ContainsKey("RetroactiveGloryToken"))
            {
                bool retroactiveGloryTokenTemp = false;

                bool.TryParse(gameDataJson["RetroactiveGloryToken"].ToString(), out retroactiveGloryTokenTemp);

                mRetroactiveGloryToken = retroactiveGloryTokenTemp;
            }

            mDailyDemonicRealmStone = new();

            if (gameDataJson.ContainsKey("DemonicRealmDailyStone"))
            {
                var demonicRealmDailyStone = new DailyCounter();

                demonicRealmDailyStone.SetServerDataToLocal(gameDataJson["DemonicRealmDailyStone"]);

                mDailyDemonicRealmStone = demonicRealmDailyStone;
            }

            if (gameDataJson.ContainsKey("SoulStone"))
            {
                int soulStoneTemp = 0;

                int.TryParse(gameDataJson["SoulStone"].ToString(), out soulStoneTemp);

                mSoulStone = soulStoneTemp;
            }

            if (gameDataJson.ContainsKey("CostumeUpgrade"))
            {
                double costumeUpgradeTemp = 0;

                double.TryParse(gameDataJson["CostumeUpgrade"].ToString(), out costumeUpgradeTemp);

                mCostumeUpgrade = costumeUpgradeTemp;
            }

            if (gameDataJson.ContainsKey("ManaEssence"))
            {
                int manaEssenceTemp = 0;

                int.TryParse(gameDataJson["ManaEssence"].ToString(), out manaEssenceTemp);

                mManaEssence = manaEssenceTemp;
            }

            UpdateFreeTicket();
        }

        public bool GetRetroactiveElementalStone()
        {
            return mRetroactiveElementalStone;
        }

        public bool RetroactiveElementalStone()
        {
            if (mRetroactiveElementalStone)
                return false;

            mRetroactiveElementalStone = true;

            mElementalStone = Mathf.RoundToInt(mElementalStone * Lance.GameData.CommonData.retroactiveElementalStoneValue);

            SetIsChangedData(true);

            return true;
        }

        public bool GetRetroactiveGloryToken()
        {
            return mRetroactiveGloryToken;
        }

        public bool RetroactiveGloryToken()
        {
            if (mRetroactiveGloryToken)
                return false;

            mRetroactiveGloryToken = true;

            SetIsChangedData(true);

            return true;
        }

        public override void RandomizeKey()
        {
            base.RandomizeKey();

            mGold.RandomizeCryptoKey();
            mGem.RandomizeCryptoKey();
            mUpgradeStones.RandomizeCryptoKey();
            mSkillPiece.RandomizeCryptoKey();
            mMileage.RandomizeCryptoKey();
            mPetFood.RandomizeCryptoKey();
            mReforgeStone.RandomizeCryptoKey();
            mElementalStone.RandomizeCryptoKey();
            mJoustingTicket.RandomizeCryptoKey();
            mSoulStone.RandomizeCryptoKey();
            mCostumeUpgrade.RandomizeCryptoKey();
            mManaEssence.RandomizeCryptoKey();

            mRetroactiveElementalStone.RandomizeCryptoKey();
            mRetroactiveDailyJoustingTicket.RandomizeCryptoKey();
            mRetroactiveGloryToken.RandomizeCryptoKey();

            foreach (var ticket in mDungeonTickets)
            {
                ticket.RandomizeCryptoKey();
            }

            foreach (var demonicRealmStone in mDemonicRealmStones)
            {
                demonicRealmStone.RandomizeCryptoKey();
            }

            mDailyTicket.RandomizeKey();
            mDailyPetTicket.RandomizeKey();
            mDailyReforgeTicket.RandomizeKey();   
            mDailyGrowthTicket.RandomizeKey();    
            mDailyAncientTicket.RandomizeKey();   
            mDailyJoustingTicket.RandomizeKey();
            mRankDailyJoustingTicket.RandomizeKey();
            mDailyDemonicRealmStone.RandomizeKey();
        }

        public double GetGold()
        {
            return mGold;
        }

        public bool UseGold(double gold)
        {
            if (IsEnoughGold(gold) == false)
                return false;

            mGold -= gold;

            SetIsChangedData(true);

            return true;
        }

        public void AddGold(double gold)
        {
            if (gold <= 0)
                return;

            mGold += gold;

            SetIsChangedData(true);
        }

        public bool IsEnoughGold(double gold)
        {
            return mGold >= gold;
        }

        public double GetGem()
        {
            return mGem;
        }

        public bool UseGem(double gem)
        {
            if (IsEnoughGem(gem) == false)
                return false;

            mGem -= gem;

            SetIsChangedData(true);

            return true;
        }

        public void AddGem(double gem)
        {
            if (gem <= 0)
                return;

            mGem += gem;

            SetIsChangedData(true);
        }

        public bool IsEnoughGem(double gem)
        {
            return mGem >= gem;
        }

        public double GetSkillPiece()
        {
            return mSkillPiece;
        }

        public bool UseSkillPiece(double skillPiece)
        {
            if (IsEnoughSkillPiece(skillPiece) == false)
                return false;

            mSkillPiece -= skillPiece;

            SetIsChangedData(true);

            return true;
        }

        public void AddSkillPiece(double skillPiece)
        {
            if (skillPiece <= 0)
                return;

            mSkillPiece += skillPiece;

            SetIsChangedData(true);
        }

        public bool IsEnoughSkillPiece(double skillPiece)
        {
            return mSkillPiece >= skillPiece;
        }

        public double GetUpgradeStones()
        {
            return mUpgradeStones;
        }

        public bool UseUpgradeStones(double stones)
        {
            if (IsEnoughUpgradeStones(stones) == false)
                return false;

            mUpgradeStones -= stones;

            SetIsChangedData(true);

            return true;
        }

        public void AddUpgradeStones(double upgradeStones)
        {
            if (upgradeStones <= 0)
                return;

            mUpgradeStones += upgradeStones;

            SetIsChangedData(true);
        }

        public bool IsEnoughUpgradeStones(double upgradeStones)
        {
            return mUpgradeStones >= upgradeStones;
        }

        public double GetMileage()
        {
            return mMileage;
        }

        public bool UseMileage(double mileage)
        {
            if (IsEnoughMileage(mileage) == false)
                return false;

            mMileage -= mileage;

            SetIsChangedData(true);

            return true;
        }

        public void AddMileage(double mileage)
        {
            if (mileage <= 0)
                return;

            mMileage += mileage;

            SetIsChangedData(true);
        }

        public bool IsEnoughMileage(double mileage)
        {
            return mMileage >= mileage;
        }

        public int GetPetFood()
        {
            return mPetFood;
        }

        public bool UsePetFood(int petFood)
        {
            if (IsEnoughPetFood(petFood) == false)
                return false;

            mPetFood -= petFood;

            SetIsChangedData(true);

            return true;
        }

        public void AddPetFood(int petFood)
        {
            if (petFood <= 0)
                return;

            mPetFood += petFood;

            SetIsChangedData(true);
        }

        public bool IsEnoughPetFood(int petFood)
        {
            return mPetFood >= petFood;
        }

        public int GetDungeonTicket(StageType type)
        {
            int typeIndex = (int)type.ChangeToDungeonType();

            if (mDungeonTickets == null || mDungeonTickets.Length <= typeIndex)
                return 0;

            return mDungeonTickets[typeIndex];
        }

        public bool UseDungeonTicket(StageType type, int count)
        {
            if (IsEnoughDungeonTicket(type, count) == false)
                return false;

            int typeIndex = (int)type.ChangeToDungeonType();

            if (mDungeonTickets == null || mDungeonTickets.Length <= typeIndex)
                return false;

            mDungeonTickets[typeIndex] -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddDungeonTicket(DungeonType type, int count)
        {
            if (count <= 0)
                return;

            int typeIndex = (int)type;

            if (mDungeonTickets == null || mDungeonTickets.Length <= typeIndex)
                return;

            mDungeonTickets[typeIndex] += count;

            SetIsChangedData(true);
        }

        public void AddDungeonTicket(StageType type, int count)
        {
            if (count <= 0)
                return;

            int typeIndex = (int)type.ChangeToDungeonType();

            if (mDungeonTickets == null || mDungeonTickets.Length <= typeIndex)
                return;

            mDungeonTickets[typeIndex] += count;

            SetIsChangedData(true);
        }

        public bool IsEnoughDungeonTicket(StageType type, int count)
        {
            int typeIndex = (int)type.ChangeToDungeonType();

            if (mDungeonTickets == null || mDungeonTickets.Length <= typeIndex)
                return false;

            return mDungeonTickets[typeIndex] >= count;
        }

        public void UpdateFreeTicket()
        {
            if (mDailyTicket.StackCount(1))
            {
                AddDungeonTicket(DungeonType.Gold, 2);
                AddDungeonTicket(DungeonType.Stone, 2);
                AddDungeonTicket(DungeonType.Raid, 2);
            }

            if (mDailyPetTicket.StackCount(1))
            {
                AddDungeonTicket(DungeonType.Pet, 2);
            }

            if (mDailyReforgeTicket.StackCount(1))
            {
                AddDungeonTicket(DungeonType.Reforge, 2);
            }

            if (mDailyGrowthTicket.StackCount(1))
            {
                AddDungeonTicket(DungeonType.Growth, 2);
            }

            if (mDailyAncientTicket.StackCount(1))
            {
                AddDungeonTicket(DungeonType.Ancient, 2);
            }

            // 업데이트 이전에 이미 티켓을 수령한적이 있으면
            // 티켓을 수령하면안된다.

            // 티켓을 이미 수령했는지 확인하는 변수를 넣어두자
            // 계정을 처음 생성한 사람은 새로운 일일 티켓 기준으로 주면된다.
            if (mRetroactiveDailyJoustingTicket)
            {
                if (mRankDailyJoustingTicket.StackCount(1))
                {
                    AddJoustingTicket(Lance.GameData.JoustingCommonData.dailyFreeTicket);
                }
            }
            else
            {
                mRetroactiveDailyJoustingTicket = true;

                SetIsChangedData(true);

                // 업데이트 이전에 이미 티켓을 받은적이 있는지 확인한다.
                if (mDailyJoustingTicket.IsMaxCount(1))
                {
                    mRankDailyJoustingTicket.StackCount(1);
                }
                else
                {
                    // 티켓을 받은적이 없다면 티켓을 바로 지급해주자
                    if (mRankDailyJoustingTicket.StackCount(1))
                    {
                        AddJoustingTicket(Lance.GameData.JoustingCommonData.dailyFreeTicket);
                    }
                }
            }
        }

        public int GetElementalStone()
        {
            return mElementalStone;
        }

        public bool UseElementalStone(int count)
        {
            if (IsEnoughElementalStone(count) == false)
                return false;

            mElementalStone -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddElementalStone(int count)
        {
            if (count <= 0)
                return;

            mElementalStone += count;

            SetIsChangedData(true);
        }

        public void AddForcedElementalStone(int count)
        {
            mElementalStone += count;

            SetIsChangedData(true);
        }

        public bool IsEnoughElementalStone(int count)
        {
            return mElementalStone >= count;
        }

        public int GetEssence(EssenceType type)
        {
            int typeIndex = (int)type;

            if (mEssences == null || mEssences.Length <= typeIndex)
                return 0;

            return mEssences[typeIndex];
        }

        public bool UseEssence(EssenceType type, int count)
        {
            if (IsEnoughEssence(type, count) == false)
                return false;

            int typeIndex = (int)type; ;

            if (mEssences == null || mEssences.Length <= typeIndex)
                return false;

            mEssences[typeIndex] -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddEssence(EssenceType type, int count)
        {
            if (count <= 0)
                return;

            int typeIndex = (int)type;

            if (mEssences == null || mEssences.Length <= typeIndex)
                return;

            mEssences[typeIndex] += count;

            SetIsChangedData(true);
        }

        public bool IsEnoughEssence(EssenceType type, int count)
        {
            int typeIndex = (int)type;

            if (mEssences == null || mEssences.Length <= typeIndex)
                return false;

            return mEssences[typeIndex] >= count;
        }

        public double GetReforgeStone()
        {
            return mReforgeStone;
        }

        public bool UseReforgeStone(double stones)
        {
            if (IsEnoughReforgeStone(stones) == false)
                return false;

            mReforgeStone -= stones;

            SetIsChangedData(true);

            return true;
        }

        public void AddReforgeStone(double stones)
        {
            if (stones <= 0)
                return;

            mReforgeStone += stones;

            SetIsChangedData(true);
        }

        public bool IsEnoughReforgeStone(double stones)
        {
            return mReforgeStone >= stones;
        }

        public double GetAncientEssence()
        {
            return mAncientEssence;
        }

        public bool UseAncientEssence(int count)
        {
            if (IsEnoughAncientEssence(count) == false)
                return false;

            mAncientEssence -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddAncientEssence(int count)
        {
            if (count <= 0)
                return;

            mAncientEssence += count;

            SetIsChangedData(true);
        }

        public bool IsEnoughAncientEssence(int count)
        {
            return mAncientEssence >= count;
        }

        public int GetJoustingTicket()
        {
            return mJoustingTicket;
        }

        public bool UseJoustingTicket(int count)
        {
            if (IsEnoughJoustingTicket(count) == false)
                return false;

            mJoustingTicket -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddJoustingTicket(int count)
        {
            if (count <= 0)
                return;

            mJoustingTicket += count;

            SetIsChangedData(true);
        }

        public void SetJoustingTicket(int count)
        {
            if (count < 0)
                return;

            mJoustingTicket = count;

            SetIsChangedData(true);
        }

        public bool IsEnoughJoustingTicket(int count)
        {
            return mJoustingTicket >= count;
        }

        public int GetJoustingCoin()
        {
            return mJoustingCoin;
        }

        public bool UseJoustingCoin(int count)
        {
            if (IsEnoughJoustingCoin(count) == false)
                return false;

            mJoustingCoin -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddJoustingCoin(int count)
        {
            if (count <= 0)
                return;

            mJoustingCoin += count;

            SetIsChangedData(true);
        }

        public bool IsEnoughJoustingCoin(int count)
        {
            return mJoustingCoin >= count;
        }

        public int GetGloryToken()
        {
            return mGloryToken;
        }

        public bool UseGloryToken(int count)
        {
            if (IsEnoughGloryToken(count) == false)
                return false;

            mGloryToken -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddGloryToken(int count)
        {
            if (count <= 0)
                return;

            mGloryToken += count;

            SetIsChangedData(true);
        }

        public bool IsEnoughGloryToken(int count)
        {
            return mGloryToken >= count;
        }

        public int GetDemonicRealmStone(StageType type)
        {
            int typeIndex = (int)type.ChangeToDemonicRealmType();

            if (mDemonicRealmStones == null || mDemonicRealmStones.Length <= typeIndex)
                return 0;

            return mDemonicRealmStones[typeIndex];
        }

        public bool UseDemonicRealmStone(StageType type, int count)
        {
            if (IsEnoughDemonicRealmStone(type, count) == false)
                return false;

            int typeIndex = (int)type.ChangeToDemonicRealmType();

            if (mDemonicRealmStones == null || mDemonicRealmStones.Length <= typeIndex)
                return false;

            mDemonicRealmStones[typeIndex] -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddDemonicRealmStone(DemonicRealmType type, int count)
        {
            if (count <= 0)
                return;

            int typeIndex = (int)type;

            if (mDemonicRealmStones == null || mDemonicRealmStones.Length <= typeIndex)
                return;

            mDemonicRealmStones[typeIndex] += count;

            SetIsChangedData(true);
        }

        public void AddDemonicRealmStone(StageType type, int count)
        {
            if (count <= 0)
                return;

            int typeIndex = (int)type.ChangeToDemonicRealmType();

            if (mDemonicRealmStones == null || mDemonicRealmStones.Length <= typeIndex)
                return;

            mDemonicRealmStones[typeIndex] += count;

            SetIsChangedData(true);
        }

        public bool IsEnoughDemonicRealmStone(StageType type, int count)
        {
            int typeIndex = (int)type.ChangeToDemonicRealmType();

            if (mDemonicRealmStones == null || mDemonicRealmStones.Length <= typeIndex)
                return false;

            return mDemonicRealmStones[typeIndex] >= count;
        }

        public void UpdateFreeDemonicRealmStone()
        {
            if (mDailyDemonicRealmStone.StackCount(1))
            {
                AddDemonicRealmStone(DemonicRealmType.Accessory, 2);
            }
        }

        public int GetSoulStone()
        {
            return mSoulStone;
        }

        public bool UseSoulStone(int count)
        {
            if (IsEnoughSoulStone(count) == false)
                return false;

            mSoulStone -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddSoulStone(int count)
        {
            if (count <= 0)
                return;

            mSoulStone += count;

            SetIsChangedData(true);
        }

        public bool IsEnoughSoulStone(int count)
        {
            return mSoulStone >= count;
        }

        public double GetCostumeUpgrade()
        {
            return mCostumeUpgrade;
        }

        public bool UseCostumeUpgrade(double count)
        {
            if (IsEnoughCostumeUpgrade(count) == false)
                return false;

            mCostumeUpgrade -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddCostumeUpgrade(double count)
        {
            if (count <= 0)
                return;

            mCostumeUpgrade += count;

            SetIsChangedData(true);
        }

        public bool IsEnoughCostumeUpgrade(double count)
        {
            return mCostumeUpgrade >= count;
        }

        public int GetManaEssence()
        {
            return mManaEssence;
        }

        public bool UseManaEssence(int count)
        {
            if (IsEnoughManaEssence(count) == false)
                return false;

            mManaEssence -= count;

            SetIsChangedData(true);

            return true;
        }

        public void AddManaEssence(int count)
        {
            if (count <= 0)
                return;

            mManaEssence += count;

            SetIsChangedData(true);
        }

        public bool IsEnoughManaEssence(int count)
        {
            return mManaEssence >= count;
        }
    }
}