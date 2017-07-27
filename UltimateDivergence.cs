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
	public class UltimateDivergence : Indicator
	{
		private UltimateOscillator UltimateOscillator1;
		private Swing 		Swing1;
		private Swing 		Swing2;
		private Series<double> lowSwingOsc;
		
		/*
		bool higherLowOscFound = higherLowOSC();
			bool lowerLowPriceFound = lowerLowPrice();
		*/
		private Series<bool> higherLowOscFound;
		private Series<bool> lowerLowPriceFound;
		
		double swingLowO;
		double swingHighO;
		double lastSwingLowO;
		double lastSwingHighO;
		
		double swingLowP;
		double swingHighP;
		double lastSwingLowP;
		double lastSwingHighP;
		
		int lastSwingLowObar = 0; 
		int swingLowObar = 0;
		int lastSwingLowPbar = 0;
		int swingLowPbar = 0;
		
		int minSwingLowOdist = 3;
		int swingLowOdist;
		
		bool drawText = false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "UltimateDivergence";
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
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				UltimateOscillator1	= UltimateOscillator(7, 14, 28);
				Swing1				= Swing( UltimateOscillator1, 3);
				Swing2				= Swing(3);
				lowSwingOsc 		= new Series<double>(this, MaximumBarsLookBack.Infinite); // for starategy integration
				/*
				private Series<bool> higherLowOscFound;
		private Series<bool> lowerLowPriceFound;
				*/
				higherLowOscFound 		= new Series<bool>(this, MaximumBarsLookBack.Infinite);
				lowerLowPriceFound 		= new Series<bool>(this, MaximumBarsLookBack.Infinite);
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 20)
			return;

			
			/*
			Bullish Div = HL on Osc + ll or equal low on price
			Bear Div 	= LH on Osc + HH or equal H on price
			
			flag = low LL on Price
			flag pojawebhgu[obSWWUPOEGB[Osubdgr[ouWERG'LKJs
			*/
			// int minSwingLowOdist = 3 int swingLowOdist;
			swingLowOdist = CurrentBar - swingLowObar;
			if ( swingLowOdist > 3  ) {
				
			}
			swingLowO = Swing1.SwingLow[ 3 ];
			swingHighO = Swing1.SwingHigh[ 3 ];
			swingLowP = Swing2.SwingLow[ 3 ];
			swingHighP = Swing2.SwingHigh[ 3 ];

			// bullish div
			higherLowOscFound[0] = higherLowOSC();
			lowerLowPriceFound[0] = lowerLowPrice();
			//bool lowerHighOSCFound = LowerHighOSC();
			
			//bool higherHighPriceFound = higherHighPrice();
			
			
//			int lastSwingLowObar; 
//		int swingLowObar;
//		int lastSwingLowPbar;
//		int swingLowPbar;
			
			
			/// bullish divergence  = HL on Osc + ll or equal low on price
			if ( ( higherLowOscFound[0] && lowerLowPriceFound[0] )
				|| (  higherLowOscFound[1] && lowerLowPriceFound[0]  )
				
				) {
				Draw.Dot(this, "bullDiv"+CurrentBar, true, 3, Low[3], Brushes.Green);
			}
			/// bearish divergence
//			if ( lowerHighOSCFound && higherHighPriceFound ) {
//				Draw.Dot(this, "bearDiv"+CurrentBar, true, 3, High[3], Brushes.Red);
//			}
		
			/// Set 1
			///if (UltimateOscillator1[0] == High[0]) {}
		}
		
//		/// Osc LH
//		public bool LowerHighOSC() {
//			bool result = false;
//			if(swingHighO == UltimateOscillator1[ 3 ]) {
//				if (swingHighO < lastSwingHighO ) {
//					if ( drawText )
//					Draw.Text(this, "hh"+ CurrentBar.ToString(),  "LH", 3, High[3] + (TickSize * 2));
//					result = true;
//				}
//				lastSwingHighO = swingHighO;
//			}
//			return result;
//		}
		
		// int minSwingLowOdist = 3 int swingLowOdist;
		/// Find Osc HL  /// bullish divergence  = HL on Osc + ll or equal low on price
		public bool higherLowOSC() {
			bool result = false;
			if(swingLowO == UltimateOscillator1[ 3 ]) {
				swingLowObar = CurrentBar - 3;
				if (swingLowO > lastSwingLowO ) {
					if ( drawText )
					Draw.Text(this, "HLo"+ CurrentBar.ToString(),  "HLo", 3, Low[3] - (TickSize * 2));
					result = true;
				}
				lastSwingLowO = swingLowO;
				lastSwingLowObar = swingLowObar;
			}
			return result;
		}
		
		/// Price LL or = L  /// bullish divergence  = HL on Osc + ll or equal low on price
		public bool lowerLowPrice() {
			bool result = false;
			if(swingLowP == Low[ 3 ]) {
				swingLowPbar = CurrentBar - 3;
				if (swingLowP <= lastSwingLowP ) {
					if ( drawText )
					Draw.Text(this, "LLp"+ CurrentBar.ToString(),  "LLp", 3, Low[3] - (TickSize * 4));
					result = true;
				}
				lastSwingLowP = swingLowP;
				lastSwingLowPbar = swingLowPbar;
			}
			return result;
		}
		
//		/// Price  HH or = H
//		public bool higherHighPrice() {
//			bool result = false;
//			if(swingLowP == Low[ 3 ]) {
				
//				if (swingLowP >= lastSwingLowP ) {
//					if ( drawText )
//					Draw.Text(this, "HLp"+ CurrentBar.ToString(),  "HLp", 3, Low[3] - (TickSize * 8));
//					result = true;
//				}
//				lastSwingLowP = swingLowP;
//			}
//			return result;
//		}
		
		
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private UltimateDivergence[] cacheUltimateDivergence;
		public UltimateDivergence UltimateDivergence()
		{
			return UltimateDivergence(Input);
		}

		public UltimateDivergence UltimateDivergence(ISeries<double> input)
		{
			if (cacheUltimateDivergence != null)
				for (int idx = 0; idx < cacheUltimateDivergence.Length; idx++)
					if (cacheUltimateDivergence[idx] != null &&  cacheUltimateDivergence[idx].EqualsInput(input))
						return cacheUltimateDivergence[idx];
			return CacheIndicator<UltimateDivergence>(new UltimateDivergence(), input, ref cacheUltimateDivergence);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.UltimateDivergence UltimateDivergence()
		{
			return indicator.UltimateDivergence(Input);
		}

		public Indicators.UltimateDivergence UltimateDivergence(ISeries<double> input )
		{
			return indicator.UltimateDivergence(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.UltimateDivergence UltimateDivergence()
		{
			return indicator.UltimateDivergence(Input);
		}

		public Indicators.UltimateDivergence UltimateDivergence(ISeries<double> input )
		{
			return indicator.UltimateDivergence(input);
		}
	}
}

#endregion
