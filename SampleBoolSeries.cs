// 
// Copyright (C) 2015, NinjaTrader LLC <www.ninjatrader.com>.
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

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    public class SampleBoolSeries : Indicator
    {
		/* We declare two Series<bool> objects here. We will expose these objects with the use of 
		public properties later. When we want to expose an object we should always use the associated
		ISeries class type with it. This will ensure that the value accessed is kept up-to-date. */
		private Series<bool> bearIndication;
		private Series<bool> bullIndication;
		
		/* If you happen to have an object that does not have an ISeries class that can be used, you
		will need to manually ensure its values are kept up-to-date. This process will be done in the
		"Properties" region of the code. */
		private double exposedVariable;

        protected override void OnStateChange()
        {
			if(State == State.SetDefaults)
			{
				Name					= "Sample bool series";
	            Calculate				= Calculate.OnBarClose;
	            IsOverlay				= true;
			}
			
			else if(State == State.Configure)
			{
				/* "this" syncs the Series<bool> to the historical bar object of the indicator. It will generate
				one bool value for every price bar. */
				bearIndication			= new Series<bool>(this);
				bullIndication			= new Series<bool>(this);
			}
        }

        protected override void OnBarUpdate()
        {
			// MACD Crossover: Fast Line cross above Slow Line
			if (CrossAbove(MACD(12, 26, 9), MACD(12, 26, 9).Avg, 1))
			{
				// Paint the current price bar lime to draw our attention to it
				BarBrushes[0]		= Brushes.Lime;
				
				/* This crossover condition is considered bullish so we set the "bullIndication" Series<bool> object to true.
				We also set the "bearIndication" object to false so it does not take on a null value. */
				bullIndication[0]	= (true);
				bearIndication[0]	= (false);
			}
			
			// MACD Crossover: Fast Line cross below Slow Line
			else if (CrossBelow(MACD(12, 26, 9), MACD(12, 26, 9).Avg, 1))
			{
				// Paint the current price bar magenta to draw our attention to it
				BarBrushes[0]		= Brushes.Magenta;
				
				/* This crossover condition is considered bearish so we set the "bearIndication" Series<bool> object to true.
				We also set the "bullIndication" object to false so it does not take on a null value. */
				bullIndication[0]	= (false);
				bearIndication[0]	= (true);
			}
			
			// MACD Crossover: No cross
			else
			{
				/* Since no crosses occured we are not receiving any bullish or bearish signals so we
				set our Series<bool> objects both to false. */
				bullIndication[0] = (false);
				bearIndication[0] = (false);
			}
			
			// We set our variable to the close value.
			exposedVariable = Close[0];
        }

		// Important code segment in the Properties section. Please expand to view.
        #region Properties
		// Creating public properties that access our internal Series<bool> allows external access to this indicator's Series<bool>
		[Browsable(false)]
		[XmlIgnore]
        public Series<bool> BearIndication
        {
            get { return bearIndication; }	// Allows our public BearIndication Series<bool> to access and expose our interal bearIndication Series<bool>
        }
		
		[Browsable(false)]
		[XmlIgnore]		
        public Series<bool>  BullIndication
        {
            get { return bullIndication; }	// Allows our public BullIndication Series<bool> to access and expose our interal bullIndication Series<bool>
        }

        public double ExposedVariable
        {
			// We need to call the Update() method to ensure our exposed variable is in up-to-date.
            get { Update(); return exposedVariable; }
        }
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SampleBoolSeries[] cacheSampleBoolSeries;
		public SampleBoolSeries SampleBoolSeries()
		{
			return SampleBoolSeries(Input);
		}

		public SampleBoolSeries SampleBoolSeries(ISeries<double> input)
		{
			if (cacheSampleBoolSeries != null)
				for (int idx = 0; idx < cacheSampleBoolSeries.Length; idx++)
					if (cacheSampleBoolSeries[idx] != null &&  cacheSampleBoolSeries[idx].EqualsInput(input))
						return cacheSampleBoolSeries[idx];
			return CacheIndicator<SampleBoolSeries>(new SampleBoolSeries(), input, ref cacheSampleBoolSeries);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SampleBoolSeries SampleBoolSeries()
		{
			return indicator.SampleBoolSeries(Input);
		}

		public Indicators.SampleBoolSeries SampleBoolSeries(ISeries<double> input )
		{
			return indicator.SampleBoolSeries(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SampleBoolSeries SampleBoolSeries()
		{
			return indicator.SampleBoolSeries(Input);
		}

		public Indicators.SampleBoolSeries SampleBoolSeries(ISeries<double> input )
		{
			return indicator.SampleBoolSeries(input);
		}
	}
}

#endregion
