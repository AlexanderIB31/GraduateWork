using System;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using NPlot;

namespace Diplom1
{
	public class Program
	{
		static public void CreateLineGraph(int[] X, decimal[] Y, string name)
		{
			NPlot.Bitmap.PlotSurface2D npSurface = new NPlot.Bitmap.PlotSurface2D(700, 500);

			NPlot.LinePlot npPlot1 = new LinePlot();

			//Font definitions:
			Font TitleFont = new Font("Arial", 12);
			Font AxisFont = new Font("Arial", 10);
			Font TickFont = new Font("Arial", 8);

			//Legend definition:
			NPlot.Legend npLegend = new NPlot.Legend();

			//Prepare PlotSurface:
			npSurface.Clear();
			npSurface.Title = "Movement";
			npSurface.BackColor = System.Drawing.Color.White;

			//Left Y axis grid:
			NPlot.Grid p = new Grid();
			npSurface.Add(p, NPlot.PlotSurface2D.XAxisPosition.Bottom,
						  NPlot.PlotSurface2D.YAxisPosition.Left);

			//Weight:
			npPlot1.AbscissaData = X;
			npPlot1.DataSource = Y;
			npPlot1.Label = name;
			npPlot1.Color = System.Drawing.Color.Blue;

			npSurface.Add(npPlot1, NPlot.PlotSurface2D.XAxisPosition.Bottom,
						  NPlot.PlotSurface2D.YAxisPosition.Left);

			//X axis
			npSurface.XAxis1.Label = "Time";
			npSurface.YAxis1.NumberFormat = "{0:####0}";
			//npSurface.XAxis1.TicksLabelAngle = 90;
			//npSurface.XAxis1.TickTextNextToAxis = true;
			//npSurface.XAxis1.FlipTicksLabel = true;
			//npSurface.XAxis1.LabelOffset = 110;
			//npSurface.XAxis1.LabelOffsetAbsolute = true;
			npSurface.XAxis1.LabelFont = AxisFont;
			npSurface.XAxis1.TickTextFont = TickFont;

			//Y axis
			npSurface.YAxis1.Label = name;
			npSurface.YAxis1.NumberFormat = "{0:####0.0}";
			npSurface.YAxis1.LabelFont = AxisFont;
			npSurface.YAxis1.TickTextFont = TickFont;

			//Add legend:
			npLegend.AttachTo(NPlot.PlotSurface2D.XAxisPosition.Top,
					 NPlot.PlotSurface2D.YAxisPosition.Right);
			npLegend.VerticalEdgePlacement = NPlot.Legend.Placement.Inside;
			npLegend.HorizontalEdgePlacement = NPlot.Legend.Placement.Outside;
			npLegend.BorderStyle = NPlot.LegendBase.BorderType.Line;
			npSurface.Legend = npLegend;

			//Update PlotSurface:
			npSurface.Refresh();

			npSurface.Bitmap.Save($"C:\\Users\\Alex31\\Desktop\\graph_{name}.png");
		}

		static void Main(string[] args)
		{
			Console.WriteLine(@"Укажите время работы алгоритма (мс):");
			var ms = int.Parse(Console.ReadLine(), CultureInfo.InvariantCulture);
			// dS --- расстояние между автомобилями; V - скорость
			// Первые 7 параметров - особые точки для dS (в метрах).
			// Оставшиеся 3 параметра - особые точки для V, используемые при вычислении среднего центра
			// задаются в м/с.
			var s = new Solution(-0.2m, -0.1m, -0.05m, 0m, 0.05m, 0.1m, 0.2m,
								-0.2m, -0.1m, -0.05m, 0m, 0.05m, 0.1m, 0.2m,
								-1.3m, -0.3m, 0m, 0.3m, 1.3m);
			s.ToSolve(ms);
			Console.WriteLine("Вычисления выполнены. Графики построены. Нажмите ENTER...");
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
		// Особые точки для dV
		private readonly decimal _z1;
		private readonly decimal _z2;
		private readonly decimal _z3;
		private readonly decimal _z4;
		private readonly decimal _z5;
		private readonly decimal _z6;
		private readonly decimal _z7;
		// Особые точки для lambda
		private readonly decimal _y1;
		private readonly decimal _y2;
		private readonly decimal _y3;
		private readonly decimal _y4;
		private readonly decimal _y5;

		// Скорость в (м/с)
		private decimal _mySpeed = 16.7m;
		// Цель движется равномерно
		private decimal _entrySpeed = 16.7m;
		// Требуемая дистанция в (м)
		private decimal _perfectDistance = 30m;
		// Текущая дистанция в (м)
		private decimal _currentDistance = 300m;
		// Частота вычислений: 1 (сек)
		private decimal _teta = 1m;
		private Dictionary<String, decimal> _rules = new Dictionary<string, decimal>();

		public Solution(decimal x1, decimal x2, decimal x3, decimal x4, decimal x5, decimal x6, decimal x7,
						decimal z1, decimal z2, decimal z3, decimal z4, decimal z5, decimal z6, decimal z7,
					decimal y1, decimal y2, decimal y3, decimal y4, decimal y5)
		{
			_x1 = x1;
			_x2 = x2;
			_x3 = x3;
			_x4 = x4;
			_x5 = x5;
			_x6 = x6;
			_x7 = x7;

			_z1 = z1;
			_z2 = z2;
			_z3 = z3;
			_z4 = z4;
			_z5 = z5;
			_z6 = z6;
			_z7 = z7;

			_y1 = y1;
			_y2 = y2;
			_y3 = y3;
			_y4 = y4;
			_y5 = y5;

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

		private decimal _lessSpeed(decimal z)
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

		private decimal _zeroSpeed(decimal z)
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

		private decimal _moreSpeed(decimal z)
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

		public void ToSolve(int time)
		{
			List<int> Time = new List<int>();
			List<decimal> Speed = new List<decimal>();
			List<decimal> Distance = new List<decimal>();

			decimal deltaDistance, deltaSpeed;
			for (int i = 0; i < time; ++i)
			{
				deltaDistance = (_currentDistance - _perfectDistance) / _perfectDistance;
				deltaSpeed = (_mySpeed - _entrySpeed) / _entrySpeed;
				#region Fuzzification
				// Три области фазиффикации: близко (прямоуг трапеция), средне (треугольник), далеко (прямоуг. трапеция)
				decimal A = _closeDistance(deltaDistance);
				decimal B = _zeroDistance(deltaDistance);
				decimal C = _farDistance(deltaDistance);
				decimal D = _lessSpeed(deltaSpeed);
				decimal E = _zeroSpeed(deltaSpeed);
				decimal F = _moreSpeed(deltaSpeed);
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
				// Дефазиффикация осуществляется методом Среднего Центра
				 var lambda = (_y3 * Math.Min(_rules["CloseDist"], _rules["LessSpeed"])
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

				var a = lambda * Math.Abs(_mySpeed - _entrySpeed);
				_mySpeed += _teta * a;
				_currentDistance = _currentDistance - _teta * (_mySpeed - _entrySpeed);
				Time.Add(i);
				Speed.Add(Decimal.Round(_mySpeed * 3600 / 1000, 4));
				Distance.Add(Decimal.Round(_currentDistance, 4));
				//Console.WriteLine($"Новая скорость: {Decimal.Round(_mySpeed * 3600 / 1000, 4)} (км/ч); Расстояние: {Decimal.Round(_currentDistance, 4)} (м)");
			}
			Program.CreateLineGraph(Time.ToArray(), Speed.ToArray(), "Speed");
			Program.CreateLineGraph(Time.ToArray(), Distance.ToArray(), "Distination");
		}
	}
}
