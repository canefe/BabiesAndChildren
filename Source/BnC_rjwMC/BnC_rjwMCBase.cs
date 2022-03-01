using HugsLib;
using RimWorld;
using System.Runtime.Remoting.Messaging;
using Verse;

namespace BabiesAndChildren
{
    public class BnCMC : ModBase
    {
        public static BnCMC Instance { get; private set; }
        
        public override string ModIdentifier => "Babies_and_Children_MC_Patch";

        private BnCMC()
        {
            Instance = this;
        }

        public override void DefsLoaded()
        {
            if (BnCMCSettings.enabled)
            {
                CLog.Message("MC Patch loaded and enabled!");
                BnCSettings.isMCEnabled = true;
                MCHarmonyPatches.Patch();
            }
        }

    }


}