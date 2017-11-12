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
	public class FibStrategy : Indicator
	{
		private NinjaTrader.NinjaScript.Indicators.GoldenZoneTrading.GZT_GoldenFibs_MTF GZT_GoldenFibs_MTF1;
		private MAEnvelopes MAEnvelopes1;
		private Series<double> mpeAvg;
		private double [] fibs = new double[20];
		List<double> avgMpe = new List<double>();
		List<double> avgMae = new List<double>();
		
		private int counter;
		private bool foundFib;
		
		private bool longSignal;
		private bool shortSignal;
		private double longEntryPrice;
		private double shortEntryPrice;
		
		private double longEntryActual;
		private double shortEntryActual;
		
		private bool inLongTrade;
		private bool inShortTrade;
		private double mpe;
		private double mae;
		
		private double totalGain;
		private double totalLoss;
		private double winCount;
		private double lossCount;
		
		private int tradesToday;
		private int winsToday;
		
		private double stopSize = 4;
		private double targetSize = 4;
		
		private int barsSinceEntryL = 0;
		private int barsSinceEntryS = 0;
		private int daycount;
		
		private string tradeDirection = "Short";
		private bool upperRangeOpen;
		private bool lowerRangeOpen;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "FibStrategy";
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
				
				AddPlot(Brushes.Red, "Upper");
				AddPlot(Brushes.DodgerBlue, "Lower");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(BarsPeriodType.Minute, 5); 
				AddDataSeries(BarsPeriodType.Minute, 15); 
				AddDataSeries(BarsPeriodType.Minute, 30); 
				AddDataSeries(BarsPeriodType.Minute, 60); 
				AddDataSeries(BarsPeriodType.Day, 1); 
			}
			else if (State == State.DataLoaded)
			{				
				GZT_GoldenFibs_MTF1			= GZT_GoldenFibs_MTF(Close);
				MAEnvelopes1				= MAEnvelopes(Close, 0.125, 1, 34);
				ClearOutputWindow();  
				mpeAvg = new Series<double>(this, MaximumBarsLookBack.Infinite); 
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 1) { return; }
			if (BarsInProgress >= 1) { return; }
			if (Time[0].DayOfWeek == DayOfWeek.Sunday) { return; }
			if (ToTime(Time[0]) >= 60000 && ToTime(Time[0]) <= 60003 ) { daycount++; tradesToday = 0;}
			upperRangeOpen = outsideRange(upper: true);
			lowerRangeOpen = outsideRange(upper: false);
			
			Upper[0] = MAEnvelopes1.Upper[0];
			Lower[0] = MAEnvelopes1.Lower[0];
			
			optimizeRuns( stops: false,  targets: false,  buffer: 0.25 );
			
			if ( !inLongTrade ) { longSignal = entrySignal(longEntry: true); }
			showLongTrades();
			manageLongTrade();
			
			if ( !inShortTrade ) { shortSignal = entrySignal(longEntry: false); }
			showShortTrades();
			manageShortTrade();
			/// God I commit this work to you. I pledge none to margarie and 20% to your kingdom
			/// [X] entry @ fib 	  Trades 52   Win %62  Gains 60
			/// [X] add 3 trade limit Trades:22   Win %73  Gains:50.0
			/// [X] add 2 win limit	  Trades:52   Win %62  Gains:60.0 both: Trades:22  Win %73  Gains:50.0
			/// [X] optimize stop target, show avg mae, mpe - No Effect
			/// [X] try 3 min bars - better with smaller targets / stops
			/// [X] no target first bar    Trades:20  Wins12.0  Win %60  Gains:20.0
			/// [X] optimize stop target,  Trades:25  Wins15.0  Win %60  Gains:27.7
			/// [X] stop hit before target Trades:20  Wins12.0  Win %60  Gains:18.0  Avg Day:4.2  ROI 8.0%
			/// [X] add short entry trades T\Trades:25  Wins14.0  Win 56%  Gains:12.0  Avg Day:5.6  ROI 5.3%			
			/// [ ] if win rate > 50% find an orderflow filter like 1 min bars with trades and market replay 
			/// or make entry on the 1 min time frame...
			
		}
		
		private void showLongTrades() {
			if ( tradesToday == 3 ) {return;}
			if (!inLongTrade  && longSignal) {
				tradesToday++ ;
				longEntryActual = longEntryPrice;
				Draw.ArrowUp(this, "LE"+CurrentBar, true, 0, longEntryActual, Brushes.Lime);
				Draw.Text(this, "letoday"+CurrentBar, tradesToday.ToString(), 0, Low[0] -2, Brushes.DodgerBlue);
				inLongTrade = true;
				barsSinceEntryL = 0;
				mpe = 0; mae = 0;
				tradeDirection = "LE";
			}
			
			if ( inLongTrade ) {
				var mpeNow = High[0] - longEntryActual;
				var maeNow = longEntryActual -  Low[0];
				
				if (mpeNow > mpe) {
					mpe = mpeNow;
				}
				
				if (maeNow > mae ) {
					mae = maeNow;
				}
			}
			
			
		}
		
		private void manageLongTrade() {
			if ( !inLongTrade ) { barsSinceEntryL = 0; return; }
			if ( barsSinceEntryL >= 1 ) { 
				Draw.Text(this, "bsel"+CurrentBar, barsSinceEntryL.ToString(), 0, Low[0] -1, Brushes.Cyan);
				var stopPrice = longEntryActual - stopSize;
				var targetPrice = longEntryActual + targetSize;
				/// Stopped out
				if (Low[0] <= stopPrice ) {
					inLongTrade = false;
					Draw.Dot(this, "Stop"+CurrentBar, true, 0, stopPrice, Brushes.Crimson);
					totalLoss += stopSize;
					lossCount += 1;
					calcStats();
					barsSinceEntryL = 0;
					return;
				}
				/// target hit
				if (High[0] >= targetPrice ) {
					inLongTrade = false;
					Draw.Dot(this, "Target"+CurrentBar, true, 0, targetPrice, Brushes.Lime);
					totalGain += targetSize;
					winCount += 1;
					barsSinceEntryL = 0;
					calcStats();
				}
			}
			barsSinceEntryL++;
		}
		
		private void showShortTrades() {
			
			if ( tradesToday == 3 ) {return;} // same results -  if ( winsToday == 2 ) {return;}
			if (!inShortTrade  && shortSignal) {
				tradesToday++ ;
				shortEntryActual = shortEntryPrice;
				Draw.ArrowDown(this, "SE"+CurrentBar, true, 0, shortEntryActual, Brushes.Crimson);
				Draw.Text(this, "sttoday"+CurrentBar, tradesToday.ToString(), 0, High[0] +2, Brushes.Red);
				inShortTrade = true;
				barsSinceEntryS = 0;
				
				mpe = 0; mae = 0;
				tradeDirection = "SE";
	
//				Draw.Text(this, "StopLine"+CurrentBar, "=", 0, stopPrice);
//				Draw.Text(this, "EntryLine"+CurrentBar, "-", 0, shortEntryPrice);
//				Draw.Text(this, "TargetLine"+CurrentBar, "+", 0, targetPrice);
			}
			
			if ( inShortTrade ) {
				var mpeNow = shortEntryActual - Low[0];
				var maeNow = High[0] - shortEntryActual;
				
				if (mpeNow > mpe) {
					mpe = mpeNow;
				}
				
				if (maeNow > mae ) {
					mae = maeNow;
				}
			}
			
			
		}
				
		private void manageShortTrade() {
			if ( !inShortTrade ) { barsSinceEntryS = 0; return; }
			if ( barsSinceEntryS >= 1 ) { 
			Draw.Text(this, "bses"+CurrentBar, barsSinceEntryS.ToString(), 0, High[0] +1, Brushes.Magenta);
				var stopPrice = shortEntryActual + stopSize;
				var targetPrice = shortEntryActual - targetSize;
				//Draw.Text(this, "StopLine"+CurrentBar, "=", 0, stopPrice);
				//Draw.Text(this, "EntryLine"+CurrentBar, "-", 0, shortEntryPrice);
				//Draw.Text(this, "TargetLine"+CurrentBar, "+", 0, targetPrice);
				/// Stopped out
				if (High[0] >= stopPrice ) {
					inShortTrade = false;
					Draw.Dot(this, "Stop"+CurrentBar, true, 0, stopPrice, Brushes.Crimson);
					totalLoss += stopSize;
					lossCount += 1;
					calcStats();
					barsSinceEntryS = 0;
					return;
				}
				/// target hit
				if (Low[0] <= targetPrice ) {
					inShortTrade = false;
					Draw.Dot(this, "Target"+CurrentBar, true, 0, targetPrice, Brushes.Lime);
					totalGain += targetSize;
					winCount += 1;
					barsSinceEntryS = 0;
					calcStats();
				}
			}
			barsSinceEntryS++;
		}
				
		private void optimizeRuns(bool stops, bool targets, double buffer ) {
			
			if ( avgMpe.Count > 1 ) {
					if ( targets ) {targetSize = avgMpe.Average(); }
					if ( stops ) { 
						stopSize = avgMae.Average() + buffer;
						if (stopSize > 5) {
							stopSize = 5;
						}
					}
				}
		}
		
		private void calcStats() {
			var message = "";
			var screenMessage = "\n";
			message += daycount.ToString();
			message += "   ";
			
			message += tradeDirection;
			message += "   ";
			
			message += Time[0].ToShortDateString();
			message += "  MPE:";
			message +=  mpe.ToString("0.0");
			
			avgMpe.Add(mpe);
			if (avgMpe.Count > 1 ) {
				message += "  Avg:";
				message +=  avgMpe.Average().ToString("0.0");
			}
			message += "  MAE:";
			message +=  mae.ToString("0.0");
			avgMae.Add(mae);
			if (avgMae.Count > 1 ) {
				message += "  Avg:";
				message +=  avgMae.Average().ToString("0.0");
			}
			message += "  Trades:";
			var total = winCount + lossCount;
			message +=  total.ToString("0");
			screenMessage += total.ToString("0");
			screenMessage += " Trades\n";
			
			message += "  Wins";
			//var winPct = winCount / total;    
			message +=  winCount.ToString("0.0");
			
			message += "  Win ";
			var winPct = ( winCount / total ) * 100;    
			message +=  winPct.ToString("0");
			message += "%";
			screenMessage += winPct.ToString("0");
			screenMessage += "%\n";
			
			message += "  Gains:";
			var grossProfit = totalGain - totalLoss;
			message +=  grossProfit.ToString("0.0");
			screenMessage += grossProfit.ToString("0.0");
			screenMessage += " Gain\n";
			
			message += "  Avg Day:";
			var avgDay = totalGain / daycount ;
			message +=  avgDay.ToString("0.0");
			
			message += "  ROI ";
			var roi = (( grossProfit * 20 ) / 4500 ) * 100;
			message +=  roi.ToString("0.0");
			message += "%";
			screenMessage += roi.ToString("0.0");
			screenMessage += " ROI";
			
			Print(message);
			Draw.TextFixed(this, "results", screenMessage, TextPosition.TopLeft);
		}
		
		private bool isTradingHours() {
			var canTrade = false;	
			// Only trade between 7:45 AM and 1:45 PM
			if (ToTime(Time[0]) >= 63000 && ToTime(Time[0]) <= 90000)
			{
			    canTrade = true;
			} else {
				tradesToday = 0;
				winsToday = 0;
			}
			
			return canTrade;
		}
		
		private bool entrySignal(bool longEntry) {
			foundFib = checkForFibCross(buffer: 0.25);
			//if ( foundFib ) { counter = counter + 1; }
			//Print(counter);
			var answer = false;
			if ( longEntry ) {
				if ( foundFib && lowerRangeOpen && isTradingHours() ) {
					CandleOutlineBrush = Brushes.DodgerBlue;
					BarBrush = Brushes.DodgerBlue;
					answer = true;
				}
			} else {
				if ( foundFib && upperRangeOpen && isTradingHours() ) {
					CandleOutlineBrush = Brushes.Crimson;
					BarBrush = Brushes.Crimson;
					answer = true;
				}	
			}
			return answer;
		}
		
		private bool outsideRange(bool upper) {
			var answer = false;
			if ( upper  ) {
				if ( High[0] >= MAEnvelopes1.Upper[0] )
				{
					answer = true;
				}
			} else {
				if ( Low[0] <= MAEnvelopes1.Lower[0] )
				{
					answer = true;
				}
			}
			return answer;
		}
				
		private bool checkForFibCross(double buffer) {
			var foundFib = false;
			var barHighBuffer = High[0] + buffer;
			var barLowBuffer =  Low[0] - buffer;
			populateFibArray();
			
			for (int i = 0; i < 19; i++) 
			{
				if ( fibs[i] <= MAEnvelopes1.Lower[0] ) {
					if (fibs[i] <= barHighBuffer && fibs[i] >= barLowBuffer ) { 
						foundFib = true; 
						longEntryPrice = fibs[i]; 
					}
				}
				if ( fibs[i] >= MAEnvelopes1.Upper[0] ) {
					if (fibs[i] <= barHighBuffer && fibs[i] >= barLowBuffer ) { 
						foundFib = true; 
						shortEntryPrice = fibs[i]; 
					}
				}
			}
			return foundFib;
		}
		
		private void populateFibArray() {
			fibs[0] = GZT_GoldenFibs_MTF1.Confluence0[0];
			fibs[1] = GZT_GoldenFibs_MTF1.Confluence1[0];
			fibs[2] = GZT_GoldenFibs_MTF1.Confluence2[0];
			fibs[3] = GZT_GoldenFibs_MTF1.Confluence3[0];
			fibs[4] = GZT_GoldenFibs_MTF1.Confluence4[0];
			fibs[5] = GZT_GoldenFibs_MTF1.Confluence5[0];
			fibs[6] = GZT_GoldenFibs_MTF1.Confluence6[0];
			fibs[7] = GZT_GoldenFibs_MTF1.Confluence7[0];
			fibs[8] = GZT_GoldenFibs_MTF1.Confluence8[0];
			fibs[9] = GZT_GoldenFibs_MTF1.Confluence9[0];
			fibs[10] = GZT_GoldenFibs_MTF1.Confluence10[0];
			fibs[11] = GZT_GoldenFibs_MTF1.Confluence11[0];
			fibs[12] = GZT_GoldenFibs_MTF1.Confluence12[0];
			fibs[13] = GZT_GoldenFibs_MTF1.Confluence13[0];
			fibs[14] = GZT_GoldenFibs_MTF1.Confluence14[0];
			fibs[15] = GZT_GoldenFibs_MTF1.Confluence15[0];
			fibs[16] = GZT_GoldenFibs_MTF1.Confluence16[0];
			fibs[17] = GZT_GoldenFibs_MTF1.Confluence17[0];
			fibs[18] = GZT_GoldenFibs_MTF1.Confluence18[0];
			fibs[19] = GZT_GoldenFibs_MTF1.Confluence19[0];
		}
		
		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
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
		private FibStrategy[] cacheFibStrategy;
		public FibStrategy FibStrategy()
		{
			return FibStrategy(Input);
		}

		public FibStrategy FibStrategy(ISeries<double> input)
		{
			if (cacheFibStrategy != null)
				for (int idx = 0; idx < cacheFibStrategy.Length; idx++)
					if (cacheFibStrategy[idx] != null &&  cacheFibStrategy[idx].EqualsInput(input))
						return cacheFibStrategy[idx];
			return CacheIndicator<FibStrategy>(new FibStrategy(), input, ref cacheFibStrategy);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FibStrategy FibStrategy()
		{
			return indicator.FibStrategy(Input);
		}

		public Indicators.FibStrategy FibStrategy(ISeries<double> input )
		{
			return indicator.FibStrategy(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FibStrategy FibStrategy()
		{
			return indicator.FibStrategy(Input);
		}

		public Indicators.FibStrategy FibStrategy(ISeries<double> input )
		{
			return indicator.FibStrategy(input);
		}
	}
}

#endregion
