using System;
using System.Data.Entity;
using System.Linq;
using System.Text.Json;

namespace ConsoleApp3
{

    /// <summary>
    /// Работа с EntityFramework6 использовался метод CodeFirst (БД создаётся автоматически).
    /// </summary>
    class UserOptionsContext : DbContext
    {
        /// <summary>
        /// контекст данных, используемый для взаимодействия с базой данных
        /// </summary>
        public UserOptionsContext()
            : base("DbConnection")
        { }

        /// <summary>
        /// набор объектов, которые хранятся в базе данных
        /// </summary>
        public DbSet<UserOptions> UsersOptions { get; set; }
    }

    /// <summary>
    /// Пользовательские настройки.
    /// </summary>
    public class UserOptions
    {
        /// <summary>
        /// Ключ для работы с EntityFramework
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Имя пользователя(идентификатор/логин)
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// Наименование приложения.
        /// </summary>
        public string Application { get; set; }

        /// <summary>
        /// JSON - Строка, пользовательских настроек.
        /// </summary>
        public string Options { get; set; }
    }


    /// <summary>
    /// Класс для хранения/получения пользовательских настроек в централизованном месте.
    /// Подразумевается, что параметры сохранения будут зарание известный и прописаны в
    /// секции connectionStrings под алиасом DbConnection. Класс статический, предоставляет
    /// функции для сохранения и получения настроек пользователя. 
    /// </summary>
    static class UserOption
    {
        /// <summary>
        /// Сохраняем пользовательские настройки.
        /// </summary>
        /// <typeparam name="T">класс настроек</typeparam>
        /// <param name="options">настройки</param>
        /// <param name="user">пользователь, чьи настройки необходимо сохрать.</param>
        /// <param name="application">приложение, настройки которого необходимо сохранить</param>
        public static void Save<T>(T options, string user, string application)
        {
            SetOptionsToDB(new UserOptions { UserName = user, Application = application, Options = JsonSerializer.Serialize<T>(options) });
        }

        /// <summary>
        /// Получаем пользовательские настройки.
        /// </summary>
        /// <typeparam name="T">класс настроек</typeparam>
        /// <param name="user">ользователь, чьи настройки необходимо получить</param>
        /// <param name="application">приложение, настройки которого необходимо получить</param>
        /// <returns>Класс настроек, или Null в случае если не удалось получить пользовательские настройки</returns>
        public static T Load<T>(string user, string application)
        {
            var q = GetOptinosFromDB(user, application);
            if (q != null)
            {
                return JsonSerializer.Deserialize<T>(q);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// Удаляем настройки всех приложений определённого пользователя.
        /// </summary>
        /// <param name="userName">Имя пользователя чьи настройки необходимо удалить</param>
        public static void RemoveAllOptionsFromUser(string userName)
        {
            using (UserOptionsContext db = new UserOptionsContext())
            {
                db.UsersOptions.Where(x => x.UserName == userName).ToList().ForEach(x => db.UsersOptions.Remove(x));
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Удаляем настройки всех пользователей определённого приложания.
        /// </summary>
        /// <param name="applicationName">наименование приложения чьи настройки необходимо удалить</param>
        public static void RemoveAllOptinonsFromApllication(string applicationName)
        {
            using (UserOptionsContext db = new UserOptionsContext())
            {
                db.UsersOptions.Where(x => x.Application == applicationName).ToList().ForEach(x => db.UsersOptions.Remove(x));
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Удаляем настройки всех пользователей определённого приложания.
        /// </summary>
        /// <param name="applicationName">наименование приложения чьи настройки необходимо удалить</param>
        public static void RemoveOptinonsFromUserAndApllication(string userName, string applicationName)
        {
            using (UserOptionsContext db = new UserOptionsContext())
            {
                db.UsersOptions.Where(x => x.Application == applicationName && x.UserName == userName).ToList().ForEach(x => db.UsersOptions.Remove(x));
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Работа с БД (сохранение/обновление настроек).
        /// </summary>
        /// <param name="userOptions">Клас пользовательских настроек</param>
        private static void SetOptionsToDB(UserOptions userOptions)
        {
            using (UserOptionsContext db = new UserOptionsContext())
            {
                var q = db.UsersOptions.FirstOrDefault(x =>
                    x.Application == userOptions.Application && x.UserName == userOptions.UserName);
                if (q != null)
                {
                    q.Options = userOptions.Options;
                }
                else
                {
                    db.UsersOptions.Add(userOptions);
                }
                db.SaveChanges();
            }
        }

        /// <summary>
        /// Работа с БД(получение пользовательских настроек)
        /// </summary>
        /// <param name="user">Пользователь, настройки которого необходимо получить</param>
        /// <param name="application">Приложение, настройки которого необходимо получить</param>
        /// <returns>JSON-строка - Сериализованный класс или Null, если не удалось найти пользовательские данные</returns>
        private static string GetOptinosFromDB(string user, string application)
        {
            using (UserOptionsContext db = new UserOptionsContext())
            {
                var q = db.UsersOptions.FirstOrDefault(x =>
                    x.Application == application && x.UserName == user);
                if (q != null)
                {
                    return q.Options;
                }
                return null;
            }
        }
    }

    /// <summary>
    /// Тестовые классы для демонстрации;
    /// </summary>
    class Person
    {
        public string Name { get; set; }
        public int Age { get; set; }
    }

    /// <summary>
    /// Тестовые классы для демонстрации
    /// </summary>
    class User
    {
        public string Name { get; set; }
        public string Login { get; set; }

    }

    class Program
    {
        static void Main(string[] args)
        {
            Person personTom = new Person { Name = "Tom", Age = 35 };
            User userTom = new User { Name = "Tom", Login = "tom35" };
            UserOption.Save<Person>(personTom, "usr", "app");
            UserOption.Save<User>(userTom, "usr", "app2");
            
            //UserOption.RemoveAllOptinonsFromApllication("app2"); 
            //UserOption.RemoveAllOptionsFromUser("usr");
            //UserOption.RemoveOptinonsFromUserAndApllication("usr", "app2");
            
            var q = UserOption.Load<Person>("usr", "app");
            if (q != null)
            {
                Console.WriteLine(q.Name);
            }
            var q1 = UserOption.Load<User>("usr", "app2");
            if (q1 != null)
            {
                Console.WriteLine(q1.Login);
            }
            Console.WriteLine("=");
            Console.ReadKey();
        }
    }
}
