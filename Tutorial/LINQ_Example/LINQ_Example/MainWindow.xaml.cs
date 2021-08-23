using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LINQ_Example
{
    /// <summary>
    /// MainWindow.xaml 的互動邏輯
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        void LINQExample()
        {
            // Specify the data source.
            int[] scores = new int[] { 97, 92, 81, 60 };

            // Define the query expression.
            IEnumerable<int> scoreQuery =
                from score in scores
                where score > 80
                select score;

            // Execute the query.
            foreach (int i in scoreQuery)
            {
                Console.Write(i + " ");
            }
        }

        void MaxMethod()
        {
            int[] scores = new int[] { 97, 92, 81, 60 };
            var scoreQ = scores.Select(x => x).Max();
            var average = scores.Average();
            var max = scores.Average();


            var players = new List<Player> {
                new Player { Name = "Alex", Team = "A", Score = 10 },
                new Player { Name = "Anna", Team = "A", Score = 20 },
                new Player { Name = "Luke", Team = "L", Score = 60 },
                new Player { Name = "Lucy", Team = "L", Score = 40 },
            };

            var teamBestScores =
                from player in players
                group player by player.Team into playerGroup
                select new
                {
                    Team = playerGroup.Key,
                    BestScore = playerGroup.Max(x => x.Score),
                };

            foreach (var i in teamBestScores)
            {
                var team = i.Team;
                var score = i.BestScore;
                Console.WriteLine("Best score of Team {0} is {1}", team, score);
            }
        }

        void ReflectionHowTO()
        {
            Assembly assembly = Assembly.Load("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken= b77a5c561934e089");
            var pubTypesQuery = from type in assembly.GetTypes()
                                where type.IsPublic
                                from method in type.GetMethods()
                                where method.ReturnType.IsArray == true
                                    || (method.ReturnType.GetInterface(
                                        typeof(System.Collections.Generic.IEnumerable<>).FullName) != null
                                    && method.ReturnType.FullName != "System.String")
                                group method.ToString() by type.ToString();

            foreach (var groupOfMethods in pubTypesQuery)
            {
                Console.WriteLine("Type: {0}", groupOfMethods.Key);
                foreach (var method in groupOfMethods)
                {
                    Console.WriteLine("  {0}", method);
                }
            }

            Console.WriteLine("Press any key to exit... ");
            Console.ReadKey();
        }

        void LINQ_Example()
        {
            var source = Enumerable.Range(1, 10000);

            // Opt in to PLINQ with AsParallel.
            var evenNums = from num in source
                           where num % 2 == 0
                           select num;
            Console.WriteLine("{0} even numbers out of {1} total",
                              evenNums.Count(), source.Count());
            // The example displays the following output:
            //       5000 even numbers out of 10000 total
            foreach (var i in evenNums)
            {
                Console.WriteLine(i);
            }
        }

        void PLINQ_Example()
        {
            var source = Enumerable.Range(1, 10000);

            // Opt in to PLINQ with AsParallel.
            var evenNums = from num in source.AsParallel()
                           where num % 2 == 0
                           select num;
            Console.WriteLine("{0} even numbers out of {1} total",
                              evenNums.Count(), source.Count());
            // The example displays the following output:
            //       5000 even numbers out of 10000 total

            evenNums.ForAll(e => Console.WriteLine(e));
        }

        class Player
        {
            public string Name { get; set; }
            public string Team { get; set; }
            public int Score { get; set; }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            LINQExample();

            MaxMethod();

            ReflectionHowTO();

            var timer = new Stopwatch();
            timer.Start();
            LINQ_Example();
            timer.Stop();

            var timer2 = new Stopwatch();
            timer2.Start();
            PLINQ_Example();
            timer2.Stop();

            // PLINQ is fast than LINQ
            Console.WriteLine("LINQ Durarion: {0}", timer.Elapsed.TotalMilliseconds);
            Console.WriteLine("PLINQ Durarion: {0}", timer2.Elapsed.TotalMilliseconds);
        }
    }
}
