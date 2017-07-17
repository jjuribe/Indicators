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
/*
	ablesys daily and 4H Signal
	pullback to MA entry
	marketPosition = 1 -1 then
	close below ATR as exit
	P+L  
	explore atr target 
		show stop and 1X Target
	get rid of entry bar exits
	make sections into functions or reigons
	functions
	pullback to ma
	bullish bar
	draw line from entry to exit
	eas trend 1
	if 2 LR wait for break in range?
	after 2 LR mostly wins 2LR no good
	make longer time fram adjustable from daily 240m 60m 30m
		Add These Vars
		private int munutesHtf = 1440;
		private bool 	enterNearMA	=	true; 	// 	Good	38tr 52.6% No Bar Dir = 43t 53.5%
		private bool 	enterT2		=	false; 	//	Bad		XX% No Bar Dir = 37tr 37.8%
		private bool 	barDir		=	false;	//	Bad
		private bool	enterT1		=	true; 	// 	Good	56% No Bar Dir = 58%
		private double 	minStopDistance = 0.008;
		private bool 	useAtrStop = false;  // tend to hurt profit
		private bool 	useAtrTraget = true;
	Add Signal output 1 = long, 2 = LX, -1 = Short, -2 = SX
	Make strategy
	Optimize strategy

	when strat is 60% profitable - forex boat improvements
	1. Filters
	2. Volume
	3. Trailstop
	4. session time
	5. Stop + Target
		statistical hard stop at entry
		statistical volatility stop at entry
		ststistical atr target that is continually optimized my mean of MPE

*/
namespace NinjaTrader.NinjaScript.Indicators
{
	public class AblesysMTFD : Indicator
	{
		private Series<double> myDataSeries;
		private Series<int> trend;
		private Series<int> signals;
		
		private double 	UpperLTF;
		private double 	LowerLTF;
		private double 	UpperHTF;
		private double 	LowerHTF;
		private	double	LTFstate 	= 0;	// 0 = short 1 = long
		private	double	HTFstate 	= 0;	// 0 = short 1 = long
		private bool	shadingOn	=	true;
		//private Font	drawfont	= new Font("Arial", 12, FontStyle.Bold);
		private TextPosition tpos	= TextPosition.BottomRight;
		private Brush	bgcolor		= Brushes.DodgerBlue;
		private Brush	fgcolor		= Brushes.White;
		private int		opacity		= 5;
		private string	text		= "Here";
	
		private string 	alertSoundFile 	= "Trend_Long.wav";
		private string 	alertSoundFileShort 	= "Trend_Short.wav";
		private bool	audio		=	true;
		private int		opacityBKG	= 25;
		
		private double 	buyZone;
		private double 	sellZone;
		private int		entrySpace = 200;
		private int		marketPosition = 0;
		
		private double	dATR		= 2;
		private int		dPeriod		= 12;
		private int		dRisk		= 4;
		
		// performance
		private double	entryPrice;
		private double	stopPrice;
		private double	exitPrice;
		private double	tradeProfitLoss;
		private double	cumProfitLoss;
		private double	numTrades;
		private double	numWin;
		private double	pctWin;
		private double	sumWinners;
		private double	sumLosers;
		private double	numLoss; 
		private double	profitFactor;
		private string 	performanceSummary;
		
		private double	origStop; 
		private double	origTarget; 
		
		private Brush BackGreen	= new SolidColorBrush(Color.FromArgb(255, 0,70,30));
		private Brush BackRed	= new SolidColorBrush(Color.FromArgb(255, 255,0,0));
		private Brush TextColor;
		
		private double stopDistance;
		private int entryBarNumber;
		private bool colorBarsAgreement = false;
		
		// draw trade lines
		private	int 	exitBarNumber;
		private	bool 	win;
		private bool 	longFilter;
		private bool 	shortFilter;
		
		/*
		// highertime frame
		private int munutesHtf = 1440;
		
		/// filters
		private bool 	enterNearMA	=	true; 	// 	Good	38tr 52.6% No Bar Dir = 43t 53.5%
		private bool 	enterT2		=	false; 	//	Bad		XX% No Bar Dir = 37tr 37.8%
		private bool 	barDir		=	false;	//	Bad
		private bool	enterT1		=	true; 	// 	Good	56% No Bar Dir = 58%
		
		//  0.008 nearMA + T1  37 tr 56.8% $652    No Bar Dir = 34tr 58.8% $799
		
		/// target
		private double minStopDistance = 0.008;
		/// stop
		private bool useAtrStop = false;  // tend to hurt profit
		private bool useAtrTraget = true;
		*/
		/// reporting
//		private bool 	showTradesChart = true;
//		private bool 	showTradesLog = true;
		
		#region Using OnStateChange
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Ablesys MTF D";
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
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Dot, "ATRTrailingUp");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "ATRTrailingDn");
				AddPlot(new Stroke(Brushes.DodgerBlue, 2), PlotStyle.Dot, "HTFup");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "HTFdn");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Minute, munutesHtf);
			}
			else if(State == State.DataLoaded)
			  {
			    //clear the output window as soon as the bars data is loaded
			    ClearOutputWindow();  	
				myDataSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				trend = new Series<int>(this, MaximumBarsLookBack.Infinite);    
				signals = new Series<int>(this, MaximumBarsLookBack.Infinite);
			  }
		}
		#endregion

		protected override void OnBarUpdate()
		{
			
			#region Ablesys Signals
			
			if (CurrentBars[0] < 20)
			return;

			// set up higher time frame
			foreach(int CurrentBarI in CurrentBars)
			{
				if (CurrentBarI < BarsRequiredToPlot)
				{
					return;
				}
			}

			// HTF bars
			if (BarsInProgress == 1)
			{
				LowerHTF = AblesysT2(dATR, dPeriod, dRisk).Lower[0];
				UpperHTF = AblesysT2(dATR, dPeriod, dRisk).Upper[0];
				
				if ( LowerHTF != 0 )		// short signal
					{
						HTFdn[0] = LowerHTF;
						HTFstate = 0;
					}	
				if ( UpperHTF != 0 )		// Long signal
					{
						HTFup[0] = UpperHTF;
						HTFstate = 1;
					}
				return;
			}
			
			// lower time frame bars
			if (BarsInProgress == 0)
			{

				// Ablesys trend 1
				myDataSeries[0]		= (-100 * (MAX(High, Period)[0] - Close[0]) / (MAX(High, Period)[0] - MIN(Low, Period)[0] == 0 ? 1 : MAX(High, Period)[0] - MIN(Low, Period)[0]));
		
				if (myDataSeries[0] >= -30)
				{
					CandleOutlineBrush = Brushes.DarkBlue;
					if(Open[0]<Close[0] ) {
							BarBrush = Brushes.Transparent;
						} else{
							BarBrush = Brushes.DodgerBlue;
						}							
					trend[0] = 1;
				}
				else
				if (myDataSeries[0] <= -70)
				{
					CandleOutlineBrush = Brushes.Crimson;
					if(Open[0]<Close[0]  ) {
							BarBrush = Brushes.Transparent;
						} else{
							BarBrush = Brushes.Red;
						}	
					trend[0] = -1;
				}
				else
					{
					CandleOutlineBrush = Brushes.ForestGreen;
					if(Open[0]<Close[0]  ) {
							BarBrush = Brushes.Transparent;
						} else{
							BarBrush = Brushes.Lime;
						}	
					trend[0] = 0;
				
				}
				
				UpperLTF = AblesysT2(ATR, Period, Risk).Upper[0];
				LowerLTF = AblesysT2(ATR, Period, Risk).Lower[0];
				
				if ( LowerLTF != 0 )// short signal
					{
						ATRTrailingDn[0] = LowerLTF;
						LTFstate = 0;
						ATRTrailingUp.Reset ();
					}	
				if ( UpperLTF != 0 )
					{
						ATRTrailingUp[0]  = UpperLTF;	// Long signal
						LTFstate = 1;	
					}
					
				// Show MTF Confluence
				if( colorBarsAgreement ) {
					BarBrush = Brushes.Lime;
					CandleOutlineBrush = Brushes.Lime;
					if (Close[0] > Open[0]) {
						BarBrush = Brushes.Transparent;
					}
				}
				
			#endregion
					
			#region Short Entry
					if( HTFstate == 0 && LTFstate == 0 )	// short signal
						{
							if( colorBarsAgreement ) {
								BarBrush = Brushes.Red;
								CandleOutlineBrush = Brushes.Red;
								if (Close[0] > Open[0]) {
									BarBrush = Brushes.Transparent;
								}
							}

							/// show entry near 10 sma
							if (enterNearMA) {
								shortFilter = maPullback( Bullish: false,  BarDir: barDir);  // this one true
							}
							/// show entry near ma
							if (enterT2) {	
								shortFilter = enterNearT2(Bullish: false);
								Print("Inside Enter T2");
							}
							/// show entry with T1
							if( enterT1 ) {
								shortFilter = enterWithT1(Bullish: false, BarDir: barDir);
							}
							
							
							if (LowerLTF != 0 && shortFilter ) {
								if (showTradesChart){
									ArrowDown myArrowDn = Draw.ArrowDown(this, "Sell"+CurrentBar.ToString(), true, Time[0], High[0] , Brushes.Red);
									myArrowDn.OutlineBrush = Brushes.Black; }
								marketPosition = -1;
								entryPrice = Close[0];
								numTrades += 1;
								entryBarNumber = CurrentBar;
								signals[0] = -1;
								// set stop + target
								origStop = LowerLTF;
								origTarget = Close[0] -( origStop - Close[0]) ;
								
								// set stop + target
								origStop = LowerLTF;
								stopDistance = ( origStop - Close[0]);
								// min target
								if (stopDistance < minStopDistance) {
									stopDistance = minStopDistance;
								}
								origTarget = Close[0] - stopDistance ;
								
							}
						}
			#endregion
							
			#region Long Entry		
					///----- Long signal	-----
					if( HTFstate == 1 && LTFstate == 1 )	
						{
							if( colorBarsAgreement ) {
								BarBrush = Brushes.DodgerBlue;
								CandleOutlineBrush = Brushes.DodgerBlue;
								if (Close[0] > Open[0]) {
									BarBrush = Brushes.Transparent;
								}
							}
							
							/// show entry near 10 sma
							if (enterNearMA) {
								longFilter = maPullback( Bullish: true,  BarDir: barDir); 
							}
							/// show entry near ma
							if (enterT2) {
								longFilter = enterNearT2(Bullish: true); 
							}
							/// show entry with T1
							if( enterT1 ) {
								longFilter = enterWithT1(Bullish: true, BarDir: barDir);
							}
							
							if ( longFilter ) {
								if (showTradesChart){
									ArrowUp myArrowUp = Draw.ArrowUp(this, "Buy"+CurrentBar.ToString(), true, Time[0], Low[0], Brushes.DodgerBlue);
									myArrowUp.OutlineBrush = Brushes.DodgerBlue;}
								marketPosition = 1;
								entryPrice = Close[0];
								numTrades += 1;
								entryBarNumber = CurrentBar;
								signals[0] = 1;
								// set stop + target
								origStop = UpperLTF;
								stopDistance = (Close[0] - origStop);
								// min target
								if (stopDistance < minStopDistance) {
									stopDistance = minStopDistance;
								}
								
								origTarget = Close[0] + stopDistance ;
							}
						}
			#endregion
						
			#region Long Exits
					// show orig stop and tgt
					if ( showTradesChart && marketPosition != 0 && useAtrTraget) {
						Draw.Text(this, "stop"+CurrentBar.ToString(), "-", 0,origStop, Brushes.Red);
						Draw.Text(this, "tgt" + CurrentBar.ToString(), "-", 0, origTarget, Brushes.Green);
					}	
					
					//---- show LX at close < atr stop[1]
					if ( useAtrStop && marketPosition == 1 && Close[0] < AblesysT2(ATR, Period, Risk).Upper[1] && CurrentBar > entryBarNumber ) {
						marketPosition = 0;
						if (showTradesChart){
							Dot myDot = Draw.Dot(this, "LX" + CurrentBar.ToString(), true, 0, Low[0] - TickSize, Brushes.Blue);}
						exitPrice = Close[0];
						exitBarNumber = CurrentBar;
						tradeProfitLoss = profitLossCalc( longEntry: true);
						RecordResults(onChart: showTradesChart,  showLog: showTradesLog);
	signals[0] = 2;
					}
					
					// useAtrTraget Long
					if (useAtrTraget) {
						if ( marketPosition == 1  && CurrentBar > entryBarNumber){
							
							// target hit
							if (High[0] >= origTarget) {
								if (showTradesChart){
									Dot myDot = Draw.Dot(this, "LX" + CurrentBar.ToString(), true, 0, origTarget, Brushes.DodgerBlue);}
								exitPrice = origTarget;	
								marketPosition = 0;
								exitBarNumber = CurrentBar;
								tradeProfitLoss = profitLossCalc( longEntry: true);
								signals[0] = 2;
							}
							
							// stop hit
							if(Close[0] < origStop ) {
								if (showTradesChart){
									Dot myDot = Draw.Dot(this, "LX" + CurrentBar.ToString(), true, 0, origStop, Brushes.DodgerBlue);}
								exitPrice = Close[0];	
								marketPosition = 0;
								exitBarNumber = CurrentBar;
								tradeProfitLoss = profitLossCalc( longEntry: true);
								signals[0] = 2;
							}
							RecordResults(onChart: showTradesChart,  showLog: showTradesLog);
						}
					}
					
					
			#endregion
					
			#region Short Exits		
					// show SX at close > atr stop[1]
					if (  useAtrStop && marketPosition == -1 && Close[0] > AblesysT2(ATR, Period, Risk).Lower[1]  && CurrentBar > entryBarNumber ) {
						marketPosition = 0;
						if (showTradesChart){
							Dot myDot = Draw.Dot(this, "SX" + CurrentBar.ToString(), true, 0, High[0] + TickSize, Brushes.Red);}
						exitPrice = Close[0];
						tradeProfitLoss = profitLossCalc( longEntry: false);
						RecordResults(onChart: showTradesChart,  showLog: showTradesLog);
						exitBarNumber = CurrentBar;
	signals[0] = -2;
					}
					
					// useAtrTraget Short
					if (useAtrTraget && marketPosition == -1 && CurrentBar > entryBarNumber) {							
						// target hit
						if ( Low[0] <= origTarget ) {
							if (showTradesChart){
								Dot myDot = Draw.Dot(this, "SX" + CurrentBar.ToString(), true, 0, origTarget, Brushes.Red);}
							exitPrice = origTarget;	
							marketPosition = 0;
							exitBarNumber = CurrentBar;
							tradeProfitLoss = profitLossCalc( longEntry: false);
							signals[0] = -2;
						}
						
						// stop hit
						if(Close[0] > origStop ) {
							if (showTradesChart){
								Dot myDot = Draw.Dot(this, "SX" + CurrentBar.ToString(), true, 0, origStop, Brushes.Red);}
							exitPrice = Close[0];	
							marketPosition = 0;
							exitBarNumber = CurrentBar;
							tradeProfitLoss = profitLossCalc( longEntry: false);
							signals[0] = -2;
						}

						RecordResults(onChart: showTradesChart,  showLog: showTradesLog);
					}
				}
			#endregion
		
		}
		
		#region Functions
		

		
		public double profitLossCalc(bool longEntry) {
			
			double tradeResult;
			
			if (longEntry) {
				tradeResult = ( exitPrice - entryPrice ) * 10000;
			} else {
				tradeResult = (entryPrice - exitPrice ) * 10000;
			}

			return tradeResult;
		}
		
		public string PlotTrades(bool onChart, bool showLog){
		    string result = numTrades.ToString() + "  $ " + tradeProfitLoss.ToString("0") + " $" + cumProfitLoss.ToString("0");
			result = result + "\nWin " + numWin.ToString() + "  Loss " + numLoss.ToString() + "\n"+ pctWin.ToString("0.0") + "% "; 	
			result = result + profitFactor.ToString("0.00")+ " pf" ;
			result = result + "\n+"+ sumWinners.ToString("0") + " "+ sumLosers.ToString("0") ;
			if (showLog) {
				Print(" ");
				Print(result);
			}
			
			if (onChart) {
				Draw.Text(this, "perform" + CurrentBar.ToString(), performanceSummary, 0, MIN(Low, 20)[0] - (TickSize*400)  , ChartControl.Properties.ChartText);	
			}
			
		    return result;
		}
		
		public void RecordResults(bool onChart, bool showLog) {
			if (marketPosition == 0){

				cumProfitLoss =  cumProfitLoss + tradeProfitLoss;

				if(tradeProfitLoss > 0) { 
					sumWinners = sumWinners + tradeProfitLoss;
					numWin += 1;
					TextColor = BackGreen;
					if (onChart){
						drawTrades(entryBar: entryBarNumber, entryPrice: entryPrice,  win: true);}
					} else {
						numLoss +=1;
						sumLosers = sumLosers + tradeProfitLoss;
						TextColor = BackRed;
						if (onChart){
							drawTrades(entryBar: entryBarNumber, entryPrice: entryPrice,  win: false);}
					}
				pctWin = ( numWin / numTrades ) * 100;
				profitFactor = ( sumWinners / sumLosers ) * -1;
				performanceSummary = PlotTrades(onChart: onChart, showLog: showLog);	
			}
		}
		
		/// draw a line from entry to exit, green win, red loss
		public void drawTrades(int entryBar, double entryPrice,  bool win){
			
			int entryDistance = CurrentBar - entryBar;
			
			if(win) {
				Draw.Line(this, "tradeLine"+CurrentBar.ToString(), true, entryDistance, entryPrice, 0, Close[0], Brushes.LimeGreen, DashStyleHelper.Solid, 2);
			} else {
				Draw.Line(this, "tradeLine"+CurrentBar.ToString(), true, entryDistance, entryPrice, 0, Close[0], Brushes.Crimson, DashStyleHelper.Solid, 2);
			}
		}
		
		public bool maPullback(bool Bullish, bool BarDir) {
			
		 	bool signal = false;
			// Bull pullback to keltner
			if ( marketPosition != 1 && Bullish && Low[0] <= KeltnerChannel(0.5, 10).Upper[0] ) {
				signal = true;
				/// filter with bar direction too
				if (BarDir && Close[0] >= Open[0]) {
					signal = true;
				} 
				if (BarDir && Close[0] <= Open[0]) {
					signal = false;
				}
			} 
				
			if ( marketPosition != -1 && !Bullish && High[0] >= KeltnerChannel(0.5, 10).Lower[0] ) {
				signal = true;
				/// filter with bar direction too
				if (BarDir && Close[0] <= Open[0]) {
					signal = true;
				} 
				if (BarDir && Close[0] > Open[0]) {
						signal = false;
					}
			} 
			
			
				
			return signal;	
		}
		
		public bool enterNearT2(bool Bullish) {
			bool nearT2 = false;
			
			if (marketPosition != 1 && Bullish) {
				buyZone = UpperLTF +  ( TickSize * entrySpace);
							
				if (Low[0] <= buyZone )
					nearT2 = true;
			}
			
			if (marketPosition != -1 && !Bullish) {
				sellZone = LowerLTF - ( TickSize * entrySpace);
							
				if ( LowerLTF != 0 && High[0] >= sellZone )
					nearT2 = true;
			}
			
			return nearT2;
		}
		
		public bool  enterWithT1(bool Bullish, bool BarDir) {
			bool t1 = false;
			if (  marketPosition != 1 && Bullish && trend[0] == 1 ) {
				t1 = true;
				if (BarDir && Close[0] >= Open[0]) {
					t1 = true;
				} 
				if (BarDir && Close[0] <= Open[0]){
					t1 = false;	
				}
			}
			
			if (  marketPosition != -1 && !Bullish && trend[0] == -1 ) {
				t1 = true;
				if (BarDir && Close[0] <= Open[0]) {
					t1 = true;
				} 
				if (BarDir && Close[0] >= Open[0]) {
					t1 = false;	
				}
			}
			return t1;
		}
				
		#endregion



		#region Properties
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="ATR", Description="Number of ATR Multipliers", Order=1, GroupName="Parameters")]
		public int ATR
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Period", Order=2, GroupName="Parameters")]
		public int Period
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Risk", Description="Risk ranges from 1-10, default is 3", Order=3, GroupName="Parameters")]
		public int Risk
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="HTF Minutes", Description="munutesHtf", Order=4, GroupName="Parameters")]
		public int munutesHtf
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Enter Near MA", Description="enterNearMA", Order=5, GroupName="Parameters")]
		public bool enterNearMA
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Enter Near T2", Description="enterT2", Order=6, GroupName="Parameters")]
		public bool enterT2
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Use Bar Direction", Description="barDir", Order=7, GroupName="Parameters")]
		public bool barDir
		{ get; set; }
		[NinjaScriptProperty]
		[Display(Name="Enter With T1", Description="enterT1", Order=8, GroupName="Parameters")]
		public bool enterT1
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(0.0001, double.MaxValue)]
		[Display(Name="Min Stop Distance", Description="minStopDistance", Order=9, GroupName="Parameters")]
		public double minStopDistance
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Use Atr Trail Stop", Description="useAtrStop", Order=10, GroupName="Parameters")]
		public bool useAtrStop
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Use Atr Traget", Description="useAtrTraget", Order=11, GroupName="Parameters")]
		public bool useAtrTraget
		{ get; set; }
		
		 
		
		[NinjaScriptProperty]
		[Display(Name="Show Trades On Chart", Description="showTradesChart", Order=12, GroupName="Parameters")]
		public bool showTradesChart
		{ get; set; }
		
		
		
		
		[NinjaScriptProperty]
		[Display(Name="Show Trades On Log", Description="showTradesLog", Order=13, GroupName="Parameters")]
		public bool showTradesLog
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ATRTrailingUp
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ATRTrailingDn
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HTFup
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HTFdn
		{
			get { return Values[3]; }
		}
		
	
		[Browsable(false)]
		[XmlIgnore]
		public Series<int> Signals
		{
			get { return signals; }
		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AblesysMTFD[] cacheAblesysMTFD;
		public AblesysMTFD AblesysMTFD(int aTR, int period, int risk, int munutesHtf, bool enterNearMA, bool enterT2, bool barDir, bool enterT1, double minStopDistance, bool useAtrStop, bool useAtrTraget, bool showTradesChart, bool showTradesLog)
		{
			return AblesysMTFD(Input, aTR, period, risk, munutesHtf, enterNearMA, enterT2, barDir, enterT1, minStopDistance, useAtrStop, useAtrTraget, showTradesChart, showTradesLog);
		}

		public AblesysMTFD AblesysMTFD(ISeries<double> input, int aTR, int period, int risk, int munutesHtf, bool enterNearMA, bool enterT2, bool barDir, bool enterT1, double minStopDistance, bool useAtrStop, bool useAtrTraget, bool showTradesChart, bool showTradesLog)
		{
			if (cacheAblesysMTFD != null)
				for (int idx = 0; idx < cacheAblesysMTFD.Length; idx++)
					if (cacheAblesysMTFD[idx] != null && cacheAblesysMTFD[idx].ATR == aTR && cacheAblesysMTFD[idx].Period == period && cacheAblesysMTFD[idx].Risk == risk && cacheAblesysMTFD[idx].munutesHtf == munutesHtf && cacheAblesysMTFD[idx].enterNearMA == enterNearMA && cacheAblesysMTFD[idx].enterT2 == enterT2 && cacheAblesysMTFD[idx].barDir == barDir && cacheAblesysMTFD[idx].enterT1 == enterT1 && cacheAblesysMTFD[idx].minStopDistance == minStopDistance && cacheAblesysMTFD[idx].useAtrStop == useAtrStop && cacheAblesysMTFD[idx].useAtrTraget == useAtrTraget && cacheAblesysMTFD[idx].showTradesChart == showTradesChart && cacheAblesysMTFD[idx].showTradesLog == showTradesLog && cacheAblesysMTFD[idx].EqualsInput(input))
						return cacheAblesysMTFD[idx];
			return CacheIndicator<AblesysMTFD>(new AblesysMTFD(){ ATR = aTR, Period = period, Risk = risk, munutesHtf = munutesHtf, enterNearMA = enterNearMA, enterT2 = enterT2, barDir = barDir, enterT1 = enterT1, minStopDistance = minStopDistance, useAtrStop = useAtrStop, useAtrTraget = useAtrTraget, showTradesChart = showTradesChart, showTradesLog = showTradesLog }, input, ref cacheAblesysMTFD);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AblesysMTFD AblesysMTFD(int aTR, int period, int risk, int munutesHtf, bool enterNearMA, bool enterT2, bool barDir, bool enterT1, double minStopDistance, bool useAtrStop, bool useAtrTraget, bool showTradesChart, bool showTradesLog)
		{
			return indicator.AblesysMTFD(Input, aTR, period, risk, munutesHtf, enterNearMA, enterT2, barDir, enterT1, minStopDistance, useAtrStop, useAtrTraget, showTradesChart, showTradesLog);
		}

		public Indicators.AblesysMTFD AblesysMTFD(ISeries<double> input , int aTR, int period, int risk, int munutesHtf, bool enterNearMA, bool enterT2, bool barDir, bool enterT1, double minStopDistance, bool useAtrStop, bool useAtrTraget, bool showTradesChart, bool showTradesLog)
		{
			return indicator.AblesysMTFD(input, aTR, period, risk, munutesHtf, enterNearMA, enterT2, barDir, enterT1, minStopDistance, useAtrStop, useAtrTraget, showTradesChart, showTradesLog);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AblesysMTFD AblesysMTFD(int aTR, int period, int risk, int munutesHtf, bool enterNearMA, bool enterT2, bool barDir, bool enterT1, double minStopDistance, bool useAtrStop, bool useAtrTraget, bool showTradesChart, bool showTradesLog)
		{
			return indicator.AblesysMTFD(Input, aTR, period, risk, munutesHtf, enterNearMA, enterT2, barDir, enterT1, minStopDistance, useAtrStop, useAtrTraget, showTradesChart, showTradesLog);
		}

		public Indicators.AblesysMTFD AblesysMTFD(ISeries<double> input , int aTR, int period, int risk, int munutesHtf, bool enterNearMA, bool enterT2, bool barDir, bool enterT1, double minStopDistance, bool useAtrStop, bool useAtrTraget, bool showTradesChart, bool showTradesLog)
		{
			return indicator.AblesysMTFD(input, aTR, period, risk, munutesHtf, enterNearMA, enterT2, barDir, enterT1, minStopDistance, useAtrStop, useAtrTraget, showTradesChart, showTradesLog);
		}
	}
}

#endregion
