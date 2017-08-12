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
	public class Sideways : Indicator
	{
		private double twoPctUp;
		private	double twoPctDn;
		private int consolCount;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Sideways";
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
				AddPlot(new Stroke(Brushes.DimGray, 2), PlotStyle.Bar, "SideHist");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if(CurrentBar < 20) { return; }
			
			setBands( Pct: 0.005, Length: 10);
			SideHist[0] = 0;
			if ( High[0] < twoPctUp && Low[0] > twoPctDn ) {
				SideHist[0] = 0.5;
				consolCount ++;
			} else  {
				consolCount = 0;
			}
			if ( consolCount > 3 ) {
				SideHist[0] = 2;
				double maxConsol = MAX(High, 3)[0];
				double minConsol = MIN(Low, 3)[0];
				double medianConsol = ( maxConsol - minConsol ) + minConsol;
				Draw.Line(this, "consolMid", 0, medianConsol, 3, medianConsol, Brushes.Red);
			}
		}

		protected void setBands(double Pct, int Length)
		{
			double sma0		= Math.Abs(SMA(Length)[0]);
			twoPctUp = Math.Abs(( sma0 * Pct ) + sma0);
			twoPctDn = Math.Abs(( sma0 * Pct ) - sma0);

		}
		
//		protected void setTrend(bool debug)
//		{
//			///  Bull =  Close  > Upper Band  200 MA + 2%
//			///  Bear = Close < Lower Band 200 MA - 2%
//			///  Sideway = Close inside Bands
//			double todayClose = Math.Abs(Close[0]);
			
//			if ( todayClose > twoPctUp ) {
//				bull = true; 
//				bear = false; 
//				sideways = false; 
//			}
					
//			else if ( todayClose < twoPctDn ) {
//				bull = false; 
//				bear = true; 
//				sideways = false;
//			}
//			else {		
//				bull = false; 
//				bear = false; 
//				sideways = true;
//			}
			
//			if (bull && showBands) {
//				PlotBrushes[0][0] = Brushes.DodgerBlue;
//				PlotBrushes[1][0] = Brushes.DodgerBlue;
//			}
//			if (bear && showBands ) {
//				PlotBrushes[0][0] = Brushes.Crimson;
//				PlotBrushes[1][0] = Brushes.Crimson;
//			}
//			if (sideways && showBands ) {
//				PlotBrushes[0][0] = Brushes.Goldenrod;
//				PlotBrushes[1][0] = Brushes.Goldenrod;
//			}
//			if ( debug ) { Print(" v " + bear + " ^ " + bull + " <> " + sideways);}
//		}
		
		#region Properties

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SideHist
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
		private Sideways[] cacheSideways;
		public Sideways Sideways()
		{
			return Sideways(Input);
		}

		public Sideways Sideways(ISeries<double> input)
		{
			if (cacheSideways != null)
				for (int idx = 0; idx < cacheSideways.Length; idx++)
					if (cacheSideways[idx] != null &&  cacheSideways[idx].EqualsInput(input))
						return cacheSideways[idx];
			return CacheIndicator<Sideways>(new Sideways(), input, ref cacheSideways);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.Sideways Sideways()
		{
			return indicator.Sideways(Input);
		}

		public Indicators.Sideways Sideways(ISeries<double> input )
		{
			return indicator.Sideways(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.Sideways Sideways()
		{
			return indicator.Sideways(Input);
		}

		public Indicators.Sideways Sideways(ISeries<double> input )
		{
			return indicator.Sideways(input);
		}
	}
}

#endregion
