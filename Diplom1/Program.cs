using System;
using System.Collections.Generic;
using System.Globalization;
using System.Drawing;
using NPlot;

namespace Algorithm
{
	public class Program
	{
		private static void CreateLineGraph(int[] X, double[] Y1, double[] Y2, string name)
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

			npPlot1.AbscissaData = X;
			npPlot1.DataSource = Y1;
			npPlot1.Label = name;
			npPlot1.Color = System.Drawing.Color.Blue;

			npSurface.Add(npPlot1, NPlot.PlotSurface2D.XAxisPosition.Bottom,
						  NPlot.PlotSurface2D.YAxisPosition.Left);

			if (Y2 != null)
			{
				NPlot.LinePlot npPlot2 = new LinePlot();
				npPlot2.AbscissaData = X;
				npPlot2.DataSource = Y2;
				npPlot2.Label = name + "_entry";
				npPlot2.Color = System.Drawing.Color.Red;
				npSurface.Add(npPlot2, NPlot.PlotSurface2D.XAxisPosition.Bottom,
							  NPlot.PlotSurface2D.YAxisPosition.Left);
			}

			//X axis
			npSurface.XAxis1.Label = "Time";
			npSurface.YAxis1.NumberFormat = "{0:####0}";
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

			npSurface.Bitmap.Save($"{AppDomain.CurrentDomain.BaseDirectory}\\..\\..\\..\\graph_{name}.png");
		}

		public static void Main(string[] args)
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
			// Считываем параметры для Solution из файла params.txt, сгенерированным проектом GA
			using (var fr = new System.IO.StreamReader($"{AppDomain.CurrentDomain.BaseDirectory}\\..\\..\\..\\params.txt"))
			{
				parameters = new List<double>(Array.ConvertAll(fr.ReadToEnd().TrimEnd('|').Split('|'), double.Parse));
			}
			// dS --- расстояние между автомобилями; 
			// dV --- разница скоростей;
			// Первые 7 параметров - особые точки для dS (в метрах);
			// Вторые 7 параметров - особые точки для dV (в м/c);
			// Оставшиеся 5 параметров - особые точки для V*
			var s = new Solution(parameters.ToArray(), perfectDist, curDist, mySpeed, entrySpeed);
			var res = s.ToSolve(ms);
			CreateLineGraph(time.ToArray(), res.OwnSpeeds.ToArray(), res.EntrySpeeds.ToArray(), "Speed");
			CreateLineGraph(time.ToArray(), res.Distances.ToArray(), null, "Distination");
			CreateLineGraph(time.ToArray(), res.Accelerations.ToArray(), null, "Acceleration");
			Console.WriteLine("Вычисления выполнены. Графики построены. Нажмите ENTER...");
			Console.ReadKey();
		}
	}

	public class Solution
	{
		// Особые точки для dS
		private readonly double _x1;
		private readonly double _x2;
		private readonly double _x3;
		private readonly double _x4;
		private readonly double _x5;
		private readonly double _x6;
		private readonly double _x7;
		// Особые точки для dV
		private readonly double _z1;
		private readonly double _z2;
		private readonly double _z3;
		private readonly double _z4;
		private readonly double _z5;
		private readonly double _z6;
		private readonly double _z7;
		// Особые точки для V*
		private readonly double _y1;
		private readonly double _y2;
		private readonly double _y3;
		private readonly double _y4;
		private readonly double _y5;

		// Скорость в (м/с)
		private double _mySpeed = 0;
		// Цель движется неравномерно
		private double _entrySpeed = 16.7;
		// Требуемая дистанция в (м)
		private double _perfectDistance = 30;
		// Текущая дистанция в (м)
		private double _currentDistance = 300;
		// Частота вычислений: 1 (сек)
		private double _teta = 1;
		// Коэф. при вычислении ускорения
		private double _lambda = 0.98;
		private Dictionary<string, double> _rules = new Dictionary<string, double>();

		public Solution(double[] args, double perfectDist, double curDist, double mySpeed, double entrySpeed)
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
				Console.WriteLine("! проверьте кол-во передаваемых в args параметров, их должно быть 20 !");
				throw ex;
			}
		}

		private double CloseDistance(double x)
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

		private double ZeroDistance(double x)
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

		private double FarDistance(double x)
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

		private double LessSpeed(double z)
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

		private double ZeroSpeed(double z)
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

		private double MoreSpeed(double z)
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

		public Result ToSolve(int time)
		{
			Random r = new Random();

			List<int> Time = new List<int>();
			List<double> ownSpeeds = new List<double>();
			List<double> entrySpeeds = new List<double>();
			List<double> distances = new List<double>();
			List<double> accelerations = new List<double>();

			double deltaDistance, deltaSpeed;
			for (int i = 0; i < time; ++i)
			{
				entrySpeeds.Add(_entrySpeed * 3600 / 1000);
				deltaDistance = (_currentDistance - _perfectDistance) / _perfectDistance;
				deltaSpeed = (_mySpeed - _entrySpeed) / _entrySpeed;
				// Дефазификация осуществляется по методу Мамдани
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
				// Дефазиффикация осуществляется методом Среднего Центра
				var resSpeed = (_y3 * Math.Min(_rules["CloseDist"], _rules["LessSpeed"])
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

				var a = _lambda * (resSpeed - _mySpeed);
				_mySpeed += _teta * a;
				_currentDistance -= _teta * (_mySpeed - _entrySpeed);
				Time.Add(i);
				ownSpeeds.Add(_mySpeed * 3600 / 1000);
				distances.Add(_currentDistance);
				accelerations.Add(a);
				_entrySpeed += (r.Next(-40, 40) * r.NextDouble());
				_entrySpeed = Math.Min(40, Math.Max(_entrySpeed, 5));
			}
			return new Result(ownSpeeds, entrySpeeds, distances, accelerations);
		}

		public double ToSolveNow(double entrySpeed, double mySpeed, double curDistance, double perfectDistance)
		{
			var deltaDistance = (curDistance - perfectDistance) / perfectDistance;
			var deltaSpeed = (mySpeed - entrySpeed) / entrySpeed;
			// Дефазификация осуществляется по методу Мамдани
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
			// Дефазиффикация осуществляется методом Среднего Центра
			var resSpeed = (_y3 * Math.Min(_rules["CloseDist"], _rules["LessSpeed"])
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

			return _lambda * (resSpeed - mySpeed);
		}
	}

	public class Result
	{
		public List<double> OwnSpeeds { get; set; }
		public List<double> EntrySpeeds { get; set; }
		public List<double> Distances { get; set; }
		public List<double> Accelerations { get; set; }

		public Result(List<double> a, List<double> b, List<double> c, List<double> d)
		{
			OwnSpeeds = a;
			EntrySpeeds = b;
			Distances = c;
			Accelerations = d;
		}
	}
}
