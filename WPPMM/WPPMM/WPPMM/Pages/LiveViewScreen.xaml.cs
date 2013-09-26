﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using WPPMM.CameraManager;
using System.Windows.Media.Imaging;
using System.Diagnostics;
using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Phone;
using Microsoft.Xna.Framework.Media;
using System.Windows.Resources;
using WPPMMComp;
using System.Text;


namespace WPPMM.Pages
{
    public partial class LiveViewScreen : PhoneApplicationPage
    {

        private CameraManager.CameraManager cameraManager = null;
        private bool isRequestingLiveview = false;
        private BitmapImage screenBitmapImage;
        private MemoryStream screenMemoryStream;
        private WriteableBitmap screenWritableBitmap;

        private byte[] screenData;
        private int screenDataLen;

        private Direct3DInterop m_d3dInterop = null;
        private Stopwatch watch;

        private System.Text.StringBuilder stringBuilder;


        public LiveViewScreen()
        {
            InitializeComponent();

            cameraManager = CameraManager.CameraManager.GetInstance();
            cameraManager.RegisterUpdateListener(UpdateListener);
            cameraManager.StartLiveView();
            cameraManager.SetLiveViewUpdateListener(LiveViewUpdateListener);

            isRequestingLiveview = true;

            screenBitmapImage = new BitmapImage();
            screenBitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;


            screenData = new byte[1];
            screenDataLen = screenData.Length;

            screenWritableBitmap = new WriteableBitmap(640, 480);

            screenMemoryStream = new MemoryStream();
            watch = new Stopwatch();
            watch.Start();
            stringBuilder = new System.Text.StringBuilder();

        }


        public void UpdateListener()
        {
            if (isRequestingLiveview && CameraManager.CameraManager.GetLiveviewUrl() != null)
            {
                // starting liveview
                cameraManager.ConnectLiveView();
            }
                
        }

        public void LiveViewUpdateListener(MemoryStream ms)
        {
            Debug.WriteLine("Live view update listener");
            BitmapImage bitmap = new BitmapImage();
            bitmap.CreateOptions = BitmapCreateOptions.DelayCreation;
            bitmap.SetSource(ms);
            ScreenImage.Source = bitmap;
        }

        public void LiveViewUpdateListener(byte[] data)
        {


            Debug.WriteLine("[" + watch.ElapsedMilliseconds + "ms" + "][LiveViewScreen] from last calling. ");

            int size = data.Length;
            // Debug.WriteLine("debug value: " + m_d3dInterop.GetDebugValue());
            stringBuilder.Clear();
            stringBuilder.Append("data: ");
            for (int i = 1000; i < 1050; i++)
            {
                stringBuilder.Append(" ");
                stringBuilder.Append(data[i].ToString());
            }
            Debug.WriteLine(stringBuilder.ToString());

            ScreenImage.Source = null;
            
            screenMemoryStream = new MemoryStream(data, 0, data.Length);

            
            screenBitmapImage.SetSource(screenMemoryStream);
            
            
            WriteableBitmap bmp = new WriteableBitmap(screenBitmapImage);
            // screenWritableBitmap.SetSource(screenMemoryStream);

            Debug.WriteLine("[" + watch.ElapsedMilliseconds + "ms" + "][LiveViewScreen] set source to WritableBitmap. " + size + "bytes. ");
            

            // m_d3dInterop.setTexture(out screenWritableBitmap.Pixels[0], screenWritableBitmap.PixelWidth, screenWritableBitmap.PixelHeight);
            
            ScreenImage.Source = screenBitmapImage;
            


            screenMemoryStream.Close();

        }

        private void DrawingSurface_Loaded(object sender, RoutedEventArgs e)
        {
            
            if (m_d3dInterop == null)
            {
                m_d3dInterop = new Direct3DInterop();

                

                // Set window bounds in dips
                m_d3dInterop.WindowBounds = new Windows.Foundation.Size(
                    (float)ScreenSurface.ActualWidth,
                    (float)ScreenSurface.ActualHeight
                    );

                // Set native resolution in pixels
                m_d3dInterop.NativeResolution = new Windows.Foundation.Size(
                    (float)Math.Floor(ScreenSurface.ActualWidth * Application.Current.Host.Content.ScaleFactor / 100.0f + 0.5f),
                    (float)Math.Floor(ScreenSurface.ActualHeight * Application.Current.Host.Content.ScaleFactor / 100.0f + 0.5f)
                    );

                // Set render resolution to the full native resolution
                m_d3dInterop.RenderResolution = m_d3dInterop.NativeResolution;

                // m_d3dInterop.SetTestNum(101);



                // m_d3dInterop.SetScreenData(screenData);

                // Hook-up native component to DrawingSurface
                ScreenSurface.SetContentProvider(m_d3dInterop.CreateContentProvider());
                ScreenSurface.SetManipulationHandler(m_d3dInterop);

            }
        }
    }
}