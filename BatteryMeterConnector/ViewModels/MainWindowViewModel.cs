// Author: Iulian Maftei

using BatteryMeterConnector.Helpers;
using BatteryMeterConnector.Properties;
using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Threading;
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
                if(value != null)
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

        public BatteryMeterCommunication BatteryMeterCommunication { get; set; }

        public ICommand RefreshCommand { get; set; }
        public ICommand ConnectCommand { get; set; }

        public ICommand DisconnectCommand { get; set; }

        public Timer ResetStatusTimer { get; set; }

        public MainWindowViewModel()
        {
            BatteryMeterCommunication = new BatteryMeterCommunication();

            Baudrate = Settings.Default.BatteryMeterBaudRate;

            AvailablePorts = new ObservableCollection<string>();

            RefreshCommand = new RelayCommand(RefreshCommandAction);
            ConnectCommand = new RelayCommand(ConnectCommandAction);
            DisconnectCommand = new RelayCommand(DisconnectCommandAction);

            ResetStatusTimer = new Timer(ClearStatusTextTrigger, null, Timeout.Infinite, Timeout.Infinite);

            GetVersionNumber();
            GetAvailableCOMPorts();
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
            string[] portNames = SerialPort.GetPortNames();
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

        public void ClearStatusTextTrigger(object sender)
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
