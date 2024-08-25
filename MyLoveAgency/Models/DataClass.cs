using MyLoveAgency.Models.Database;
using Microsoft.Extensions.Configuration;

namespace MyLoveAgency.Models
{
    public static class DataClass
    {
        // Путь к файлу с паролем для доступа к Управляющей странице
        public static string pathToPasswordFile = "wwwroot/data/password.txt";

        // Пароль доступа к Управляющей странице
        public static string password = "";

        // Путь к файлу с данными о рассылке сообщений
        public static string pathToMailingFile = "wwwroot/data/mailing.txt";

        // Данные о рассылке сообщений
        public static List<(string email, string key)> Mailing = new List<(string email, string key)>();

        // Путь к файлу с почтами для уведомлений
        public static string pathToEmailNotification = "wwwroot/data/emails.txt";

        // Почты для уведомлений
        public static List<string> EmailForNotifications = new List<string>();

        // Путь к файлу логирования
        public static string pathToLogFile = "wwwroot/data/logs.txt";

        // Строка подключения к базе данных
        public static string? connectionString = "";

        // Массивы с контентом
        public static List<TypeService> TypeService = new List<TypeService>();
        public static List<Service> Services = new List<Service>();
        public static List<PackageService> Packages = new List<PackageService>();
        public static List<StorageImageService> ServiceImages = new List<StorageImageService>();
        public static List<StorageImageGallery> GalleryImages = new List<StorageImageGallery>();
        public static List<Faq> FAQ = new List<Faq>();
        public static List<Contact> Contacts = new List<Contact>();

        // Массив кодов подтверждения
        public static Dictionary<string, (string, DateTime)> StorageCode = new Dictionary<string, (string, DateTime)>();
    }
}
