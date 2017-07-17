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
	public class AblesysT2 : Indicator
	{
//		private int risk = 4;
//		private int period= 12;
		private double atrtimes = 2;
		private int counter1= 0;
		private int counter2= 0;
		private int sideside= 1;
		private double dotplot = double.NaN;
		private Series<double> TrueRange;
		private Series<double>  Sideside;
		private int once;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Support and resistance levels.";
				Name										= "Ablesys T2";
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
//				ATR											= 2;
//				Period										= 12;
//				Risk										= 4;
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "Upper");
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Dot, "Lower");
			}
			else if (State == State.Configure)
			{
				TrueRange = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Sideside = new Series<double>(this, MaximumBarsLookBack.Infinite);
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar <20){return;}
			double truerange;
				double updotplot;
				double lowdotplot;
				if (CurrentBar == 0)
				{
					TrueRange[0] = ((High[0] - Low[0]));
				}
				else
				{
					truerange = (High[0] - Low[0]);
					truerange = Math.Max (Math.Abs ((Low[0] - Close[1])), Math.Max (truerange, Math.Abs ((High[0] - Close[1]))));
					TrueRange[0] = truerange;
				}
				if (CurrentBar < 2)
				{
					return;
				}
				double AtrValue = SMA(TrueRange,Period)[1];//
				double Acelfactor =( ( atrtimes+0.1*Risk) * AtrValue);
				double Sigclose;
				
				if (sideside == 1)
				{
					Sigclose = High[1];
					//Sigclose = Math.Max(High[0],High[1]);
					updotplot = (Sigclose - Acelfactor);
					
					if (double.IsNaN (dotplot) || (updotplot >= dotplot))
					{
						dotplot = Bars.Instrument.MasterInstrument.RoundToTickSize( updotplot);
					}
				}
				else if (sideside == -1)
				{
					//Sigclose = Math.Min(Low[0],Low[1]);
					Sigclose = Low[1];
					lowdotplot = (Sigclose + Acelfactor);
					
					if (double.IsNaN (dotplot) || (lowdotplot <= dotplot))
					{
						dotplot =Bars.Instrument.MasterInstrument.RoundToTickSize( lowdotplot);
					}
				}
				counter1++;
				counter2++;
				
				
				if (! double.IsNaN (dotplot))
				{
					if (sideside != 1)
					{
						if ((sideside == -1) && (Close[0] > dotplot))			// was high
						{
							sideside = 1;
							counter2 = 0;
							dotplot = Bars.Instrument.MasterInstrument.RoundToTickSize((High[0] - Acelfactor));
						}
					}
					else if (Close[0] < dotplot)								// was low
					{
						sideside = -1;
						counter2 = 0;
						dotplot = Bars.Instrument.MasterInstrument.RoundToTickSize((Low[0] + Acelfactor));
					}
				}
				Sideside[0] = sideside;
				if(Sideside[0]!=Sideside[1])
				{
					once=0;
				}
				if (sideside == 1)
				{
					if(once==1)
					{
						Upper[0] = dotplot;
						Lower.Reset ();
					}
					if(once==0)
					{
						//DrawArrowUp("arrow" + CurrentBar, true, 0,  dotplot, Color.Blue);
					}
					once=1;
				}
				
				if (sideside == -1)
				{
					if(once==1)
					{
						Lower[0] = dotplot;
						Upper.Reset ();
					}
					if(once==0)
					{
						//DrawArrowDown("arrow" + CurrentBar, true, 0,  dotplot, Color.Red);
					}
					once=1;
				}	
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name = "ATR", Description = "Number of ATR Multipliers", Order=1, GroupName="Parameters")]
		public double ATR
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=2, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Risk", Description = "Risk ranges from 1-10, default is 3.", Order=3, GroupName="Parameters")]
		public int Risk
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Upper
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Lower
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
		private AblesysT2[] cacheAblesysT2;
		public AblesysT2 AblesysT2(double aTR, int period, int risk)
		{
			return AblesysT2(Input, aTR, period, risk);
		}

		public AblesysT2 AblesysT2(ISeries<double> input, double aTR, int period, int risk)
		{
			if (cacheAblesysT2 != null)
				for (int idx = 0; idx < cacheAblesysT2.Length; idx++)
					if (cacheAblesysT2[idx] != null && cacheAblesysT2[idx].ATR == aTR && cacheAblesysT2[idx].Period == period && cacheAblesysT2[idx].Risk == risk && cacheAblesysT2[idx].EqualsInput(input))
						return cacheAblesysT2[idx];
			return CacheIndicator<AblesysT2>(new AblesysT2(){ ATR = aTR, Period = period, Risk = risk }, input, ref cacheAblesysT2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AblesysT2 AblesysT2(double aTR, int period, int risk)
		{
			return indicator.AblesysT2(Input, aTR, period, risk);
		}

		public Indicators.AblesysT2 AblesysT2(ISeries<double> input , double aTR, int period, int risk)
		{
			return indicator.AblesysT2(input, aTR, period, risk);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AblesysT2 AblesysT2(double aTR, int period, int risk)
		{
			return indicator.AblesysT2(Input, aTR, period, risk);
		}

		public Indicators.AblesysT2 AblesysT2(ISeries<double> input , double aTR, int period, int risk)
		{
			return indicator.AblesysT2(input, aTR, period, risk);
		}
	}
}

#endregion
