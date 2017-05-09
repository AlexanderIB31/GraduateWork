using System;
using System.Collections.Generic;

namespace Diplom1
{
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
		// Особые точки для dV
		private static double _z1;
		private static double _z2;
		private static double _z3;
		private static double _z4;
		private static double _z5;
		private static double _z6;
		private static double _z7;
		// Особые точки для V*
		private static double _y1;
		private static double _y2;
		private static double _y3;
		private static double _y4;
		private static double _y5;
		#endregion

		// Минимальное расстояние до цели
		private readonly static double _eps = 3;
		// Скорость в (м/с)
		private static double _mySpeed = 0;
		// Цель движется неравномерно
		private static double _entrySpeed = 16.7;
		// Требуемая дистанция в (м)
		private static double _perfectDistance = 30;
		// Текущая дистанция в (м)
		private static double _currentDistance = 300;
		// Частота вычислений: 1 (сек)
		private static double _tau = 1;
		// Коэф. при вычислении ускорения
		private static double _lambda = 0.98;

		// Таблица правил вывода
		private static Dictionary<string, double> _rules = new Dictionary<string, double>();

		public static void SetParams(double[] args, double perfectDist, double curDist, double mySpeed, double entrySpeed)
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

				_z1 = args[7];
				_z2 = args[8];
				_z3 = args[9];
				_z4 = args[10];
				_z5 = args[11];
				_z6 = args[12];
				_z7 = args[13];

				_y1 = args[14];
				_y2 = args[15];
				_y3 = args[16];
				_y4 = args[17];
				_y5 = args[18];

				_perfectDistance = perfectDist;
				_currentDistance = curDist;
				_mySpeed = mySpeed;
				_entrySpeed = entrySpeed;
				_lambda = args[19];
			}
			catch (ArgumentOutOfRangeException ex)
			{
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine("Ошибка: проверьте кол-во передаваемых в args параметров, их должно быть 20!");
				Console.ResetColor();
				throw ex;
			}
		}

		#region Фаззификация для Дистанции
		private static double CloseDistance(double x)
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

		private static double ZeroDistance(double x)
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

		private static double FarDistance(double x)
		{
			if (_x5 <= x && x <= _x7)
			{
				return (x - _x5) / (_x7 - _x5);
			}
			if (_x7 <= x)
			{
				return 1;
			}
			return 0;
		}
		#endregion
		#region Фаззификация для Скорости
		private static double LessSpeed(double z)
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

		private static double ZeroSpeed(double z)
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

		private static double MoreSpeed(double z)
		{
			if (_z5 <= z && z <= _z7)
			{
				return (z - _z5) / (_z7 - _z5);
			}
			if (_z7 <= z)
			{
				return 1;
			}
			return 0;
		}
		#endregion

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
				_entrySpeed = CalcEntrySpeed(ta);

				entrySpeeds.Add(_entrySpeed * 3600 / 1000);
				Time.Add(i);

				var accel = ToSolveNow();
				_mySpeed = Math.Max(_mySpeed + _tau * accel, 0);
				Console.ForegroundColor = ConsoleColor.Cyan;
				Console.WriteLine($"Speed: {_mySpeed}");
				Console.ResetColor();
				if (_currentDistance - _tau * (_mySpeed - _entrySpeed) <= _eps)
				{
					_mySpeed = 0;
				}

				_currentDistance -= _tau * (_mySpeed - _entrySpeed);

				ownSpeeds.Add(_mySpeed * 3600 / 1000);
				distances.Add(_currentDistance);
				accelerations.Add(accel);
			}

			return new SolutionResult(ownSpeeds, entrySpeeds, distances, accelerations);
		}

		public static double ToSolveNow()
		{
			var accel = MamdaniSchema();
			return accel;
		}

		private static double MamdaniSchema()
		{
			var deltaDistance = _perfectDistance != 0 ? (_currentDistance - _perfectDistance) / _perfectDistance : 0;
			var deltaSpeed = _entrySpeed != 0 ? (_mySpeed - _entrySpeed) / _entrySpeed : 0;

			#region Fuzzification
			// Три области фазиффикации: близко (прямоуг трапеция), средне (треугольник), далеко (прямоуг. трапеция)
			double A = CloseDistance(deltaDistance);
			double B = ZeroDistance(deltaDistance);
			double C = FarDistance(deltaDistance);
			double D = LessSpeed(deltaSpeed);
			double E = ZeroSpeed(deltaSpeed);
			double F = MoreSpeed(deltaSpeed);
			#endregion
			#region InferenceRule
			// Правило вывода: прямое соответствие расстояние и скорость - коэффициент лямбда
			// ++ - Сильно Увеличить
			// -- - Сильно Снизить
			// + - Немного Увеличить
			// - - Немного Уменьшить
			// 0 - Ничего не делать
			// dS\dV	-	0	+
			// -		0	-	--
			// 0		+	0	-
			// +		++	+	0
			_rules["CloseDist"] = A;
			_rules["ZeroDist"] = B;
			_rules["FarDist"] = C;
			_rules["LessSpeed"] = D;
			_rules["ZeroSpeed"] = E;
			_rules["MoreSpeed"] = F;
			#endregion
			#region Defuzzification
			// Дефазиффикация осуществляется методом Среднего Центра (центроидный метод)
			var resAccel = (_y3 * Math.Min(_rules["CloseDist"], _rules["LessSpeed"])
						   + _y2 * Math.Min(_rules["CloseDist"], _rules["ZeroSpeed"])
						   + _y1 * Math.Min(_rules["CloseDist"], _rules["MoreSpeed"])
							+ _y4 * Math.Min(_rules["ZeroDist"], _rules["LessSpeed"])
							+ _y3 * Math.Min(_rules["ZeroDist"], _rules["ZeroSpeed"])
							+ _y2 * Math.Min(_rules["ZeroDist"], _rules["MoreSpeed"])
							+ _y5 * Math.Min(_rules["FarDist"], _rules["LessSpeed"])
							+ _y4 * Math.Min(_rules["FarDist"], _rules["ZeroSpeed"])
							+ _y3 * Math.Min(_rules["FarDist"], _rules["MoreSpeed"]))
					   / (Math.Min(_rules["CloseDist"], _rules["LessSpeed"])
						   + Math.Min(_rules["CloseDist"], _rules["ZeroSpeed"])
						   + Math.Min(_rules["CloseDist"], _rules["MoreSpeed"])
						   + Math.Min(_rules["ZeroDist"], _rules["LessSpeed"])
						   + Math.Min(_rules["ZeroDist"], _rules["ZeroSpeed"])
						   + Math.Min(_rules["ZeroDist"], _rules["MoreSpeed"])
						   + Math.Min(_rules["FarDist"], _rules["LessSpeed"])
						   + Math.Min(_rules["FarDist"], _rules["ZeroSpeed"])
						   + Math.Min(_rules["FarDist"], _rules["MoreSpeed"]));
			#endregion
			Console.ForegroundColor = ConsoleColor.Magenta;
			Console.WriteLine($"Accel: {resAccel}");
			Console.ResetColor();
			return resAccel;
		}

		private static double SugenoSchema()
		{
			return 0;
		}
	
		private static double CalcEntrySpeed(TypeAction ta)
		{
			double val = _entrySpeed;
			switch (ta)
			{
				case TypeAction.Acceleration:
					val = Math.Min(val + 5, 50);
					break;
				case TypeAction.Braking:
					val = Math.Max(val - 7, 0);
					break;
				case TypeAction.Smooth:
					val = Math.Max(Math.Min(Math.Sin(val) * 3 + val, 50), 0);
					break;
			}
			return val;
		}
	}

	public class SolutionResult
	{
		public List<double> OwnSpeeds { get; set; }
		public List<double> EntrySpeeds { get; set; }
		public List<double> Distances { get; set; }
		public List<double> Accelerations { get; set; }

		public SolutionResult(List<double> a, List<double> b, List<double> c, List<double> d)
		{
			OwnSpeeds = a;
			EntrySpeeds = b;
			Distances = c;
			Accelerations = d;
		}
	}

}
