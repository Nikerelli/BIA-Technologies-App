using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.SQLite;
using System.Threading;
using System.Data.Entity;
using System.Data.Linq;
using System.Configuration;
using System.Data.SQLite.Linq;

namespace Application
{

    class Program
    {
        //Вы обладаете удивительной мягкостью и выдержкой, стыдно за такие ошибки.
        static void Main(string[] args)
        {
            Thread bkThread = new Thread(() =>
            {
                //[FIXME] Паттерн "Где мой 1998" :) Перепиши на System.Threading.Timer.
                //Период обновления - в конфиг
                //[FIXED] Исправлено.
                Timer timer = new Timer(new TimerCallback(WorkFlowFun), null, 0, Int32.Parse(ConfigurationManager.AppSettings["DatabaseRefreshTime"]));
            });
            bkThread.Start();

            //[FIXME] Зачем тебе петля? Испытываешь процессор на холостую нагрузку?
            //[FIXED?] Так поток же ждет ввода, по идее не сильно будет грузить?
            while (true)
            {
                CommandHandler.Execute(Console.ReadLine());
            }

        }

        static void WorkFlowFun(object a)
        {
            if (File.Exists(@"projects.json"))
            {
                //[FIXME] Уверен, что файл всегда будет десериализовываться?
                //И зачем тебе DataSet, если есть метадата всех моделей?
                //[FIXED] О боже, зачем я нагородил такую негибкую гору, если можно было сразу так...
                ProjectInfo pi = Deserializer.Deserialize(ConfigurationManager.AppSettings["json"].ToString());
                try
                {
                    SQLiteConnection con = new SQLiteConnection(ConfigurationManager.ConnectionStrings["DefaultConnection"].ToString());
                    AppContext db = new AppContext(con);

                    Console.WriteLine("Подключение с БД установлено.");

                    //Подготовка к обновлению, т.к. записей немного, то можно перезаписать все
                    db.ExecuteCommand(ConfigurationManager.AppSettings["ResetDatabase"]);
                    db.SubmitChanges();

                    //[FIXME] Хардкооодик. Вынести в ресурсы. Раз уж взялся руками всё делать
                    //[FIXED] Все-таки LinqToSql нужно было использовать,постарался уйти от хардкода, похоже?
                    db.Projects.InsertAllOnSubmit(pi.Projects);
                    db.Owners.InsertAllOnSubmit(pi.ProjectOwners);
                    db.SubmitChanges();
                    
                    Console.WriteLine("Данные в БД были обновлены!");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("Необходимо поместить файл project.json в {0}", Directory.GetCurrentDirectory());
            }
        }
    }
}
