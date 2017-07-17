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
	public class SMMA : Indicator
	{
		private double	smma1	= 0;
		private double	sum1	= 0;
		private double	prevsum1 = 0;
		private double	prevsmma1 = 0;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @".The Smoothed Moving average";
				Name										= "SMMA";
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
				Period					= 14;
				AddPlot(Brushes.ForestGreen, "SmoothedMA");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
           if(CurrentBar == Period)
			{
				sum1 = SUM(Input,Period)[0];
				smma1 = sum1/Period;
				Value[0] = (smma1);
			}
			else if (CurrentBar > Period)
			{
				if (IsFirstTickOfBar)
				{
					prevsum1 = sum1;
					prevsmma1 = smma1;
				}
				Value[0] = ((prevsum1-prevsmma1+Input[0])/Period);
				sum1 = prevsum1-prevsmma1+Input[0];
				smma1 = (sum1-prevsmma1+Input[0])/Period;
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=1, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SmoothedMA
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
		private SMMA[] cacheSMMA;
		public SMMA SMMA(int period)
		{
			return SMMA(Input, period);
		}

		public SMMA SMMA(ISeries<double> input, int period)
		{
			if (cacheSMMA != null)
				for (int idx = 0; idx < cacheSMMA.Length; idx++)
					if (cacheSMMA[idx] != null && cacheSMMA[idx].Period == period && cacheSMMA[idx].EqualsInput(input))
						return cacheSMMA[idx];
			return CacheIndicator<SMMA>(new SMMA(){ Period = period }, input, ref cacheSMMA);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SMMA SMMA(int period)
		{
			return indicator.SMMA(Input, period);
		}

		public Indicators.SMMA SMMA(ISeries<double> input , int period)
		{
			return indicator.SMMA(input, period);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SMMA SMMA(int period)
		{
			return indicator.SMMA(Input, period);
		}

		public Indicators.SMMA SMMA(ISeries<double> input , int period)
		{
			return indicator.SMMA(input, period);
		}
	}
}

#endregion
