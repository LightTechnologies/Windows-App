namespace LightVPN.Client.Windows.Converters
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using MaterialDesignThemes.Wpf;
    using Models;

    [ValueConversion(typeof(ConnectionState), typeof(PackIconKind))]
    internal sealed class ConnectionStateToPackIconKindConverter : IValueConverter
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
                ConnectionState.Connected => PackIconKind.Close,
                _ => PackIconKind.Connection,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
