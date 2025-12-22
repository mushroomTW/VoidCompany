using RimWorld;
using Verse;
using Verse.Sound;

namespace VoidCompany
{
    public class Projectile_VC_StunMarker : Bullet
    {
        public static readonly string DelayedDeathHediffDefName = "VC_Hediff_PorccubusDelayedDeath";

        protected override void Impact(Thing hitThing, bool blockedByShield = false)
        {
            // 執行基礎撞擊邏輯
            base.Impact(hitThing, blockedByShield);

            if (hitThing != null && hitThing is Pawn hitPawn)
            {
                // 檢查條件：肉體生物且非機械族
                if (hitPawn.RaceProps.IsFlesh && !hitPawn.RaceProps.IsMechanoid)
                {
                    HediffDef hediffDef = DefDatabase<HediffDef>.GetNamedSilentFail(DelayedDeathHediffDefName);

                    if (hediffDef != null)
                    {
                        // 1. 施加計時器 Hediff (負責 5 秒後的爆炸)
                        hitPawn.health.AddHediff(hediffDef, null, null);

                        // 2. 【修正】施加暈眩效果
                        // 改用 TakeDamage 造成 Stun 傷害，這是最安全的做法
                        // 設定傷害值為 30，這通常足以造成約 5 秒左右的暈眩 (1傷害約等於30ticks暈眩，視體型而定)
                        DamageInfo stunDinfo = new DamageInfo(
                            DamageDefOf.Stun,
                            30f,              // 傷害量 (決定暈眩時間長短)
                            999f,             // 護甲穿透 (確保一定暈眩)
                            -1f,              // 角度
                            this.launcher,    // 攻擊者 (槍手)
                            null,             // 部位
                            this.def          // 武器/子彈定義
                        );

                        // 這會讓小人原地暈眩(Stunned)，出現星星特效，無法行動但不會倒地(除非痛倒)
                        hitPawn.TakeDamage(stunDinfo);

                        // 3. 播放特殊音效
                        SoundDef hitSound = DefDatabase<SoundDef>.GetNamed("Psycast_PsychicShock_LanceHit", false)
                                            ?? DefDatabase<SoundDef>.GetNamed("EnergyShield_Broken", false);

                        if (hitSound != null)
                        {
                            hitSound.PlayOneShot(hitPawn);
                        }
                    }
                }
            }
        }
    }
}