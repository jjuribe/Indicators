#region Using declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Gui.Tools;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using System.IO;
using System.Windows.Forms;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class LoadCSV : Indicator
	{
		
		private string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		private string[] fileNames;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Load CSV";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				AddPlot(Brushes.DodgerBlue, "Profit");
				AddPlot(new Stroke(Brushes.LightSkyBlue, 2), PlotStyle.Bar, "Cost");
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				ClearOutputWindow(); 
				fileNames = getTickerNames();
				loopThrough(files: fileNames);
				
				
			}
		}

		protected override void OnBarUpdate()
		{
			//getTickerNames();
			//loadCSV(ticker: "AAPL");
		}
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Get files in folder
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		private void loopThrough(string[] files) {
			
			Print("--- Files: ---");
			foreach (string name in fileNames)
	        {
	            Print(name);
				loadTickersFrom(filePath: name);
	        }
		}

		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Get files in folder
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		private string[] getTickerNames() {
			
			var dateTime = DateTime.Today.ToString("MM_dd_yyyy") ;
			var filePath = systemPath+ @"\Channel"+"_"+ dateTime;
			
			// Put all file names in root directory into array.
	        string[] array1 = Directory.GetFiles(filePath);
			return array1;
		}
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									CSV Import
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		private void loadTickersFrom(string filePath) {
			
			//var dateTime = DateTime.Today.ToString("MM_dd_yyyy") ;
			//var filePath = systemPath+ @"\Channel"+"_"+ dateTime + @"\"+  ticker  + ".csv";
			
			String line;
			try 
			{
				//Pass the file path and file name to the StreamReader constructor
				StreamReader sr = new StreamReader(filePath);

				//Read the first line of text
				line = sr.ReadLine();

				//Continue to read until you reach end of file
				while (line != null) 
				{
					//write the lie to console window
					Print(line);
					//Read the next line
					line = sr.ReadLine();
				}

				//close the file
				sr.Close();
				Console.ReadLine();
				}
			catch(Exception e)
			{
				Print("Exception: " + e.Message);
			}
			   finally 
			{
				Print("Executing finally block.");
			}
			
		}
		
		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Profit
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Cost
		{
			get { return Values[1]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private LoadCSV[] cacheLoadCSV;
		public LoadCSV LoadCSV()
		{
			return LoadCSV(Input);
		}

		public LoadCSV LoadCSV(ISeries<double> input)
		{
			if (cacheLoadCSV != null)
				for (int idx = 0; idx < cacheLoadCSV.Length; idx++)
					if (cacheLoadCSV[idx] != null &&  cacheLoadCSV[idx].EqualsInput(input))
						return cacheLoadCSV[idx];
			return CacheIndicator<LoadCSV>(new LoadCSV(), input, ref cacheLoadCSV);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LoadCSV LoadCSV()
		{
			return indicator.LoadCSV(Input);
		}

		public Indicators.LoadCSV LoadCSV(ISeries<double> input )
		{
			return indicator.LoadCSV(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LoadCSV LoadCSV()
		{
			return indicator.LoadCSV(Input);
		}

		public Indicators.LoadCSV LoadCSV(ISeries<double> input )
		{
			return indicator.LoadCSV(input);
		}
	}
}

#endregion
