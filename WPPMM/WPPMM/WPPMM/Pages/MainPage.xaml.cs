﻿using Microsoft.Phone.Controls;
using Microsoft.Phone.Net.NetworkInformation;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using System;
using System.Diagnostics;
using System.Windows.Navigation;
using WPPMM.CameraManager;
using WPPMM.Resources;


namespace WPPMM
{
    public partial class MainPage : PhoneApplicationPage
    {

        private static CameraManager.CameraManager cameraManager;

        // コンストラクター
        public MainPage()
        {
            InitializeComponent();

            BuildLocalizedApplicationBar();

            cameraManager = CameraManager.CameraManager.GetInstance();

            // get current network status
            UpdateNetworkStatus();


        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            UpdateNetworkStatus();
            if (GetSSIDName().StartsWith("DIRECT-"))
            {
                CameraManager.CameraManager.GetInstance().RequestSearchDevices(() =>
                {
                    GoToShootingPage();
                }, () => { });
            }
        }

        private void HandleError(int code)
        {
            Debug.WriteLine("Error: " + code);
        }

        private void HandleActTakePictureResult(string[] urls)
        {
            Debug.WriteLine("HandleActTakePictureResult");
            foreach (var url in urls)
            {
                Debug.WriteLine("URL: " + url);
            }
        }

        private void UpdateNetworkStatus()
        {
            Debug.WriteLine("SSID: " + GetSSIDName());
            if (DeviceNetworkInformation.IsWiFiEnabled)
            {
                NetworkStatus.Text = AppResources.Guide_WiFiNotEnabled;
                SearchButton.IsEnabled = true;
            }
            else if (GetSSIDName().StartsWith("DIRECT-"))
            {
                NetworkStatus.Text = AppResources.Guide_CantFindDevice;
            }
            else
            {
                NetworkStatus.Text = AppResources.Guide_WiFiNotConnected;
            }

            if (cameraManager != null)
            {
                if (cameraManager.GetDeviceInfo() != null)
                {
                    String modelName = cameraManager.GetDeviceInfo().FriendlyName;
                    if (modelName != null)
                    {
                        NetworkStatus.Text = "Connected device: " + modelName;
                        StartRemoteButton.IsEnabled = true;
                    }
                }
            }

            // display initialize

            ProgressBar.Visibility = System.Windows.Visibility.Collapsed;
            cameraManager.UpdateEvent += WifiUpdateListener;
        }

        private void OnWiFiSettingButtonClicked(object sender, System.Windows.RoutedEventArgs e)
        {
            ConnectionSettingsTask connectionSettingsTask = new ConnectionSettingsTask();
            connectionSettingsTask.ConnectionSettingsType = ConnectionSettingsType.WiFi;
            connectionSettingsTask.Show();
        }

        private void SearchButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            cameraManager.InitializeConnection();
            ProgressBar.IsIndeterminate = true;

        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            GoToShootingPage();
        }

        private void GoToShootingPage()
        {
            NavigationService.Navigate(new Uri("/Pages/LiveViewScreen.xaml", UriKind.Relative));
        }

        internal void WifiUpdateListener(Status cameraStatus)
        {

            if (cameraStatus.isAvailableConnecting)
            {
                String modelName = cameraManager.GetDeviceInfo().FriendlyName;
                NetworkStatus.Text = "Connected device: " + modelName;
            }

            if (cameraStatus.isAvailableConnecting && cameraStatus.MethodTypes != null)
            {
                StartRemoteButton.IsEnabled = true;
            }

        }

        private string GetSSIDName()
        {
            foreach (var network in new NetworkInterfaceList())
            {
                if (
                    (network.InterfaceType == NetworkInterfaceType.Wireless80211) &&
                    (network.InterfaceState == ConnectState.Connected)
                    )
                {
                    return network.InterfaceName;
                }
            }
            return "<Not connected>";
        }

        private void OSS_Menu_Click(object sender, System.EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Pages/LicensePage.xaml", UriKind.Relative));
        }

        private void BuildLocalizedApplicationBar()
        {
            ApplicationBar = new ApplicationBar();
            ApplicationBar.Mode = ApplicationBarMode.Minimized;
            ApplicationBar.Opacity = 0.5;

            var OssMenuItem = new ApplicationBarMenuItem(AppResources.OSSText);
            OssMenuItem.Click += OSS_Menu_Click;
            ApplicationBar.MenuItems.Add(OssMenuItem);
        }
    }
}