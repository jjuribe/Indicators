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
	public class DiveDDTradeFramer : Indicator
	{
		public string entryType = "5DD";
		public double shares;
		public double reward;
		public double maxRisk = 50;
		public double rR;
		public double risk;
		public int space = 20;
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "DiveDDTradeFramer";
				Calculate									= Calculate.OnEachTick;
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
			    //clear the output window as soon as the bars data is loaded
			    ClearOutputWindow();  	
			  }
		}

		protected override void OnBarUpdate()
		{
			/// Line Position
			int spaceToRight = -5;
			int textSpaceToRight = -5;
			int spaceToLeft = 0;
			int centerBar = -1;
			/// Trade Frame Calc
			double maxHigh = MAX(High, 10)[0];
			double minLow = MIN(Low, 3)[0];
			double stop = minLow - 0.05;
			double entry = Close[0] + 0.05;
			Print( Close[0] + "\t" + maxHigh + "\t" + minLow);
			/// Risk & Position Size
			risk = Close[0] - stop;
			reward = maxHigh - entry;
			rR = reward / risk;
			shares = maxRisk / risk;
			
			/// Trade Frame Lines
			Draw.Line(this, "vert", true, centerBar, MAX(High, 10)[0], centerBar, stop, Brushes.CornflowerBlue, DashStyleHelper.Solid, 4);
			Draw.Line(this, "Target", true, spaceToLeft, MAX(High, 10)[0], spaceToRight, MAX(High, 10)[0], Brushes.CornflowerBlue, DashStyleHelper.Solid, 4);
			Draw.Line(this, "Stop", true, spaceToLeft, stop, spaceToRight, stop, Brushes.CornflowerBlue, DashStyleHelper.Solid, 4);
			Draw.Line(this, "entry", true, spaceToLeft, entry, spaceToRight, entry, Brushes.CornflowerBlue, DashStyleHelper.Solid, 4);
			/// ma 200
			if (SMA(200)[0] < maxHigh && SMA(200)[0] > stop ) {
				Draw.Line(this, "200MA", true, spaceToLeft, SMA(200)[0] , spaceToRight, SMA(200)[0] , Brushes.CornflowerBlue, DashStyleHelper.Solid, 4);
			}
			/// ma 10
			if (SMA(10)[0] < maxHigh && SMA(10)[0] > stop ) {
				Draw.Line(this, "10MA", true, spaceToLeft, SMA(10)[0] , spaceToRight, SMA(10)[0] , Brushes.CornflowerBlue, DashStyleHelper.Dash, 4);
			}
			
			/// Entry Text
			Draw.Text(this, "entryTxt", entry.ToString("0.00"), textSpaceToRight, entry + ( TickSize* space ), Brushes.CornflowerBlue);
			/// Stop Text
			Draw.Text(this, "stopTxt", stop.ToString("0.00"), textSpaceToRight, stop + ( TickSize* space ), Brushes.CornflowerBlue);
			/// target Text
			Draw.Text(this, "TargetTxt", maxHigh.ToString("0.00"), textSpaceToRight, maxHigh + ( TickSize* space ), Brushes.CornflowerBlue);
			
			/// consolidation
			/// consolidation high

			setTextBox( textInBox: popuateStatsTextBox());
			/// market replay
				 
		}
		
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Text Box
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////		
		protected string popuateStatsTextBox() {

			string bodyMessage = "\n\t";
			bodyMessage = bodyMessage + entryType+"\t\n\t";
			bodyMessage = bodyMessage + "$"+maxRisk+" maxRisk\t\n\t";
			bodyMessage = bodyMessage + shares.ToString("0")+" shares\t\n\t";	
			bodyMessage = bodyMessage + rR.ToString("0.00")+" RR: \t\n";
			return bodyMessage;
		}		
		protected void setTextBox(string textInBox)
		{
			/// show market condition
			TextFixed myTF = Draw.TextFixed(this, "tradeStat", textInBox, TextPosition.BottomLeft);
			myTF.TextPosition = TextPosition.BottomLeft;
			myTF.AreaBrush = Brushes.White;
			myTF.AreaOpacity = 90;
			myTF.TextBrush = Brushes.Black;
		}
	}
	
	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DiveDDTradeFramer[] cacheDiveDDTradeFramer;
		public DiveDDTradeFramer DiveDDTradeFramer()
		{
			return DiveDDTradeFramer(Input);
		}

		public DiveDDTradeFramer DiveDDTradeFramer(ISeries<double> input)
		{
			if (cacheDiveDDTradeFramer != null)
				for (int idx = 0; idx < cacheDiveDDTradeFramer.Length; idx++)
					if (cacheDiveDDTradeFramer[idx] != null &&  cacheDiveDDTradeFramer[idx].EqualsInput(input))
						return cacheDiveDDTradeFramer[idx];
			return CacheIndicator<DiveDDTradeFramer>(new DiveDDTradeFramer(), input, ref cacheDiveDDTradeFramer);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DiveDDTradeFramer DiveDDTradeFramer()
		{
			return indicator.DiveDDTradeFramer(Input);
		}

		public Indicators.DiveDDTradeFramer DiveDDTradeFramer(ISeries<double> input )
		{
			return indicator.DiveDDTradeFramer(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DiveDDTradeFramer DiveDDTradeFramer()
		{
			return indicator.DiveDDTradeFramer(Input);
		}

		public Indicators.DiveDDTradeFramer DiveDDTradeFramer(ISeries<double> input )
		{
			return indicator.DiveDDTradeFramer(input);
		}
	}
}

#endregion
