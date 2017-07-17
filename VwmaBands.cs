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
	public class VwmaBands : Indicator
	{
		private StdDev					stdDev;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "VwmaBands";
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
			else if (State == State.DataLoaded)
			{
				stdDev	= StdDev(VwmaAverage);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < VwmaAverage)
			return;

			double sma0		= VWMA(Close, VwmaAverage)[0];			
			double smoothRange = SMA(ATR(RangeLength), SmoothLength)[0];
			
			UpperBandThree[0]	= sma0 + ( smoothRange * BandThree );
			UpperBandTwo[0]		= sma0 + ( smoothRange * BandTwo );
			UpperBandOne[0]		= sma0 + ( smoothRange * BandOne );
			Vwma[0]				= sma0;
			LowerBandOne[0]		= sma0 - ( smoothRange * BandOne);
			LowerBandTwo[0]		= sma0 - ( smoothRange * BandTwo);
			LowerBandThree[0]	= sma0 - ( smoothRange * BandThree);
			
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
		private VwmaBands[] cacheVwmaBands;
		public VwmaBands VwmaBands(int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			return VwmaBands(Input, vwmaAverage, rangeLength, smoothLength, bandOne, bandTwo, bandThree);
		}

		public VwmaBands VwmaBands(ISeries<double> input, int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			if (cacheVwmaBands != null)
				for (int idx = 0; idx < cacheVwmaBands.Length; idx++)
					if (cacheVwmaBands[idx] != null && cacheVwmaBands[idx].VwmaAverage == vwmaAverage && cacheVwmaBands[idx].RangeLength == rangeLength && cacheVwmaBands[idx].SmoothLength == smoothLength && cacheVwmaBands[idx].BandOne == bandOne && cacheVwmaBands[idx].BandTwo == bandTwo && cacheVwmaBands[idx].BandThree == bandThree && cacheVwmaBands[idx].EqualsInput(input))
						return cacheVwmaBands[idx];
			return CacheIndicator<VwmaBands>(new VwmaBands(){ VwmaAverage = vwmaAverage, RangeLength = rangeLength, SmoothLength = smoothLength, BandOne = bandOne, BandTwo = bandTwo, BandThree = bandThree }, input, ref cacheVwmaBands);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VwmaBands VwmaBands(int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			return indicator.VwmaBands(Input, vwmaAverage, rangeLength, smoothLength, bandOne, bandTwo, bandThree);
		}

		public Indicators.VwmaBands VwmaBands(ISeries<double> input , int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			return indicator.VwmaBands(input, vwmaAverage, rangeLength, smoothLength, bandOne, bandTwo, bandThree);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VwmaBands VwmaBands(int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			return indicator.VwmaBands(Input, vwmaAverage, rangeLength, smoothLength, bandOne, bandTwo, bandThree);
		}

		public Indicators.VwmaBands VwmaBands(ISeries<double> input , int vwmaAverage, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree)
		{
			return indicator.VwmaBands(input, vwmaAverage, rangeLength, smoothLength, bandOne, bandTwo, bandThree);
		}
	}
}

#endregion
