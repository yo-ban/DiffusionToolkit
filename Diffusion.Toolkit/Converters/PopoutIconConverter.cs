using System;
using System.Globalization;
using System.Windows.Data;
using FontAwesome5;

namespace Diffusion.Toolkit.Converters;

public class PopoutIconConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? EFontAwesomeIcon.Solid_Columns : EFontAwesomeIcon.Solid_WindowRestore;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}