namespace LightVPN.Client.Windows.Converters
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;
    using MaterialDesignThemes.Wpf;

    [ValueConversion(typeof(WindowState), typeof(PackIconKind))]
    internal sealed class WindowStateToPackIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not WindowState state) throw new ArgumentException("The value must be a WindowState");

            return state switch
            {
                WindowState.Maximized => PackIconKind.WindowRestore,
                _ => PackIconKind.WindowMaximize,
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
