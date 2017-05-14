using System;
using System.Collections.Generic;
using System.Globalization;

namespace Diplom1
{
	public class Program
	{
		private static void WriteLineToConsole(ConsoleColor color, string body)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(body);
			Console.ResetColor();
		}

		public static void Main(string[] args)
		{
			var typeAction = TypeAction.Acceleration;

			WriteLineToConsole(ConsoleColor.White, "Укажите время работы алгоритма (мс):");
			var ms = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

			List<int> time = new List<int>();
			for (int i = 0; i < ms; ++i)
			{
				time.Add(i);
			}

			double criticalDist = 30;
			double curDist = 300;
			double mySpeed = 0;
			double entrySpeed = 16.7;
			double cruiseControlSpeed = 16.7;

			WriteLineToConsole(ConsoleColor.White, "Введите скорость, которую требуется поддерживать автомобилем (в км/ч):");
			cruiseControlSpeed = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
			cruiseControlSpeed = Solution.ConvertSpeedFromKilometersToMeters(cruiseControlSpeed);

			WriteLineToConsole(ConsoleColor.White, "Введите скорость Вашего автомобиля (в км/ч):");
			mySpeed = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
			mySpeed = Solution.ConvertSpeedFromKilometersToMeters(mySpeed);

			WriteLineToConsole(ConsoleColor.White, "Введите минимальную дистанцию до впереди идущего автомобиля (в метрах):");
			criticalDist = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

			WriteLineToConsole(ConsoleColor.White, "Введите расстояние до впереди идущего автомобиля (в метрах):");
			curDist = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);

			WriteLineToConsole(ConsoleColor.White, "Введите скорость вперди идущего автомобиля (в км/ч):");
			entrySpeed = double.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
			entrySpeed = Solution.ConvertSpeedFromKilometersToMeters(entrySpeed);

			List<double> parameters;
			var path = @"./params.txt";
			// Считываем параметры для Solution из файла params.txt
			using (var fr = new System.IO.StreamReader($"{path}"))
			{
				parameters = new List<double>(Array.ConvertAll(fr.ReadToEnd().Trim('|').Split('|'), double.Parse));
			}

			// dS --- расстояние между автомобилями; 
			// dV --- разница скоростей;
			// Первые 7 параметров - особые точки для dS (в метрах);
			// Вторые 7 параметров - особые точки для dV (в м/c);
			// Оставшиеся 5 параметров - особые точки для V*
			Solution.SetParams(parameters.ToArray(), criticalDist, curDist, mySpeed, entrySpeed, cruiseControlSpeed);
			var res = Solution.ToSolve(ms, typeAction);

			Drawing.ToDraw(time.ToArray(), res.EntrySpeeds.ToArray(), res.OwnSpeeds.ToArray(), typeAction, TypeMeasure.Speed);
			Drawing.ToDraw(time.ToArray(), null, res.Distances.ToArray(), typeAction, TypeMeasure.Distance);

			WriteLineToConsole(ConsoleColor.Green, "Вычисления выполнены. Графики построены.");
		}
	}
}
