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
	public class AblesysSystem : Indicator
	{
		///  T! 
		private int	myperiod	= 27;
		private Series<double> myDataSeries; 
		private int risk=3;
		public int trend = 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Ablesys System";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				//IsSuspendedWhileInactive					= true;
				PaintBar					= false;
				ShowArrow					= true;
				ShowStripe					= true;
			}
			else if (State == State.Configure)
			{
				myDataSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
			}
		}

		protected override void OnBarUpdate()
		{
			var trendOne = setTrendOne();
			Print(trendOne);
		}
		
		public int setTrendOne() {
			
			myDataSeries[0] = (-100 * (MAX(High, myperiod)[0] - Close[0]) / (MAX(High, myperiod)[0] - MIN(Low, myperiod)[0] == 0 ? 1 : MAX(High, myperiod)[0] - MIN(Low, myperiod)[0]));
			
			/// Uptrend
			if (myDataSeries[0] >= -33+risk)
			{
				trend = 1;
				if ( PaintBar ) {
					CandleOutlineBrush  = Brushes.DarkBlue;
		
					if(Open[0]<Close[0] ) {
							BarBrush  = Brushes.Transparent;
					} else{
							BarBrush  = Brushes.DodgerBlue;
					}
				}
	
			}
			else   /// downtrend
			if (myDataSeries[0] <= -67-risk) {
				if ( PaintBar ) {
					CandleOutlineBrush  = Brushes.Crimson;
					if(Open[0]<Close[0] ) {
							BarBrush  = Brushes.Transparent;
					} else{
						BarBrush  = Brushes.Red;
					}			
				}
				trend = -1;
			}
			else   /// sideways
				{
				if ( PaintBar ) {
					CandleOutlineBrush  = Brushes.LimeGreen;
					if(Open[0]<Close[0] ) {
						BarBrush  = Brushes.Transparent;
					} else{
						BarBrush  = Brushes.LightGreen;
					}	
				}
				trend = 0;
			}
			return trend ;
		}
		
		

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="PaintBar", Order=1, GroupName="Parameters")]
		public bool PaintBar
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowArrow", Order=2, GroupName="Parameters")]
		public bool ShowArrow
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowStripe", Order=3, GroupName="Parameters")]
		public bool ShowStripe
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AblesysSystem[] cacheAblesysSystem;
		public AblesysSystem AblesysSystem(bool paintBar, bool showArrow, bool showStripe)
		{
			return AblesysSystem(Input, paintBar, showArrow, showStripe);
		}

		public AblesysSystem AblesysSystem(ISeries<double> input, bool paintBar, bool showArrow, bool showStripe)
		{
			if (cacheAblesysSystem != null)
				for (int idx = 0; idx < cacheAblesysSystem.Length; idx++)
					if (cacheAblesysSystem[idx] != null && cacheAblesysSystem[idx].PaintBar == paintBar && cacheAblesysSystem[idx].ShowArrow == showArrow && cacheAblesysSystem[idx].ShowStripe == showStripe && cacheAblesysSystem[idx].EqualsInput(input))
						return cacheAblesysSystem[idx];
			return CacheIndicator<AblesysSystem>(new AblesysSystem(){ PaintBar = paintBar, ShowArrow = showArrow, ShowStripe = showStripe }, input, ref cacheAblesysSystem);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AblesysSystem AblesysSystem(bool paintBar, bool showArrow, bool showStripe)
		{
			return indicator.AblesysSystem(Input, paintBar, showArrow, showStripe);
		}

		public Indicators.AblesysSystem AblesysSystem(ISeries<double> input , bool paintBar, bool showArrow, bool showStripe)
		{
			return indicator.AblesysSystem(input, paintBar, showArrow, showStripe);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AblesysSystem AblesysSystem(bool paintBar, bool showArrow, bool showStripe)
		{
			return indicator.AblesysSystem(Input, paintBar, showArrow, showStripe);
		}

		public Indicators.AblesysSystem AblesysSystem(ISeries<double> input , bool paintBar, bool showArrow, bool showStripe)
		{
			return indicator.AblesysSystem(input, paintBar, showArrow, showStripe);
		}
	}
}

#endregion
