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
	public class PriorDayandNight : Indicator
	{
		
	 	private int 	startTime 	= 930; 
        private int	 	endTime 	= 1600;
		private int	 	IBendTime 	= 1030;
		private int		ninja_Start_Time;
		private int		ninja_End_Time;
		private int		ninja_IB_End_Time;
		private int		rthCounter;
		private int		counter;
		private int		openingBarNum;
		private int		latestRTHbar;
		private int		lastBar;
	
		private double	gxHigh;
		private double	gxLow;
		private double	gxMid;
		private int 	gxMidBarsAgo;
		private int		lastGxBar;
		private int 	openingGxBarNum;
	
		private int 	ibCounter;
		private int 	ibMidBarsAgo;
		private double	ibHigh;
		private double	ibLow;
		private double	ibMid;
		
		private double	open_D;
	
		private double	yHigh;
		private double	yLow;
		private double	yMid;
	
		private double	todaysHigh;
		private double	todaysLow;
		private double	todaysClose;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "PriorDayandNight";
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
				RTHopen					= 1;
				RTHclose					= 1;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar < 20 )
				{
					return;
				}
				
			lastBar = CurrentBar - 1;
				
			// Cnnvert Military Time to Ninja Time
			ninja_Start_Time = startTime * 100;
			ninja_End_Time = endTime * 100;
			ninja_IB_End_Time = IBendTime * 100;
				
			// find globex range
            if (ToTime(Time[0]) <= ninja_Start_Time || ToTime(Time[0]) >= ninja_End_Time )  {
					counter = counter + 1;
					gxHigh = MAX(High, counter)[0];
					gxLow = MIN(Low, counter)[0];
					gxMid = ( ( gxHigh - gxLow ) /2  )+ gxLow;
					gxMidBarsAgo = counter / 2;
				
				}
			// Session Start
			if (ToTime(Time[1])<= ninja_Start_Time && ToTime(Time[0]) >= ninja_Start_Time) {
					counter = 0;
					rthCounter = 0;
					ibCounter = 0;
					open_D = Open[0];
					openingBarNum = CurrentBar;
				}
			// Find RTH Range
			if (ToTime(Time[0]) >= ninja_Start_Time && ToTime(Time[0]) < ninja_End_Time  ) {
					rthCounter = rthCounter + 1;
					todaysHigh = MAX(High, rthCounter)[0];
					todaysLow = MIN(Low, rthCounter)[0];				
				}
			
			// Find IB Range
			if (ToTime(Time[0]) >= ninja_Start_Time && ToTime(Time[0]) < ninja_IB_End_Time  ) {
					ibCounter = ibCounter + 1;
					ibHigh = MAX(High, ibCounter)[0];
					ibLow = MIN(Low, ibCounter)[0];	
					ibMid = ( ( ibHigh - ibLow ) /2  )+ ibLow;
				if ( ibCounter > 2 ) {
					ibMidBarsAgo = ibCounter / 2;
				} else {
					ibMidBarsAgo = ibCounter;
				}
					
				}
			
			
			// Session End
			if (ToTime(Time[1])<= ninja_End_Time && ToTime(Time[0]) >= ninja_End_Time) { 
					yHigh = todaysHigh;
					yLow = todaysLow;
					todaysClose = Close[0];
					yMid = ( ( yHigh - yLow ) /2  )+ yLow;
					openingGxBarNum = CurrentBar;
				}
				
			// show prior day Hi and Low
			// RTH
			if (ToTime(Time[0]) >= ninja_Start_Time && ToTime(Time[0]) < ninja_End_Time  ) {
					latestRTHbar = CurrentBar - openingBarNum;
					
				if( yLow != 0 && yMid != 0 && yHigh != 0) {
					RemoveDrawObject("RthBoxU"+lastBar);
					RemoveDrawObject("RthBoxL"+lastBar);
					Draw.Rectangle(this, "RthBoxU"+ CurrentBar.ToString(), true, latestRTHbar, yMid, 0, yHigh, Brushes.Transparent, Brushes.DarkBlue, 20);
					Draw.Rectangle(this, "RthBoxL"+ CurrentBar.ToString(), true, latestRTHbar, yLow, 0, yMid, Brushes.Transparent, Brushes.DarkRed, 20);
				}
					
				}
			
			// plot globex
            if (ToTime(Time[0]) <= ninja_Start_Time || ToTime(Time[0]) >= ninja_End_Time )  //  Find GX Hi/Lo
				{
					lastGxBar = CurrentBar - openingGxBarNum;
					RemoveDrawObject("GxBox"+lastBar);
					Draw.Rectangle(this, "GxBox"+ CurrentBar.ToString(), true, lastGxBar, gxLow, 0, gxHigh, Brushes.Goldenrod, Brushes.Goldenrod, 10);
				}
				
			// Draw arrow from GX mid to OR mid
				if (ToTime(Time[0]) >= ninja_Start_Time && ToTime(Time[0]) < ninja_IB_End_Time  ) {
					RemoveDrawObject("ibArrow"+lastBar);
					if ( ibMid > gxMid ) {
						Draw.ArrowLine(this, "ibArrow"+ CurrentBar.ToString(),  gxMidBarsAgo + rthCounter, gxMid, ibMidBarsAgo, ibMid, Brushes.DodgerBlue, DashStyleHelper.Solid, 6);
					} else {
						Draw.ArrowLine(this, "ibArrow"+ CurrentBar.ToString(),  gxMidBarsAgo + rthCounter, gxMid, ibMidBarsAgo, ibMid, Brushes.DarkRed, DashStyleHelper.Solid, 6);
					}
					
				}
				
				
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RTHopen", Order=1, GroupName="Parameters")]
		public int RTHopen
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="RTHclose", Order=2, GroupName="Parameters")]
		public int RTHclose
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PriorDayandNight[] cachePriorDayandNight;
		public PriorDayandNight PriorDayandNight(int rTHopen, int rTHclose)
		{
			return PriorDayandNight(Input, rTHopen, rTHclose);
		}

		public PriorDayandNight PriorDayandNight(ISeries<double> input, int rTHopen, int rTHclose)
		{
			if (cachePriorDayandNight != null)
				for (int idx = 0; idx < cachePriorDayandNight.Length; idx++)
					if (cachePriorDayandNight[idx] != null && cachePriorDayandNight[idx].RTHopen == rTHopen && cachePriorDayandNight[idx].RTHclose == rTHclose && cachePriorDayandNight[idx].EqualsInput(input))
						return cachePriorDayandNight[idx];
			return CacheIndicator<PriorDayandNight>(new PriorDayandNight(){ RTHopen = rTHopen, RTHclose = rTHclose }, input, ref cachePriorDayandNight);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PriorDayandNight PriorDayandNight(int rTHopen, int rTHclose)
		{
			return indicator.PriorDayandNight(Input, rTHopen, rTHclose);
		}

		public Indicators.PriorDayandNight PriorDayandNight(ISeries<double> input , int rTHopen, int rTHclose)
		{
			return indicator.PriorDayandNight(input, rTHopen, rTHclose);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PriorDayandNight PriorDayandNight(int rTHopen, int rTHclose)
		{
			return indicator.PriorDayandNight(Input, rTHopen, rTHclose);
		}

		public Indicators.PriorDayandNight PriorDayandNight(ISeries<double> input , int rTHopen, int rTHclose)
		{
			return indicator.PriorDayandNight(input, rTHopen, rTHclose);
		}
	}
}

#endregion
