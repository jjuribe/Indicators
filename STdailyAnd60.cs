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
	public class STdailyAnd60 : Indicator	
	{
		// time series for bands
		private double DailSTdouble;
		private double HourSTdouble;
		private double MinsSTdouble;
		
		private bool dailyTrendUp;
		private bool hourlyTrendUp;
		private bool minuteTrendUp;
		
		private int longSignalCounter;
		private int shortSignalCounter;
		private int noSignalCounter;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "STdailyAnd60";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				DailyCalc					= true;
				HourlyCalc					= true;
				AddPlot(new Stroke(Brushes.Firebrick, 2), PlotStyle.Dot, "DailyST");
				AddPlot(new Stroke(Brushes.DarkRed, 2), PlotStyle.Dot, "HourlyST");
				AddPlot(new Stroke(Brushes.DarkRed, 1), PlotStyle.Hash, "MinuteST");
			}
			else if (State == State.Configure)
			{
				AddDataSeries(Data.BarsPeriodType.Day, 1);
				AddDataSeries(Data.BarsPeriodType.Minute, 60);
			}
			
		}

		protected override void OnBarUpdate()
		{
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
			// HTF bars hour
			if (BarsInProgress == 2)
			{
				HourSTdouble = TSSuperTrend(SuperTrendMode.ATR, MovingAverageType.HMA, 14, 2.2, 14, true, false, false, false)[0];
				return;
			}
			
			// HTF bars Day
			if (BarsInProgress == 1)
			{
				DailSTdouble	= TSSuperTrend(SuperTrendMode.ATR, MovingAverageType.HMA, 14, 2.2, 14, true, false, false, false)[0];
				return;
			}
			
			// lower time frame bars
			if (BarsInProgress == 0)
			{
				 DailyST[0] = DailSTdouble;
				 if (Close[0] > DailSTdouble) {
					 PlotBrushes[0][0] = Brushes.DodgerBlue;
					 dailyTrendUp = true;
				 }
				 else {
					 PlotBrushes[0][0] = Brushes.Red;
					 dailyTrendUp = false;
				 }
				 
				 HourlyST[0] = HourSTdouble;
				 if (Close[0] > HourSTdouble) {
					 PlotBrushes[1][0] = Brushes.DodgerBlue;
					 hourlyTrendUp = true;
				 }
				 else {
					 PlotBrushes[1][0] = Brushes.Red;
					 hourlyTrendUp = false;
				 }
				 
				 MinsSTdouble = TSSuperTrend(SuperTrendMode.ATR, MovingAverageType.HMA, 14, 2.2, 14, true, false, false, false)[0];
				 
				
				 MinuteST[0] = MinsSTdouble;
				 if (Close[0] > MinsSTdouble) {
					 PlotBrushes[2][0] = Brushes.DodgerBlue;
					 minuteTrendUp = true;
				 } else {
					 PlotBrushes[2][0] = Brushes.Red;
					 minuteTrendUp = false;
				 }
				 
				 /*
				 		private int longSignalCounter;
						private int shortSignalCounter;
						private int noSignalCounter;
				 */
				 
				  //// Long Signal
				 if (dailyTrendUp && hourlyTrendUp && minuteTrendUp) {
					  if ( DailyCalc )
						 BarBrush = Brushes.DodgerBlue;
					 // Send Alert
					 string messageTitle = "Trade Alert Long On " + Instrument.MasterInstrument.Name;
					 string messageBody = "On " + Time[0].ToShortDateString() + " at " + Time[0].ToShortTimeString() + " A Long Entry on " + Instrument.MasterInstrument.Name + " was generated at " + Close[0];
					 //Print(" "); Print(messageTitle); Print(messageBody); Print(" "); Print(" ");
					 if (State == State.Historical) 
    					return; 
					 Print( Time[0].ToShortTimeString()  + " LONG LOGIC -- Long Singal: " + longSignalCounter.ToString() + " Short Signal: " + shortSignalCounter.ToString() + "No Signal Counter: " + noSignalCounter.ToString());
					 if ( longSignalCounter < 4 )
					 Share("Hotmail", messageBody, new object[]{ "3103824522@tmomail.net", messageTitle, @"C:\Users\MBPtrader\Pictures\EURUSD_Opt_6_27.PNG"});
					 longSignalCounter = longSignalCounter +  1;
					 shortSignalCounter = 0;
					 noSignalCounter = 0;
					 
					  //// Short Signal	 
					 } else if (!dailyTrendUp && !hourlyTrendUp && !minuteTrendUp) {
						  if ( DailyCalc )
						 	BarBrush = Brushes.Red;
						 // Send Alert
						 string messageTitle = "Trade Alert Short On " + Instrument.MasterInstrument.Name;
						 string messageBody = "On " + Time[0].ToShortDateString() + " at " + Time[0].ToShortTimeString() + " A Short Entry on " + Instrument.MasterInstrument.Name + " was generated at " + Close[0];
						 //Print(" "); Print(messageTitle); Print(messageBody); Print(" "); Print(" ");
						 if (State == State.Historical) 
	    					return; 
						 Print(Time[0].ToShortTimeString() + " SHORT LOGIC -- Long Singal: " + longSignalCounter.ToString() + " Short Signal: " + shortSignalCounter.ToString() + "No Signal Counter: " + noSignalCounter.ToString());
						 if ( shortSignalCounter < 4)
						 Share("Hotmail", messageBody, new object[]{ "3103824522@tmomail.net", messageTitle, @"C:\Users\MBPtrader\Pictures\EURUSD_Opt_6_27.PNG"});
						 shortSignalCounter = shortSignalCounter +  1;
					 	 longSignalCounter = 0;
					 	 noSignalCounter = 0;
						 
					 } else {
						 //// No Signal	 
						 if ( DailyCalc )
							BarBrush = Brushes.Lime;
						 if (State == State.Historical) 
	    					return; 
//						 Print(Time[0].ToShortTimeString() + " No Sig LOGIC -- Long Singal: " + longSignalCounter.ToString() + " Short Signal: " + shortSignalCounter.ToString() + "No Signal Counter: " + noSignalCounter.ToString());
//						 if ( noSignalCounter < 4)
//						 Share("Hotmail", "Green Bars", new object[]{ "3103824522@tmomail.net", "Green Bars", @"C:\Users\MBPtrader\Pictures\EURUSD_Opt_6_27.PNG"});
						 noSignalCounter = noSignalCounter +  1;
					 	 longSignalCounter = 0;
					 	 shortSignalCounter = 0;
				 }
        			
			}
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="DailyCalc", Order=1, GroupName="Parameters")]
		public bool DailyCalc
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="HourlyCalc", Order=2, GroupName="Parameters")]
		public bool HourlyCalc
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> DailyST
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> HourlyST
		{
			get { return Values[1]; }
		}
		
		// MinuteST
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> MinuteST
		{
			get { return Values[2]; }
		}
		
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private STdailyAnd60[] cacheSTdailyAnd60;
		public STdailyAnd60 STdailyAnd60(bool dailyCalc, bool hourlyCalc)
		{
			return STdailyAnd60(Input, dailyCalc, hourlyCalc);
		}

		public STdailyAnd60 STdailyAnd60(ISeries<double> input, bool dailyCalc, bool hourlyCalc)
		{
			if (cacheSTdailyAnd60 != null)
				for (int idx = 0; idx < cacheSTdailyAnd60.Length; idx++)
					if (cacheSTdailyAnd60[idx] != null && cacheSTdailyAnd60[idx].DailyCalc == dailyCalc && cacheSTdailyAnd60[idx].HourlyCalc == hourlyCalc && cacheSTdailyAnd60[idx].EqualsInput(input))
						return cacheSTdailyAnd60[idx];
			return CacheIndicator<STdailyAnd60>(new STdailyAnd60(){ DailyCalc = dailyCalc, HourlyCalc = hourlyCalc }, input, ref cacheSTdailyAnd60);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.STdailyAnd60 STdailyAnd60(bool dailyCalc, bool hourlyCalc)
		{
			return indicator.STdailyAnd60(Input, dailyCalc, hourlyCalc);
		}

		public Indicators.STdailyAnd60 STdailyAnd60(ISeries<double> input , bool dailyCalc, bool hourlyCalc)
		{
			return indicator.STdailyAnd60(input, dailyCalc, hourlyCalc);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.STdailyAnd60 STdailyAnd60(bool dailyCalc, bool hourlyCalc)
		{
			return indicator.STdailyAnd60(Input, dailyCalc, hourlyCalc);
		}

		public Indicators.STdailyAnd60 STdailyAnd60(ISeries<double> input , bool dailyCalc, bool hourlyCalc)
		{
			return indicator.STdailyAnd60(input, dailyCalc, hourlyCalc);
		}
	}
}

#endregion
