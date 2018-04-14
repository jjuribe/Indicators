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
	public class BetterProAmDot : Indicator
	{
		private Series<double> Var1;
		
		//public double	Var1 = 0;
		public int 		Var2 = 20;
		public double 	Var3 = 65280; 
		public bool 	Var4 = false;
		public bool 	Var5 = false; 
		public bool 	Var6 = false; 
		public bool 	Var7 = false;
		public int 		downtick = 0;
		public bool 	proColor = false;
		public bool 	amColor = false;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Better ProAm Dot";
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
				ProAlert					= true;
				AMAlert					= true;
			}
			else if (State == State.Configure)
			{
			}
			else if(State == State.DataLoaded)
			  {
			    //clear the output window as soon as the bars data is loaded
			    ClearOutputWindow();  	
				Var1 = new Series<double>(this, MaximumBarsLookBack.Infinite);

			  }
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 20) {
				return;
			}
			
			if(BarsPeriod.BarsPeriodType == BarsPeriodType.Minute || BarsPeriod.BarsPeriodType == BarsPeriodType.Tick ) {
				if (Close[0] > Close[1])
					downtick++;
				Var1[0] = Volume[0] + downtick;
			} else {
				Var1[0] = Volume[0];
			}
//			Draw.Text(this, "vol"+CurrentBar, Var1[0].ToString(), 0, Low[0] - (TickSize * 2), Brushes.AntiqueWhite);
//			Draw.Text(this, "Volume[0]"+CurrentBar, Volume[0].ToString(), 0, High[0] + (TickSize * 2), Brushes.AntiqueWhite);
			//if Highest (Var1, Var2) <> 0 then
			//Var4 = Var1 = NthMaxList (1, Var1, Var1[1], Var1[2], Var1[3], Var1[4], Var1[5], Var1[6], Var1[7], Var1[8], Var1[9], Var1[10], Var1[11], Var1[12], Var1[13], Var1[14], Var1[15], Var1[16], Var1[17], Var1[18], Var1[19]) ;
			if( MAX(Var1, Var2)[0] != 0 ) {
				
				if ( Var1[0] == MAX(Var1, 19)[0] ) {
					Var4 = true;
				} else {
					Var4 = false;
				}
			
				//Print(Var4);
				//Var5 = Var1 = NthMaxList (2, Var1, Var1[1], Var1[2], Var1[3], Var1[4], Var1[5], Var1[6], Var1[7], Var1[8], Var1[9], Var1[10], Var1[11], Var1[12], Var1[13], Var1[14], Var1[15], Var1[16], Var1[17], Var1[18], Var1[19]) ;
				List<double> ListOfNums = new List<double> {Var1[0], Var1[1], Var1[2], Var1[3], Var1[4], Var1[5], Var1[6], Var1[7], Var1[8], Var1[9], Var1[10], Var1[11], Var1[12], Var1[13], Var1[14], Var1[15], Var1[16], Var1[17], Var1[18], Var1[19]};
				var secondMax = ListOfNums.OrderByDescending(r => r).Skip(1).FirstOrDefault();
		
				if ( Var1[0] == secondMax ) {
						Var5 = true;
					} else {
						Var5 = false;
					}
				//Print(secondMax); Print(Var5);	
				//Var6 = Var1 = NthMinList (1, Var1, Var1[1], Var1[2], Var1[3], Var1[4], Var1[5], Var1[6], Var1[7], Var1[8], Var1[9], Var1[10], Var1[11], Var1[12], Var1[13], Var1[14], Var1[15], Var1[16], Var1[17], Var1[18], Var1[19]) ;
				if ( Var1[0] == MIN(Var1, 19)[0] ) {
					Var6 = true;
				} else {
					Var6 = false;
				}
	  			// Var7 = Var1 = NthMinList (2, Var1, Var1[1], Var1[2], Var1[3], Var1[4], Var1[5], Var1[6], Var1[7], Var1[8], Var1[9], Var1[10], Var1[11], Var1[12], Var1[13], Var1[14], Var1[15], Var1[16], Var1[17], Var1[18], Var1[19]) ;
				var secondMin = ListOfNums.OrderByDescending(r => r).Skip(1).LastOrDefault();
				if ( Var1[0] == secondMin ) {
						Var7 = true;
					} else {
						Var7 = false;
					}
				//if Var4 OR Var5 then Var3 = ProColor ;
					if (  Var4 || Var5 ) {
						proColor = true;
					} else {
						proColor = false;
					}
   				//if Var6 OR Var7 then Var3 = AmColor ;
					if (  Var6 || Var7 ) {
						amColor = true;
					} else {
						amColor = false;
					}
					
//				BarBrush = Brushes.DarkGreen;
//				CandleOutlineBrush = Brushes.DarkGreen;
					
				if ( ProAlert && proColor ) {
					if ( Close[0] < Open[0] ) {
						Dot myDot = Draw.Dot(this, "pro" + CurrentBar, true, 0, High[0] + TickSize * 100, Brushes.DodgerBlue);	
					} else {
						Dot myDot = Draw.Dot(this, "pro" + CurrentBar, true, 0, Low[0] - TickSize * 100, Brushes.DodgerBlue);	
					}
				}
				
				if ( AMAlert && amColor ) {
					if ( Close[0] < Open[0] ) {
						Dot myDot = Draw.Dot(this, "pro" + CurrentBar, true, 0, High[0] + TickSize * 100, Brushes.Gold);	
					} else {
						Dot myDot = Draw.Dot(this, "pro" + CurrentBar, true, 0, Low[0] - TickSize * 100, Brushes.Gold);	
					}
				}
//				if ( amColor ) {
//					BarBrush = Brushes.Gold;
//					CandleOutlineBrush = Brushes.Gold;	
//				}
			}
		}
		


		#region Properties
		[NinjaScriptProperty]
		[Display(Name="ProAlert", Order=1, GroupName="Parameters")]
		public bool ProAlert
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="AMAlert", Order=2, GroupName="Parameters")]
		public bool AMAlert
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private BetterProAmDot[] cacheBetterProAmDot;
		public BetterProAmDot BetterProAmDot(bool proAlert, bool aMAlert)
		{
			return BetterProAmDot(Input, proAlert, aMAlert);
		}

		public BetterProAmDot BetterProAmDot(ISeries<double> input, bool proAlert, bool aMAlert)
		{
			if (cacheBetterProAmDot != null)
				for (int idx = 0; idx < cacheBetterProAmDot.Length; idx++)
					if (cacheBetterProAmDot[idx] != null && cacheBetterProAmDot[idx].ProAlert == proAlert && cacheBetterProAmDot[idx].AMAlert == aMAlert && cacheBetterProAmDot[idx].EqualsInput(input))
						return cacheBetterProAmDot[idx];
			return CacheIndicator<BetterProAmDot>(new BetterProAmDot(){ ProAlert = proAlert, AMAlert = aMAlert }, input, ref cacheBetterProAmDot);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.BetterProAmDot BetterProAmDot(bool proAlert, bool aMAlert)
		{
			return indicator.BetterProAmDot(Input, proAlert, aMAlert);
		}

		public Indicators.BetterProAmDot BetterProAmDot(ISeries<double> input , bool proAlert, bool aMAlert)
		{
			return indicator.BetterProAmDot(input, proAlert, aMAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.BetterProAmDot BetterProAmDot(bool proAlert, bool aMAlert)
		{
			return indicator.BetterProAmDot(Input, proAlert, aMAlert);
		}

		public Indicators.BetterProAmDot BetterProAmDot(ISeries<double> input , bool proAlert, bool aMAlert)
		{
			return indicator.BetterProAmDot(input, proAlert, aMAlert);
		}
	}
}

#endregion
