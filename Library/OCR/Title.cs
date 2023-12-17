using System;
using System.Collections.Generic;
using GalacticSwampBot.Library.Utils;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Point=System.Drawing.Point;
namespace GalacticSwampBot.Library.OCR {
    public class Title : GalaxyWindowPart {
        public readonly string Dir;
        private readonly string _fileName;
        public string GalaxyName { get; set; } = "Unknown System";
        public Point Location { get; set; } = new Point(-1, -1);
        public Title(string dir, string fileName)
        {
            Dir = dir;
            _fileName = fileName;
        }
        public Image<Rgba32> GetImage()
        {
            return GetImage(Dir + "/" + _fileName);
        }
        public Image<Rgba32> GetWhiteImage()
        {
            return GetImage(Dir + "/" + Constants.ImageNames.TITLE_CROPPED);
        }
        public Image<Rgba32>[] GetCuts()
        {
            var img = GetImage();
            img.CropEz(new Rectangle(0, 0, (int)(img.Width - img.Width * 0.15), img.Height))
                .CropByColor(Color.White);
            img.Save(Dir + "/" + Constants.ImageNames.TITLE_CROPPED);
            img.ApplyPostProcessing();
            // img.Save(Dir + "/title_cropped_preprocessed.png");
            return GetCuts(img);
        }
        private Image<Rgba32>[] GetCuts(Image<Rgba32> image)
        {
            const int lessThan = 175;
            var cuts = new List<Image<Rgba32>>();
            var blackList = new List<List<int>>();
            var partialList = new List<int>();
            var minusAmount = 2;
            
            image.ProcessPixelRows(accessor => {
                //set minusAmount
                Span<Rgba32> pixelRow = accessor.GetRowSpan(accessor.Height-minusAmount);
                if (!(pixelRow[0].R == 5 && pixelRow[0].B == 5 && pixelRow[0].G > lessThan)){
                    minusAmount = 1;
                    pixelRow = accessor.GetRowSpan(accessor.Height-minusAmount); //-2 because crop isn't always good enough for the images
                }
                for (var j = 0; j < accessor.Width; j++)
                {
                    ref var tempColor = ref pixelRow[j];
                    if (tempColor.G <= lessThan){
                        if (partialList.Count > 0){
                            if (partialList[partialList.Count - 1] + 1 != j){
                                blackList.Add(partialList);
                                partialList = new List<int>();
                            }
                        }
                        partialList.Add(j);
                    }
                }
            });
            if (partialList.Count > 0){
                blackList.Add(partialList);
            }
            if (blackList.Count > 0){
                //name
                var x1 = 0;
                var x2 = blackList[0][0]-3;
                cuts.Add(image.Cut(new Rectangle(x1, 0, x2, image.Height)));
                if (blackList.Count > 1){
                    //x
                    x1 = blackList[0][0] + 2;
                    x2 = blackList[1][0]-1 - x1;
                    cuts.Add(image.Cut(new Rectangle(x1, 0, x2, image.Height)));
                    if (blackList.Count > 2){
                        //y
                        x1 = blackList[1][blackList[1].Count-1]+2;
                        x2 = blackList[2][0]-1 - x1;
                        cuts.Add(image.Cut(new Rectangle(x1, 0, x2, image.Height)));
                    }
                }
            }
            for (var i = 0; i < cuts.Count; i++){
                cuts[i] = cuts[i].CropByGReversed(215);
            }
            return cuts.ToArray();
        }

        public void InitializeName(string text)
        {
            GalaxyName = text.Replace(" ", string.Empty);
        }
        public void InitializeLocation(string text)
        {
            text = text.Replace(" ", string.Empty);
            // var splitted = ReplaceSplitters(text, TitleSplitValue).ToLower().Split(TitleSplitValue.Replace(" ", string.Empty).ToLower());
            var splitted = ReplaceSplittersShort(text).ToLower().Split(TitleSplitValue.ToLower());
            var x = -1;
            var y = -1;
            if (string.IsNullOrEmpty(splitted[0]) || !int.TryParse(ReplaceForDigit(splitted[0]), out x)){
                x = -1;
            }
            if (splitted.Length > 1){
                if (string.IsNullOrEmpty(splitted[1]) || !int.TryParse(ReplaceForDigit(splitted[1]), out y)){
                    y = -1;
                }
            }
            if (x < 0 || y < 0){
                bool ok = true;
            }
            Location = new Point(x,y);
        }
    }
}
