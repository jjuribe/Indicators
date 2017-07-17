// 
// Copyright (C) 2016, NinjaTrader LLC <www.ninjatrader.com>.
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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// VwmaStdDev Bands are plotted at standard deviation levels above and below a moving average. 
	/// Since standard deviation is a measure of volatility, the bands are self-adjusting: 
	/// widening during volatile markets and contracting during calmer periods.
	/// </summary>
	public class VwmaStdDev : Indicator
	{
		private SMA						sma;
		private StdDev					stdDev;
		private double					multiplier05			= 0.5;
		private double					multiplier1				= 1.0;
		private double					multiplier15			= 1.5;
		private double					multiplier2				= 2.0;
		private double					multiplier3				= 3.0;
		private double					offset					= 0.0;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionBollinger;
				Name						= "VwmaStdDev";
				IsOverlay					= true;
				IsSuspendedWhileInactive	= true;
				NumStdDev					= 2;
				Period						= 42;

				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.BollingerUpperBand);
				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.BollingerMiddleBand);
				AddPlot(Brushes.Goldenrod, NinjaTrader.Custom.Resource.BollingerLowerBand);
			}
			else if (State == State.DataLoaded)
			{
				sma		= SMA(Period);
				stdDev	= StdDev(Period);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < Period)
			return;

			double sma0		= VWMA(Close, Period)[0];
			double stdDev0	= stdDev[0];
			
			int rangeLen = 100;
			int smoothLen = 100;
			double bandOne = 1;
			double bandTwo = 2;
			double bandThree = 3;
			
			double smoothRange = SMA(ATR(rangeLen), smoothLen)[0];
			//Print("The current ATR value is " + rangeValue.ToString());

			Upper[0]		= sma0 + ( smoothRange * bandTwo );
			Middle[0]		= sma0;
			Lower[0]		= sma0 - ( smoothRange * bandTwo);
		}

		#region Properties
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Middle
		{
			get { return Values[1]; }
		}

		[Range(0, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NumStdDev", GroupName = "NinjaScriptParameters", Order = 0)]
		public double NumStdDev
		{ get; set; }

		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 1)]
		public int Period
		{ get; set; }

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
		private VwmaStdDev[] cacheVwmaStdDev;
		public VwmaStdDev VwmaStdDev(double numStdDev, int period)
		{
			return VwmaStdDev(Input, numStdDev, period);
		}

		public VwmaStdDev VwmaStdDev(ISeries<double> input, double numStdDev, int period)
		{
			if (cacheVwmaStdDev != null)
				for (int idx = 0; idx < cacheVwmaStdDev.Length; idx++)
					if (cacheVwmaStdDev[idx] != null && cacheVwmaStdDev[idx].NumStdDev == numStdDev && cacheVwmaStdDev[idx].Period == period && cacheVwmaStdDev[idx].EqualsInput(input))
						return cacheVwmaStdDev[idx];
			return CacheIndicator<VwmaStdDev>(new VwmaStdDev(){ NumStdDev = numStdDev, Period = period }, input, ref cacheVwmaStdDev);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.VwmaStdDev VwmaStdDev(double numStdDev, int period)
		{
			return indicator.VwmaStdDev(Input, numStdDev, period);
		}

		public Indicators.VwmaStdDev VwmaStdDev(ISeries<double> input , double numStdDev, int period)
		{
			return indicator.VwmaStdDev(input, numStdDev, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.VwmaStdDev VwmaStdDev(double numStdDev, int period)
		{
			return indicator.VwmaStdDev(Input, numStdDev, period);
		}

		public Indicators.VwmaStdDev VwmaStdDev(ISeries<double> input , double numStdDev, int period)
		{
			return indicator.VwmaStdDev(input, numStdDev, period);
		}
	}
}

#endregion
