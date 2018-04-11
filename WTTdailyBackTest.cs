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
	
	public class WTTdailyBackTest : Indicator
	{
					
		int[] highArray = { 20161228, 20170228, 20170511, 20170727, 20171003, 20171205, 20180125, 20180308 }; 
		int[] lowArray = { 20170212, 20170316, 20170622, 20170818, 20171115, 20180105, 20180213, 20180404 }; 
		int today = 0;
		int lineLength = 10;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "WTTdailyBackTest";
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
				ShowText					= true;
				TextSpacing					= 0.2;

			}
			else if (State == State.Configure)
			{
			}
			else if(State == State.DataLoaded)
			{
				  ClearOutputWindow(); 
			}
		}

		protected override void OnBarUpdate()
		{
			//if (IsRising(SMA(20)))
			/// set up line length for the chart type
			if ( CurrentBar < 20 ) {
				if (BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && BarsPeriod.Value >= 60)
			    {
			        lineLength = 70;
			    }
				if (BarsPeriod.BarsPeriodType == BarsPeriodType.Minute && BarsPeriod.Value <= 30)
			    {
			        lineLength = 100;
			    }
				return;
			}

			/// only print 1 bar per day
			if (today == ToDay(Time[0])) {
				return;
			}
			
			/// lower band
			for (int i = 0; i < lowArray.Length; i++) {
	            if ( ToDay(Time[0]) == lowArray[i] ) {
					int lowestBar = LowestBar(Low, 20);
				    double lowestPrice = Low[lowestBar];
					lowestPrice = lowestPrice - (lowestPrice * 0.01);
					Draw.Line(this, "bottom" + CurrentBar, true, lineLength, lowestPrice, -lineLength, lowestPrice, Brushes.DodgerBlue, DashStyleHelper.Solid, 4);
				}
	        }
			
			/// upper band
			for (int i = 0; i < highArray.Length; i++) {
	            if ( ToDay(Time[0]) == highArray[i] ) {
					int highestBar = HighestBar(High, 20);
				    double highestPrice = High[highestBar];
					highestPrice = highestPrice + (highestPrice * 0.01);
					Draw.Line(this, "top" + CurrentBar, true, lineLength, highestPrice, -lineLength, highestPrice, Brushes.Crimson, DashStyleHelper.Solid, 4);
				}
	        }
				
			today = ToDay(Time[0]);	
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ShowText", Order=1, GroupName="Parameters")]
		public bool ShowText
		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="TextSpacing", Order=2, GroupName="Parameters")]
		public double TextSpacing
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WTTdailyBackTest[] cacheWTTdailyBackTest;
		public WTTdailyBackTest WTTdailyBackTest(bool showText, double textSpacing)
		{
			return WTTdailyBackTest(Input, showText, textSpacing);
		}

		public WTTdailyBackTest WTTdailyBackTest(ISeries<double> input, bool showText, double textSpacing)
		{
			if (cacheWTTdailyBackTest != null)
				for (int idx = 0; idx < cacheWTTdailyBackTest.Length; idx++)
					if (cacheWTTdailyBackTest[idx] != null && cacheWTTdailyBackTest[idx].ShowText == showText && cacheWTTdailyBackTest[idx].TextSpacing == textSpacing && cacheWTTdailyBackTest[idx].EqualsInput(input))
						return cacheWTTdailyBackTest[idx];
			return CacheIndicator<WTTdailyBackTest>(new WTTdailyBackTest(){ ShowText = showText, TextSpacing = textSpacing }, input, ref cacheWTTdailyBackTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WTTdailyBackTest WTTdailyBackTest(bool showText, double textSpacing)
		{
			return indicator.WTTdailyBackTest(Input, showText, textSpacing);
		}

		public Indicators.WTTdailyBackTest WTTdailyBackTest(ISeries<double> input , bool showText, double textSpacing)
		{
			return indicator.WTTdailyBackTest(input, showText, textSpacing);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WTTdailyBackTest WTTdailyBackTest(bool showText, double textSpacing)
		{
			return indicator.WTTdailyBackTest(Input, showText, textSpacing);
		}

		public Indicators.WTTdailyBackTest WTTdailyBackTest(ISeries<double> input , bool showText, double textSpacing)
		{
			return indicator.WTTdailyBackTest(input, showText, textSpacing);
		}
	}
}

#endregion
