using System.Collections.Generic;
namespace GalacticSwampBot.Library.OCR {
    public class ProcessedResult {
        public ProcessedResult(bool anyWorthy, string worthy, string all, bool tag = false) : 
            this(anyWorthy, worthy, all, new List<int>(), tag)
        {
        }
        public ProcessedResult(bool anyWorthy, string worthy, string all, List<int> worthyFromRows, bool tag = false)
        {
            AnyWorthy = anyWorthy;
            Worthy = worthy;
            All = all;
            WorthyFromRows = worthyFromRows;
            Tag = tag;
        }
        public bool Tag { get; }
        public bool AnyWorthy { get; }
        public string Worthy { get; }
        public string All { get; }
        public List<int> WorthyFromRows { get; }
    }
}
