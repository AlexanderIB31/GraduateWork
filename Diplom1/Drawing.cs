using System;
using System.IO;
using NPlot.Bitmap;
using System.Drawing;

namespace Diplom1
{
	public class StringValueAttribute : Attribute
	{
		private string _value;

		public StringValueAttribute(string value)
		{
			_value = value;
		}

		public string Value {
			get
			{
				return _value;
			}
		}
	}

	public enum TypeAction
	{
		[StringValue("Acceleration")]
		Acceleration,
		[StringValue("Smooth")]
		Smooth,
		[StringValue("Braking")]
		Braking
	}

	public enum TypeMeasure
	{
		[StringValue("Speed")]
		Speed,
		[StringValue("Distance")]
		Distance
	}

	internal static class Drawing
	{
		private static PlotSurface2D _npSurface = new PlotSurface2D(700, 500);

		public static Font TitleFont { get; set; } = new Font("Arial", 12);
		public static Font AxisFont { get; set; } = new Font("Arial", 10);
		public static Font TickFont { get; set; } = new Font("Arial", 8);
		public static string Path { get; set; } = @"./Plots";

		internal static void ToDraw(int[] time, double[] valEntry, double[] valAlgo, TypeAction ta, TypeMeasure tm)
		{
			_npSurface.Clear();
			_npSurface.Title = $"{tm} : {ta}";
			_npSurface.BackColor = Color.White;

			NPlot.Grid grid = new NPlot.Grid();
			_npSurface.Add(grid, NPlot.PlotSurface2D.XAxisPosition.Bottom,
						  NPlot.PlotSurface2D.YAxisPosition.Left);

			if (tm == TypeMeasure.Distance)
			{
				NPlot.LinePlot plot = new NPlot.LinePlot();

				plot.AbscissaData = time;
				plot.DataSource = valAlgo;
				plot.Label = "Algorithm";
				plot.Color = Color.Blue;

				_npSurface.Add(plot, NPlot.PlotSurface2D.XAxisPosition.Bottom,
							  NPlot.PlotSurface2D.YAxisPosition.Left);
			}
			else
			{
				NPlot.LinePlot plotAlgo = new NPlot.LinePlot();
				NPlot.LinePlot plotEntry = new NPlot.LinePlot();

				plotAlgo.AbscissaData = time;
				plotAlgo.DataSource = valAlgo;
				plotAlgo.Label = "Algorithm";
				plotAlgo.Color = Color.Blue;

				_npSurface.Add(plotAlgo, NPlot.PlotSurface2D.XAxisPosition.Bottom,
							  NPlot.PlotSurface2D.YAxisPosition.Left);


				plotEntry.AbscissaData = time;
				plotEntry.DataSource = valEntry;
				plotEntry.Label = "Entry";
				plotEntry.Color = Color.Red;

				_npSurface.Add(plotEntry, NPlot.PlotSurface2D.XAxisPosition.Bottom,
							  NPlot.PlotSurface2D.YAxisPosition.Left);
			}

			_npSurface.XAxis1.Label = "Time";
			_npSurface.XAxis1.NumberFormat = "{0:##0}";
			_npSurface.XAxis1.LabelFont = AxisFont;
			_npSurface.XAxis1.TickTextFont = TickFont;

			_npSurface.YAxis1.Label = $"{tm}";
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
				if (!Directory.Exists(Path))
				{
					DirectoryInfo di = Directory.CreateDirectory(Path);
					Console.WriteLine("The directory was created successfully at {0}.", Directory.GetCreationTime(Path));
				}
				var files = Directory.GetFiles($"{Path}/", $"*plot-{ta}-{tm}*.png");
				_npSurface.Bitmap.Save($"{Path}/plot-{ta}-{tm}-{files.Length}.png");
			}
			catch (Exception e)
			{
				Console.WriteLine("The process failed: {0}", e.ToString());
			}
		}
	}
}
