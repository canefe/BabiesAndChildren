using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace BabiesAndChildren
{
    public class AlienChildDef : Def
    {
        public string babyGraphic; // texture path for baby
        public string toddlerGraphic; // texture path for toddler
        public string childHeadGraphic; // texture path for children head
        public float childHeadOffset = 1f; // head offset
        public string childBodyGraphic; // texture path for children body
        public bool scaleChild = true;
        public bool scaleTeen = true;
        [Obsolete]
        public bool disabled = false; // is race disabled
        [Obsolete]
        public bool disableBackstories = false; // disable backstories added by this mod
        public SoundDef cryingSound; // custom crying sound def
    }
}
