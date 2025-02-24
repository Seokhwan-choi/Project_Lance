using System.Linq;

namespace Lance
{
    public partial class Account
    {
        public bool LevelUpAbility(string id)
        {
            var data = Lance.GameData.AbilityData.TryGet(id);
            if (data == null)
                return false;

            if (CanLevelUpAbility(id) == false)
                return false;

            int level = Ability.GetAbilityLevel(id);
            int requierAP = AbilityUtil.CalcRequireAP(id, level);

            if (ExpLevel.UseAP(requierAP))
            {
                if (Ability.LevelUp(id))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanLevelUpAbility(string id)
        {
            var data = Lance.GameData.AbilityData.TryGet(id);
            if (data == null)
                return false;

            if (Ability.IsMeetRequirements(id) == false)
                return false;

            if (Ability.IsMaxLevel(id))
                return false;

            int level = Ability.GetAbilityLevel(id);
            int requireAp = AbilityUtil.CalcRequireAP(id, level);

            if (ExpLevel.IsEnoughAP(requireAp) == false)
                return false;

            return true;
        }

        public bool AnyAbilityCanLevelUp()
        {
            var datas = Ability.GetBestStepDatas();
            if (datas == null || datas.Count() <= 0)
                return false;

            foreach(var data in datas)
            {
                int level = Ability.GetAbilityLevel(data.id);
                int requireAp = AbilityUtil.CalcRequireAP(data.id, level);
                if (ExpLevel.IsEnoughAP(requireAp))
                    return true;
            }

            return false;
        }
    }
}