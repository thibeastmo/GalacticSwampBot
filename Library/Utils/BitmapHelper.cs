// using System;
// using System.Drawing;
// using System.Drawing.Imaging;
// using SixLabors.ImageSharp;
// using SixLabors.ImageSharp.PixelFormats;
// using SixLabors.ImageSharp.Processing;
// using Color=System.Drawing.Color;
// using Rectangle=System.Drawing.Rectangle;
// namespace GalacticSwampBot.Library.Utils {
//     
//     public static class BitmapHelper
//     {
//         public static int GetYWhereMiddlePartHasBValue(this Bitmap bmp, int b, int yCoordinate = 0)
//         {
//             var startMiddle = bmp.Width / 3;
//             var endMiddle = bmp.Width - startMiddle;
//             for (var i = yCoordinate; i < bmp.Height; i++){
//                 bool ok = true;
//                 for (var j = startMiddle; j < endMiddle; j++){
//                     if (bmp.GetPixel(j, i).B < b){
//                         ok = false;
//                         break;
//                     }
//                 }
//                 if (ok){
//                     return i;
//                 }
//             }
//             return -1;
//         }
//         public static int GetXWhereMiddlePartHasBValue(this Bitmap bmp, int b, int xCoordinate = 0)
//         {
//             var startMiddle = bmp.Height / 3;
//             var endMiddle = bmp.Height - startMiddle;
//             for (var i = xCoordinate; i < bmp.Width; i++){
//                 bool ok = true;
//                 for (var j = startMiddle; j < endMiddle; j++){
//                     if (bmp.GetPixel(i, j).B < b){
//                         ok = false;
//                         break;
//                     }
//                 }
//                 if (ok){
//                     return i;
//                 }
//             }
//             return -1;
//         }
//         public static Color GetColorAfterSkippingColorOfFirstPixel(this Bitmap bmp, int firstColors, int xCoordinate = 0)
//         {
//             var firstColor = bmp.GetPixel(xCoordinate, 0);
//             var foundFirstColors = -1;
//             for (var j = 0; j < bmp.Height; j++){
//                 var color = bmp.GetPixel(xCoordinate, j);
//                 if (foundFirstColors + 1 < firstColors){
//                     if (firstColor != color){
//                         firstColor = color;
//                         foundFirstColors++;
//                     }
//                 }
//                 else{
//                     return color;
//                 }
//             }
//             return new Color();
//         }
//         public static Color GetColorVerticallyWithBOrLarger(this Bitmap bmp, int b, int xCoordinate = 0)
//         {
//             for (var j = 0; j < bmp.Height; j++){
//                 var color = bmp.GetPixel(xCoordinate, j);
//                 if (color.B >= b){
//                     return color;
//                 }
//             }
//             return new Color();
//         }
//         /// <summary>
//         /// Finds a color in another image but when the occuranceNumber is not equal to 1, it will look the the n-th occurance that is not connected to the previous occurance.
//         /// Which means that if an occurance is next to another occurance, it will count it as 1 occurance.
//         /// </summary>
//         /// <param name="originalBitmap"></param>
//         /// <param name="searchingForColor"></param>
//         /// <param name="xCoordinate"></param>
//         /// <param name="occuranceNumber"></param>
//         /// <returns></returns>
//         public static int FoundNotConnectedVerticallyOccurance(this Bitmap originalBitmap, Color searchingForColor, int xCoordinate = 0, int occuranceNumber = 1)
//         {
//             var foundOccurance = 0;
//             var lastYOccurance = -2;
//             for (var j = 0; j < originalBitmap.Height; j++){
//                 if (originalBitmap.GetPixel(xCoordinate, j) != searchingForColor){
//                     if (foundOccurance == occuranceNumber){
//                         return j;
//                     }
//                     continue;
//                 }
//                 if (lastYOccurance + 1 >= j) continue;
//                 foundOccurance++;
//                 lastYOccurance = j;
//             }
//             return -1;
//         }
//         public static bool CheckIfImagesAreTheSame(this Bitmap img1, Bitmap img2)
//         {
//             string img1_ref, img2_ref;
//             if (img1.Width == img2.Width && img1.Height == img2.Height)
//             {
//                 for (var i = 0; i < img1.Width; i++)
//                 {
//                     for (var j = 0; j < img1.Height; j++)
//                     {
//                         img1_ref = img1.GetPixel(i, j).ToString();
//                         img2_ref = img2.GetPixel(i, j).ToString();
//                         if (img1_ref != img2_ref)
//                         {
//                             return false;
//                         }
//                     }
//                 }
//             }
//             else return false;
//             return true;
//         }
//         public static Bitmap CropByColor(this Bitmap bitmap, Color color)
//         {
//             return CropByColor(bitmap, new Color[] { color });
//         }
//         public static Bitmap CropByColorReverse(this Bitmap bitmap, Color color)
//         {
//             return CropByColorReverse(bitmap, new Color[] { color });
//         }
//         public static Bitmap CropByColor(this Bitmap bitmap, Color[] colors)
//         {
//             int left = -1;
//             int right = -1;
//             int top = -1;
//             int bottom = -1;
//             int modus = 10;
//             for (int i = 0; i < bitmap.Width; i++)
//             {
//                 for (int j = 0; j < bitmap.Height; j++)
//                 {
//                     var tempColor = bitmap.GetPixel(i, j);
//                     for (int k = 0; k < colors.Length; k++)
//                     {
//                         Color color = colors[k];
//                         if (tempColor.R > color.R - modus && tempColor.R < color.R + modus &&
//                         tempColor.G > color.G - modus && tempColor.G < color.G + modus &&
//                         tempColor.B > color.B - modus && tempColor.B < color.B + modus)
//                         {
//                             if (right < i)
//                             {
//                                 right = i;
//                                 if (left < 0)
//                                 {
//                                     left = i;
//                                 }
//                             }
//                             if (left > i)
//                             {
//                                 left = i;
//                             }
//                             if (bottom < j)
//                             {
//                                 bottom = j;
//                                 if (top < 0)
//                                 {
//                                     top = i;
//                                 }
//                             }
//                             if (top > j)
//                             {
//                                 top = j;
//                             }
//                         }
//                     }
//                 }
//             }
//             if (right - left > 0 && bottom - top > 0)
//             {
//                 var rect = new Rectangle(left /*- (left > 0 ? 1 : 0)*/, top, right - left + 2, bottom - top + 1);
//                 //var bitmaprect = new Rectangle(0,0,bitmap.Width,bitmap.Height);
//                 if (rect.Width > bitmap.Width)
//                 {
//                     rect.Width = bitmap.Width;
//                 }
//                 if (rect.Height > bitmap.Height)
//                 {
//                     rect.Height = bitmap.Height;
//                 }
//                 if (rect.Width + rect.X > bitmap.Width)
//                 {
//                     rect.Width = bitmap.Width - rect.X;
//                 }
//                 if (rect.Height + rect.Y > bitmap.Height)
//                 {
//                     rect.Height = bitmap.Height - rect.Y;
//                 }
//                 return bitmap.Clone(rect, bitmap.PixelFormat);
//             }
//             return bitmap;
//         }
//         public static Bitmap CropByColorB(this Bitmap bitmap, int b)
//         {
//             int left = -1;
//             int right = -1;
//             int top = -1;
//             int bottom = -1;
//             bool foundNonEqualInHeight = false;
//             bool foundNonEqualInWidth = false;
//             for (int i = 0; i < bitmap.Width; i++)
//             {
//                 for (int j = 0; j < bitmap.Height; j++)
//                 {
//                     var tempColor = bitmap.GetPixel(i, j);
//                     if (tempColor.B == b)
//                     {
//                         if (!foundNonEqualInWidth)
//                         {
//                             left = i+1;
//                         }
//                         else if (right < 0) {
//                             right = i;
//                         }
//                         if (!foundNonEqualInHeight)
//                         {
//                             top = j+1;
//                         }
//                         else if (bottom < 0) {
//                             bottom = j;
//                         }
//                     }
//                     else
//                     {
//                         if (top > -1){
//                             foundNonEqualInHeight = true;
//                         }
//                         if (left > -1){
//                             foundNonEqualInWidth = true;
//                         }
//                     }
//                 }
//             }
//             var rect = new Rectangle(left, top, right - left, bottom - top);
//             //var bitmaprect = new Rectangle(0,0,bitmap.Width,bitmap.Height);
//             if (rect.Width > bitmap.Width || rect.Width <= 0)
//             {
//                 rect.Width = bitmap.Width;
//             }
//             if (rect.Height > bitmap.Height || rect.Height <= 0)
//             {
//                 rect.Height = bitmap.Height;
//             }
//             if (rect.Width + rect.X > bitmap.Width)
//             {
//                 rect.Width = bitmap.Width - rect.X;
//             }
//             if (rect.Height + rect.Y > bitmap.Height)
//             {
//                 rect.Height = bitmap.Height - rect.Y;
//             }
//             return bitmap.Clone(rect, bitmap.PixelFormat);
//         }
//         public static Bitmap CropByColorA(this Bitmap bitmap, int a)
//         {
//             int left = -1;
//             int right = -1;
//             int top = -1;
//             int bottom = -1;
//             bool foundNonEqualInHeight = false;
//             bool foundNonEqualInWidth = false;
//             for (int i = 0; i < bitmap.Width; i++)
//             {
//                 for (int j = 0; j < bitmap.Height; j++)
//                 {
//                     var tempColor = bitmap.GetPixel(i, j);
//                     if (tempColor.A == a)
//                     {
//                         if (!foundNonEqualInWidth)
//                         {
//                             left = i+1;
//                         }
//                         else if (right < 0) {
//                             right = i;
//                         }
//                         if (!foundNonEqualInHeight)
//                         {
//                             top = j+1;
//                         }
//                         else if (bottom < 0) {
//                             bottom = j;
//                         }
//                     }
//                     else
//                     {
//                         if (top > -1){
//                             foundNonEqualInHeight = true;
//                         }
//                         if (left > -1){
//                             foundNonEqualInWidth = true;
//                         }
//                     }
//                 }
//             }
//             var rect = new Rectangle(left, top, right - left, bottom - top);
//             //var bitmaprect = new Rectangle(0,0,bitmap.Width,bitmap.Height);
//             if (rect.Width > bitmap.Width || rect.Width <= 0)
//             {
//                 rect.Width = bitmap.Width;
//             }
//             if (rect.Height > bitmap.Height || rect.Height <= 0)
//             {
//                 rect.Height = bitmap.Height;
//             }
//             if (rect.Width + rect.X > bitmap.Width)
//             {
//                 rect.Width = bitmap.Width - rect.X;
//             }
//             if (rect.Height + rect.Y > bitmap.Height)
//             {
//                 rect.Height = bitmap.Height - rect.Y;
//             }
//             return bitmap.Clone(rect, bitmap.PixelFormat);
//         }
//         public static Bitmap CropByColorReverse(this Bitmap bitmap, Color[] colors)
//         {
//             int left = -1;
//             int right = 0;
//             int modus = 10;
//             for (int i = 0; i < bitmap.Width; i++)
//             {
//                 for (int j = 0; j < bitmap.Height; j++)
//                 {
//                     var tempColor = bitmap.GetPixel(i, j);
//                     for (int k = 0; k < colors.Length; k++)
//                     {
//                         Color color = colors[k];
//                         if (tempColor.R < color.R - modus || tempColor.R > color.R + modus &&
//                         tempColor.G < color.G - modus || tempColor.G > color.G + modus &&
//                         tempColor.B < color.B - modus || tempColor.B > color.B + modus)
//                         {
//                             if (left < 0)
//                             {
//                                 left = i;
//                             }
//                             right = i;
//                             break;
//                         }
//                     }
//                 }
//             }
//
//             int top = -1;
//             int bottom = 0;
//             for (int i = 0; i < bitmap.Height; i++)
//             {
//                 for (int j = 0; j < bitmap.Width; j++)
//                 {
//                     var tempColor = bitmap.GetPixel(j, i);
//                     for (int k = 0; k < colors.Length; k++)
//                     {
//                         Color color = colors[k];
//                         if (tempColor.R < color.R - modus || tempColor.R > color.R + modus &&
//                         tempColor.G < color.G - modus || tempColor.G > color.G + modus &&
//                         tempColor.B < color.B - modus || tempColor.B > color.B + modus)
//                         {
//                             if (top < 0)
//                             {
//                                 top = i;
//                             }
//                             bottom = i;
//                             break;
//                         }
//                     }
//                 }
//             }
//
//             var rect = new Rectangle(left, top, right - left + 1, bottom - top + 1);
//             if (rect.Width == 0 || rect.Height == 0 || top < 0 || bottom < 0)
//             {
//                 return bitmap;
//             }
//
//             return bitmap.Clone(rect, bitmap.PixelFormat);
//         }
//
//         public static Bitmap RemoveOtherColors(this Bitmap bitmap, Color[] colors)
//         {
//             int modus = 4;
//             Bitmap newBitmap = new Bitmap(bitmap.Width, bitmap.Height);
//             using (Graphics g = Graphics.FromImage(newBitmap))
//             {
//                 for (int i = 0; i < bitmap.Width; i++)
//                 {
//                     for (int j = 0; j < bitmap.Height; j++)
//                     {
//                         var tempColor = bitmap.GetPixel(i, j);
//                         bool trueOnce = false;
//                         for (int k = 0; k < colors.Length && !trueOnce; k++)
//                         {
//                             Color color = colors[k];
//                             if (tempColor.R > color.R - modus && tempColor.R < color.R + modus &&
//                         tempColor.G > color.G - modus && tempColor.G < color.G + modus &&
//                         tempColor.B > color.B - modus && tempColor.B < color.B + modus)
//                             {
//                                 trueOnce = true;
//                             }
//                         }
//                         if (trueOnce)
//                         {
//                             var point = new System.Drawing.Point(i, j);
//                             var rect = new Rectangle(point, new System.Drawing.Size(1, 1));
//                             g.DrawImage(bitmap.Clone(rect, bitmap.PixelFormat), point);
//                         }
//                     }
//                 }
//             }
//             return newBitmap;
//         }
//
//
//         public static Bitmap ScaleBitmap(this Bitmap bitmap, int width, int height)
//         {
//             return new Bitmap((System.Drawing.Image)bitmap, new System.Drawing.Size(width, height));
//         }
//         public static Bitmap ResizeBitmap(this Bitmap bitmap, int width, int height, bool center = false)
//         {
//             Bitmap newBitmap = new Bitmap(width, height);
//             Graphics graphics = Graphics.FromImage(newBitmap);
//             if (center)
//             {
//                 int diff = width - bitmap.Width;
//                 int half = diff / 2;
//                 int left = diff - half;
//                 graphics.DrawImage(bitmap, left, 0);
//             }
//             else
//             {
//                 graphics.DrawImage(bitmap, 0, 0);
//             }
//             return newBitmap;
//         }
//
//
//         public static Color SetColorContrast(Color color, int threshold)
//         {
//             var contrast = Math.Pow((100.0 + threshold) / 100.0, 2);
//
//             var oldColor = color;
//             var red = ((((oldColor.R / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
//             var green = ((((oldColor.G / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
//             var blue = ((((oldColor.B / 255.0) - 0.5) * contrast) + 0.5) * 255.0;
//             if (red > 255)
//                 red = 255;
//             if (red < 0)
//                 red = 0;
//             if (green > 255)
//                 green = 255;
//             if (green < 0)
//                 green = 0;
//             if (blue > 255)
//                 blue = 255;
//             if (blue < 0)
//                 blue = 0;
//
//             return Color.FromArgb(oldColor.A, (int)red, (int)green, (int)blue);
//         }
//         public static Bitmap SetContrast(this Bitmap bmp, int threshold)
//         {
//             var lockedBitmap = new Bitmap(bmp);
//
//             for (int y = 0; y < lockedBitmap.Height; y++)
//             {
//                 for (int x = 0; x < lockedBitmap.Width; x++)
//                 {
//                     var oldColor = lockedBitmap.GetPixel(x, y);
//
//                     var newColor = SetColorContrast(oldColor, threshold);
//                     lockedBitmap.SetPixel(x, y, newColor);
//                 }
//             }
//             return lockedBitmap;
//         }
//         public static Bitmap SetRGBValues(this Bitmap bmp, int r = -1, int g = -1, int b = -1)
//         {
//             var lockedBitmap = new Bitmap(bmp);
//
//             for (int y = 0; y < lockedBitmap.Height; y++)
//             {
//                 for (int x = 0; x < lockedBitmap.Width; x++)
//                 {
//                     var oldColor = lockedBitmap.GetPixel(x, y);
//
//                     var newColor = Color.FromArgb(oldColor.A, r < 0 ? oldColor.R : r, g < 0 ? oldColor.G : g, b < 0 ? oldColor.B : b);
//                     lockedBitmap.SetPixel(x, y, newColor);
//                 }
//             }
//             return lockedBitmap;
//         }
//         public static Bitmap SetInvertColors(this Bitmap bmp)
//         {
//             var lockedBitmap = new Bitmap(bmp);
//
//             for (int y = 0; y < lockedBitmap.Height; y++)
//             {
//                 for (int x = 0; x < lockedBitmap.Width; x++)
//                 {
//                     var oldColor = lockedBitmap.GetPixel(x, y);
//
//                     var newColor = Color.FromArgb(oldColor.A, 255-oldColor.G, 255-oldColor.G, 255-oldColor.G);
//                     lockedBitmap.SetPixel(x, y, newColor);
//                 }
//             }
//             return lockedBitmap;
//         }
//         public static Bitmap SetDividedValue(this Bitmap bmp, int value)
//         {
//             var lockedBitmap = new Bitmap(bmp);
//
//             for (int y = 0; y < lockedBitmap.Height; y++)
//             {
//                 for (int x = 0; x < lockedBitmap.Width; x++)
//                 {
//                     var oldColor = lockedBitmap.GetPixel(x, y);
//
//                     var newColor = Color.FromArgb(oldColor.A, oldColor.G / value, oldColor.G / value, oldColor.G / value);
//                     lockedBitmap.SetPixel(x, y, newColor);
//                 }
//             }
//             return lockedBitmap;
//         }
//         public static Bitmap SetBlackAndWhite(this Bitmap SourceImage)
//         {
//             using (Graphics gr = Graphics.FromImage(SourceImage)) // SourceImage is a Bitmap object
//             {
//                 var gray_matrix = new float[][] {
//                 new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
//                 new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
//                 new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
//                 new float[] { 0,      0,      0,      1, 0 },
//                 new float[] { 0,      0,      0,      0, 1 }
//             };
//
//                 var ia = new System.Drawing.Imaging.ImageAttributes();
//                 ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(gray_matrix));
//                 ia.SetThreshold(0.8f); // Change this threshold as needed
//                 var rc = new Rectangle(0, 0, SourceImage.Width, SourceImage.Height);
//                 gr.DrawImage(SourceImage, rc, 0, 0, SourceImage.Width, SourceImage.Height, GraphicsUnit.Pixel, ia);
//             }
//             return SourceImage;
//         }
//         public static Bitmap SetBrightness(this Bitmap bmp, float brightness = 1.0f)
//         {
//             Bitmap originalImage = bmp;
//             Bitmap adjustedImage = new Bitmap(originalImage.Width, originalImage.Height);
//             float contrast = 2.0f; // twice the contrast
//             float gamma = 1.0f; // no change in gamma
//
//             float adjustedBrightness = brightness - 1.0f;
//             // create matrix that will brighten and contrast the image
//             float[][] ptsArray ={
//         new float[] {contrast, 0, 0, 0, 0}, // scale red
//         new float[] {0, contrast, 0, 0, 0}, // scale green
//         new float[] {0, 0, contrast, 0, 0}, // scale blue
//         new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
//         new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};
//
//             ImageAttributes imageAttributes = new ImageAttributes();
//             imageAttributes.ClearColorMatrix();
//             imageAttributes.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Bitmap);
//             imageAttributes.SetGamma(gamma, ColorAdjustType.Bitmap);
//             Graphics g = Graphics.FromImage(adjustedImage);
//             g.DrawImage(originalImage, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height)
//                 , 0, 0, originalImage.Width, originalImage.Height,
//                 GraphicsUnit.Pixel, imageAttributes);
//
//             return adjustedImage;
//         }
//         public static Bitmap SetBackground(this Bitmap bmp, Color color)
//         {
//             Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);
//             using (Graphics g = Graphics.FromImage(newBitmap))
//             {
//                 g.Clear(color);
//                 g.DrawImage(bmp, 0, 0);
//             }
//             return newBitmap;
//         }
//
//         public static Bitmap ApplyPostProcessing(this Bitmap bmp)
//         {
//             if (true)
//             {
//                 int margin = 10;
//                 int height = 9;
//                 var img = bmp.ToImageSixLabors();
//                 img = img.ScaleImageHeight(16);
//                 //img = img.SetDividedValue(-2);
//                 img = img.SetContrast(45);
//                 //img = img.SetTimesValue(3);
//                 //img = img.ThickenEdges();
//                 img = img.SetColorsInverted();
//                 img = img.SetRGBValues(r: 0, b: 0);
//                 //img = img.SetContrast(45);
//                 
//                 //img = img.SetBlackAndWhite();
//                 img = img.SetDividedValue(1);
//                 img = img.SetRGBValues(r: 0, b: 0);
//
//                 var newImage = new Image<Rgba32>(img.Width + margin, img.Height + margin);
//                 //newImage.SetBackground(SixLabors.ImageSharp.Color.White);
//                 newImage.SetBackground(new SixLabors.ImageSharp.Color(new Rgb24(0, 255, 0)));
//                 newImage.Mutate(x => x.DrawImage(img, new SixLabors.ImageSharp.Point(margin / 2, margin / 2), 1f));
//
//                 bmp = newImage.ToBitmap();
//                 //img = bmp.ToImageSixLabors();
//
//                 ////end
//                 //bmp = img.ToBitmap();
//             }
//             else
//             {
//                 int scaler = 4;
//                 bmp = bmp.SetBrightness(0.9f);
//                 bmp = bmp.SetContrast(100);
//                 bmp = bmp.SetRGBValues(r: 0, b: 0);
//                 bmp = bmp.SetDividedValue(1);
//                 //bmp = bmp.SetBrightness(-0.1f);
//                 bmp = bmp.SetContrast(100);
//                 //bmp = bmp.SetInvertColors();
//                 bmp = bmp.ScaleBitmap(bmp.Width * scaler, bmp.Height * scaler);
//                 bmp = bmp.SetContrast(100);
//             }
//             return bmp;
//         }
//         public static void MirrorHorizontally(this Bitmap bmp)
//         {
//             bmp.RotateFlip(RotateFlipType.Rotate180FlipY);
//         }
//         public static void MirrorVertically(this Bitmap bmp)
//         {
//             bmp.RotateFlip(RotateFlipType.Rotate180FlipX);
//         }
//     }
// }
