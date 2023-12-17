using System.Collections.Generic;
namespace GalacticSwampBot.Library.OCR.Api {
    public class ParseRequest {
        public List<ParsedResult_> ParsedResults { get; set; }
        public int OCRExitCode { get; set; }
        public bool IsErroredOnProcessing { get; set; }
        public string ProcessingTimeInMilliseconds { get; set; }
        public string SearchablePDFURL { get; set; }
        
        public class Line_
        {
            public string LineText { get; set; }
            public List<Word_> Words { get; set; }
            public double MaxHeight { get; set; }
            public double MinTop { get; set; }
        }

        public class ParsedResult_
        {
            public TextOverlay_ TextOverlay { get; set; }
            public string TextOrientation { get; set; }
            public int FileParseExitCode { get; set; }
            public string ParsedText { get; set; }
            public string ErrorMessage { get; set; }
            public string ErrorDetails { get; set; }
        }

        public class TextOverlay_
        {
            public List<Line_> Lines { get; set; }
            public bool HasOverlay { get; set; }
        }

        public class Word_
        {
            public string WordText { get; set; }
            public double Left { get; set; }
            public double Top { get; set; }
            public double Height { get; set; }
            public double Width { get; set; }
        }
    }
}
