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
	public class BottomTail : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "BottomTail";
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
		}

		protected override void OnBarUpdate()
		{
			if ( CurrentBar < 1 ) {return;}
			
			/// top tail
			if (Close[0] <= Open[0] && Close[0] <= Median[0] && Open[0] <= Median[0] && Close[0] > EMA(34)[0] ) {
				//Draw.ArrowDown(this, "SE"+CurrentBar, true, 0, High[0] + 0.25, Brushes.Crimson);
				//Draw.Text(this, "TT"+CurrentBar, "TT", 0, High[0] + 0.25);
				CandleOutlineBrush = Brushes.Crimson;
				BarBrush = Brushes.Crimson;
			}
			/// bottom tail
			if (Close[0] >= Open[0] && Close[0] >= Median[0] && Open[0] >= Median[0] && Close[0] < EMA(34)[0] ) {
				//Draw.ArrowUp(this, "LE"+CurrentBar, true, 0, Low[0] - 0.25, Brushes.DodgerBlue);
				//Draw.Text(this, "TT"+CurrentBar, "TT", 0, High[0] + 0.25);
				CandleOutlineBrush = Brushes.DodgerBlue;
				BarBrush = Brushes.DodgerBlue;
			} else if (Close[0] >= Open[0] && Close[0] >= Median[0] && Open[0] > Low[0] && Close[0] < EMA(34)[0] && Low[0] < Low[1] ) {
				/// alt bottom tail
				//Draw.ArrowUp(this, "LE"+CurrentBar, true, 0, Low[0] - 0.25, Brushes.DodgerBlue);
				//Draw.Text(this, "TT"+CurrentBar, "TT", 0, High[0] + 0.25);
				CandleOutlineBrush = Brushes.DodgerBlue;
				BarBrush = Brushes.Cyan;
			}
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BottomTail[] cacheBottomTail;
		public BottomTail BottomTail()
		{
			return BottomTail(Input);
		}

		public BottomTail BottomTail(ISeries<double> input)
		{
			if (cacheBottomTail != null)
				for (int idx = 0; idx < cacheBottomTail.Length; idx++)
					if (cacheBottomTail[idx] != null &&  cacheBottomTail[idx].EqualsInput(input))
						return cacheBottomTail[idx];
			return CacheIndicator<BottomTail>(new BottomTail(), input, ref cacheBottomTail);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BottomTail BottomTail()
		{
			return indicator.BottomTail(Input);
		}

		public Indicators.BottomTail BottomTail(ISeries<double> input )
		{
			return indicator.BottomTail(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BottomTail BottomTail()
		{
			return indicator.BottomTail(Input);
		}

		public Indicators.BottomTail BottomTail(ISeries<double> input )
		{
			return indicator.BottomTail(input);
		}
	}
}

#endregion
