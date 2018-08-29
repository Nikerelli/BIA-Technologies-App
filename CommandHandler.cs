using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Configuration;

namespace Application
{
    static public class CommandHandler
    {
        //[FIXME] То есть, консоль на ввод условной команды deactivate выдаст мне отчет по активным проектам?
        //Кроме того, хардкод, мягко говоря, не приветствуется. Лучше брать команды из app.config через ConfigurationManager       
        //[FIXED]
        static Regex activeMask = new Regex(ConfigurationManager.AppSettings["activeReportCommand"].ToString());
        static Regex ratingMask = new Regex(ConfigurationManager.AppSettings["ratingReportCommand"].ToString());
        const string quit = "quit";

        //[THINKABOUT] Working directory всегда совпадает с директорией, из которой запущена конкретная сборка?
        //[FIXED]

        //[FIXME] Порядок следования модификаторов типа несколько иной
        //[FIXME] Переделать метод на работу с app.config
        //[FIXED]
        public static void Execute(string command)
        {
            //[FIXME] Литералы, которые ТОЧНО не изменятся -в const string, остальные -в app.config
            //[FIXED]
            if (command.ToLower() == quit)
                Environment.Exit(0);
            if (ratingMask.IsMatch(command))
            {
                //[THINKABOUT] А появится еще 10 команд - для каждой будем пилить новый метод?
                //Как можно забороть необходимость постоянно править класс? (Вспомни про O в SOLID)
                //[HANDLED]Думаю, что я бы сделал возможность добавлять комманды в Dictionary<key,Action<>>
                CreateRatingReport();
            }
            else
            {
                if (activeMask.IsMatch(command))
                {
                    command = command.Trim();
                    string[] parts = command.Split(' ');
                    //[THINKABOUT] После появления третьего параметра опять изменять метод
                    //[HANDLED] Возможо это можно решить добавлением масок
                    if (parts.Length >= 2)
                    {
                        //[THINKABOUT] if (DateTime.TryParse(parts[1], out DateTime time)) - такая нотация экономит строку
                        //Вообще, всю кучу строк из этого блока можно заменить на две
                        //[HANDLED] Не смог найти такой вариант...
                        DateTime time;
                        if (DateTime.TryParse(parts[1], out time))
                            CreateActiveProjectsReport(time);
                        else
                        {
                            Console.WriteLine("Программа не смогла распознать дату, для отчета будет использована сегодняшняя дата.");
                            CreateActiveProjectsReport(DateTime.Now);
                        }
                    }
                    else
                    {
                        CreateActiveProjectsReport(DateTime.Now);
                    }
                }
            }
        }

        public static void CreateActiveProjectsReport(DateTime time)
        {
            try
            {
                //[FIXME] Приведи реальный кейс, когда time может быть null
                //[FIXED]

                //[FIXME] Если уж ты совсем не хочешь использовать ORM-слой всилу простоты запроса или каких-то других соображений - 
                //вынеси хотя бы запрос в ресурсный файл. Изменение БД не должно каждый раз заставлять нас лезть переписывать логику класса. Да и не очень читаемо это всё.
                //И зачем линковать EF6, если потом всё равно собираешь запрос и обрабатываешь выборку руками?
                //[FIXED]

                //[FIXME] Почему в конструктор хардкодится одна строка, а в app.config - совершенно другая? Кстати, вообще почему строка подключения хардкодится?
                //В частности, я распаковал архив с двумя солюшенами as is - и Generator не может найти БД. Представь, что я такой обидчивый клиент, которому ты кинул сборки. Куда бежать? Что делать?
                //[FIXED]

                SQLiteConnection con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());
                using (AppContext db = new AppContext(con))
                {
                    //[THINKABOUT] Может, имеет смысл хотя бы простые инициализации делать с помощью var? Даже Java этому научилась, а мы-то моднее-молодёжнее!
                    //[HANDLED] Учел.

                    var projects = (from p in db.Projects
                                   where p.StartDate < time
                                   select p).ToList();
                    var owners = db.Owners.ToList();

                    int size = projects.Count + 1;
                    string[][] input = new string[size][];

                    //[THINKABOUT] 10 раз сформировали отчет - 10 раз строится шапка. Что можно сделать?
                    //[HANDLED] Заранее в отчет шапку вбить.
                    input[0] = new string[] { "Наименование проекта", "Дата старта проекта", "Владелец проекта" };

                    if (!File.Exists(ConfigurationManager.AppSettings["reportsDirectory"] + ConfigurationManager.AppSettings["activeReportName"]))
                        File.Create(ConfigurationManager.AppSettings["reportsDirectory"] + ConfigurationManager.AppSettings["activeReportName"]);

                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["reportsDirectory"] + ConfigurationManager.AppSettings["activeReportName"], false, Encoding.Default))
                    {
                        for (int index = 1; index < size; index++)
                        {
                            string[] item = new string[3];
                            item[0] = projects[index - 1].Name;
                            //[FIXME] А тут точно может быть null?
                            //[FIXED]
                            item[1] = projects[index - 1].StartDate.ToString();
                            item[2] = owners.First(o => o.GUID == projects[index - 1].OwnerGUID).Name;

                            input[index] = item;
                        }
                        string delimiter = ";";

                        foreach (string[] str in input)
                        {
                            sw.WriteLine(string.Join(delimiter, str));
                        }
                    };
                    Console.WriteLine("Отчет сформирован успешно!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Возникла ошибка, отчет не сформирован!");
                Console.WriteLine(e.Message);
            }

        }

        //[FIXME] Святотатство в порядке модификаторов
        //[FIXED]
        public static void CreateRatingReport()
        {

            try
            {
                //[FIXME] Ну var же есть! Хардкод убирай. И ты уверен, что я смогу подключиться к твоему data source? 
                //[FIXED]
                SQLiteConnection con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());
                using (AppContext db = new AppContext(con))
                {
                    var projects = (from p in db.Projects
                                    where p.FinishDate < DateTime.Now
                                    select p).ToList();
                    var owners = db.Owners.ToList();

                    List<RatingByFinished> rating = new List<RatingByFinished>();
                    foreach (var owner in owners)
                        rating.Add(new RatingByFinished { Owner = owner.Name, Finished = projects.Count(p => p.OwnerGUID == owner.GUID) });
                    rating = (from o in rating
                              orderby o.Finished 
                              select o).Reverse().ToList();


                    int size = owners.Count + 1;
                    string[][] input = new string[size][];

                    //[FIXME] Шапка не совпадает с выводимыми полями
                    //[FIXED]
                    input[0] = new string[] { "Позиция", "Имя владельца проекта", "Количество закрытых проектов" };

                    if (!File.Exists(ConfigurationManager.AppSettings["reportsDirectory"] + ConfigurationManager.AppSettings["ratingReportName"]))
                        File.Create(ConfigurationManager.AppSettings["reportsDirectory"] + ConfigurationManager.AppSettings["ratingReportName"]);


                    using (StreamWriter sw = new StreamWriter(ConfigurationManager.AppSettings["reportsDirectory"] + ConfigurationManager.AppSettings["ratingReportName"], false, Encoding.UTF8))
                    {
                        for (int index = 1; index < size; index++)
                        {
                            string[] item = new string[3];
                            item[0] = index.ToString();
                            item[1] = rating[index - 1].Owner;
                            item[2] = rating[index - 1].Finished.ToString() ?? "0";

                            input[index] = item;
                        }
                        string delimiter = ";";

                        foreach (string[] str in input)
                        {
                            sw.WriteLine(string.Join(delimiter, str),Encoding.Default);
                        }
                    };
                    Console.WriteLine("Отчет сформирован успешно!");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Возникла ошибка, отчет не сформирован!");
                Console.WriteLine(e.Message);
            }
        }
    }
}
