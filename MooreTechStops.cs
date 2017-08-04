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
	public class MooreTechStops : Indicator
	{
		private Swing 		Swing1;	
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MooreTechStops";
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
			}
			else if (State == State.Configure)
			{
			}
			else if(State == State.DataLoaded)
			  {  
				  Swing1				= Swing(5);	// for piv stops
			  }
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}
		
//		///******************************************************************************************************************************
//		/// 
//		/// 										set Pivot Stop
//		/// 
//		/// ****************************************************************************************************************************
//		public void setPivotStop(int swingSize, double pivotSlop) {
			
//			double lastSwingLow = Swing1.SwingLow[ swingSize ];
//			double lastSwingHigh = Swing1.SwingHigh[ swingSize ];
			
//			/// long pivots, count pivots above entry for 2nd piv stop if  short /// close > entryswing 
//			 if ( entry.inLongTrade && (( lastSwingLow + pivotSlop ) <  entry.lastPivotValue ) && entry.barsSinceEntry > 8 ) {
//				 entry.pivStopCounter++;
//				 Draw.Text(this, "LowSwingtxt"+ CurrentBar.ToString(),  entry.pivStopCounter.ToString(), swingSize, Low[swingSize] - (TickSize * 10));
//				 entry.lastPivotValue = lastSwingLow;
//			 }
//			 /// short pivots, count pivots above entry for 2nd piv stop if  short /// close > entryswing 
//			 if ( entry.inShortTrade && ( lastSwingHigh - pivotSlop )  > entry.lastPivotValue && entry.barsSinceEntry > 8 ) {
//				 entry.pivStopCounter++;
//				 Text myText = Draw.Text(this, "HighSwingtxt"+ CurrentBar.ToString(), entry.pivStopCounter.ToString(), swingSize, High[swingSize] + (TickSize * 10));
//				 entry.lastPivotValue = lastSwingHigh;
//			 }
//			 /// draw the 2nd piv stop line //drawPivStops();
//			 if(entry.inLongTrade || entry.inShortTrade )
//			 if ( entry.pivStopCounter == 2) {
//				int lineLength = 0; 
//				entry.pivLineLength++; 
//				RemoveDrawObject("pivStop" + (CurrentBar - 1));
//				Draw.Line(this, "pivStop"  +CurrentBar.ToString(), false, entry.pivLineLength, entry.lastPivotValue, 0, 
//						entry.lastPivotValue, Brushes.Magenta, DashStyleHelper.Dot, 2);
//			 } 
//			/// exit at pivot line
//			exitFromPivotStop(pivotSlop: pivotSlop);
//		}

//		/// exit trade after pivot stop
//		public void exitFromPivotStop(double pivotSlop) {
			
//			if (CurrentBar > entry.longEntryBarnum &&  entry.pivStopCounter >= 2 && entry.inLongTrade && Low[0] <= entry.lastPivotValue ) {
//				Draw.Dot(this, "testDot"+CurrentBar, true, 0, entry.lastPivotValue, Brushes.Magenta);
//				entry.inLongTrade = false;
//                signals[0]  = 2;
//				entry.longPivStopBarnum = CurrentBar;
//				tradeData.signalName = "LX - PS";
//				entry.pivLineLength = 0;
//				entry.pivStopCounter = 0;
//				entry.barsSinceEntry = 0;
//				secondPivStopFlag = true;
//			}
			
//			if (CurrentBar > entry.shortEntryBarnum &&  entry.pivStopCounter >= 2 && entry.inShortTrade && High[0] >= entry.lastPivotValue ) {
//				Draw.Dot(this, "testDot"+CurrentBar, true, 0, entry.lastPivotValue, Brushes.Magenta);
//				entry.inShortTrade = false;
//                signals[0] = -2;	
//				entry.shortPivStopBarnum = CurrentBar;
//				tradeData.signalName = "SX - PS";
//				entry.pivLineLength = 0;
//				entry.pivStopCounter = 0;
//				entry.barsSinceEntry = 0;
//				secondPivStopFlag = true;
//			}
//		}
//		///******************************************************************************************************************************
//		/// 
//		/// 										set Hard Stop
//		/// 
//		/// ****************************************************************************************************************************
//		public void setHardStop(double pct, int shares, bool plot) {
//			/// find long entry price /// calc trade cost
//			if (CurrentBar == entry.longEntryBarnum ) {
//				double pctPrice = pct * 0.01;
//				entry.hardStopLine = Math.Abs(Close[0]  - ( Close[0] * pctPrice));
//				}
//			/// find short entry price /// calc trade cost
//			if (CurrentBar == entry.shortEntryBarnum ) {
//				double pctPrice = pct * 0.01;
//				entry.hardStopLine = Math.Abs(Close[0]  + ( Close[0] * pctPrice));
//			}
//			/// draw hard stop line
//			drawHardStops(plot: plot);
//			/// exit at hard stop
//			exitFromStop();
//		}
		
//		/// exit at hard stop
//		public void exitFromStop() {
//			if ( entry.inLongTrade && Low[0] <= entry.hardStopLine ) {
//				/// need short trades to debug this no long stops hit
//				entry.inLongTrade = false;
//                signals[0] = 2;
//				entry.longHardStopBarnum	= CurrentBar;
//				tradeData.signalName = "LX - HS";
//				entry.barsSinceEntry = 0;
//			} else if ( entry.inShortTrade && High[0] >= entry.hardStopLine ) {
//				/// need short trades to debug this no long stops hit
//				entry.inShortTrade = false;
//                signals[0] = -2;
//				entry.shortHardStopBarnum	= CurrentBar;
//				tradeData.signalName = "SX - HS";
//				entry.barsSinceEntry = 0;
//			}
//		}
		
//		public void drawHardStops(bool plot) {
//			if( !plot ) {
//				return;
//			}
//			/// draw hard stop line 
//			int lineLength = 0;
//			string lineName = "";
//			if ( entry.inLongTrade ) { 
//				lineLength = entry.barsSinceEntry; 
//				lineName = "hardStopLong";
//			}
//			if ( entry.inShortTrade ) { 
//				lineLength = CurrentBar - entry.shortEntryBarnum; 
//				lineName = "hardStopShort";
//			}
//			if(entry.barsSinceEntry > 1)
//				RemoveDrawObject(lineName + (CurrentBar - 1));
//			Draw.Line(this, lineName +CurrentBar.ToString(), false, lineLength, entry.hardStopLine, 0, 
//					entry.hardStopLine, Brushes.DarkGray, DashStyleHelper.Dot, 2);
//		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MooreTechStops[] cacheMooreTechStops;
		public MooreTechStops MooreTechStops()
		{
			return MooreTechStops(Input);
		}

		public MooreTechStops MooreTechStops(ISeries<double> input)
		{
			if (cacheMooreTechStops != null)
				for (int idx = 0; idx < cacheMooreTechStops.Length; idx++)
					if (cacheMooreTechStops[idx] != null &&  cacheMooreTechStops[idx].EqualsInput(input))
						return cacheMooreTechStops[idx];
			return CacheIndicator<MooreTechStops>(new MooreTechStops(), input, ref cacheMooreTechStops);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MooreTechStops MooreTechStops()
		{
			return indicator.MooreTechStops(Input);
		}

		public Indicators.MooreTechStops MooreTechStops(ISeries<double> input )
		{
			return indicator.MooreTechStops(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MooreTechStops MooreTechStops()
		{
			return indicator.MooreTechStops(Input);
		}

		public Indicators.MooreTechStops MooreTechStops(ISeries<double> input )
		{
			return indicator.MooreTechStops(input);
		}
	}
}

#endregion
