using Verse;
using System.Xml;
using System.Text;
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

                    StringBuilder builder = new StringBuilder(operations.node.InnerXml);
                    CLog.Message("Minimum age for spawning kids: " + BnCSettings.minage);
                    if (!BnCSettings.rarekids)
                    {
                        builder.Replace("{a0}", "30");
                    }
                    else
                    {
                        builder.Replace("{a0}", "1");
                        CLog.Message("Rarer children is active. (<14 yo)");
                    }
                    builder.Replace("{minage}", BnCSettings.minage.ToString());
                    builder.Replace("{minagep}", (BnCSettings.minage + 1).ToString());
                    
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(builder.ToString());
                    XmlNode newNode = doc.DocumentElement;
                    PatchOperation patch = DirectXmlToObject.ObjectFromXml<PatchOperation>(newNode, false);
                    if (!patch.Apply(xml))
                    {
                        return false;
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
        private XmlContainer operations;
        private XmlContainer minageten;
    }

}
