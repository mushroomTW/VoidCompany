using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System.Reflection; // 用於反射
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace VoidCompany
{
    public class CompChangeableMode : CompEquippable
    {
        private int currentModeIndex = 0;

        // 用來標記我們是否已經把 Verb 替換成我們的複製版本了
        private bool isInitialized = false;

        // 取得持有者 (Pawn)
        public Pawn HolderPawn => (this.parent?.ParentHolder as Pawn_EquipmentTracker)?.pawn;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref this.currentModeIndex, "currentModeIndex", 0);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                InitVerbsForModeSwitching();
            }
        }

        public override void Notify_Equipped(Pawn pawn)
        {
            base.Notify_Equipped(pawn);
            InitVerbsForModeSwitching();
        }

        public void InitVerbsForModeSwitching()
        {
            // 如果已經初始化過，就不要再複製了，直接更新狀態即可
            if (isInitialized)
            {
                UpdateVerbStats();
                return;
            }

            var verbs = this.AllVerbs;
            if (verbs == null || verbs.Count == 0) return;

            foreach (var verb in verbs)
            {
                // 【核心修正】使用 MemberwiseClone 建立一個完美的副本
                VerbProperties newProps = ShallowCloneVerbProperties(verb.verbProps);

                // 將 Verb 的屬性指向這個副本
                verb.verbProps = newProps;

                // 確保 Caster 連結正確
                if (this.HolderPawn != null)
                {
                    verb.caster = this.HolderPawn;
                }
            }

            isInitialized = true;
            UpdateVerbStats();
        }

        private VerbProperties ShallowCloneVerbProperties(VerbProperties original)
        {
            if (original == null) return null;
            MethodInfo cloneMethod = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
            return (VerbProperties)cloneMethod.Invoke(original, null);
        }

        private void UpdateVerbStats()
        {
            var verbs = this.AllVerbs;
            if (verbs.NullOrEmpty()) return;

            // 確保索引安全
            if (this.currentModeIndex >= verbs.Count) this.currentModeIndex = 0;

            for (int i = 0; i < verbs.Count; i++)
            {
                bool isActive = (i == this.currentModeIndex);

                // 直接修改我們複製出來的屬性
                verbs[i].verbProps.isPrimary = isActive;

                // 【重要】控制是否顯示攻擊按鈕 (解決雙重範圍問題)
                verbs[i].verbProps.hasStandardCommand = isActive;
            }

            // 【絕對不要呼叫 VerbsNeedReinitOnLoad】
            // 移除該行代碼，避免遊戲在運算途中遺失 Verb 參照導致崩潰
        }

        public override IEnumerable<Gizmo> CompGetEquippedGizmosExtra()
        {
            foreach (Gizmo g in base.CompGetEquippedGizmosExtra()) yield return g;

            if (this.HolderPawn != null && this.HolderPawn.Drafted)
            {
                yield return new Command_Action
                {
                    defaultLabel = "切換射擊模式",
                    defaultDesc = "切換模式。\n目前: " + GetCurrentModeLabel(),

                    // 已更新為你的圖示路徑
                    icon = ContentFinder<Texture2D>.Get("UI/shotgun", true),

                    action = delegate
                    {
                        ChangeMode();
                        SoundDefOf.Tick_Low.PlayOneShotOnCamera(null);
                    }
                };
            }
        }

        private void ChangeMode()
        {
            var verbs = this.AllVerbs;
            if (verbs.Count < 2) return;

            this.currentModeIndex = (this.currentModeIndex + 1) % verbs.Count;

            UpdateVerbStats();

            // 強制中斷當前動作，讓小人重新讀取新的 Verb 數據
            if (this.HolderPawn != null)
            {
                this.HolderPawn.stances?.CancelBusyStanceSoft();
                this.HolderPawn.jobs?.EndCurrentJob(JobCondition.InterruptForced);
            }
        }

        private string GetCurrentModeLabel()
        {
            var verbs = this.AllVerbs;
            if (verbs.NullOrEmpty()) return "Unknown";

            string label = verbs[this.currentModeIndex].verbProps.label;
            return !string.IsNullOrEmpty(label) ? label.CapitalizeFirst() : ("Mode " + (currentModeIndex + 1));
        }
    }
}