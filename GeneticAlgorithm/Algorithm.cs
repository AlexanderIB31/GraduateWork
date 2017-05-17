using System;
using System.Collections.Generic;
using System.Resources;
using System.Reflection;
using GeneticSharp.Domain;
using GeneticSharp.Domain.Populations;
using GeneticSharp.Domain.Chromosomes;
using GeneticSharp.Domain.Fitnesses;
using GeneticSharp.Domain.Crossovers;
using GeneticSharp.Domain.Mutations;
using GeneticSharp.Domain.Selections;
using GeneticSharp.Domain.Randomizations;
using GeneticSharp.Domain.Terminations;
using Diplom1;

namespace GA
{
	public class Algorithm
	{
		private static List<double> _params;
		private static TypeAction _ta;
		private static int _ms;
		private static string _path = $"{GA.Properties.Resources.pathSolution}" + @"\params.config";
		private static double _penaltyAccident = 1000;

		public static void Main(string[] args)
		{
			if (args.Length == 0 || args.Length > 2)
			{
				throw new IndexOutOfRangeException("Не верное количество аргументов.");
			}
			Program.WriteLineToConsole(ConsoleColor.Magenta,
@"1 --- тип действия (1 - ускорение, (1 - ускорение, 2 - переменное движение (резкое), 3 - переменное движение (плавное), 4 - торможение)
2 --- время работы алгоритма
3 --- путь к файлу со стационарными конфигурациями [необязательный параметр]");
			try
			{
				switch (args[0])
				{
					case "1":
						_ta = TypeAction.Acceleration;
						break;
					case "2":
						_ta = TypeAction.Smooth;
						break;
					case "3":
						_ta = TypeAction.Braking;
						break;
					default:
						throw new Exception("Некорректное значение 2-го аргумента (типа поведения впереди идущего автомобиля).\n1 - ускорение\n2 - попеременное ускорение и торможение\n3 - торможение");
				}
				_ms = int.Parse(args[1]);
			}
			catch (FormatException ex)
			{
				throw new Exception("Один из аргументов имеет некорректное значение. Либо проверьте порядок аргументов.", ex);
			}

			try
			{
				_path = args[2];
			}
			catch (IndexOutOfRangeException) { }

			using (var fr = new System.IO.StreamReader($"{_path}"))
			{
				_params = new List<double>(Array.ConvertAll(fr.ReadToEnd().Trim('|').Split('|'), double.Parse));
			}

			var selection = new EliteSelection();
			var crossover = new UniformCrossover();
			var mutation = new UniformMutation();
			var fitness = new MyProblemFitness();
			var chromosome = new MyProblemChromosome();
			var population = new Population(50, 100, chromosome);

			var ga = new GeneticAlgorithm(population, fitness, selection, crossover, mutation);
			ga.Termination = new GenerationNumberTermination(100);

			Program.WriteLineToConsole(ConsoleColor.Cyan, "Запускаю Генетические Алгоритмы ...");
			ga.Start();

			Program.WriteLineToConsole(ConsoleColor.Green, $"Лучшее решение имеет значение функции равное {ga.BestChromosome.Fitness}.");

			using (var fw = new System.IO.StreamWriter($"{GA.Properties.Resources.pathSolution}" + @"\params-with-GA.config"))
			{
				foreach (var x in ga.BestChromosome.GetGenes())
				{
					fw.Write($"|{x}");
				}
				fw.Write("|");
			}
			Program.WriteLineToConsole(ConsoleColor.Cyan, "Генетические Алгоритмы завершили работу ...");
			Console.ReadKey();
		}

		internal class MyProblemChromosome : ChromosomeBase
		{
			private List<double> _paramsGA;
			private readonly double _eps = 0.01;

			public MyProblemChromosome() : base(_params.Count)
			{
				_paramsGA = new List<double>();
				CreateGenes();
			}
			/// <summary>
			/// Создаем ген, который пополнит нашу популяцию
			/// </summary>
			/// <param name="geneIndex">порядковый номер гена</param>
			/// <returns>Возвращает структуру - Ген</returns>
			public override Gene GenerateGene(int geneIndex)
			{
				if (_params.Count == 17) // Схема 3-3-3
				{
					switch (geneIndex)
					{
						case 0:
						case 7:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(_params[geneIndex] - 0.1, _params[geneIndex] + 0.3));
							break;
						case 1:
						case 8:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(_paramsGA[geneIndex - 1] / 3, _params[geneIndex] + _eps));
							break;
						case 2:
						case 9:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(_paramsGA[geneIndex - 1] - 2 * _eps, 0 - _eps));
							break;
						case 3:
						case 10:
						case 15:
							_paramsGA.Add(0);
							break;
						case 4:
						case 11:
							_paramsGA.Add(-_paramsGA[geneIndex - 2]);
							break;
						case 5:
						case 12:
							_paramsGA.Add(-_paramsGA[geneIndex - 4]);
							break;
						case 6:
						case 13:
							_paramsGA.Add(-_paramsGA[geneIndex - 6]);
							break;
						case 14:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(-9.0, -2.0));
							break;
						case 16:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(1.5, 7.0));
							break;
					}
				}
				else if (_params.Count == 31) // Схема 5-5-5
				{
					switch (geneIndex)
					{
						case 0:
						case 13:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(_params[geneIndex] - 0.1, _params[geneIndex] + 0.1));
							break;
						case 1:
						case 14:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(_params[geneIndex] - 0.1, _params[geneIndex] + 0.1 - _eps));
							break;
						case 2:
						case 15:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(_params[geneIndex] - 0.1 + _eps, _params[geneIndex] + 0.1 - _eps));
							break;
						case 3:
						case 16:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(_params[geneIndex] - 0.1 + _eps, _params[geneIndex] + 0.1 - _eps));
							break;
						case 4:
						case 17:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(_params[geneIndex] - 0.1 + _eps, _params[geneIndex] + 0.1 - _eps));
							break;
						case 5:
						case 18:
							_paramsGA.Add(2 * _paramsGA[geneIndex - 2] - _paramsGA[geneIndex - 4]);
							break;
						case 6:
						case 19:
						case 28:
							_paramsGA.Add(0);
							break;
						case 7:
						case 20:
							_paramsGA.Add(-_paramsGA[geneIndex - 2]);
							break;
						case 8:
						case 21:
							_paramsGA.Add(-_paramsGA[geneIndex - 4]);
							break;
						case 9:
						case 22:
							_paramsGA.Add(-_paramsGA[geneIndex - 6]);
							break;
						case 10:
						case 23:
							_paramsGA.Add(-_paramsGA[geneIndex - 8]);
							break;
						case 11:
						case 24:
							_paramsGA.Add(-_paramsGA[geneIndex - 10]);
							break;
						case 12:
						case 25:
							_paramsGA.Add(-_paramsGA[geneIndex - 12]);
							break;
						case 26:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(-9.0, -6.0));
							break;
						case 27:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(-5.0, -2.0));
							break;
						case 29:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(1.0, 4.0));
							break;
						case 30:
							_paramsGA.Add(RandomizationProvider.Current.GetDouble(5.0, 7.5));
							break;
					}
				}

				return new Gene(_paramsGA[geneIndex]);
			}

			public override IChromosome CreateNew()
			{
				return new MyProblemChromosome();
			}
		}

		/// <summary>
		/// Функция приспособленности
		/// </summary>
		internal class MyProblemFitness : IFitness
		{
			/// <summary>
			/// Вычисление функции приспособленности;
			/// Подсчитывает среднеквадратичное отклонение от минимальной дистанции (безопасной)
			/// </summary>
			/// <param name="chromosome">"Сильнейшие" хромосомы</param>
			/// <returns>Возвращает вещественное число - значение функции приспособленности</returns>
			public double Evaluate(IChromosome chromosome)
			{
				List<double> param = new List<double>();
				foreach (var t in chromosome.GetGenes())
				{
					param.Add((double)t.Value);
				}

				#region Проверка на корректность границ нечетких множеств
				if (param.Count == 17)
				{
					for (int i = 0; i < 6; ++i)
					{
						if (param[i] > param[i + 1])
							return -_penaltyAccident * 100;
					}

					for (int i = 7; i < 13; ++i)
					{
						if (param[i] > param[i + 1])
							return -_penaltyAccident * 100;
					}

					for (int i = 14; i < 16; ++i)
					{
						if (param[i] > param[i + 1])
							return -_penaltyAccident * 100;
					}
				}
				else
				{
					for (int i = 0; i < 12; ++i)
					{
						if (param[i] > param[i + 1])
							return -_penaltyAccident * 100;
					}

					for (int i = 13; i < 25; ++i)
					{
						if (param[i] > param[i + 1])
							return -_penaltyAccident * 100;
					}

					for (int i = 26; i < 30; ++i)
					{
						if (param[i] > param[i + 1])
							return -_penaltyAccident * 100;
					}
				}
				#endregion

				List<int> time = new List<int>();
				for (int i = 0; i < _ms; ++i)
				{
					time.Add(i);
				}

				double criticalDist = 30;
				double curDist = 300;
				double mySpeed = 0;
				double entrySpeed = 16.7;
				double cruiseControlSpeed = 16.7;

				Solution.SetParams(param.ToArray(), criticalDist, curDist, mySpeed, entrySpeed, cruiseControlSpeed);
				var res = Solution.ToSolve(_ms, _ta);

				double eval = 0, x;

				for (int i = 0; i < _ms; i++)
				{
					x = criticalDist - res.Distances[i];
					if (x <= 0)
					{
						eval += x * x;
					}
					else
					{
						eval -= res.Distances[i] > 0 && x > 0 ? x * x / 3 : _penaltyAccident;
					}
				}

				return Math.Sqrt(eval / _ms);
			}
		}
	}


}