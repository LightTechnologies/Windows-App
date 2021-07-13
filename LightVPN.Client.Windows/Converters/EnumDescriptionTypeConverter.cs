namespace LightVPN.Client.Windows.Converters
{
    using System;
    using System.ComponentModel;
    using System.Globalization;

    /* Source: https://brianlagunas.com/a-better-way-to-data-bind-enums-in-wpf/ */

    public sealed class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type)
            : base(type)
        {
        }

        /// <inheritdoc />
        /// <summary>
        ///     This is for WPF bindings, since we are only using MVVM to display data. This method
        ///     should only be called by WPF therefore it's params are not documented
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value,
            Type destinationType)
        {
            if (destinationType != typeof(string)) return string.Empty;

            var fi = value?.GetType().GetField(value.ToString() ?? string.Empty);
            if (fi == null) return string.Empty;

            var attributes = (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return attributes.Length > 0 && !string.IsNullOrEmpty(attributes[0].Description)
                ? attributes[0].Description
                : value.ToString();
        }
    }
}
