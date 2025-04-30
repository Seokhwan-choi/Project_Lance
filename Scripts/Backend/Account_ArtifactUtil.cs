using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Lance
{
    public partial class Account
    {
        public bool IsMaxLevelArtifact(string id)
        {
            if (DataUtil.IsAncientArtifact(id))
            {
                return AncientArtifact.IsMaxLevel(id);
            }
            else
            {
                return Artifact.IsMaxLevel(id);
            }
        }

        public bool CanLevelUpArtifact(string id)
        {
            if (DataUtil.IsAncientArtifact(id))
            {
                return AncientArtifact.CanLevelUp(id);
            }
            else
            {
                return Artifact.CanLevelUp(id);
            }
        }

        public bool CanDismantleArtifact(string id)
        {
            if (DataUtil.IsAncientArtifact(id))
            {
                return AncientArtifact.CanDismantle(id);
            }
            else
            {
                return Artifact.CanDismantle(id);
            }
        }

        public bool CanSellArtifact(string id)
        {
            if (DataUtil.IsAncientArtifact(id))
            {
                return false;
            }
            else
            {
                return Artifact.CanSell(id);
            }
        }

        public int GetCountArtifact(string id)
        {
            if (DataUtil.IsAncientArtifact(id))
            {
                return AncientArtifact.GetCount(id);
            }
            else
            {
                return Artifact.GetCount(id);
            }
        }

        public int GetUpgradeFailedCountArtifact(string id)
        {
            if (DataUtil.IsAncientArtifact(id))
            {
                return AncientArtifact.GetFailedCount(id);
            }
            else
            {
                return Artifact.GetFailedCount(id);
            }
        }

        public bool IsEnoughArtifactCount(string id, int count)
        {
            if (DataUtil.IsAncientArtifact(id))
            {
                return AncientArtifact.IsEnoughCount(id, count);
            }
            else
            {
                return Artifact.IsEnoughCount(id, count);
            }
        }

        public int GetLevelArtifact(string id)
        {
            if (DataUtil.IsAncientArtifact(id))
            {
                return AncientArtifact.GetLevel(id);
            }
            else
            {
                return Artifact.GetLevel(id);
            }
        }

        public bool CanUpgradeExcalibur()
        {
            if (Lance.Account.Excalibur.IsMaxStepExcalibur())
                return false;

            if (Excalibur.IsAllMaxLevelExcaliburForces() == false)
                return false;

            return IsSatisfiedExcaliburUltimateLimitBreak();
        }

        public bool IsSatisfiedExcaliburUltimateLimitBreak()
        {
            var stepData = Lance.GameData.ExcaliburStepData.TryGet(Excalibur.GetStep());
            if (stepData != null)
                return stepData.requireUltimateLimitBreakStep <= ExpLevel.GetUltimateLimitBreak();
            else
                return false;
        }

        public bool UpgradeExcalibur()
        {
            if (CanUpgradeExcalibur() == false)
                return false;

            return Excalibur.UpgradeExcalibur();
        }

        public bool AnyCanUpgradeExcaliburForce()
        {
            for(int i = 0; i < (int)ExcaliburForceType.Count; ++i)
            {
                ExcaliburForceType type = (ExcaliburForceType)i;

                if (CanUpgradeExcaliburForce(type))
                    return true;
            }

            return false;
        }

        public bool CanUpgradeExcaliburForce(ExcaliburForceType type)
        {
            if (Excalibur.IsMaxLevelExcaliburForce(type))
                return false;

            int require = Excalibur.GetUpgradeRequire(type);

            return IsEnoughAncientEssence(require);
        }

        public bool UpgradeExcaliburForce(ExcaliburForceType type)
        {
            if (Excalibur.IsMaxLevelExcaliburForce(type))
                return false;

            int require = Excalibur.GetUpgradeRequire(type);

            if (IsEnoughAncientEssence(require) == false)
                return false;

            if (UseAncientEssence(require))
            {
                if (Excalibur.UpgradeExcaliburForce(type))
                {
                    return true;
                }
            }

            return false;
        }

        public bool LevelUpArtifact(string id)
        {
            if (DataUtil.IsAncientArtifact(id))
            {
                return AncientArtifact.LevelUp(id);
            }
            else
            {
                return Artifact.LevelUp(id);
            }
        }
        public void AddArtifact(string id, int count)
        {
            if (DataUtil.IsAncientArtifact(id))
                AncientArtifact.AddArtifact(id, count);
            else
                Artifact.AddArtifact(id, count);
        }

#if UNITY_EDITOR
        public void AddArtifact(string id, int count, int level)
        {
            if (DataUtil.IsAncientArtifact(id))
                AncientArtifact.AddArtifact(id, level, count);
            else
                Artifact.AddArtifact(id, level, count);
        }
#endif
    }
}