using System;
using System.Collections.Generic;
using System.Linq;
using GalacticSwampBot.Library.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
namespace GalacticSwampBot.Library.OCR {
    public class Players : GalaxyWindowPart {
        private readonly string _dir;
        private readonly string[] _fileNames;
        public const string PlayerNamesPrefix = "playerName";
        public string[] PlayerNames { get; set; } = Array.Empty<string>();
        public Players(string dir, string[] fileNames)
        {
            _dir = dir;
            _fileNames = fileNames;
        }
        public Image<Rgba32>[] GetCuts(float zoom, int atLeast)
        {
            var playerNames = new List<Image<Rgba32>>();
            for (var i = 0; i < _fileNames.Length; i++){
                var bmp = GetImage(_dir + "/" + _fileNames[i]);
                var bmps = Cut(bmp, zoom);
                for (var j = 0; j < bmps.Length; j++){
                    var playerNameFile = _dir + "/" +PlayerNamesPrefix + (i * bmps.Length + j) + ".png";
                    bmps[j].Save(playerNameFile);
                    bmps[j].ApplyPostProcessing();
                    // bmps[j].Save(playerNameFile.Replace(".png", "_preprocessed.png"));
                    bmps[j].CropByGReversed(220);
                    // bmps[j].Save(playerNameFile.Replace(".png", "_cropped.png"));
                    bmps[j].ScaleImageHeight(16);
                    // bmps[j].Save(playerNameFile.Replace(".png", "_scaled.png"));
                    if (atLeast > i*bmps.Length+j || !IsNonCharacterImage(bmps[j], checkForBlackColor: false)){
                        playerNames.Add(bmps[j]);
                    }
                }
            }
            return playerNames.ToArray();
        }
        private static Image<Rgba32>[] Cut(Image<Rgba32> bmp, float zoom)
        {
            var xLocations = new[]
            {
                33,117,200,283,366,450
            };
            const int maxWidth = 66;
            const int extraWidth = 18;
            var width = (int)((maxWidth + extraWidth) * zoom);
            var bmps = new Image<Rgba32>[xLocations.Length];
            for (var i = 0; i < xLocations.Length; i++){
                var rect = new Rectangle((int)(xLocations[i] * zoom) - extraWidth/2, 0, width, bmp.Height); //+1 is because it sometimes gets 1 pixel from the icon next to it which fks up the ocr
                bmps[i] = bmp.Cut(rect);
                // bmps[i].Save("Processing/cut"+i+".png");
                bmps[i].CropByRBB(85);
            }
            return bmps;
        }
        private Image<Rgba32> Cover(Image<Rgba32> bmp, float zoom, int row, IReadOnlyList<int> playerLevels)
        {
            var xLocations = new[]
            {
                33,117,200,283,366,450
            };
            const int maxWidth = 66;
            for (var i = 0; i < xLocations.Length; i++){
                if (!IsWorthy(playerLevels[row * xLocations.Length + i])) {
                    var rect = new RectangleF((int)(xLocations[i] * zoom), 0, maxWidth, bmp.Height);
                    bmp.Mutate(image => image.Fill(Color.Black, rect));
                }
            }
            return bmp.ApplyPostProcessing();
        }
        public void Initialize(string text)
        {
            text = text.Replace(" ", string.Empty);
            var splitted = ReplaceSplitters(text, PlayerNameSplitValue).ToLower().Split(PlayerNameSplitValue.Replace(" ", string.Empty).ToLower());
            var namesList = splitted.ToList();
            if (string.IsNullOrEmpty(namesList[splitted.Length-1])) namesList.RemoveAt(splitted.Length - 1);
            if (namesList[namesList.Count-1].ToLower() == "free"){
                namesList.RemoveAt(namesList.Count-1);
            }
            PlayerNames = namesList.ToArray();
            if (PlayerNames.Length == 0){
                bool ok = true;
            }
            for (var i = 0; i < PlayerNames.Length; i++){
                if (string.IsNullOrEmpty(PlayerNames[i])){
                    bool ok = true;
                }
            }
        }
    }
}
