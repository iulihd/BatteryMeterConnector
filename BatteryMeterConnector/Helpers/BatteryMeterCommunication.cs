﻿// Author: Iulian Maftei

using BatteryMeterConnector.Properties;
using NLog;
using System;
using System.IO;
using System.IO.Ports;

namespace BatteryMeterConnector.Helpers
{
    public class BatteryMeterCommunication : ObservableObject
    {
        private static readonly Logger errorLog = LogManager.GetCurrentClassLogger();

        private SerialPort serialPortConnection;
        public SerialPort SerialPortConnection
        {
            get { return serialPortConnection; }
            set
            {
                OnPropertyChanged(ref serialPortConnection, value);
            }
        }

        private bool isConnected;
        public bool IsConnected
        {
            get { return isConnected; }
            set
            {
                OnPropertyChanged(ref isConnected, value);
            }
        }
        public string Port { get; set; } = string.Empty;

        public BatteryMeterCommunication()
        {
            SerialPortConnection = new SerialPort();
            SerialPortConnection.BaudRate = Settings.Default.BatteryMeterBaudRate;
            SerialPortConnection.ReadTimeout = Settings.Default.BatteryMeterReadTimeout;
            SerialPortConnection.WriteTimeout = Settings.Default.BatteryMeterWriteTimeout;
        }


        public string SetPort(string port)
        {
            string returnValue = string.Empty;
            if (SerialPortConnection.IsOpen)
            {
                ClosePort();
                returnValue = $"Port {Port} is closed";
            }
            Port = port;
            SerialPortConnection.PortName = Port;
            return returnValue;
        }

        public void SetBaudRate(int value)
        {
            SerialPortConnection.BaudRate = value;
        }

        public void SetReadTimeOut(int value)
        {
            SerialPortConnection.ReadTimeout = value;
        }

        public void SetWriteTimeout(int value)
        {
            SerialPortConnection.WriteTimeout = value;
        }

        public void Dispose()
        {
            if (SerialPortConnection.IsOpen)
            {
                SerialPortConnection.Close();
                IsConnected = false;
            }
            SerialPortConnection.Dispose();
        }

        private void ReopenPort()
        {
            errorLog.Info("Trying to reopen the port.");
            OpenPort();
        }

        public string GetBatteryPercentage()
        {
            string returnMe = string.Empty;
            try
            {
                string command = "batteryPercent";
                SerialPortConnection.WriteLine(command);
                string resp = SerialPortConnection.ReadLine();
                if (!string.IsNullOrEmpty(resp))
                {
                    returnMe = resp.Substring(resp.IndexOf('=') + 1).Trim();
                }
                return returnMe;
            }
            catch (Exception ex)
            {
               errorLog.Error(ex);
                return returnMe;
            }
        }

        public int GetBatteryStatus()
        {
            int returnMe = 0;
            try
            {
                string command = "isCharging";
                SerialPortConnection.WriteLine(command);
                string resp = SerialPortConnection.ReadLine();
                if (!string.IsNullOrEmpty(resp))
                {
                    returnMe = int.Parse(resp.Substring(resp.IndexOf('=') + 1).Trim());
                }
                return returnMe;
            }
            catch (Exception ex)
            {
                errorLog.Error(ex);
                return returnMe;
            }
        }

        public string OpenPort()
        {
            try
            {
                if (!SerialPortConnection.IsOpen)
                {
                    SerialPortConnection.Open();
                    IsConnected = true;
                    return $"Port {Port} is open";
                }
                else
                {
                    IsConnected = true;
                    return ($"Port {Port} is already open");
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                errorLog.Error(ex);
                return ex.Message;
            }
        }

        public string ClosePort()
        {
            try
            {
                if (SerialPortConnection.IsOpen)
                {
                    SerialPortConnection.Close();
                    IsConnected = false;
                    return $"Port {Port} is closed";
                }
                else
                {
                    IsConnected = false;
                    return $"Port {Port} is already closed";
                }
            }
            catch (Exception ex)
            {
                IsConnected = false;
                errorLog.Error(ex);
                return ex.Message;
            }
        }

    }

}
