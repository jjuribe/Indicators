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
	public class VwmaBandsWeekly : Indicator
	{
		private 	StdDev			stdDev;
		double 		sma0;	
		double 		smoothRange;
		double 		bandRange = 0.125;
		// time series for bands
		private Series<double> UpperBandOneU;
		private Series<double> UpperBandOneL;
		
		private Series<double> UpperBandTwoU;
		private Series<double> UpperBandTwoL;
		
		private Series<double> UpperBandThreeU;
		private Series<double> UpperBandThreeL;
		
		private Series<double> LowerBandOneU;
		private Series<double> LowerBandOneL;
		
		private Series<double> LowerBandTwoU;
		private Series<double> LowerBandTwoL;
		
		private Series<double> LowerBandThreeU;
		private Series<double> LowerBandThreeL;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "VwmaBandsWeekly";
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
				VwmaAverage					= 42;
				RangeLength					= 100;
				SmoothLength					= 100;
				
				BandOne					= 1;
				BandTwo					= 2;
				BandThree					= 3;
				AddPlot(Brushes.DarkGray, "Vwma");
				AddPlot(Brushes.Crimson, "UpperBandOne");
				AddPlot(Brushes.Crimson, "UpperBandTwo");
				AddPlot(Brushes.Crimson, "UpperBandThree");
				AddPlot(Brushes.DodgerBlue, "LowerBandOne");
				AddPlot(Brushes.DodgerBlue, "LowerBandTwo");
				AddPlot(Brushes.DodgerBlue, "LowerBandThree");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Week, 1);
			}
			else if (State == State.DataLoaded)
			{
				stdDev	= StdDev(VwmaAverage);
				
				UpperBandOneU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				UpperBandOneL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				UpperBandTwoU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				UpperBandTwoL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				UpperBandThreeU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				UpperBandThreeL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				LowerBandOneU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowerBandOneL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				LowerBandTwoU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowerBandTwoL = new Series<double>(this, MaximumBarsLookBack.Infinite);
				
				LowerBandThreeU = new Series<double>(this, MaximumBarsLookBack.Infinite);
				LowerBandThreeL = new Series<double>(this, MaximumBarsLookBack.Infinite);
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
				sma0		= VWMA(Close, VwmaAverage)[0];			
				smoothRange = SMA(ATR(RangeLength), SmoothLength)[0];
				//Print(sma0);
				return;
			}
			
			// lower time frame bars
			if (BarsInProgress == 0)
			{
			
				UpperBandThree[0]	= sma0 + ( smoothRange * BandThree );
				UpperBandTwo[0]		= sma0 + ( smoothRange * BandTwo );
				UpperBandOne[0]		= sma0 + ( smoothRange * BandOne );
				Vwma[0]				= sma0;
				LowerBandOne[0]		= sma0 - ( smoothRange * BandOne);
				LowerBandTwo[0]		= sma0 - ( smoothRange * BandTwo);
				LowerBandThree[0]	= sma0 - ( smoothRange * BandThree);
				
				// time series for bands
				UpperBandOneU[0] = sma0 + ( smoothRange * BandOne) + (smoothRange* bandRange);
				UpperBandOneL[0] = sma0 + ( smoothRange * BandOne) - (smoothRange* bandRange);
			
				UpperBandTwoU[0] = sma0 + ( smoothRange * BandTwo) + (smoothRange* bandRange);
				UpperBandTwoL[0] = sma0 + ( smoothRange * BandTwo) - (smoothRange* bandRange);
				
				UpperBandThreeU[0] = sma0 + ( smoothRange * BandThree) + (smoothRange* bandRange);
				UpperBandThreeL[0] = sma0 + ( smoothRange * BandThree) - (smoothRange* bandRange);
				
				LowerBandOneU[0] = sma0 - ( smoothRange * BandOne) + (smoothRange* bandRange);
				LowerBandOneL[0] = sma0 - ( smoothRange * BandOne) - (smoothRange* bandRange);
			
				LowerBandTwoU[0] = sma0 - ( smoothRange * BandTwo) + (smoothRange* bandRange);
				LowerBandTwoL[0] = sma0 - ( smoothRange * BandTwo) - (smoothRange* bandRange);
				
				LowerBandThreeU[0] = sma0 - ( smoothRange * BandThree) + (smoothRange* bandRange);
				LowerBandThreeL[0] = sma0 - ( smoothRange * BandThree) - (smoothRange* bandRange);
				
				// Draw a region over std dev lines
				
				
//				Draw.Region(this, "UpperBandOne"+CurrentBar.ToString(), CurrentBar, 0, UpperBandOneL,  UpperBandOneU, null, Brushes.Crimson, 1);
//				Draw.Region(this, "UpperBandTwo"+CurrentBar.ToString(), CurrentBar, 0, UpperBandTwoL,  UpperBandTwoU, null, Brushes.Crimson, 1);
//				Draw.Region(this, "UpperBandThree"+CurrentBar.ToString(), CurrentBar, 0, UpperBandThreeL,  UpperBandThreeU, null, Brushes.Crimson, 1);
				
//				Draw.Region(this, "LowerBandOne"+CurrentBar.ToString(), CurrentBar, 0, LowerBandOneL,  LowerBandOneU, null, Brushes.DarkBlue, 1);
//				Draw.Region(this, "LowerBandTwo"+CurrentBar.ToString(), CurrentBar, 0, LowerBandTwoL,  LowerBandTwoU, null, Brushes.DarkBlue, 1);
//				Draw.Region(this, "LowerBandThree"+CurrentBar.ToString(), CurrentBar, 0, LowerBandThreeL,  LowerBandThreeU, null, Brushes.DarkBlue, 1);
				
				
				
				return;
			}
			
			
			
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="VwmaAverage", Order=1, GroupName="Parameters")]
		public int VwmaAverage
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RangeLength", Order=2, GroupName="Parameters")]
		public int RangeLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SmoothLength", Order=3, GroupName="Parameters")]
		public int SmoothLength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="BandOne", Order=4, GroupName="Parameters")]
		public double BandOne
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="BandTwo", Order=5, GroupName="Parameters")]
		public double BandTwo
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, double.MaxValue)]
		[Display(Name="BandThree", Order=6, GroupName="Parameters")]
		public double BandThree
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Vwma
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperBandOne
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperBandTwo
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> UpperBandThree
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerBandOne
		{
			get { return Values[4]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerBandTwo
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LowerBandThree
		{
			get { return Values[6]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private VwmaBandsWeekly[] cacheVwmaBandsWeekly;
		public VwmaBandsWeekly VwmaBandsWeekly(int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			return VwmaBandsWeekly(Input, vwmaAverage, rangeLength, smoothLength, bandOne, bandTwo, bandThree);
		}

		public VwmaBandsWeekly VwmaBandsWeekly(ISeries<double> input, int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			if (cacheVwmaBandsWeekly != null)
				for (int idx = 0; idx < cacheVwmaBandsWeekly.Length; idx++)
					if (cacheVwmaBandsWeekly[idx] != null && cacheVwmaBandsWeekly[idx].VwmaAverage == vwmaAverage && cacheVwmaBandsWeekly[idx].RangeLength == rangeLength && cacheVwmaBandsWeekly[idx].SmoothLength == smoothLength && cacheVwmaBandsWeekly[idx].BandOne == bandOne && cacheVwmaBandsWeekly[idx].BandTwo == bandTwo && cacheVwmaBandsWeekly[idx].BandThree == bandThree && cacheVwmaBandsWeekly[idx].EqualsInput(input))
						return cacheVwmaBandsWeekly[idx];
			return CacheIndicator<VwmaBandsWeekly>(new VwmaBandsWeekly(){ VwmaAverage = vwmaAverage, RangeLength = rangeLength, SmoothLength = smoothLength, BandOne = bandOne, BandTwo = bandTwo, BandThree = bandThree }, input, ref cacheVwmaBandsWeekly);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VwmaBandsWeekly VwmaBandsWeekly(int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			return indicator.VwmaBandsWeekly(Input, vwmaAverage, rangeLength, smoothLength, bandOne, bandTwo, bandThree);
		}

		public Indicators.VwmaBandsWeekly VwmaBandsWeekly(ISeries<double> input , int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			return indicator.VwmaBandsWeekly(input, vwmaAverage, rangeLength, smoothLength, bandOne, bandTwo, bandThree);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VwmaBandsWeekly VwmaBandsWeekly(int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			return indicator.VwmaBandsWeekly(Input, vwmaAverage, rangeLength, smoothLength, bandOne, bandTwo, bandThree);
		}

		public Indicators.VwmaBandsWeekly VwmaBandsWeekly(ISeries<double> input , int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			return indicator.VwmaBandsWeekly(input, vwmaAverage, rangeLength, smoothLength, bandOne, bandTwo, bandThree);
		}
	}
}

#endregion
