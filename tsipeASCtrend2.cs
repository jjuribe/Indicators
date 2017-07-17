// 
// Copyright (C) 2017, NinjaTrader LLC <www.ninjatrader.com>.
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
    /// <summary>
    /// eASCtrend2.
    /// </summary>
    // [Description("eASCtrend2")]
    // [Gui.Design.DisplayName("tsipeASCtrend2")]
    public class tsipeASCtrend2 : Indicator
    {
        #region Variables
			private int risk = 4;
			private int period= 12;
			private double atrtimes = 2;
			private int counter1= 0;
			private int counter2= 0;
			private int sideside= 1;
			private double dotplot = double.NaN;
		
			// private Series<double> myDataSeries; 
			// private DataSeries TrueRange;
			// internal DataSeries Sideside;
			private Series<double> TrueRange;
			private Series<double>  Sideside;
		
			private int once;
        #endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
//        protected override void Initialize()
//        {
//            	Add(new Plot(Color.Blue, PlotStyle.Dot, "ATR Trailing Up"));
//				Add(new Plot(Color.Red, PlotStyle.Dot, "ATR Trailing Dn"));
//				Plots[0].Pen.DashStyle = DashStyle.Dot;
//				Plots[1].Pen.DashStyle = DashStyle.Dot;
//				CalculateOnBarClose = false;
//				Overlay = true;
//				PriceTypeSupported = false;
//				PaintPriceMarkers	= false;
								
//				TrueRange = new DataSeries (this);
//				Sideside = new DataSeries (this);

//        }
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				// [Description("Easctrend")]
    			// [Gui.Design.DisplayName("tsipEasctrend1")]
				
				Description							= @"eASCtrend2";
				Name								= "tsipeASCtrend2";				
				Calculate							= Calculate.OnPriceChange;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= false;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
//				IsSuspendedWhileInactive			= true;
//				Length					= 14;
//				Multiplier				= 2.2;
//				Smooth					= 14;
//       			MaType = MovingAverageType.HMA;
//        		StMode = SuperTrendMode.ATR;				
//				ShowIndicator			= true;
//				ShowArrows				= false;
//				ColorBars				= false;
//				PlayAlert				= false;
//				UpColor					= Brushes.DodgerBlue;
//				DownColor				= Brushes.Red;
//				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Hash, "TrendPlot");
				
				//Add(new Plot(Color.Blue, PlotStyle.Dot, "ATR Trailing Up"));
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Dot, "ATR Trailing Up");
				//Add(new Plot(Color.Red, PlotStyle.Dot, "ATR Trailing Dn"));
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "ATR Trailing Dn");
				// Plots[0].Pen.DashStyle = DashStyle.Dot;
				//Plots[0].Pen.DashStyle = DashStyle.Dot;
				//Plots[1].Pen.DashStyle = DashStyle.Dot;
			}
			else if (State == State.Configure)
			{
				TrueRange = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Sideside = new Series<double>(this, MaximumBarsLookBack.Infinite);
				//myDataSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				//_trend = new Series<bool>(this, MaximumBarsLookBack.Infinite);
				
			}
		}

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
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
				double AtrValue = SMA(TrueRange,period)[1];//
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

        	[Browsable(false)]
			[XmlIgnore()]
			public  Series<double> Lower
			{
				get { return Values[1]; }
			}

			
			[Description("Number of ATR Multipliers")]
        	[Category("Parameters")]
			//[Gui.Design.DisplayNameAttribute("Number of ATR (Ex. 3 Time ATR) ")]
			[Display(Name = "Number of ATR (Ex. 3 Time ATR)?", Description = "Number of ATR Multipliers", Order = 1, GroupName = "1. Parameters")]
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
			public  Series<double> Upper
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
		private tsipeASCtrend2[] cachetsipeASCtrend2;
		public tsipeASCtrend2 tsipeASCtrend2()
		{
			return tsipeASCtrend2(Input);
		}

		public tsipeASCtrend2 tsipeASCtrend2(ISeries<double> input)
		{
			if (cachetsipeASCtrend2 != null)
				for (int idx = 0; idx < cachetsipeASCtrend2.Length; idx++)
					if (cachetsipeASCtrend2[idx] != null &&  cachetsipeASCtrend2[idx].EqualsInput(input))
						return cachetsipeASCtrend2[idx];
			return CacheIndicator<tsipeASCtrend2>(new tsipeASCtrend2(), input, ref cachetsipeASCtrend2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.tsipeASCtrend2 tsipeASCtrend2()
		{
			return indicator.tsipeASCtrend2(Input);
		}

		public Indicators.tsipeASCtrend2 tsipeASCtrend2(ISeries<double> input )
		{
			return indicator.tsipeASCtrend2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.tsipeASCtrend2 tsipeASCtrend2()
		{
			return indicator.tsipeASCtrend2(Input);
		}

		public Indicators.tsipeASCtrend2 tsipeASCtrend2(ISeries<double> input )
		{
			return indicator.tsipeASCtrend2(input);
		}
	}
}

#endregion
