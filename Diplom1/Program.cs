using System;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using NPlot;

namespace Diplom1
{
	public class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine(@"Укажите время работы алгоритма (мс):");
			var ms = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

			List<int> time = new List<int>();
			for (int i = 0; i < ms; ++i)
			{
				time.Add(i);
			}

			double perfectDist = 30;
			double curDist = 300;
			double mySpeed = 0;
			double entrySpeed = 16.7;

			Console.WriteLine("Введите скорость преследуемого автомобиля (в км/ч):");
			entrySpeed = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture) * 1000 / 3600;
			Console.WriteLine("Введите скорость Вашего автомобиля (в км/ч):");
			mySpeed = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture) * 1000 / 3600;
			Console.WriteLine("Введите дистанцию, которую требуется соблюдать (в метрах):");
			perfectDist = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
			Console.WriteLine("Введите расстояние до преследуемого автомобиля (в метрах):");
			curDist = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

			List<double> parameters;
			var path = @"./params.txt";
			// Считываем параметры для Solution из файла params.txt, сгенерированным проектом GA
			using (var fr = new System.IO.StreamReader($"{path}"))
			{
				parameters = new List<double>(Array.ConvertAll(fr.ReadToEnd().Trim('|').Split('|'), double.Parse));
			}
			// dS --- расстояние между автомобилями; 
			// dV --- разница скоростей;
			// Первые 7 параметров - особые точки для dS (в метрах);
			// Вторые 7 параметров - особые точки для dV (в м/c);
			// Оставшиеся 5 параметров - особые точки для V*
			var typeAction = TypeAction.Acceleration;
			Solution.SetParams(parameters.ToArray(), perfectDist, curDist, mySpeed, entrySpeed);
			var res = Solution.ToSolve(ms, typeAction);
			Drawing.ToDraw(time.ToArray(), res.EntrySpeeds.ToArray(), res.OwnSpeeds.ToArray(), typeAction, TypeMeasure.Speed);
			Drawing.ToDraw(time.ToArray(), null, res.Distances.ToArray(), typeAction, TypeMeasure.Distance);
			//CreateLineGraph(time.ToArray(), res.OwnSpeeds.ToArray(), res.EntrySpeeds.ToArray(), "Speed");
			//CreateLineGraph(time.ToArray(), res.Distances.ToArray(), null, "Distination");
			//CreateLineGraph(time.ToArray(), res.Accelerations.ToArray(), null, "Acceleration");
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine("Вычисления выполнены. Графики построены. Нажмите ENTER...");
			Console.ResetColor();
			//Console.ReadKey();
		}
	}
}
