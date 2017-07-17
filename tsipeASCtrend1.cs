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
	/// Modified HeikenAshi to be used with Squeeze Indicator.
	/// Modified by Huntchuks 10/29/08, added trend to candles and trend boxes. Tidied up code.
    /// </summary>
    //[Description("Easctrend")]
    //[Gui.Design.DisplayName("tsipEasctrend1")]
	
	
	public class tsipeASCtrend : Indicator
    {
		
        #region Variables
		
		private int	myperiod	= 27;
		// private Series<double> Avg;
		// private DataSeries myDataSeries; 
		private Series<double> myDataSeries; 
		private int risk=3;
		public int trend = 0;
		private bool		textWarnings = true;

		#endregion

        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
//        protected override void Initialize()
//        {
//			Overlay				= true;
//			myDataSeries = new DataSeries(this); 
//			CalculateOnBarClose = false;
//        }
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				// [Description("Easctrend")]
    			// [Gui.Design.DisplayName("tsipEasctrend1")]
				
				Description							= @"Easctrend";
				Name								= "tsipEasctrend1";				
				Calculate							= Calculate.OnPriceChange;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
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
			}
			else if (State == State.Configure)
			{
				myDataSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				//_trend = new Series<bool>(this, MaximumBarsLookBack.Infinite);
				
			}
		}

        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {			
			
			myDataSeries[0] = (-100 * (MAX(High, myperiod)[0] - Close[0]) / (MAX(High, myperiod)[0] - MIN(Low, myperiod)[0] == 0 ? 1 : MAX(High, myperiod)[0] - MIN(Low, myperiod)[0]));
			
			if (myDataSeries[0] >= -33+risk)
			{
				CandleOutlineBrush  = Brushes.DarkBlue;
				//if(Open[0]<Close[0] && ChartControl.ChartStyleType == ChartStyleType.CandleStick ) {
				if(Open[0]<Close[0] ) {
						BarBrush  = Brushes.Transparent;
					} else{
						BarBrush  = Brushes.DodgerBlue;
					}								
				//BarColor = Color.Blue;
				trend = 1;
			}
			else
			if (myDataSeries[0] <= -67-risk)
			{
				CandleOutlineBrush  = Brushes.Crimson;
				if(Open[0]<Close[0] ) {
						BarBrush  = Brushes.Transparent;
					} else{
						BarBrush  = Brushes.Red;
					}			
				//BarColor = Color.Red;
				trend = -1;
			}
			else
				{
				CandleOutlineBrush  = Brushes.LimeGreen;
				if(Open[0]<Close[0] ) {
						BarBrush  = Brushes.Transparent;
					} else{
						BarBrush  = Brushes.LightGreen;
					}			
				trend = 0;
			}
				
				//Text Section
			
//			if(textWarnings)	
//			{	
//			if(trend > 0)
//			{DrawTextFixed("TrendUp", " Trend Up!", TextPosition.BottomLeft, Color.Black, new Font("Arial", 12), Color.Blue, Color.Blue, 7);}
//				else
//			{RemoveDrawObject("TrendUp");}
			
//			if(trend < 0)
//			{DrawTextFixed("TrendDn", " Trend Down!", TextPosition.BottomLeft, Color.Black, new Font("Arial", 12), Color.Red, Color.Red, 7);}
//				else
//			{RemoveDrawObject("TrendDn");}
			
//			if(trend == 0)
//			{DrawTextFixed("NoTrend", " No Trend", TextPosition.BottomLeft, Color.Black, new Font("Arial", 12), Color.Green, Color.Green, 7);}
//				else
//			{RemoveDrawObject("NoTrend");}
//			}
	
		}

        #region Properties
		[Browsable (false)]
		[Category("Parameters")]
		public int Trend
        {
            get { return trend; }
            set { trend = Math.Max(-11, value); }
        }
		[Description("Risk ranges from 1-10(Usual value is 3).")]
		[Category("Parameters")]
		public int Risk
        {
            get { return risk; }
            set { risk = Math.Max(1, value); }
        }
		[Description("Text marker showing Trend.")]
		// [Display(Name = "ST Mode", Description = "SuperTrend Mode", Order = 1, GroupName = "1. Parameters")]
		//[Gui.Design.DisplayName ("Show Trend Message?")]
		[Display(Name = "Show Trend Message?", Description = "Show Trend Message?", Order = 1, GroupName = "1. Parameters")]
        [Category("Parameters")]
        public bool TextWarnings
        {
            get { return textWarnings; }
            set { textWarnings = value; }
        }
	
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private tsipeASCtrend[] cachetsipeASCtrend;
		public tsipeASCtrend tsipeASCtrend()
		{
			return tsipeASCtrend(Input);
		}

		public tsipeASCtrend tsipeASCtrend(ISeries<double> input)
		{
			if (cachetsipeASCtrend != null)
				for (int idx = 0; idx < cachetsipeASCtrend.Length; idx++)
					if (cachetsipeASCtrend[idx] != null &&  cachetsipeASCtrend[idx].EqualsInput(input))
						return cachetsipeASCtrend[idx];
			return CacheIndicator<tsipeASCtrend>(new tsipeASCtrend(), input, ref cachetsipeASCtrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.tsipeASCtrend tsipeASCtrend()
		{
			return indicator.tsipeASCtrend(Input);
		}

		public Indicators.tsipeASCtrend tsipeASCtrend(ISeries<double> input )
		{
			return indicator.tsipeASCtrend(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.tsipeASCtrend tsipeASCtrend()
		{
			return indicator.tsipeASCtrend(Input);
		}

		public Indicators.tsipeASCtrend tsipeASCtrend(ISeries<double> input )
		{
			return indicator.tsipeASCtrend(input);
		}
	}
}

#endregion
