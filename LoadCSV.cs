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
			/// Trade Count, Date In, Date Out, Ticker, profit, cumProfit,
			public  int 	tradeNum 	{ get; set; }
			public  string 	dateEntry 	{ get; set; }
			public  string 	dateExit 	{ get; set; }
			public  string 	ticker 	{ get; set; }
			public  double 	profit 	{ get; set; }
			public  double 	cumProfit 	{ get; set; }
			
			/// exit name, profit factor, winPct, Consecutive losers, 
			public  string 	exitName 	{ get; set; }
			public  double 	profitFactor 	{ get; set; }
			public  double 	winPct 	{ get; set; }
			public  int 	consecutiveLosers 	{ get; set; }
			
			/// largest loser, largest winner, profit per month
			public  double 	largestLoser 	{ get; set; }
			public  double 	largestWinner 	{ get; set; }
			public  double 	profitPerMonth 	{ get; set; }
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
				//Print(rows);
				/// Trade Count, Date In, Date Out, Ticker, lastProfitCurrency, cumProfit, exit name, profit factor, winPct Consecutive losers, largest loser, largest winner, profit per month
				var columns = rows.Split(',');
				if ( debug ) { Print(
					columns[0] +"\t"+ 	// count
					columns[1] +"\t"+ 	// Date In
					columns[2] +"\t" + 	// Date out
					columns[3] +"\t"+ 	// Ticker
					columns[4] +"\t"+ 	// profit
					columns[5] +"\t" + 	// cumulative
					columns[6] +"\t" + 	// exit name
					columns[7] +"\t"+ 	// pf
					columns[8] +"\t"+ 	// winPct
					columns[9] +"\t" + 	// consec loser
					columns[10] +"\t"+	// LL
					columns[11] +"\t"+ 	// LW
					columns[12] +"\t"); } // monthlyProfit
				
				/*
				/// Trade Count, Date In, Date Out, Ticker, profit, cumProfit,
			public  int 	tradeNum 	{ get; set; }
			public  string 	dateEntry 	{ get; set; }
			public  string 	dateExit 	{ get; set; }
			public  string 	ticker 	{ get; set; }
			public  double 	profit 	{ get; set; }
			public  double 	cumProfit 	{ get; set; }
			
			/// exit name, profit factor, winPct, Consecutive losers, 
			public  string 	exitName 	{ get; set; }
			public  double 	profitFactor 	{ get; set; }
			public  double 	winPct 	{ get; set; }
			public  int 	consecutiveLosers 	{ get; set; }
			
			/// largest loser, largest winner, profit per month
			public  double 	largestLoser 	{ get; set; }
			public  double 	largestWinner 	{ get; set; }
			public  double 	profitPerMonth 	{ get; set; }
				*/
				tradeResults.tradeNum = Convert.ToInt16(columns[0]);
				tradeResults.dateEntry = columns[1];
				tradeResults.dateExit = columns[2];
				tradeResults.ticker = columns[3];		
				tradeResults.profit = Convert.ToDouble( columns[4]);
				tradeResults.cumProfit = Convert.ToDouble( columns[5]); 
				
				tradeResults.exitName = columns[6];
				tradeResults.profitFactor = Convert.ToDouble( columns[7]); 
				tradeResults.winPct = Convert.ToDouble( columns[8]); 		
				tradeResults.consecutiveLosers = Convert.ToInt16(columns[9]);

				tradeResults.largestLoser = Convert.ToDouble( columns[10]);
				tradeResults.largestWinner = Convert.ToDouble( columns[11]);
				tradeResults.profitPerMonth = Convert.ToDouble( columns[12]);
				
				allTradeResults.Add(tradeResults);
				
			}
			
			filesParsedCount += 1;
			
			///show struct array when filished
			if ( filesParsedCount == fileCount ) {
				Print("\n"+ filesParsedCount + " of " + fileCount + " ticker backtest files parsed\n");
				Print("\nFinished with allTradeResults...");
				foreach (TradeResults thing in allTradeResults) {	
					Print(thing.tradeNum +" \t\t\t"+ thing.dateEntry +"\t\t\t"+ thing.dateExit +" \t\t\t"+ thing.ticker +"\t\t\t"+ thing.profit + "\t\t\t"+ thing.cumProfit +
					" \t\t\t"+ thing.exitName +"\t\t\t"+ thing.profitFactor +" \t\t\t"+ thing.winPct +"\t\t\t"+ thing.consecutiveLosers +
					"\t\t\t"+ thing.largestLoser +" \t\t\t"+ thing.largestWinner +"\t\t\t"+ thing.profitPerMonth
					
					);
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
