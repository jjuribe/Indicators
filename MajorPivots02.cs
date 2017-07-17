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
/*
"My experience is what I agree to attend to. Only those items which I notice shape my mind.” –William James, The Principles of Psychology, Vol.1
I’m developing a solid NQ swing system, 
I 've had amazing loving bj's and more are right around the corner.
I have a desired mind and body

Todays work:
Mark the highs and lows
Draw zigzags
Draw Fib until price > 38%
Section the lines to mark entries
skinney up the fib plot

if new lows inside the range no new trades... reset the major low on short entry
make functions
Make into strategy 
Make hard stops
Make 2nd pic stops
Start trading omniscience’s NQ YM And ES

*/
//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class MajorPivots02 : Indicator
	{
		private FibonacciRetracements myFibRetUp;
		private FibonacciRetracements myFibRetDn;
		private int		desiredFibSpan = 100;
		private double minSwingPoints = 65;	// 35pts * 20.00 = $700 full swing
		
		private double 	pivotHigh = 0;
		private int 	pivotHighBar;
		private double 	lastPivotHigh;
		private int 	lastHighDistance;
		private bool 	foundNewHigh = false;
		private int 	priorPivotHighBar;
		
		private double  MajorHighAnchorPrice;
		private int  	MajorHighAnchorBar;
		
		private double 	pivotLow;
		private int		pivotLowBar;
		private bool	foundNewLow = false;
		private double	lastPivotLow = 90000;
		private int 	priorPivotLowBar;
		private double	majorLowSwingRetraceLevel;
		private double	majorHighSwingRetraceLevel;
		
		private bool 	pushingUp;
		private bool	pushingDn;
		private bool 	longEntry;
		private bool 	shortEntry;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MajorPivots02";
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
				//Strength					= 5;
				PivotLookBack					= 20;
			}
			else if (State == State.Configure)
			{
			}
			else if (State == State.DataLoaded)
			{				
				ClearOutputWindow(); 
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 10 ){ 
				lastPivotLow = 9000000.00;
				lastPivotHigh = 0;
				
				pivotLow = Close[0];
				pivotHigh = Close[0];
				return;
			}
			
			/// Find Highs
			findNewHighs(strength: Strength, drawDot: true);

			/// Find Lows			
			findNewLows(strength: Strength, drawDot: true);
			

		}
		///------------------------------------------------------------------------------------------------------------
		/// functions
		///	1. is this high > Last N highs?
		/// 2. is this high > any high since new last low?
		/// 3. Save price and barnum
		/// 4. remove last dot after last low
		/// 5. plot dot
		/// 
		///	1. is this low < Last N lows?
		/// 2. is this low < any low since new last high?
		/// 3. Save price and barnum
		/// 4. remove last dot after last high
		/// 5. plot dot
		/// Tuple<double, int> pivotLowTuple = new Tuple<double, int>(5860.00, 55);
		/// Tuple<double, int> pivotHighTuple = new Tuple<double, int>(5860.00, 55);
		
		/// find highs
		public void findNewHighs(int strength, bool drawDot) {	
			
			double swingDistance = High[0]  - pivotLow;
			//Draw.Text(this, "ref"+CurrentBar, swingDistance.ToString("0"), 0, High[0], Brushes.White);
			///	1. is this high > Last N highs? 
			if ( High[0] >= High[1] && swingDistance > 15 ) {
				//Draw.Text(this, "lvh"+CurrentBar, lastPivotHigh.ToString("0"), 0, High[0], Brushes.White);
				/// 2. is this high > any high since new last low?
				if ( High[0] > lastPivotHigh  ) {
					//Draw.Text(this, "ref"+CurrentBar, "3", 0, High[0], Brushes.White);
					///	save lastpivot
					lastPivotHigh = pivotHigh; priorPivotHighBar = pivotHighBar;
					/// 3. Save price and barnum and last barnum
					/// priorPivotHighBar = pivotHighBar;
					pivotHigh = High[0]; pivotHighBar = CurrentBar;
					/// 4. remove last dot after last low
					if (drawDot) {
						bool test = false;
						if ( priorPivotHighBar > pivotLowBar  ) {
							test = true;
						}
//						Print(priorPivotHighBar + " > " + priorPivotLowBar + " " + test.ToString());
//						if ( pivotHighBar > priorPivotLowBar ) {	// dont remove last high
//							RemoveDrawObject("NewHigh"+ priorPivotHighBar);
//						}
						/// 5. plot dot 	
						//Draw.Dot(this, "NewHigh"+CurrentBar, true, 0, pivotHigh, Brushes.Red);
						//Draw.Text(this, "NewHigh"+CurrentBar, pivotHigh.ToString("0"), 0, pivotHigh);
					}
					/// 2. is this low < any low since new last high?
					/// remark the low and barnum
					lastPivotLow = Low[0]; priorPivotLowBar = CurrentBar;
				}
			} else {
				//Draw.Text(this, "noHi"+CurrentBar, "+", 0, lastPivotHigh);
			}
			Draw.Text(this, "Hi"+CurrentBar, "-", 0, pivotHigh, Brushes.Red);
		}
				
		/*
		else if (State == State.DataLoaded)
			{				
				AcmeVwapValueChannel1				= AcmeVwapValueChannel();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBars[0] < 1)
			return;

			 // Set 1
			if (AcmeVwapValueChannel1[0] == High[0])
			{
			}
		*/
		/// find lows
		public void findNewLows(int strength, bool drawDot) {
			
			double swingDistance = pivotHigh - Low[0];
			
			///	1. is this low < Last N lows?
			if ( Low[0] <=Low[1] && swingDistance > 15 ){
				/// 2. is this low < any low since new last high?
				if (  Low[0] < lastPivotLow  ) {
					//Print("Low < LPL");
					/// save last pivot and barnum
					lastPivotLow = pivotLow; priorPivotLowBar = pivotLowBar;
					/// 3. Save price and barnum and last barnum
					///priorPivotLowBar = pivotLowBar;
					pivotLow = Low[0]; pivotLowBar = CurrentBar;
					/// 4. remove last dot after last high
					if (drawDot) {
						//if ( priorPivotLowBar > priorPivotHighBar ) {	// dont remove last low
						//	RemoveDrawObject("NewLow"+ priorPivotLowBar);
						//}
						/// 5. plot dot
						//Draw.Dot(this, "NewLow"+CurrentBar, true, 0, pivotLow, Brushes.Green);
					}
					/// 2. is this high > any high since new last low?
					/// remark the high and barnum 
					lastPivotHigh = High[0]; priorPivotHighBar = CurrentBar;
					//pivotHigh = High[0];
				}
			} else {
				//Draw.Text(this, "noLo"+CurrentBar, "-", 0, lastPivotLow);
			}
			Draw.Text(this, "Lo"+CurrentBar, "-", 0, pivotLow, Brushes.Green);
		}
		
		/// draw a line from last high to current low
//		public void plotZigAndFibDown(bool plotLine, bool plotFib, bool printLog){
//			if (foundNewLow ){
//				//Print(priorPivotLowBar + " " + CurrentBar);
//				int lastHighDistance = CurrentBar - MajorHighAnchorBar;
				
//				/// print zigzag
//				if ( plotLine ) {
//					RemoveDrawObject("zig1"+ priorPivotLowBar);
//					Draw.Line(this, "zig1"+CurrentBar, false, 
//					lastHighDistance, MajorHighAnchorPrice, 0, lastPivotLow, Brushes.White, DashStyleHelper.Solid, 3);
//				}
				
//				/// draw fib
//				if ( plotFib ) {
////					RemoveDrawObject("fib1"+ priorPivotLowBar);
////					FibonacciRetracements myFibRet = Draw.FibonacciRetracements(this, "fib1"+CurrentBar, true, lastHighDistance, MajorHighAnchorPrice, 0, lastPivotLow);
				
//					// skinney up the fib plot
//					int currentfibSpan = lastHighDistance;
					
//					if ( currentfibSpan < desiredFibSpan ) {
//						RemoveDrawObject("fib1"+ priorPivotLowBar);
//						myFibRetDn = Draw.FibonacciRetracements(this, "fib1"+CurrentBar, true, lastHighDistance, MajorHighAnchorPrice, 0, lastPivotLow);
//					} else {
//						int adjustment = (currentfibSpan -desiredFibSpan) /2 ;
						
//						RemoveDrawObject("fib1"+ priorPivotLowBar);
//						myFibRetDn = Draw.FibonacciRetracements(this, "fib1"+CurrentBar, true, lastHighDistance - adjustment, MajorHighAnchorPrice, adjustment, lastPivotLow);
//					}
					
//					/// Print each price level in the PriceLevels collection contain in myRetracements
//					if ( printLog ) {
//						Print(" ");
//						Print(Time[0].ToString() + " Long Entry Search");}
//						  foreach (PriceLevel p in myFibRetDn.PriceLevels)
//						  {
//							  double totalPriceRange = myFibRetDn.StartAnchor.Price - myFibRetDn.EndAnchor.Price;
//							  double price = p.GetPrice(myFibRetDn.EndAnchor.Price, totalPriceRange, false);
//							  if ( printLog ) 
//							  	Print(p.Name + " " + price.ToString());
//							  if ( p.Name  == "38.20 %" ) {
//								  Print("Found 38% " + price);
//								  majorLowSwingRetraceLevel = price;
//							  }
//						  }
					
//				}				
//			  //downtrend = true;
//			 // uptrend = false;
//				/// show long entry line
//				if( !longEntry  ) {
//					int counter =+ 1;
//					// RemoveDrawObject("entryLevel"+ lastBar);
//					Draw.Line(this, "entryLevel"+CurrentBar, false, 
//						counter, majorLowSwingRetraceLevel, 0, majorLowSwingRetraceLevel, Brushes.CornflowerBlue, DashStyleHelper.Solid, 2);
//				}
			
//			}
//		}
		
//		/// draw a line from last low to current high
//		/// TODO: printSlim
//		public void plotZigAndFibUp(bool plotLine, bool plotFib, bool printSlim, bool printLog){
//			if (foundNewHigh){
//				//Print(priorPivotHighBar + " " + CurrentBar);
//				int lastLowDistance = CurrentBar - pivotLowBar;
				
//				if ( plotLine ) {
//					RemoveDrawObject("zig2"+ priorPivotHighBar);
//					Draw.Line(this, "zig2"+CurrentBar, false, 
//						lastLowDistance, pivotLow, 0, pivotHigh, Brushes.White, DashStyleHelper.Solid, 3);
//				}
//				/// draw fib
//				if ( plotFib ) {
//					// skinney up the fib plot
//					int currentfibSpan = lastLowDistance;
					
//					if ( currentfibSpan < desiredFibSpan ) {
//						RemoveDrawObject("fib2"+ priorPivotHighBar);
//						myFibRetUp = Draw.FibonacciRetracements(this, "fib2"+CurrentBar, true, lastLowDistance, pivotLow, 0, pivotHigh);
//					} else {
//						int adjustment = (currentfibSpan -desiredFibSpan) /2 ;
						
//						RemoveDrawObject("fib2"+ priorPivotHighBar);
//						myFibRetUp = Draw.FibonacciRetracements(this, "fib2"+CurrentBar, true, lastLowDistance - adjustment, pivotLow, adjustment, pivotHigh);
//					}
					
//					/// Print each price level in the PriceLevels collection contain in myRetracements
//					if ( printLog ) {
//						Print(" ");
//						Print(Time[0].ToString() + " Shrt Entry Search");}
//					  foreach (PriceLevel p in myFibRetUp.PriceLevels)
//					  {
					   		
//						  double totalPriceRange = myFibRetUp.StartAnchor.Price - myFibRetUp.EndAnchor.Price;
//						  double price = p.GetPrice(myFibRetUp.EndAnchor.Price, totalPriceRange, false);
//						  if ( printLog ) 
//						  	Print(p.Name + " " + price.ToString());
//						  if ( p.Name  == "38.20 %" ) {
//							  Print("Found 38% " + price);
//							  majorHighSwingRetraceLevel = price;
//						  }
//					  }
//				}
//				//downtrend = false;
//				//uptrend = true;
				
//				/// show short entry line
//				if( !shortEntry  ) {
//					int counter =+ 1;
//					// RemoveDrawObject("entryLevel"+ lastBar);
//					Draw.Line(this, "entryLevel2"+CurrentBar, false, 
//						counter, majorHighSwingRetraceLevel, 0, majorHighSwingRetraceLevel, Brushes.CornflowerBlue, DashStyleHelper.Solid, 2);
//				}
//			}
//		}
		/// show long entry line
		
		/// enter long
		
		///	show short entry line
		
		///	enter short
		
		
		
		
//		public double profitLossCalc(bool longEntry) {
			
//			double tradeResult;
			
//			if (longEntry) {
//				tradeResult = ( exitPrice - entryPrice ) * 10000;
//			} else {
//				tradeResult = (entryPrice - exitPrice ) * 10000;
//			}

//			return tradeResult;
//		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Strength", Description="Pivot Strength", Order=1, GroupName="Parameters")]
		public int Strength
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="PivotLookBack", Order=2, GroupName="Parameters")]
		public int PivotLookBack
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MajorPivots02[] cacheMajorPivots02;
		public MajorPivots02 MajorPivots02(int strength, int pivotLookBack)
		{
			return MajorPivots02(Input, strength, pivotLookBack);
		}

		public MajorPivots02 MajorPivots02(ISeries<double> input, int strength, int pivotLookBack)
		{
			if (cacheMajorPivots02 != null)
				for (int idx = 0; idx < cacheMajorPivots02.Length; idx++)
					if (cacheMajorPivots02[idx] != null && cacheMajorPivots02[idx].Strength == strength && cacheMajorPivots02[idx].PivotLookBack == pivotLookBack && cacheMajorPivots02[idx].EqualsInput(input))
						return cacheMajorPivots02[idx];
			return CacheIndicator<MajorPivots02>(new MajorPivots02(){ Strength = strength, PivotLookBack = pivotLookBack }, input, ref cacheMajorPivots02);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MajorPivots02 MajorPivots02(int strength, int pivotLookBack)
		{
			return indicator.MajorPivots02(Input, strength, pivotLookBack);
		}

		public Indicators.MajorPivots02 MajorPivots02(ISeries<double> input , int strength, int pivotLookBack)
		{
			return indicator.MajorPivots02(input, strength, pivotLookBack);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MajorPivots02 MajorPivots02(int strength, int pivotLookBack)
		{
			return indicator.MajorPivots02(Input, strength, pivotLookBack);
		}

		public Indicators.MajorPivots02 MajorPivots02(ISeries<double> input , int strength, int pivotLookBack)
		{
			return indicator.MajorPivots02(input, strength, pivotLookBack);
		}
	}
}

#endregion
