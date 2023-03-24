// Author: Iulian Maftei

using BatteryMeterConnector.Properties;
//using log4net;
using System;
using System.IO.Ports;

namespace BatteryMeterConnector.Helpers
{
    public class BatteryMeterCommunication
    {
        //ILog errorLog;
        public SerialPort SerialPortConnection { get; set; }

        //public BatteryMeterCommunication(ILog _errorLog)
        //{
        //    SerialPortConnection = new SerialPort();

        //    errorLog = _errorLog;
        //}


        public void Dispose()
        {
            if (SerialPortConnection.IsOpen)
            {
                SerialPortConnection.Close();
            }
            SerialPortConnection.Dispose();
        }

        private void ReopenPort()
        {
           // errorLog.Info("Trying to reopen the port.");
            //OpenPort();
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
            catch (InvalidOperationException ex)
            {
               // errorLog.Error(ex);
                ReopenPort();
                return returnMe;
            }
            catch (Exception ex)
            {
               // errorLog.Error(ex);
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
            catch (InvalidOperationException ex)
            {
                //errorLog.Error(ex);
                ReopenPort();
                return returnMe;
            }
            catch (Exception ex)
            {
                //errorLog.Error(ex);
                return returnMe;
            }
        }

        //public string OpenPort()
        //{
        //    try
        //    {
        //        if (!SerialPortConnection.IsOpen)
        //        {
        //            SerialPortConnection.BaudRate = Settings.Default.BatteryMeterBaudRate;
        //            //SerialPortConnection.PortName = Settings.Default.BatteryMeterPort;
        //            SerialPortConnection.ReadTimeout = Settings.Default.BatteryMeterReadTimeout;
        //            SerialPortConnection.WriteTimeout = Settings.Default.BatteryMeterWriteTimeout;
        //            SerialPortConnection.Open();
        //            //return $"Port {Settings.Default.BatteryMeterPort} is open";
        //        }
        //        else
        //        {
        //            //return ($"Port {Settings.Default.BatteryMeterPort} is already open");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //       // errorLog.Error(ex);
        //        return ex.Message;
        //    }
        //}

        //public string ClosePort()
        //{
        //    try
        //    {
        //        if (SerialPortConnection.IsOpen)
        //        {
        //            SerialPortConnection.Close();
        //            return $"Port {Settings.Default.BatteryMeterPort} is closed";
        //        }
        //        else
        //        {
        //            return $"Port {Settings.Default.BatteryMeterPort} is already closed";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //errorLog.Error(ex);
        //        return ex.Message;
        //    }
        //}

    }

}
