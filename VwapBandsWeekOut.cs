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
	public class VwapBandsWeekOut : Indicator
	{
		private 	StdDev			stdDev;
		double 		sma0;	
		double 		smoothRange;
		
//		double 		bandRange 				= 0.125;
//		int			VwmaAverage				= 42;
//		int			RangeLength				= 100;
//		int			SmoothLength			= 100;
		
//		int			BandOne					= 1;
//		int			BandTwo					= 2;
//		double		BandThree				= 3;
		
		// time series for bands
		private Series<double> Vwma;
		private Series<double> VwmaU;
		private Series<double> VwmaL;
		
		private Series<double> UpperBandOneU;
		private Series<double> UpperBandOneL;
		private Series<double> UpperBandOne;
		
		private Series<double> UpperBandTwoU;
		private Series<double> UpperBandTwoL;
		private Series<double> UpperBandTwo;
		
		private Series<double> UpperBandThreeU;
		private Series<double> UpperBandThreeL;
		private Series<double> UpperBandThree;
		
		private Series<double> LowerBandOneU;
		private Series<double> LowerBandOneL;
		private Series<double> LowerBandOne;
		
		private Series<double> LowerBandTwoU;
		private Series<double> LowerBandTwoL;
		private Series<double> LowerBandTwo;
		
		private Series<double> LowerBandThreeU;
		private Series<double> LowerBandThreeL;
		private Series<double> LowerBandThree;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "VwapBandsWeekOut";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= false;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				AddPlot(Brushes.Orange, "OutputSignal");
				
				// added inputs
				BandRange				= 1;
				Average					= 42;
				RangeLength				= 100;
				SmoothLength			= 100;
				BandOne					= 1;
				BandTwo					= 2;
				BandThree				= 3;
				ColorBars				= true;
				PlotArrows				= true;
				
//				CSIPosOne = 100;
//				CSIPosTwo = 180;
//				CSINegOne = -100;
//				CSINegTwo = -180;
//				RSIperiod = 14;
				//		double 		bandRange 				= 0.125;
				//		int			VwmaAverage				= 42;
				//		int			RangeLength				= 100;
				//		int			SmoothLength			= 100;
						
				//		int			BandOne					= 1;
				//		int			BandTwo					= 2;
				//		double		BandThree				= 3;

			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Week, 1);
			} else if (State == State.DataLoaded)
			{
				stdDev	= StdDev(Average);
				
				Vwma = new Series<double>(this, MaximumBarsLookBack.Infinite);
				VwmaU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				VwmaL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				UpperBandOneU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				UpperBandOneL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				UpperBandOne = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				UpperBandTwoU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				UpperBandTwoL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				UpperBandTwo = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				UpperBandThreeU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				UpperBandThreeL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				UpperBandThree = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				LowerBandOneU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowerBandOneL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowerBandOne = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				LowerBandTwoU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowerBandTwoL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowerBandTwo = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				LowerBandThreeU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowerBandThreeL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowerBandThree = new Series<double>(this, MaximumBarsLookBack.Infinite);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 20)
			return;
			
			OutputSignal[0] = 0;
			
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
				sma0		= VWMA(Close, Average)[0];			
				smoothRange = SMA(ATR(RangeLength), SmoothLength)[0];
				return;
			}
			
			// lower time frame bars
			if (BarsInProgress == 0)
			{
			
				
				UpperBandThree[0]	= sma0 + ( smoothRange * BandThree );
				//Print(UpperBandThree[0]);
				UpperBandTwo[0]		= sma0 + ( smoothRange * BandTwo );
				UpperBandOne[0]		= sma0 + ( smoothRange * BandOne );	
				Vwma[0]				= sma0;	
				LowerBandOne[0]		= sma0 - ( smoothRange * BandOne);
				LowerBandTwo[0]		= sma0 - ( smoothRange * BandTwo);
				LowerBandThree[0]	= sma0 - ( smoothRange * BandThree);
					
				// time series for bands
				VwmaU[0] = sma0  + (smoothRange* BandRange);
				VwmaL[0] = sma0  - (smoothRange* BandRange); 
				
				UpperBandOneU[0] = sma0 + ( smoothRange * BandOne) + (smoothRange* BandRange);
				UpperBandOneL[0] = sma0 + ( smoothRange * BandOne) - (smoothRange* BandRange);
			
				UpperBandTwoU[0] = sma0 + ( smoothRange * BandTwo) + (smoothRange* BandRange);
				UpperBandTwoL[0] = sma0 + ( smoothRange * BandTwo) - (smoothRange* BandRange);
				
				UpperBandThreeU[0] = sma0 + ( smoothRange * BandThree) + (smoothRange* BandRange);
				UpperBandThreeL[0] = sma0 + ( smoothRange * BandThree) - (smoothRange* BandRange);
				
				LowerBandOneU[0] = sma0 - ( smoothRange * BandOne) + (smoothRange* BandRange);
				LowerBandOneL[0] = sma0 - ( smoothRange * BandOne) - (smoothRange* BandRange);
			
				LowerBandTwoU[0] = sma0 - ( smoothRange * BandTwo) + (smoothRange* BandRange);
				LowerBandTwoL[0] = sma0 - ( smoothRange * BandTwo) - (smoothRange* BandRange);
				
				LowerBandThreeU[0] = sma0 - ( smoothRange * BandThree) + (smoothRange* BandRange);
				LowerBandThreeL[0] = sma0 - ( smoothRange * BandThree) - (smoothRange* BandRange);
				
				// Draw a region over std dev lines
				
				
//				Draw.Region(this, "UpperBandOne"+CurrentBar.ToString(), CurrentBar, 0, UpperBandOneL,  UpperBandOneU, null, Brushes.Crimson, 1);
//				Draw.Region(this, "UpperBandTwo"+CurrentBar.ToString(), CurrentBar, 0, UpperBandTwoL,  UpperBandTwoU, null, Brushes.Crimson, 1);
//				Draw.Region(this, "UpperBandThree"+CurrentBar.ToString(), CurrentBar, 0, UpperBandThreeL,  UpperBandThreeU, null, Brushes.Crimson, 1);
				
//				Draw.Region(this, "LowerBandOne"+CurrentBar.ToString(), CurrentBar, 0, LowerBandOneL,  LowerBandOneU, null, Brushes.DarkBlue, 1);
//				Draw.Region(this, "LowerBandTwo"+CurrentBar.ToString(), CurrentBar, 0, LowerBandTwoL,  LowerBandTwoU, null, Brushes.DarkBlue, 1);
//				Draw.Region(this, "LowerBandThree"+CurrentBar.ToString(), CurrentBar, 0, LowerBandThreeL,  LowerBandThreeU, null, Brushes.DarkBlue, 1);
		
			
				//OutputSignal[0] = 0;
				
				// show price near Vwma	
				if ((High[0] > VwmaL[0] && Low[0] < VwmaL[0]) || (High[0] < VwmaU[0] && Low[0] > VwmaL[0]) || (High[0] > VwmaU[0] && Low[0] < VwmaU[0]))
					{
						OutputSignal[0] = 4;
					} 		
		
				// show price near Upper Band One	
				if ((High[0] > UpperBandOneL[0] && Low[0] < UpperBandOneL[0]) || (High[0] < UpperBandOneU[0] && Low[0] > UpperBandOneL[0]) || (High[0] > UpperBandOneU[0] && Low[0] < UpperBandOneU[0]))
					{
						OutputSignal[0] = 1;
					} 	
				// show price near Upper Band Two	
				if ((High[0] > UpperBandTwoL[0] && Low[0] < UpperBandTwoL[0]) || (High[0] < UpperBandTwoU[0] && Low[0] > UpperBandTwoL[0]) || (High[0] > UpperBandTwoU[0] && Low[0] < UpperBandTwoU[0]))
					{
						OutputSignal[0] = 2;
					} 	
				// show price near Upper Band Three	
				if ((High[0] > UpperBandThreeL[0] && Low[0] < UpperBandThreeL[0]) || (High[0] < UpperBandThreeU[0] && Low[0] > UpperBandThreeL[0]) || (High[0] > UpperBandThreeU[0] && Low[0] < UpperBandThreeU[0]))
					{
						OutputSignal[0] = 3;
					} 				
					
				// show price near Lower Band One	
				if ((High[0] > LowerBandOneL[0] && Low[0] < LowerBandOneL[0]) || (High[0] < LowerBandOneU[0] && Low[0] > LowerBandOneL[0]) || (High[0] > LowerBandOneU[0] && Low[0] < LowerBandOneU[0]))
					{
						OutputSignal[0] = -1;
					} 
				// show price near Lower Band Two	
				if ((High[0] > LowerBandTwoL[0] && Low[0] < LowerBandTwoL[0]) || (High[0] < LowerBandTwoU[0] && Low[0] > LowerBandTwoL[0]) || (High[0] > LowerBandTwoU[0] && Low[0] < LowerBandTwoU[0]))
					{
						OutputSignal[0] = -2;
					} 
					
				// show price near Lower Band Three	
				if ((High[0] > LowerBandThreeL[0] && Low[0] < LowerBandThreeL[0]) || (High[0] < LowerBandThreeU[0] && Low[0] > LowerBandThreeL[0]) || (High[0] > LowerBandThreeU[0] && Low[0] < LowerBandThreeU[0]))
					{
						OutputSignal[0] = -3;
					} 
				
//				if (OutputSignal[0] == 0) {
//					Print("Signal is 0");	
//				}
				
				//Print("---------:" + OutputSignal[0].ToString());
				
				return;
				
				
			}
			
			
		
		}

		#region Properties
	
//		[NinjaScriptProperty]
//		[Range(1, int.MaxValue)]
//		[Display(Name="RSI Period", Order=10, GroupName="Parameters")]
//		public int RSIperiod
//		{ get; set; }
		
//		[NinjaScriptProperty]
//		[Range(-300, int.MaxValue)]
//		[Display(Name="RSI + One", Order=11, GroupName="Parameters")]
//		public int CSIPosOne
//		{ get; set; }
		
		
//		[NinjaScriptProperty]
//		[Range(-300, int.MaxValue)]
//		[Display(Name="RSI + Two", Order=12, GroupName="Parameters")]
//		public int CSIPosTwo
//		{ get; set; }
		
//		[NinjaScriptProperty]
//		[Range(-300, int.MaxValue)]
//		[Display(Name="RSI - One", Order=13, GroupName="Parameters")]
//		public int CSINegOne
//		{ get; set; }
		
//		[NinjaScriptProperty]
//		[Range(-300, int.MaxValue)]
//		[Display(Name="RSI - Two", Order=14, GroupName="Parameters")]
//		public int CSINegTwo
//		{ get; set; }

		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="BandRange", Order=1, GroupName="Parameters")]
		public double BandRange
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Average", Order=2, GroupName="Parameters")]
		public int Average
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RangeLength", Order=3, GroupName="Parameters")]
		public int RangeLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SmoothLength", Order=4, GroupName="Parameters")]
		public int SmoothLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="BandOne", Order=5, GroupName="Parameters")]
		public double BandOne
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="BandTwo", Order=6, GroupName="Parameters")]
		public double BandTwo
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="BandThree", Order=7, GroupName="Parameters")]
		public double BandThree
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ColorBars", Order=8, GroupName="Parameters")]
		public bool ColorBars
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="PlotArrows", Order=9, GroupName="Parameters")]
		public bool PlotArrows
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> OutputSignal
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
		private VwapBandsWeekOut[] cacheVwapBandsWeekOut;
		public VwapBandsWeekOut VwapBandsWeekOut(double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows)
		{
			return VwapBandsWeekOut(Input, bandRange, average, rangeLength, smoothLength, bandOne, bandTwo, bandThree, colorBars, plotArrows);
		}

		public VwapBandsWeekOut VwapBandsWeekOut(ISeries<double> input, double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows)
		{
			if (cacheVwapBandsWeekOut != null)
				for (int idx = 0; idx < cacheVwapBandsWeekOut.Length; idx++)
					if (cacheVwapBandsWeekOut[idx] != null && cacheVwapBandsWeekOut[idx].BandRange == bandRange && cacheVwapBandsWeekOut[idx].Average == average && cacheVwapBandsWeekOut[idx].RangeLength == rangeLength && cacheVwapBandsWeekOut[idx].SmoothLength == smoothLength && cacheVwapBandsWeekOut[idx].BandOne == bandOne && cacheVwapBandsWeekOut[idx].BandTwo == bandTwo && cacheVwapBandsWeekOut[idx].BandThree == bandThree && cacheVwapBandsWeekOut[idx].ColorBars == colorBars && cacheVwapBandsWeekOut[idx].PlotArrows == plotArrows && cacheVwapBandsWeekOut[idx].EqualsInput(input))
						return cacheVwapBandsWeekOut[idx];
			return CacheIndicator<VwapBandsWeekOut>(new VwapBandsWeekOut(){ BandRange = bandRange, Average = average, RangeLength = rangeLength, SmoothLength = smoothLength, BandOne = bandOne, BandTwo = bandTwo, BandThree = bandThree, ColorBars = colorBars, PlotArrows = plotArrows }, input, ref cacheVwapBandsWeekOut);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VwapBandsWeekOut VwapBandsWeekOut(double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows)
		{
			return indicator.VwapBandsWeekOut(Input, bandRange, average, rangeLength, smoothLength, bandOne, bandTwo, bandThree, colorBars, plotArrows);
		}

		public Indicators.VwapBandsWeekOut VwapBandsWeekOut(ISeries<double> input , double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows)
		{
			return indicator.VwapBandsWeekOut(input, bandRange, average, rangeLength, smoothLength, bandOne, bandTwo, bandThree, colorBars, plotArrows);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VwapBandsWeekOut VwapBandsWeekOut(double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows)
		{
			return indicator.VwapBandsWeekOut(Input, bandRange, average, rangeLength, smoothLength, bandOne, bandTwo, bandThree, colorBars, plotArrows);
		}

		public Indicators.VwapBandsWeekOut VwapBandsWeekOut(ISeries<double> input , double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows)
		{
			return indicator.VwapBandsWeekOut(input, bandRange, average, rangeLength, smoothLength, bandOne, bandTwo, bandThree, colorBars, plotArrows);
		}
	}
}

#endregion
