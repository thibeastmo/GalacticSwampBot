using System;
using System.Collections.Generic;
using Image=SixLabors.ImageSharp.Image;
using Color=SixLabors.ImageSharp.Color;
using Rectangle=SixLabors.ImageSharp.Rectangle;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace GalacticSwampBot.Library.Utils {
    public static class ImageHelper {
        public static Image<Rgba32> Cut(this Image<Rgba32> image, Rectangle rect)
        {
            var cloned = image.Clone();
            cloned.Mutate(i => i.Crop(rect));
            return cloned;
        }
        public static Image<Rgba32> CropEz(this Image<Rgba32> image, Rectangle rect)
        {
            image.Mutate(i => i.Resize(image.Width, image.Height).Crop(rect));
            return image;
        }
        public static Image<Rgba32> CropByColor(this Image<Rgba32> bitmap, Color color)
        {
            return CropByColor(bitmap, new Color[] { color });
        }
        public static Image<Rgba32> CropByColorReverse(this Image<Rgba32> bitmap, Color color)
        {
            return CropByColorReverse(bitmap, new Color[] { color });
        }
        public static Image<Rgba32> CropByColor(this Image<Rgba32> image, Color[] colors)
        {
            int left = -1;
            int right = -1;
            int top = -1;
            int bottom = -1;
            int modus = 10;
            image.ProcessPixelRows(accessor => {
                for (int j = 0; j < accessor.Height; j++){
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (int i = 0; i < pixelRow.Length; i++){
                        // Get a reference to the pixel at position x
                        ref Rgba32 tempColor = ref pixelRow[i];
                        for (int k = 0; k < colors.Length; k++){
                            Rgba32 color = colors[k];
                            if (tempColor.R > color.R - modus && tempColor.R < color.R + modus &&
                                tempColor.G > color.G - modus && tempColor.G < color.G + modus &&
                                tempColor.B > color.B - modus && tempColor.B < color.B + modus){
                                if (right < i){
                                    right = i;
                                    if (left < 0){
                                        left = i;
                                    }
                                }
                                if (left > i){
                                    left = i;
                                }
                                if (bottom < j){
                                    bottom = j;
                                    if (top < 0){
                                        top = i;
                                    }
                                }
                                if (top > j){
                                    top = j;
                                }
                            }
                        }
                    }
                }
            });
            if (right - left > 0 && bottom - top > 0){
                var rect = new Rectangle(left/*- (left > 0 ? 1 : 0)*/, top, right - left + 2, bottom - top + 1);
                //var bitmaprect = new Rectangle(0,0,bitmap.Width,bitmap.Height);
                if (rect.Width > image.Width){
                    rect.Width = image.Width;
                }
                if (rect.Height > image.Height){
                    rect.Height = image.Height;
                }
                if (rect.Width + rect.X > image.Width){
                    rect.Width = image.Width - rect.X;
                }
                if (rect.Height + rect.Y > image.Height){
                    rect.Height = image.Height - rect.Y;
                }
                image.CropEz(rect);
            }
            return image;
        }
        public static Image<Rgba32> CropByColorReverse(this Image<Rgba32> bitmap, Color[] colors)
        {
            int left = -1;
            int right = 0;
            int modus = 10;
            Image<Rgba32> image = Image.Load<Rgba32>("my_file.png");
            image.ProcessPixelRows(accessor => {
                // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                Rgba32 transparent = Color.Transparent;

                for (int j = 0; j < accessor.Height; j++){
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);

                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (int i = 0; i < pixelRow.Length; i++){
                        // Get a reference to the pixel at position x
                        ref Rgba32 tempColor = ref pixelRow[i];

                        for (int k = 0; k < colors.Length; k++){
                            Rgba32 color = colors[k];
                            if (tempColor.R < color.R - modus || tempColor.R > color.R + modus &&
                                tempColor.G < color.G - modus || tempColor.G > color.G + modus &&
                                tempColor.B < color.B - modus || tempColor.B > color.B + modus){
                                if (left < 0){
                                    left = i;
                                }
                                right = i;
                                break;
                            }
                        }
                    }
                }
            });

            int top = -1;
            int bottom = 0;
            image.ProcessPixelRows(accessor => {
                // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                Rgba32 transparent = Color.Transparent;

                for (int j = 0; j < accessor.Height; j++){
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);

                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (int i = 0; i < pixelRow.Length; i++){
                        // Get a reference to the pixel at position x
                        ref Rgba32 tempColor = ref pixelRow[i];

                        for (int k = 0; k < colors.Length; k++){
                            Rgba32 color = colors[k];
                            if (tempColor.R < color.R - modus || tempColor.R > color.R + modus &&
                                tempColor.G < color.G - modus || tempColor.G > color.G + modus &&
                                tempColor.B < color.B - modus || tempColor.B > color.B + modus){
                                if (top < 0){
                                    top = i;
                                }
                                bottom = i;
                                break;
                            }
                        }
                    }
                }
            });

            var rect = new Rectangle(left, top, right - left + 1, bottom - top + 1);
            if (rect.Width == 0 || rect.Height == 0 || top < 0 || bottom < 0){
                return bitmap;
            }

            bitmap.CropEz(rect);
            return bitmap;
        }


        public static Image<Rgba32> CropByG(this Image<Rgba32> image, int g)
        {
            var left = -1;
            var right = -1;
            var top = -1;
            var bottom = -1;
            image.ProcessPixelRows(accessor => {
                for (var j = 0; j < accessor.Height; j++){
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (var i = 0; i < pixelRow.Length; i++){
                        // Get a reference to the pixel at position x
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.G > g){
                            if (right < i){
                                right = i;
                                if (left < 0){
                                    left = i;
                                }
                            }
                            if (left > i){
                                left = i;
                            }
                            if (bottom < j){
                                bottom = j;
                                if (top < 0){
                                    top = i;
                                }
                            }
                            if (top > j){
                                top = j;
                            }
                        }
                    }
                }
            });
            if (right - left > 0 && bottom - top > 0){
                var rect = new Rectangle(left/*- (left > 0 ? 1 : 0)*/, top, right - left + 2, bottom - top + 1);
                //var bitmaprect = new Rectangle(0,0,bitmap.Width,bitmap.Height);
                if (rect.Width > image.Width){
                    rect.Width = image.Width;
                }
                if (rect.Height > image.Height){
                    rect.Height = image.Height;
                }
                if (rect.Width + rect.X > image.Width){
                    rect.Width = image.Width - rect.X;
                }
                if (rect.Height + rect.Y > image.Height){
                    rect.Height = image.Height - rect.Y;
                }
                image.CropEz(rect);
            }
            return image;
        }
        public static Image<Rgba32> CropByB(this Image<Rgba32> image, int b)
        {
            var left = -1;
            var right = -1;
            var top = -1;
            var bottom = -1;
            var leftFound = false;
            var topFound = false;
            image.ProcessPixelRows(accessor => {
                //check height
                for (var j = 0; j < accessor.Height; j++){
                    var anyLessThanG = false;
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    for (var i = 0; i < pixelRow.Length && !anyLessThanG; i++){
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.B > b && tempColor.A != 0){
                            anyLessThanG = true;
                        }
                    }
                    if (anyLessThanG){
                        if (!topFound){
                            topFound = true;
                            top = j;
                        }
                        else{
                            bottom = j;
                        }
                    }
                }
                //check width
                for (var i = 0; i < accessor.Width; i++){
                    var anyLessThanG = false;
                    for (var j = 0; j < accessor.Height && !anyLessThanG; j++){
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.B > b && tempColor.A != 0){
                            anyLessThanG = true;
                        }
                    }
                    if (anyLessThanG){
                        if (!leftFound){
                            leftFound = true;
                            left = i;
                        }
                        else{
                            right = i;
                        }
                    }
                }
            });
            if (right - left > 0 && bottom - top > 0){
                var rect = new Rectangle(left/*- (left > 0 ? 1 : 0)*/, top, right - left + 2, bottom - top + 1);
                //var bitmaprect = new Rectangle(0,0,bitmap.Width,bitmap.Height);
                if (rect.Width > image.Width){
                    rect.Width = image.Width;
                }
                if (rect.Height > image.Height){
                    rect.Height = image.Height;
                }
                if (rect.Width + rect.X > image.Width){
                    rect.Width = image.Width - rect.X;
                }
                if (rect.Height + rect.Y > image.Height){
                    rect.Height = image.Height - rect.Y;
                }
                image.CropEz(rect);
            }
            return image;
        }
        public static Image<Rgba32> CropByRBB(this Image<Rgba32> image, int value)
        {
            var left = -1;
            var right = -1;
            var top = -1;
            var bottom = -1;
            var leftFound = false;
            var topFound = false;
            image.ProcessPixelRows(accessor => {
                //check height
                for (var j = 0; j < accessor.Height; j++){
                    var anyLessThanG = false;
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    for (var i = 0; i < pixelRow.Length && !anyLessThanG; i++){
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.B > value && tempColor.R > value && tempColor.G > value && tempColor.A != 0){
                            anyLessThanG = true;
                        }
                    }
                    if (anyLessThanG){
                        if (!topFound){
                            topFound = true;
                            top = j;
                        }
                        else{
                            bottom = j;
                        }
                    }
                }
                //check width
                for (var i = 0; i < accessor.Width; i++){
                    var anyLessThanG = false;
                    for (var j = 0; j < accessor.Height && !anyLessThanG; j++){
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.B > value && tempColor.R > value && tempColor.G > value && tempColor.A != 0){
                            anyLessThanG = true;
                        }
                    }
                    if (anyLessThanG){
                        if (!leftFound){
                            leftFound = true;
                            left = i;
                        }
                        else{
                            right = i;
                        }
                    }
                }
            });
            if (right - left > 0 && bottom - top > 0){
                var rect = new Rectangle(left/*- (left > 0 ? 1 : 0)*/, top, right - left + 2, bottom - top + 1);
                //var bitmaprect = new Rectangle(0,0,bitmap.Width,bitmap.Height);
                if (rect.Width > image.Width){
                    rect.Width = image.Width;
                }
                if (rect.Height > image.Height){
                    rect.Height = image.Height;
                }
                if (rect.Width + rect.X > image.Width){
                    rect.Width = image.Width - rect.X;
                }
                if (rect.Height + rect.Y > image.Height){
                    rect.Height = image.Height - rect.Y;
                }
                image.CropEz(rect);
            }
            return image;
        }
        public static Image<Rgba32> CropByGReversed(this Image<Rgba32> image, int g, int modus = 0)
        {
            var left = -1;
            var right = -1;
            var top = -1;
            var bottom = -1;
            var leftFound = false;
            var topFound = false;
            image.ProcessPixelRows(accessor => {
                //check height
                for (var j = 0; j < accessor.Height; j++){
                    var anyLessThanG = false;
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    for (var i = 0; i < pixelRow.Length && !anyLessThanG; i++){
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.G + modus < g && tempColor.A != 0){
                            anyLessThanG = true;
                        }
                    }
                    if (anyLessThanG){
                        if (!topFound){
                            topFound = true;
                            top = j;
                        }
                        else{
                            bottom = j;
                        }
                    }
                }
                //check width
                for (var i = 0; i < accessor.Width; i++){
                    var anyLessThanG = false;
                    for (var j = 0; j < accessor.Height && !anyLessThanG; j++){
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.G + modus < g && tempColor.A != 0){
                            anyLessThanG = true;
                        }
                    }
                    if (anyLessThanG){
                        if (!leftFound){
                            leftFound = true;
                            left = i;
                        }
                        else{
                            right = i;
                        }
                    }
                }
            });
            if (right - left > 0 && bottom - top > 0){
                var rect = new Rectangle(left/*- (left > 0 ? 1 : 0)*/, top, right - left + 1, bottom - top + 1);
                if (rect.Width > image.Width){
                    rect.Width = image.Width;
                }
                if (rect.Height > image.Height){
                    rect.Height = image.Height;
                }
                if (rect.Width + rect.X > image.Width){
                    rect.Width = image.Width - rect.X;
                }
                if (rect.Height + rect.Y > image.Height){
                    rect.Height = image.Height - rect.Y;
                }
                image.CropEz(rect);
            }
            return image;
        }
        public static Image<Rgba32> CropByA(this Image<Rgba32> image, int a)
        {
            var left = -1;
            var right = -1;
            var top = -1;
            var bottom = -1;
            image.ProcessPixelRows(accessor => {
                for (var j = 0; j < accessor.Height; j++){
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (var i = 0; i < pixelRow.Length; i++){
                        // Get a reference to the pixel at position x
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.A > a){
                            if (right < i){
                                right = i;
                                if (left < 0){
                                    left = i;
                                }
                            }
                            if (left > i){
                                left = i;
                            }
                            if (bottom < j){
                                bottom = j;
                                if (top < 0){
                                    top = i;
                                }
                            }
                            if (top > j){
                                top = j;
                            }
                        }
                    }
                }
            });
            if (right - left > 0 && bottom - top > 0){
                var rect = new Rectangle(left/*- (left > 0 ? 1 : 0)*/, top, right - left + 2, bottom - top + 1);
                //var bitmaprect = new Rectangle(0,0,bitmap.Width,bitmap.Height);
                if (rect.Width > image.Width){
                    rect.Width = image.Width;
                }
                if (rect.Height > image.Height){
                    rect.Height = image.Height;
                }
                if (rect.Width + rect.X > image.Width){
                    rect.Width = image.Width - rect.X;
                }
                if (rect.Height + rect.Y > image.Height){
                    rect.Height = image.Height - rect.Y;
                }
                image.CropEz(rect);
            }
            return image;
        }
        public static Image<Rgba32> CropByLeftSplitColor(this Image<Rgba32> image, Rgba32 color)
        {
            var left = -1;
            var right = -1;
            image.ProcessPixelRows(accessor => {
                bool firstFullColumnFound = false;
                for (var i = 0; i < accessor.Width && !firstFullColumnFound; i++){
                    var allTheSame = true;
                    for (var j = 0; j < accessor.Height; j++){
                        Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                        // Get a reference to the pixel at position x
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor != color){
                            allTheSame = false;
                            break;
                        }
                    }
                    if (allTheSame){
                        firstFullColumnFound = true;
                        left = i;
                        right = image.Width - 1;
                    }
                }
            });
            if (right - left > 0){
                var rect = new Rectangle(left, 0, right - left + 2, image.Height);
                if (rect.Width > image.Width){
                    rect.Width = image.Width;
                }
                if (rect.Height > image.Height){
                    rect.Height = image.Height;
                }
                if (rect.Width + rect.X > image.Width){
                    rect.Width = image.Width - rect.X;
                }
                if (rect.Height + rect.Y > image.Height){
                    rect.Height = image.Height - rect.Y;
                }
                image.CropEz(rect);
            }
            return image;
        }

        public static Image RemoveOtherColors(this Image<Rgba32> bitmap, Color[] colors, Color? changeOtherColorsTo = null)
        {
            int modus = 4;
            Image<Rgba32> image = Image.Load<Rgba32>("my_file.png");
            image.ProcessPixelRows(accessor => {
                // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                Rgba32 transparent = Color.Transparent;

                for (int j = 0; j < accessor.Height; j++){
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);

                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (int i = 0; i < pixelRow.Length; i++){
                        // Get a reference to the pixel at position x
                        ref Rgba32 tempColor = ref pixelRow[i];
                        bool trueOnce = false;
                        for (int k = 0; k < colors.Length && !trueOnce; k++){
                            Rgba32 color = colors[k];
                            if (tempColor.R > color.R - modus && tempColor.R < color.R + modus &&
                                tempColor.G > color.G - modus && tempColor.G < color.G + modus &&
                                tempColor.B > color.B - modus && tempColor.B < color.B + modus){
                                trueOnce = true;
                            }
                        }
                        if (trueOnce){
                            if (changeOtherColorsTo.HasValue){
                                pixelRow[i] = changeOtherColorsTo.Value;
                            }
                        }
                    }
                }
            });
            return bitmap;
        }

        public static bool ContainsBlack(this Image<Rgba32> image)
        {
            var containsBlack = false;
            image.ProcessPixelRows(accessor => {
                for (var j = 0; j < accessor.Height && !containsBlack; j++){
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (var i = 0; i < pixelRow.Length && !containsBlack; i++){
                        // Get a reference to the pixel at position x
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.B == 0 && tempColor.R == 0 && tempColor.G == 0){
                            containsBlack = true;
                        }
                    }
                }
            });
            return containsBlack;
        }

        public static Image<Rgba32> IncreaseG(this Image<Rgba32> image, int gExtra)
        {
            var extra = (byte)gExtra;
            image.ProcessPixelRows(accessor => {
                for (var j = 0; j < accessor.Height; j++){
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (var i = 0; i < pixelRow.Length; i++){
                        // Get a reference to the pixel at position x
                        ref Rgba32 tempColor = ref pixelRow[i];
                        tempColor.G += 255 > tempColor.G + extra ? extra : (byte)(255 - tempColor.G);
                    }
                }
            });
            return image;
        }
        public static Image<Rgba32> DecreaseG(this Image<Rgba32> image, int gLess, int discardValue = 256)
        {
            var less = (byte)gLess;
            image.ProcessPixelRows(accessor => {
                for (var j = 0; j < accessor.Height; j++){
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    // pixelRow.Length has the same value as accessor.Width,
                    // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                    for (var i = 0; i < pixelRow.Length; i++){
                        // Get a reference to the pixel at position x
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.G < discardValue){
                            tempColor.G -= 0 < tempColor.G - less ? less : tempColor.G;
                        }
                    }
                }
            });
            return image;
        }
        public static Image SetRGBValues(this Image img, int r = -1, int g = -1, int b = -1)
        {
            if (img is Image<Rgb24>){
                ((Image<Rgb24>)img).ProcessPixelRows(accessor => {
                    // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                    Rgba32 transparent = Color.Transparent;

                    for (int j = 0; j < accessor.Height; j++){
                        var pixelRow = accessor.GetRowSpan(j);

                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int i = 0; i < pixelRow.Length; i++){
                            // Get a reference to the pixel at position x
                            ref Rgb24 tempColor = ref pixelRow[i];
                            pixelRow[i] = new Rgb24(
                            r < 0 ? (byte)tempColor.R : (byte)r,
                            g < 0 ? (byte)tempColor.G : (byte)g,
                            b < 0 ? (byte)tempColor.B : (byte)b);
                        }
                    }
                });
            }
            else{
                ((Image<Rgba32>)img).ProcessPixelRows(accessor => {
                    // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                    Rgba32 transparent = Color.Transparent;

                    for (int j = 0; j < accessor.Height; j++){
                        var pixelRow = accessor.GetRowSpan(j);

                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int i = 0; i < pixelRow.Length; i++){
                            // Get a reference to the pixel at position x
                            ref Rgba32 tempColor = ref pixelRow[i];
                            pixelRow[i] = new Rgba32(
                            r < 0 ? (byte)tempColor.R : (byte)r,
                            g < 0 ? (byte)tempColor.G : (byte)g,
                            b < 0 ? (byte)tempColor.B : (byte)b,
                            tempColor.A);
                        }
                    }
                });
            }
            return img;
        }
        public static Image<Rgba32>[] SeparateCharacters(this Image<Rgba32> img, int r = 256, int g = 256, int b = 256)
        {
            var characters = new List<Image<Rgba32>>();
            var lastSeparationPoint = 0;
            img.ProcessPixelRows(accessor => {
                for (var i = 0; i < accessor.Width; i++){
                    var separateCharacter = true;
                    for (var j = 0; j < accessor.Height && separateCharacter; j++){
                        var pixelRow = accessor.GetRowSpan(j);
                        ref Rgba32 tempColor = ref pixelRow[i];
                        if (tempColor.R < r && tempColor.G < g && tempColor.B < b){
                            separateCharacter = false;
                        }
                    }
                    if (separateCharacter){
                        if (i - lastSeparationPoint > 2){
                            characters.Add(img.Cut(new Rectangle(lastSeparationPoint, 0, i - lastSeparationPoint, img.Height)));
                        }
                        lastSeparationPoint = i;
                    }
                }
                if (lastSeparationPoint + 1 < accessor.Width){
                    characters.Add(img.Cut(new Rectangle(lastSeparationPoint, 0, accessor.Width - lastSeparationPoint, img.Height)));
                }
            });
            return characters.ToArray();
        }
        public static Image SetTimesValue(this Image img, int value)
        {
            if (img is Image<Rgb24>){
                ((Image<Rgb24>)img).ProcessPixelRows(accessor => {
                    // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                    Rgba32 transparent = Color.Transparent;

                    for (int j = 0; j < accessor.Height; j++){
                        var pixelRow = accessor.GetRowSpan(j);

                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int i = 0; i < pixelRow.Length; i++){
                            // Get a reference to the pixel at position x
                            ref Rgb24 tempColor = ref pixelRow[i];
                            pixelRow[i] = new Rgb24(
                            (byte)(tempColor.R * value),
                            (byte)(tempColor.G * value),
                            (byte)(tempColor.B * value));
                        }
                    }
                });
            }
            else{
                ((Image<Rgba32>)img).ProcessPixelRows(accessor => {
                    // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                    Rgba32 transparent = Color.Transparent;

                    for (int j = 0; j < accessor.Height; j++){
                        var pixelRow = accessor.GetRowSpan(j);

                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int i = 0; i < pixelRow.Length; i++){
                            // Get a reference to the pixel at position x
                            ref Rgba32 tempColor = ref pixelRow[i];
                            pixelRow[i] = new Rgba32(
                            (byte)(tempColor.R * value),
                            (byte)(tempColor.G * value),
                            (byte)(tempColor.B * value),
                            tempColor.A);
                        }
                    }
                });
            }
            return img;
        }
        public static Image SetDividedValue(this Image img, int value)
        {
            if (img is Image<Rgb24>){
                ((Image<Rgb24>)img).ProcessPixelRows(accessor => {
                    // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                    Rgba32 transparent = Color.Transparent;

                    for (int j = 0; j < accessor.Height; j++){
                        var pixelRow = accessor.GetRowSpan(j);

                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int i = 0; i < pixelRow.Length; i++){
                            // Get a reference to the pixel at position x
                            ref Rgb24 tempColor = ref pixelRow[i];
                            pixelRow[i] = new Rgb24(
                            (byte)(tempColor.R / value),
                            (byte)(tempColor.G / value),
                            (byte)(tempColor.B / value));
                        }
                    }
                });
            }
            else{
                ((Image<Rgba32>)img).ProcessPixelRows(accessor => {
                    // Color is pixel-agnostic, but it's implicitly convertible to the Rgba32 pixel type
                    Rgba32 transparent = Color.Transparent;

                    for (int j = 0; j < accessor.Height; j++){
                        var pixelRow = accessor.GetRowSpan(j);

                        // pixelRow.Length has the same value as accessor.Width,
                        // but using pixelRow.Length allows the JIT to optimize away bounds checks:
                        for (int i = 0; i < pixelRow.Length; i++){
                            // Get a reference to the pixel at position x
                            ref Rgba32 tempColor = ref pixelRow[i];
                            pixelRow[i] = new Rgba32(
                            (byte)(tempColor.R / value),
                            (byte)(tempColor.G / value),
                            (byte)(tempColor.B / value),
                            tempColor.A);
                        }
                    }
                });
            }
            return img;
        }

        public static Image<Rgba32> ScaleImage(this Image<Rgba32> bitmap, int width, int height)
        {
            bitmap.Mutate(x => x.Resize(width, height));
            return bitmap;
        }
        public static Image<Rgba32> ScaleImageWidth(this Image<Rgba32> bitmap, int width)
        {
            double scale = (double)width / (double)bitmap.Width;
            bitmap.Mutate(x => x.Resize(width, (int)((double)bitmap.Height * scale)));
            return bitmap;
        }
        public static Image<Rgba32> ScaleImageHeight(this Image<Rgba32> bitmap, int height)
        {
            double scale = (double)height / (double)bitmap.Height;
            bitmap.Mutate(x => x.Resize((int)((double)bitmap.Width * scale), height));
            return bitmap;
        }
        public static Image<Rgba32> ResizeImage(this Image<Rgba32> img, int width, int height)
        {
            var clone = img.Clone();
            img = new Image<Rgba32>(width, height);
            img.DrawImage(clone, new Point());
            return img;
        }

        public static Image<Rgba32> SetContrast(this Image<Rgba32> bmp, int threshold)
        {
            float contrast = threshold;
            if (contrast < -100) contrast = -100;
            if (contrast > 100) contrast = 100;
            contrast = (100.0f + contrast) / 100.0f;
            contrast *= contrast;
            bmp.Mutate(x => x.Contrast(contrast));
            return bmp;
            //Color col;
            //for (int i = 0; i < bmap.Width; i++)
            //{
            //    for (int j = 0; j < bmap.Height; j++)
            //    {
            //        col = bmap.GetPixel(i, j);
            //        double pRed = col.R / 255.0;
            //        pRed -= 0.5;
            //        pRed *= contrast;
            //        pRed += 0.5;
            //        pRed *= 255;
            //        if (pRed < 0) pRed = 0;
            //        if (pRed > 255) pRed = 255;

            //        double pGreen = col.G / 255.0;
            //        pGreen -= 0.5;
            //        pGreen *= contrast;
            //        pGreen += 0.5;
            //        pGreen *= 255;
            //        if (pGreen < 0) pGreen = 0;
            //        if (pGreen > 255) pGreen = 255;

            //        double pBlue = col.B / 255.0;
            //        pBlue -= 0.5;
            //        pBlue *= contrast;
            //        pBlue += 0.5;
            //        pBlue *= 255;
            //        if (pBlue < 0) pBlue = 0;
            //        if (pBlue > 255) pBlue = 255;

            //        bmap.SetPixel(i, j,
            //Color.FromArgb((byte)pRed, (byte)pGreen, (byte)pBlue));
            //    }
            //}
            //return bmap;
        }
        public static Image<Rgba32> SetBlackAndWhite(this Image<Rgba32> image)
        {
            // SourceImage.Mutate(x => x.BlackWhite());
            const int ceil = 3;
            image.ProcessPixelRows(accessor => {
                for (var j = 0; j < accessor.Height; j++){
                    var pixelRow = accessor.GetRowSpan(j);
                    for (var i = 0; i < pixelRow.Length; i++){
                        ref Rgba32 tempColor = ref pixelRow[i];
                        var value = (tempColor.R + tempColor.G + tempColor.B) / ceil;
                        pixelRow[i] = new Rgba32(value, value, value, tempColor.A);
                    }
                }
            });
            return image;
            //using (Graphics gr = Graphics.FromImage(SourceImage)) // SourceImage is a Image object
            //{
            //    var gray_matrix = new float[][] {
            //    new float[] { 0.299f, 0.299f, 0.299f, 0, 0 },
            //    new float[] { 0.587f, 0.587f, 0.587f, 0, 0 },
            //    new float[] { 0.114f, 0.114f, 0.114f, 0, 0 },
            //    new float[] { 0,      0,      0,      1, 0 },
            //    new float[] { 0,      0,      0,      0, 1 }
            //};

            //    var ia = new System.Drawing.Imaging.ImageAttributes();
            //    ia.SetColorMatrix(new System.Drawing.Imaging.ColorMatrix(gray_matrix));
            //    ia.SetThreshold(0.8f); // Change this threshold as needed
            //    var rc = new Rectangle(0, 0, SourceImage.Width, SourceImage.Height);
            //    gr.DrawImage(SourceImage, rc, 0, 0, SourceImage.Width, SourceImage.Height, GraphicsUnit.Pixel, ia);
            //}
            //return SourceImage;
        }
        public static Image<Rgba32> SetBrightness(this Image<Rgba32> bmp, float brightness = 1.0f)
        {
            bmp.Mutate(x => x.Brightness(brightness));
            return bmp;
            //    Image originalImage = bmp;
            //    Image adjustedImage = new Image(originalImage.Width, originalImage.Height);
            //    float contrast = 2.0f; // twice the contrast
            //    float gamma = 1.0f; // no change in gamma

            //    float adjustedBrightness = brightness - 1.0f;
            //    // create matrix that will brighten and contrast the image
            //    float[][] ptsArray ={
            //new float[] {contrast, 0, 0, 0, 0}, // scale red
            //new float[] {0, contrast, 0, 0, 0}, // scale green
            //new float[] {0, 0, contrast, 0, 0}, // scale blue
            //new float[] {0, 0, 0, 1.0f, 0}, // don't scale alpha
            //new float[] {adjustedBrightness, adjustedBrightness, adjustedBrightness, 0, 1}};

            //    ImageAttributes imageAttributes = new ImageAttributes();
            //    imageAttributes.ClearColorMatrix();
            //    imageAttributes.SetColorMatrix(new ColorMatrix(ptsArray), ColorMatrixFlag.Default, ColorAdjustType.Image);
            //    imageAttributes.SetGamma(gamma, ColorAdjustType.Image);
            //    Graphics g = Graphics.FromImage(adjustedImage);
            //    g.DrawImage(originalImage, new Rectangle(0, 0, adjustedImage.Width, adjustedImage.Height)
            //        , 0, 0, originalImage.Width, originalImage.Height,
            //        GraphicsUnit.Pixel, imageAttributes);

            //    return adjustedImage;
        }
        //public static Image SetRGValues(this Image bmp, int amount)
        //{
        //    var lockedImage = new Image(bmp);

        //    for (int y = 0; y < lockedImage.Height; y++)
        //    {
        //        for (int x = 0; x < lockedImage.Width; x++)
        //        {
        //            lockedImage.SetPixel(x, y, new );
        //        }
        //    }
        //    return lockedImage;
        //}
        //public static Image ChangeNonTrasparentPixelsToColor(this Image bmp, Color color)
        //{
        //    var lockedImage = new Image(bmp);

        //    for (int y = 0; y < lockedImage.Height; y++)
        //    {
        //        for (int x = 0; x < lockedImage.Width; x++)
        //        {
        //            lockedImage.SetPixel(x, y, color);
        //        }
        //    }
        //    return lockedImage;
        //}
        public static Image<Rgba32> SetBackground(this Image<Rgba32> img, Color color, int margin = 0)
        {
            var clone = img.Clone();
            img = new Image<Rgba32>(img.Width + margin * 2, img.Height + margin * 2);
            img.Mutate(x => x.BackgroundColor(color));
            img.DrawImage(clone, new Point(margin, margin));
            return img;
        }
        public static Image<Rgba32> CropByFirstXCoordinateOfContainedImage(this Image<Rgba32> bmp, Image containedImage)
        {
            int firstXInBmp = 0;
            int firstYInBmp = 0;
            ((Image<Rgba32>)bmp).ProcessPixelRows(accessor => {
                for (int j = 0; j < accessor.Height && firstYInBmp + containedImage.Height < accessor.Height; j++){
                    Span<Rgba32> pixelRow = accessor.GetRowSpan(j);
                    for (int i = 0; i < pixelRow.Length && firstXInBmp + containedImage.Width < pixelRow.Length; i++){
                        ref Rgba32 tempColor = ref pixelRow[i];
                        int xDiff = i - firstXInBmp;
                        int yDiff = j - firstYInBmp;
                        if (xDiff > -1 && yDiff > -1 && xDiff < containedImage.Width && yDiff < containedImage.Height){
                            var color = ((Image<Rgb24>)containedImage)[xDiff, yDiff];
                            if (tempColor.R == color.R && tempColor.G == color.G && tempColor.B == color.B){
                                if (xDiff == containedImage.Width && yDiff == containedImage.Height){
                                    break;
                                }
                            }
                            else{
                                firstXInBmp = i + (i + 1 < accessor.Width ? 1 : -i);
                                firstYInBmp = j + (i + 1 == accessor.Width && j + 1 < accessor.Height ? 1 : -j);
                            }
                        }
                    }
                }
            });
            bmp.CropEz(new Rectangle(0, 0, firstYInBmp, firstXInBmp));
            return bmp;
        }
        public static Image<Rgba32> ThickenEdges(this Image<Rgba32> bmp)
        {
            bmp.Mutate(x => x.DetectEdges());
            return bmp;
        }
        public static Image<Rgba32> SetColorsInverted(this Image<Rgba32> bmp)
        {
            bmp.Mutate(x => x.Invert());
            return bmp;
            //var lockedImage = new Image(bmp);
            //for (int y = 0; (y <= (bmp.Height - 1)); y++)
            //{
            //    for (int x = 0; (x <= (bmp.Width - 1)); x++)
            //    {
            //        Color inv = bmp.GetPixel(x, y);
            //        inv = Color.FromArgb(255, (255 - inv.R), (255 - inv.G), (255 - inv.B));
            //        lockedImage.SetPixel(x, y, inv);
            //    }
            //}
            //return lockedImage;
        }


        public static Rgba32 PreprocessBackgroundColor = new(0, 255, 0);

        public static Image<Rgba32> ApplyPostProcessing(this Image<Rgba32> img)
        {
            const int margin = 10;
            img = img.ScaleImageHeight(16);
            //img = img.SetDividedValue(-2);
            img = img.SetContrast(45);
            //img = img.SetTimesValue(3);
            //img = img.ThickenEdges();
            img = img.SetColorsInverted();
            img = (Image<Rgba32>)img.SetRGBValues(r: 0, b: 0);
            //img = img.SetContrast(45);

            //img = img.SetBlackAndWhite();
            img = (Image<Rgba32>)img.SetDividedValue(1);
            img = (Image<Rgba32>)img.SetRGBValues(r: 0, b: 0);

            var newImage = new Image<Rgba32>(img.Width + margin, img.Height + margin);
            //newImage.SetBackground(SixLabors.ImageSharp.Color.White);
            newImage.SetBackground(PreprocessBackgroundColor);
            newImage.Mutate(x => x.DrawImage(img, new SixLabors.ImageSharp.Point(margin / 2, margin / 2), 1f));
            return newImage;
        }
        public static Image<Rgba32> Write(this Image<Rgba32> img, string text, Font font, Color color, PointF location)
        {
            img.Mutate(x => x.DrawText(text, font, color, location));
            return img;
        }
        public static Image<Rgba32> DrawImage(this Image<Rgba32> img, Image<Rgba32> layerImage, Point location)
        {
            img.Mutate(x => x.DrawImage(layerImage, location, 1));
            return img;
        }
    }
}
