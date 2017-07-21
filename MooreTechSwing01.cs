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
using System.IO;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Adaptation of Morretech Swing system intended for 15min bars and longer
	/// Developed by Warren Hansen at TradeStrat on 7/20/2017
	/// whansen1@mac.com
	/// </summary>
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
		/// actual entry
		public	double 	shortEntryActual { get; set; }
		public	double 	longEntryActual { get; set; }
		public	int 	barsSinceEntry 	{ get; set; }
		/// hard stops
		public	int 	longHardStopBarnum	{ get; set; }
		public	int 	shortHardStopBarnum	{ get; set; }
		public	double 	hardStopLine 	{ get; set; }
	
		/// pivot stops
		public	int 	longPivStopBarnum	{ get; set; }
		public	int 	shortPivStopBarnum	{ get; set; }
		public	int 	pivStopCounter	{ get; set; }
		public	double 	lastPivotValue 	{ get; set; }
		public	int 	pivLineLength	{ get; set; }  
	}
	
	public struct TradeData
	{
		public 	string	signalName 		{ get; set; }
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
		public 	double	cost			{ get; set; }
		public 	double	roi				{ get; set; }
		public 	double	openProfit		{ get; set; }
		public 	double	largestOpenDraw	{ get; set; }
		public 	string	report 			{ get; set; }
		public 	string	reportSimple	{ get; set; }
		public 	string	csvFile			{ get; set; }
		
	}
	
	public class MooreTechSwing01 : Indicator
	{
		private Swing 		Swing1;
		private RSI			Rsi1;
		private	Bollinger	Bollinger1;		
		/// colors
		private Brush 	upColor 	= Brushes.Green;
		private Brush 	downColor	= Brushes.Red;
		private Brush 	textColor	= Brushes.Red;
		
		///  signal 
		private Series<int> signals;
		
		
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
				Name										= "_MooreTech Swing 01";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive					= true;
				/// inputs 
				shares				= 100;			/// 	int		shares			= 100;
				swingPct			= 0.005;		///		double  swingPct		= 0.005;
				minBarsToLastSwing 	= 70;			/// 	int MinBarsToLastSwing 	= 70;
				enableHardStop 		= true;			/// 	bool setHardStop = true, int pctHardStop 3, 
				pctHardStop  		= 3;
				enablePivotStop 	= true;			/// 	bool setPivotStop = true, int pivotStopSwingSize = 5, 
				pivotStopSwingSize 	= 5;
				pivotStopPivotRange = 0.2;			///		double pivotStopPivotSlop = 0.2
				/// swow plots
				showUpCount 			= false;		/// 	bool ShowUpCount 			= false;
				showHardStops 			= false;		/// 	bool show hard stops 		= false;
				printTradesOnChart		= false;		/// 	bool printtradesOn Chart	= false
				printTradesSimple 		= false;		/// 	bool printTradesSimple 		= false
				printTradesTolog 		= true;			/// 	bool printTradesTolog 		= true;
			}
			else if (State == State.Configure)
			{
                signals = new Series<int>(this, MaximumBarsLookBack.Infinite); // for starategy integration
            }
			else if(State == State.DataLoaded)
			  {
			    ClearOutputWindow();     
				  Swing1				= Swing(5);	// for piv stops
				  Rsi1					= RSI(14, 1);
				  Bollinger1			= Bollinger(2, 20);	
			  } 
		}
		///******************************************************************************************************************************
		/// 
		/// 										on bar update
		/// 
		/// ****************************************************************************************************************************
		protected override void OnBarUpdate()
		{
			if ( CurrentBar < 20 ) { resetStruct(doIt: false); return; }
			
			resetBarsSinceEntry();
			int 	upcount 		= edgeCount(up: true, plot: showUpCount );
			int 	dncount 		= edgeCount(up:false, plot: showUpCount );
			
			findNewHighs(upCount: upcount, minSwing: minBarsToLastSwing );
			findNewLows(dnCount: dncount, minSwing: minBarsToLastSwing );
			
			findShortEntry();
			findLongeEntry();
			
			drawLongEntryLine(inLongTrade: entry.inLongTrade);
			drawShortEntryLine(inShortTrade: entry.inShortTrade);
			
			if( enableHardStop ) { setHardStop(pct: pctHardStop, shares: shares, plot: showHardStops);}
			if ( enablePivotStop ) { setPivotStop(swingSize: pivotStopSwingSize, pivotSlop: pivotStopPivotRange); }
			
			///	these functions under developement and disabled
			//  recordTrades(printChart: printTradesOnChart, printLog: printTradesTolog, hiLow: true, simple: printTradesSimple);
			//	gapPastEntry();
			//	OpenProfitLockedIn();
			
			
			///  Good Spy 22.41 pts 27.7 ROI, Gush, QQQ, Not good Uso, Eur, Fx
			/// 
			///  enable data disconnect, order rejected share
			/// 
			///  deploy to VPS + start auto trading
			///  confirm message settings on platform
			///  make FOMC look for neg afffects
			///  use dx graphica
			///  not finding any gap entries use playback to confirm
			///  clac position size from 1. account size, 2. number of strategies
			///  plot on 1500 tick chart
			/// 
			}
		
		
		
		public void resetBarsSinceEntry() {
			if ( entry.inShortTrade == true) { entry.barsSinceEntry = CurrentBar - entry.shortEntryBarnum; }
			else if ( entry.inLongTrade == true) { entry.barsSinceEntry = CurrentBar - entry.longEntryBarnum; }
			else { entry.barsSinceEntry = 0;}
		}
		
		public void OpenProfitLockedIn(){
			/// get current bar
			if (Count - 2 == CurrentBar)
			  {
				  string upDate = "Long Open Profit of ";
				  if( entry.inLongTrade ) {
					  tradeData.openProfit = Close[0] -  entry.longEntryActual;
					}
				  if( entry.inShortTrade ) {
					  tradeData.openProfit = entry.shortEntryActual  - Close[0];
					  upDate = "Short Open Profit of " ;
				  }
				  double openGain = tradeData.openProfit * shares;
			  }
		}

		///******************************************************************************************************************************
		/// 
		/// 										gap paast entry
		/// 
		/// ****************************************************************************************************************************
		public void gapPastEntry() {
			bool gapDown, gapUp; 
			double gapHigh, gapLow;
			
			if ( Low[1] - Open[0] > 0.10 ) {
				gapDown = true;
				gapHigh = Low[1];
				gapLow = Open[0];
			} else { 
				gapDown = false; 
				gapLow = 0;
				gapHigh = 0;
			}
			if (gapDown) {
				//Print(Time[0].ToString() + "  gapDown");	
				//Draw.Dot(this, "gapDown"+CurrentBar, true, 0, Open[0], Brushes.DarkRed);
			}
			
			if ( Open[0] - High[1]  > 0.10 ) {
				gapUp = true;
				gapHigh = Open[1];
				gapLow = High[1];
			} else { 
				gapUp = false; 
				gapLow = 0;
				gapHigh = 0;
			}
			if (gapUp) {
				//Print(Time[0].ToString() + "  gapUp");	
				//Draw.Dot(this, "gapUp"+CurrentBar, true, 0, Open[0], Brushes.DarkGreen);
			}
			
			if ( gapDown || gapUp ){
				/// look for long in gap 
				if( !entry.inLongTrade && gapHigh > entry.longEntryPrice && gapLow < entry.longEntryPrice ) {
					Print(Time[0].ToString() + "----> Long Inside Gap");	
					Draw.Dot(this, "lGap"+CurrentBar, true, 0, entry.longEntryPrice, Brushes.LimeGreen);
				}
				/// look for short in gap 
				if( !entry.inShortTrade && gapHigh > entry.shortEntryPrice && gapLow < entry.shortEntryPrice ) {
					Print(Time[0].ToString() + "----> Short Inside Gap");	
					Draw.Dot(this, "sGap"+CurrentBar, true, 0, entry.shortEntryPrice, Brushes.LimeGreen);
				}
			}
		}
		
		///******************************************************************************************************************************
		/// 
		/// 										set Pivot Stop
		/// 
		/// ****************************************************************************************************************************
		public void setPivotStop(int swingSize, double pivotSlop) {
			
			double lastSwingLow = Swing1.SwingLow[ swingSize ];
			double lastSwingHigh = Swing1.SwingHigh[ swingSize ];
			
			/// long pivots, count pivots above entry for 2nd piv stop if  short /// close > entryswing 
			 if ( entry.inLongTrade && (( lastSwingLow + pivotSlop ) <  entry.lastPivotValue ) && entry.barsSinceEntry > 8 ) {
				 entry.pivStopCounter++;
				 Draw.Text(this, "LowSwingtxt"+ CurrentBar.ToString(),  entry.pivStopCounter.ToString(), swingSize, Low[swingSize] - (TickSize * 10));
				 entry.lastPivotValue = lastSwingLow;
			 }
			 /// short pivots, count pivots above entry for 2nd piv stop if  short /// close > entryswing 
			 if ( entry.inShortTrade && ( lastSwingHigh - pivotSlop )  > entry.lastPivotValue && entry.barsSinceEntry > 8 ) {
				 entry.pivStopCounter++;
				 Draw.Text(this, "HighSwingtxt"+ CurrentBar.ToString(), entry.pivStopCounter.ToString(), swingSize, High[swingSize] + (TickSize * 10));
				 entry.lastPivotValue = lastSwingHigh;
			 }
			 /// draw the 2nd piv stop line //drawPivStops();
			 if(entry.inLongTrade || entry.inShortTrade )
			 if ( entry.pivStopCounter == 2) {
				int lineLength = 0; 
				entry.pivLineLength++; 
				RemoveDrawObject("pivStop" + (CurrentBar - 1));
				Draw.Line(this, "pivStop"  +CurrentBar.ToString(), false, entry.pivLineLength, entry.lastPivotValue, 0, 
						entry.lastPivotValue, Brushes.Magenta, DashStyleHelper.Dot, 2);
			 } 
			/// exit at pivot line
			exitFromPivotStop(pivotSlop: pivotSlop);
		}

		/// exit trade after pivot stop
		public void exitFromPivotStop(double pivotSlop) {
			
			if (CurrentBar > entry.longEntryBarnum &&  entry.pivStopCounter >= 2 && entry.inLongTrade && Low[0] <= entry.lastPivotValue ) {
				Draw.Dot(this, "testDot"+CurrentBar, true, 0, entry.lastPivotValue, Brushes.Magenta);
				entry.inLongTrade = false;
                signals[0]  = 2;
				entry.longPivStopBarnum = CurrentBar;
				tradeData.signalName = "LX - PS";
				entry.pivLineLength = 0;
				entry.pivStopCounter = 0;
				entry.barsSinceEntry = 0;
			}
			
			if (CurrentBar > entry.shortEntryBarnum &&  entry.pivStopCounter >= 2 && entry.inShortTrade && High[0] >= entry.lastPivotValue ) {
				Draw.Dot(this, "testDot"+CurrentBar, true, 0, entry.lastPivotValue, Brushes.Magenta);
				entry.inShortTrade = false;
                signals[0] = -2;	
				entry.shortPivStopBarnum = CurrentBar;
				tradeData.signalName = "SX - PS";
				entry.pivLineLength = 0;
				entry.pivStopCounter = 0;
				entry.barsSinceEntry = 0;
			}
		}
		///******************************************************************************************************************************
		/// 
		/// 										set Hard Stop
		/// 
		/// ****************************************************************************************************************************
		public void setHardStop(double pct, int shares, bool plot) {
			/// find long entry price /// calc trade cost
			if (CurrentBar == entry.longEntryBarnum ) {
				double pctPrice = pct * 0.01;
				entry.hardStopLine = Close[0]  - ( Close[0] * pctPrice);
				}
			/// find short entry price /// calc trade cost
			if (CurrentBar == entry.shortEntryBarnum ) {
				double pctPrice = pct * 0.01;
				entry.hardStopLine = Close[0]  + ( Close[0] * pctPrice);
			}
			/// draw hard stop line
			drawHardStops(plot: plot);
			/// exit at hard stop
			exitFromStop();
		}
		
		/// exit at hard stop
		public void exitFromStop() {
			if ( entry.inLongTrade && Low[0] <= entry.hardStopLine ) {
				/// need short trades to debug this no long stops hit
				entry.inLongTrade = false;
                signals[0] = 2;
				entry.longHardStopBarnum	= CurrentBar;
				tradeData.signalName = "LX - HS";
				entry.barsSinceEntry = 0;
			} else if ( entry.inShortTrade && High[0] >= entry.hardStopLine ) {
				/// need short trades to debug this no long stops hit
				entry.inShortTrade = false;
                signals[0] = -2;
				entry.shortHardStopBarnum	= CurrentBar;
				tradeData.signalName = "SX - HS";
				entry.barsSinceEntry = 0;
			}
		}
		
		public void drawHardStops(bool plot) {
			if( !plot ) {
				return;
			}
			/// draw hard stop line 
			int lineLength = 0;
			string lineName = "";
			if ( entry.inLongTrade ) { 
				lineLength = entry.barsSinceEntry; 
				lineName = "hardStopLong";
			}
			if ( entry.inShortTrade ) { 
				lineLength = CurrentBar - entry.shortEntryBarnum; 
				lineName = "hardStopShort";
			}
			if(entry.barsSinceEntry > 1)
				RemoveDrawObject(lineName + (CurrentBar - 1));
			Draw.Line(this, lineName +CurrentBar.ToString(), false, lineLength, entry.hardStopLine, 0, 
					entry.hardStopLine, Brushes.DarkGray, DashStyleHelper.Dot, 2);
		}
		///******************************************************************************************************************************
		/// 
		/// 										RECORD TRADES
		/// 
		/// ****************************************************************************************************************************
		public void recordTrades(bool printChart, bool printLog, bool hiLow, bool simple){
			
		    /// calc short profit at long entry
			if (CurrentBar == entry.longEntryBarnum ) {
				tradeData.tradeNum++;
				if(checkForFirstEntry()) { 
					tradeData.tradeProfit = 0;
					tradeData.lastLong = entry.longEntryPrice;
					calcAndShowOnChart(printChart: printChart, printLog: printLog, hiLow: hiLow, simple: simple);
					return; }
				tradeData.tradeProfit =  entry.shortEntryActual - entry.longEntryActual;
			 	tradeData.lastLong = entry.longEntryPrice;
				calcAndShowOnChart(printChart: printChart, printLog: printLog, hiLow: hiLow, simple: simple);
			} else
			/// calc long profit at short entry
			if (CurrentBar == entry.shortEntryBarnum ) {
				tradeData.tradeNum++;
				if(checkForFirstEntry()) { 
					tradeData.tradeProfit =  0;
			 		tradeData.lastShort = entry.shortEntryPrice;
					calcAndShowOnChart(printChart: printChart, printLog: printLog, hiLow: hiLow, simple: simple);
					return; }
			  	tradeData.tradeProfit =  entry.shortEntryActual - entry.longEntryActual; //entry.shortEntryPrice - tradeData.lastLong;
			 	tradeData.lastShort = entry.shortEntryPrice;
				calcAndShowOnChart(printChart: printChart, printLog: printLog, hiLow: hiLow, simple: simple);
			 } else
			/// calc loss from short hard stop hit
			if ( CurrentBar == entry.shortHardStopBarnum  ) {
				tradeData.tradeNum++;
			    tradeData.tradeProfit = entry.shortEntryPrice -  entry.hardStopLine;
			 	tradeData.lastShort = entry.shortEntryPrice;
				calcAndShowOnChart(printChart: printChart, printLog: printLog, hiLow: hiLow, simple: simple);
			} else
			/// calc loss from long hard stop hit
			if ( CurrentBar == entry.longHardStopBarnum || CurrentBar == entry.longPivStopBarnum  ) {	
				tradeData.tradeNum++;
			  	tradeData.tradeProfit = entry.hardStopLine - entry.longEntryPrice;
			 	tradeData.lastLong = entry.longEntryPrice;
				calcAndShowOnChart(printChart: printChart, printLog: printLog, hiLow: hiLow, simple: simple);
			} else
			/// calc loss from short piv stop hit
			if (  CurrentBar == entry.shortPivStopBarnum ) {
				Draw.Dot(this, "sps"+CurrentBar, true, 0, Close[0], Brushes.Magenta);
				tradeData.tradeNum++;
			  	tradeData.tradeProfit = entry.shortEntryActual -  Close[0];
			 	tradeData.lastShort = entry.shortEntryPrice;
				calcAndShowOnChart(printChart: printChart, printLog: printLog, hiLow: hiLow, simple: simple);
			} 
			/// calc loss from long piv stop hit
			if (  CurrentBar == entry.longPivStopBarnum ) {
				Draw.Dot(this, "lps"+CurrentBar, true, 0, Close[0], Brushes.Magenta);
				tradeData.tradeNum++;
			  	tradeData.tradeProfit = entry.longEntryActual -  Close[0];
			 	tradeData.lastLong = entry.longEntryPrice;
				calcAndShowOnChart(printChart: printChart, printLog: printLog, hiLow: hiLow, simple: simple);
			} 
		}
		
		public bool checkForFirstEntry() {
			if ( CurrentBar == entry.longEntryBarnum && tradeData.tradeNum == 1) { 
				tradeData.lastLong = entry.longEntryPrice;
				return true;
			} else if (CurrentBar == entry.shortEntryBarnum && tradeData.tradeNum == 1) {	  
				tradeData.lastShort = entry.shortEntryPrice;
				return true;
			} else { return false;}	
		}
		
		/// report results
		public void calcAndShowOnChart(bool printChart, bool printLog, bool hiLow, bool simple) {
			calcTradeStats();
			concatStats();
			if( printChart ) { customDrawTrades(show: hiLow, simple: simple);}
			
			if ( printLog )
				Print("\n"+Time[0].ToString() +" "+ tradeData.report );
		}
		
		public void calcTradeStats() {
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
			tradeData.cost = (SMA(100)[0] * (double)shares ) / 3;
			tradeData.roi = ( ( tradeData.totalProfit * (double)shares ) / tradeData.cost ) * 100;
		}
		
		///  full report and simple
		public void concatStats(){
				string allStats = "#" + tradeData.tradeNum.ToString() + " " + tradeData.signalName + "  $" + tradeData.tradeProfit.ToString("0.00");
				//allStats = allStats + "\n" + tradeData.totalProfit.ToString("0.00") + " pts" + " " + tradeData.pctWin.ToString("0.0") + "%";
				tradeData.reportSimple = allStats;
			
				allStats = "#" + tradeData.tradeNum.ToString() + " " + tradeData.signalName + "  $" + tradeData.tradeProfit.ToString("0.00");
				allStats = allStats + "\n" + tradeData.totalProfit.ToString("0.00") + " pts" + " " + tradeData.pctWin.ToString("0.0") + "%";
				allStats = allStats + "\n" + tradeData.profitFactor.ToString("0.00") + "Pf  LL " + tradeData.largestLoss.ToString("0.00");
				allStats = allStats + "\n$" + tradeData.cost.ToString("0") + " Cost " + tradeData.roi.ToString("0.0") + "% Roi"; 
				writeToCsv();
				//tradeData.signalName = "*";
				tradeData.report = allStats;
		}
		
		public void writeToCsv() {
			string thisDate = "_"+DateTime.Now.ToString("yyyy-M-dd");
			
			var	filePath = @"C:\Users\MBPtrader\Documents\NT_CSV\" + Instrument.MasterInstrument.Name + thisDate+".csv" ;
			//before your loop
		    var csv = new StringBuilder();

			string titleRow = "Trade"+","+"Date"+","+"Signal"+","+"Profit"+","+"Sum Profit"+","+"Pct Win"+","+"PF"+","+"LL"+","+"Cost"+","+"ROI";
			string allStats = tradeData.tradeNum.ToString() + "," +Time[0].ToString("M-dd-yyyy")+","+ tradeData.signalName + "," + tradeData.tradeProfit.ToString("0.00");
			allStats = allStats + "," + tradeData.totalProfit.ToString("0.00") + "," +  tradeData.pctWin.ToString("0.0");
			allStats = allStats + "," + tradeData.profitFactor.ToString("0.00") + "," + tradeData.largestLoss.ToString("0.00");
			allStats = allStats + "," + tradeData.cost.ToString("0") + "," + tradeData.roi.ToString("0.0"); 

			// create the file 
			if ( tradeData.tradeNum == 1 ) {
		    	csv.AppendLine(titleRow);
				csv.AppendLine(allStats); 
				File.WriteAllText(filePath, csv.ToString());
			} else if (allStats != tradeData.csvFile ) {
		    	csv.AppendLine(allStats);  
				File.AppendAllText(filePath, csv.ToString());
			}
			tradeData.csvFile = allStats;
		}
		
		public void customDrawTrades(bool show, bool simple) {
			// set color
			if( tradeData.tradeProfit >= 0 ) { textColor = upColor;
			} else { textColor = downColor;}
			// set text
			string reportData = tradeData.report;
			if(simple) { reportData = tradeData.reportSimple;}
			if(show) {
			if (CurrentBar == entry.longEntryBarnum ) {
				Draw.Text(this, "LE"+CurrentBar, reportData, 0, MIN(Low, 20)[0], textColor); }
			if (CurrentBar == entry.shortEntryBarnum ) {
				Draw.Text(this, "SE"+CurrentBar, reportData, 0, MAX(High, 20)[0], textColor); }
			if (CurrentBar == entry.shortHardStopBarnum ) {
				Draw.Text(this, "SXh"+CurrentBar, reportData, 0, MAX(High, 20)[0], textColor); }
			if (CurrentBar == entry.longHardStopBarnum ) {
				Draw.Text(this, "LXh"+CurrentBar, reportData, 0, MIN(Low, 20)[0], textColor); }
			if (CurrentBar == entry.shortPivStopBarnum ) {
				Draw.Text(this, "SXp"+CurrentBar, reportData, 0, MAX(High, 20)[0], textColor); }
			if (CurrentBar == entry.longPivStopBarnum ) {
				Draw.Text(this, "LXp"+CurrentBar, reportData, 0, MIN(Low, 20)[0], textColor); }
			} else {
				Draw.Text(this, "report"+CurrentBar, reportData, 0, MIN(Low, 20)[0]);
			}
		}
 
		/// Long Entry Logic
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
		/// 										long entry
		/// 
		/// ****************************************************************************************************************************
		public void showLongEntryArrow(bool inLongTrade){
			
			if ( inLongTrade ) { 
				return;
			}
				
			if ( High[0] > entry.longEntryPrice && Low[0] < entry.longEntryPrice ) {
				
				ArrowUp myArrowUp = Draw.ArrowUp(this, "LEmade"+ CurrentBar.ToString(), true, 0, entry.longEntryPrice - (TickSize * 5), Brushes.LimeGreen);
				//myArrowUp.OutlineBrush = Brushes.LimeGreen;
				entry.inLongTrade = true;
                //signals[0] = 1;
                signals[0] = 1;
				tradeData.signalName = "LE";
				entry.inShortTrade = false;
				entry.longEntryBarnum = CurrentBar;
				/// reset pivot data
				entry.pivStopCounter = 0;
				entry.lastPivotValue = swingData.lastLow ;
				entry.longEntryActual = entry.longEntryPrice;
				entry.pivLineLength = 0;
				entry.barsSinceEntry = 0;
			}
		}
		

		public void drawShortEntryLine(bool inShortTrade){
		
			if ( inShortTrade ) {
				return;
			}
			
			entry.shortLineLength++ ;
			
			if ( entry.shortEntryPrice != 0 ) {
				RemoveDrawObject("SeLine"+ (CurrentBar - 1));
				Draw.Line(this, "SeLine"+CurrentBar.ToString(), false, entry.shortLineLength, entry.shortEntryPrice, 0, 
					entry.shortEntryPrice, Brushes.Red, DashStyleHelper.Solid, 4);
				showShortEntryArrow(inShortTrade: entry.inShortTrade);
			}	
		}
		
		///******************************************************************************************************************************
		/// 
		/// 										Short Entry
		/// 	
		/// ****************************************************************************************************************************		
		public void showShortEntryArrow(bool inShortTrade) {
			
			if ( inShortTrade ) {
				return;
			}
			
			if ( High[0] > entry.shortEntryPrice && Low[0] < entry.shortEntryPrice ) {
				ArrowDown myArrowDn = Draw.ArrowDown(this, "SEmade"+ CurrentBar.ToString(), true, 0, entry.shortEntryPrice + (TickSize * 5), Brushes.Red);
				//myArrowDn.OutlineBrush = Brushes.Red;
				entry.inLongTrade = false;
				entry.inShortTrade = true;
                //signals[0] = -1;
                signals[0] = -1;
				tradeData.signalName = "SE";
				entry.shortEntryBarnum = CurrentBar;
				/// reset pivot data
				entry.pivStopCounter = 0;
				entry.lastPivotValue =  swingData.lastHigh;
				entry.shortEntryActual = entry.shortEntryPrice;
				entry.pivLineLength = 0;
				entry.barsSinceEntry = 0;
			}
		}
		///******************************************************************************************************************************
		/// 
		/// 										find edges
		/// 
		/// ****************************************************************************************************************************
		/// Show the number of confirmations of an edge
		public int edgeCount(bool up, bool plot){

			int upCount = 0;
			int dnCount = 0;
			int result = 0;
			
			/// rsi section
			if ( Rsi1[0]> 70 ) { upCount ++;}		
			if ( Rsi1[0] < 30 ) {	dnCount ++;} 

			/// bollinger section			
			if ( High[0] > Bollinger1.Upper[0] ) {	upCount ++; }	
			if ( Low[0] <  Bollinger1.Lower[0] ) {	dnCount ++; }
				
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
		
		/// find new highs 
		public void findNewHighs(int upCount, double minSwing){
			/// find min swing as pct of close, old hard coded value is 1.5
			/// 226 * 0.00663 = 1.49
			/// swingPct 0.005 = .9 - 1.2 and much better results
			double minPriceSwing = Close[0] * swingPct;;

			if ( upCount!= 0 && High[0] - swingData.lastLow > minPriceSwing ) {
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
		public void findNewLows(int dnCount, double minSwing){
			double minPriceSwing = Close[0] * swingPct;
			if ( dnCount!= 0 && swingData.lastHigh - Low[0] > minPriceSwing ) {
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
		public void findShortEntry() {
			if ( swingData.lastHighBarnum > swingData.lastLowBarnum ) {
				int distanceToLow = CurrentBar - swingData.lastLowBarnum;
				int distanceToHigh = CurrentBar - swingData.lastHighBarnum;
				int lastBar = CurrentBar -1;
				double upSwingDistance = swingData.lastHigh - swingData.lastLow;
				double upSwingEntry = upSwingDistance * 0.382;
				entry.shortEntryPrice = swingData.lastHigh - upSwingEntry;
				/// draw upswing in red
				RemoveDrawObject("upline"+lastBar);
				Draw.Line(this, "upline"+CurrentBar.ToString(), false, distanceToLow, swingData.lastLow, distanceToHigh, swingData.lastHigh, Brushes.DarkRed, DashStyleHelper.Dash, 2);
			
				/// draw entry line
				RemoveDrawObject("shortEntry"+lastBar);
				Draw.Line(this, "shortEntry"+CurrentBar.ToString(), false, distanceToLow, entry.shortEntryPrice , distanceToHigh, entry.shortEntryPrice , Brushes.Red, DashStyleHelper.Dash, 2);
				/// show swing high height
				double swingProfit = (upSwingDistance * 0.236) * 100;
			} else {
				// disable short entry
				entry.shortEntryPrice = 0;
				entry.shortLineLength = 0;
			}
		}
		
		/// looking long
		public void findLongeEntry() {
			if ( swingData.lastHighBarnum < swingData.lastLowBarnum ) {
				int distanceToHigh = CurrentBar - swingData.lastHighBarnum;
				int distanceToLow = CurrentBar - swingData.lastLowBarnum;
				int lastBar = CurrentBar -1;
				double dnSwingDistance = ( swingData.lastLow - swingData.lastHigh ) * -1;
				double dnSwingEntry = dnSwingDistance * 0.382;
				entry.longEntryPrice = swingData.lastLow + dnSwingEntry;
				
				/// draw down swing in green
				RemoveDrawObject("dnline"+lastBar);
				Draw.Line(this, "dnline"+CurrentBar.ToString(), false, distanceToHigh, swingData.lastHigh, distanceToLow, swingData.lastLow, Brushes.DarkGreen, DashStyleHelper.Dash, 2);
				
				/// draw entry line
				RemoveDrawObject("longEntry"+lastBar);
				Draw.Line(this, "longEntry"+CurrentBar.ToString(), false, distanceToHigh, entry.longEntryPrice, distanceToLow, entry.longEntryPrice, Brushes.LimeGreen, DashStyleHelper.Dash, 2);
				
				/// show swing low height
				double swingProfit = (dnSwingDistance * 0.236) * 100;				
			}	else {
				/// disable long entry
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
		
		
		#region Properies
		///  signal
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<int> Signals
		{
			get { return signals; }
		}
		
		///  inputs
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Shares", Order=1, GroupName="Parameters")]
		public int shares
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Swing Pct", Order=2, GroupName="Parameters")]
		public double swingPct
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Min Bars To Last Swing", Order=3, GroupName="Parameters")]
		public int minBarsToLastSwing
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Enable Hard Stop", Order=4, GroupName="Parameters")]
		public bool enableHardStop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Pct Hard Stop", Order=5, GroupName="Parameters")]
		public int pctHardStop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Enable Pivot Stop", Order=6, GroupName="Parameters")]
		public bool enablePivotStop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Pivot Stop Swing Size", Order=7, GroupName="Parameters")]
		public int pivotStopSwingSize
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="Pivot Stop Range", Order=8, GroupName="Parameters")]
		public double pivotStopPivotRange
		{ get; set; }
		
		/// Statistics
		[NinjaScriptProperty]
		[Display(Name="Show Up Count", Order=1, GroupName="Statistics")]
		public bool showUpCount
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Show Hard Stops", Order=2, GroupName="Statistics")]
		public bool showHardStops
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Show Trades On Chart", Order=3, GroupName="Statistics")]
		public bool printTradesOnChart
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Show Trades Simple", Order=4, GroupName="Statistics")]
		public bool printTradesSimple
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Send Trades To log", Order=5, GroupName="Statistics")]
		public bool printTradesTolog
		{ get; set; }
		
		#endregion
		
	}	
}



#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MooreTechSwing01[] cacheMooreTechSwing01;
		public MooreTechSwing01 MooreTechSwing01(int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog)
		{
			return MooreTechSwing01(Input, shares, swingPct, minBarsToLastSwing, enableHardStop, pctHardStop, enablePivotStop, pivotStopSwingSize, pivotStopPivotRange, showUpCount, showHardStops, printTradesOnChart, printTradesSimple, printTradesTolog);
		}

		public MooreTechSwing01 MooreTechSwing01(ISeries<double> input, int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog)
		{
			if (cacheMooreTechSwing01 != null)
				for (int idx = 0; idx < cacheMooreTechSwing01.Length; idx++)
					if (cacheMooreTechSwing01[idx] != null && cacheMooreTechSwing01[idx].shares == shares && cacheMooreTechSwing01[idx].swingPct == swingPct && cacheMooreTechSwing01[idx].minBarsToLastSwing == minBarsToLastSwing && cacheMooreTechSwing01[idx].enableHardStop == enableHardStop && cacheMooreTechSwing01[idx].pctHardStop == pctHardStop && cacheMooreTechSwing01[idx].enablePivotStop == enablePivotStop && cacheMooreTechSwing01[idx].pivotStopSwingSize == pivotStopSwingSize && cacheMooreTechSwing01[idx].pivotStopPivotRange == pivotStopPivotRange && cacheMooreTechSwing01[idx].showUpCount == showUpCount && cacheMooreTechSwing01[idx].showHardStops == showHardStops && cacheMooreTechSwing01[idx].printTradesOnChart == printTradesOnChart && cacheMooreTechSwing01[idx].printTradesSimple == printTradesSimple && cacheMooreTechSwing01[idx].printTradesTolog == printTradesTolog && cacheMooreTechSwing01[idx].EqualsInput(input))
						return cacheMooreTechSwing01[idx];
			return CacheIndicator<MooreTechSwing01>(new MooreTechSwing01(){ shares = shares, swingPct = swingPct, minBarsToLastSwing = minBarsToLastSwing, enableHardStop = enableHardStop, pctHardStop = pctHardStop, enablePivotStop = enablePivotStop, pivotStopSwingSize = pivotStopSwingSize, pivotStopPivotRange = pivotStopPivotRange, showUpCount = showUpCount, showHardStops = showHardStops, printTradesOnChart = printTradesOnChart, printTradesSimple = printTradesSimple, printTradesTolog = printTradesTolog }, input, ref cacheMooreTechSwing01);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MooreTechSwing01 MooreTechSwing01(int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog)
		{
			return indicator.MooreTechSwing01(Input, shares, swingPct, minBarsToLastSwing, enableHardStop, pctHardStop, enablePivotStop, pivotStopSwingSize, pivotStopPivotRange, showUpCount, showHardStops, printTradesOnChart, printTradesSimple, printTradesTolog);
		}

		public Indicators.MooreTechSwing01 MooreTechSwing01(ISeries<double> input , int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog)
		{
			return indicator.MooreTechSwing01(input, shares, swingPct, minBarsToLastSwing, enableHardStop, pctHardStop, enablePivotStop, pivotStopSwingSize, pivotStopPivotRange, showUpCount, showHardStops, printTradesOnChart, printTradesSimple, printTradesTolog);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MooreTechSwing01 MooreTechSwing01(int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog)
		{
			return indicator.MooreTechSwing01(Input, shares, swingPct, minBarsToLastSwing, enableHardStop, pctHardStop, enablePivotStop, pivotStopSwingSize, pivotStopPivotRange, showUpCount, showHardStops, printTradesOnChart, printTradesSimple, printTradesTolog);
		}

		public Indicators.MooreTechSwing01 MooreTechSwing01(ISeries<double> input , int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog)
		{
			return indicator.MooreTechSwing01(input, shares, swingPct, minBarsToLastSwing, enableHardStop, pctHardStop, enablePivotStop, pivotStopSwingSize, pivotStopPivotRange, showUpCount, showHardStops, printTradesOnChart, printTradesSimple, printTradesTolog);
		}
	}
}

#endregion
