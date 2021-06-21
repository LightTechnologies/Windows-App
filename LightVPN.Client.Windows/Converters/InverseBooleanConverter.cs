using System;
using System.Globalization;
using System.Windows.Data;

namespace LightVPN.Client.Windows.Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    internal sealed class InverseBooleanConverter : IValueConverter
    {
        /// <inheritdoc />
        /// <summary>
        ///     Converts a boolean to it's inverse
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>The boolean, which is now inverted</returns>
        public object Convert(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (targetType != typeof(bool))
                throw new InvalidOperationException("The target must be a boolean");

            return (bool?)value == false;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Converts the input boolean back to it's original value, although not a good idea because
        ///     you might break something and by might I mean you WILL cause an exception, just look at
        ///     the method body :)))
        /// </summary>
        /// <param name="value"></param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>The boolean with it's original value</returns>
        public object ConvertBack(object value, Type targetType, object parameter,
            CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}