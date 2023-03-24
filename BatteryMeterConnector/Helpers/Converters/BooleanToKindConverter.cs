// Author: Iulian Maftei

using MahApps.Metro.IconPacks;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace BatteryMeterConnector.Helpers.Converters
{
    public class BooleanToKindConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || !(value is bool))
                return PackIconMaterialKind.LinkVariantOff;

            bool boolValue = (bool)value;
            return boolValue ? PackIconMaterialKind.LinkVariant : PackIconMaterialKind.LinkVariantOff;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
