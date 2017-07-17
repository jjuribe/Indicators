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

/*
"My experience is what I agree to attend to. Only those items which I notice shape my mind.” –William James, The Principles of Psychology, Vol.1
I’m developing a solid NQ swing system, 
I 've had amazing loving bj's and more are right around the corner.
I have a desired mind and body

Todays work:
Mark the highs and lows
Draw zigzags
Section the lines to mark entries
Make into strategy 
Make hard stops
Make 2nd pic stops
Start trading omniscience’s NQ YM And ES

*/
//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class DailyPivots : Indicator
	{
		private Swing Swing1;
		
		private double pivotHigh;
		private double pivotLow;
		private double pivotHighBar;
		private double pivotLowBar;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "DailyPivots";
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
				Strength					= 5;
				AddPlot(new Stroke(Brushes.Green, 2), PlotStyle.Dot, "PivotLow");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "PivotHigh");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Day, 1);
			}
			else if (State == State.DataLoaded)
			{				
				Swing1				= Swing(Strength);
				ClearOutputWindow(); 
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
			
			// HTF bars
			if (BarsInProgress == 1)
			{
//				if (Swing1.SwingHigh[0] != 0 ) {
//					pivotHigh =  Swing1.SwingHigh[0];	
//				}
				
				//pivotHigh =  Swing1.SwingHigh[0];
				
				// Find Highs and Lows
				if ( High[0] > MAX(High, 3 )[1] && pivotHighBar != CurrentBar) {
					pivotHigh = High[0];
					pivotHighBar = CurrentBar;
					Print(pivotHigh);
				}
					
				Print("HTF Barnum " + CurrentBar);
				return;
			}
			
			// LTF Bars
			if (BarsInProgress == 0)
			{
//				if (pivotHigh == High[ Strength + 1 ] ) {
//					Print(pivotHigh);
					
//					Draw.Dot(this, "DailyHigh"+CurrentBar, true, 0, pivotHigh, Brushes.Red);
//				}
				
				//Draw.Dot(this, "DailyHigh"+CurrentBar, true, 0, pivotHigh, Brushes.Red);
				
				if (pivotHighBar == CurrentBar ) {
					RemoveDrawObject("DailyHigh"+ (CurrentBar-1));
					Print("reomved");
				}
				Print("CB "+CurrentBar + " PH " + pivotHigh);
				Draw.Line(this, "DailyHigh"+CurrentBar, false, 10, pivotHigh, 0, pivotHigh, Brushes.Red, DashStyleHelper.Solid, 2);
				
			
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Strength", Order=1, GroupName="Parameters")]
		public int Strength
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PivotLow
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> PivotHigh
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
		private DailyPivots[] cacheDailyPivots;
		public DailyPivots DailyPivots(int strength)
		{
			return DailyPivots(Input, strength);
		}

		public DailyPivots DailyPivots(ISeries<double> input, int strength)
		{
			if (cacheDailyPivots != null)
				for (int idx = 0; idx < cacheDailyPivots.Length; idx++)
					if (cacheDailyPivots[idx] != null && cacheDailyPivots[idx].Strength == strength && cacheDailyPivots[idx].EqualsInput(input))
						return cacheDailyPivots[idx];
			return CacheIndicator<DailyPivots>(new DailyPivots(){ Strength = strength }, input, ref cacheDailyPivots);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DailyPivots DailyPivots(int strength)
		{
			return indicator.DailyPivots(Input, strength);
		}

		public Indicators.DailyPivots DailyPivots(ISeries<double> input , int strength)
		{
			return indicator.DailyPivots(input, strength);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DailyPivots DailyPivots(int strength)
		{
			return indicator.DailyPivots(Input, strength);
		}

		public Indicators.DailyPivots DailyPivots(ISeries<double> input , int strength)
		{
			return indicator.DailyPivots(input, strength);
		}
	}
}

#endregion
