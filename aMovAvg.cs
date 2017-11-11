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
	public class aMovAvg : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "aMovAvg";
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
				AddPlot(Brushes.Red, "Upper");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
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
		private aMovAvg[] cacheaMovAvg;
		public aMovAvg aMovAvg()
		{
			return aMovAvg(Input);
		}

		public aMovAvg aMovAvg(ISeries<double> input)
		{
			if (cacheaMovAvg != null)
				for (int idx = 0; idx < cacheaMovAvg.Length; idx++)
					if (cacheaMovAvg[idx] != null &&  cacheaMovAvg[idx].EqualsInput(input))
						return cacheaMovAvg[idx];
			return CacheIndicator<aMovAvg>(new aMovAvg(), input, ref cacheaMovAvg);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.aMovAvg aMovAvg()
		{
			return indicator.aMovAvg(Input);
		}

		public Indicators.aMovAvg aMovAvg(ISeries<double> input )
		{
			return indicator.aMovAvg(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.aMovAvg aMovAvg()
		{
			return indicator.aMovAvg(Input);
		}

		public Indicators.aMovAvg aMovAvg(ISeries<double> input )
		{
			return indicator.aMovAvg(input);
		}
	}
}

#endregion
