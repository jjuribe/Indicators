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
	public class AblesysT1 : Indicator
	{
		private Series<double> myDataSeries;
		private Series<int> trend;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Ablesys T1";
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
				IsSuspendedWhileInactive					= true;
				Period					= 27;
			}
			else if (State == State.Configure)
			{
			} else if (State == State.DataLoaded) {	
				myDataSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				trend = new Series<int>(this, MaximumBarsLookBack.Infinite);
			}
		}

		protected override void OnBarUpdate()
		{
			myDataSeries[0]		= (-100 * (MAX(High, Period)[0] - Close[0]) / (MAX(High, Period)[0] - MIN(Low, Period)[0] == 0 ? 1 : MAX(High, Period)[0] - MIN(Low, Period)[0]));
		
			if (myDataSeries[0] >= -30)
			{
				CandleOutlineBrush = Brushes.DarkBlue;
				if(Open[0]<Close[0] ) {
						BarBrush = Brushes.Transparent;
					} else{
						BarBrush = Brushes.Blue;
					}							
				trend[0] = 1;
			}
			else
			if (myDataSeries[0] <= -70)
			{
				CandleOutlineBrush = Brushes.Crimson;
				if(Open[0]<Close[0]  ) {
						BarBrush = Brushes.Transparent;
					} else{
						BarBrush = Brushes.Red;
					}	
				trend[0] = -1;
			}
			else
				{
				CandleOutlineBrush = Brushes.ForestGreen;
				if(Open[0]<Close[0]  ) {
						BarBrush = Brushes.Transparent;
					} else{
						BarBrush = Brushes.Lime;
					}	
				trend[0] = 0;
			
			}
				Print(trend.ToString());
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }
		
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<int> Trend
		{
			get { return trend; }
		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AblesysT1[] cacheAblesysT1;
		public AblesysT1 AblesysT1(int period)
		{
			return AblesysT1(Input, period);
		}

		public AblesysT1 AblesysT1(ISeries<double> input, int period)
		{
			if (cacheAblesysT1 != null)
				for (int idx = 0; idx < cacheAblesysT1.Length; idx++)
					if (cacheAblesysT1[idx] != null && cacheAblesysT1[idx].Period == period && cacheAblesysT1[idx].EqualsInput(input))
						return cacheAblesysT1[idx];
			return CacheIndicator<AblesysT1>(new AblesysT1(){ Period = period }, input, ref cacheAblesysT1);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AblesysT1 AblesysT1(int period)
		{
			return indicator.AblesysT1(Input, period);
		}

		public Indicators.AblesysT1 AblesysT1(ISeries<double> input , int period)
		{
			return indicator.AblesysT1(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AblesysT1 AblesysT1(int period)
		{
			return indicator.AblesysT1(Input, period);
		}

		public Indicators.AblesysT1 AblesysT1(ISeries<double> input , int period)
		{
			return indicator.AblesysT1(input, period);
		}
	}
}

#endregion
