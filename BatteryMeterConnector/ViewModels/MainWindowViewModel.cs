// Author: Iulian Maftei

using BatteryMeterConnector.Helpers;
using BatteryMeterConnector.Properties;
using MahApps.Metro.IconPacks;
using NLog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace BatteryMeterConnector.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {

        #region PROPS

        private string status = string.Empty;
        public string Status
        {
            get { return status; }
            set
            {
                if (value != String.Empty)
                {
                    LastMessageTimeStamp = $"{DateTime.Now.ToString("HH:mm:ss")}";
                }
                else
                {
                    LastMessageTimeStamp = String.Empty;
                }
                OnPropertyChanged(ref status, value);
            }
        }

        private int batteryPercent = 0;
        public int BatteryPercent
        {
            get { return batteryPercent; }
            set
            {
                OnPropertyChanged(ref batteryPercent, value);
            }
        }


        private double batteryMaxCapacity = 0;
        public double BatteryMaxCapacity
        {
            get { return batteryMaxCapacity; }
            set
            {
                NewBatteryMaxCapacity = value;
                OnPropertyChanged(ref batteryMaxCapacity, value);
            }
        }

        private double newBatteryMaxCapacity = 0;
        public double NewBatteryMaxCapacity
        {
            get { return newBatteryMaxCapacity; }
            set
            {
                OnPropertyChanged(ref newBatteryMaxCapacity, value);
            }
        }

        private int batterySleepTimer = 0;
        public int BatterySleepTimer
        {
            get { return batterySleepTimer; }
            set
            {
                NewBatterySleepTimer = value;
                OnPropertyChanged(ref batterySleepTimer, value);
            }
        }

        private int newBatterySleepTimer = 0;
        public int NewBatterySleepTimer
        {
            get { return newBatterySleepTimer; }
            set
            {
                OnPropertyChanged(ref newBatterySleepTimer, value);
            }
        }

        private int batteryButtonCooldown = 0;
        public int BatteryButtonCooldown
        {
            get { return batteryButtonCooldown; }
            set
            {
                NewBatteryButtonCooldown = value;
                OnPropertyChanged(ref batteryButtonCooldown, value);
            }
        }

        private int newBatteryButtonCooldown = 0;
        public int NewBatteryButtonCooldown
        {
            get { return newBatteryButtonCooldown; }
            set
            {
                OnPropertyChanged(ref newBatteryButtonCooldown, value);
            }
        }

        private int batteryScreenBrightness = 0;
        public int BatteryScreenBrightness
        {
            get { return batteryScreenBrightness; }
            set
            {
                NewBatteryScreenBrightness = value;
                OnPropertyChanged(ref batteryScreenBrightness, value);
            }
        }

        private int newBatteryScreenBrightness = 0;
        public int NewBatteryScreenBrightness
        {
            get { return newBatteryScreenBrightness; }
            set
            {
                OnPropertyChanged(ref newBatteryScreenBrightness, value);
            }
        }


        private bool batteryIsCharging = false;
        public bool BatteryIsCharging
        {
            get { return batteryIsCharging; }
            set
            {
                OnPropertyChanged(ref batteryIsCharging, value);
            }
        }

        private bool batteryIsDischarging = false;
        public bool BatteryIsDischarging
        {
            get { return batteryIsDischarging; }
            set
            {
                OnPropertyChanged(ref batteryIsDischarging, value);
            }
        }

        private string lastMessageTimeStamp = string.Empty;
        public string LastMessageTimeStamp
        {
            get { return lastMessageTimeStamp; }
            set
            {
                OnPropertyChanged(ref lastMessageTimeStamp, value);
            }
        }

        private Visibility portStatusVisibility = Visibility.Hidden;
        public Visibility PortStatusVisibility
        {
            get { return portStatusVisibility; }
            set
            {
                OnPropertyChanged(ref portStatusVisibility, value);
            }
        }

        private string windowTitle = "BATTERY METER CONNECTER";
        public string WindowTitle
        {
            get { return windowTitle; }
            set
            {
                OnPropertyChanged(ref windowTitle, value);
            }
        }

        private string selectedPortCOM = string.Empty;
        public string SelectedPortCOM
        {
            get { return selectedPortCOM; }
            set {
                if (value != null)
                {
                    SetStatus(BatteryMeterCommunication.SetPort(value));
                    PortStatusVisibility = Visibility.Visible;
                }
                else
                {
                    PortStatusVisibility = Visibility.Hidden;
                }
                OnPropertyChanged(ref selectedPortCOM, value);
            }
        }

        private int baudrate = 0;
        public int Baudrate
        {
            get { return baudrate; }
            set
            {
                Settings.Default.BatteryMeterBaudRate = value;
                OnPropertyChanged(ref baudrate, value);
                BatteryMeterCommunication.SetBaudRate(value);
                Settings.Default.Save();
            }
        }

        private int readTimeOut = 0;
        public int ReadTimeOut
        {
            get { return readTimeOut; }
            set
            {
                Settings.Default.BatteryMeterReadTimeout = value;
                OnPropertyChanged(ref readTimeOut, value);
                BatteryMeterCommunication.SetReadTimeOut(value);
                Settings.Default.Save();
            }
        }

        private int writeTimeOut = 0;
        public int WriteTimeOut
        {
            get { return writeTimeOut; }
            set
            {
                Settings.Default.BatteryMeterWriteTimeout = value;
                OnPropertyChanged(ref writeTimeOut, value);
                BatteryMeterCommunication.SetWriteTimeout(value);
                Settings.Default.Save();
            }
        }


        public ObservableCollection<string> AvailablePorts { get; set; }

        #endregion


        private static readonly Logger errorLog = LogManager.GetCurrentClassLogger();
        public BatteryMeterCommunication BatteryMeterCommunication { get; set; }

        public ICommand RefreshCommand { get; set; }
        public ICommand ConnectCommand { get; set; }
        public ICommand DisconnectCommand { get; set; }
        public ICommand ApplyCommand { get; set; }


        public Timer ResetStatusTimer { get; set; }
        public Timer PollingTimer { get; set; }

        public SemaphoreSlim Semaphore { get; set; } = new SemaphoreSlim(1);


        public MainWindowViewModel()
        {
            BatteryMeterCommunication = new BatteryMeterCommunication();
            BatteryMeterCommunication.IsConnectedChangedEvent += BatteryMeterCommunication_IsConnectedChangedEvent;

            Baudrate = Settings.Default.BatteryMeterBaudRate;
            ReadTimeOut = Settings.Default.BatteryMeterReadTimeout;
            WriteTimeOut = Settings.Default.BatteryMeterWriteTimeout;

            AvailablePorts = new ObservableCollection<string>();

            RefreshCommand = new RelayCommand(RefreshCommandAction);
            ConnectCommand = new RelayCommand(ConnectCommandAction);
            DisconnectCommand = new RelayCommand(DisconnectCommandAction);
            ApplyCommand = new RelayCommand(ApplyCommandAction);

            ResetStatusTimer = new Timer(ClearStatusTextTrigger, null, Timeout.Infinite, Timeout.Infinite);
            PollingTimer = new Timer(PollBatteryMeter, null, Timeout.Infinite, Timeout.Infinite);

            GetVersionNumber();
            GetAvailableCOMPorts();
        }

        private void BatteryMeterCommunication_IsConnectedChangedEvent(bool isConnected)
        {
            if(isConnected)
            {
                PollingTimer.Change(0, Settings.Default.PollingInterval);
            }
            else
            {
                PollingTimer.Change(Timeout.Infinite, Timeout.Infinite);
                Application.Current.Dispatcher.Invoke(() =>
                {
                    GetAvailableCOMPorts();
                });
            }
        }


        //TODO:
        private async void ApplyCommandAction(object obj)
        {
            string tag = (string)obj;
            await Semaphore.WaitAsync();
            try
            {
                await Task.Run(() =>
                {
                    if (tag == "BatteryMaxCapacity")
                    {
                        SetStatus(BatteryMeterCommunication.SetMaxBatteryCapacity(NewBatteryMaxCapacity));
                    }
                    else if (tag == "SleepTimer")
                    {
                        BatteryMeterCommunication.SetSleepTimer(NewBatterySleepTimer);
                    }
                    else if (tag == "ButtonCooldown")
                    {
                        BatteryMeterCommunication.SetButtonCooldown(NewBatteryButtonCooldown);
                    }
                    else if (tag == "ScreenBrightness")
                    {
                        BatteryMeterCommunication.SetScreenBrightness(NewBatteryScreenBrightness);
                    }
                });
            }
            catch (Exception ex)
            {
                errorLog.Error(ex);
            }
            finally
            {
                Semaphore.Release();
            }

        }

        private async void InitialPolling()
        {
            await Semaphore.WaitAsync();
            try
            {
                await Task.Run(() =>
                {
                    BatteryMaxCapacity = BatteryMeterCommunication.GetMaxBatteryCapacity();
                    BatterySleepTimer = BatteryMeterCommunication.GetSleepTimer();
                    BatteryButtonCooldown = BatteryMeterCommunication.GetButtonCooldown();
                    BatteryScreenBrightness = BatteryMeterCommunication.GetScreenBrightness();
                });

            }
            catch (Exception ex)
            {
                errorLog.Error(ex);
            }
            finally
            {
                Semaphore.Release();
            }
        }

        private async void PollBatteryMeter(object? state)
        {
            await Semaphore.WaitAsync();
            try
            {
                BatteryPercent = int.Parse(BatteryMeterCommunication.GetBatteryPercentage());
                BatteryIsCharging = (BatteryMeterCommunication.GetIsChargingStatus() != 0);
                BatteryIsDischarging = (BatteryMeterCommunication.GetIsDischargingStatus() != 0);

            }catch(Exception ex)
            {
                errorLog.Error(ex);
            }
            finally 
            {
                Semaphore.Release();
            }
        }

        private void DisconnectCommandAction(object obj)
        {
            string result = BatteryMeterCommunication.ClosePort();
            SetStatus(result);  
        }

        private void ConnectCommandAction(object obj)
        {
            string result = BatteryMeterCommunication.OpenPort();
            SetStatus(result);
            InitialPolling();
        }

        private void RefreshCommandAction(object obj)
        {
            GetAvailableCOMPorts();
            SetStatus("Refreshing ports...");
        }

        private void GetVersionNumber()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine(version);
            WindowTitle = $"{WindowTitle} v{version}";
        }

        public void GetAvailableCOMPorts()
        {
            List<string> portNames = new List<string>();

            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity WHERE (Name LIKE '%(COM%)')"))
            {
                foreach (var device in searcher.Get())
                {
                    string name = device.GetPropertyValue("Name")?.ToString();
                    if (!string.IsNullOrEmpty(name))
                    {
                        int startIndex = name.IndexOf("(COM") + 1;
                        int endIndex = name.IndexOf(")", startIndex);
                        if (startIndex > 0 && endIndex > startIndex)
                        {
                            string portName = name.Substring(startIndex, endIndex - startIndex);
                            portNames.Add(portName);
                        }
                    }
                }
            }

            AvailablePorts.Clear();
            foreach(string portName in portNames)
            {
                AvailablePorts.Add(portName);
            }
            if(AvailablePorts.Count > 0)
            {
                SelectedPortCOM = AvailablePorts[0];
            }
        }

        public void ClearStatusTextTrigger(object? state)
        {
            Status = string.Empty;
        }

        public void SetStatus(string status)
        {
            Status = status;
            ResetStatusTimer.Change(Settings.Default.StatusResetDelay, 0);
        }

    }
}
