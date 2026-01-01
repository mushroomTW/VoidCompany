using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace VoidCompany
{
    // 1. 定義屬性 (XML 用)
    public class HediffCompProperties_HangryDiff : HediffCompProperties
    {
        public HediffCompProperties_HangryDiff()
        {
            this.compClass = typeof(HediffComp_HangryDiff);
        }
    }

    // 2. 定義邏輯
    public class HediffComp_HangryDiff : HediffComp
    {
        // 設定檢查頻率：250 ticks 約 4 秒，省效能
        private const int CheckInterval = 250;

        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            Pawn pawn = this.Pawn; // Hediff 直接就有 Pawn 屬性可以抓人

            // 1. 基礎檢查 (人還活著、不是每 tick 都跑)
            if (pawn == null || pawn.Dead || !pawn.IsHashIntervalTick(CheckInterval))
                return;

            // 2. 檢查飽食度
            // 必須檢查 needs.food 是否存在 (避免機械體或不需要吃東西的種族報錯)
            Need_Food foodNeed = pawn.needs?.food;

            if (foodNeed != null)
            {
                // 3. 飽食度歸零 (<= 0) 且 當前沒有發瘋
                if (foodNeed.CurLevelPercentage <= 0f && !pawn.InMentalState)
                {
                    // 4. 觸發狂暴
                    pawn.mindState.mentalStateHandler.TryStartMentalState(
                        MentalStateDefOf.Berserk,
                        "Hunger induced rage", // 原因 (Log用)
                        true, // 強制
                        false // 不檢查條件
                    );
                }
            }
        }
    }
}
