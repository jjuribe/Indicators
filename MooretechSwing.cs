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
	public class MooretechSwing : Indicator
	{
		//private double minSwing = 150; 
		//private int SigDuration = 6;
		//private int StopPts = 300;
		//private bool UseRSI = true;
		//private bool UseBoliBand = true;
		//private bool ShowEntry = true;
		//private bool ShowEntryPrice = false;
		//private bool ShowEntryLine = true;
		
		private double ATRmulti = 10;
		private double myATR;
		private double minSwingATR;
		
		//private int mySpacer = 10;
		private bool RSIgoL;
		private bool RSIgoS;
		private bool BoliBandGo;
		private double HiAnchor; 
		private double LoAnchor;
		private double myRSI;
		private double AnchorShort = 0; 
		private double AnchorLong = 0;
		private double CurRetraceAmtS;
		private double CurRetracePriceS;
		private double CurRetraceAmtL;
		private double CurRetracePriceL;
		private int LoBarNum;
		private int HiBarNum;
		private int LastLoAge;
		private int LastHiAge;
		private bool InLong = false;
		private bool InShort = false;
		private bool GapEvent;
		private double EntryPriceS;
		private double EntryPriceL;
		private double ShortStop;
		private double LongStop;
		
		private int pivotStrength = 4;
		private double LastSwingHigh;
		private int newHighCounter;
		private double firstPivHigh;
		private double secondSwingHigh;
		
		private double LastSwingLow;
		private int newLowCounter;
		private double firstPivLow;
		private double secondSwingLow;
		
		private int myEntryBar;
		private int myEntryBarS;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MooretechSwing";
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
				MinSwing					= 150;
				MySpacer					= 10;
				SigDuration					= 6;
				StopPts					= 300;
				UseRSI					= true;
				UseBoliBand					= true;
				ShowEntryLine					= true;
				ShowEntry					= true;
				ShowEntryPrice					= true;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
		  if ( CurrentBar < 21 )	
				return;
		  // Initialize Hi's and Lo's
          if ( CurrentBar == 21 )	{
				CurRetraceAmtS = MAX(High, 20)[0];
				CurRetraceAmtL = MIN(Low, 20)[0];
				LoAnchor = MIN(Low, 20)[0];
				HiAnchor = MAX(High, 20)[0];
			}
		// Show Gap Down
			if( Open[0]  < Close[0] && Close[1] - Open[0] > 5 ) {
				GapEvent = true;
				Draw.Text(this, "gap"+CurrentBar, "Gap", 0, High[0] + ( TickSize*ATR(20)[0] ), ChartControl.Properties.ChartText);
			} else if( Open[0]  > Close[0] &&   Open[0] - Close[1] > 5 ) {
				GapEvent = true;
				Draw.Text(this, "gap"+CurrentBar, "Gap", 0, High[0] + ( TickSize*ATR(20)[0] ), ChartControl.Properties.ChartText);
			}
			else GapEvent = false;
				
			// RSI as filter
			myRSI = RSI(14, 3)[0];
			if( UseRSI && myRSI >= 70  )
				RSIgoL = true;
			else
				RSIgoL = false;
			if( UseRSI && myRSI <= 30  )
				RSIgoS = true;
			else
				RSIgoS = false;
			if( !UseRSI ) {
				RSIgoL = true;
				RSIgoS = true;
			}
			
		// ATR as Min Swing
			myATR = ATR(300)[0];
			minSwingATR = ATRmulti *myATR;
	
			// Change min swing from points to ticks
			MinSwing = MinSwing;// * TickSize;
			
			// Find Highs and Lows
				if ( High[0] > High[1] 
				&& High[0] - LoAnchor >= MinSwing
				//&& High[0] - LoAnchor >=	minSwingATR
				&& High[0] > MAX(High, 10 )[1]
				//&& AnchorLong == 0	// not in trade
				&& RSIgoL	)
					{
						HiAnchor = High[0];
						//DrawText("HiAnchor"+CurrentBar, "H", 0, High[0] + mySpacer * TickSize, Color.DarkGray);
						HiBarNum = CurrentBar;
					}
				
				if ( Low[0] < Low[1] 
					&& HiAnchor - Low[0] >= MinSwing
					//&& HiAnchor - Low[0] >= minSwingATR
					&& Low[0] < MIN(Low, 10 )[1]
					//&& AnchorShort == 0	// not in trade
					&& RSIgoS	)
					{
						LoAnchor = Low[0];
						//DrawText("LoAnchor"+CurrentBar, "L", 0, Low[0] - mySpacer * TickSize, Color.DarkGray);
						LoBarNum = CurrentBar;
					}
			// Determint last high age - might need to put this later
				LastLoAge = CurrentBar - LoBarNum;
				LastHiAge = CurrentBar - HiBarNum;
			// Show Promentnt swings worthly of marking with fib tool	
			//SigDuration = 5;
			if( LastHiAge == SigDuration )
				{
					//DrawText("LastHiAge"+CurrentBar, "High Anchor", 0, HiAnchor, Color.DarkGray);
					//DrawSquare("MySquare"+CurrentBar, true, SigDuration, HiAnchor, Color.DarkGray);
					Draw.Square(this, "MySquare"+CurrentBar, true, SigDuration, HiAnchor, ChartControl.Properties.ChartText);
					//RemoveDrawObject("LastHiAge"+(CurrentBar-1) );
					//DrawLine("LastHiAge"+CurrentBar, true, CurrentBar-LastHiAge, HiAnchor, 0, HiAnchor, 
					//	Color.DarkGray, DashStyle.Solid, 1 );
				}	
			if( LastLoAge == SigDuration )
				//DrawText("LastLoAge"+CurrentBar, "-", 0, LoAnchor, Color.DarkGray);
				//DrawSquare("LastLoAge"+CurrentBar, true, SigDuration, LoAnchor, Color.DarkGray);
				Draw.Square(this, "LastLoAge"+CurrentBar, true, SigDuration, LoAnchor, ChartControl.Properties.ChartText);
				
			// Find retracement level if seeking new high
				if( LastLoAge > LastHiAge 
					&& LastHiAge > SigDuration
					&&  !InShort
					)
					{
						CurRetraceAmtS = (  HiAnchor - LoAnchor )* 0.382;
						CurRetracePriceS = HiAnchor - CurRetraceAmtS;
						if( ShowEntryLine )
						//DrawText("HiRetrace"+CurrentBar, "-", 0, CurRetracePriceS, Color.Red);
						Draw.Text(this, "HiRetrace"+CurrentBar, "-", 0, CurRetracePriceS, ChartControl.Properties.ChartText);
						//CurRetracePriceDS.Set( CurRetraceAmt );
					}	
				if( LastLoAge < LastHiAge 
					&&  !InLong	
					)
					{
						CurRetraceAmtL = (  HiAnchor - LoAnchor )* 0.382;
						CurRetracePriceL = LoAnchor + CurRetraceAmtL;
						if( ShowEntryLine )
							Draw.Text(this, "LoRetrace"+CurrentBar, "-", 0, CurRetracePriceL, ChartControl.Properties.ChartText);
						//CurRetracePriceDS.Set( CurRetraceAmt );
					}
							
				//***********	TRADE ENTRIES	*********************************************************************
				//	Short Entry
				if ( Low[0] <= CurRetracePriceS &&  Low[1] > CurRetracePriceS
					 && !InShort  // && !GapEvent 
					&& High[0] >= CurRetracePriceS
					)
					{
						InShort = true;
						InLong = false;
						AnchorShort = HiAnchor;
						AnchorLong = 0;
						CurRetracePriceL = HiAnchor + ( MinSwing*20 ); // reset entry price
						EntryPriceS = CurRetracePriceS;
						if( ShowEntry ) {
							//DrawArrowDown("ShortEntry"+CurrentBar, 0, High[0]+ ( TickSize*ATR(20)[0] ), Color.Red);
							ArrowDown myArrowDn = Draw.ArrowDown(this, "Sell"+CurrentBar.ToString(), true, Time[0],  High[0]+ ( TickSize*ATR(20)[0] ), Brushes.Red);
							myArrowDn.OutlineBrush = Brushes.Red;
							//BackColor = Color.DarkRed;
							newLowCounter = 0;
							secondSwingLow = 0;
							myEntryBarS = CurrentBar;
							//Entry.Set( -1 );
						}
						
					}
				// Long Entry
				 if ( (  High[0] >= CurRetracePriceL && High[1] < CurRetracePriceL )
					//||( !InLong && High[0] > CurRetracePriceDS[1] )
					&& !InLong // && !GapEvent
					&& Low[0] <= CurRetracePriceL
					)
					{
						InShort = false;
						InLong = true;
						AnchorLong = LoAnchor;
						AnchorShort = 0;
						CurRetracePriceS = 0;
						EntryPriceL = CurRetracePriceL;
						if( ShowEntry )
						{
							//DrawArrowUp("InLongEntry"+CurrentBar, 0, Low[0]- ( TickSize*ATR(20)[0] ), Color.Lime);
							ArrowUp myArrowUp = Draw.ArrowUp(this, "InLongEntry"+CurrentBar.ToString(), true, Time[0],  Low[0]- ( TickSize*ATR(20)[0] ), Brushes.LimeGreen);
							myArrowUp.OutlineBrush = Brushes.LimeGreen;
							//BackColor = Color.DarkGreen;
							newHighCounter = 0;
							secondSwingHigh = 0;
							myEntryBar = CurrentBar;
							//Entry.Set( 1 );
						}
						
					}
				// Show Entry Price
					if( ShowEntryPrice )
					{
					if ( InShort && Close[0] <= EntryPriceS )
						//DrawText("InShortW"+CurrentBar, "$", 0, EntryPriceS, Color.Red);
						Draw.Text(this, "InShortW"+CurrentBar, "$", 0, EntryPriceS, Brushes.Red);
					if ( InShort && Close[0] > EntryPriceS )
						//DrawText("InShortL"+CurrentBar, "*", 0, EntryPriceS, Color.DarkGray);
						Draw.Text(this, "InShortL"+CurrentBar, "*", 0, EntryPriceS, Brushes.Red);
					
					if ( InLong && Close[0] >= EntryPriceL  )
						//DrawText("InLongW"+CurrentBar, "$", 0, EntryPriceL, Color.LimeGreen);
						Draw.Text(this, "InLongW"+CurrentBar, "$", 0, EntryPriceL, Brushes.LimeGreen);
					if ( InLong && Close[0] < EntryPriceL  )
						//DrawText("InLongW"+CurrentBar, "*", 0, EntryPriceL, Color.DarkGray);
						Draw.Text(this, "InLongW"+CurrentBar, "*", 0, EntryPriceL, Brushes.LimeGreen);
					}
				//***********	STOPS	*********************************************************************
				// Show hard Stops
					if ( InShort ) {
						ShortStop = EntryPriceS + StopPts;
						//DrawText("ShortStop"+CurrentBar, "=", 0, ShortStop, Color.DarkMagenta);
						if( secondSwingHigh != 0 )
							//DrawText("secondSwingHighStop"+CurrentBar, "2", 0, secondSwingHigh, Color.DarkMagenta);
							Draw.Text(this, "secondSwingHighStop"+CurrentBar, "2", 0, secondSwingHigh, Brushes.Red);
						
						if( CurrentBar == myEntryBarS )
							//DrawText("ShortStop"+CurrentBar, "Short Stop", 10, ShortStop, Color.DarkGray);
							Draw.Text(this, "ShortStop"+CurrentBar, "Short Stop", 10, ShortStop, Brushes.Red);
							RemoveDrawObject("ShortStopLine"+(CurrentBar-1) );
							//DrawLine("ShortStopLine"+CurrentBar, false, CurrentBar-myEntryBarS, ShortStop, 0, ShortStop, Color.DarkGray, DashStyle.Dash, 1 );
							Draw.Line(this, "ShortStopLine"+CurrentBar, false,  CurrentBar-myEntryBarS, ShortStop, 0, ShortStop,  Brushes.Red, DashStyleHelper.Dot, 2);
						}
					if ( InLong )	{
						LongStop = EntryPriceL - StopPts;
						if( CurrentBar == myEntryBar )
							//DrawText("LongStop"+CurrentBar, "Long Stop", 10, LongStop, Color.DarkGray);
							Draw.Text(this, "LongStop"+CurrentBar, "Long Stop", 10, LongStop, Brushes.LimeGreen);
						RemoveDrawObject("LongStopLine"+(CurrentBar-1) );
						//DrawLine("LongStopLine"+CurrentBar, false, CurrentBar-myEntryBar, LongStop, 0, LongStop, Color.DarkGray, DashStyle.Dash, 1 );
							Draw.Line(this, "LongStopLine"+CurrentBar, false,  CurrentBar-myEntryBar, LongStop, 0, LongStop,  Brushes.LimeGreen, DashStyleHelper.Dot, 2);
						}
				// Exit trade when stop hit
					if ( InLong && Low[0] <= LongStop )	{	 // stop out on static stop
						InLong = false;
						//DrawDot("LongStopHit"+CurrentBar, true, 0, LongStop, Color.White);
						Draw.Dot(this, "LongStopHit"+CurrentBar, true, 0, LongStop, Brushes.White);
						newLowCounter = 0;
						secondSwingLow = 0;
						//Stop.Set( 2 );
					}
					if ( InLong && Low[0] <= secondSwingLow && newLowCounter > 1 )	{ // stop out on 2nd swing high
						InLong = false;
						//DrawDot("LongStopHit"+CurrentBar, true, 0, secondSwingLow, Color.White);
						Draw.Dot(this, "LongStopHit"+CurrentBar, true, 0, secondSwingLow, Brushes.White);
						newLowCounter = 0;
						secondSwingLow = 0;
						//Stop.Set( 2 );
					}
					
					if ( InShort && High[0] >= ShortStop )	{ // stop out on static stop
						InShort = false;
						//DrawDot("ShortStopHit"+CurrentBar, true, 0, ShortStop, Color.White);
						Draw.Dot(this, "ShortStopHit"+CurrentBar, true, 0, ShortStop, Brushes.White);
						newHighCounter = 0;
						secondSwingHigh = 0;
						//Stop.Set( -2 );
					}
					if ( InShort && High[0] >= secondSwingHigh && newHighCounter > 1 )	{ // stop out on 2nd swing high
						InShort = false;
						//DrawDot("ShortStopHit"+CurrentBar, true, 0, secondSwingHigh, Color.White);
						Draw.Dot(this, "ShortStopHit"+CurrentBar, true, 0, secondSwingHigh, Brushes.White);
						newHighCounter = 0;
						secondSwingHigh = 0;
						//Stop.Set( -2 );
					}
					
				// Exit short trade when 2nd pivot is broken
					if( InShort && newHighCounter < 2 ) 
					{
						LastSwingHigh = Swing( pivotStrength ).SwingHigh[ 0 ];
						if( LastSwingHigh > EntryPriceS && newHighCounter == 0 )	// 1st pivot above entry
						if( LastSwingHigh ==  High[ pivotStrength ] )
							{
								//DrawTriangleDown("LastSwingHigh"+CurrentBar, true,  pivotStrength, High[ pivotStrength ], Color.DarkRed);
								Draw.TriangleDown(this, "LastSwingHigh"+CurrentBar, true,  pivotStrength, High[ pivotStrength ], Brushes.Red);
								newHighCounter = newHighCounter +1;
								//DrawText("newHighCounter"+CurrentBar, newHighCounter.ToString() , pivotStrength, High[ pivotStrength ]+ ( TickSize*ATR(20)[0] ), Color.White);
								firstPivHigh = LastSwingHigh;
							}
						if( LastSwingHigh ==  High[ pivotStrength ] )
						if( LastSwingHigh > firstPivHigh )	// 2nd pivot above entry
							{
								//DrawTriangleDown("LastSwingHigh"+CurrentBar, true,  pivotStrength, High[ pivotStrength ], Color.DarkRed);
								Draw.TriangleDown(this, "LastSwingHigh"+CurrentBar, true,  pivotStrength, High[ pivotStrength ], Brushes.Red);
								newHighCounter = newHighCounter +1;
								//DrawText("newHighCounter"+CurrentBar, newHighCounter.ToString() , pivotStrength, High[ pivotStrength ]+ ( TickSize*ATR(20)[0] ), Color.White);
								secondSwingHigh = LastSwingHigh;
							}
					}
				// Exit long trade when 2nd pivot is broken
					if( InLong && newLowCounter < 2 ) 
					{
						LastSwingLow = Swing( pivotStrength ).SwingLow[ 0 ];
						if( LastSwingLow < EntryPriceL && newLowCounter == 0 )	// 1st pivot above entry
						if( LastSwingLow ==  Low[ pivotStrength ] )
							{
								//DrawTriangleUp("LastSwingLow"+CurrentBar, true,  pivotStrength, Low[ pivotStrength ], Color.DarkRed);
								Draw.TriangleDown(this, "LastSwingLow"+CurrentBar, true,  pivotStrength, Low[ pivotStrength ], Brushes.Red);
								newLowCounter = newLowCounter +1;
								//DrawText("newLowCounter"+CurrentBar, newLowCounter.ToString() , pivotStrength, Low[ pivotStrength ]- ( TickSize*ATR(20)[0] ), Color.White);
								firstPivLow = LastSwingLow;
							}
						if( LastSwingLow ==  Low[ pivotStrength ] )
						if( LastSwingLow < firstPivLow )	// 2nd pivot above entry
							{
								//DrawTriangleUp("LastSwingLow"+CurrentBar, true,  pivotStrength, Low[ pivotStrength ], Color.DarkRed);
								Draw.TriangleDown(this, "LastSwingLow"+CurrentBar, true,  pivotStrength, Low[ pivotStrength ], Brushes.Red);
								newLowCounter = newLowCounter +1;
								//DrawText("newLowCounter"+CurrentBar, newLowCounter.ToString() , pivotStrength, Low[ pivotStrength ]- ( TickSize*ATR(20)[0] ), Color.White);
								secondSwingLow = LastSwingLow;
							}
					}
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MinSwing", Order=1, GroupName="Parameters")]
		public int MinSwing
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="MySpacer", Order=2, GroupName="Parameters")]
		public int MySpacer
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="SigDuration", Order=3, GroupName="Parameters")]
		public int SigDuration
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="StopPts", Order=4, GroupName="Parameters")]
		public int StopPts
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="UseRSI", Order=5, GroupName="Parameters")]
		public bool UseRSI
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="UseBoliBand", Order=6, GroupName="Parameters")]
		public bool UseBoliBand
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowEntryLine", Order=7, GroupName="Parameters")]
		public bool ShowEntryLine
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowEntry", Order=8, GroupName="Parameters")]
		public bool ShowEntry
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowEntryPrice", Order=9, GroupName="Parameters")]
		public bool ShowEntryPrice
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MooretechSwing[] cacheMooretechSwing;
		public MooretechSwing MooretechSwing(int minSwing, int mySpacer, int sigDuration, int stopPts, bool useRSI, bool useBoliBand, bool showEntryLine, bool showEntry, bool showEntryPrice)
		{
			return MooretechSwing(Input, minSwing, mySpacer, sigDuration, stopPts, useRSI, useBoliBand, showEntryLine, showEntry, showEntryPrice);
		}

		public MooretechSwing MooretechSwing(ISeries<double> input, int minSwing, int mySpacer, int sigDuration, int stopPts, bool useRSI, bool useBoliBand, bool showEntryLine, bool showEntry, bool showEntryPrice)
		{
			if (cacheMooretechSwing != null)
				for (int idx = 0; idx < cacheMooretechSwing.Length; idx++)
					if (cacheMooretechSwing[idx] != null && cacheMooretechSwing[idx].MinSwing == minSwing && cacheMooretechSwing[idx].MySpacer == mySpacer && cacheMooretechSwing[idx].SigDuration == sigDuration && cacheMooretechSwing[idx].StopPts == stopPts && cacheMooretechSwing[idx].UseRSI == useRSI && cacheMooretechSwing[idx].UseBoliBand == useBoliBand && cacheMooretechSwing[idx].ShowEntryLine == showEntryLine && cacheMooretechSwing[idx].ShowEntry == showEntry && cacheMooretechSwing[idx].ShowEntryPrice == showEntryPrice && cacheMooretechSwing[idx].EqualsInput(input))
						return cacheMooretechSwing[idx];
			return CacheIndicator<MooretechSwing>(new MooretechSwing(){ MinSwing = minSwing, MySpacer = mySpacer, SigDuration = sigDuration, StopPts = stopPts, UseRSI = useRSI, UseBoliBand = useBoliBand, ShowEntryLine = showEntryLine, ShowEntry = showEntry, ShowEntryPrice = showEntryPrice }, input, ref cacheMooretechSwing);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MooretechSwing MooretechSwing(int minSwing, int mySpacer, int sigDuration, int stopPts, bool useRSI, bool useBoliBand, bool showEntryLine, bool showEntry, bool showEntryPrice)
		{
			return indicator.MooretechSwing(Input, minSwing, mySpacer, sigDuration, stopPts, useRSI, useBoliBand, showEntryLine, showEntry, showEntryPrice);
		}

		public Indicators.MooretechSwing MooretechSwing(ISeries<double> input , int minSwing, int mySpacer, int sigDuration, int stopPts, bool useRSI, bool useBoliBand, bool showEntryLine, bool showEntry, bool showEntryPrice)
		{
			return indicator.MooretechSwing(input, minSwing, mySpacer, sigDuration, stopPts, useRSI, useBoliBand, showEntryLine, showEntry, showEntryPrice);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MooretechSwing MooretechSwing(int minSwing, int mySpacer, int sigDuration, int stopPts, bool useRSI, bool useBoliBand, bool showEntryLine, bool showEntry, bool showEntryPrice)
		{
			return indicator.MooretechSwing(Input, minSwing, mySpacer, sigDuration, stopPts, useRSI, useBoliBand, showEntryLine, showEntry, showEntryPrice);
		}

		public Indicators.MooretechSwing MooretechSwing(ISeries<double> input , int minSwing, int mySpacer, int sigDuration, int stopPts, bool useRSI, bool useBoliBand, bool showEntryLine, bool showEntry, bool showEntryPrice)
		{
			return indicator.MooretechSwing(input, minSwing, mySpacer, sigDuration, stopPts, useRSI, useBoliBand, showEntryLine, showEntry, showEntryPrice);
		}
	}
}

#endregion
