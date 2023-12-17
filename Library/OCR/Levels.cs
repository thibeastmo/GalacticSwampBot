using System;
using System.Collections.Generic;
using SixLabors.ImageSharp;
using GalacticSwampBot.Library.Utils;
using SixLabors.ImageSharp.PixelFormats;
namespace GalacticSwampBot.Library.OCR {
    public class Levels : GalaxyWindowPart {
        private readonly string _dir;
        private readonly string[] _fileNames;
        public int[] PlayerLevels { get; set; } = Array.Empty<int>();
        public Levels(string dir, string[] fileNames)
        {
            _dir = dir;
            _fileNames = fileNames;
        }
        public Image<Rgba32>[] GetCuts(float zoom, int amountOfPlayers = -1)
        {
            var playerLevels = new List<Image<Rgba32>>();
            var playerLevelsAll = new List<Image<Rgba32>>();
            const string playerLevelPrefix = "playerLevel";
            for (var i = 0; i < _fileNames.Length; i++){
                var bmp = GetImage(_dir + "/" + _fileNames[i]);
                var bmps = Cut(bmp, zoom);
                for (var j = 0; j < bmps.Length && (amountOfPlayers < 0 || (amountOfPlayers > 0 && amountOfPlayers > i * bmps.Length + j)); j++){
                    // var playerLevelFile = _dir + "/" +playerLevelPrefix + (i * bmps.Length + j) + ".png";
                    // bmps[j].Save(playerLevelFile);
                    bmps[j].ApplyPostProcessing();
                    bmps[j].CropByLeftSplitColor(ImageHelper.PreprocessBackgroundColor);
                    bmps[j].CropByGReversed(200, modus: 2);
                    bmps[j] = bmps[j].ScaleImageHeight(14);
                    bmps[j].CropByGReversed(200, modus: 2);
                    // if (bmps[j].ContainsBlack()){
                    //     bmps[j].SetBlackAndWhite();
                    //     bmps[j].Save(playerLevelFile.Replace(".png", "_blackandwhite.png"));
                    //     bmps[j].SetRGBValues(r: 0, b: 0);
                    //     bmps[j].CropByGReversed(ImageHelper.PreprocessBackgroundColor.G, modus: 2);
                    // }
                    bmps[j].ScaleImageHeight(16);
                    // bmps[j].Save(playerLevelFile.Replace(".png", "_preprocessed.png"));
                    if (amountOfPlayers > 0 || !IsNonCharacterImage(bmps[j])){
                        if (playerLevels.Count != playerLevelsAll.Count){
                            playerLevels.Clear();
                            playerLevels.AddRange(playerLevelsAll);
                        }
                        playerLevels.Add(bmps[j]);
                    }
                    playerLevelsAll.Add(bmps[j]);
                }
            }
            return playerLevels.ToArray();
        }
        private static Image<Rgba32>[] Cut(Image<Rgba32> bmp, float zoom)
        {
            var xLocations = new[]
            {
                76, 160, 242, 326, 409, 492
            };
            const int maxWidth = 51;
            var bmps = new Image<Rgba32>[xLocations.Length];
            for (var i = 0; i < xLocations.Length; i++){
                var rect = new Rectangle((int)(xLocations[i] * zoom)-maxWidth/2, 0, maxWidth, bmp.Height);
                bmps[i] = bmp.Cut(rect);
                bmps[i].CropByB(100);
            }
            return bmps;
        }
        public void Initialize(string text)
        {
            text = text.Replace(" ", string.Empty);
            // var splitted = ReplaceSplitters(text, PlayerLevelSplitValue).ToLower().Replace("ax>", "<x>").Split(PlayerLevelSplitValue.Replace(" ", string.Empty).ToLower());
            var splitted = ReplaceSplittersShort(text).ToLower().Split(PlayerLevelSplitValue.ToLower());
            for (var i = 0; i + 1 < splitted.Length; i++){
                if (string.IsNullOrEmpty(splitted[i])){
                    splitted[i] = "1";
                }
            }
            var x = 0;
            var ints = new List<int>();
            foreach (var s in splitted){
                if (int.TryParse(ReplaceForDigit(s), out x)){
                    ints.Add(x);
                }
                else{
                    ints.Add(Unworthy);
                }
            }
            ints.RemoveAt(ints.Count-1);
            for (var i = 0; i < ints.Count; i++){
                if (ints[i] > 899){
                    ints[i] -= 500;
                }
            }
            PlayerLevels = ints.ToArray();
            for (var i = 0; i < ints.Count; i++){
                if (ints[i] < 0){
                    bool ok = true;
                }
            }
        }
    }
}
