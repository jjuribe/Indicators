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

using System.Net;
using System.Net.Cache;
using System.Web.Script.Serialization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	/// <summary>
	/// Adaptation of Morretech Swing system intended for 15min bars and longer
	/// Developed by Warren Hansen at SwiftSense on 7/20/2017
	/// whansen1@mac.com
	/// </summary>
	
	public class MooreTechSwing03 : Indicator
	{
	
		private struct SwingData
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
		
		private struct EntryData
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
		
		private struct TradeData
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
		
		private struct PriceData
		{
			public  string 	date 	{ get; set; }
			public  string 	ticker 	{ get; set; }
			public  string 	bartype 	{ get; set; }
			public	double 	open	{ get; set; }
			public	double 	high 	{ get; set; }
			public	double 	low		{ get; set; }
			public  double 	close	{ get; set; }
			public	double 	signal	{ get; set; }
			public	int 	trade	{ get; set; }
			public 	string	connectStatus { get; set; }
			public 	string	connectTime { get; set; }
			
			public  double 	longEntryPrice	{ get; set; }
			public	double 	shortEntryPrice	{ get; set; }
			
			public	int 	longLineLength	{ get; set; }
			public	int 	shortLineLength	{ get; set; }
			public	int 	currentBar	{ get; set; }
			
			public	bool	inLong { get; set; }
			public	bool	inShort { get; set; }
			
			public 	DateTime	barTime; // { get; set; };
			
			//entry.inLongTrade = false;
		}
		
		private PriceData priceData = new PriceData{};
		private List<PriceData> myList = new List<PriceData>();
		private string conMessage = "No Message";
		private string conTime = "00:00 AM";
		
		private Swing 		Swing1;	
		private FastPivotFinder FastPivotFinder1;
		
		private Brush 	upColor; 
		private Brush 	downColor;
		private Brush 	textColor;
		
		private Series<int> signals;
		private SwingData swingData = new SwingData{};
		private EntryData entry = new EntryData{};
		private TradeData tradeData = new TradeData{};

		private bool secondPivStopFlag = false;
		int cofirmationCount = 1;
		
		private DateTime 	myDateTime; 
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "_MooreTech Swing 3";
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
				shares				= 100;				
				swingPct			= 0.005;			
				minBarsToLastSwing 	= 70;				
				enableHardStop 		= true;				
				pctHardStop  		= 3;
				enablePivotStop 	= true;				
				pivotStopSwingSize 	= 5;
				pivotStopPivotRange = 0.2;				
				/// swow plots
				showUpCount 			= false;		
				showHardStops 			= false;		
				printTradesOnChart		= false;		
				printTradesSimple 		= false;		
				printTradesTolog 		= true;	
				sendOnlyCurrentBarToFirebase	= true;
			}
			else if (State == State.Configure)
			{
				upColor 	= Brushes.LimeGreen;
				downColor	= Brushes.Crimson;
				textColor	= Brushes.Crimson;
            }
			else if(State == State.DataLoaded)
			  {
				  ClearOutputWindow();     
				  Swing1				= Swing(5);	// for piv stops
				  FastPivotFinder1 = FastPivotFinder(false, false, 70, 0.005, 1);
				  signals = new Series<int>(this, MaximumBarsLookBack.Infinite); // for starategy integration
			  } 
		}
		
		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
		  if(connectionStatusUpdate.Status == ConnectionStatus.Connected)
		  {
		     Print("observed Connected at " + DateTime.Now);
			 priceData.connectStatus = "Connected";
			 priceData.connectTime		=	DateTime.Now.ToShortTimeString();
			 conMessage = "Connected";  conTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
			 updateConnection(); 	  
		  }
		  else if(connectionStatusUpdate.Status == ConnectionStatus.ConnectionLost)
		  {
		     Print("observed Connection lost at: " + DateTime.Now);
			 priceData.connectStatus = "Connection lost";
			 priceData.connectTime		=	DateTime.Now.ToShortTimeString();
			 conMessage = priceData.connectStatus; conTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
			 updateConnection() ;
		  }
		  else if(connectionStatusUpdate.Status == ConnectionStatus.Disconnected)
		  {
		     Print("observed Disconnected  at: " + DateTime.Now);
			 priceData.connectStatus = "Disconnected";
			 priceData.connectTime		=	DateTime.Now.ToShortTimeString();
			 conMessage = priceData.connectStatus; conTime = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToShortTimeString();
			 updateConnection();
		  }
		}
		
		///******************************************************************************************************************************
		/// 
		/// 										on bar update
		/// 
		/// ****************************************************************************************************************************
		protected override void OnBarUpdate()
		{
			if ( CurrentBar < 20 ) { resetStruct(doIt: true); return; }
			
			resetBarsSinceEntry();
			findSwings();

			findShortEntry();
			findLongeEntry();
			
			drawLongEntryLine(inLongTrade: entry.inLongTrade);
			drawShortEntryLine(inShortTrade: entry.inShortTrade);
			
			showEntryDelay();
			
			if( enableHardStop ) { setHardStop(pct: pctHardStop, shares: shares, plot: showHardStops);}
			if ( enablePivotStop ) { setPivotStop(swingSize: pivotStopSwingSize, pivotSlop: pivotStopPivotRange); }
			
			updatePriceData();
			///MARK: - TODO Make ASYNC
			pushToFirebase(debug: false);
		}
		
		private void updateConnection() {
		
			/// get last file
			int lastIndex = myList.Count - 1;
			var lastFile = myList[lastIndex];
			string oldMessage = lastFile.connectStatus;
			Print("Last File: " + lastFile.date + " " + oldMessage);
			/// update the copy of last file
			lastFile.connectStatus = conMessage;
			lastFile.connectTime = conTime;
			Print("Update last file index? "  + lastIndex + " "+ lastFile.date + " " + lastFile.connectStatus + " " + lastFile.connectTime);
			/// delete last item in array
			myList.RemoveAt(lastIndex);
			/// add this copy of item to array
			myList.Add(lastFile);
			/// prove it
			var checkFile = myList[lastIndex];
			Print("\nProve it: " + checkFile.date + "---> " + checkFile.connectStatus + " " + checkFile.connectTime +"\n");
			pushToFirebase(debug: false);
		}
		
		private void updatePriceData() {
			/// update data
			priceData.ticker 	=  	Instrument.MasterInstrument.Name;
			priceData.bartype	=	BarsPeriod.Value.ToString() + " " + BarsPeriod.BarsPeriodType.ToString();
			priceData.date		= 	Time[0].ToString();
			priceData.open		= 	Open[0];
			priceData.high 		= 	High[0];
			priceData.low		=	Low[0];
			priceData.close		=	Close[0];
			
			if (signals[1] == 0 && signals[0] != 0 ) {	/// onlu add 1 entry arrow
				priceData.signal 	= 	signals[0];
			} else {
				priceData.signal 	=  0;
			}
			
			
			priceData.trade  		=	signals[0]; 						/// int -2,-1, 0 1 2 for trade entry
			priceData.connectStatus = 	conMessage;
			priceData.connectTime 	= 	conTime;
			
			priceData.longEntryPrice 	= 	Math.Abs(entry.longEntryPrice);
			priceData.shortEntryPrice 	= 	Math.Abs(entry.shortEntryPrice);
			priceData.longLineLength 	= 	entry.longLineLength;
			priceData.shortLineLength 	= 	entry.shortLineLength;
			priceData.currentBar = CurrentBar - 20;				/// match current bar to index number in firebase
			priceData.barTime = Time[0];
			priceData.inShort = entry.inShortTrade;
			priceData.inLong = entry.inLongTrade;

			///add to array
			myList.Add(priceData);
			var timeloading = Time[0];
		}
		
		private async Task pushToFirebase( bool debug) {
			
			var thirtyMin = new TimeSpan(0, 30, 0);
			var lastPrice = myList.Last();
			var myDateTime = lastPrice.barTime;
			var myDateTimePlusSome = myDateTime.Add(thirtyMin);
			
			/// only update on last bar loaded
			if (true)
			if (myDateTimePlusSome < DateTime.Now && sendOnlyCurrentBarToFirebase ) { return; }
			Print("Passed time test, sending to firebase");
			
			string jsonDataset = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(myList);
			var myFirebase = "https://mtdash01.firebaseio.com/.json";
            var request = WebRequest.CreateHttp(myFirebase);
			request.Method = "PUT";		// put wrote over
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonDataset);
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;
			Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
			dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
			reader.Close();
            dataStream.Close();
            response.Close();
            request.Abort();
		}
		
		public void checkForNil(object myobj) {
			if (myobj == null) {
				Print( CurrentBar.ToString() +  " Nil: " + myobj.ToString() );
			}
		}
		
		public void findSwings() {
			/// attempt to tighten swing confirmation after 2nd piv stop out
			if ( secondPivStopFlag ) {
				cofirmationCount = 3;
				BarBrush = Brushes.Goldenrod;
				CandleOutlineBrush = Brushes.Goldenrod;
			
				swingData.lastHigh 			= FastPivotFinder(false, false, 70, 0.005, cofirmationCount).LastHigh[0];
				swingData.lastHighBarnum	= FastPivotFinder(false, false, 70, 0.005, cofirmationCount).LastHighBarnum;
				swingData.lastLow 			= FastPivotFinder(false, false, 70, 0.005, cofirmationCount).LastLow[0];
				swingData.lastLowBarnum		= FastPivotFinder(false, false, 70, 0.005, cofirmationCount).LastLowBarnum;
				swingData.prevHigh			= FastPivotFinder(false, false, 70, 0.005, cofirmationCount).PrevHigh;
				swingData.prevHighBarnum	= FastPivotFinder(false, false, 70, 0.005, cofirmationCount).PrevHighBarnum;
				swingData.prevLow			= FastPivotFinder(false, false, 70, 0.005, cofirmationCount).PrevLow;
				swingData.prevLowBarnum		= FastPivotFinder(false, false, 70, 0.005, cofirmationCount).PrevLowBarnum;	
				
			} else {
				cofirmationCount = 1;
			
				swingData.lastHigh 			= FastPivotFinder1.LastHigh[0];
				swingData.lastHighBarnum	= FastPivotFinder1.LastHighBarnum;
				swingData.lastLow 			= FastPivotFinder1.LastLow[0];
				swingData.lastLowBarnum		= FastPivotFinder1.LastLowBarnum;
				swingData.prevHigh			= FastPivotFinder1.PrevHigh;
				swingData.prevHighBarnum	= FastPivotFinder1.PrevHighBarnum;
				swingData.prevLow			= FastPivotFinder1.PrevLow;
				swingData.prevLowBarnum		= FastPivotFinder1.PrevLowBarnum;	
			}
		}

		public void resetBarsSinceEntry() {
			if ( entry.inShortTrade == true) { entry.barsSinceEntry = CurrentBar - entry.shortEntryBarnum; }
			else if ( entry.inLongTrade == true) { entry.barsSinceEntry = CurrentBar - entry.longEntryBarnum; }
			else { entry.barsSinceEntry = 0;}
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
				 Text myText = Draw.Text(this, "HighSwingtxt"+ CurrentBar.ToString(), entry.pivStopCounter.ToString(), swingSize, High[swingSize] + (TickSize * 10));
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
				secondPivStopFlag = true;
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
				secondPivStopFlag = true;
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
				entry.hardStopLine = Math.Abs(Close[0]  - ( Close[0] * pctPrice));
				}
			/// find short entry price /// calc trade cost
			if (CurrentBar == entry.shortEntryBarnum ) {
				double pctPrice = pct * 0.01;
				entry.hardStopLine = Math.Abs(Close[0]  + ( Close[0] * pctPrice));
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
		
		public void customDrawTrades(bool show, bool simple) {
			// set color
			if( tradeData.tradeProfit >= 0 ) { textColor = upColor;
			} else { textColor = downColor;}
			// set text
			string reportData = tradeData.report;
			if(simple) { reportData = tradeData.reportSimple;}
			if(show) {
			if (CurrentBar == entry.longEntryBarnum ) {
				Draw.Text(this, "LE"+CurrentBar, reportData, 0, MIN(Low, 20)[0] - (TickSize * 5), textColor); }
			if (CurrentBar == entry.shortEntryBarnum ) {
				Draw.Text(this, "SE"+CurrentBar, reportData, 0, MAX(High, 20)[0] + (TickSize * 5), textColor); }
			if (CurrentBar == entry.shortHardStopBarnum ) {
				Draw.Text(this, "SXh"+CurrentBar, reportData, 0, MAX(High, 20)[0] + (TickSize * 5), textColor); }
			if (CurrentBar == entry.longHardStopBarnum ) {
				Draw.Text(this, "LXh"+CurrentBar, reportData, 0, MIN(Low, 20)[0] - (TickSize * 5), textColor); }
			if (CurrentBar == entry.shortPivStopBarnum ) {
				Draw.Text(this, "SXp"+CurrentBar, reportData, 0, MAX(High, 20)[0] + (TickSize * 5), textColor); }
			if (CurrentBar == entry.longPivStopBarnum ) {
				Draw.Text(this, "LXp"+CurrentBar, reportData, 0, MIN(Low, 20)[0] - (TickSize * 5), textColor); }
			} else {
				Draw.Text(this, "report"+CurrentBar, reportData, 0, MIN(Low, 20)[0]);
			}
		}
		
		/// Long Entry Line ************************************************************************************************************
		public void drawLongEntryLine(bool inLongTrade){
			if ( inLongTrade ) { return; }
			entry.longLineLength++ ;
			if ( entry.longEntryPrice != 0 ) {
				if(DrawObjects["LeLine"+(CurrentBar-1).ToString()] != null)
					RemoveDrawObject("LeLine"+ (CurrentBar - 1));
				Draw.Line(this, "LeLine"+CurrentBar.ToString(), false, entry.longLineLength, entry.longEntryPrice, 0, 
						entry.longEntryPrice, Brushes.LimeGreen, DashStyleHelper.Solid, 4);
				showLongEntryArrow(inLongTrade: entry.inLongTrade);
			}	
		}
		/// <summary>
		///  Long Entry Arrow 
		/// </summary>
		/// <param name="inLongTrade"></param>
		public void showLongEntryArrow(bool inLongTrade){	
			if ( inLongTrade ) { return; }	
			if(entry.longEntryPrice == null) { return; }
			if ( High[0] > entry.longEntryPrice && Low[0] < entry.longEntryPrice ) {
				//Draw.Text(this, "LE"+CurrentBar.ToString(), "LE", 0, entry.longEntryPrice - (TickSize * 10), Brushes.LimeGreen);
				//customDrawTrades( show: true,  simple: false);
				ArrowUp myArrowUp = Draw.ArrowUp(this, "LEmade"+ CurrentBar.ToString(), true, 0, entry.longEntryPrice - (TickSize * 5), Brushes.LimeGreen);
				signals[0] = 1;
				secondPivStopFlag = false;
				//debugEntry(isOn:true);
			}
		}

		/// Short Entry Line ************************************************************************************************************
		public void drawShortEntryLine(bool inShortTrade){
			if ( inShortTrade ) {return;}
			entry.shortLineLength++ ;
			if ( entry.shortEntryPrice != 0 ) {
				if(DrawObjects["SeLine"+(CurrentBar-1).ToString()] != null)
					RemoveDrawObject("SeLine"+ (CurrentBar - 1));
				Draw.Line(this, "SeLine"+CurrentBar.ToString(), false, entry.shortLineLength, entry.shortEntryPrice, 0, 
					entry.shortEntryPrice, Brushes.Red, DashStyleHelper.Solid, 4);
				showShortEntryArrow(inShortTrade: entry.inShortTrade);
			}	
		}
		
		/// <summary>
		///  Short Entry Arrow 
		/// </summary>
		/// <param name="inLongTrade"></param>		
		public void showShortEntryArrow(bool inShortTrade) {
			if ( inShortTrade ) {return;}
			if(entry.shortEntryPrice == null) { return; }
			if ( High[0] > entry.shortEntryPrice && Low[0] < entry.shortEntryPrice ) {
				//Draw.Text(this, "SE"+CurrentBar.ToString(), "SE", 0, entry.shortEntryPrice + (TickSize * 10), Brushes.Crimson);
				ArrowDown myArrowDn = Draw.ArrowDown(this, "SEmade"+ CurrentBar.ToString(), true, 0, entry.shortEntryPrice + (TickSize * 5), Brushes.Red);
				signals[0] = -1;
				secondPivStopFlag = false;
			}
		}
		///  entry marked and rcorded next bar for pullback benifit
		public void showEntryDelay() {
			/// Long Signal Found 1 bar ago
			if ( signals[1] == 1 ) {
				
				/// if short or flat
				if (entry.inShortTrade || ( !entry.inShortTrade && ! entry.inLongTrade )) {
					/// if long entry benificial or still over entry line go long else exit
					if (Close[0] <= Close[1] || Close[0] <= Low[1] || ( High[0] > entry.longEntryPrice && Low[0] < entry.longEntryPrice )) {
						entry.inLongTrade = true;
						entry.inShortTrade = false;
						tradeData.signalName = "LE";
						entry.longEntryBarnum = CurrentBar;
						/// reset pivot data
						entry.pivStopCounter = 0;
						entry.lastPivotValue = swingData.lastLow ;
						entry.longEntryActual = Close[0];
						entry.pivLineLength = 0;
						entry.barsSinceEntry = 0;
						Draw.Dot(this, "actualLE"+CurrentBar, true, 0, Open[0], Brushes.LimeGreen);
						customDrawTrades( show: true,  simple: false);
					} else {
						exitFromGap();
						Draw.Dot(this, "SXGapDot"+CurrentBar, true, 0, Close[0], Brushes.Yellow);
						Draw.Text(this, "SXGap"+CurrentBar, "SX-Gap", 0, Open[0], Brushes.Yellow);
					}	
				}
			}
			/// Short signal found
			if ( signals[1] == -1 ) {
				//entry.inShortTrade = true;
				/// if long or flat and missed signal was short, go short
				if (entry.inLongTrade || ( !entry.inShortTrade && ! entry.inLongTrade )) {
					/// if short entry benificial or still over entry line go short else exit
					if (Close[0] >= Close[1] || Close[0] > Low[1] || ( High[0] > entry.shortEntryPrice && Low[0] < entry.shortEntryPrice ) ) {
						/// normal trade entry
						entry.inLongTrade = false;
						entry.inShortTrade = true;
						tradeData.signalName = "SE";
						entry.shortEntryBarnum = CurrentBar;
						/// reset pivot data
						entry.pivStopCounter = 0;
						entry.lastPivotValue =  swingData.lastHigh;
						entry.shortEntryActual = entry.shortEntryPrice;
						entry.pivLineLength = 0;
						entry.barsSinceEntry = 0;
						Draw.Dot(this, "actualSE"+CurrentBar, true, 0, Open[0], Brushes.Crimson);
						customDrawTrades( show: true,  simple: false);
						
					} else {
						exitFromGap();
						Draw.Dot(this, "LXGapDot"+CurrentBar, true, 0, Open[0], Brushes.Yellow);
						Draw.Text(this, "LXGap"+CurrentBar, "LX-Gap", 0, High[0], Brushes.Yellow);
					}
				}
			}
		}
		
		/// exit from adversarial gap
		public void exitFromGap() {
			if ( entry.inLongTrade  ) {
				/// need short trades to debug this no long stops hit
				entry.inLongTrade = false;
                signals[0] = 2;
				entry.longHardStopBarnum	= CurrentBar;
				tradeData.signalName = "LX - Gap";
				entry.barsSinceEntry = 0;
			} else if ( entry.inShortTrade  ) {
				/// need short trades to debug this no long stops hit
				entry.inShortTrade = false;
                signals[0] = -2;
				entry.shortHardStopBarnum	= CurrentBar;
				tradeData.signalName = "SX - HS";
				entry.barsSinceEntry = 0;
			}
		}
		
		/// looking short
		public void findShortEntry() {
			if ( swingData.lastHighBarnum > swingData.lastLowBarnum ) {
				int distanceToLow = CurrentBar - swingData.lastLowBarnum;
				int distanceToHigh = CurrentBar - swingData.lastHighBarnum;
				int lastBar = CurrentBar -1;
				double upSwingDistance = Math.Abs(swingData.lastHigh - swingData.lastLow);
				double upSwingEntry = Math.Abs(upSwingDistance * 0.382);
				entry.shortEntryPrice = Math.Round(swingData.lastHigh - upSwingEntry, 2);
				
				/// draw upswing in red
				RemoveDrawObject("upline"+lastBar);
				Draw.Line(this, "upline"+CurrentBar.ToString(), false, distanceToLow, swingData.lastLow, distanceToHigh, swingData.lastHigh, Brushes.DarkRed, DashStyleHelper.Dash, 2);
				/// draw entry line
				RemoveDrawObject("shortEntry"+lastBar);
				Draw.Line(this, "shortEntry"+CurrentBar.ToString(), false, distanceToLow, entry.shortEntryPrice , distanceToHigh, entry.shortEntryPrice , Brushes.Red, DashStyleHelper.Dash, 2);
				/// show swing high height
				
				double swingProfit = Math.Abs((upSwingDistance * 0.236) * 100);
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
				double dnSwingDistance = Math.Abs(( swingData.lastLow - swingData.lastHigh ) * -1);
				double dnSwingEntry = Math.Abs(dnSwingDistance * 0.382);
				entry.longEntryPrice = Math.Round( swingData.lastLow + dnSwingEntry, 2);  // inputValue = Math.Round(inputValue, 2);
		
				/// draw down swing in green
				RemoveDrawObject("dnline"+lastBar);
				Draw.Line(this, "dnline"+CurrentBar.ToString(), false, distanceToHigh, swingData.lastHigh, distanceToLow, swingData.lastLow, Brushes.DarkGreen, DashStyleHelper.Dash, 2);
				/// draw entry line
				RemoveDrawObject("longEntry"+lastBar);
				Draw.Line(this, "longEntry"+CurrentBar.ToString(), false, distanceToHigh, entry.longEntryPrice, distanceToLow, entry.longEntryPrice, Brushes.LimeGreen, DashStyleHelper.Dash, 2);
				
				/// show swing low height
				double swingProfit = Math.Abs((dnSwingDistance * 0.236) * 100);				
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
			entry.inShortTrade = true;
			entry.inLongTrade = true; 
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
		
		//disableFullPush
		[NinjaScriptProperty]
		[Display(Name="Only send currentbar to Firebase", Order=6, GroupName="Statistics")]
		public bool sendOnlyCurrentBarToFirebase
		{ get; set; }
		#endregion
		
	}	
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private MooreTechSwing03[] cacheMooreTechSwing03;
		public MooreTechSwing03 MooreTechSwing03(int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog, bool sendOnlyCurrentBarToFirebase)
		{
			return MooreTechSwing03(Input, shares, swingPct, minBarsToLastSwing, enableHardStop, pctHardStop, enablePivotStop, pivotStopSwingSize, pivotStopPivotRange, showUpCount, showHardStops, printTradesOnChart, printTradesSimple, printTradesTolog, sendOnlyCurrentBarToFirebase);
		}

		public MooreTechSwing03 MooreTechSwing03(ISeries<double> input, int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog, bool sendOnlyCurrentBarToFirebase)
		{
			if (cacheMooreTechSwing03 != null)
				for (int idx = 0; idx < cacheMooreTechSwing03.Length; idx++)
					if (cacheMooreTechSwing03[idx] != null && cacheMooreTechSwing03[idx].shares == shares && cacheMooreTechSwing03[idx].swingPct == swingPct && cacheMooreTechSwing03[idx].minBarsToLastSwing == minBarsToLastSwing && cacheMooreTechSwing03[idx].enableHardStop == enableHardStop && cacheMooreTechSwing03[idx].pctHardStop == pctHardStop && cacheMooreTechSwing03[idx].enablePivotStop == enablePivotStop && cacheMooreTechSwing03[idx].pivotStopSwingSize == pivotStopSwingSize && cacheMooreTechSwing03[idx].pivotStopPivotRange == pivotStopPivotRange && cacheMooreTechSwing03[idx].showUpCount == showUpCount && cacheMooreTechSwing03[idx].showHardStops == showHardStops && cacheMooreTechSwing03[idx].printTradesOnChart == printTradesOnChart && cacheMooreTechSwing03[idx].printTradesSimple == printTradesSimple && cacheMooreTechSwing03[idx].printTradesTolog == printTradesTolog && cacheMooreTechSwing03[idx].sendOnlyCurrentBarToFirebase == sendOnlyCurrentBarToFirebase && cacheMooreTechSwing03[idx].EqualsInput(input))
						return cacheMooreTechSwing03[idx];
			return CacheIndicator<MooreTechSwing03>(new MooreTechSwing03(){ shares = shares, swingPct = swingPct, minBarsToLastSwing = minBarsToLastSwing, enableHardStop = enableHardStop, pctHardStop = pctHardStop, enablePivotStop = enablePivotStop, pivotStopSwingSize = pivotStopSwingSize, pivotStopPivotRange = pivotStopPivotRange, showUpCount = showUpCount, showHardStops = showHardStops, printTradesOnChart = printTradesOnChart, printTradesSimple = printTradesSimple, printTradesTolog = printTradesTolog, sendOnlyCurrentBarToFirebase = sendOnlyCurrentBarToFirebase }, input, ref cacheMooreTechSwing03);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.MooreTechSwing03 MooreTechSwing03(int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog, bool sendOnlyCurrentBarToFirebase)
		{
			return indicator.MooreTechSwing03(Input, shares, swingPct, minBarsToLastSwing, enableHardStop, pctHardStop, enablePivotStop, pivotStopSwingSize, pivotStopPivotRange, showUpCount, showHardStops, printTradesOnChart, printTradesSimple, printTradesTolog, sendOnlyCurrentBarToFirebase);
		}

		public Indicators.MooreTechSwing03 MooreTechSwing03(ISeries<double> input , int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog, bool sendOnlyCurrentBarToFirebase)
		{
			return indicator.MooreTechSwing03(input, shares, swingPct, minBarsToLastSwing, enableHardStop, pctHardStop, enablePivotStop, pivotStopSwingSize, pivotStopPivotRange, showUpCount, showHardStops, printTradesOnChart, printTradesSimple, printTradesTolog, sendOnlyCurrentBarToFirebase);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.MooreTechSwing03 MooreTechSwing03(int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog, bool sendOnlyCurrentBarToFirebase)
		{
			return indicator.MooreTechSwing03(Input, shares, swingPct, minBarsToLastSwing, enableHardStop, pctHardStop, enablePivotStop, pivotStopSwingSize, pivotStopPivotRange, showUpCount, showHardStops, printTradesOnChart, printTradesSimple, printTradesTolog, sendOnlyCurrentBarToFirebase);
		}

		public Indicators.MooreTechSwing03 MooreTechSwing03(ISeries<double> input , int shares, double swingPct, int minBarsToLastSwing, bool enableHardStop, int pctHardStop, bool enablePivotStop, int pivotStopSwingSize, double pivotStopPivotRange, bool showUpCount, bool showHardStops, bool printTradesOnChart, bool printTradesSimple, bool printTradesTolog, bool sendOnlyCurrentBarToFirebase)
		{
			return indicator.MooreTechSwing03(input, shares, swingPct, minBarsToLastSwing, enableHardStop, pctHardStop, enablePivotStop, pivotStopSwingSize, pivotStopPivotRange, showUpCount, showHardStops, printTradesOnChart, printTradesSimple, printTradesTolog, sendOnlyCurrentBarToFirebase);
		}
	}
}

#endregion
