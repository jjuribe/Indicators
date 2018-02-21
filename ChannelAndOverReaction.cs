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
	public class ChannelAndOverReaction : Indicator
	{
		public double 	entryPrice;
		public int 		entryBar;
		public int 		entryBar2;
		public bool 	entryOne	 	= false;
		public bool 	entryTwo 		= false;
		/// short
		public int		entryBar2short 	= 1;
		public bool 	entryTwoShort 	= true;
		
		
		///  money management
		public double 	shares;
		//public int  	portfolioSize	= 826000;
		//public int		numSystems		= 10;
		private int 	initialBalance;
		private	int 	cashAvailiable;
		private	double 	priorTradesCumProfit;
		private	int 	priorTradesCount;
		private	double 	sharesFraction;
		
		/// stops
		private double 	stopLine;
		private double 	trailStop = 0.0;
		private double  lossInPonts;
		private bool	autoStop = true;
		private double  entryBarnum;
		private double  stopDistance;
		
		///  indicators
		private MarketCondition MarketCondition1;
		private SMA		sma0;
		
		///  reporting
		private double gainInPoints; 
		private	double riskInLastTrade; 
		private	double rValue; 
		private	double totalR; 
		public string 	ComputerName	= "MBP";
		public string textForBox;
		public string textForBoxToAdd;
		public string entryType;
		
		Gui.Chart.Chart myChart;
		Gui.Chart.ChartTrader chartTrader;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Channel + OverReaction";
				Calculate									= Calculate.OnEachTick;
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
				
				MaxRisk					= 100;
				Pct						= 3;
				On = true;
			}
			else if (State == State.Configure)
			{
				MarketCondition1 = MarketCondition(showBands: false, showVolatilityText: false);
			}
			else if (State == State.DataLoaded)
			{
				sma0				= SMA(200);
				
				ClearOutputWindow(); 
			}
		}

		protected override void OnBarUpdate()
		{
			if (!On) {   return; }
			if (CurrentBar < 20 ) { return; }
			//setTextBox( textInBox: "No Entry");
			textForBox = "\n\tNo Channel Entry\t\t\n";
			textForBoxToAdd = "\n\tNo Overreaction Entry\t\t\n";
			bool chLong = entryConditionsChannel();
			bool orLong = entryConditionsORlong();
			bool orShort = entryConditionsORshort();
			
			///  Add Target Rules to Indicator
			///  Add On / Off
			
			textForBox = textForBox + textForBoxToAdd;
			setTextBox( textInBox: textForBox);
			
			if (Low[0] > Low[1]) {
				double newStop =  Low[0] - stopDistance;
				if (newStop > trailStop) {
					trailStop = newStop;
				}
			}
			Draw.TriangleUp(this,"trailStop"+CurrentBar.ToString(), false, 0, trailStop, Brushes.DimGray);
			
			
		}
		
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									POSTION SIZE
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
	
		protected int calcPositionSize(double stopPrice, bool isLong) {
			if (isLong) {
				sharesFraction = MaxRisk / ( Close[0] -stopPrice );
			} else {
				sharesFraction = MaxRisk / ( stopPrice - Close[0] );
			}
			//Print(sharesFraction);
			return (int)sharesFraction;
		}
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									STOP PRICE
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////		
		protected double calcInitialStop(int pct, bool isLong) {
			double result;
			/// set stop 3% if Nasdaq 5%
			foreach(Exchange exchange in Bars.Instrument.MasterInstrument.Exchanges)
				{
					if (exchange.ToString() != "Default") {
						//Print(exchange); // Default, Nasdaq, NYSE
						if (exchange.ToString() == "Nasdaq") {
							Print("\tListed on Nasdaq, Change stiop to 5%");
							pct = 5;
						}						
					}
				} 
			double convertedPct = pct * 0.01;

			if (isLong) {
				stopDistance =  Close[0] * convertedPct;
				result = Close[0] - stopDistance;
				drawStopTarget(stop: result );
				///sendSharesToCT(stops: result, targets: 22.2 );
				trailStop = result;
				entryBarnum = CurrentBar;
			} else {
				result = Close[0] + ( Close[0] * convertedPct);
			}
			
			
			
			return result; 
		}
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 							Send Shares to Chart Trader
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		protected void sendSharesToCT(double stops, double targets)
		{
			//find chart trader from myChart's Chart Control by its Automation ID: "ChartWindowChartTrader"
			  chartTrader = Window.GetWindow(myChart.ActiveChartControl.Parent).FindFirst("ChartWindowChartTraderControl") as Gui.Chart.ChartTrader;
			 //chartTrader.SetCurrentValue(order
			  if (chartTrader == null)
			  {
			      return;
			  }
			/// Here is an example of changing a stop:
			///AtmStrategyChangeStopTarget(0, stops, "Stop1", atmStrategyId);
			/// Here is an example of changing a profit target:
			///AtmStrategyChangeStopTarget(targets, 0, "Target1", atmStrategyId);
			
		}
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 							Show Stop Target Size On Chart
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		protected void drawStopTarget(double stop)
		{
			double result = stop;
			Draw.RiskReward(this, "MyRiskReward", true, 0, Close[0], 10, result, 0.5, true);
			double target2 =  MAX(SMA(10), 20)[0];
			double sharesText = result - (Instrument.MasterInstrument.PointValue * 0.3);
			sharesText = MIN(Low, 20)[0];
			Draw.Line(this, "target2", false, 0, target2, -10, target2, Brushes.Silver, DashStyleHelper.Dash, 3);
			Draw.Text(this, "shares", sharesFraction.ToString("0"), 0, sharesText, ChartControl.Properties.ChartText);
		}
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Channel Long
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////		
		protected bool entryConditionsChannel()
		{
			bool signal = false;		// && High[0] < SMA(10)[0] 
			if ( Close[0] > Math.Abs(sma0[0]) && WilliamsR(10)[0] < -80 ){
				//signal = true;
				Draw.Dot(this, "CH"+CurrentBar, true, 0, Low[0] - (TickSize * 20), Brushes.DarkGreen);
				
				double theStop = calcInitialStop(pct: Pct, isLong: true);
				shares = calcPositionSize(stopPrice: theStop, isLong: true); 
				entryType = "Channel";
				textForBox = popuateStatsTextBox( entryType: entryType, shares: shares, maxLoss: MaxRisk , stopPrice: theStop);
				Print(Time[0].ToShortDateString() +"\n"+ textForBox);
				Draw.Text(this, "stop"+CurrentBar, "-", 0, theStop);
			}
			
			return signal;
		}
		
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Overreaction Long
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		protected bool entryConditionsORlong()
		{
			bool signal = false;
			double onePercent = Close[0] * 0.01;
			if ((Close[0] > Math.Abs(sma0[0])  && High[0] < SMA(10)[0] && Close[0] < ( SMA(10)[0]- ATR(14)[1])) ||
				(Close[0] > Math.Abs(sma0[0])  && High[0] < SMA(10)[0] && Close[0] < ( SMA(10)[0]- onePercent )) ){
				signal = true;
				Draw.Dot(this, "ORl"+CurrentBar, true, 0, Low[0] - (TickSize * 60), Brushes.Cyan);
					
				double theStop = calcInitialStop(pct: Pct, isLong: true);
				shares = calcPositionSize(stopPrice: theStop, isLong: true); 
				
				textForBoxToAdd = popuateStatsTextBox( entryType: "Overreaction Long", shares: shares, maxLoss: MaxRisk , stopPrice: theStop);
				Print(Time[0].ToShortDateString() +"\n"+ textForBox);
				Draw.Text(this, "stop"+CurrentBar, "-", 0, theStop);
			}
			return signal;
		}
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Overreaction Short
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		protected bool entryConditionsORshort()
		{
			bool signal = false;
			double onePercent = Close[0] * 0.01;
			if ((Close[0] < Math.Abs(sma0[0])  && Low[0] > SMA(10)[0] && Close[0] > ( SMA(10)[0]+ ATR(14)[1])) ||
				(Close[0] < Math.Abs(sma0[0])  && Low[0] > SMA(10)[0] && Close[0] > ( SMA(10)[0]+ onePercent )) ){
				signal = true;
				Draw.Dot(this, "ORs"+CurrentBar, true, 0, High[0] + (TickSize * 60), Brushes.Magenta);
					
				double theStop = calcInitialStop(pct: Pct, isLong: false);
				shares = calcPositionSize(stopPrice: theStop, isLong: false); 
				textForBox = popuateStatsTextBox( entryType: "Overreaction Short", shares: shares, maxLoss: MaxRisk , stopPrice: theStop);
				Print(Time[0].ToShortDateString() +"\n"+ textForBox);
				Draw.Text(this, "stop"+CurrentBar, "-", 0, theStop);
			}
			return signal;
		}
		/// ////////////////////////////////////////////////////////////////////////////////////////////////
		/// 	
		/// 									Text Box
		/// 
		/// ////////////////////////////////////////////////////////////////////////////////////////////////		
		protected string popuateStatsTextBox(string entryType, double shares, int maxLoss, double stopPrice) {

			string bodyMessage = "\n\t";
			
			bodyMessage = bodyMessage + Time[0].ToShortDateString()+"\t\n\t";
			bodyMessage = bodyMessage + entryType+"\t\n\tEnter Tomorrow's Open\n\t";
			bodyMessage = bodyMessage + shares+" shares\t\n\t";
			
			bodyMessage = bodyMessage + Pct+"% stop\t";
			
			bodyMessage = bodyMessage + "$"+maxLoss+" risk\t\n\t";
			bodyMessage = bodyMessage + stopPrice.ToString("0.00")+" stop order\t\n";
			if (entryType == "Channel" ) {
				bodyMessage = bodyMessage + "\tTarget: %R > -30\t\n";
			} else {
				bodyMessage = bodyMessage + "\tTarget:  > SMA(10)\t\n";
			}
			
			
			
			return bodyMessage;
		}	
		
		protected void setTextBox(string textInBox)
		{
			/// show market condition
			TextFixed myTF = Draw.TextFixed(this, "tradeStat", textInBox, TextPosition.BottomLeft);
			myTF.TextPosition = TextPosition.TopLeft;
			myTF.AreaBrush = Brushes.Black;
			myTF.AreaOpacity = 100;
			myTF.TextBrush = Brushes.LightGray;
		}
		
	
		#region Properties
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Max Risk", Order=2, GroupName="Parameters")]
		public int MaxRisk
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(ResourceType = typeof(Custom.Resource), Name="Percent Stop", Order=3, GroupName="Parameters")]
		public int Pct
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="Indicator On", Description="is this thing on?", Order=1, GroupName="Parameters")]
		public bool On
		{ get; set; }

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ChannelAndOverReaction[] cacheChannelAndOverReaction;
		public ChannelAndOverReaction ChannelAndOverReaction(int maxRisk, int pct, bool on)
		{
			return ChannelAndOverReaction(Input, maxRisk, pct, on);
		}

		public ChannelAndOverReaction ChannelAndOverReaction(ISeries<double> input, int maxRisk, int pct, bool on)
		{
			if (cacheChannelAndOverReaction != null)
				for (int idx = 0; idx < cacheChannelAndOverReaction.Length; idx++)
					if (cacheChannelAndOverReaction[idx] != null && cacheChannelAndOverReaction[idx].MaxRisk == maxRisk && cacheChannelAndOverReaction[idx].Pct == pct && cacheChannelAndOverReaction[idx].On == on && cacheChannelAndOverReaction[idx].EqualsInput(input))
						return cacheChannelAndOverReaction[idx];
			return CacheIndicator<ChannelAndOverReaction>(new ChannelAndOverReaction(){ MaxRisk = maxRisk, Pct = pct, On = on }, input, ref cacheChannelAndOverReaction);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ChannelAndOverReaction ChannelAndOverReaction(int maxRisk, int pct, bool on)
		{
			return indicator.ChannelAndOverReaction(Input, maxRisk, pct, on);
		}

		public Indicators.ChannelAndOverReaction ChannelAndOverReaction(ISeries<double> input , int maxRisk, int pct, bool on)
		{
			return indicator.ChannelAndOverReaction(input, maxRisk, pct, on);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ChannelAndOverReaction ChannelAndOverReaction(int maxRisk, int pct, bool on)
		{
			return indicator.ChannelAndOverReaction(Input, maxRisk, pct, on);
		}

		public Indicators.ChannelAndOverReaction ChannelAndOverReaction(ISeries<double> input , int maxRisk, int pct, bool on)
		{
			return indicator.ChannelAndOverReaction(input, maxRisk, pct, on);
		}
	}
}

#endregion
