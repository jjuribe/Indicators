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
		
		private struct TradeResults
		{
			/// Date, Ticker, profit, exit name, profit factor, Consecutive losers, largest loser, largest winner, profit per month
			public  string 	date 	{ get; set; }
			public  string 	ticker 	{ get; set; }
			public  double 	profit 	{ get; set; }
//			public  string 	exitName 	{ get; set; }
//			public  double 	profitFactor 	{ get; set; }
			
//			public  double 	consecutiveLosers 	{ get; set; }
//			public  double 	largestLoser 	{ get; set; }
//			public  double 	largestWinner 	{ get; set; }
//			public  double 	profitPerMonth 	{ get; set; }
		}
		
		private TradeResults tradeResults = new TradeResults{};
		private List<TradeResults> allTradeResults = new List<TradeResults>();
		private int  fileCount;
		private int  filesParsedCount;
		
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
				fileNames = getFileNames();
				readAllofThe(files: fileNames);
			}
		}

		protected override void OnBarUpdate()
		{
		}
		
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Create Struct from line
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		private void createStructFrom(string[] oneFile, bool debug) {

			foreach (String rows in oneFile) {		
				//Print(lines);
				var columns = rows.Split(',');
				if ( debug ) { Print(columns[0] +"\t"+ columns[1] +"\t"+ columns[2] +"\t"); }
				tradeResults.date = columns[0];
				tradeResults.ticker = columns[1];
				tradeResults.profit = Convert.ToDouble( columns[2]);
				allTradeResults.Add(tradeResults);
				
			}
			
			filesParsedCount += 1;
			
			///show struct array when filished
			if ( filesParsedCount == fileCount ) {
				Print("\n"+ filesParsedCount + " of " + fileCount + " ticker backtest files parsed\n");
				Print("\nFinished with allTradeResults...");
				foreach (TradeResults thing in allTradeResults) {	
					Print(thing.date +" \t\t\t"+ thing.ticker +"\t\t\t"+ thing.profit);
				}
			}
		}
	
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Get files in folder
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		private void readAllofThe(string[] files) {
			
			Print("--- Files: ---");
			foreach (string name in fileNames)
	        {
	            Print(name);
				readAllLinesFrom(filePath: name, debug: false);
	        }
		}

		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Get files in folder
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		private string[] getFileNames() {
			
			var dateTime = DateTime.Today.ToString("MM_dd_yyyy") ;
			var filePath = systemPath+ @"\Channel"+"_"+ dateTime;
			// Put all file names in root directory into array.
	        string[] array1 = Directory.GetFiles(filePath);
			fileCount = array1.Length; 
			return array1;
		}
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Read entire CSV 
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		private void readAllLinesFrom(string filePath, bool debug) {
			
			try 
			{
				string[] oneFile = System.IO.File.ReadAllLines(filePath);
				createStructFrom(oneFile: oneFile, debug: false);
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
