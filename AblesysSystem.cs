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
		
		/// T3
		private int risk3 = 3;
		private int period3 = 10;
		private double atrtimes3 = 2;
		private int counter13= 0;
		private int counter23= 0;
		private int sideside3 = 1;
		private double dotplot3 = double.NaN;
		private Series<double> TrueRange3;
		internal Series<double> Sideside3;
		private int once3;
		
		/// Background Brushes
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
				PaintBar					= true;
				ShowArrow					= true;
				ShowStripe					= false;
				
				/// T2
				AddPlot(new Stroke(Brushes.Blue, 2), PlotStyle.Dot, "ATR Trailing Up");
				AddPlot(new Stroke(Brushes.Red, 2), PlotStyle.Dot, "ATR Trailing Dn");
				
				/// T3
				AddPlot(new Stroke(Brushes.Red, 4), PlotStyle.Dot, "ATR Trailing Up3");
				AddPlot(new Stroke(Brushes.Blue, 4), PlotStyle.Dot, "ATR Trailing Dn3");
				/// This should be a line is a sub panel
				AddPlot(new Stroke(Brushes.DodgerBlue, 1), PlotStyle.Dot, "TrueRangeSMA3");
			}
			else if (State == State.Configure)
			{
				/// T1
				trendOneDataSeries = new Series<double>(this, MaximumBarsLookBack.Infinite);
				/// T2
				TrueRange = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Sideside = new Series<double>(this, MaximumBarsLookBack.Infinite);
				/// T3
				TrueRange3 = new Series<double>(this, MaximumBarsLookBack.Infinite);
				Sideside3 = new Series<double>(this, MaximumBarsLookBack.Infinite);
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
			var trendTHree = setTrendThree();
			
			
			// MARK: - TODO show stripe when a 3 agree
			// Mark: TODO Make ATR a sub panel?
			
			if ( !ShowStripe ) { return; }
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
			
			
			return sideside;
			
		}
		
		public int setTrendThree() {

			double truerange3;
			double updotplot3;
			double lowdotplot3;
			if (CurrentBar == 21)
			{
				TrueRange3[0] = ((High[0] - Low[0]));
			}
			else
			{
				truerange3 = (High[0] - Low[0]);
				truerange3 = Math.Max (Math.Abs ((Low[0] - Close[1])), Math.Max (truerange3, Math.Abs ((High[0] - Close[1]))));
				TrueRange3[0] =  (truerange3);
			}

			double AtrValue3 = SMA(TrueRange3,period3)[1];
			TrueRangeSMA3[0] = (SMA(TrueRange3,period3)[1]);
			double Acelfactor3 =( ( atrtimes3 +0.1 * risk3) * AtrValue3);
			double Sigclose3;
			
			if (sideside3 == 1)
			{
				Sigclose3 = High[1];
				updotplot3 = (Sigclose3 - Acelfactor3);
				
				if (double.IsNaN (dotplot3) || (updotplot3 >= dotplot3))
				{
					dotplot3 = Bars.Instrument.MasterInstrument.RoundToTickSize( updotplot3);
				}
			}
			else if (sideside3 == -1)
			{
				Sigclose3 = Low[1];
				lowdotplot3 = (Sigclose3 + Acelfactor3);
				
				if (double.IsNaN (dotplot3) || (lowdotplot3 <= dotplot3))
				{
					dotplot3 =Bars.Instrument.MasterInstrument.RoundToTickSize( lowdotplot3);
				}
			}
			counter13++;
			counter23++;
			
			
			if (! double.IsNaN (dotplot3))
			{
				if (sideside3 != 1)
				{
					if ((sideside3 == -1) && ((High[0]+Low[0])/2 >= dotplot3))
					{
						sideside3 = 1;
						counter23 = 0;
						dotplot3 = Bars.Instrument.MasterInstrument.RoundToTickSize((High[0] - Acelfactor3));
					}
				}
				else if ((High[0]+Low[0])/2 <= dotplot3)
				{
					sideside3 = -1;
					counter23 = 0;
					dotplot3 = Bars.Instrument.MasterInstrument.RoundToTickSize((Low[0] + Acelfactor3));
				}
			}
			Sideside3[0] = (sideside3);
			if(Sideside3[0] != Sideside3[1])
			{
				once3 = 0;
			}
			if (sideside3 == 1)
			{
				if(once3 == 1)
				{
					Upper3[0] =  (dotplot3);
					Lower3.Reset ();
				}
				if(once3 == 0)
				{
					Upper3[0] =  (dotplot3);
					Lower3.Reset ();
				}
				once3 = 1;
			}
			
			if (sideside3 == -1)
			{
				if(once3 == 1)
				{
					Lower3[0] =  (dotplot3);
					Upper3.Reset ();
				}
				if(once3 == 0)
				{
					Lower3[0] = (dotplot3);
					Upper3.Reset ();
				}
				once3 = 1;
			}
			// Draw.Text(this, "MyText"+CurrentBar, sideside.ToString(), 0, High[0] + 100 * TickSize, Brushes.Blue);
			return sideside3;
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
		public  Series<double> Upper
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public  Series<double> Lower
		{
			get { return Values[1]; }
		}
			
		[Description("Number of ATR Multipliers")]
    	[Category("Parameters")]
		[Display(Name = "Number of ATR (Ex. 3 Time ATR)?", Description = "Number of ATR Multipliers", Order = 1, GroupName = "T2")]
		public double NumberOfAtrs
		{
			get	{	return atrtimes;}
			set	{	atrtimes = Math.Max (1, value);	}
		}
	
		[Description("Period")]
    	[Category("Parameters")]
		[Display(Name = "Period", Description = "Period", Order = 2, GroupName = "T2")]
		public int Period
		{
			get	{	return period;}	
			set	{	period = Math.Max(0, value);}
		}

		[Description("Risk ranges from 1-10, default is 3.")]
    	[Category("Parameters")]
		[Display(Name = "Risk 2", Description = "Risk ranges from 1-10, default is 3.", Order = 3, GroupName = "T2")]
		public int Risk2
		{
			get	{	return risk2;}	
			set	{	risk2 = Math.Max(1, value);}
		}
		
		/// T3
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Lower3
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> Upper3
		{
			get { return Values[3]; }
		}
		
		[Browsable(false)]
		[XmlIgnore()]
		public Series<double> TrueRangeSMA3
		{
			get { return Values[4]; }
		}
		
		[Description("Number of ATR Multipliers 3")]
    	[Category("T 3")]
		[Display(Name = "Number of ATR 3 (Ex. 3 Time ATR)", Description = "Number of ATR Multipliers", Order = 1, GroupName = "T3")]
		public double NumberOfAtrs3
		{
			get	{	return atrtimes3;}
			set	{	atrtimes3 = Math.Max (1, value);	}
		}
	
		
		[Description("Period 3")]
    	[Category("T 3")]
		[Display(Name = "Period 3", Description = "Period", Order = 2, GroupName = "T3")]
		public int Period3
		{
			get	{	return period3;}	
			set	{	period3 = Math.Max(0, value);}
		}
		
		[Description("Risk ranges from 1-10, default is 3.")]
    	[Category("T 3")]
		[Display(Name = "Risk 3", Description = "Risk ranges from 1-10, default is 3.", Order = 3, GroupName = "T3")]
		public int Risk3
		{
			get	{	return risk3;}	
			set	{	risk3 = Math.Max(1, value);}
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
