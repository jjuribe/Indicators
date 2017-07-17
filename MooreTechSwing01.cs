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
	public struct SwingData
	{
		public  double 	lastHigh 		{ get; set; }
		public	int 	lastHighBarnum	{ get; set; }
		public	double 	lastLow 		{ get; set; }
		public	int 	lastLowBarnum	{ get; set; }
		public  double 	prevHigh		{ get; set; }
		public	int 	prevHighBarnum	{ get; set; }
		public	double 	prevLow			{ get; set; }
		public	int 	prevLowBarnum	{ get; set; }
	}
	
	public struct EntryData
	{		
		public	double 	longEntryPrice 	{ get; set; }
		public	int 	longEntryBarnum	{ get; set; }
		public	double 	shortEntryPrice { get; set; }
		public	int 	shortEntryBarnum { get; set; }
		public	int 	shortLineLength { get; set; }
		public	int 	longLineLength 	{ get; set; }
		public 	bool	inLongTrade 	{ get; set; }
		public 	bool	inShortTrade 	{ get; set; }
		/// hard stops
		public	int 	longHardStopBarnum	{ get; set; }
		public	int 	shortHardStopBarnum	{ get; set; }
		public	double 	hardStopLine 	{ get; set; }
	
		/// pivot stops
		public	int 	longPivStopBarnum	{ get; set; }
		public	int 	shortPivStopBarnum	{ get; set; }
		public	int 	pivStopCounter	{ get; set; }
		public	double 	lastPivotValue 	{ get; set; }

	}
	
	public struct TradeData
	{
		public 	double	lastShort 		{ get; set; }
		public 	double	lastLong 		{ get; set; }
		public	double 	tradeNum 		{ get; set; }
		public	double 	numWins 		{ get; set; }
		public 	double	tradeProfit		{ get; set; }
		public 	double	totalProfit		{ get; set; }
		public 	double	winTotal		{ get; set; }
		public 	double	lossTotal		{ get; set; }
		public 	double	profitFactor	{ get; set; }
		public 	double	pctWin			{ get; set; }
		public 	double	largestLoss		{ get; set; }
		
	}
	
	public class MooreTechSwing01 : Indicator
	{
		private Swing Swing1;
		
		private SwingData swingData = new SwingData
		{
			lastHigh 		= 0.00,
			lastHighBarnum  = 0,
			lastLow  		= 0.00,
			lastLowBarnum  	= 0,
			prevHigh  		= 0.00,
			prevHighBarnum  = 0,
			prevLow  		= 0.00,
			prevLowBarnum 	= 0
		};
		
		private EntryData entry = new EntryData
		{
			/// <summary>
			///  TODO: initialize struct
			/// </summary>
		};
		
		private TradeData tradeData = new TradeData
		{
			/// <summary>
			///  TODO: initialize struct
			/// </summary>
		};
		
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "MooreTech Swing 01";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
			}
			else if (State == State.Configure)
			{
			}
			else if(State == State.DataLoaded)
			  {
			    ClearOutputWindow();     
				  Swing1				= Swing(5);
			  } 
		  
		}

		protected override void OnBarUpdate()
		{
			if ( CurrentBar < 20 ) {
				resetStruct(doIt: false);
				return;
			}
			
			//Print(Time[0].ToShortDateString().ToString() +  " LH " + swingData.lastHigh + " LL " + swingData.lastLow + " PH " + swingData.prevHigh + " PL " + swingData.prevLow);

			int 	upcount 		= edgeCount(up: true, plot: false );
			int 	dncount 		= edgeCount(up:false, plot: false );
			int MinSwing = 70;
			
			findNewHighs(upCount: upcount, minSwing: MinSwing );
			findNewLows(dnCount: dncount, minSwing: MinSwing );
			
			findShortEntry();
			findLongeEntry();
			
			drawLongEntryLine(inLongTrade: entry.inLongTrade);
			drawShortEntryLine(inShortTrade: entry.inShortTrade);
			
			setHardStop(pct: 3, shares: 100);
			/// woking on pivot stops
			//setPivotStop(swingSize: 5);
			
			recordTrades();
			///[ ] show exit when gap past next entry
		}
		
		///******************************************************************************************************************************
		/// 
		/// 										set Pivot Stop
		/// 
		/// ****************************************************************************************************************************
		/// 
		public void setPivotStop(int swingSize) {
			
			double lastSwingLow = Swing1.SwingLow[ swingSize ];
			double lastSwingHigh = Swing1.SwingHigh[ swingSize ];
			
			/// if  long /// close < entryswing 
			 if ( entry.inLongTrade && lastSwingLow < entry.lastPivotValue   ) {
				 entry.pivStopCounter++;
				 //Draw.Dot(this, "SwingLow"+ CurrentBar.ToString(), true, swingSize, Low[swingSize], Brushes.White);
				 Draw.Text(this, "lowswingtxt"+ CurrentBar.ToString(), entry.pivStopCounter.ToString(), swingSize, Low[swingSize] - (TickSize * 5));
				 entry.lastPivotValue = lastSwingLow;
				 entry.longPivStopBarnum = CurrentBar;
			 }
			 /// if  short /// close > entryswing 
			 if ( entry.inShortTrade && lastSwingHigh > entry.lastPivotValue   ) {
				 entry.pivStopCounter++;
				 //Draw.Dot(this, "SwingHigh"+ CurrentBar.ToString(), true, swingSize, High[swingSize], Brushes.Magenta);
				 Draw.Text(this, "HighSwingtxt"+ CurrentBar.ToString(), entry.pivStopCounter.ToString(), swingSize, High[swingSize] + (TickSize * 5));
				 entry.lastPivotValue = lastSwingHigh;
				 entry.shortPivStopBarnum = CurrentBar;
			 }
			/// draw pivot stop line after 2
//			  if ( entry.pivStopCounter == 1) {
//			 	entry.pivStopBarnum = CurrentBar;
//			  }
			 if ( entry.pivStopCounter == 2) {
				 drawPivStops();
				 //Draw.Text(this, "2ndPiv"+ CurrentBar.ToString(), "-", 0, entry.lastPivotValue);
				 /// exit at pivot line
				//exitFromPivotStop();
			 }
			
		}
		public void drawPivStops() {
			/// draw hard stop line 
			int lineLength = 0;
			string lineName = "";
			if ( entry.inLongTrade ) { 
				lineLength = CurrentBar - (entry.longPivStopBarnum +1); 
				lineName = "pivStopLong";
			}
			if ( entry.inShortTrade ) { 
				lineLength = CurrentBar - (entry.shortPivStopBarnum +1); 
				lineName = "pivStopShort";
			}
			RemoveDrawObject(lineName + (CurrentBar - 1));
			Draw.Line(this, lineName +CurrentBar.ToString(), false, lineLength, entry.lastPivotValue, 0, 
					entry.lastPivotValue, Brushes.DarkGray, DashStyleHelper.Dot, 2);
		}
		/// <summary>
		/// enter new trade after stop
		/// </summary>
		public void exitFromPivotStop() {
			if (CurrentBar > entry.longEntryBarnum && entry.pivStopCounter >= 2 && entry.inLongTrade && Low[0] <= entry.lastPivotValue ) {
				/// need short trades to debug this no long stops hit
				entry.inLongTrade = false;
				entry.longPivStopBarnum	= CurrentBar;
				Print("*********"+ Time[0].ToString() + "Exit Long with pivot Stop");
			} else if (CurrentBar > entry.shortEntryBarnum &&  entry.pivStopCounter >= 2 && entry.inShortTrade && High[0] >= entry.lastPivotValue ) {
				/// need short trades to debug this no long stops hit
				entry.inShortTrade = false;
				entry.shortPivStopBarnum = CurrentBar;
				Print("*********"+ Time[0].ToString() + "Exit Short with pivot Stop");
			}
		}
		
		public void setHardStop(double pct, int shares) {
		
			/// find long entry price /// calc trade cost
			if (CurrentBar == entry.longEntryBarnum ) {
				double tradeCost = entry.longEntryPrice * shares;
				/// calc 3%
				double risk = tradeCost * (pct * 0.01);
				/// convert 3% to points
				double riskAsPoints = risk / shares;
				entry.hardStopLine = entry.longEntryPrice - riskAsPoints;
			}
			
			/// find short entry price /// calc trade cost
			if (CurrentBar == entry.shortEntryBarnum ) {
				double tradeCost = entry.shortEntryPrice * shares;
				/// calc 3%
				double risk = tradeCost * (pct * 0.01);
				/// convert 3% to points
				double riskAsPoints = risk / shares;
				entry.hardStopLine = entry.shortEntryPrice + riskAsPoints;
			}
			/// draw hard stop line
			drawHardStops();
			/// exit at hard stop
			exitFromStop();
		}
		
		/// exit at hard stop
		public void exitFromStop() {
			if ( entry.inLongTrade && Low[0] <= entry.hardStopLine ) {
				/// need short trades to debug this no long stops hit
				entry.inLongTrade = false;
				entry.longHardStopBarnum	= CurrentBar;
				Print("*********"+ Time[0].ToString() + " Exit Long with hard Stop");
			} else if ( entry.inShortTrade && High[0] >= entry.hardStopLine ) {
				/// need short trades to debug this no long stops hit
				entry.inShortTrade = false;
				Print("*********"+ Time[0].ToString() + " Exit Short with hard Stop");
				entry.shortHardStopBarnum	= CurrentBar;
			}
		}
		
		public void drawHardStops() {
			/// draw hard stop line 
			int lineLength = 0;
			string lineName = "";
			if ( entry.inLongTrade ) { 
				lineLength = CurrentBar - entry.longEntryBarnum; 
				lineName = "hardStopLong";
			}
			if ( entry.inShortTrade ) { 
				lineLength = CurrentBar - entry.shortEntryBarnum; 
				lineName = "hardStopShort";
			}
			RemoveDrawObject(lineName + (CurrentBar - 1));
			Draw.Line(this, lineName +CurrentBar.ToString(), false, lineLength, entry.hardStopLine, 0, 
					entry.hardStopLine, Brushes.DarkGray, DashStyleHelper.Dot, 2);
		}
		
		public void recordTrades(){
		    /// calc short profit at long entry
			if (CurrentBar == entry.longEntryBarnum ) {
				tradeData.tradeNum++;
				if ( tradeData.tradeNum == 1) { 
					tradeData.lastLong = entry.longEntryPrice;
					return ;}
				tradeData.tradeProfit =  tradeData.lastShort - entry.longEntryPrice;
				Print( Time[0].ToString() + " short profit inside record trades " + tradeData.tradeProfit.ToString("0.00"));
			 	tradeData.lastLong = entry.longEntryPrice;
				tradeReport();
			}
			/// calc long profit at short entry
			if (CurrentBar == entry.shortEntryBarnum ) {
				tradeData.tradeNum++;
				if ( tradeData.tradeNum == 1) { 
					tradeData.lastShort = entry.shortEntryPrice;
					return; }
			  	tradeData.tradeProfit =  entry.shortEntryPrice - tradeData.lastLong;
				Print( Time[0].ToString() + " long profit inside record trades " + tradeData.tradeProfit.ToString("0.00"));
			 	tradeData.lastShort = entry.shortEntryPrice;
				tradeReport();
			 }
			
			/// calc loss from short hard stop hit
			if ( CurrentBar == entry.shortHardStopBarnum  ) {
				
				tradeData.tradeNum++;
				if ( tradeData.tradeNum == 1) { 
					tradeData.lastShort = entry.shortEntryPrice;
					return; }
			    tradeData.tradeProfit = entry.shortEntryPrice -  entry.hardStopLine;
				Print( Time[0].ToString() + " short hard stop hit inside record trades " + tradeData.tradeProfit.ToString("0.00"));
			 	tradeData.lastShort = entry.shortEntryPrice;
				tradeReport();
			}
			/// calc loss from long hard stop hit
			if ( CurrentBar == entry.longHardStopBarnum || CurrentBar == entry.longPivStopBarnum  ) {
				
				tradeData.tradeNum++;
				if ( tradeData.tradeNum == 1) { 
					tradeData.lastLong = entry.longEntryPrice;
					return; }
			  	tradeData.tradeProfit = entry.hardStopLine - entry.longEntryPrice;
				Print( Time[0].ToString() + " long stop hit inside record trades " + tradeData.tradeProfit.ToString("0.00"));
			 	tradeData.lastLong = entry.longEntryPrice;
				tradeReport();
			}
			
//			/// calc loss from short piv stop hit
//			if (  CurrentBar == entry.shortPivStopBarnum ) {
				
//				tradeData.tradeNum++;
//				if ( tradeData.tradeNum == 1) { 
//					tradeData.lastShort = entry.shortEntryPrice;
//					return; }
//			  	tradeData.tradeProfit = entry.shortEntryPrice -  entry.lastPivotValue;
//				Print("");
//				Print( Time[0].ToString() + " Short Piv Stop hit and inside record trades" + tradeData.tradeProfit.ToString("0.00"));
//				Print("ep "+entry.shortEntryPrice +" - lp "+ entry.lastPivotValue +    " = tp " +tradeData.tradeProfit);
//				Print("");
//			 	tradeData.lastShort = entry.shortEntryPrice;
//				tradeReport();
//			}
			

		 
		}
		
		/// report results
		public void tradeReport() {
	
			tradeData.totalProfit = tradeData.totalProfit + tradeData.tradeProfit;
			
			if (tradeData.tradeProfit >= 0 ) {
				tradeData.numWins++; 
				tradeData.winTotal = tradeData.winTotal + tradeData.tradeProfit;
			}

			if (tradeData.tradeProfit < 0 ) {
				tradeData.lossTotal = tradeData.lossTotal + tradeData.tradeProfit; 
				if ( tradeData.tradeProfit < tradeData.largestLoss) {
					tradeData.largestLoss = tradeData.tradeProfit;
				}
			}
			tradeData.pctWin = (tradeData.numWins / tradeData.tradeNum) * 100;
			tradeData.profitFactor = (tradeData.winTotal / tradeData.lossTotal) * -1;
			
			string report = "#" + tradeData.tradeNum.ToString() + "  $" + tradeData.tradeProfit.ToString("0.00");
			report = report + "\n" + tradeData.totalProfit.ToString("0.00") + " pts" + " " + tradeData.pctWin.ToString("0.0") + "%";
			report = report + "\n" + tradeData.profitFactor.ToString("0.00") + "Pf  LL " + tradeData.largestLoss.ToString("0.00");
			if (CurrentBar == entry.longEntryBarnum ) {
				Draw.Text(this, "LE"+CurrentBar, report, 0, entry.longEntryPrice - (TickSize * 40)); }
			if (CurrentBar == entry.shortEntryBarnum ) {
				Draw.Text(this, "SE"+CurrentBar, report, 0, entry.shortEntryPrice + (TickSize * 40)); }
			if (CurrentBar == entry.shortHardStopBarnum ) {
				Draw.Text(this, "SXh"+CurrentBar, report, 0, High[0] + (TickSize * 40)); }
			if (CurrentBar == entry.longHardStopBarnum ) {
				Draw.Text(this, "LXh"+CurrentBar, report, 0, Low[0] - (TickSize * 40)); }
//			if (CurrentBar == entry.shortPivStopBarnum ) {
//				Draw.Text(this, "SXp"+CurrentBar, report, 0, High[0] + (TickSize * 40)); }
//			if (CurrentBar == entry.longPivStopBarnum ) {
//				Draw.Text(this, "LXp"+CurrentBar, report, 0, Low[0] - (TickSize * 40)); }
		}
 
		/// <summary>
		/// Long Entry Logic
		/// </summary>
		public void drawLongEntryLine(bool inLongTrade){
			
			if ( inLongTrade ) { 
				return;
			}
			
			entry.longLineLength++ ;
			
			if ( entry.longEntryPrice != 0 ) {
				//Draw.Text(this, "LE"+CurrentBar, "*", 0, entry.longEntryPrice);
				RemoveDrawObject("LeLine"+ (CurrentBar - 1));
				Draw.Line(this, "LeLine"+CurrentBar.ToString(), false, entry.longLineLength, entry.longEntryPrice, 0, 
					entry.longEntryPrice, Brushes.LimeGreen, DashStyleHelper.Solid, 4);
				showLongEntryArrow(inLongTrade: entry.inLongTrade);
			}	
			
			
		}
		
		
		///******************************************************************************************************************************
		/// 
		/// 										mark long entry
		/// 
		/// ****************************************************************************************************************************
		public void showLongEntryArrow(bool inLongTrade){
			
			if ( inLongTrade ) { 
				return;
			}
				
			if ( High[0] > entry.longEntryPrice && Low[0] < entry.longEntryPrice ) {
				ArrowUp myArrowUp = Draw.ArrowUp(this, "LEmade"+ CurrentBar.ToString(), true, 0, entry.longEntryPrice - (TickSize * 5), Brushes.LimeGreen);
				myArrowUp.OutlineBrush = Brushes.LimeGreen;
				entry.inLongTrade = true;
				entry.inShortTrade = false;
				entry.longEntryBarnum = CurrentBar;
				/// reset pivot data
				entry.pivStopCounter = 0;
				entry.lastPivotValue = swingData.lastLow ;
			}
		}
		
		/// <summary>
		/// Short Entry Logic
		/// </summary>
		public void drawShortEntryLine(bool inShortTrade){
		
			if ( inShortTrade ) {
				return;
			}
			
			entry.shortLineLength++ ;
			
			if ( entry.shortEntryPrice != 0 ) {
				//Draw.Text(this, "SE"+CurrentBar, "*", 0, entry.shortEntryPrice);
				RemoveDrawObject("SeLine"+ (CurrentBar - 1));
				Draw.Line(this, "SeLine"+CurrentBar.ToString(), false, entry.shortLineLength, entry.shortEntryPrice, 0, 
					entry.shortEntryPrice, Brushes.Red, DashStyleHelper.Solid, 4);
				showShortEntryArrow(inShortTrade: entry.inShortTrade);
			}	
		}
		
		public void showShortEntryArrow(bool inShortTrade) {
			
			if ( inShortTrade ) {
				return;
			}
			
			if ( High[0] > entry.shortEntryPrice && Low[0] < entry.shortEntryPrice ) {
				ArrowDown myArrowDn = Draw.ArrowDown(this, "SEmade"+ CurrentBar.ToString(), true, 0, entry.shortEntryPrice + (TickSize * 5), Brushes.Red);
				myArrowDn.OutlineBrush = Brushes.Red;
				entry.inLongTrade = false;
				entry.inShortTrade = true;
				entry.shortEntryBarnum = CurrentBar;
				/// reset pivot data
				entry.pivStopCounter = 0;
				entry.lastPivotValue =  swingData.lastHigh;
			}
		}
		
		/// <summary>
		/// Show the number of confirmations of an edge
		/// </summary>
		/// <param name="up"></param>
		/// <returns></returns>
		public int edgeCount(bool up, bool plot){

			int upCount = 0;
			int dnCount = 0;
			int result = 0;
			
			/// rsi section
			if ( RSI(14, 1)[0] > 70 ) { upCount ++;}		
			if ( RSI(14, 1)[0] < 30 ) {	dnCount ++;} 

			/// bollinger section			
			if ( High[0] > Bollinger(2, 20).Upper[0]) {	upCount ++; }	
			if ( Low[0] < Bollinger(2, 20).Lower[0]) {	dnCount ++; }
				
			/// highest high section
			if (High[0] >= MAX(High, 20)[1] ) { upCount ++;}
			if (Low[0] <= MIN(Low, 20)[1] ) { dnCount ++; }
				
			/// show the numbers
			if (up) {
				result = upCount;
				if (upCount > 0 && plot ) {
					Draw.Text(this, "upCount"+CurrentBar, upCount.ToString(), 0, High[0] + (TickSize * 10));
				}
			} 
			if ( up == false ) {
				result = dnCount;
				if (dnCount > 0 && !up && plot ) {
					Draw.Text(this, "dnCount"+CurrentBar, dnCount.ToString(), 0, Low[0] - (TickSize * 10));
				}
			}
			
		    return result;
		}
		
		/// <summary>
		/// find new highs 
		/// </summary>
		/// <param name="upCount"></param>
		/// would take upcount as param
		/// would have to return an array [double lastHigh, int lastHighBarnum, double prevHigh, int prevHighBarnum]
		public void findNewHighs(int upCount, double minSwing){

			if ( upCount!= 0 && High[0] - swingData.lastLow > 1.5 ) {
				swingData.prevHigh = swingData.lastHigh;
				swingData.prevHighBarnum = swingData.lastHighBarnum;
				swingData.lastHigh = High[0];
				/// remove lower high at highs
				swingData.lastHighBarnum = CurrentBar;
				int distanceToLastHigh = swingData.lastHighBarnum - swingData.prevHighBarnum;
				if(High[0] < swingData.prevHigh && distanceToLastHigh < minSwing ) {
					swingData.lastHigh = swingData.prevHigh;
					swingData.lastHighBarnum = CurrentBar - distanceToLastHigh;
				}
			}			
		}
		
		/// find new lows
		/// </summary>
		/// <param name="upCount"></param>
		public void findNewLows(int dnCount, double minSwing){
			
			if ( dnCount!= 0 && swingData.lastHigh - Low[0] > 1.5 ) {
				swingData.prevLow = swingData.lastLow;
				swingData.prevLowBarnum = swingData.lastLowBarnum;
				swingData.lastLow = Low[0];
				swingData.lastLowBarnum = CurrentBar;
				/// remove higher low at lows
				int distanceToLastLow = swingData.lastLowBarnum - swingData.prevLowBarnum;
				if(Low[0] > swingData.prevLow && distanceToLastLow < minSwing ) {
					swingData.lastLow = swingData.prevLow;
					swingData.lastLowBarnum = swingData.prevLowBarnum;
				} 
			}
		}
		
		/// looking short
		/// </summary>
		/// <param name="upCount"></param>
		public void findShortEntry() {
			if ( swingData.lastHighBarnum > swingData.lastLowBarnum ) {
				//BarBrush = Brushes.Red;
				//CandleOutlineBrush = Brushes.Red;
				int distanceToLow = CurrentBar - swingData.lastLowBarnum;
				int distanceToHigh = CurrentBar - swingData.lastHighBarnum;
				int lastBar = CurrentBar -1;
				double upSwingDistance = swingData.lastHigh - swingData.lastLow;
				double upSwingEntry = upSwingDistance * 0.382;
				//double upSwingEntryPrice = swingData.lastHigh - upSwingEntry;
				entry.shortEntryPrice = swingData.lastHigh - upSwingEntry;
				
				/// draw upswing in red
				RemoveDrawObject("upline"+lastBar);
				Draw.Line(this, "upline"+CurrentBar.ToString(), false, distanceToLow, swingData.lastLow, distanceToHigh, swingData.lastHigh, Brushes.DarkRed, DashStyleHelper.Dash, 2);
			
				/// draw entry line
				RemoveDrawObject("shortEntry"+lastBar);
				Draw.Line(this, "shortEntry"+CurrentBar.ToString(), false, distanceToLow, entry.shortEntryPrice , distanceToHigh, entry.shortEntryPrice , Brushes.Red, DashStyleHelper.Dash, 2);
				/// show swing high height
				double swingProfit = (upSwingDistance * 0.236) * 100;
//RemoveDrawObject("upDist"+lastBar);
//Draw.Text(this, "upDist"+CurrentBar, upSwingDistance.ToString("0.00") + "\n$" + swingProfit.ToString("0"), distanceToHigh, swingData.lastHigh + (TickSize * 20));
			} else {
				// disable short entry
				entry.shortEntryPrice = 0;
				entry.shortLineLength = 0;
			}
		}
		
		/// looking long
		/// </summary>
		/// <param name="upCount"></param>
		public void findLongeEntry() {
			if ( swingData.lastHighBarnum < swingData.lastLowBarnum ) {
				//BarBrush = Brushes.Green;
				//CandleOutlineBrush = Brushes.Green;
				int distanceToHigh = CurrentBar - swingData.lastHighBarnum;
				int distanceToLow = CurrentBar - swingData.lastLowBarnum;
				int lastBar = CurrentBar -1;
				double dnSwingDistance = ( swingData.lastLow - swingData.lastHigh ) * -1;
				double dnSwingEntry = dnSwingDistance * 0.382;
				//double dnSwingEntryPrice = swingData.lastLow + dnSwingEntry;
				entry.longEntryPrice = swingData.lastLow + dnSwingEntry;
				
				/// draw down swing in green
				RemoveDrawObject("dnline"+lastBar);
				Draw.Line(this, "dnline"+CurrentBar.ToString(), false, distanceToHigh, swingData.lastHigh, distanceToLow, swingData.lastLow, Brushes.DarkGreen, DashStyleHelper.Dash, 2);
				
				/// draw entry line
				RemoveDrawObject("longEntry"+lastBar);
				Draw.Line(this, "longEntry"+CurrentBar.ToString(), false, distanceToHigh, entry.longEntryPrice, distanceToLow, entry.longEntryPrice, Brushes.LimeGreen, DashStyleHelper.Dash, 2);
				
				/// show swing low height
				double swingProfit = (dnSwingDistance * 0.236) * 100;
//				RemoveDrawObject("dnDist"+lastBar);
//				Draw.Text(this, "dnDist"+CurrentBar, dnSwingDistance.ToString("0.00") + "\n$" + swingProfit.ToString("0"), distanceToLow, swingData.lastLow - (TickSize * 20));
				
			}	else {
				// disable long entry
				entry.longEntryPrice = 0;
				entry.longLineLength = 0;
			}
		}
		
		/// init params
		public void resetStruct(bool doIt) {
				swingData.lastHigh 		= Low[0];
				swingData.lastHighBarnum  = 0;
				swingData.lastLow  		= Low[0];
				swingData.lastLowBarnum  	= 0;
				swingData.prevHigh  		= Low[0];
				swingData.prevHighBarnum  = 0;
				swingData.prevLow  		= Low[0];
				swingData.prevLowBarnum 	= 0;	
		}
	}
}




#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MooreTechSwing01[] cacheMooreTechSwing01;
		public MooreTechSwing01 MooreTechSwing01()
		{
			return MooreTechSwing01(Input);
		}

		public MooreTechSwing01 MooreTechSwing01(ISeries<double> input)
		{
			if (cacheMooreTechSwing01 != null)
				for (int idx = 0; idx < cacheMooreTechSwing01.Length; idx++)
					if (cacheMooreTechSwing01[idx] != null &&  cacheMooreTechSwing01[idx].EqualsInput(input))
						return cacheMooreTechSwing01[idx];
			return CacheIndicator<MooreTechSwing01>(new MooreTechSwing01(), input, ref cacheMooreTechSwing01);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MooreTechSwing01 MooreTechSwing01()
		{
			return indicator.MooreTechSwing01(Input);
		}

		public Indicators.MooreTechSwing01 MooreTechSwing01(ISeries<double> input )
		{
			return indicator.MooreTechSwing01(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MooreTechSwing01 MooreTechSwing01()
		{
			return indicator.MooreTechSwing01(Input);
		}

		public Indicators.MooreTechSwing01 MooreTechSwing01(ISeries<double> input )
		{
			return indicator.MooreTechSwing01(input);
		}
	}
}

#endregion
