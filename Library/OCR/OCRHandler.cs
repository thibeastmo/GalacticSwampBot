using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Entities;
using GalacticSwampBot.Library.OCR.Api;
using GalacticSwampBot.Library.Utils;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using JsonConvert=Newtonsoft.Json.JsonConvert;
namespace GalacticSwampBot.Library.OCR {
    public class OCRHandler {

        private readonly Title _title;
        private readonly Players _players;
        private readonly Levels _levels;
        private readonly Starbases _starbases;
        private readonly float _zoom;
        public OCRHandler(Title title, Players players, Levels levels, Starbases starbases)
        {
            _title = title;
            _players = players;
            _levels = levels;
            _starbases = starbases;

            try{
                var img = title.GetImage();
                _zoom = img.Width / 552f;
            }
            catch (Exception ex){
                _zoom = 1f;
            }
        }
        public ProcessedResult Read(DiscordChannel[] discordChannels, int amountOfPlayers = -1, int newMargin = -1, short ocrEngine = 5)
        {
            newMargin = newMargin < 0 ? 40 : newMargin; 
            try{
                var img = new Image<Rgba32>(1, 1);
                var images = _title.GetCuts();
                images[0] = images[0].ScaleImageHeight(16);
                img = CombineNextToEachOther(img, new[] { images[0] }, string.Empty, margin: FirstLastImageHeightMargin);
                var imgCoordX = new Image<Rgba32>(1, 1);
                var imgCoordY = new Image<Rgba32>(1, 1);
                var imgCoordXAmount = 0;
                var imgCoordYAmount = 0;
                if (images.Length > 1){
                    var coordImages = images[1].SeparateCharacters(g: 120);
                    imgCoordXAmount = coordImages.Length;
                    for (var i = 0; i < coordImages.Length; i++){
                        coordImages[i] = coordImages[i].CropByGReversed(215).ScaleImageHeight(16);
                    }
                    imgCoordX = CombineNextToEachOther(imgCoordX, coordImages, string.Empty, margin: 0, extraLeftMargin: 0);
                    imgCoordX = imgCoordX.CropByGReversed(215);
                    if (images.Length > 2){
                        coordImages = images[2].SeparateCharacters(g: 120);
                        imgCoordYAmount = coordImages.Length;
                        for (var i = 0; i < coordImages.Length; i++){
                            coordImages[i] = coordImages[i].CropByGReversed(215).ScaleImageHeight(16);
                        }
                        imgCoordY = CombineNextToEachOther(imgCoordY, coordImages, string.Empty, margin: 0, extraLeftMargin: 0);
                        imgCoordY = imgCoordY.CropByGReversed(215);
                    }
                }
                img = CombineNextToEachOther(img, new[] { imgCoordX.IncreaseG(120), imgCoordY.IncreaseG(120) }, GalaxyWindowPart.TitleSplitValue, margin:newMargin);
                images = _levels.GetCuts(_zoom, amountOfPlayers);
                var amountOfLevels = images.Length;
                img = CombineNextToEachOther(img, images, GalaxyWindowPart.PlayerLevelSplitValue, margin:newMargin);
                images = DisposeImages(images);
                images = _starbases.GetCuts(_zoom, amountOfPlayers);
                var amountOfStarbases = images.Length;
                img = CombineNextToEachOther(img, images, GalaxyWindowPart.StarbaseLevelSplitValue, margin:newMargin);
                images = DisposeImages(images);
                images = _players.GetCuts(_zoom, amountOfLevels);
                var expectedAmountOfPlayers = Math.Max(Math.Max(amountOfStarbases, amountOfLevels), images.Length);
                if (images.Length != amountOfLevels || images.Length != amountOfStarbases) return Read(discordChannels, amountOfPlayers: expectedAmountOfPlayers, newMargin: newMargin);
                img = CombineNextToEachOther(img, images, GalaxyWindowPart.PlayerNameSplitValue, margin:newMargin, widthMargin:10);
                img = img.CropByGReversed(200);
                img = img.SetBackground(ImageHelper.PreprocessBackgroundColor, margin: FirstLastImageHeightMargin);
                // images = DisposeImages(images);
                var combinedFileName = _title.Dir + "/combined.png";
                img.Save(combinedFileName);
                if (expectedAmountOfPlayers == 0) return new ProcessedResult(false, string.Empty, "No players in the system.");

                ////////////////////////
                var parseRequest = OCRSpaceRequester.PostRequest(discordChannels, combinedFileName, ocrEngine, detectOrientation: false);
                if (parseRequest == null) return new ProcessedResult(false, string.Empty, "API request failed");
                if (parseRequest.ParsedResults != null && parseRequest.ParsedResults.Count > 0 && 
                    !string.IsNullOrEmpty(parseRequest.ParsedResults[0].ErrorMessage) && parseRequest.ParsedResults[0].ErrorMessage == Constants.OCRApiAnswers.TimedOut) return new ProcessedResult(false, string.Empty, Constants.OCRApiAnswers.TimedOut);
                ////////////////////////

                if (ContainsSplitValue(parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText, GalaxyWindowPart.PlayerNameSplitValue)){
                    parseRequest =  OCRSpaceRequester.PostRequest(discordChannels, combinedFileName, ocrEngine, detectOrientation: true);
                }

                var planetInfos = new List<GalaxyWindow.PlanetInfo>();
                var planetInfosAll = new List<GalaxyWindow.PlanetInfo>();
                var worthyPlayerRows = new List<int>();
                var definiteWrongLocation = false;
                if (!parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText.ToLower().Contains(" " + GalaxyWindowPart.TitleSplitValue.ToLower() + " ") && !parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText.ToLower().Contains(" " + GalaxyWindowPart.TitleSplitValue.ToLower()) && !parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText.ToLower().Contains(GalaxyWindowPart.TitleSplitValue.ToLower() + " "))
                {
                    _title.InitializeName(parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText);
                    parseRequest.ParsedResults[0].TextOverlay.Lines.RemoveAt(0);
                }
                if (parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText.ToLower().Contains(GalaxyWindowPart.TitleSplitValue.ToLower()) || int.TryParse(parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText, out int parsedValue)){
                    _title.InitializeLocation(parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText);
                    if (_title.Location.X.ToString().Length != imgCoordXAmount || _title.Location.Y.ToString().Length != imgCoordYAmount) definiteWrongLocation = true;
                    parseRequest.ParsedResults[0].TextOverlay.Lines.RemoveAt(0);
                }
                if (parseRequest.ParsedResults[0].TextOverlay.Lines.Count > 1 && GalaxyWindowPart.ReplaceSplittersShort(parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText.ToLower()).Contains(GalaxyWindowPart.PlayerLevelSplitValue.ToLower())){
                    _levels.Initialize(parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText);
                    parseRequest.ParsedResults[0].TextOverlay.Lines.RemoveAt(0);
                }
                if (parseRequest.ParsedResults[0].TextOverlay.Lines.Count > 1 && GalaxyWindowPart.ReplaceSplittersShort(parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText.ToLower()).Contains(GalaxyWindowPart.StarbaseLevelSplitValue.ToLower())){
                    _starbases.Initialize(parseRequest.ParsedResults[0].TextOverlay.Lines[0].LineText);
                    parseRequest.ParsedResults[0].TextOverlay.Lines.RemoveAt(0);
                }
                if (parseRequest.ParsedResults[0].TextOverlay.Lines.Count > 0){
                    var sb = new StringBuilder();
                    foreach (var line in parseRequest.ParsedResults[0].TextOverlay.Lines){
                        sb.Append(line.LineText);
                    }
                    _players.Initialize(sb.ToString());
                }
                if (amountOfPlayers < 0 && _players != null && (_players.PlayerNames.Length > _levels.PlayerLevels.Length || _players.PlayerNames.Length > _starbases.StarbaseLevels.Length)){
                    return Read(discordChannels, _players.PlayerNames.Length, newMargin: newMargin);
                }
                var mostVariables = _levels.PlayerLevels.Length;
                if (_players.PlayerNames.Length > mostVariables) mostVariables = _players.PlayerNames.Length;
                if (_starbases.StarbaseLevels.Length > mostVariables) mostVariables = _starbases.StarbaseLevels.Length;
                var worthyPlayerImages = new List<Image<Rgba32>>();
                for (var i = 0; i < mostVariables; i++){
                    var playerLevel = -1;
                    var starbaseLevel = -1;
                    var playerName = string.Empty;
                    if (_levels.PlayerLevels.Length > i){
                        playerLevel = _levels.PlayerLevels[i];
                    }
                    if (_starbases.StarbaseLevels.Length > i){
                        starbaseLevel = _starbases.StarbaseLevels[i];
                    }
                    if (_players.PlayerNames.Length > i){
                        playerName = _players.PlayerNames[i];
                    }
                    var gw = new GalaxyWindow.PlanetInfo()
                    {
                        PlayerLevel = playerLevel,
                        PlayerName = playerName,
                        StarbaseLevel = starbaseLevel
                    };
                    if (GalaxyWindowPart.IsWorthy(playerLevel)){
                        planetInfos.Add(gw);
                        worthyPlayerImages.Add((Image<Rgba32>)Image.Load(_title.Dir + "/" + Players.PlayerNamesPrefix + i + ".png"));
                        worthyPlayerRows.Add((int)Math.Ceiling((double)_players.PlayerNames.Length / 6) - 1);
                    }
                    planetInfosAll.Add(gw);
                }
                worthyPlayerRows = worthyPlayerRows.Distinct().ToList();
                if (worthyPlayerImages.Count > 0){
                    var img2 = _title.GetWhiteImage();
                    img2 = CombineNextToEachOther(img2, worthyPlayerImages.ToArray(), string.Empty, margin: 5, maxNextToEachOther: 2, scale: 2);
                    img2.Save(_title.Dir + "/" + Constants.ImageNames.WORTHY);
                }
                else if (planetInfosAll.Count == 0){
                    bool ok = true;
                }
                bool doTag = false;
                for (var i = 0; i < planetInfosAll.Count; i++){
                    if (GalaxyWindowPart.IsWorthy(planetInfosAll[i].PlayerLevel)){
                        if (planetInfosAll[i].StarbaseLevel < 0){
                            if (newMargin != 20) return Read(discordChannels, amountOfPlayers, newMargin: 20);
                            bool ok = true; 
                        }
                        if (string.IsNullOrEmpty(planetInfosAll[i].PlayerName)){
                            bool ok = true;
                        }
                    }
                    else if (planetInfosAll[i].PlayerLevel < 0){
                        doTag = true;
                    }
                }
                if (_title.Location.X < 0 || _title.Location.Y < 0 || _title.Location.X > 2000 || _title.Location.Y > 2000){
                    definiteWrongLocation = true;
                    bool ok = true;
                }
                var galaxyWindow = new GalaxyWindow()
                {
                    Title = _title.GalaxyName,
                    Location = _title.Location,
                    PlanetInfos = planetInfos,
                    DefiniteWrongLocation = definiteWrongLocation
                };
                var galaxyWindowAll = new GalaxyWindow()
                {
                    Title = _title.GalaxyName,
                    Location = _title.Location,
                    PlanetInfos = planetInfosAll,
                    DefiniteWrongLocation = definiteWrongLocation
                };
                return new ProcessedResult(planetInfos.Count > 0, JsonConvert.SerializeObject(galaxyWindow), JsonConvert.SerializeObject(galaxyWindowAll), worthyPlayerRows, tag: doTag);
            }
            catch (Exception ex){
                Bot.WriteInfoLog(ex.Message + ":\n\n" + ex.StackTrace);
                if (ex.InnerException != null){
                    Bot.WriteInfoLog("\n\nINNER EXCEPTION:\n" + ex.InnerException.Message + ":\n\n" + ex.InnerException.StackTrace);
                }
                return new ProcessedResult(false, string.Empty, "Probably something wrong with the image");
            }
        }
        private static bool ContainsSplitValue(string text, string splitValue)
        {
            splitValue = splitValue.Replace(" ", string.Empty);
            text = text.Replace(" ", string.Empty);
            text = text.ToLower().Replace('а', 'a');
            splitValue = splitValue.ToLower();
            if (text.Contains(splitValue)){
                return true;
            }
            var splitValues = new List<string>()
            {
                GalaxyWindowPart.PlayerLevelSplitValue.Replace(" ", string.Empty).ToLower(),
                GalaxyWindowPart.StarbaseLevelSplitValue.Replace(" ", string.Empty).ToLower(),
                GalaxyWindowPart.PlayerNameSplitValue.Replace(" ", string.Empty).ToLower(),
            };
            splitValues.Remove(splitValue);
            if (text.Contains(splitValues[0]) || text.Contains(splitValues[1])){
                return false;
            }
            if (splitValue.Length > 1 && text.Contains(splitValue.Remove(1, 1))){
                return true;
            }
            return false;
        }

        private const int FirstLastImageHeightMargin = 13;
        private static Image<Rgba32> CombineNextToEachOther(Image<Rgba32> img, Image<Rgba32>[] cuts, string inbetweenKeyword,
            int margin = 60, int widthMargin = 13, int maxNextToEachOther = int.MaxValue, int scale = 1, int extraLeftMargin = 20)
        {
            if (cuts == null || cuts.Length == 0) return img;
            if (scale != 1){
                for (var i = 0; i < cuts.Length; i++){
                    cuts[i] = cuts[i].ScaleImageHeight(cuts[i].Height * scale);
                }
            }
            var originalImageHeight = img.Height;
            var inbetweenKeywordRectangle = inbetweenKeyword.Length > 0 ? GetKeywordSize(inbetweenKeyword) : new FontRectangle();
            var totalHeight = 0;
            var totalWidth = extraLeftMargin;
            var nextToEachOtherCounter = 0;
            foreach (var cut in cuts){
                if (totalHeight < cut.Height) totalHeight = cut.Height;
                totalWidth += widthMargin + (int)inbetweenKeywordRectangle.Width + +((int)inbetweenKeywordRectangle.Width > 0 ? widthMargin : 0) + cut.Width + widthMargin;
                if (nextToEachOtherCounter == maxNextToEachOther){
                    totalHeight += cut.Height + margin;
                    nextToEachOtherCounter = 0;
                }
                nextToEachOtherCounter++;
            }
            if (totalHeight < inbetweenKeywordRectangle.Height) totalHeight = (int)inbetweenKeywordRectangle.Height;
            totalHeight += originalImageHeight + margin + margin;
            if (img.Width > totalWidth) totalWidth = img.Width;
            img = img.ResizeImage(totalWidth, totalHeight);
            var totalX = extraLeftMargin;
            var totalY = originalImageHeight + margin - 1;
            nextToEachOtherCounter = 0;
            foreach (var cut in cuts){
                if (totalX > extraLeftMargin) totalX += widthMargin;
                img = img.DrawImage(cut, new Point(totalX, totalY));
                nextToEachOtherCounter++;
                if (nextToEachOtherCounter < maxNextToEachOther){
                    totalX += cut.Width;
                }
                else{
                    totalX = extraLeftMargin;
                    totalY += cut.Height + margin;
                    nextToEachOtherCounter = 0;
                }
                if (inbetweenKeywordRectangle.Width > 0){
                    totalX += (int)(widthMargin*1.5);
                    Write(img, new PointF(totalX, totalY-2), inbetweenKeyword);
                    totalX += (int)inbetweenKeywordRectangle.Width + (int)(widthMargin*.5);
                }
            }
            return img;
        }
        private static FontRectangle GetKeywordSize(string keyword)
        {
            return TextMeasurer.Measure(keyword, new TextOptions(Font));
        }
        // private static readonly Font FontBig = SystemFonts.CreateFont("Arial Black", 17f, FontStyle.Bold);
        private static readonly Font Font = SystemFonts.CreateFont("Arial", 19f);
        private static void Write(Image<Rgba32> img, PointF location, string keyword)
        {
            img.Write(keyword, Font, Color.Black, location);
        }
        private static Image<Rgba32>[] DisposeImages(Image<Rgba32>[] images)
        {
            foreach (var image in images){
                image.Dispose();
            }
            return images;
        }
    }
}
