using System;
using System.Globalization;
using System.Windows.Data;

namespace CyberSecurityChatBotPart2
{
    
    public class BotMessageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string line = value as string ?? "";
            return line.StartsWith("Bot:") || line.StartsWith("      ");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
