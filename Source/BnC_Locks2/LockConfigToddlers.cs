using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Verse;
using BabiesAndChildren.api;
using Locks2.Core;

namespace BabiesAndChildren.Locks2
{
    public partial class LockConfigBase : LockConfig
    {
        public class LockConfigToddlers : IConfigRule
        {
            public bool enabled;

            public override float Height => 54;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public override bool Allows(Pawn pawn)
            {
                if (enabled && AgeStages.IsYoungerThan(pawn, AgeStages.Child)) return true;
                return false;
            }

            public override void DoContent(IEnumerable<Pawn> pawns, Rect rect, Action notifySelectionBegan,
                Action notifySelectionEnded)
            {
                var before = enabled;
                Widgets.CheckboxLabeled(rect, "Allow toddlers/babies", ref enabled);
                if (before != enabled) Notify_Dirty();
            }

            public override IConfigRule Duplicate()
            {
                return new ConfigRulePrisoners { enabled = enabled };
            }

            public override void ExposeData()
            {
                Scribe_Values.Look(ref enabled, "enabled", true);
            }
        }
    }
    
}
