using System;
using System.Globalization;
using System.Windows.Data;
using LightVPN.Client.Windows.Models;

namespace LightVPN.Client.Windows.Converters
{
    [ValueConversion(typeof(ConnectionState), typeof(bool))]
    internal sealed class ConnectionStateToBooleanConverter : IValueConverter
    {
        /// <inheritdoc />
        /// <summary>
        ///     Convert a ConnectionState enum to PackIconKind (this is my custom converter)
        /// </summary>
        /// <param name="value">ConnectionState enum</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>PackIconKind</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ConnectionState connState)
                throw new ArgumentException("The value must be a ConnectionState");

            return connState switch
            {
                ConnectionState.Connecting => false,
                ConnectionState.Disconnecting => false,
                _ => true
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}