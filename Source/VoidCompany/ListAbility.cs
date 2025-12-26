using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace VoidCompany
{//綠菇主筆以下代碼
    // 1. 屬性定義 (XML 用)
    public class CompProperties_ListAbility : CompProperties
    {
        // 技能列表
        public List<AbilityDef> AbilityDefs = new List<AbilityDef>();

        public CompProperties_ListAbility()
        {
            this.compClass = typeof(CompListAbility);
        }
    }

    // 2. 邏輯實現
    public class CompListAbility : CompEquippable
    {
        // 快速存取屬性
        public CompProperties_ListAbility Props => (CompProperties_ListAbility)this.props;

        // 當裝備時 (武器/服裝)
        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);

            // 防呆檢查
            if (pawn.abilities == null || Props.AbilityDefs.NullOrEmpty()) return;

            foreach (AbilityDef def in Props.AbilityDefs)
            {
                // GainAbility 內部已經有防重複機制，直接呼叫即可
                pawn.abilities.GainAbility(def);
            }
        }

        // 當卸下時
        public override void Notify_Unequipped(Pawn pawn)
        {
            base.Notify_Unequipped(pawn);

            if (pawn.abilities == null || Props.AbilityDefs.NullOrEmpty()) return;

            foreach (AbilityDef def in Props.AbilityDefs)
            {
                // 移除技能
                pawn.abilities.RemoveAbility(def);
            }
        }
    }
}
