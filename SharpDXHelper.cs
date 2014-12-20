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

namespace SwapChainPanelD2D
{
    public class SharpDXHelper
    {
        public D2D.Device Device { get; private set; }
        public D2D.DeviceContext Context { get; private set; }
        public DisposeCollector DisposeCollector { get; private set; }
        public DW.Factory1 Factory { get; private set; }

        //backing private field to use ref
        D2D.Bitmap1 _bitmapTarget;
        public D2D.Bitmap1 BitmapTarget {
            get { return _bitmapTarget; } 
            private set { _bitmapTarget = value; } }


        GraphicsPresenter _presenter;
        public GraphicsPresenter Presenter 
            {get { return _presenter; } 
            private set { _presenter = value; } }

        public GraphicsDevice GraphicsDevice { get; private set; }
        const D2D.DebugLevel D2DDebugLevel =
            #if DEBUG
             D2D.DebugLevel.Information
            #else
             D2D.DebugLevel.Error
            #endif
        ;

        public SwapChainPanel SwapChain { get; set; }

        public SharpDXHelper(SwapChainPanel swapChain)
        {
            DisposeCollector = new DisposeCollector();
            SwapChain = swapChain;
            Init();
        }

        private void DisposeAndUnload()
        {
            DisposeCollector.DisposeAndClear();

            Context = null;
            Device = null;
            Factory = null;
            Presenter = null;
            GraphicsDevice = null;
        }
        //probably should remove rendering event before calling
        public void StopRendering()
        {
            DisposeRenderTarget();

            GraphicsDevice.Presenter = null;
            
            DisposeCollector.RemoveAndDispose(ref _presenter);
        }

        public void ResetSize(Size size)
        {
            DisposeRenderTarget();

            Presenter.Resize((int)SwapChain.Width, (int)SwapChain.Height, Presenter.Description.BackBufferFormat);
            
            Init();
        }

        public void Present()
        {
            GraphicsDevice.Present();
        }

        public void DisposeRenderTarget()
        {
            Context.Target = null;
            
            DisposeCollector.RemoveAndDispose(ref _bitmapTarget);
        }

        void Init()
        {
            GraphicsDevice = DisposeCollector.Collect(GraphicsDevice.New(DeviceCreationFlags.BgraSupport));

            using (
                var dxgiDevice = ((SharpDX.Direct3D11.Device)GraphicsDevice)
                    .QueryInterface<SharpDX.DXGI.Device>())
            {
                Device = DisposeCollector.Collect(
                    new D2D.Device(
                        dxgiDevice, new D2D.CreationProperties { DebugLevel = D2DDebugLevel }));

                Context = DisposeCollector.Collect(
                    new D2D.DeviceContext(
                        Device, D2D.DeviceContextOptions.EnableMultithreadedOptimizations));
            }

            Factory = DisposeCollector.Collect(new SharpDX.DirectWrite.Factory1());

            var parameters = new PresentationParameters(
                (int)SwapChain.Width, (int)SwapChain.Height, SwapChain);

            Presenter = DisposeCollector.Collect(
                new SwapChainGraphicsPresenter(GraphicsDevice, parameters));

            GraphicsDevice.Presenter = Presenter;

            var renderTarget = Presenter.BackBuffer;

            var dpi = DisplayInformation.GetForCurrentView().LogicalDpi;

            var bitmapProperties = new D2D.BitmapProperties1(
                new SharpDX.Direct2D1.PixelFormat(
                    renderTarget.Format,
                    D2D.AlphaMode.Premultiplied),
                        dpi,
                        dpi,
                        D2D.BitmapOptions.CannotDraw | D2D.BitmapOptions.Target);

            BitmapTarget = DisposeCollector.Collect(
                new D2D.Bitmap1(Context, renderTarget, bitmapProperties));

            Context.Target = BitmapTarget;
        }

    }
}
