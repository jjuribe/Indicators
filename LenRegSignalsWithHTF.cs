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
	public class LenRegSignalsWithHTF : Indicator
	{
		private Indicators.RegressionChannel RegressionChannel1;
		
		// HTF Values
		private double UTFst;
		private int UTFdir = 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "LenRegSignalsWithHTF";
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
				RegressionChannel1				= RegressionChannel(125, 4);
				RegressionChannel1.Plots[0].Brush = Brushes.DarkGray;
				RegressionChannel1.Plots[1].Brush = Brushes.DodgerBlue;
				RegressionChannel1.Plots[2].Brush = Brushes.DodgerBlue;
				//AddChartIndicator(RegressionChannel1);
				//TSSuperTrend1				= TSSuperTrend(SuperTrendMode.ATR, MovingAverageType.HMA, 14, 2.2, 14, false, false, false, false);
			}
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			//print("Connection Lost");
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
			
			// day bars
			if (BarsInProgress == 1)
			{
				UTFst = TSSuperTrend(SuperTrendMode.ATR, MovingAverageType.HMA, 14, 2.2, 14, false, false, false, false)[0];
				return;
			}
			
			// lower time frame bars
			if (BarsInProgress == 0)
			{
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
				
				// plot ltf activity
				// set long signal form HTF
				if (Close[0] > UTFst && UTFst > 0 )
				{
					UTFdir = 1;
					Draw.TriangleUp(this, @"HTF Up"+CurrentBar.ToString(), true, 0, UTFst - TickSize, Brushes.DodgerBlue);
				}  
				else
				{
					UTFdir = 0;
					if (UTFst > 0 ) {
						Draw.TriangleDown(this, @"HTF dn "+CurrentBar.ToString(), true, 0, UTFst + TickSize, Brushes.Crimson);
					}
				}
				
				// Set Long Signal
				if ((Low[0] <= lowerBandOne) && (UTFdir == 1))
				{
	
					Draw.ArrowUp(this, @"lin reg long"+CurrentBar.ToString(), true, 0, Low[0]- 0.0001, Brushes.Lime);
				}
				
				// Set Short Signal from lin reg
				if ((High[0] >= upperBandOne ) && (UTFdir == 0))
				{
					Draw.ArrowDown(this, @"lin reg short"+CurrentBar.ToString(), true, 0, High[0]+ 0.0001, Brushes.Red);
				} 

				return;
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
		private LenRegSignalsWithHTF[] cacheLenRegSignalsWithHTF;
		public LenRegSignalsWithHTF LenRegSignalsWithHTF(bool colorBarsDailyStrend)
		{
			return LenRegSignalsWithHTF(Input, colorBarsDailyStrend);
		}

		public LenRegSignalsWithHTF LenRegSignalsWithHTF(ISeries<double> input, bool colorBarsDailyStrend)
		{
			if (cacheLenRegSignalsWithHTF != null)
				for (int idx = 0; idx < cacheLenRegSignalsWithHTF.Length; idx++)
					if (cacheLenRegSignalsWithHTF[idx] != null && cacheLenRegSignalsWithHTF[idx].ColorBarsDailyStrend == colorBarsDailyStrend && cacheLenRegSignalsWithHTF[idx].EqualsInput(input))
						return cacheLenRegSignalsWithHTF[idx];
			return CacheIndicator<LenRegSignalsWithHTF>(new LenRegSignalsWithHTF(){ ColorBarsDailyStrend = colorBarsDailyStrend }, input, ref cacheLenRegSignalsWithHTF);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.LenRegSignalsWithHTF LenRegSignalsWithHTF(bool colorBarsDailyStrend)
		{
			return indicator.LenRegSignalsWithHTF(Input, colorBarsDailyStrend);
		}

		public Indicators.LenRegSignalsWithHTF LenRegSignalsWithHTF(ISeries<double> input , bool colorBarsDailyStrend)
		{
			return indicator.LenRegSignalsWithHTF(input, colorBarsDailyStrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.LenRegSignalsWithHTF LenRegSignalsWithHTF(bool colorBarsDailyStrend)
		{
			return indicator.LenRegSignalsWithHTF(Input, colorBarsDailyStrend);
		}

		public Indicators.LenRegSignalsWithHTF LenRegSignalsWithHTF(ISeries<double> input , bool colorBarsDailyStrend)
		{
			return indicator.LenRegSignalsWithHTF(input, colorBarsDailyStrend);
		}
	}
}

#endregion
