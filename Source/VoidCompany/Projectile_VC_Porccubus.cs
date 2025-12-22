using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;

namespace VoidCompany
{
    public class Projectile_VC_Porccubus : Bullet
    {
        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        // 子彈命中
        {
            base.Impact(hitThing, blockedByShield);
            // 調用基類的Impact方法處理基本命中邏輯

            Pawn pawn = hitThing as Pawn;
            if (pawn == null) return;
            // 嘗試將命中物轉換為Pawn，如果失敗則返回

            var race = pawn.RaceProps;
            if (race == null) return;
            // 獲取Pawn的種族屬性，如果為空則返回

            if (!race.IsFlesh || race.IsMechanoid) return;
            // 如果Pawn不是有血有肉的生物或是機械人，則返回

            if (pawn.stances == null || pawn.stances.stunner == null) return;
            // 如果Pawn沒有姿態或stunner，則返回

            int stunTicks = 60;
            // 設定眩暈持續時間為60個遊戲刻

            pawn.stances.stunner.StunFor(stunTicks, launcher);
            // 對Pawn施加眩暈效果，持續時間為stunTicks，施加者為launcher

        }
    }
}
