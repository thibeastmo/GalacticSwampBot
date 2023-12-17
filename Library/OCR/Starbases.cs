using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using GalacticSwampBot.Library.Utils;
namespace GalacticSwampBot.Library.OCR {
    public class Starbases : GalaxyWindowPart {
        private readonly string _dir;
        private readonly string[] _fileNames;
        public int[] StarbaseLevels { get; set; } = Array.Empty<int>();
        public Starbases(string dir, string[] fileNames)
        {
            _dir = dir;
            _fileNames = fileNames;
        }
        public Image<Rgba32>[] GetCuts(float zoom, int amountOfPlayers = -1)
        {
            var starbaseLevels = new List<Image<Rgba32>>();
            var starbaseLevelsAll = new List<Image<Rgba32>>();
            const string starbaseLevelPrefix = "starbaseLevel";
            for (var i = 0; i < _fileNames.Length; i++){
                var bmp = GetImage(_dir + "/" + _fileNames[i]);
                var bmps = Cut(bmp, zoom);
                for (var j = 0; j < bmps.Length && (amountOfPlayers < 0 || (amountOfPlayers > 0 && amountOfPlayers > i * bmps.Length + j)); j++){
                    // var starbaseLevelFile = _dir + "/" +starbaseLevelPrefix + (i * bmps.Length + j) + ".png";
                    // bmps[j].Save(starbaseLevelFile);
                    bmps[j].ApplyPostProcessing();
                    bmps[j].CropByGReversed(ImageHelper.PreprocessBackgroundColor.G);
                    // bmps[j].Save(starbaseLevelFile.Replace(".png", "_preprocessed.png"));
                    if (amountOfPlayers > 0 || !IsNonCharacterImage(bmps[j])){
                        if (starbaseLevels.Count != starbaseLevelsAll.Count){
                            starbaseLevels.Clear();
                            starbaseLevels.AddRange(starbaseLevelsAll);
                        }
                        starbaseLevels.Add(bmps[j]);
                    }
                    starbaseLevelsAll.Add(bmps[j]);
                }
            }
            return starbaseLevels.ToArray();
        }

        private static Image<Rgba32>[] Cut(Image<Rgba32> bmp, float zoom)
        {
            var xLocations = new[]
            {
                76, 160, 242, 326, 409, 492
            };
            const int maxWidth = 16;
            var bmps = new Image<Rgba32>[xLocations.Length];
            for (var i = 0; i < xLocations.Length; i++){
                var rect = new Rectangle((int)(xLocations[i] * zoom) - maxWidth / 2, 0, maxWidth, bmp.Height); //+1 is because it sometimes gets 1 pixel from the icon next to it which fks up the ocr
                bmps[i] = bmp.Cut(rect);
                bmps[i].CropByB(100);
            }
            return bmps;
        }

        public void Initialize(string text)
        {
            text = text.Replace(" ", string.Empty).Replace('0','C');
            // var splitted = ReplaceSplitters(text, StarbaseLevelSplitValue).ToLower().Split(StarbaseLevelSplitValue.Replace(" ", string.Empty).ToLower());
            var lowerText = ReplaceSplittersShort(text.ToLower());
            var splitted = lowerText.Split(StarbaseLevelSplitValue.ToLower());
            if (splitted.Length == 1 && string.IsNullOrEmpty(splitted[0])) splitted[0] = "1";
            var x = 0;
            var ints = new List<int>();
            var a = new List<string>()
            {
                '"'.ToString(),
                'ф'.ToString()
            };
            foreach (var s in splitted){
                if (a.Contains(s)){
                    continue;
                }
                if (int.TryParse(ReplaceForDigit(s), out x)){
                    ints.Add(x);
                }
                else{
                    ints.Add(Unworthy);
                }
            }
            ints.RemoveAt(ints.Count-1);
            StarbaseLevels = ints.ToArray();
            for (var i = 0; i < ints.Count; i++){
                if (ints[i] < 0){
                    bool ok = true;
                }
            }
        }
    }
}
