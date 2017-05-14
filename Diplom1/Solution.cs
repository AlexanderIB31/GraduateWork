using System;
using System.Collections.Generic;

namespace Diplom1
{
	/// <summary>
	/// Алгоритм адаптивного круиз-контроля с использованием Нечеткой Логики
	/// </summary>
	public static class Solution
	{
		#region Ключевые точки нечетких множеств
		// Особые точки для dS
		private static double _x1;
		private static double _x2;
		private static double _x3;
		private static double _x4;
		private static double _x5;
		private static double _x6;
		private static double _x7;
		private static double _x8;
		private static double _x9;
		private static double _x10;
		private static double _x11;
		private static double _x12;
		private static double _x13;
		// Особые точки для dV
		private static double _z1;
		private static double _z2;
		private static double _z3;
		private static double _z4;
		private static double _z5;
		private static double _z6;
		private static double _z7;
		private static double _z8;
		private static double _z9;
		private static double _z10;
		private static double _z11;
		private static double _z12;
		private static double _z13;
		private static double _y1;
		private static double _y2;
		private static double _y3;
		#endregion

		// Минимальное расстояние до цели
		private readonly static double _eps = 3;
		// Скорость в (м/с)
		private static double _mySpeed = 0;
		// Цель движется неравномерно
		private static double _entrySpeed = 0;
		// Скорость которую выставляем круиз-контролю (которую требуется поддерживать)
		private static double _cruiseControlSpeed = 16.7;
		// Минимальная безопасная дистанция до впереди идущей машины в (м)
		private static double _criticalDistance = 30;
		// Текущая дистанция в (м)
		private static double _currentDistance = 300;
		// Частота вычислений: 1 (сек)
		private readonly static double _tau = 1;

		// Таблица правил вывода
		private static Dictionary<string, double> _rules = new Dictionary<string, double>();
		/// <summary>
		/// Задание параметров (границ нечетких множеств, скоростей и дистанции); Инициализация
		/// </summary>
		/// <param name="args">Границы нечетких множеств (для дистанции (входные данные), для скорости (входные данные), для ускорения (выходная переменная))</param>
		/// <param name="criticalDist">Минимальное расстояние до впереди идущего автомобиля</param>
		/// <param name="curDist">Начальное расстояние до впереди идущего автомобиля</param>
		/// <param name="mySpeed">Начальная скорость автомобиля с адаптивным круиз-контролем</param>
		/// <param name="entrySpeed">Начальная скорость впереди идущего автомобиля</param>
		/// <param name="cruiseControlSpeed">Скорость, которую должен поддерживать адаптивный круиз-контроль</param>
		public static void SetParams(double[] args, double criticalDist, double curDist, double mySpeed, double entrySpeed, double cruiseControlSpeed)
		{
			try
			{
				_x1 = args[0];
				_x2 = args[1];
				_x3 = args[2];
				_x4 = args[3];
				_x5 = args[4];
				_x6 = args[5];
				_x7 = args[6];
				_x8 = args[7];
				_x9 = args[8];
				_x10 = args[9];
				_x11 = args[10];
				_x12 = args[11];
				_x13 = args[12];

				_z1 = args[13];
				_z2 = args[14];
				_z3 = args[15];
				_z4 = args[16];
				_z5 = args[17];
				_z6 = args[18];
				_z7 = args[19];
				_z8 = args[20];
				_z9 = args[21];
				_z10 = args[22];
				_z11 = args[23];
				_z12 = args[24];
				_z13 = args[25];

				_y1 = args[26];
				_y2 = args[27];
				_y3 = args[28];
				_y4 = args[29];
				_y5 = args[30];

				_criticalDistance = criticalDist;
				_currentDistance = curDist;
				_mySpeed = mySpeed;
				_entrySpeed = entrySpeed;
				_cruiseControlSpeed = cruiseControlSpeed;
			}
			catch (IndexOutOfRangeException ex)
			{				
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Ошибка: проверьте кол-во передаваемых в args параметров, их должно быть 17!");
				Console.ResetColor();
				throw ex;
			}
		}

		#region Фаззификация для Дистанции
		private static double VeryCloseDistance(double x)
		{
			if (_x1 <= x && x <= _x3)
			{
				return (_x3 - x) / (_x3 - _x1);
			}
			if (x <= _x1)
			{
				return 1;
			}
			return 0;
		}

		private static double CloseDistance(double x)
		{
			if (_x2 <= x && x <= _x4)
			{
				return (x - _x2) / (_x4 - _x2);
			}
			if (_x4 <= x && x <= _x6)
			{
				return (_x6 - x) / (_x6 - _x4);
			}
			return 0;
		}

		private static double ZeroDistance(double x)
		{
			if (_x5 <= x && x <= _x7)
			{
				return (x - _x5) / (_x7 - _x5);
			}
			if (_x7 <= x && x <= _x9)
			{
				return (_x9 - x) / (_x9 - _x7);
			}
			return 0;
		}

		private static double FarDistance(double x)
		{
			if (_x8 <= x && x <= _x10)
			{
				return (x - _x8) / (_x10 - _x8);
			}
			if (_x10 <= x && x <= _x12)
			{
				return (_x12 - x) / (_x12 - _x10);
			}
			return 0;
		}

		private static double VeryFarDistance(double x)
		{
			if (_x11 <= x && x <= _x13)
			{
				return (x - _x11) / (_x13 - _x11);
			}
			if (_x13 <= x)
			{
				return 1;
			}
			return 0;
		}
		#endregion

		#region Фаззификация для Скорости
		private static double VeryLessSpeed(double z)
		{
			if (_z1 <= z && z <= _z3)
			{
				return (_z3 - z) / (_z3 - _z1);
			}
			if (z <= _z1)
			{
				return 1;
			}
			return 0;
		}

		private static double LessSpeed(double z)
		{
			if (_z2 <= z && z <= _z4)
			{
				return (z - _z2) / (_z4 - _z2);
			}
			if (_z4 <= z && z <= _z6)
			{
				return (_z6 - z) / (_z6 - _z4);
			}
			return 0;
		}

		private static double ZeroSpeed(double z)
		{
			if (_z5 <= z && z <= _z7)
			{
				return (z - _z5) / (_z7 - _z5);
			}
			if (_z7 <= z && z <= _z9)
			{
				return (_z9 - z) / (_z9 - _z7);
			}
			return 0;
		}

		private static double MoreSpeed(double z)
		{
			if (_z8 <= z && z <= _z10)
			{
				return (z - _z8) / (_z10 - _z8);
			}
			if (_z10 <= z && z <= _z12)
			{
				return (_z12 - z) / (_z12 - _z10);
			}
			return 0;
		}

		private static double VeryMoreSpeed(double z)
		{
			if (_z11 <= z && z <= _z13)
			{
				return (z - _z11) / (_z13 - _z11);
			}
			if (_z13 <= z)
			{
				return 1;
			}
			return 0;
		}
		#endregion

		/// <summary>
		/// Полное решение на протяжении всего интервала времени
		/// </summary>
		/// <param name="time">Время работы алгоритма</param>
		/// <param name="ta">Тип поведения впереди идущего автомобиля</param>
		/// <returns></returns>
		public static SolutionResult ToSolve(int time, TypeAction ta)
		{
			Random r = new Random();
			List<int> Time = new List<int>();
			List<double> ownSpeeds = new List<double>();
			List<double> entrySpeeds = new List<double>();
			List<double> distances = new List<double>();
			List<double> accelerations = new List<double>();

			for (int i = 0; i < time; ++i)
			{
				_entrySpeed = CalcEntrySpeed(ta, i);

				entrySpeeds.Add(ConvertSpeedFromMetersToKilometers(_entrySpeed));
				Time.Add(i);

				var accel = ToSolveNow(_currentDistance, _criticalDistance, _mySpeed, _cruiseControlSpeed);
				_mySpeed = Math.Max(_mySpeed + _tau * accel, 0);

				if (_currentDistance - _tau * (_mySpeed - _entrySpeed) <= _eps)
				{
					_mySpeed = 0;
				}

				_currentDistance -= _tau * (_mySpeed - _entrySpeed);

				ownSpeeds.Add(ConvertSpeedFromMetersToKilometers(_mySpeed));
				distances.Add(_currentDistance);
				accelerations.Add(accel);
			}

			return new SolutionResult(ownSpeeds, entrySpeeds, distances, accelerations);
		}

		/// <summary>
		/// Решение в конкретный момент времени
		/// </summary>
		/// <returns>Возвращает ускорение, с которым должен двигаться в данный момент автомобиль с адаптивным круиз-контролем</returns>
		public static double ToSolveNow(double curDistance, double critDistance, double mySpeed, double cruiseCntrlSpeed)
		{
			var deltaDistance = critDistance != 0 ? (curDistance - critDistance) / critDistance : 0;
			var deltaSpeed = cruiseCntrlSpeed != 0 ? (mySpeed - cruiseCntrlSpeed) / cruiseCntrlSpeed : 0;

			#region Fuzzification
			// Пять областей фазиффикации: очень близко (прямоуг трапеция), близко (треугольник), отлично (треугольник), далеко (треугольник), очень далеко (прямоуг. трапеция)
			double A1 = VeryCloseDistance(deltaDistance);
			double B1 = CloseDistance(deltaDistance);
			double C1 = ZeroDistance(deltaDistance);
			double D1 = FarDistance(deltaDistance);
			double E1 = VeryFarDistance(deltaDistance);

			double A2 = VeryLessSpeed(deltaSpeed);
			double B2 = LessSpeed(deltaSpeed);
			double C2 = ZeroSpeed(deltaSpeed);
			double D2 = MoreSpeed(deltaSpeed);
			double E2 = VeryMoreSpeed(deltaSpeed);
			#endregion
			#region InferenceRule
			_rules["VeryCloseDist"] = A1;
			_rules["CloseDist"] = B1;
			_rules["ZeroDist"] = C1;
			_rules["FarDist"] = D1;
			_rules["VeryFarDist"] = E1;
			_rules["VeryLessSpeed"] = A2;
			_rules["LessSpeed"] = B2;
			_rules["ZeroSpeed"] = C2;
			_rules["MoreSpeed"] = D2;
			_rules["VeryMoreSpeed"] = E2;
			#endregion
			#region Defuzzification
			// Дефазиффикация осуществляется методом Среднего Центра (центроидный метод)
			var resAccel = (_y1 * Math.Min(_rules["VeryCloseDist"], _rules["VeryLessSpeed"])
						   + _y1 * Math.Min(_rules["VeryCloseDist"], _rules["LessSpeed"])
						   + _y1 * Math.Min(_rules["VeryCloseDist"], _rules["ZeroSpeed"])
							 + _y1 * Math.Min(_rules["VeryCloseDist"], _rules["MoreSpeed"])
							 + _y1 * Math.Min(_rules["VeryCloseDist"], _rules["VeryMoreSpeed"])

							 + _y2 * Math.Min(_rules["CloseDist"], _rules["VeryLessSpeed"])
							 + _y2 * Math.Min(_rules["CloseDist"], _rules["LessSpeed"])
							 + _y2 * Math.Min(_rules["CloseDist"], _rules["ZeroSpeed"])
							 + _y1 * Math.Min(_rules["CloseDist"], _rules["MoreSpeed"])
							 + _y1 * Math.Min(_rules["CloseDist"], _rules["VeryMoreSpeed"])

							 + _y3 * Math.Min(_rules["ZeroDist"], _rules["VeryLessSpeed"])
							 + _y3 * Math.Min(_rules["ZeroDist"], _rules["LessSpeed"])
							 + _y3 * Math.Min(_rules["ZeroDist"], _rules["ZeroSpeed"])
							 + _y2 * Math.Min(_rules["ZeroDist"], _rules["MoreSpeed"])
							 + _y1 * Math.Min(_rules["ZeroDist"], _rules["VeryMoreSpeed"])

							 + _y5 * Math.Min(_rules["FarDist"], _rules["VeryLessSpeed"])
							 + _y4 * Math.Min(_rules["FarDist"], _rules["LessSpeed"])
							 + _y3 * Math.Min(_rules["FarDist"], _rules["ZeroSpeed"])
							 + _y2 * Math.Min(_rules["FarDist"], _rules["MoreSpeed"])
							 + _y1 * Math.Min(_rules["FarDist"], _rules["VeryMoreSpeed"])

							 + _y5 * Math.Min(_rules["VeryFarDist"], _rules["VeryLessSpeed"])
							 + _y4 * Math.Min(_rules["VeryFarDist"], _rules["LessSpeed"])
							 + _y3 * Math.Min(_rules["VeryFarDist"], _rules["ZeroSpeed"])
							 + _y2 * Math.Min(_rules["VeryFarDist"], _rules["MoreSpeed"])
							 + _y2 * Math.Min(_rules["VeryFarDist"], _rules["VeryMoreSpeed"]))
						/ (Math.Min(_rules["VeryCloseDist"], _rules["VeryLessSpeed"])
							 + Math.Min(_rules["VeryCloseDist"], _rules["LessSpeed"])
							 + Math.Min(_rules["VeryCloseDist"], _rules["ZeroSpeed"])
							 + Math.Min(_rules["VeryCloseDist"], _rules["MoreSpeed"])
							 + Math.Min(_rules["VeryCloseDist"], _rules["VeryMoreSpeed"])

							 + Math.Min(_rules["CloseDist"], _rules["VeryLessSpeed"])
							 + Math.Min(_rules["CloseDist"], _rules["LessSpeed"])
							 + Math.Min(_rules["CloseDist"], _rules["ZeroSpeed"])
							 + Math.Min(_rules["CloseDist"], _rules["MoreSpeed"])
							 + Math.Min(_rules["CloseDist"], _rules["VeryMoreSpeed"])

							 + Math.Min(_rules["ZeroDist"], _rules["VeryLessSpeed"])
							 + Math.Min(_rules["ZeroDist"], _rules["LessSpeed"])
							 + Math.Min(_rules["ZeroDist"], _rules["ZeroSpeed"])
							 + Math.Min(_rules["ZeroDist"], _rules["MoreSpeed"])
							 + Math.Min(_rules["ZeroDist"], _rules["VeryMoreSpeed"])

							 + Math.Min(_rules["FarDist"], _rules["VeryLessSpeed"])
							 + Math.Min(_rules["FarDist"], _rules["LessSpeed"])
							 + Math.Min(_rules["FarDist"], _rules["ZeroSpeed"])
							 + Math.Min(_rules["FarDist"], _rules["MoreSpeed"])
							 + Math.Min(_rules["FarDist"], _rules["VeryMoreSpeed"])

							 + Math.Min(_rules["VeryFarDist"], _rules["VeryLessSpeed"])
							 + Math.Min(_rules["VeryFarDist"], _rules["LessSpeed"])
							 + Math.Min(_rules["VeryFarDist"], _rules["ZeroSpeed"])
							 + Math.Min(_rules["VeryFarDist"], _rules["MoreSpeed"])
							 + Math.Min(_rules["VeryFarDist"], _rules["VeryMoreSpeed"]));
			#endregion

			return resAccel;
		}

		private static double CalcEntrySpeed(TypeAction ta, int curTime)
		{
			double val = _entrySpeed;
			double tmp = Math.Sin(Math.PI / ((curTime % 20) + 1));
			double g = ((curTime / 20) & 1) == 1 ?  tmp * 3 : -tmp * 3;
			switch (ta)
			{
				case TypeAction.Acceleration:
					val = Math.Min(val + 3, 50);
					break;
				case TypeAction.Braking:
					val = Math.Max(val - 5, 0);
					break;
				case TypeAction.Smooth:
					val = Math.Max(Math.Min(g + val, 50), 0);
					break;
			}
			return val;
		}
		/// <summary>
		/// Преобразование скорости из км/ч в м/с
		/// </summary>
		/// <param name="from">Скорость в км/ч</param>
		/// <returns></returns>
		public static double ConvertSpeedFromKilometersToMeters(double from)
		{
			return from * 1000 / 3600;
		}
		/// <summary>
		/// Преобразование скорости из м/с в км/ч
		/// </summary>
		/// <param name="from">Скорость в м/с</param>
		/// <returns></returns>
		public static double ConvertSpeedFromMetersToKilometers(double from)
		{
			return from * 3600 / 1000;
		}
		/// <summary>
		/// Преобразование скорости из км/ч в м/с
		/// </summary>
		/// <param name="from">Скорость в км/ч</param>
		/// <returns></returns>
		public static double ConvertSpeedFromKilometersToMeters(double from)
		{
			return from * 1000 / 3600;
		}
		/// <summary>
		/// Преобразование скорости из м/с в км/ч
		/// </summary>
		/// <param name="from">Скорость в м/с</param>
		/// <returns></returns>
		public static double ConvertSpeedFromMetersToKilometers(double from)
		{
			return from * 3600 / 1000;
		}
	}
	/// <summary>
	/// Класс агрегатор для возвращения из моей программы всех необхоимых данных, для отрисовки графиков
	/// </summary>
	public class SolutionResult
	{
		/// <summary>
		/// Скорость автомобиля с адаптивным круиз-контролем на протяжении всей работы алгоритма
		/// Массив скоростей в каждый момент времени
		/// </summary>
		public List<double> OwnSpeeds { get; set; }
		/// <summary>
		/// Скорость впереди идущего автомобиля на протяжении всей работы алгорима
		/// Массив скоростей в каждый момент времени
		/// </summary>
		public List<double> EntrySpeeds { get; set; }
		/// <summary>
		/// Расстояние до впереди идущего автомобиля на протяжении всей работы алгоритма
		/// Массив расстояний в каждый момент времени
		/// </summary>
		public List<double> Distances { get; set; }
		/// <summary>
		/// Ускорение автомобиля с адаптивным круиз-контролем на протяжении всей рабоыт алгоритма
		/// Массив ускорений в каждый момент времени
		/// </summary>
		public List<double> Accelerations { get; set; }

		/// <summary>
		/// Тривиальный конструктор, каждому свойству присваивается соответствующий аргумент-список из параметров конструктора
		/// </summary>
		/// <param name="ownspeeds">массив изменения скорости автомобиля с адаптивным круиз-контролем на протяжении всей работы алгоритма</param>
		/// <param name="entryspeeds">массив изменения скорости впереди идущего автомобиля на протяжении всей работы алгоритма</param>
		/// <param name="distances">массив изменения расстояния до впереди идущего автомобиля на протяжении всей работы алгоритма</param>
		/// <param name="accelerations">массив изменения ускорения автомобиля с адаптивным круиз-контролем на протяжении всей работы алгоритма</param>
		public SolutionResult(List<double> ownspeeds, List<double> entryspeeds, List<double> distances, List<double> accelerations)
		{
			OwnSpeeds = ownspeeds;
			EntrySpeeds = entryspeeds;
			Distances = distances;
			Accelerations = accelerations;
		}
	}
}
