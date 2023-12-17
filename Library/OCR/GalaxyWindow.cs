using System.Collections.Generic;
using System.Drawing;
namespace GalacticSwampBot.Library.OCR {
    public class GalaxyWindow {
        public string Title { get; set; }
        public Point Location { get; set; }
        public IEnumerable<PlanetInfo> PlanetInfos { get; set; }
        public bool DefiniteWrongLocation { get; set; }

        public class PlanetInfo {
            public string PlayerName { get; set; }
            public int PlayerLevel { get; set; }
            public int StarbaseLevel { get; set; }
        }
    }
}
