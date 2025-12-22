using System.Collections.Generic;
using System.Reflection; // 引用反射庫
using RimWorld;
using Verse;

namespace VoidCompany
{
    public class VC_Gun_Porccubus : ThingWithComps
    {
        public override void PostMake()
        {
            base.PostMake();
            SwapCompEquippable();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            // 在讀取存檔後也要執行替換
            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                SwapCompEquippable();
            }
        }

        private void SwapCompEquippable()
        {
            // 1. 檢查是否已經成功替換成我們的組件
            // 這裡必須直接檢查清單，避開 GetComp 的快取
            bool alreadyHasIt = false;
            for (int i = 0; i < this.AllComps.Count; i++)
            {
                if (this.AllComps[i] is CompChangeableMode)
                {
                    alreadyHasIt = true;
                    break;
                }
            }
            if (alreadyHasIt) return;

            // 2. 找到原本遊戲自動生成的 CompEquippable (它會在清單中)
            CompEquippable oldComp = GetComp<CompEquippable>();

            if (oldComp != null)
            {
                // 3. 建立我們的新組件
                CompChangeableMode newComp = new CompChangeableMode();
                newComp.parent = this;

                // 4. 移植 VerbTracker (保留 XML 定義的射擊模式)
                newComp.verbTracker = oldComp.verbTracker;
                if (newComp.verbTracker != null)
                {
                    newComp.verbTracker.directOwner = newComp;
                }

                // 5. 替換清單中的物件
                if (this.AllComps.Contains(oldComp))
                {
                    this.AllComps.Remove(oldComp);
                    this.AllComps.Add(newComp);
                }

                // 6. 【關鍵修正】清除 compsByType 快取
                // 這會強迫遊戲下次 GetComp 時重新掃描清單，才會發現我們的新組件
                ClearCompsCache();

                // 7. 初始化新組件
                newComp.InitVerbsForModeSwitching();
            }
        }

        // 使用反射來存取並清除私有的 compsByType 字典
        private void ClearCompsCache()
        {
            try
            {
                // 取得 ThingWithComps 類別中的 compsByType 欄位資訊
                FieldInfo cacheField = typeof(ThingWithComps).GetField("compsByType", BindingFlags.Instance | BindingFlags.NonPublic);

                if (cacheField != null)
                {
                    // 將其設為 null，遊戲下次 GetComp 時會自動重建快取
                    cacheField.SetValue(this, null);
                }
            }
            catch (System.Exception ex)
            {
                Log.Warning("[VC_Gun_Porccubus] 清除組件快取失敗: " + ex.Message);
            }
        }
    }
}