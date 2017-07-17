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
	public class VwapWeeklyCsi : Indicator
	{
		private VwapBandsWeekOut VwapBandsWeekOut1;
		//private VwmaBandsWeekly VwmaBandsWeekly1;
		private double BandValue = 0;
		private double LastBandValue = 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "VwapWeeklyCsi";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				BandRange				= 0.125;
				Average					= 42;
				RangeLength				= 100;
				SmoothLength			= 100;
				BandOne					= 1;
				BandTwo					= 2;
				BandThree				= 3;
				ColorBars				= true;
				PlotArrows				= true;
				//DownColor				= Brushes.Red;
				
				CSIPosOne = 100;
				CSIPosTwo = 180;
				CSINegOne = -100;
				CSINegTwo = -180;
				CCIperiod = 14;
				
				UseLastBandValue = false;
				
				AddPlot(Brushes.Red, "Signal");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Week, 1);
			}
			else if (State == State.DataLoaded)
			{				
				VwapBandsWeekOut1				= VwapBandsWeekOut(BandRange, Average, RangeLength, SmoothLength, BandOne, BandTwo, BandThree, true, true);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 20)
			return;
			
			//Print("Update " + VwapBandsWeekOut1[0].ToString());
			
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
				return;
			}
			
			// lower time frame bars
			if (BarsInProgress == 0)
			{
				if (UseLastBandValue) {
					BandValue  = VwapBandsWeekOut1[0];
					LastBandValue = VwapBandsWeekOut1[1];
				} else {
					BandValue  = VwapBandsWeekOut1[0];
					LastBandValue = VwapBandsWeekOut1[0];
				}
				
				//Print("Daily : " + VwapBandsWeekOut1[0].ToString());
				//Draw.Text(this, "signal"+CurrentBar.ToString(), VwapBandsWeekOut1[1].ToString(), 0, Low[0] -0.001, Brushes.Yellow);
				
				if (BandValue != 0 || LastBandValue != 0)
				{
					// crossing down
					if (CrossBelow(CCI(CCIperiod), CSIPosTwo, 1) )
	      			{
						BarBrush = Brushes.Crimson;
						CandleOutlineBrush = Brushes.Crimson;
						//Draw.ArrowDown(this, "200"+CurrentBar.ToString(), true, 0, High[0] + ( TickSize * 200), Brushes.Red);	
						//Print("We Crossed Below 180");
						Signal[0] = -2;
					}
					
					if (CrossBelow(CCI(CCIperiod), CSIPosOne, 1))
	      			{
						BarBrush = Brushes.Magenta;
						CandleOutlineBrush = Brushes.DarkMagenta;
						//Draw.TriangleDown(this, "100"+CurrentBar.ToString(), true, 0, High[0] + ( TickSize * 200), Brushes.Crimson);
						//Print("We Crossed Below 100");
						Signal[0] = -1;
					}
					
					// crossing up
					if (CrossAbove(CCI(CCIperiod), CSINegTwo, 1))
	      			{
						BarBrush = Brushes.DodgerBlue;
						CandleOutlineBrush = Brushes.DodgerBlue;
						//Draw.ArrowUp(this, "200"+CurrentBar.ToString(), true, 0, Low[0] - ( TickSize * 200), Brushes.Blue);	
						//Print("We Crossed above 180");
						Signal[0] = 2;
					}
					
					if (CrossAbove(CCI(CCIperiod), CSINegOne, 1))
	      			{
						BarBrush = Brushes.Cyan;
						CandleOutlineBrush = Brushes.DarkCyan;
						//Draw.TriangleUp(this, "100"+CurrentBar.ToString(), true, 0, Low[0] - ( TickSize * 200), Brushes.DodgerBlue);
						//Print("We Crossed above 100");
						Signal[0] = 1;
					}
				}
			}

		}

		#region Properties
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
		
		
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="CCI Period", Order=10, GroupName="Parameters")]
		public int CCIperiod
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-300, int.MaxValue)]
		[Display(Name="CCI + One", Order=11, GroupName="Parameters")]
		public int CSIPosOne
		{ get; set; }
		
		
		[NinjaScriptProperty]
		[Range(-300, int.MaxValue)]
		[Display(Name="CCI + Two", Order=12, GroupName="Parameters")]
		public int CSIPosTwo
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-300, int.MaxValue)]
		[Display(Name="CCI - One", Order=13, GroupName="Parameters")]
		public int CSINegOne
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(-300, int.MaxValue)]
		[Display(Name="CCI - Two", Order=14, GroupName="Parameters")]
		public int CSINegTwo
		{ get; set; }
		
		// UseLastBandValue
		[NinjaScriptProperty]
		[Display(Name="Use Last Band Value", Order=15, GroupName="Parameters")]
		public bool UseLastBandValue
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Signal
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
		private VwapWeeklyCsi[] cacheVwapWeeklyCsi;
		public VwapWeeklyCsi VwapWeeklyCsi(double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows, int cCIperiod, int cSIPosOne, int cSIPosTwo, int cSINegOne, int cSINegTwo, bool useLastBandValue)
		{
			return VwapWeeklyCsi(Input, bandRange, average, rangeLength, smoothLength, bandOne, bandTwo, bandThree, colorBars, plotArrows, cCIperiod, cSIPosOne, cSIPosTwo, cSINegOne, cSINegTwo, useLastBandValue);
		}

		public VwapWeeklyCsi VwapWeeklyCsi(ISeries<double> input, double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows, int cCIperiod, int cSIPosOne, int cSIPosTwo, int cSINegOne, int cSINegTwo, bool useLastBandValue)
		{
			if (cacheVwapWeeklyCsi != null)
				for (int idx = 0; idx < cacheVwapWeeklyCsi.Length; idx++)
					if (cacheVwapWeeklyCsi[idx] != null && cacheVwapWeeklyCsi[idx].BandRange == bandRange && cacheVwapWeeklyCsi[idx].Average == average && cacheVwapWeeklyCsi[idx].RangeLength == rangeLength && cacheVwapWeeklyCsi[idx].SmoothLength == smoothLength && cacheVwapWeeklyCsi[idx].BandOne == bandOne && cacheVwapWeeklyCsi[idx].BandTwo == bandTwo && cacheVwapWeeklyCsi[idx].BandThree == bandThree && cacheVwapWeeklyCsi[idx].ColorBars == colorBars && cacheVwapWeeklyCsi[idx].PlotArrows == plotArrows && cacheVwapWeeklyCsi[idx].CCIperiod == cCIperiod && cacheVwapWeeklyCsi[idx].CSIPosOne == cSIPosOne && cacheVwapWeeklyCsi[idx].CSIPosTwo == cSIPosTwo && cacheVwapWeeklyCsi[idx].CSINegOne == cSINegOne && cacheVwapWeeklyCsi[idx].CSINegTwo == cSINegTwo && cacheVwapWeeklyCsi[idx].UseLastBandValue == useLastBandValue && cacheVwapWeeklyCsi[idx].EqualsInput(input))
						return cacheVwapWeeklyCsi[idx];
			return CacheIndicator<VwapWeeklyCsi>(new VwapWeeklyCsi(){ BandRange = bandRange, Average = average, RangeLength = rangeLength, SmoothLength = smoothLength, BandOne = bandOne, BandTwo = bandTwo, BandThree = bandThree, ColorBars = colorBars, PlotArrows = plotArrows, CCIperiod = cCIperiod, CSIPosOne = cSIPosOne, CSIPosTwo = cSIPosTwo, CSINegOne = cSINegOne, CSINegTwo = cSINegTwo, UseLastBandValue = useLastBandValue }, input, ref cacheVwapWeeklyCsi);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VwapWeeklyCsi VwapWeeklyCsi(double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows, int cCIperiod, int cSIPosOne, int cSIPosTwo, int cSINegOne, int cSINegTwo, bool useLastBandValue)
		{
			return indicator.VwapWeeklyCsi(Input, bandRange, average, rangeLength, smoothLength, bandOne, bandTwo, bandThree, colorBars, plotArrows, cCIperiod, cSIPosOne, cSIPosTwo, cSINegOne, cSINegTwo, useLastBandValue);
		}

		public Indicators.VwapWeeklyCsi VwapWeeklyCsi(ISeries<double> input , double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows, int cCIperiod, int cSIPosOne, int cSIPosTwo, int cSINegOne, int cSINegTwo, bool useLastBandValue)
		{
			return indicator.VwapWeeklyCsi(input, bandRange, average, rangeLength, smoothLength, bandOne, bandTwo, bandThree, colorBars, plotArrows, cCIperiod, cSIPosOne, cSIPosTwo, cSINegOne, cSINegTwo, useLastBandValue);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VwapWeeklyCsi VwapWeeklyCsi(double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows, int cCIperiod, int cSIPosOne, int cSIPosTwo, int cSINegOne, int cSINegTwo, bool useLastBandValue)
		{
			return indicator.VwapWeeklyCsi(Input, bandRange, average, rangeLength, smoothLength, bandOne, bandTwo, bandThree, colorBars, plotArrows, cCIperiod, cSIPosOne, cSIPosTwo, cSINegOne, cSINegTwo, useLastBandValue);
		}

		public Indicators.VwapWeeklyCsi VwapWeeklyCsi(ISeries<double> input , double bandRange, int average, int rangeLength, int smoothLength, double bandOne, double bandTwo, double bandThree, bool colorBars, bool plotArrows, int cCIperiod, int cSIPosOne, int cSIPosTwo, int cSINegOne, int cSINegTwo, bool useLastBandValue)
		{
			return indicator.VwapWeeklyCsi(input, bandRange, average, rangeLength, smoothLength, bandOne, bandTwo, bandThree, colorBars, plotArrows, cCIperiod, cSIPosOne, cSIPosTwo, cSINegOne, cSINegTwo, useLastBandValue);
		}
	}
}

#endregion
