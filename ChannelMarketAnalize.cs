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
	public class ChannelMarketAnalize : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Channel Market Analize";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= false;
				DrawVerticalGridLines						= false;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				AddPlot(Brushes.Orange, "Signal");
			}
			else if (State == State.Configure)
			{
				
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 200 )
				Value[0] = 0;
			else
			{
				Value[0] = entryConditionsChannel(); 
				//Value[0] = 100;
			}
		}
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Channel Long
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////		
		protected double entryConditionsChannel()
		{
			Double signal = 0;		// && High[0] < SMA(10)[0] 
			if ( Close[0] > Math.Abs(SMA(200)[0]) && Close[0] < Math.Abs(SMA(10)[0]) && WilliamsR(10)[0] < -80 ) { //  
				//signal = true;
				//Draw.Dot(this, "CH"+CurrentBar, true, 0, Low[0] - (TickSize * 20), Brushes.DarkGreen);
				signal = 1;
				//double theStop = calcInitialStop(pct: Pct, isLong: true);
				//shares = calcPositionSize(stopPrice: theStop, isLong: true); 
				//entryType = "Channel";
				//textForBox = popuateStatsTextBox( entryType: entryType, shares: shares, maxLoss: MaxRisk , stopPrice: theStop);
				//Print(Time[0].ToShortDateString() +"\n"+ textForBox);
				//Draw.Text(this, "stop"+CurrentBar, "-", 0, theStop);
			} else {
				signal = 0;
			}
			Value[0] = signal;
			return signal;
		}

		#region Properties

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
		private ChannelMarketAnalize[] cacheChannelMarketAnalize;
		public ChannelMarketAnalize ChannelMarketAnalize()
		{
			return ChannelMarketAnalize(Input);
		}

		public ChannelMarketAnalize ChannelMarketAnalize(ISeries<double> input)
		{
			if (cacheChannelMarketAnalize != null)
				for (int idx = 0; idx < cacheChannelMarketAnalize.Length; idx++)
					if (cacheChannelMarketAnalize[idx] != null &&  cacheChannelMarketAnalize[idx].EqualsInput(input))
						return cacheChannelMarketAnalize[idx];
			return CacheIndicator<ChannelMarketAnalize>(new ChannelMarketAnalize(), input, ref cacheChannelMarketAnalize);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChannelMarketAnalize ChannelMarketAnalize()
		{
			return indicator.ChannelMarketAnalize(Input);
		}

		public Indicators.ChannelMarketAnalize ChannelMarketAnalize(ISeries<double> input )
		{
			return indicator.ChannelMarketAnalize(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChannelMarketAnalize ChannelMarketAnalize()
		{
			return indicator.ChannelMarketAnalize(Input);
		}

		public Indicators.ChannelMarketAnalize ChannelMarketAnalize(ISeries<double> input )
		{
			return indicator.ChannelMarketAnalize(input);
		}
	}
}

#endregion
