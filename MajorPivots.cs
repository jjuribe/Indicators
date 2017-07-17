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
	public class MajorPivots : Indicator
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
				Name										= "MajorPivots";
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
				lastPivotLow = Low[0];
				lastPivotHigh = High[0];
				return;
			}
			
			/// Find Highs
			foundNewHigh = findNewHighs(strength: Strength, drawDot: true);

			/// Find Lows			
			foundNewLow = findNewLows(strength: Strength, drawDot: true);
			
			/// draw a line from last high to current low
			//plotZigAndFibDown( plotLine: true,  plotFib: false,  printLog: true);

			/// draw a line from last Low to current High
			//plotZigAndFibUp( plotLine: true,  plotFib: false, printSlim: true, printLog: true);
			
//			/// enter long
//			if (!longEntry && downtrend && High[0] > majorLowSwingRetraceLevel) {
//				Print(Time[0].ToString() + " LE " + majorLowSwingRetraceLevel);
//				Draw.Dot(this, "Long"+CurrentBar, true, 0, majorLowSwingRetraceLevel, Brushes.Green);
				
//				longEntry = true;
//				shortEntry = false;
				
//				//foundNewHigh = false;
//				lastPivotHigh = Close[0];
				
//			}
//			///------------------------------------------------------------------------------------------------
//			/// enter short
//			if (!shortEntry && uptrend && Low[0] < majorHighSwingRetraceLevel) {
//				Print(Time[0].ToString() + " SE " + majorHighSwingRetraceLevel);
//				Draw.Dot(this, "Short"+CurrentBar, true, 0, majorHighSwingRetraceLevel, Brushes.Red);
				
//				shortEntry = true;
//				longEntry = false;
				
//				//foundNewLow = true;
//				lastPivotLow =   Close[0];
				
//			}
		}
		///------------------------------------------------------------------------------------------------------------
		/// functions
		
		
		///	comment every line
		/// is 165 reversed?
		/// what doe
		
		
		
		/// find highs
		public bool findNewHighs(int strength, bool drawDot) {
			bool plotLine = false;
			bool result;
			
			/// determine swing size from last low
			double thisSwing = High[0] - lastPivotLow ;
	
			/// new high > last recorded high
			if ( High[0] > MAX(High, strength )[1] &&  High[0] > lastPivotHigh && thisSwing >  minSwingPoints  ) {
				
				//// record new high
				pivotHigh = High[0];
					
				/// mark new high
				if (drawDot) {
					if ( priorPivotHighBar > priorPivotLowBar ) {	// dont remove last high
						RemoveDrawObject("NewHigh"+ priorPivotHighBar);
					}
					
					Draw.Dot(this, "NewHigh"+CurrentBar, true, 0, pivotHigh, Brushes.DarkGray);
				}
				
				/// ditance to last pivot low
				int lastLowDistance = CurrentBar - pivotLowBar;
				/// draw zig up
				if ( plotLine && pivotLow != 0) {
					if ( priorPivotHighBar > priorPivotLowBar ) {	// dont remove last high
						RemoveDrawObject("zig2"+ priorPivotHighBar);
					}
					
					Draw.Line(this, "zig2"+CurrentBar, false, 
						lastLowDistance, pivotLow, 0, pivotHigh, Brushes.White, DashStyleHelper.Solid, 2);
				}
				
				/// record last pivot high
				lastPivotHigh = pivotHigh;
				/// save new high barnumber
				priorPivotHighBar = pivotHighBar;
				
				pivotHighBar = CurrentBar;
				result = true;
				
				// reset lastPivotLow so findNewLowsWillStart searching new lows
				lastPivotLow = Close[0];

			} else {
				result = false;	
			}
			return result;
		}
		
		/// find lows
		public bool findNewLows(int strength, bool drawDot) {
			
			bool plotLine = false;
			bool result;
			double thisSwing = lastPivotHigh - Low[0];
	
			if ( Low[0] < MIN(Low, strength )[1] &&  Low[0] < lastPivotLow && thisSwing >  minSwingPoints  ) {
				
				pivotLow = Low[0];
				
				/// mark new low
				if (drawDot) {
					if ( priorPivotLowBar > priorPivotHighBar ) {	// dont remove last low
						RemoveDrawObject("NewLow"+ priorPivotLowBar);
					}
					
					Draw.Dot(this, "NewLow"+CurrentBar, true, 0, pivotLow, Brushes.DarkGray);
				}
				
				// draw zig down
				int lastHighDistance = CurrentBar - pivotHighBar;
				
				if ( plotLine && pivotLow != 0) {
					if ( priorPivotLowBar >  priorPivotHighBar ) {	// dont remove last high
						RemoveDrawObject("zig1"+ priorPivotLowBar);
					}
					
					Draw.Line(this, "zig1"+CurrentBar, false, 
					lastHighDistance, pivotHigh, 0, pivotLow, Brushes.White, DashStyleHelper.Solid, 2);
				}
				
				/*
				RemoveDrawObject("zig1"+ priorPivotLowBar);
/				Draw.Line(this, "zig1"+CurrentBar, false, 
					lastHighDistance, MajorHighAnchorPrice, 0, lastPivotLow, Brushes.White, DashStyleHelper.Solid, 3);
				*/
				/// save new low barnumber
				lastPivotLow = pivotLow;
				priorPivotLowBar = pivotLowBar;
				pivotLowBar = CurrentBar;
				result = true;
				
				// reset lastPivotHigh so findNewHighs WillStart searching new highs
				lastPivotHigh = Close[0];
				
			} else {
				result = false;
			}
			return result;
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
		private MajorPivots[] cacheMajorPivots;
		public MajorPivots MajorPivots(int strength, int pivotLookBack)
		{
			return MajorPivots(Input, strength, pivotLookBack);
		}

		public MajorPivots MajorPivots(ISeries<double> input, int strength, int pivotLookBack)
		{
			if (cacheMajorPivots != null)
				for (int idx = 0; idx < cacheMajorPivots.Length; idx++)
					if (cacheMajorPivots[idx] != null && cacheMajorPivots[idx].Strength == strength && cacheMajorPivots[idx].PivotLookBack == pivotLookBack && cacheMajorPivots[idx].EqualsInput(input))
						return cacheMajorPivots[idx];
			return CacheIndicator<MajorPivots>(new MajorPivots(){ Strength = strength, PivotLookBack = pivotLookBack }, input, ref cacheMajorPivots);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MajorPivots MajorPivots(int strength, int pivotLookBack)
		{
			return indicator.MajorPivots(Input, strength, pivotLookBack);
		}

		public Indicators.MajorPivots MajorPivots(ISeries<double> input , int strength, int pivotLookBack)
		{
			return indicator.MajorPivots(input, strength, pivotLookBack);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MajorPivots MajorPivots(int strength, int pivotLookBack)
		{
			return indicator.MajorPivots(Input, strength, pivotLookBack);
		}

		public Indicators.MajorPivots MajorPivots(ISeries<double> input , int strength, int pivotLookBack)
		{
			return indicator.MajorPivots(input, strength, pivotLookBack);
		}
	}
}

#endregion
