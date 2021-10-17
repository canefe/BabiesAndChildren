using Verse;
using System.Xml;
using System.Collections.Generic;

namespace BabiesAndChildren
{
    public static class Tweaks
    {
        [TweakValue("BSH", -2f, 2f)] public static float HuHeadlocZ = 0.061f;
        [TweakValue("BSH", -2f, 2f)] public static float G_offset = 0f;
        [TweakValue("BSH", -2f, 2f)] public static float G_offsetfac = 1f;
    }


    public class HumanPatchConditional : PatchOperation
    {
        protected override bool ApplyWorker(XmlDocument xml)
        {
            if (BnCSettings.patchhumans)
            {
                try
                {
                    foreach (PatchOperation patchOperation in this.operations)
                    {
                        if (!patchOperation.Apply(xml))
                        {
                            return false;
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return true;
        }
        private List<PatchOperation> operations;
    }

}
