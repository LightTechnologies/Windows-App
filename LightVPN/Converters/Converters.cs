/* --------------------------------------------
 * 
 * UI (WPF) converters - Main class
 * Copyright (C) Light Technologies LLC
 * 
 * File: Converters.cs
 * 
 * Created: 04-03-21 Khrysus
 * 
 * --------------------------------------------
 */

using LightVPN.Common.Models;
using LightVPN.Models;
using MaterialDesignThemes.Wpf;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace LightVPN.Converters
{
    [ValueConversion(typeof(ConnectionState), typeof(PackIconKind))]
    public class ConnectionStateToPackIconKindConverter : IValueConverter
    {
        /// <summary>
        /// Convert a ConnectionState enum to PackIconKind (this is my custom converter) - Khrysus
        /// </summary>
        /// <param name="value">ConnectionState enum</param>
        /// <param name="targetType"></param>
        /// <param name="parameter"></param>
        /// <param name="culture"></param>
        /// <returns>PackIconKind</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not ConnectionState connState) return null;
            return connState switch
            {
                ConnectionState.Connected => PackIconKind.Close,
                _ => PackIconKind.Connection,
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    //Source: https://brianlagunas.com/a-better-way-to-data-bind-enums-in-wpf/
    public class EnumDescriptionTypeConverter : EnumConverter
    {
        public EnumDescriptionTypeConverter(Type type)
            : base(type)
        {
        }
        /// <summary>
        /// This is for WPF bindings, since we are only using MVVM to display data. This method should only be called by WPF therefore it's params are not documented
        /// </summary>
        /// <param name="context"></param>
        /// <param name="culture"></param>
        /// <param name="value"></param>
        /// <param name="destinationType"></param>
        /// <returns></returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                if (value != null)
                {
                    FieldInfo fi = value.GetType().GetField(value.ToString());
                    if (fi != null)
                    {
                        var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                        return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Description))) ? attributes[0].Description : value.ToString();
                    }
                }

                return string.Empty;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }

    [ValueConversion(typeof(bool), typeof(bool))]
    public class InverseBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts a boolean to it's inverse
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

            return !(bool)value;
        }
        /// <summary>
        /// Converts the input boolean back to it's original value, although not a good idea because you might break something and by might I mean you WILL cause an exception, just
        /// look at the method body :)))
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