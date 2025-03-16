using GGPlayer.Models;
using Data.Entities;
using Shared.Services;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace GGPlayer.Converters
{
    public class MaterialTypeToImageConverter : IValueConverter
    {
        //конвертация в изображение
        public object? Convert(object material, Type targetType, object parameter, CultureInfo culture)
        {
            switch (material)
            {
                case Material:
                    var m = material as Material;
                    // Если материал имеет аудиофайл, возвращаем изображение для аудио
                    if (m.AudioPath != null)
                    {
                        return new BitmapImage(UriService.GetAbsoluteUri("/Images/listening.png", "Shared"));
                    }
                    // Если нет аудиофайла, возвращаем изображение для чтения
                    else
                    {
                        return new BitmapImage(UriService.GetAbsoluteUri("/Images/reading.png", "Shared"));
                    }
                case FakeSegmentMaterial:
                    return new BitmapImage(UriService.GetAbsoluteUri("/Images/testing.png", "Shared"));
            }

            return null;
        }

        // Метод преобразования из изображения обратно в значение материала(надо реализовать)
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
