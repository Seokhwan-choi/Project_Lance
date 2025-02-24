using System.Collections.Generic;
using System;

namespace Lance
{
    enum ActionType
    {
        Attack,
        Idle,
        Move,
        Hit,
        Skill,
        Rage,
        Knockback,

        Count
    }

    class ActionManager
    {
        Character mParent;
        List<ActionInst> mActionList;
        ActionInst mCurrAction;
        ActionDecider mActionDecider;
        public ActionInst CurrAction => mCurrAction;
        public bool IsIdle => mCurrAction is Action_Idle;
        public void Init(Character parent)
        {
            mParent = parent;
            mActionList = new List<ActionInst>();

            mActionDecider = ActionDeciderCreator.Create(parent);
            mActionDecider.Init(parent);
        }

        public void RandomizeKey()
        {
            mActionDecider.RandomizeKey();
        }

        public void OnUpdate(float dt)
        {
            if (mCurrAction == null)
            {
                mCurrAction = PopAction();
                mCurrAction?.OnStart(mParent);
            }
            else
            {
                if (mCurrAction.IsFinish)
                {
                    mCurrAction.OnFinish();
                    mCurrAction = null;
                    mCurrAction = PopAction();
                    mCurrAction?.OnStart(mParent);
                }
            }

            mCurrAction?.OnUpdate(dt);
            mActionDecider.OnUpdate(dt);
        }

        public void OnRelease()
        {
            mActionList.Clear();
            mActionList = null;
        }

        public void ReserveSkillCast(SkillData skillData)
        {
            Action_SkillCast skillAction = new Action_SkillCast(skillData);

            if (mActionList.Count <= 0)
            {
                mActionList.Add(skillAction);
            }
            else
            {
                // 이미 스킬이 예약되어 있는지 확인한다.
                // 스킬이 예약되어 있다면 다음에 예약하자..
                if (AnySkillCastActionReserved() == false)
                {
                    // 무조건 제일 앞에 예약한다.
                    mActionList.Insert(0, skillAction);
                }
            }
        }

        bool AnySkillCastActionReserved()
        {
            if (mActionList.Count > 0)
            {
                foreach(var action in mActionList)
                {
                    if (action is Action_SkillCast)
                        return true;
                }
            }

            return false;
        }

        public void PlayRageAction(string rageActionId)
        {
            var rageActionData = Lance.GameData.RageActionData.TryGet(rageActionId);
            if (rageActionData == null)
                return;

            mActionList.Add(new Action_Rage(rageActionData));
        }

        public void PlayAction(ActionType type, bool forced = false)
        {
            ActionInst action = CreateAction(type);

            if (action != null)
            {
                if (forced)
                {
                    mCurrAction?.OnFinish();
                    mCurrAction = null;

                    mCurrAction = action;
                    mCurrAction.OnStart(mParent);
                }
                else
                {
                    mActionList.Add(action);
                }
            }
        }

        public ActionInst CreateAction(ActionType type)
        {
            switch (type)
            {
                case ActionType.Attack:
                    return new Action_Attack();
                case ActionType.Move:
                    return new Action_Move();
                case ActionType.Knockback:
                    return new Action_Knockback();
                default:
                    return new Action_Idle();
            }
        }

        public void AllCancelReservedAction()
        {
            mActionList.Clear();
        }

        ActionInst PopAction()
        {
            if (mParent.IsDeath)
                return null;

            if (mActionList.Count > 0)
            {
                var firstAction = mActionList[0];
                mActionList.RemoveAt(0);

                return firstAction;
            }
            else
            {
                // 모든 액션을 마무리 했으면 지금 기준으로
                // 새로운 액션을 만들어주자
                return mActionDecider.Decide();
            }
        }
    }
}