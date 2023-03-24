// Author: Iulian Maftei

using BatteryMeterConnector.Helpers;
using BatteryMeterConnector.Properties;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Reflection;

namespace BatteryMeterConnector.ViewModels
{
    public class MainWindowViewModel : ObservableObject
    {

        #region PROPS

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
                Settings.Default.Save();
            }
        }

        public ObservableCollection<string> AvailablePorts { get; set; }

        #endregion

        public MainWindowViewModel()
        {
            Baudrate = Settings.Default.BatteryMeterBaudRate;

            AvailablePorts = new ObservableCollection<string>();

            GetVersionNumber();
            GetAvailableCOMPorts();
        }

        private void GetVersionNumber()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Console.WriteLine(version);
            WindowTitle = $"{WindowTitle} V{version}";
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


    }
}
