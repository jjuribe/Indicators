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
	public class AblesysSystem : Indicator
	{
		///  T1
		private int	myperiod	= 27;
		private Series<double> trendOneDataSeries; 
		private int risk=3;
		public int trend = 0;
		///  T2
		private Series<double> TrueRange;
		private Series<double>  Sideside;
		private int risk2 = 4;
		private int period= 12;
		private double atrtimes = 2;
		private int counter1= 0;
		private int counter2= 0;
		private int sideside= 1;
		private int once;
		
		
		// Background Brushes
		 private double iBrushOpacity = 0.1;
		 private Brush iBrushBackUp = new SolidColorBrush(Colors.Blue);
		 private Brush iBrushBackDown = new SolidColorBrush(Colors.Red);
		
			private double dotplot = double.NaN;
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Ablesys System";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= true;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				//IsSuspendedWhileInactive					= true;
				PaintBar					= false;
				ShowArrow					= true;
				ShowStripe					= true;
				
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Dot, "ATR Trailing Up");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "ATR Trailing Dn");
			}
			else if (State == State.Configure)
			{
				trendOneDataSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				TrueRange = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Sideside = new Series<double>(this, MaximumBarsLookBack.Infinite);
				// opacity right after OK or APPLY)
				 iBrushBackUp.Opacity = iBrushOpacity;
				 iBrushBackUp.Freeze();
				 iBrushBackDown.Opacity = iBrushOpacity;
				 iBrushBackDown.Freeze();
			}
			else if(State == State.DataLoaded)
			{
				ClearOutputWindow();
			}
		}

		protected override void OnBarUpdate()
		{
			if (CurrentBar <20) { return; }
			var trendOne = setTrendOne();
			var trendTwo = setTrendTwo();
			if (trendOne == 1 && trendTwo == 1) {
				BackBrush = iBrushBackUp;
				//BackBrushesAll[1] = Brushes.Gold;
			}
			if (trendOne == -1 && trendTwo == -1) {
				BackBrush = iBrushBackDown;
				//BackBrushesAll[1] = Brushes.Gold;
			}
			// Print(trendOne);
		}
		
		public int setTrendOne() {
			
			trendOneDataSeries[0] = (-100 * (MAX(High, myperiod)[0] - Close[0]) / (MAX(High, myperiod)[0] - MIN(Low, myperiod)[0] == 0 ? 1 : MAX(High, myperiod)[0] - MIN(Low, myperiod)[0]));
			
			/// Uptrend
			if (trendOneDataSeries[0] >= -33+risk)
			{
				trend = 1;
				if ( PaintBar ) {
					CandleOutlineBrush  = Brushes.DarkBlue;
		
					if(Open[0]<Close[0] ) {
							BarBrush  = Brushes.Transparent;
					} else{
							BarBrush  = Brushes.DodgerBlue;
					}
				}
	
			}
			else   /// downtrend
			if (trendOneDataSeries[0] <= -67-risk) {
				if ( PaintBar ) {
					CandleOutlineBrush  = Brushes.Crimson;
					if(Open[0]<Close[0] ) {
							BarBrush  = Brushes.Transparent;
					} else{
						BarBrush  = Brushes.Red;
					}			
				}
				trend = -1;
			}
			else   /// sideways
				{
				if ( PaintBar ) {
					CandleOutlineBrush  = Brushes.LimeGreen;
					if(Open[0]<Close[0] ) {
						BarBrush  = Brushes.Transparent;
					} else{
						BarBrush  = Brushes.LightGreen;
					}	
				}
				trend = 0;
			}
			return trend ;
		}
		
		public int setTrendTwo() {

			double truerange;
			double updotplot;
			double lowdotplot;
			
			if (CurrentBar == 21)
			{
				TrueRange[0] = ((High[0] - Low[0]));
			}
			else
			{
				truerange = (High[0] - Low[0]);
				truerange = Math.Max (Math.Abs ((Low[0] - Close[1])), Math.Max (truerange, Math.Abs ((High[0] - Close[1]))));
				TrueRange[0] = truerange;
			}

			double AtrValue = SMA(TrueRange,period)[1];//
			double Acelfactor =( ( atrtimes+0.1*risk2) * AtrValue);
			double Sigclose;
			
			if (sideside == 1)
			{
				Sigclose = High[1];
				updotplot = (Sigclose - Acelfactor);
				
				if (double.IsNaN (dotplot) || (updotplot >= dotplot))
				{
					dotplot = Bars.Instrument.MasterInstrument.RoundToTickSize( updotplot);
				}
			}
			else if (sideside == -1)
			{
				Sigclose = Low[1];
				lowdotplot = (Sigclose + Acelfactor);
				
				if (double.IsNaN (dotplot) || (lowdotplot <= dotplot))
				{
					dotplot =Bars.Instrument.MasterInstrument.RoundToTickSize( lowdotplot);
				}
			}
			counter1++;
			counter2++;
			
			
			if (! double.IsNaN (dotplot))
			{
				if (sideside != 1)
				{
					if ((sideside == -1) && (Close[0] > dotplot))			// was high
					{
						sideside = 1;
						counter2 = 0;
						dotplot = Bars.Instrument.MasterInstrument.RoundToTickSize((High[0] - Acelfactor));
					}
				}
				else if (Close[0] < dotplot)								// was low
				{
					sideside = -1;
					counter2 = 0;
					dotplot = Bars.Instrument.MasterInstrument.RoundToTickSize((Low[0] + Acelfactor));
				}
			}
			Sideside[0] = sideside;
			if(Sideside[0]!=Sideside[1])
			{
				once=0;
			}
			if (sideside == 1)
			{
				if(once==1)
				{
					Upper[0] = dotplot;
					Lower.Reset ();
				}
				if(once==0)
				{
					//DrawArrowUp("arrow" + CurrentBar, true, 0,  dotplot, Color.Blue);
				}
				once=1;
			}
			
			if (sideside == -1)
			{
				if(once==1)
				{
					Lower[0] = dotplot;
					Upper.Reset ();
				}
				if(once==0)
				{
					//DrawArrowDown("arrow" + CurrentBar, true, 0,  dotplot, Color.Red);
				}
				once=1;
			}	
			
			// Draw.Text(this, "MyText"+CurrentBar, sideside.ToString(), 0, High[0] + 100 * TickSize, Brushes.Blue);
			return sideside;
			
		}
		
		

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="PaintBar", Order=1, GroupName="Parameters")]
		public bool PaintBar
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowArrow", Order=2, GroupName="Parameters")]
		public bool ShowArrow
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="ShowStripe", Order=3, GroupName="Parameters")]
		public bool ShowStripe
		{ get; set; }
		
		/// T2
		[Browsable(false)]
		[XmlIgnore()]
		public  Series<double> Lower
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public  Series<double> Upper
		{
			get { return Values[0]; }
		}
			
		[Description("Number of ATR Multipliers")]
    	[Category("Parameters")]
		//[Gui.Design.DisplayNameAttribute("Number of ATR (Ex. 3 Time ATR) ")]
		[Display(Name = "Number of ATR (Ex. 3 Time ATR)?", Description = "Number of ATR Multipliers", Order = 1, GroupName = "1. Parameters")]
		public double NumberOfAtrs
		{
			get	{	return atrtimes;}
			set	{	atrtimes = Math.Max (1, value);	}
		}
	
		[Description("Period")]
    	[Category("Parameters")]
		//[Gui.Design.DisplayNameAttribute("Periods")]
		[Display(Name = "Period", Description = "Period", Order = 2, GroupName = "1. Parameters")]
		public int Period
		{
			get	{	return period;}	
			set	{	period = Math.Max(0, value);}
		}

		[Description("Risk ranges from 1-10, default is 3.")]
    	[Category("Parameters")]
		//[Gui.Design.DisplayNameAttribute("Risk")]
		[Display(Name = "Risk 2", Description = "Risk ranges from 1-10, default is 3.", Order = 3, GroupName = "1. Parameters")]
		public int Risk2
		{
			get	{	return risk2;}	
			set	{	risk2 = Math.Max(1, value);}
		}
			
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private AblesysSystem[] cacheAblesysSystem;
		public AblesysSystem AblesysSystem(bool paintBar, bool showArrow, bool showStripe)
		{
			return AblesysSystem(Input, paintBar, showArrow, showStripe);
		}

		public AblesysSystem AblesysSystem(ISeries<double> input, bool paintBar, bool showArrow, bool showStripe)
		{
			if (cacheAblesysSystem != null)
				for (int idx = 0; idx < cacheAblesysSystem.Length; idx++)
					if (cacheAblesysSystem[idx] != null && cacheAblesysSystem[idx].PaintBar == paintBar && cacheAblesysSystem[idx].ShowArrow == showArrow && cacheAblesysSystem[idx].ShowStripe == showStripe && cacheAblesysSystem[idx].EqualsInput(input))
						return cacheAblesysSystem[idx];
			return CacheIndicator<AblesysSystem>(new AblesysSystem(){ PaintBar = paintBar, ShowArrow = showArrow, ShowStripe = showStripe }, input, ref cacheAblesysSystem);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.AblesysSystem AblesysSystem(bool paintBar, bool showArrow, bool showStripe)
		{
			return indicator.AblesysSystem(Input, paintBar, showArrow, showStripe);
		}

		public Indicators.AblesysSystem AblesysSystem(ISeries<double> input , bool paintBar, bool showArrow, bool showStripe)
		{
			return indicator.AblesysSystem(input, paintBar, showArrow, showStripe);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.AblesysSystem AblesysSystem(bool paintBar, bool showArrow, bool showStripe)
		{
			return indicator.AblesysSystem(Input, paintBar, showArrow, showStripe);
		}

		public Indicators.AblesysSystem AblesysSystem(ISeries<double> input , bool paintBar, bool showArrow, bool showStripe)
		{
			return indicator.AblesysSystem(input, paintBar, showArrow, showStripe);
		}
	}
}

#endregion
