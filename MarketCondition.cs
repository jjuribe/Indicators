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
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class MarketCondition : Indicator
	{
		private SMA		sma;
		private StdDev	stdDev;
		private ATR		atr;
		private double 	sma0;
		private	double 	twoPctUp;
		private	double 	twoPctDn;
		private bool bear = false, bull = false, sideways = false;
		private bool volatil = false, normal = false, quiet = false;
		
		private Series<double> atrPctSeries;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Market Condition";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				AddPlot(Brushes.DarkGray, "Upper"); // Stored in Plots[0]
      			AddPlot(Brushes.DarkGray, "Lower");   // Stored in Plots[1]
			}
			else if (State == State.DataLoaded)
			{
				sma		= SMA(200);
				atr		= ATR(14);
				stdDev	= StdDev(100);
				atrPctSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				ClearOutputWindow(); 
			}
		}

		protected override void OnBarUpdate()
		{	
			if(CurrentBar < 101) { return; }
			setBands(debug: false);
			setTrend(debug: false);
			setVolatility( debug: false, showTxt: false);
			
			/// show This creates 9 potential market states (see chart below): 
			/// show guidance for next day
			/// Based on this classification system and a test period of the last 16 years: 
			/// a. Tomorrow, on average, is favorable when today’s market condition is any Bull or Sideways Quiet
			/// b. Tomorrow, on average is flat when today’s market condition is Sideways Volatile or Sideways Neutral.
			/// c. Tomorrow, on average, is down when today’s market condition is any Bear.
			/// 
			///  add spy data series, so I can add this to other charts?
			///  add show MA Band
			///  add show daily text
			///  write this as a rolling value indicator?
			///  make public trend and volatility
			///  compare to examples on the web site

		}
		
		#region Misc_functions
		
		protected void setVolatility(bool debug, bool showTxt)
		{
			///  Volatility
			///  ATR(12) / Close 
			atrPctSeries[0] =  atr[0] / Close[0];
			if ( debug ) { Print(Time[0].ToShortDateString() + "   ATR " + atr[0].ToString("0.00") + "   ATR% " + atrPctSeries[0].ToString("0.0000"));}
			
			var numbersArray = new double[100];
			for (var i = 1; i <= 100; i++) {
			    numbersArray[i -1] = atrPctSeries[i];
			}
			///  find max + min
			var max = numbersArray.Max();
			var min = numbersArray.Min();
			///  find average
			var avg = numbersArray.Average();
			
			/// find Std Dev
			double sumOfSquaresOfDifferences = numbersArray.Sum(val => (val - avg) * (val - avg));
			double stdDev = Math.Sqrt(sumOfSquaresOfDifferences / (numbersArray.Length - 1));
			//Print(numbersArray[0] + "   " + numbersArray[1] + "   " + numbersArray[2] + "  Max " + max + "  Min " + min + "  Avg " + avg);
			//double halfStdDev = stdDev + 0.5;
			//double calcHi =  100 - (100 * 0.16667);
			//double calcLo =  1 + (100 * 0.16667);
			///  find +/- 1 StdDev //double stdDev0	= stdDev[0];
			double stdDevClacHi = avg + stdDev;
			double stdDevClacLo = avg - stdDev;
			///  top 1/6 = volatile
			///  bottom 1/6 quiet
			///  middle 2/3 Normal
			if ( debug ) { 
				Print("-----");
				Print("stdDev  " + stdDev);
				Print("   max  " + max);
				Print("ClacHi  " +  stdDevClacHi );
				Print("ClacLo  " +  stdDevClacLo );
				Print("   min  " + min);
				Print("-----");
			}
			
			//// double check last 100 days
//			Print(CurrentBar);
//			if (CurrentBar == 1138) {
//				Draw.Text(this, "x"+CurrentBar, "X", 0, Low[0], Brushes.Yellow);
//			}
			
			///	Volatility: Volatile, Normal, Quiet 
			///	a. Volatile: today’s ATR% is more than 1 StDev  higher than the average ATR% of the last 100 days 
			///	b. Normal: today’s ATR% is within +/- 1 StDev  of the average ATR% of the last 100 days 
			///	c. Quiet: today’s ATR% is more than 1 StDev  less than the average ATR% of the last 100 days 
			
			/// Volatile
			if( atrPctSeries[0] > stdDevClacHi ) {
				volatil = true; 
				normal = false;
				quiet = false; 
			}
			else if( atrPctSeries[0] < stdDevClacLo ) {
				volatil = false; 
				normal = false;
				quiet = true;
			}
			else {
				volatil = false; 
				normal = true;
				quiet = false;
			}
			
			if ( debug ) {  Print(" V " + volatil + " N " + normal + " Q " + quiet);}
			
			if(showTxt) {
				if (volatil) {
					Draw.Text(this, "V"+CurrentBar, "V", 0, Low[0] - (TickSize * 40), Brushes.Red);
				}
				if (normal) {
					Draw.Text(this, "N"+CurrentBar, "N", 0, Low[0] - (TickSize * 40), Brushes.DarkGray);
				}
				if (quiet) {
					Draw.Text(this, "Q"+CurrentBar, "Q", 0, Low[0] - (TickSize * 40), Brushes.DarkCyan);
				}
			}
		}
		
		protected void setBands(bool debug)
		{
			sma0		= Math.Abs(sma[0]);
			twoPctUp = Math.Abs(( sma[0] * 0.02 ) + sma0);
			twoPctDn = Math.Abs(( sma[0] * 0.02 ) - sma0);
			Upper[0] = twoPctUp;
			Lower[0] = twoPctDn;
			if ( debug ) { Print(sma0 + " + " + twoPctUp + " - " + twoPctDn);}
		}
		
		protected void setTrend(bool debug)
		{
			///  Bull =  Close  > Upper Band  200 MA + 2%
			///  Bear = Close < Lower Band 200 MA - 2%
			///  Sideway = Close inside Bands
			double todayClose = Math.Abs(Close[0]);
			
			if ( todayClose > twoPctUp ) {
				bull = true; 
				bear = false; 
				sideways = false; 
			}
					
			else if ( todayClose < twoPctDn ) {
				bull = false; 
				bear = true; 
				sideways = false;
			}
			else {		
				bull = false; 
				bear = false; 
				sideways = true;
			}
			
			if (bull) {
				PlotBrushes[0][0] = Brushes.DodgerBlue;
				PlotBrushes[1][0] = Brushes.DodgerBlue;
			}
			if (bear) {
				PlotBrushes[0][0] = Brushes.Crimson;
				PlotBrushes[1][0] = Brushes.Crimson;
			}
			if (sideways) {
				PlotBrushes[0][0] = Brushes.Goldenrod;
				PlotBrushes[1][0] = Brushes.Goldenrod;
			}
			if ( debug ) { Print(" v " + bear + " ^ " + bull + " <> " + sideways);}
		}
		
		#endregion
		
		#region Properties
		public Series<double> Upper
		{
		  get { return Values[0]; }
		}
		  
		public Series<double> Lower
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
		private MarketCondition[] cacheMarketCondition;
		public MarketCondition MarketCondition()
		{
			return MarketCondition(Input);
		}

		public MarketCondition MarketCondition(ISeries<double> input)
		{
			if (cacheMarketCondition != null)
				for (int idx = 0; idx < cacheMarketCondition.Length; idx++)
					if (cacheMarketCondition[idx] != null &&  cacheMarketCondition[idx].EqualsInput(input))
						return cacheMarketCondition[idx];
			return CacheIndicator<MarketCondition>(new MarketCondition(), input, ref cacheMarketCondition);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MarketCondition MarketCondition()
		{
			return indicator.MarketCondition(Input);
		}

		public Indicators.MarketCondition MarketCondition(ISeries<double> input )
		{
			return indicator.MarketCondition(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MarketCondition MarketCondition()
		{
			return indicator.MarketCondition(Input);
		}

		public Indicators.MarketCondition MarketCondition(ISeries<double> input )
		{
			return indicator.MarketCondition(input);
		}
	}
}

#endregion
