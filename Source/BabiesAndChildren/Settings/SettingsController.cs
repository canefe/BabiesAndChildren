using UnityEngine;
using Verse;

namespace BabiesAndChildren.Settings
{
    public class BnCSettingsController : Mod
    {
        public BnCSettingsController(ModContentPack content) : base(content)
        {
            GetSettings<BnCSettings>();
        }

        public override string SettingsCategory()
        {
            return "BnCSettings1".Translate();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            BnCSettings.DoWindowContents(inRect);
        }
    }
}