/* This header copied for license reasons.
 * 
//   Project:           WriteableBitmapEx - WriteableBitmap extensions
//   Description:       Collection of extension methods for the WriteableBitmap class.
//
//   Changed by:        $Author$
//   Changed on:        $Date$
//   Changed in:        $Revision$
//   Project:           $URL$
//   Id:                $Id$
//
//
//   Copyright © 2009-2015 Rene Schulte and WriteableBitmapEx Contributors
//
//   This code is open source. Please read the License.txt for details. No worries, we won't sue you! ;)
*/
using System;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CASimulator
{
    public unsafe static class WriteableBitmapEx
    {
        internal const int SizeOfArgb = 4;

        public static int ConvertColor(double opacity, Color color)
        {
            if (opacity < 0.0 || opacity > 1.0)
            {
                throw new ArgumentOutOfRangeException("opacity", "Opacity must be between 0.0 and 1.0");
            }

            color.A = (byte)(color.A * opacity);

            return ConvertColor(color);
        }

        public static int ConvertColor(Color color)
        {
            var col = 0;

            if (color.A != 0)
            {
                var a = color.A + 1;
                col = (color.A << 24)
                  | ((byte)((color.R * a) >> 8) << 16)
                  | ((byte)((color.G * a) >> 8) << 8)
                  | ((byte)((color.B * a) >> 8));
            }

            return col;
        }

        /// <summary>
        /// Draws a rectangle.
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color.</param>
        public static void DrawRectangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            var col = ConvertColor(color);
            bmp.DrawRectangle(x1, y1, x2, y2, col);
        }

        /// <summary>
        /// Draws a rectangle.
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color.</param>
        public static void DrawRectangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color)
        {
            using (var context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                var w = context.Width;
                var h = context.Height;
                var pixels = context.Pixels;

                // Check boundaries
                if ((x1 < 0 && x2 < 0) || (y1 < 0 && y2 < 0)
                 || (x1 >= w && x2 >= w) || (y1 >= h && y2 >= h))
                {
                    return;
                }

                // Clamp boundaries
                if (x1 < 0) { x1 = 0; }
                if (y1 < 0) { y1 = 0; }
                if (x2 < 0) { x2 = 0; }
                if (y2 < 0) { y2 = 0; }
                if (x1 >= w) { x1 = w - 1; }
                if (y1 >= h) { y1 = h - 1; }
                if (x2 >= w) { x2 = w - 1; }
                if (y2 >= h) { y2 = h - 1; }

                var startY = y1 * w;
                var endY = y2 * w;

                var offset2 = endY + x1;
                var endOffset = startY + x2;
                var startYPlusX1 = startY + x1;

                // top and bottom horizontal scanlines
                for (var x = startYPlusX1; x <= endOffset; x++)
                {
                    pixels[x] = color; // top horizontal line
                    pixels[offset2] = color; // bottom horizontal line
                    offset2++;
                }

                // offset2 == endY + x2

                // vertical scanlines
                endOffset = startYPlusX1 + w;
                offset2 -= w;

                for (var y = startY + x2 + w; y <= offset2; y += w)
                {
                    pixels[y] = color; // right vertical line
                    pixels[endOffset] = color; // left vertical line
                    endOffset += w;
                }
            }
        }

        /// <summary>
        /// Draws a filled rectangle.
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color.</param>
        public static void FillRectangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, Color color)
        {
            var col = ConvertColor(color);
            bmp.FillRectangle(x1, y1, x2, y2, col);
        }

        /// <summary>
        /// Draws a filled rectangle with or without alpha blending (default = false).
        /// x2 has to be greater than x1 and y2 has to be greater than y1.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="x1">The x-coordinate of the bounding rectangle's left side.</param>
        /// <param name="y1">The y-coordinate of the bounding rectangle's top side.</param>
        /// <param name="x2">The x-coordinate of the bounding rectangle's right side.</param>
        /// <param name="y2">The y-coordinate of the bounding rectangle's bottom side.</param>
        /// <param name="color">The color.</param>
        /// <param name="doAlphaBlend">True if alpha blending should be performed or false if not.</param>
        public static void FillRectangle(this WriteableBitmap bmp, int x1, int y1, int x2, int y2, int color, bool doAlphaBlend = false)
        {
            using (var context = bmp.GetBitmapContext())
            {
                // Use refs for faster access (really important!) speeds up a lot!
                var w = context.Width;
                var h = context.Height;

                int sa = ((color >> 24) & 0xff);
                int sr = ((color >> 16) & 0xff);
                int sg = ((color >> 8) & 0xff);
                int sb = ((color) & 0xff);

                bool noBlending = !doAlphaBlend || sa == 255;

                var pixels = context.Pixels;

                // Check boundaries
                if ((x1 < 0 && x2 < 0) || (y1 < 0 && y2 < 0)
                 || (x1 >= w && x2 >= w) || (y1 >= h && y2 >= h))
                {
                    return;
                }

                // Clamp boundaries
                if (x1 < 0) { x1 = 0; }
                if (y1 < 0) { y1 = 0; }
                if (x2 < 0) { x2 = 0; }
                if (y2 < 0) { y2 = 0; }
                if (x1 >= w) { x1 = w - 1; }
                if (y1 >= h) { y1 = h - 1; }
                if (x2 >= w) { x2 = w - 1; }
                if (y2 >= h) { y2 = h - 1; }

                //swap values
                if (y1 > y2)
                {
                    y2 -= y1;
                    y1 += y2;
                    y2 = (y1 - y2);
                }

                // Fill first line
                var startY = y1 * w;
                var startYPlusX1 = startY + x1;
                var endOffset = startY + x2;
                for (var idx = startYPlusX1; idx < endOffset; idx++)
                {
                    pixels[idx] = noBlending ? color : AlphaBlendColors(pixels[idx], sa, sr, sg, sb);
                }

                // Copy first line
                var len = (x2 - x1);
                var srcOffsetBytes = startYPlusX1 * SizeOfArgb;
                var offset2 = y2 * w + x1;
                for (var y = startYPlusX1 + w; y < offset2; y += w)
                {
                    if (noBlending)
                    {
                        BitmapContext.BlockCopy(context, srcOffsetBytes, context, y * SizeOfArgb, len * SizeOfArgb);
                        continue;
                    }

                    // Alpha blend line
                    for (int i = 0; i < len; i++)
                    {
                        int idx = y + i;
                        pixels[idx] = AlphaBlendColors(pixels[idx], sa, sr, sg, sb);
                    }
                }
            }
        }

        /// <summary>
        /// Sets the color of the pixel using a precalculated index (faster). 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public static void SetPixeli(this WriteableBitmap bmp, int index, byte r, byte g, byte b)
        {
            using (var context = bmp.GetBitmapContext())
            {
                context.Pixels[index] = (255 << 24) | (r << 16) | (g << 8) | b;
            }
        }

        /// <summary>
        /// Sets the color of the pixel using a precalculated index (faster). 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="color">The color.</param>
        public static void SetPixeli(this WriteableBitmap bmp, int index, Color color)
        {
            using (var context = bmp.GetBitmapContext())
            {
                context.Pixels[index] = ConvertColor(color);
            }
        }

        /// <summary>
        /// Sets the color of the pixel using an extra alpha value and a precalculated index (faster). 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="a">The alpha value of the color.</param>
        /// <param name="color">The color.</param>
        public static void SetPixeli(this WriteableBitmap bmp, int index, byte a, Color color)
        {
            using (var context = bmp.GetBitmapContext())
            {
                // Add one to use mul and cheap bit shift for multiplicaltion
                var ai = a + 1;
                context.Pixels[index] = (a << 24)
                           | ((byte)((color.R * ai) >> 8) << 16)
                           | ((byte)((color.G * ai) >> 8) << 8)
                           | ((byte)((color.B * ai) >> 8));
            }
        }

        /// <summary>
        /// Sets the color of the pixel using a precalculated index (faster).  
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="index">The coordinate index.</param>
        /// <param name="color">The color.</param>
        public static void SetPixeli(this WriteableBitmap bmp, int index, int color)
        {
            using (var context = bmp.GetBitmapContext())
            {
                context.Pixels[index] = color;
            }
        }

        /// <summary>
        /// Sets the color of the pixel. 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="r">The red value of the color.</param>
        /// <param name="g">The green value of the color.</param>
        /// <param name="b">The blue value of the color.</param>
        public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte r, byte g, byte b)
        {
            using (var context = bmp.GetBitmapContext())
            {
                context.Pixels[y * context.Width + x] = (255 << 24) | (r << 16) | (g << 8) | b;
            }
        }

        /// <summary>
        /// Sets the color of the pixel. 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="color">The color.</param>
        public static void SetPixel(this WriteableBitmap bmp, int x, int y, Color color)
        {
            using (var context = bmp.GetBitmapContext())
            {
                context.Pixels[y * context.Width + x] = ConvertColor(color);
            }
        }
        
        /// <summary>
        /// Sets the color of the pixel using an extra alpha value. 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="a">The alpha value of the color.</param>
        /// <param name="color">The color.</param>
        public static void SetPixel(this WriteableBitmap bmp, int x, int y, byte a, Color color)
        {
            using (var context = bmp.GetBitmapContext())
            {
                // Add one to use mul and cheap bit shift for multiplicaltion
                var ai = a + 1;
                context.Pixels[y * context.Width + x] = (a << 24)
                                             | ((byte)((color.R * ai) >> 8) << 16)
                                             | ((byte)((color.G * ai) >> 8) << 8)
                                             | ((byte)((color.B * ai) >> 8));
            }
        }

        /// <summary>
        /// Sets the color of the pixel. 
        /// For best performance this method should not be used in iterative real-time scenarios. Implement the code directly inside a loop.
        /// </summary>
        /// <param name="newRender">The WriteableBitmap.</param>
        /// <param name="x">The x coordinate (row).</param>
        /// <param name="y">The y coordinate (column).</param>
        /// <param name="color">The color.</param>
        public static void SetPixel(this WriteableBitmap bmp, int x, int y, int color)
        {
            using (var context = bmp.GetBitmapContext())
            {
                context.Pixels[y * context.Width + x] = color;
            }
        }

        private static int AlphaBlendColors(int pixel, int sa, int sr, int sg, int sb)
        {
            // Alpha blend
            int destPixel = pixel;
            int da = ((destPixel >> 24) & 0xff);
            int dr = ((destPixel >> 16) & 0xff);
            int dg = ((destPixel >> 8) & 0xff);
            int db = ((destPixel) & 0xff);

            destPixel = ((sa + (((da * (255 - sa)) * 0x8081) >> 23)) << 24) |
                                     ((sr + (((dr * (255 - sa)) * 0x8081) >> 23)) << 16) |
                                     ((sg + (((dg * (255 - sa)) * 0x8081) >> 23)) << 8) |
                                     ((sb + (((db * (255 - sa)) * 0x8081) >> 23)));

            return destPixel;
        }

        /// <summary>
        /// Creates a new bitmap with more room and copies the old data to it.
        /// The extra width and height must be values > 0.
        /// </summary>
        /// <param name="bmp">The WriteableBitmap.</param>
        /// <returns>A copy of the WriteableBitmap.</returns>
        public static WriteableBitmap SetSize(this WriteableBitmap bmp,
            int newWidth, int newHeight)
        {
            using (var srcContext = bmp.GetBitmapContext(ReadWriteMode.ReadOnly))
            {
                if (newWidth < srcContext.Width ||
                    newHeight < srcContext.Height)
                {
                    throw new ArgumentException("Width and height must be " +
                    "greater than zero when increasing image size.");
                }

                var result = new WriteableBitmap(
                    newWidth,
                    newHeight,
                    96.0, 96.0, PixelFormats.Pbgra32, null);

                using (var destContext = result.GetBitmapContext())
                {
                    BitmapContext.BlockCopy(srcContext, 0, destContext, 0, srcContext.Length * SizeOfArgb);
                }

                return result;
            }
        }
    }
}