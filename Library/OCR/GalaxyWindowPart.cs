using System;
using GalacticSwampBot.Library.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
namespace GalacticSwampBot.Library.OCR {
    public abstract class GalaxyWindowPart {
        public const string PlayerLevelSplitValue = "X";
        public const string StarbaseLevelSplitValue = "X";
        public const string PlayerNameSplitValue = "< S >";
        public const string TitleSplitValue = "V";
        public static string ReplaceForDigit(string text)
        {
            if (string.IsNullOrEmpty(text)) return "1";
            text = text.Replace("o", "0");
            text = text.Replace("i", "1"); //not the same
            text = text.Replace("і", "1"); //not the same
            text = text.Replace("l", "1");
            text = text.Replace("I", "1");
            text = text.Replace("›", "1");
            text = text.Replace(">", "1");
            text = text.Replace("j", "1");
            text = text.Replace("|", "1");
            text = text.Replace("\\", "1");
            text = text.Replace("}", "1");
            text = text.Replace("]", "1");
            text = text.Replace("[", "1");
            text = text.Replace("¡", "1");
            text = text.Replace("ì", "1");
            text = text.Replace("'", "1");
            text = text.Replace(")", "1");
            text = text.Replace("г", "1");
            text = text.Replace("r", "1");
            text = text.Replace("t", "1");
            text = text.Replace("m", "111");
            text = text.Replace("n", "11");
            text = text.Replace("y", "11");
            text = text.Replace("⅓", "11");
            text = text.Replace("½", "1");
            text = text.Replace("⅜", "141");
            text = text.Replace("\"", "11");
            text = text.Replace("э", "2");
            text = text.Replace("z", "2");
            text = text.Replace("?", "2");
            text = text.Replace("з", "3");
            text = text.Replace("{", "4");
            text = text.Replace("A", "4");
            text = text.Replace("a", "4");
            text = text.Replace("а", "4");
            text = text.Replace("©", "4");
            text = text.Replace("@", "4");
            text = text.Replace("&", "4");
            text = text.Replace("q", "4");
            text = text.Replace("Ц", "4");
            text = text.Replace("Ц", "4");
            text = text.Replace("ц", "4");
            text = text.Replace("д", "4");
            text = text.Replace("s", "5");
            text = text.Replace("§", "5");
            text = text.Replace("g", "8");
            text = text.Replace("B", "8");
            text = text.Replace("b", "8");
            if (text.Length > 1 && text.StartsWith("0")){
                text = text.Insert(0, "1");
            }
            return text;
        }
        public static bool IsNonCharacterImage(Image<Rgba32> image, bool checkFront = false, bool checkForBlackColor = true) //!checkFront == checkFrontAndBack
        {
            var valueFront = true;
            var valueBack = true;
            var valueHorizontal = true;
            var anyBlackColor = false;
            var blackColor = new Rgba32(0, 0, 0);
            image.ProcessPixelRows(accessor => {
                for (var j = 0; j < accessor.Height; j++)
                {
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    if (!checkFront){
                        ref var tempColor1 = ref pixelRow[0];
                        ref var tempColor2 = ref pixelRow[accessor.Width-1];
                        if (ColorIsEqualToPreprocessColor(tempColor1)){
                            valueFront = false;
                        }
                        if (ColorIsEqualToPreprocessColor(tempColor2)){
                            valueBack = false;
                        }
                        bool allEmpty = true;
                        for (var i = 0; i < pixelRow.Length; i++){
                            ref var tempColor3 = ref pixelRow[i];
                            if (!ColorIsEqualToPreprocessColor(tempColor3)){
                                allEmpty = false;
                            }
                            if (tempColor3 == blackColor) anyBlackColor = true;
                        }
                        if (allEmpty) valueHorizontal = false;
                    }
                    else{
                        ref var tempColor = ref pixelRow[0];
                        if (ColorIsEqualToPreprocessColor(tempColor, checkFront: true)){
                            valueFront = false;
                            break;
                        }
                    }
                }
            });
            if (!checkFront){
                if (!valueHorizontal){
                    return true;
                }
                if (checkForBlackColor && anyBlackColor) return true;
                return valueFront || valueBack;
            }
            else{
                return valueFront;
            }
        }
        private static bool ColorIsEqualToPreprocessColor(Rgba32 color, bool checkFront = false)
        {
            if (color == ImageHelper.PreprocessBackgroundColor){
                return true;
            }
            // var least = checkFront ? 51 : 53;
            var least = 0;
            if (color.R == ImageHelper.PreprocessBackgroundColor.R &&
                     color.G >= ImageHelper.PreprocessBackgroundColor.G - least &&
                     color.B == ImageHelper.PreprocessBackgroundColor.B){
                return true;
            }
            return false;
        }
        protected static Image<Rgba32> GetImage(string fileLocation)
        {
            try{
                var image = Image.Load(fileLocation);
                if (image is Image<Rgb24>){
                    return image.CloneAs<Rgba32>(image.GetConfiguration());
                }
                return (Image<Rgba32>)image;
            }
            catch (Exception ex){
                return null;
            }
        }
        public static bool IsWorthy(int playerLevel)
        {
            return playerLevel > 100;
        }

        public static string ReplaceSplittersShort(string text)
        {
            return text
                .Replace('р'.ToString().ToLower(), "p")
                .Replace('р', 'p')
                .Replace('¥', 'v')
                .Replace('×', 'x')
                .Replace('*', 'x');
        }
        public static string ReplaceSplitters(string text, string splitValue)
        {
            splitValue = splitValue.Replace(" ", string.Empty);
            text = text
                .Replace(" ", string.Empty)
                .Replace('‹', '<')
                .Replace('≤', '<')
                .Replace('›', '>')
                .Replace('»', '>')
                .Replace('×', 'x')
                .Replace('*', 'x')
                .Replace('«', '<')
                .Replace('А', 'A')
                .Replace('А', 'A')
                .Replace("<>", splitValue);
            if (splitValue == StarbaseLevelSplitValue.Replace(" ",string.Empty)){
                text = text.Replace("<4>", StarbaseLevelSplitValue.Replace(" ",string.Empty));
            }
            // var sb = new StringBuilder();
            // for (var i = 0; i < text.Length; i++){
            //     if (sb.Length == 0 && text[i] == splitValue[0]){
            //         sb.Append(text[i]);
            //     }
            //     else if (sb.Length > 0) {
            //         if (sb.Length == splitValue.Length){
            //             sb.Clear();
            //         }
            //         else if (string.Equals(text[i].ToString(), splitValue[sb.Length].ToString(), StringComparison.CurrentCultureIgnoreCase)){
            //             sb.Append(text[i]);
            //         }
            //         else{
            //             text = text.Insert(i, splitValue[sb.Length].ToString());
            //             sb.Append(splitValue[sb.Length]);
            //             i--;
            //         }
            //     }
            // }
            return text;
        }

        protected static int Unworthy => -1;
    }
}
