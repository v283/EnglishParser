using AngleSharp;
using AngleSharp.Dom;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace EnglishParser
{
    internal class Program
    {
        static async Task Main(string[] args)
        {

            string file = "";

            string con = "D:\\tests.db";
            string TableName = "Eng1";

            try
            {
                using (StreamReader sr = new StreamReader("C:\\Новая папка\\file2.txt"))
                {
                    file = sr.ReadToEnd();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
            }

            using var context = BrowsingContext.New(Configuration.Default);
            using var code = await context.OpenAsync(req => req.Content(file));
            var elements = code.GetElementsByClassName("watupro-choices-columns show-question watupro-unresolved");

            List<TaskModel> doneTasks = new List<TaskModel>();
            foreach (var item in elements)
            {
                doneTasks.Add(new TaskModel() { Question = GetTask(item), CorrectAnsw = GetCorrectAnsw(item), Explanation = """Explanation:  To form Present Continuous we use the following structures: I am ('m) + verb + ing. We / You / They + are ('r) + verb + ing. He/ She / It + is ('s) + verb + ing.  """ 
                    + "\n"+ """To form a negative form in Present Continuous we use the following structure (short form): I am not ('m not) + verb + ing; We / You / They are not (aren't) + verb + ing; He/ She / It is not (isn't) + verb + ing. """
                    + "\n" + """To form a question in Present Continuous we need to use one of the following structures: 1) Am I + verb + ing? 2) Are we / you / they + verb + ing? 3) Is he/she/ it + verb + ing? """
                    + "\n" +$"Correct Answer(s): {GetCorrectAnsw(item)}"
                });
            }


            using (var connection = new SqliteConnection($"Data Source={con}"))
            {
                connection.Open();

                SqliteCommand command = new SqliteCommand();
                command.Connection = connection;
               
                command.CommandText = $"CREATE TABLE {TableName} (Id INTEGER NOT NULL UNIQUE,Question TEXT,AnswA TEXT,AnswB TEXT,AnswC TEXT,AnswD TEXT,CorrectAnsw TEXT,Explanation TEXT, PRIMARY KEY(Id  AUTOINCREMENT) )";
                command.ExecuteNonQuery();
                Console.WriteLine($"Таблица {TableName} создана");
                foreach (var item in doneTasks)
                {


                    try
                    {
                        command.CommandText = $"""INSERT INTO {TableName} (Question, AnswA, AnswB, AnswC, AnswD, CorrectAnsw, Explanation) VALUES ("{item.Question}", "","","","", "{item.CorrectAnsw}", "{item.Explanation}")""";
                        command.ExecuteNonQuery(); 
                    }
                    catch (Exception)
                    {
                   
                        Console.WriteLine( "Error" + item.Question + "------" + item.CorrectAnsw);
                    }

                }


            }
        }   
        static string GetTask(AngleSharp.Dom.IElement element)
        {
            var e = element.GetElementsByClassName("show-question-content")[0];
            var images = e.GetElementsByTagName("img");
            var spans = e.GetElementsByTagName("span");

            string s = e.InnerHtml;
            s = s.Replace(spans[0].OuterHtml, "");
            foreach (var item in spans)
            {
                s = s.Replace(item.OuterHtml, "________");
            }
            foreach (var item in images)
            {
                s = s.Replace(item.OuterHtml, "");

            }

            s = s.Replace("&nbsp;", "");

            Console.WriteLine(s);

            return s;
        }

        static string GetCorrectAnsw(AngleSharp.Dom.IElement element)
        {
            var e = element.GetElementsByClassName("watupro-main-feedback feedback-incorrect")[0];
            var spans = e.GetElementsByTagName("span")[0].TextContent;


            string result = spans.Replace("Correct answer:", "");
            result = result.Replace("Correct answers:", "");
            return result;
        }
    }

}
