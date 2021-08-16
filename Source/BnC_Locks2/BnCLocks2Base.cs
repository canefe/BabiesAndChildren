using HugsLib;
using RimWorld;
using System.Runtime.Remoting.Messaging;
using Verse;

namespace BabiesAndChildren
{
    public class BnCLocks2 : ModBase
    {
        public static BnCLocks2 Instance { get; private set; }
        
        public override string ModIdentifier => "Babies_and_Children Locks2 Patch";

        private BnCLocks2()
        {
            Instance = this;
        }

        public override void DefsLoaded()
        {
            Log.Message("BnC_Locks2_Patch");

        }

    }


}