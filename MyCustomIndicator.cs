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
	public class MyCustomIndicator : Indicator
	{
		/// <summary>
		///  tend
		/// </summary>
		private double smaTwoHundred;
		private double twoPctUp;
		private double twoPctDn;
		private bool bear = false, bull = false, sideways = false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MyCustomIndicator";
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
				ShowVolatilityText					= true;
				AddPlot(Brushes.Gainsboro, "MarketCond");
			}
			else if (State == State.Configure)
			{
				AddDataSeries("SPY", Data.BarsPeriodType.Day, 1, Data.MarketDataType.Last);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < BarsRequiredToPlot || CurrentBars[1] < BarsRequiredToPlot)
        		return;
			
			var trendInt = setTrend(debug: true); // 1 bull, -1 bear, 0 sideways
		}
		
		protected void setBands(bool debug)
		{
			/// use SPY Series
			smaTwoHundred		= Math.Abs(SMA( BarsArray[1], 200)[0]);
			twoPctUp = Math.Abs(( smaTwoHundred * 0.02 ) + smaTwoHundred);
			twoPctDn = Math.Abs(( smaTwoHundred * 0.02 ) - smaTwoHundred);
			
			if ( debug ) { Print(smaTwoHundred + " + " + twoPctUp + " - " + twoPctDn);}
		}
		
		protected int setTrend(bool debug)
		{
			setBands(debug: debug);
			///  Bull =  Close  > Upper Band  200 MA + 2%
			///  Bear = Close < Lower Band 200 MA - 2%
			///  Sideway = Close inside Bands
			double spyClose = Math.Abs(Closes[1][0]);
			
			if ( spyClose > twoPctUp ) {
				bull = true; 
				bear = false; 
				sideways = false; 
				return 1;
			}
			else if ( spyClose < twoPctDn ) {
				bull = false; 
				bear = true; 
				sideways = false;
				return-1;
			}
			else {		
				bull = false; 
				bear = false; 
				sideways = true;
				return 0;
			}
			if ( debug ) { Print(" v " + bear + " ^ " + bull + " <> " + sideways);}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ShowVolatilityText", Order=1, GroupName="Parameters")]
		public bool ShowVolatilityText
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MarketCond
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
		private MyCustomIndicator[] cacheMyCustomIndicator;
		public MyCustomIndicator MyCustomIndicator(bool showVolatilityText)
		{
			return MyCustomIndicator(Input, showVolatilityText);
		}

		public MyCustomIndicator MyCustomIndicator(ISeries<double> input, bool showVolatilityText)
		{
			if (cacheMyCustomIndicator != null)
				for (int idx = 0; idx < cacheMyCustomIndicator.Length; idx++)
					if (cacheMyCustomIndicator[idx] != null && cacheMyCustomIndicator[idx].ShowVolatilityText == showVolatilityText && cacheMyCustomIndicator[idx].EqualsInput(input))
						return cacheMyCustomIndicator[idx];
			return CacheIndicator<MyCustomIndicator>(new MyCustomIndicator(){ ShowVolatilityText = showVolatilityText }, input, ref cacheMyCustomIndicator);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MyCustomIndicator MyCustomIndicator(bool showVolatilityText)
		{
			return indicator.MyCustomIndicator(Input, showVolatilityText);
		}

		public Indicators.MyCustomIndicator MyCustomIndicator(ISeries<double> input , bool showVolatilityText)
		{
			return indicator.MyCustomIndicator(input, showVolatilityText);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MyCustomIndicator MyCustomIndicator(bool showVolatilityText)
		{
			return indicator.MyCustomIndicator(Input, showVolatilityText);
		}

		public Indicators.MyCustomIndicator MyCustomIndicator(ISeries<double> input , bool showVolatilityText)
		{
			return indicator.MyCustomIndicator(input, showVolatilityText);
		}
	}
}

#endregion
