// Author: Iulian Maftei

using BatteryMeterConnector.Properties;
using NLog.Config;
using NLog;
using System;
using System.IO;
using System.Windows;

namespace BatteryMeterConnector
{
    public partial class App : Application
    {

        public App()
        {
            LogManager.Configuration = new XmlLoggingConfiguration("nlog.config");
        }
    }
}
