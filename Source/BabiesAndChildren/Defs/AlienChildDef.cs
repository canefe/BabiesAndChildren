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
        public bool disabled = false; // is race disabled
        public bool disableBackstories = false; // disable backstories added by this mod
        public SoundDef cryingSound; // custom crying sound def
    }
}
