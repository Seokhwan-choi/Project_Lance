using System.Text;
using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using CodeStage.AntiCheat.ObscuredTypes;
using CodeStage.AntiCheat.Storage;

namespace Lance
{
	public class LocalSave
	{
		public int LastLoginDateNum;
		public bool SkillEffectInit;
		public ulong BitFlags;                  // 비트 플래그
		public LangCode LangCode;

		public List<string> GetSkills;
		public List<string> GetEquipments;
		public List<string> GetAccessorys;
		public List<string> GetPetEquipments;
		public List<string> GetCostumes;

		public void AddGetSkill(string id)
        {
			if (IsNewSkill(id) == false)
				return;

			GetSkills.Add(id);

			this.Save();
        }

		public bool IsNewSkill(string id)
        {
			return GetSkills.Contains(id) == false;
        }

		public void AddGetEquipment(string id)
		{
			if (IsNewEquipment(id) == false)
				return;

			GetEquipments.Add(id);

			this.Save();
		}

		public void AddGetAccessory(string id)
        {
			if (IsNewAccessory(id) == false)
				return;

			GetAccessorys.Add(id);

			this.Save();
        }

		public bool AnyIsNewEquipment(IEnumerable<string> ids)
        {
			foreach(var id in ids)
            {
				if (IsNewEquipment(id))
					return true;
            }

			return false;
        }

		public bool IsNewEquipment(string id)
		{
			return GetEquipments.Contains(id) == false;
		}

		public bool IsNewAccessory(string id)
        {
			return GetAccessorys.Contains(id) == false;
        }

		public void AddGetCostume(string id)
		{
			if (IsNewCostume(id) == false)
				return;

			GetCostumes.Add(id);

			this.Save();
		}

		public bool IsNewCostume(string id)
		{
			return GetCostumes.Contains(id) == false;
		}

		public void AddGetPetEquipment(string id)
		{
			if (IsNewPetEquipment(id) == false)
				return;

			GetPetEquipments.Add(id);

			this.Save();
		}

		public bool IsNewPetEquipment(string id)
		{
			return GetPetEquipments.Contains(id) == false;
		}

		public void Normalize()
        {
			if (GetSkills == null)
				GetSkills = new List<string>();

			if (GetEquipments == null)
				GetEquipments = new List<string>();

			if (GetAccessorys == null)
				GetAccessorys = new List<string>();

			if (GetCostumes == null)
            {
				GetCostumes = new List<string>();
				GetCostumes.Add("BodyCostume_00");
				GetCostumes.Add("WeaponCostume_00");
				GetCostumes.Add("EtcCostume_00");
			}

			if (GetPetEquipments == null)
				GetPetEquipments = new List<string>();

			if (LastLoginDateNum == 0)
            {
				SaveBitFlags.BGMSound.Set(true);
				SaveBitFlags.SFXSound.Set(true);
				SaveBitFlags.Effect.Set(true);
				SaveBitFlags.CameraEffect.Set(true);
				SaveBitFlags.AutoSleepMode.Set(true);
				SaveBitFlags.Push.Set(true);
				SaveBitFlags.SkillEffect.Set(true);
			}

			if (SkillEffectInit == false)
            {
				SkillEffectInit = true;

				SaveBitFlags.SkillEffect.Set(true);
			}
        }
	}

	static class LocalSaveUtil
	{
		const string DataPrefKey = "3094kdfjskd";            // 아무 의미없는 값
		public static LocalSave Load()
		{
			try
			{
				string base64 = ObscuredPrefs.GetString(DataPrefKey, null);
				if (base64 != string.Empty)
				{
					LocalSave data = JsonUtil.Base64FromJsonUnity<LocalSave>(base64);
					if (data != null)
					{
						return data;
					}
				}
			}
			catch
			{
			}

			return MakeNewLocalSave();
		}

		static LocalSave MakeNewLocalSave()
		{
			LocalSave newSavePoint = new LocalSave();

			return newSavePoint;
		}

		public static void Save(this LocalSave save)
		{
			string base64 = save.ChangeToSavedStr();

			ObscuredPrefs.SetString(DataPrefKey, base64);
		}

		public static string ChangeToSavedStr(this LocalSave data)
		{
			string json = JsonUtil.ToJsonUnity(data);

			return JsonUtil.JsonToBase64String(json);
		}
	}

	public enum SaveBitFlags
	{
		BGMSound,				// o
		SFXSound,				// o
		Effect,					//
		CameraEffect,			// o
		AutoSleepMode,			//
		Push,					//
		SkillAutoCast,			// o
		BossAutoChallenge,		// o
		DungeonAutoChallenge,   // o
		ServiceTermsofUse,		// o
		SkillEffect,
		ServiceTermsofUseAgree, // o
		PetSkillAutoCastOff,	// o
		JoustingAutoBattle,     
		SkipSpawnMotion,
		BossBreakThrough,		// o
		DemonicRealmAutoChallenge,
		GoldTrainMaxLevelHide,		
		AutoResetStatictics,

		Count,
	}

	static class LocalSaveBitFlagsExt
	{
		public static void Set(this SaveBitFlags flag, bool value = true)
		{
			ulong mask = 1UL << (int)flag;

			if (value)
				Lance.LocalSave.BitFlags |= mask;
			else
				Lance.LocalSave.BitFlags &= ~mask;

			Lance.LocalSave.Save();
		}

		public static bool Get(this SaveBitFlags flag)
		{
			ulong mask = 1UL << (int)flag;

			return (Lance.LocalSave.BitFlags & mask) != 0;
		}

		public static void Toggle(this SaveBitFlags flag)
		{
			flag.Set(!flag.Get());
		}

		public static bool IsOn(this SaveBitFlags flag)
		{
			return flag.Get();
		}

		public static bool IsOff(this SaveBitFlags flag)
		{
			return flag.Get() == false;
		}
	}
}