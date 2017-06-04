using System;
using System.IO;
using NPlot.Bitmap;
using System.Drawing;
using System.Reflection;

namespace Diplom1
{
	public class StringValueAttribute : Attribute
	{
		public string StringValue { get; protected set; }

		public StringValueAttribute(string value)
		{
			this.StringValue = value;
		}
	}

	public enum TypeAction
	{
		[StringValue("Ускорение")]
		Acceleration,
		[StringValue("Плавное маневрирование")]
		Smooth,
		[StringValue("Резкое маневрирование")]
		Sharp,
		[StringValue("Торможение")]
		Braking
	}

	public enum TypeMeasure
	{
		[StringValue("Скорость")]
		Speed,
		[StringValue("Дистанция")]
		Distance
	}

	/// <summary>
	/// Класс для рисования графиков
	/// </summary>
	internal static class Drawing
	{
		private static PlotSurface2D _npSurface = new PlotSurface2D(700, 500);

		internal static Font TitleFont { get; set; } = new Font("Arial", 12);
		internal static Font AxisFont { get; set; } = new Font("Arial", 10);
		internal static Font TickFont { get; set; } = new Font("Arial", 8);
		/// <summary>
		/// Директория, в которой будут сохраняться графики
		/// </summary>
		internal static string FolderPath { get; set; } = @"./Plots";
		/// <summary>
		/// Функция рисования графиков
		/// </summary>
		/// <param name="time">массив моментов времени (ось X)</param>
		/// <param name="valEntry">массив значений функции № 1 (ось Y)</param>
		/// <param name="valAlgo">массив значений функции № 2 (ось Y)</param>
		/// <param name="ta">Тип поведения впереди идущего автомобиля</param>
		/// <param name="tm">Тип измерения, которое отображает график функции</param>
		internal static void ToDraw(int[] time, double[] valEntry, double[] valAlgo, TypeAction ta, TypeMeasure tm)
		{
			_npSurface.Clear();
			_npSurface.Title = $"Поведение - {ta.GetStringValue()}, измеряемая величина - {tm.GetStringValue()}";
			_npSurface.BackColor = Color.White;

			NPlot.Grid grid = new NPlot.Grid();
			_npSurface.Add(grid, NPlot.PlotSurface2D.XAxisPosition.Bottom,
							NPlot.PlotSurface2D.YAxisPosition.Left);

			if (tm == TypeMeasure.Distance)
			{
				NPlot.LinePlot plot = new NPlot.LinePlot();

				plot.AbscissaData = time;
				plot.DataSource = valAlgo;
				plot.Label = "Алгоритм";
				plot.Color = Color.Blue;

				_npSurface.Add(plot, NPlot.PlotSurface2D.XAxisPosition.Bottom,
								NPlot.PlotSurface2D.YAxisPosition.Left);
			}
			else
			{
				NPlot.LinePlot plotAlgo = new NPlot.LinePlot();
				NPlot.LinePlot plotEntry = new NPlot.LinePlot();

				plotEntry.AbscissaData = time;
				plotEntry.DataSource = valEntry;
				plotEntry.Label = "Цель";
				plotEntry.Color = Color.Red;

				_npSurface.Add(plotEntry, NPlot.PlotSurface2D.XAxisPosition.Bottom,
								NPlot.PlotSurface2D.YAxisPosition.Left);

				plotAlgo.AbscissaData = time;
				plotAlgo.DataSource = valAlgo;
				plotAlgo.Label = "Алгоритм";
				plotAlgo.Color = Color.Blue;

				_npSurface.Add(plotAlgo, NPlot.PlotSurface2D.XAxisPosition.Bottom,
								NPlot.PlotSurface2D.YAxisPosition.Left);
			}

			_npSurface.XAxis1.Label = "Время (с)";
			_npSurface.XAxis1.NumberFormat = "{0:##0}";
			_npSurface.XAxis1.LabelFont = AxisFont;
			_npSurface.XAxis1.TickTextFont = TickFont;

			_npSurface.YAxis1.Label = tm == TypeMeasure.Speed ? "Скорость (км/ч)" : "Дистанция (м)";
			_npSurface.YAxis1.NumberFormat = "{0:##0.0}";
			_npSurface.YAxis1.LabelFont = AxisFont;
			_npSurface.YAxis1.TickTextFont = TickFont;


			NPlot.Legend npLegend = new NPlot.Legend();

			npLegend.AttachTo(NPlot.PlotSurface2D.XAxisPosition.Top,
					 NPlot.PlotSurface2D.YAxisPosition.Right);
			npLegend.VerticalEdgePlacement = NPlot.Legend.Placement.Inside;
			npLegend.HorizontalEdgePlacement = NPlot.Legend.Placement.Outside;
			npLegend.BorderStyle = NPlot.LegendBase.BorderType.Line;
			_npSurface.Legend = npLegend;

			_npSurface.Refresh();

			try
			{
				if (!Directory.Exists(FolderPath))
				{
					DirectoryInfo di = Directory.CreateDirectory(FolderPath);
					Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(FolderPath));
				}
				var files = Directory.GetFiles($"{FolderPath}/", $"*plot-{ta}-{tm}*.png");
				_npSurface.Bitmap.Save($"{FolderPath}/plot-{ta}-{tm}-{files.Length}.png");
			}
			catch (Exception e)
			{
				Console.WriteLine("The process failed: {0}", e.ToString());
			}
		}

		public static string GetStringValue(this Enum value)
		{
			// Get the type
			Type type = value.GetType();

			// Get fieldinfo for this type
			FieldInfo fieldInfo = type.GetField(value.ToString());

			// Get the stringvalue attributes
			StringValueAttribute[] attribs = fieldInfo.GetCustomAttributes(
					typeof(StringValueAttribute), false) as StringValueAttribute[];

			// Return the first if there was a match.
			return attribs.Length > 0 ? attribs[0].StringValue : null;
		}
	}
}
