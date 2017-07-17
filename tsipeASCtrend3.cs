// 
// Copyright (C) 2006, NinjaTrader LLC <www.ninjatrader.com>.
// NinjaTrader reserves the right to modify or overwrite this NinjaScript component with each release.
//

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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    public class tsipeASCtrend3 : Indicator
    {
        #region Variables
			private int risk = 3;
			private int period= 10;
			private double atrtimes = 2;
			private int counter1= 0;
			private int counter2= 0;
			private int sideside= 1;
			private double dotplot = double.NaN;
			private Series<double> TrueRange;
			internal Series<double> Sideside;
			private int once;

		
        #endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"eASCtrend3";
				Name								= "tsipeASCtrend3";				
				Calculate							= Calculate.OnPriceChange;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= false;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
					
				//Add(new Plot(Color.Blue, PlotStyle.Dot, "ATR Trailing Up"));
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Dot, "ATR Trailing Up");
				//Add(new Plot(Color.Red, PlotStyle.Dot, "ATR Trailing Dn"));
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "ATR Trailing Dn");
				//Add(new Plot(Color.DodgerBlue, PlotStyle.Line, "TrueRangeSMA"));
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Dot, "TrueRangeSMA");
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
					TrueRange[0] =  (truerange);
				}
				if (CurrentBar < 2)
				{
					return;
				}
				double AtrValue = SMA(TrueRange,period)[1];
				TrueRangeSMA[0] = (SMA(TrueRange,period)[1]);
				double Acelfactor =( ( atrtimes+0.1*risk) * AtrValue);
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
						if ((sideside == -1) && ((High[0]+Low[0])/2 >= dotplot))
						{
							sideside = 1;
							counter2 = 0;
							dotplot = Bars.Instrument.MasterInstrument.RoundToTickSize((High[0] - Acelfactor));
						}
					}
					else if ((High[0]+Low[0])/2 <= dotplot)
					{
						sideside = -1;
						counter2 = 0;
						dotplot = Bars.Instrument.MasterInstrument.RoundToTickSize((Low[0] + Acelfactor));
					}
				}
				Sideside[0] = (sideside);
				if(Sideside[0]!=Sideside[1])
				{
					once=0;
				}
				if (sideside == 1)
				{
					if(once==1)
					{
						Upper[0] =  (dotplot);
						Lower.Reset ();
					}
					if(once==0)
					{
						Upper[0] =  (dotplot);
						Lower.Reset ();
					}
					once=1;
				}
				
				if (sideside == -1)
				{
					if(once==1)
					{
						Lower[0] =  (dotplot);
						Upper.Reset ();
					}
					if(once==0)
					{
						Lower[0] = (dotplot);
						Upper.Reset ();
					}
					once=1;
				}
				
			}

        #region Properties

        	[Browsable(false)]
			[XmlIgnore()]
			public Series<double> Lower
			{
				get { return Values[1]; }
			}
			
			[Browsable(false)]
			[XmlIgnore()]
			public Series<double> TrueRangeSMA
			{
				get { return Values[2]; }
			}
			
			[Description("Number of ATR Multipliers")]
        	[Category("Parameters")]
			//[Gui.Design.DisplayNameAttribute("Number of ATR (Ex. 3 Time ATR) ")]
			[Display(Name = "Number of ATR (Ex. 3 Time ATR)", Description = "Number of ATR Multipliers", Order = 1, GroupName = "1. Parameters")]
			public double NumberOfAtrs
			{
				get	{	return atrtimes;}
				set	{	atrtimes = Math.Max (1, value);	}
			}
		
			
			
			[Description("Period")]
        	[Category("Parameters")]
			//[Gui.Design.DisplayNameAttribute("Periods")]
			[Display(Name = "Period", Description = "Period", Order = 2, GroupName = "1. Parameters")]
			public int Period
			{
				get	{	return period;}	
				set	{	period = Math.Max(0, value);}
			}
			
			[Description("Risk ranges from 1-10, default is 3.")]
        	[Category("Parameters")]
			//[Gui.Design.DisplayNameAttribute("Risk")]
			[Display(Name = "Risk", Description = "Risk ranges from 1-10, default is 3.", Order = 3, GroupName = "1. Parameters")]
			public int Risk
			{
				get	{	return risk;}	
				set	{	risk = Math.Max(1, value);}
			}
			
			[Browsable(false)]
			[XmlIgnore()]
			public Series<double> Upper
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
		private tsipeASCtrend3[] cachetsipeASCtrend3;
		public tsipeASCtrend3 tsipeASCtrend3()
		{
			return tsipeASCtrend3(Input);
		}

		public tsipeASCtrend3 tsipeASCtrend3(ISeries<double> input)
		{
			if (cachetsipeASCtrend3 != null)
				for (int idx = 0; idx < cachetsipeASCtrend3.Length; idx++)
					if (cachetsipeASCtrend3[idx] != null &&  cachetsipeASCtrend3[idx].EqualsInput(input))
						return cachetsipeASCtrend3[idx];
			return CacheIndicator<tsipeASCtrend3>(new tsipeASCtrend3(), input, ref cachetsipeASCtrend3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.tsipeASCtrend3 tsipeASCtrend3()
		{
			return indicator.tsipeASCtrend3(Input);
		}

		public Indicators.tsipeASCtrend3 tsipeASCtrend3(ISeries<double> input )
		{
			return indicator.tsipeASCtrend3(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.tsipeASCtrend3 tsipeASCtrend3()
		{
			return indicator.tsipeASCtrend3(Input);
		}

		public Indicators.tsipeASCtrend3 tsipeASCtrend3(ISeries<double> input )
		{
			return indicator.tsipeASCtrend3(input);
		}
	}
}

#endregion
