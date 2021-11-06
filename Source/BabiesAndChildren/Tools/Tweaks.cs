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
                    if (BnCSettings.minageten)
                    {
                        StringBuilder builder = new StringBuilder(minageten.node.InnerXml);
                        if (!BnCSettings.rarekids)
                        {
                            builder.Replace("{a0}", "5");
                        }
                        else
                        {
                            builder.Replace("{a0}", "1");
                        }
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(builder.ToString());
                        XmlNode newNode = doc.DocumentElement;
                        PatchOperation patch = DirectXmlToObject.ObjectFromXml<PatchOperation>(newNode, false);
                        if (!patch.Apply(xml))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        StringBuilder builder = new StringBuilder(operations.node.InnerXml);
                        if (!BnCSettings.rarekids)
                        {
                            builder.Replace("{a0}", "10");
                            builder.Replace("{a1}", "20");
                            builder.Replace("{a2}", "40");
                        }
                        else
                        {
                            builder.Replace("{a0}", "1");
                            builder.Replace("{a1}", "3");
                            builder.Replace("{a2}", "5");
                        }
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(builder.ToString());
                        XmlNode newNode = doc.DocumentElement;
                        PatchOperation patch = DirectXmlToObject.ObjectFromXml<PatchOperation>(newNode, false);
                        if (!patch.Apply(xml))
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
        private XmlContainer operations;
        private XmlContainer minageten;
    }

}
