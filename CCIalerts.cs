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

// This namespace holds indicators in this folder and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// The Commodity Channel Index (CCIalerts) measures the variation of a security's price 
	/// from its statistical mean. High values show that prices are unusually high 
	/// compared to average prices whereas low values indicate that prices are unusually low.
	/// </summary>
	public class CCIalerts : Indicator
	{
		private SMA sma;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= NinjaTrader.Custom.Resource.NinjaScriptIndicatorDescriptionCCI;
				Name						= "CCIalerts";
				IsSuspendedWhileInactive	= true;
				Period						= 14;

				AddPlot(Brushes.Goldenrod,				NinjaTrader.Custom.Resource.NinjaScriptIndicatorNameCCI);
				AddLine(Brushes.DarkGray,	180,	NinjaTrader.Custom.Resource.CCILevel2);
				AddLine(Brushes.DarkGray,	100,	NinjaTrader.Custom.Resource.CCILevel1);
				AddLine(Brushes.DarkGray,	0,		NinjaTrader.Custom.Resource.NinjaScriptIndicatorZeroLine);
				AddLine(Brushes.DarkGray,	-100,	NinjaTrader.Custom.Resource.CCILevelMinus1);
				AddLine(Brushes.DarkGray,	-180,	NinjaTrader.Custom.Resource.CCILevelMinus2);
			}
			else if (State == State.DataLoaded)
				sma  = SMA(Typical, Period);
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar == 0)
				Value[0] = 0;
			else
			{
				double mean = 0;
				double sma0 = sma[0];

				for (int idx = Math.Min(CurrentBar, Period - 1); idx >= 0; idx--)
					mean += Math.Abs(Typical[idx] - sma0);

				Value[0] = (Typical[0] - sma0) / (mean.ApproxCompare(0) == 0 ? 1 : (0.015 * (mean / Math.Min(Period, CurrentBar + 1))));
				
				// crossing down
				if (CrossBelow(CCI(Period), 180, 1))
      			{
					Draw.ArrowDown(this, "200"+CurrentBar.ToString(), true, 0, High[0] + ( TickSize * 200), Brushes.Red);	
					//Print("We Crossed Below 180");
				}
				
				if (CrossBelow(CCI(Period), 100, 1))
      			{
					Draw.TriangleDown(this, "100"+CurrentBar.ToString(), true, 0, High[0] + ( TickSize * 200), Brushes.Crimson);
					//Print("We Crossed Below 100");
				}
				
				// crossing up
				if (CrossAbove(CCI(Period), -180, 1))
      			{
					Draw.ArrowUp(this, "200"+CurrentBar.ToString(), true, 0, Low[0] - ( TickSize * 200), Brushes.Blue);	
					//Print("We Crossed above 180");
				}
				
				if (CrossAbove(CCI(Period), -100, 1))
      			{
					Draw.TriangleUp(this, "100"+CurrentBar.ToString(), true, 0, Low[0] - ( TickSize * 200), Brushes.DodgerBlue);
					//Print("We Crossed above 100");
				}
				
			
			}
		}

		#region Properties
		[Range(1, int.MaxValue), NinjaScriptProperty]
		[Display(ResourceType = typeof(Custom.Resource), Name = "Period", GroupName = "NinjaScriptParameters", Order = 0)]
		public int Period
		{ get; set; }
		#endregion
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private CCIalerts[] cacheCCIalerts;
		public CCIalerts CCIalerts(int period)
		{
			return CCIalerts(Input, period);
		}

		public CCIalerts CCIalerts(ISeries<double> input, int period)
		{
			if (cacheCCIalerts != null)
				for (int idx = 0; idx < cacheCCIalerts.Length; idx++)
					if (cacheCCIalerts[idx] != null && cacheCCIalerts[idx].Period == period && cacheCCIalerts[idx].EqualsInput(input))
						return cacheCCIalerts[idx];
			return CacheIndicator<CCIalerts>(new CCIalerts(){ Period = period }, input, ref cacheCCIalerts);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CCIalerts CCIalerts(int period)
		{
			return indicator.CCIalerts(Input, period);
		}

		public Indicators.CCIalerts CCIalerts(ISeries<double> input , int period)
		{
			return indicator.CCIalerts(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CCIalerts CCIalerts(int period)
		{
			return indicator.CCIalerts(Input, period);
		}

		public Indicators.CCIalerts CCIalerts(ISeries<double> input , int period)
		{
			return indicator.CCIalerts(input, period);
		}
	}
}

#endregion
