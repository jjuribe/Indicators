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
	public class ATRPct : Indicator
	{
		private ATR		atr;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "ATR Pct";
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
				Period					= 14;
				AddPlot(Brushes.Snow, "ATRpctPlot");
			}
			else if (State == State.DataLoaded)
			{
				atr		= ATR(Period);
				ClearOutputWindow(); 
			}
		}

		protected override void OnBarUpdate()
		{
			ATRpctPlot[0] =  atr[0] / Close[0];
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ATRpctPlot
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
		private ATRPct[] cacheATRPct;
		public ATRPct ATRPct(int period)
		{
			return ATRPct(Input, period);
		}

		public ATRPct ATRPct(ISeries<double> input, int period)
		{
			if (cacheATRPct != null)
				for (int idx = 0; idx < cacheATRPct.Length; idx++)
					if (cacheATRPct[idx] != null && cacheATRPct[idx].Period == period && cacheATRPct[idx].EqualsInput(input))
						return cacheATRPct[idx];
			return CacheIndicator<ATRPct>(new ATRPct(){ Period = period }, input, ref cacheATRPct);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ATRPct ATRPct(int period)
		{
			return indicator.ATRPct(Input, period);
		}

		public Indicators.ATRPct ATRPct(ISeries<double> input , int period)
		{
			return indicator.ATRPct(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ATRPct ATRPct(int period)
		{
			return indicator.ATRPct(Input, period);
		}

		public Indicators.ATRPct ATRPct(ISeries<double> input , int period)
		{
			return indicator.ATRPct(input, period);
		}
	}
}

#endregion
