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
	public class EMJema : Indicator
	{
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "EMJ Cross";
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
				AddPlot(Brushes.DodgerBlue, "fast");
				AddPlot(Brushes.DarkRed, "Medium");
				AddPlot(Brushes.Goldenrod, "Slow");
				
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			double fastMa = EMA(34)[0]; 
			double medMa = EMA(68)[0]; 
			double slowMa = SMA(116)[0]; 
			
			/// Long
			if ( CrossAbove( EMA(34), EMA(68), 1 ) && Close[0] >= slowMa) {
				Draw.ArrowUp(this, "xUP"+CurrentBar.ToString(), true, 1, Close[0] ,Brushes.LimeGreen); 
			}
			/// Short
			if ( CrossBelow( EMA(34), EMA(68), 1 ) && Close[0] <= slowMa) {
				Draw.ArrowDown(this, "xDN"+CurrentBar.ToString(), true, 1, Close[0], Brushes.Red); 
			}
			
			Values[0][0] = fastMa;
			Values[1][0] = medMa;
			Values[2][0] = slowMa;
		}

		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Fast
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Medium
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> Slow
		{
			get { return Values[2]; }
		}

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private EMJema[] cacheEMJema;
		public EMJema EMJema()
		{
			return EMJema(Input);
		}

		public EMJema EMJema(ISeries<double> input)
		{
			if (cacheEMJema != null)
				for (int idx = 0; idx < cacheEMJema.Length; idx++)
					if (cacheEMJema[idx] != null &&  cacheEMJema[idx].EqualsInput(input))
						return cacheEMJema[idx];
			return CacheIndicator<EMJema>(new EMJema(), input, ref cacheEMJema);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.EMJema EMJema()
		{
			return indicator.EMJema(Input);
		}

		public Indicators.EMJema EMJema(ISeries<double> input )
		{
			return indicator.EMJema(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.EMJema EMJema()
		{
			return indicator.EMJema(Input);
		}

		public Indicators.EMJema EMJema(ISeries<double> input )
		{
			return indicator.EMJema(input);
		}
	}
}

#endregion
