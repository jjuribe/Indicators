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
using NinjaTrader.NinjaScript.Indicators;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class LinRegSignals : Indicator
	{
		private Indicators.RegressionChannel RegressionChannel1;
		
		//private TSSuperTrend TSSuperTrend1;
		
		// HTF Values
		private double UTFst;
		private int UTFdir = 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "LinRegSignals";
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
				ColorBarsDailyStrend					= true;
				AddPlot(new Stroke(Brushes.Lime, 2), PlotStyle.TriangleUp, "BuySignal");
				AddPlot(new Stroke(Brushes.Crimson, 2), PlotStyle.TriangleDown, "SellSignal");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Day, 1);
			}
			else if (State == State.DataLoaded)
			{	
				RegressionChannel1				= RegressionChannel(35, 3);
				RegressionChannel1.Plots[0].Brush = Brushes.DarkGray;
				RegressionChannel1.Plots[1].Brush = Brushes.DodgerBlue;
				RegressionChannel1.Plots[2].Brush = Brushes.DodgerBlue;
				//AddChartIndicator(RegressionChannel1);
				//TSSuperTrend1				= TSSuperTrend(SuperTrendMode.ATR, MovingAverageType.HMA, 14, 2.2, 14, false, false, false, false);
			}
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			Print("Connection Lost");
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 20)
			return;
			
			// set bands
			int VwmaAverage = 42;
			int RangeLength = 100;
			int SmoothLength = 100;
			int bandOne = 2;
			double bandTwo = 3.5;
			
			double sma0		= VWMA(Close, VwmaAverage)[0];			
			double smoothRange = SMA(ATR(RangeLength), SmoothLength)[0];
			double upperBandOne = sma0 + ( smoothRange * bandOne );
			double upperBandTwo	= sma0 + ( smoothRange * bandTwo );
			double lowerBandOne = sma0 - ( smoothRange * bandOne );
			double lowerBandTwo	= sma0 - ( smoothRange * bandTwo );

			

			 // Set Short Signal
			if ((High[0] >= upperBandOne) ) //&& (Close[0] < Open[0]))
			{
				//BarBrush = Brushes.Crimson;
				Draw.ArrowDown(this, @"Forex4Hr Arrow down"+CurrentBar.ToString(), true, 0, High[0]+ 0.0001, Brushes.Red);
			} 
			
			// Set Long Signal
			if ((Low[0] <= lowerBandOne) ) // && (Close[0] > Open[0]))
			{
				//BarBrush = Brushes.Crimson;
				Draw.ArrowUp(this, @"Forex4Hr Arrow Up"+CurrentBar.ToString(), true, 0, Low[0]- 0.0001, Brushes.Lime);
			} 
			

		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ColorBarsDailyStrend", Order=1, GroupName="Parameters")]
		public bool ColorBarsDailyStrend
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BuySignal
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SellSignal
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
		private LinRegSignals[] cacheLinRegSignals;
		public LinRegSignals LinRegSignals(bool colorBarsDailyStrend)
		{
			return LinRegSignals(Input, colorBarsDailyStrend);
		}

		public LinRegSignals LinRegSignals(ISeries<double> input, bool colorBarsDailyStrend)
		{
			if (cacheLinRegSignals != null)
				for (int idx = 0; idx < cacheLinRegSignals.Length; idx++)
					if (cacheLinRegSignals[idx] != null && cacheLinRegSignals[idx].ColorBarsDailyStrend == colorBarsDailyStrend && cacheLinRegSignals[idx].EqualsInput(input))
						return cacheLinRegSignals[idx];
			return CacheIndicator<LinRegSignals>(new LinRegSignals(){ ColorBarsDailyStrend = colorBarsDailyStrend }, input, ref cacheLinRegSignals);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LinRegSignals LinRegSignals(bool colorBarsDailyStrend)
		{
			return indicator.LinRegSignals(Input, colorBarsDailyStrend);
		}

		public Indicators.LinRegSignals LinRegSignals(ISeries<double> input , bool colorBarsDailyStrend)
		{
			return indicator.LinRegSignals(input, colorBarsDailyStrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LinRegSignals LinRegSignals(bool colorBarsDailyStrend)
		{
			return indicator.LinRegSignals(Input, colorBarsDailyStrend);
		}

		public Indicators.LinRegSignals LinRegSignals(ISeries<double> input , bool colorBarsDailyStrend)
		{
			return indicator.LinRegSignals(input, colorBarsDailyStrend);
		}
	}
}

#endregion
