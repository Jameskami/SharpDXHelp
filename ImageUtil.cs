using System;
using System.Diagnostics;
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using SharpDX;
using SharpDX.Direct3D11;
using SharpDX.Toolkit.Graphics;
using D2D = SharpDX.Direct2D1;
using DW = SharpDX.DirectWrite;
using SharpDX.WIC;
using SharpDX.Direct2D1;
using SharpDX.IO;
using Windows.ApplicationModel;

namespace SwapChainPanelD2D
{
    public static class ImageUtil
    {
        public static Bitmap1 GetBitmap(D2D.DeviceContext context, string filePath)
        {
            ImagingFactory imagingFactory = new ImagingFactory();

            NativeFileStream fileStream = new NativeFileStream(
                Package.Current.InstalledLocation.Path + filePath,
                NativeFileMode.Open, NativeFileAccess.Read);

            BitmapDecoder bitmapDecoder = new BitmapDecoder(
                imagingFactory, fileStream, DecodeOptions.CacheOnDemand);

            BitmapFrameDecode frame = bitmapDecoder.GetFrame(0);

            FormatConverter converter = new FormatConverter(imagingFactory);

            converter.Initialize(frame, SharpDX.WIC.PixelFormat.Format32bppPRGBA);

            return SharpDX.Direct2D1.Bitmap1.FromWicBitmap(context, converter);
        }

        public static BitmapBrush1 GetBrush(D2D.DeviceContext context, Bitmap1 bitmap, ExtendMode mode)
        {
            return new BitmapBrush1(context, bitmap) { ExtendModeX = mode, ExtendModeY = mode };
        }
    }
}
