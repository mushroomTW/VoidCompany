using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace VoidCompany
{
    public class Hediff_DelayedExplosiveDeath : HediffWithComps
    {
        public override void PostRemoved()
        {
            base.PostRemoved();

            // 如果已經炸得連灰都不剩，就不處理
            if (pawn.Destroyed) return;

            // 50% 機率觸發
            if (Rand.Chance(0.5f))
            {
                IntVec3 position = pawn.Position;
                Map map = pawn.Map;

                // ==========================================
                // 1. 先產生爆炸 (視覺效果優先)
                // ==========================================
                if (map != null)
                {
                    GenExplosion.DoExplosion(
                        position,
                        map,
                        1.9f,
                        DamageDefOf.Bomb,
                        null,
                        15,
                        -1f,
                        null
                    );

                    // 產生血漬
                    ThingDef bloodDef = pawn.RaceProps.BloodDef;
                    if (bloodDef != null)
                    {
                        for (int x = -1; x <= 1; x++)
                        {
                            for (int z = -1; z <= 1; z++)
                            {
                                IntVec3 cell = position + new IntVec3(x, 0, z);
                                if (cell.InBounds(map))
                                {
                                    FilthMaker.TryMakeFilth(cell, map, bloodDef, Rand.Range(1, 3));
                                }
                            }
                        }
                    }
                }

                // ==========================================
                // 2. 處理頭部消失 (關鍵修正)
                // ==========================================

                BodyPartRecord headPart = null;

                // 方法 A: 透過常見標籤尋找 (Head)
                BodyPartTagDef headTag = DefDatabase<BodyPartTagDef>.GetNamed("Head", false);
                if (headTag != null)
                {
                    headPart = pawn.health.hediffSet.GetNotMissingParts()
                        .FirstOrDefault(x => x.def.tags.Contains(headTag));
                }

                // 方法 B: 如果找不到，嘗試透過 "FullHead" 群組尋找
                if (headPart == null)
                {
                    headPart = pawn.health.hediffSet.GetNotMissingParts()
                        .FirstOrDefault(x => x.groups.Contains(BodyPartGroupDefOf.FullHead));
                }

                if (headPart != null)
                {
                    // 建立基礎 Hediff
                    Hediff hediff = HediffMaker.MakeHediff(HediffDefOf.MissingBodyPart, pawn, headPart);

                    // 【修正錯誤】這裡進行型別檢查與轉型
                    // 只有當它是 Hediff_MissingPart 時，才能存取 lastInjury
                    if (hediff is Hediff_MissingPart missingPart)
                    {
                        missingPart.lastInjury = null;
                    }

                    pawn.health.AddHediff(hediff, headPart, null);
                }

                // ==========================================
                // 3. 確保死亡
                // ==========================================
                if (!pawn.Dead)
                {
                    // 如果沒了頭還沒死，或者根本沒找到頭，強制處死
                    DamageInfo killDinfo = new DamageInfo(DamageDefOf.ExecutionCut, 9999f, 999f, -1f, null, headPart, null);
                    pawn.TakeDamage(killDinfo);

                    if (!pawn.Dead) pawn.Kill(null, null);
                }
            }
            else
            {
                // 50% 存活
                if (pawn.Map != null && !pawn.Dead)
                {
                }
            }
        }
    }
}