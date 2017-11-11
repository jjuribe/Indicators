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
	public class MovAvg2Trend : Indicator
	{
		double fast;
		double slow;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MovAvg2Trend";
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
				SLowMA					= 200;
				FastMA					= 50;
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "Strength");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			fast = SMA(FastMA)[0];
			slow = SMA(SLowMA)[0];
			
			if (fast > slow) {
				Strength[0] = 1;
				PlotBrushes[0][0] = Brushes.DodgerBlue;
				
				if (Close[0] > fast) {
					Strength[0] = 2;
					PlotBrushes[0][0] = Brushes.Blue;
				}
			}
			
			if (fast < slow) {
				Strength[0] = -1;
				PlotBrushes[0][0] = Brushes.Salmon;
				
				if (Close[0] < fast) {
					Strength[0] = -2;
					PlotBrushes[0][0] = Brushes.Red;;
				}
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SLowMA", Order=1, GroupName="Parameters")]
		public int SLowMA
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="FastMA", Order=2, GroupName="Parameters")]
		public int FastMA
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Strength
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MovAvg2Trend[] cacheMovAvg2Trend;
		public MovAvg2Trend MovAvg2Trend(int sLowMA, int fastMA)
		{
			return MovAvg2Trend(Input, sLowMA, fastMA);
		}

		public MovAvg2Trend MovAvg2Trend(ISeries<double> input, int sLowMA, int fastMA)
		{
			if (cacheMovAvg2Trend != null)
				for (int idx = 0; idx < cacheMovAvg2Trend.Length; idx++)
					if (cacheMovAvg2Trend[idx] != null && cacheMovAvg2Trend[idx].SLowMA == sLowMA && cacheMovAvg2Trend[idx].FastMA == fastMA && cacheMovAvg2Trend[idx].EqualsInput(input))
						return cacheMovAvg2Trend[idx];
			return CacheIndicator<MovAvg2Trend>(new MovAvg2Trend(){ SLowMA = sLowMA, FastMA = fastMA }, input, ref cacheMovAvg2Trend);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MovAvg2Trend MovAvg2Trend(int sLowMA, int fastMA)
		{
			return indicator.MovAvg2Trend(Input, sLowMA, fastMA);
		}

		public Indicators.MovAvg2Trend MovAvg2Trend(ISeries<double> input , int sLowMA, int fastMA)
		{
			return indicator.MovAvg2Trend(input, sLowMA, fastMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MovAvg2Trend MovAvg2Trend(int sLowMA, int fastMA)
		{
			return indicator.MovAvg2Trend(Input, sLowMA, fastMA);
		}

		public Indicators.MovAvg2Trend MovAvg2Trend(ISeries<double> input , int sLowMA, int fastMA)
		{
			return indicator.MovAvg2Trend(input, sLowMA, fastMA);
		}
	}
}

#endregion
