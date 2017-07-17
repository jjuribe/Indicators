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
	public class RSItrendSPY : Indicator
	{
		double fastSpy;
		double slowSpy;
		double fast;
		double slow;
		int stateOfSpy;
		int stateOfLocal;
		
		bool shortRsi;
		bool longRsi;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "RSItrendSPY";
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
				FastMAspy					= 50;
				SlowMAspy					= 200;
				FastMA					= 50;
				SlowMA					= 200;
				RSIperiod					= 3;
				RSIsmooth					= 14;
				AddLine(Brushes.Red, 70, "Top");
				AddLine(Brushes.Green, 30, "Bottom");
				AddPlot(Brushes.DodgerBlue, "RSI");
				AddPlot(Brushes.Brown, "Smoothed");
			}
			else if (State == State.Configure)
			{
				AddDataSeries("SPY", Data.BarsPeriodType.Day, 1, Data.MarketDataType.Last);
			}
		}

		protected override void OnBarUpdate()
		{
			/*
			--print spy strength
			-- print local strength
			--print combined level
			
			--calc rsi
			--plot rsi
			
			--color lines of rsi
			
			--show arrow entries on chart
			show square exits on char
			calc P&L on upper left of chart
			
			AAPL, MSFT, AMZN, JNJ, GOOG
			XLY, XLP, XLE, XLF, XLV, XLI, XLB, XLRE, XLK, XLU
			
			mixed results accross the board
			*/
			if (CurrentBars[0] < 20)
			return;
			
			// set up higher time frame
			foreach(int CurrentBarI in CurrentBars)
			{
				if (CurrentBarI < BarsRequiredToPlot)
				{
					return;
				}
			}
			
			// -- trend of Spy and local chart
			if(BarsInProgress == 1)			//SPY Bars
				{
					fastSpy = SMA(FastMA)[0];
					slowSpy = SMA(SlowMA)[0];
					
					if (fastSpy > slowSpy) {
						stateOfSpy = 1;
						
						if (Close[0] > fastSpy) {
							stateOfSpy = 2;
						}
					}
					
					if (fastSpy < slowSpy) {
						stateOfSpy = -1;
						
						if (Close[0] < fastSpy) {
							stateOfSpy = -2;
						}
					}
				}
				
			if(BarsInProgress == 0)			//Chart tf Bars
				{	
					fast = SMA(FastMA)[0];
					slow = SMA(SlowMA)[0];
					
					if (fast > slow) {
						stateOfLocal = 1;
						//Print(stateOfLocal);
						
						if (Close[0] > fast) {
							stateOfLocal = 2;
							//Print(stateOfLocal);
						}
					}
					
					if (fast < slow) {
						stateOfLocal = -1;
						//Print(stateOfLocal);
						
						if (Close[0] < fast) {
							stateOfLocal = -2;
							//Print(stateOfLocal);
						}
					}
					
					Print("Spy "  + stateOfSpy + "  Local " + stateOfLocal);
					
					double myRSI = RSI(14,3)[0];
					
					// rsi signals
					if (myRSI >= 70) {
						shortRsi = true;
					} else {
						shortRsi = false;
					}
					
					if (myRSI <= 30) {
						longRsi = true;
					} else {
						longRsi = false;
					}
					
					// -- plot rsi
					Values[0][0] = RSI(14,3)[0];
					Values[1][0] = EMA(RSI(14,3), 3)[0];
					
					// --color lines of rsi
	             	switch (stateOfSpy)
				      {
				          case 1:
				              PlotBrushes[0][0] = Brushes.DodgerBlue;
				              break;
				          case 2:
				              PlotBrushes[0][0] = Brushes.DodgerBlue;
				              break;
						  case -1:
				              PlotBrushes[0][0] = Brushes.Red;
				              break;
				          case -2:
				              PlotBrushes[0][0] = Brushes.Red;
				              break;
				          default:
				              break;
				      }
					  
					  switch (stateOfLocal)
				      {
				          case 1:
				              PlotBrushes[1][0] = Brushes.DodgerBlue;
				              break;
				          case 2:
				              PlotBrushes[1][0] = Brushes.DodgerBlue;
				              break;
						  case -1:
				              PlotBrushes[1][0] = Brushes.Red;
				              break;
				          case -2:
				              PlotBrushes[1][0] = Brushes.Red;
				              break;
				          default:
				              break;
				      }
					  
					  // Long Entries
					  if (stateOfSpy > 0 && stateOfLocal > 0 && longRsi) {
						 ArrowUp myArrow = Draw.ArrowUp(this, "Buysig"+CurrentBar.ToString(), true, Time[0], Low[0] - (2 * TickSize), Brushes.DodgerBlue);
 
							// Set the outline color of the Arrow
							myArrow.OutlineBrush = Brushes.DodgerBlue;
						  
//						  BarBrush = Brushes.DodgerBlue;
//						  CandleOutlineBrush = Brushes.DodgerBlue;
					  }
					  // Short Entries
					  if (stateOfSpy < 0 && stateOfLocal < 0 && shortRsi) {
						  ArrowDown myArrowDn = Draw.ArrowDown(this, "Sellsig"+CurrentBar.ToString(), true, Time[0], High[0] + (2 * TickSize), Brushes.Crimson);
 
							// Set the outline color of the Arrow
							myArrowDn.OutlineBrush = Brushes.Crimson;
						  
//						  BarBrush = Brushes.Red;
//						  CandleOutlineBrush = Brushes.Red;
					  }
				}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastMAspy", Order=1, GroupName="Parameters")]
		public int FastMAspy
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowMAspy", Order=2, GroupName="Parameters")]
		public int SlowMAspy
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastMA", Order=3, GroupName="Parameters")]
		public int FastMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowMA", Order=4, GroupName="Parameters")]
		public int SlowMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSIperiod", Order=5, GroupName="Parameters")]
		public int RSIperiod
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RSIsmooth", Order=6, GroupName="Parameters")]
		public int RSIsmooth
		{ get; set; }



		[Browsable(false)]
		[XmlIgnore]
		public Series<double> RSI
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Smoothed
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
		private RSItrendSPY[] cacheRSItrendSPY;
		public RSItrendSPY RSItrendSPY(int fastMAspy, int slowMAspy, int fastMA, int slowMA, int rSIperiod, int rSIsmooth)
		{
			return RSItrendSPY(Input, fastMAspy, slowMAspy, fastMA, slowMA, rSIperiod, rSIsmooth);
		}

		public RSItrendSPY RSItrendSPY(ISeries<double> input, int fastMAspy, int slowMAspy, int fastMA, int slowMA, int rSIperiod, int rSIsmooth)
		{
			if (cacheRSItrendSPY != null)
				for (int idx = 0; idx < cacheRSItrendSPY.Length; idx++)
					if (cacheRSItrendSPY[idx] != null && cacheRSItrendSPY[idx].FastMAspy == fastMAspy && cacheRSItrendSPY[idx].SlowMAspy == slowMAspy && cacheRSItrendSPY[idx].FastMA == fastMA && cacheRSItrendSPY[idx].SlowMA == slowMA && cacheRSItrendSPY[idx].RSIperiod == rSIperiod && cacheRSItrendSPY[idx].RSIsmooth == rSIsmooth && cacheRSItrendSPY[idx].EqualsInput(input))
						return cacheRSItrendSPY[idx];
			return CacheIndicator<RSItrendSPY>(new RSItrendSPY(){ FastMAspy = fastMAspy, SlowMAspy = slowMAspy, FastMA = fastMA, SlowMA = slowMA, RSIperiod = rSIperiod, RSIsmooth = rSIsmooth }, input, ref cacheRSItrendSPY);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RSItrendSPY RSItrendSPY(int fastMAspy, int slowMAspy, int fastMA, int slowMA, int rSIperiod, int rSIsmooth)
		{
			return indicator.RSItrendSPY(Input, fastMAspy, slowMAspy, fastMA, slowMA, rSIperiod, rSIsmooth);
		}

		public Indicators.RSItrendSPY RSItrendSPY(ISeries<double> input , int fastMAspy, int slowMAspy, int fastMA, int slowMA, int rSIperiod, int rSIsmooth)
		{
			return indicator.RSItrendSPY(input, fastMAspy, slowMAspy, fastMA, slowMA, rSIperiod, rSIsmooth);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RSItrendSPY RSItrendSPY(int fastMAspy, int slowMAspy, int fastMA, int slowMA, int rSIperiod, int rSIsmooth)
		{
			return indicator.RSItrendSPY(Input, fastMAspy, slowMAspy, fastMA, slowMA, rSIperiod, rSIsmooth);
		}

		public Indicators.RSItrendSPY RSItrendSPY(ISeries<double> input , int fastMAspy, int slowMAspy, int fastMA, int slowMA, int rSIperiod, int rSIsmooth)
		{
			return indicator.RSItrendSPY(input, fastMAspy, slowMAspy, fastMA, slowMA, rSIperiod, rSIsmooth);
		}
	}
}

#endregion
