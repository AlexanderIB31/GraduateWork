using System;
using System.Collections.Generic;
using System.Globalization;

namespace Diplom1
{
	/// <summary>
	/// Основная программа, откуда происходит запуск работы алгоритма
	/// </summary>
	public class Program
	{
		public static void WriteLineToConsole(ConsoleColor color, string body)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(body);
			Console.ResetColor();
		}
		/// <summary>
		/// Точка входа в программу
		/// </summary>
		/// <param name="args">
		/// [0] --- индекс типа поведения (1 - ускорение, 2 - переменное движение (резкое), 3 - переменное движение (плавное), 4 - торможение)
		/// [1] --- путь к файлу с параметрами настройки границ нечетких множеств [необязательный]
		/// </param>
		/// <example>
		/// Diplom1.exe 1 < acceleration.test 
		/// Diplom1.exe 1 params.config < acceleration.test
		/// </example>
		public static void Main(string[] args)
		{
			if (args.Length == 0 || args.Length > 2)
			{
				throw new IndexOutOfRangeException("Не верное количество аргументов.");
			}

			TypeAction typeAction;

			switch (args[0])
			{
				case "1":
					typeAction = TypeAction.Acceleration;
					break;
				case "2":
					typeAction = TypeAction.Smooth;
					break;
				case "3":
					typeAction = TypeAction.LittleSmooth;
					break;
				case "4":
					typeAction = TypeAction.Braking;
					break;
				default:
					throw new Exception("Некорректное значение 2-го аргумента (типа поведения впереди идущего автомобиля).\n1 - ускорение\n2 - попеременное ускорение и торможение\n3 - торможение");
			}

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
			var path = args.Length < 2 ? @"./params.config" : args[1];
			// Считываем параметры для Solution из файла params.txt
			using (var fr = new System.IO.StreamReader($"{path}"))
			{
				parameters = new List<double>(Array.ConvertAll(fr.ReadToEnd().Trim('|').Split('|'), double.Parse));
			}

			Solution.SetParams(parameters.ToArray(), criticalDist, curDist, mySpeed, entrySpeed, cruiseControlSpeed);
			var res = Solution.ToSolve(ms, typeAction);

			Drawing.ToDraw(time.ToArray(), res.EntrySpeeds.ToArray(), res.OwnSpeeds.ToArray(), typeAction, TypeMeasure.Speed);
			Drawing.ToDraw(time.ToArray(), null, res.Distances.ToArray(), typeAction, TypeMeasure.Distance);

			WriteLineToConsole(ConsoleColor.Green, "Вычисления выполнены. Графики построены.");
		}
	}
}
