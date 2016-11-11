using System;
using System.Collections.Generic;
using System.Globalization;

namespace Diplom1
{
	public class Program
	{
		static void Main(string[] args)
		{
			// dS --- расстояние между автомобилями; V - скорость
			// Первые 7 параметров - особые точки для dS (в метрах).
			// Оставшиеся 3 параметра - особые точки для V, используемые при вычислении среднего центра
			// задаются в м/с.
			var s = new Solution(-0.2m, -0.1m, -0.05m, 0m, 0.05m, 0.1m, 0.2m, 5.5m, 16.7m, 25m);
			s.ToSolve();
			Console.ReadKey();
		}
	}

	public class Solution
	{
		// Особые точки для dS
		private readonly decimal _x1;
		private readonly decimal _x2;
		private readonly decimal _x3;
		private readonly decimal _x4;
		private readonly decimal _x5;
		private readonly decimal _x6;
		private readonly decimal _x7;
		// Особые точки для V
		private readonly decimal _y1;
		private readonly decimal _y2;
		private readonly decimal _y3;

		// Скорость в (м/с)
		private decimal _mySpeed = 16.7m;
		// Цель движется равномерно
		private decimal _entrySpeed = 16.7m;
		// Требуемая дистанция в (м)
		private decimal _perfectDistance = 30m;
		// Текущая дистанция в (м)
		private decimal _currentDistance = 300m;
		// Частота вычислений: 1 (сек)
		private decimal _time = 1m;
		private Dictionary<String, decimal> _rules = new Dictionary<string, decimal>();

		public Solution(decimal x1, decimal x2, decimal x3, decimal x4, decimal x5, decimal x6, decimal x7,
					decimal y1, decimal y2, decimal y3)
		{
			_x1 = x1;
			_x2 = x2;
			_x3 = x3;
			_x4 = x4;
			_x5 = x5;
			_x6 = x6;
			_x7 = x7;

			_y1 = y1;
			_y2 = y2;
			_y3 = y3;

			Console.WriteLine("Введите скорость преследуемого автомобиля (в км/ч):");
			_entrySpeed = Decimal.Parse(Console.ReadLine(), CultureInfo.InvariantCulture) * 1000 / 3600;
			Console.WriteLine("Введите скорость Вашего автомобиля (в км/ч):");
			_mySpeed = Decimal.Parse(Console.ReadLine(), CultureInfo.InvariantCulture) * 1000 / 3600;
			Console.WriteLine("Введите дистанцию, которую требуется соблюдать (в метрах):");
			_perfectDistance = Decimal.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
			Console.WriteLine("Введите расстояние до преследуемого автомобиля (в метрах):");
			_currentDistance = Decimal.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
		}

		private decimal _closeDistance(decimal x)
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

		private decimal _zeroDistance(decimal x)
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

		private decimal _farDistance(decimal x)
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

		public void ToSolve()
		{
			decimal dist;
			for (int i = 0; i < 200; ++i)
			{
				dist = (_currentDistance - _perfectDistance)/_perfectDistance;
				#region Fuzzification
				// Три области фазиффикации: близко (прямоуг трапеция), средне (треугольник), далеко (прямоуг. трапеция)
				decimal A = _closeDistance(dist);
				decimal B = _zeroDistance(dist);
				decimal C = _farDistance(dist);
				Console.WriteLine($"Близко:{A}; Средне: {B}; Далеко: {C}");
				#endregion
				#region InferenceRule
				// Правило вывода: прямое соответствие расстояние - скорость
				// dS	V
				// -	-
				// 0	0
				// +	+
				_rules["Close"] = A;
				_rules["Zero"] = B;
				_rules["Far"] = C;
				#endregion
				#region Defuzzification
				// Дефазиффикация осуществляется методом Среднего Центра
				 _mySpeed = (_y1 * _rules["Close"]
								 + _y2 * _rules["Zero"]
								 + _y3 * _rules["Far"]) / (_rules["Close"] + _rules["Zero"] + _rules["Far"]);
				#endregion

				_currentDistance = _currentDistance - _time * (_mySpeed - _entrySpeed);
				Console.WriteLine($"Новая скорость: {Decimal.Round(_mySpeed * 3600 / 1000, 4)} (км/ч); Расстояние: {Decimal.Round(_currentDistance, 4)} (м)");
			}
		}
	}
}
