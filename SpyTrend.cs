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
	/*
	generate histogram of spy strength
	if fast > slow + 1
	if close > fast + 2
	*/
	public class SpyTrend : Indicator
	{
		double fast;
		double slow;
		int stateOfSpy;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SpyTrend";
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
				SlowMA					= 200;
				FastMA					= 50;
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Bar, "Strength");
			}
			else if (State == State.Configure)
			{
				AddDataSeries("SPY", Data.BarsPeriodType.Day, 1, Data.MarketDataType.Last);
			}
		}

		protected override void OnBarUpdate()
		{
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
			
			if(BarsInProgress == 1)			//other Bars
				{
					fast = SMA(FastMA)[0];
					slow = SMA(SlowMA)[0];
					
					if (fast > slow) {
						stateOfSpy = 1;
						//PlotBrushes[0][0] = Brushes.Blue;
						Print(stateOfSpy);
						
						if (Close[0] > fast) {
							stateOfSpy = 2;
							//PlotBrushes[0][0] = Brushes.DodgerBlue;
							Print(stateOfSpy);
						}
					}
					
					if (fast < slow) {
						stateOfSpy = -1;
						//PlotBrushes[0][0] = Brushes.Red;
						Print(stateOfSpy);
						
						if (Close[0] < fast) {
							stateOfSpy = -2;
							//PlotBrushes[0][0] = Brushes.Crimson;
							Print(stateOfSpy);
						}
					}
				}
				
			if(BarsInProgress == 0)			//Chart tf Bars
				{	
					Strength[0] = stateOfSpy;
					Print("LTF" + stateOfSpy);
	             	switch (stateOfSpy)
				      {
				          case 1:
				              PlotBrushes[0][0] = Brushes.Blue;
				              break;
				          case 2:
				              PlotBrushes[0][0] = Brushes.DodgerBlue;
				              break;
						  case -1:
				              PlotBrushes[0][0] = Brushes.Red;
				              break;
				          case -2:
				              PlotBrushes[0][0] = Brushes.Crimson;
				              break;
				          default:
				              break;
				      }
				}	
			
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SlowMA", Order=1, GroupName="Parameters")]
		public int SlowMA
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
		private SpyTrend[] cacheSpyTrend;
		public SpyTrend SpyTrend(int slowMA, int fastMA)
		{
			return SpyTrend(Input, slowMA, fastMA);
		}

		public SpyTrend SpyTrend(ISeries<double> input, int slowMA, int fastMA)
		{
			if (cacheSpyTrend != null)
				for (int idx = 0; idx < cacheSpyTrend.Length; idx++)
					if (cacheSpyTrend[idx] != null && cacheSpyTrend[idx].SlowMA == slowMA && cacheSpyTrend[idx].FastMA == fastMA && cacheSpyTrend[idx].EqualsInput(input))
						return cacheSpyTrend[idx];
			return CacheIndicator<SpyTrend>(new SpyTrend(){ SlowMA = slowMA, FastMA = fastMA }, input, ref cacheSpyTrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SpyTrend SpyTrend(int slowMA, int fastMA)
		{
			return indicator.SpyTrend(Input, slowMA, fastMA);
		}

		public Indicators.SpyTrend SpyTrend(ISeries<double> input , int slowMA, int fastMA)
		{
			return indicator.SpyTrend(input, slowMA, fastMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SpyTrend SpyTrend(int slowMA, int fastMA)
		{
			return indicator.SpyTrend(Input, slowMA, fastMA);
		}

		public Indicators.SpyTrend SpyTrend(ISeries<double> input , int slowMA, int fastMA)
		{
			return indicator.SpyTrend(input, slowMA, fastMA);
		}
	}
}

#endregion
