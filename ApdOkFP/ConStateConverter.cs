using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ApdOkFP
{
    [ValueConversion(typeof(bool), typeof(BitmapImage))]

    public class ConStateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if ((bool)value)
                return new BitmapImage(new Uri("/img/LEDgreen.png", UriKind.RelativeOrAbsolute));
            else
                return new BitmapImage(new Uri("/img/LEDgray.png", UriKind.RelativeOrAbsolute));

            //return ((bool)value) ? @"\img\LEDgreen.png" : @"\img\LEDgray.png";
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
