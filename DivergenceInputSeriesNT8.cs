#region Using declarations
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Reflection;
using System.Windows.Media;
using System.Xml.Serialization;
using NinjaTrader.Cbi;
using NinjaTrader.Data;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
using NinjaTrader.NinjaScript.Indicators;
#endregion
// Converted to NT8 by Aligator - May 20, 2017

// Note by Aligator: The original NT7 Version of this universal divergence indicator by "tulanch" is located at:

//  http://ninjatrader.com/support/forum/local_links_search.php?action=show&literal=1&search=DivergenceInputSeriesNT8&desc=1

// I have converted this indicator to NT8 using conversion tools on Ninjatrader Forum.
// I have not attempted to clean up the script per NT8 performance practices.
		
/*
	Version 1.0

	basic logic

	explanation with system in hl_mode thus uses Highs and Lows in all calculations  
		
	for illustration purposes assume input series set to CCI(14) via NT user interface
		
	Found swing point High
		set swing point high to price that is the high of current bar
		obtain input series value at swing point, which will be input[0] at this point in time 
		
	When/if price moves beyond swing point (before a new swing point high is created )
		obtain current input series value
		
	if current input series value < input series value at swing point
			signal downward divergence 
*/

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    /// <summary>
    /// This indicator
    /// </summary>
    public class DivergenceInputSeriesNT8 : Indicator
    {
        #region Variables

			private BarsPeriodType priceDataType= BarsPeriodType.Minute; // chart time, defaulted to minute
		
			int pricePeriod=1; // period of chart defaulted to 1 unit
			private int stoffset0 = 0; // signal tick offset, 0 will signal if price goes 1 tick beyound
			private int swingoffset = 2; // offset ticks from extreme of swing points  
			private int sigoffset = 4; // offset ticks from extreme of signal points 		
            private int bbefore0 = 10; // Default setting for Bbefore0
            private int bafter0 = 2; // Default setting for Bafter0
			bool displayswings0=true; // true displaye swing points	
			bool hl_mode=true; // high low if true, other wise logic based on close if false
			bool barSig_mode=true;  // default down sigs on down bars and up sigs on up bars
			bool pre_bar_swing_high=false;
			bool pre_bar_swing_low=false;
            private int numBars  = 4;  // Default setting for NumBars for swing calculation - hard wired to 4 may be input variable in future
			string dot="l"; // wingdings
			int bar_to_test=4; // changing to 4 to match fractals, 3rd bar back, but becuase I test on first tick of bar, this is the 2nd full bar back
			bool firsttime=true; // firstime in do some setup
			double indicator_data; // input[0] value
			private int dotsize = 9; // defualt size of swing point
			bool haveswinghigh; // found a swing point high
			bool haveswinglow; // found a swing point low
			double sh_price; // swing point high price
			double sh_value; // value from input series at the swing point high
			double sl_price; // swing low price
			double sl_value; // value of input data at swing low point 
			double tvalue; // test value, holds high, low, or close based on hl_mode value
			int signalbarsback;  // bars back from current bar to draw, should be equal to the barsafter value
			bool displaymsg=true; // display the error or status message
			Brush uparrowcolor = Brushes.Green;   // default color of up signal arrow
			Brush dnarrowcolor = Brushes.Red;     // default color of down signal arrow
			Brush swinglowcolor  = Brushes.Green; // default color of swing point low  
			Brush swinghighcolor  = Brushes.Red;  // default color of swing point high
		
        #endregion
		
        /// <summary>
        /// This method is used to configure the indicator and is called once before any bar data is loaded.
        /// </summary>
        protected override void OnStateChange()
        {
            if (State == State.SetDefaults)
            {
                Name = "DivergenceInputSeriesNT8";
                Description = "This indicator will check for divergence of the defined input series against price, by default it uses swing points as base point of divergence logic";
//            	AddPlot(new Stroke(Brushes.Orange, 2), PlotStyle.Line, "Plot0"); 
	
				IsOverlay	= true;
				Calculate	= Calculate.OnBarClose;
            }
			
			else if (State == State.Configure)
			{
				AddDataSeries( Instrument.FullName, priceDataType, pricePeriod);	// BarsInProgress 1

				sl_price 	= 0.0;
				sl_value 	= 0.0;
				sh_price 	= 0.0;
				sh_value 	= 0.0; 
				bar_to_test = bafter0;					
			}
        }
        /// <summary>
        /// Called on each bar update event (incoming tick)
        /// </summary>
        protected override void OnBarUpdate()
        {
			if (CurrentBar < 21 )  //based on logic that is used in the BW Pivot Calculations (BarsRequired)
			{
				return;
			}			
			
			if( firsttime )
			{
				firsttime=false;

				if( displaymsg )
				{
					if( BarsPeriods[0].Value != pricePeriod ||BarsPeriod.BarsPeriodType != priceDataType )
					{
						Draw.TextFixed(this,"divmsg", " Adjust Price Type and Period input paratmeters to match chart's values", TextPosition.BottomLeft);
					}
					else
					{
						Draw.TextFixed(this,"divmsg", " Checking for indicator divergence with price",TextPosition.BottomLeft); 
					}
				}				

			}
			
/* debug to view input data 
			if( BarsInProgress == 0 )
			{	
				Print(" input "+BarsInProgress+" "+Input[0]+" High0 "+Highs[0][0]+" Low0 "+Lows[0][0]+" Open0 "+Opens[0][0]+" Close0 "+Closes[0][0]+" High1 "+Highs[1][0]+" Low1 "+Lows[1][0]+" Open1 "+Opens[1][0]+" Close1 "+Closes[1][0]);
			}
			else 
			{
				Print(" input "+BarsInProgress+" "+Input[0]+" High "+Highs[1][0]+" Low "+Lows[1][0]+" Open "+Opens[1][0]+" Close "+Closes[1][0]);
			}
			return;			
*/			
			// only do calculations based on base bar series defined by input
			if( BarsInProgress == 0 )
			{
				indicator_data = Input[0];
 
				// ///////////////////////////////////
				// up signal - pre swing point check 
				// ///////////////////////////////////
				
				if( hl_mode )
				{
					tvalue=Lows[1][0];
				}
				else
				{
					tvalue=Closes[1][0];
				}
		
				if( tvalue <= (sl_price -(stoffset0*TickSize)))
				{
					if(indicator_data > sl_value)
					{
	
						if( barSig_mode )
						{
							if( Closes[1][0] >= Opens[1][0] ) // only displayes on up bars
								Draw.ArrowUp(this,Convert.ToString(CurrentBar)+"usig", true, 0, Lows[1][0] - (sigoffset*TickSize), uparrowcolor);  
						}
						else
						{
							Draw.ArrowUp(this,Convert.ToString(CurrentBar)+"usig", true, 0, Lows[1][0] - (sigoffset*TickSize), uparrowcolor);  
						}
						
					}
		
				}
		
				// //////////////////////////////////////////
				// down signal down  - pre swing point check 
				// //////////////////////////////////////////
				if( hl_mode )
				{
					tvalue=Highs[1][0];
				}
				else
				{
					tvalue=Closes[1][0];
				}					

				if(  tvalue >= (sh_price +(stoffset0*TickSize)))
				{
					if( indicator_data < sh_value)
					{
						if( barSig_mode )						
						{
							if( Closes[1][0] <= Opens[1][0] )  // only displayes on down bars
								Draw.ArrowDown(this,Convert.ToString(CurrentBar)+"dsig", true, 0, Highs[1][0] + (sigoffset*TickSize), dnarrowcolor); 
						}
						else
						{
							Draw.ArrowDown(this,Convert.ToString(CurrentBar)+"dsig", true, 0, Highs[1][0] + (sigoffset*TickSize), dnarrowcolor); 
						}
						
					}

				}
				
 				// //////////////////////////////////
				// swing point calculations start here
				// //////////////////////////////////			
			
				haveswinghigh=false;
				if( !pre_bar_swing_high )
				{
					haveswinghigh = isHighPivot2( bbefore0,bafter0, hl_mode );
				}
				
				haveswinglow=false;
				if( !pre_bar_swing_low )
				{
					haveswinglow = isLowPivot2( bbefore0,bafter0, hl_mode );			
				}
				// swing point high calculations closed based
				if( haveswinghigh )
				{
					
					pre_bar_swing_high=true;

					if( hl_mode )
					{
						sh_price=Highs[1][bar_to_test];
					}
					else
					{
						sh_price=Closes[1][bar_to_test];
					}
					
					sh_value = 	Input[bar_to_test];
					
					if( displayswings0 )
					{
//	debug				Print(" high at "+Highs[1][bar_to_test]);
						Draw.Text(this,Convert.ToString(CurrentBar)+"h",false,dot,bar_to_test,Highs[1][bar_to_test]+(TickSize*swingoffset),0,swinghighcolor, new SimpleFont("Wingdings",dotsize), TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					}
					
				}
				else
				{
					pre_bar_swing_high=false;
				}
				
				// swing point low calculations closed based
				
				
				if( haveswinglow )
				{

					pre_bar_swing_low=true;
					
					
					if( hl_mode )
					{
						sl_price=Lows[1][bar_to_test];
					}
					else
					{
						sl_price=Closes[1][bar_to_test];  
					}		
					
//	debug			Print("LOW SWING "+Time[0]+" "+sl_price);					
					
					sl_value = 	Input[bar_to_test];

					if( displayswings0 )
					{
//	debug				Print(" low at "+Lows[1][bar_to_test]);						
						Draw.Text(this,Convert.ToString(CurrentBar)+"l",false,dot,bar_to_test,Lows[1][bar_to_test]-(TickSize*swingoffset),0,swinglowcolor, new SimpleFont("Wingdings",dotsize), TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					}
				}
				else
				{
					pre_bar_swing_low=false;
				}
			
			} // end of base bar trigger
			
        }// end of onbar update 

		private bool isHighPivot2(int barsb4,int barsafter, bool hl_mode)
		{
			#region b4 and after version version of swingpoint high 
			int tstbar;
			int lastbar;
			int i;
			bool havemax;
			double maxvalue;
			
			tstbar  = barsafter; // note that bars index are 0 based
			lastbar = barsb4+barsafter;

			
			havemax=true;
			
			if( hl_mode )
			{
				
				maxvalue = Highs[1][tstbar];

				// ///////////
				// after bars
				// ///////////

				for( i=barsafter-1; i >= 0; i--)
				{
//					if( Highs[1][i] >= maxvalue )
					if( Highs[1][i] > maxvalue )
					{
						havemax=false;
					}
				}
				
				if( havemax ) // only continue if still have max
				{
					
					// ///////////
					// before bars
					// ///////////
					for( i=lastbar; i > barsafter; i--)
					{
//						if( Highs[1][i] >= maxvalue )
						if( Highs[1][i] > maxvalue )
						{
							havemax=false;
						}
					}
					
				}
				
				return havemax;
				
			}
			else
			{
 			
				maxvalue = Closes[1][tstbar];

				// ///////////
				// after bars
				// ///////////

				for( i=barsafter-1; i >= 0; i--)
				{
//					if( Closes[1][i] >= maxvalue )
					if( Closes[1][i] > maxvalue )
					{
						havemax=false;
					}
				}
				
				if( havemax ) // only continue if still have max
				{
					
					// ///////////
					// before bars
					// ///////////
					for( i=lastbar; i > barsafter; i--)
					{
						//example: 9 > 6
//						if( Closes[1][i] >= maxvalue )
						if( Closes[1][i] > maxvalue )
						{
							havemax=false;
						}
					}
					
				}
				
				return havemax; 			
			}
			
			#endregion			
		}
		private bool isLowPivot2(int barsb4,int barsafter, bool hl_mode)
		{
		#region  b4 and after version version of swingpoint low			
			int tstbar;
			int lastbar;
			int i;
			double minvalue;
			
			bool havemin;
		
			tstbar  = barsafter; // note that bars index are 0 based
			lastbar = barsb4+barsafter;

			havemin=true;
			
			if( hl_mode )
			{
				// s[1][0]
				minvalue = Lows[1][tstbar];

				// ///////////
				// after bars
				// ///////////

				for( i=barsafter-1; i >= 0; i--)
				{
//					if( Lows[1][i] <= minvalue )
					if( Lows[1][i] < minvalue )
					{
						havemin=false;
					}
				}
				
				if( havemin ) // only continue if still have max
				{
					// ///////////
					// before bars
					// ///////////
					for( i=lastbar; i > barsafter; i--)
					{
//						if( Lows[1][i] <= minvalue )
						if( Lows[1][i] < minvalue )
						{
							havemin=false;
						}
					}
					
				}
				
				return havemin;
			}
			else
			{
				minvalue = Closes[1][tstbar];

				// ///////////
				// after bars
				// ///////////

				for( i=barsafter-1; i >= 0; i--)
				{
//					if( Closes[1][i] <= minvalue )
					if( Closes[1][i] < minvalue )
					{
						havemin=false;
					}
				}
				
				if( havemin ) // only continue if still have max
				{
					// ///////////
					// before bars
					// ///////////
					for( i=lastbar; i > barsafter; i--)
					{
						// example: 3 < 5
//						if( Closes[1][i] <= minvalue )
						if( Closes[1][i] < minvalue )
						{
							havemin=false;
						}
					}
					
				}
				
				return havemin;
			}
			
			#endregion			
		}			
					
        #region Properties
//        [Browsable(false)]	// this line prevents the data series from being displayed in the indicator properties dialog, do not remove
//        [XmlIgnore()]		// this line ensures that the indicator can be saved/recovered as part of a chart template, do not remove
//        public DataSeries Plot0
//        {
//            get { return Values[0]; }
//        }

//        [Description("")]
//        [GridCategory("Parameters")]
//        public int MyInput0
//        {
//            get { return myInput0; }
//            set { myInput0 = Math.Max(1, value); }
//        }
		
        [NinjaScriptProperty]					
        [Display(Name = "Price Period", Description = "The period of the price data, typically set this to the same as chart", GroupName = "Price Data", Order = 1)]					
        public int PricePeriod
        {
            get { return pricePeriod; }
            set { pricePeriod = Math.Max(1, value); }
        }		
		
        [NinjaScriptProperty]		
        [Display(Name = "Signal Trigger Offset", Description = "Number of ticks price must move beyond prior swing point to possibly trigger a signal arrow", GroupName = "Divergence", Order = 1)]		
        public int Stoffset0
        {
            get { return stoffset0; }
            set { stoffset0 = Math.Max(0, value); }
        }			
 		
        [NinjaScriptProperty] 		
        [Display(Name = "Swing Point Offset", Description = "Ticks offset from bar extreme for swing point ", GroupName = "Divergence", Order = 1)] 		
        public int Swingoffset
        {
            get { return swingoffset; }
            set { swingoffset = Math.Max(0, value); }
        }		
		
        [NinjaScriptProperty]		
        [Display(Name = "Signal Offset", Description = "Ticks offset from bar extreme for singal arrow ", GroupName = "Divergence", Order = 1)]		
        public int Sigoffset
        {
            get { return sigoffset; }
            set { sigoffset = Math.Max(0, value); }
        }		
		
		[NinjaScriptProperty]		
		[Display(Name = "HLC Mode", Description = " HighLowClose mode  -True- HighLlow-of-bar logic based    -False- Close-of-bar logic based", GroupName = "Divergence", Order = 1)]		
		public bool Hl_mode
        {
            get { return hl_mode; }
            set { hl_mode = value; }
        }			
		
		[NinjaScriptProperty]		
		[Display(Name = "Display Message", Description = " Display messages in the lower right hand corner - by defualt is becuase if you change time frames and/or chart the input series can defualt back to the close easily ", GroupName = "Divergence", Order = 1)]		
		public bool Displaymsg
        {
            get { return displaymsg; }
            set { displaymsg = value; }
        }			
				
		[NinjaScriptProperty]				
		[Display(Name = "Bar Signal Mode", Description = " Up Down Bar mode  -True- signals on respective bars meaning down signal only on down bar or up signal only on up bar    -False- signals on any bar ", GroupName = "Divergence", Order = 1)]				
		public bool BarSig_mode
        {
            get { return barSig_mode; }
            set { barSig_mode = value; }
        }			
		
       [NinjaScriptProperty]				
       [Display(Name = "Bars Before", Description = "bars before to define swing - starting at the bar  to test which is Bar[ barsafter ]", GroupName = "Swing Points", Order = 1)]				
       public int Bbefore0
        {
            get { return bbefore0; }
            set { bbefore0 = Math.Max(1, value); }
        }

        [NinjaScriptProperty]
        [Display(Name = "Bars After", Description = " Bars after to define swing - number of bars back from Bar[ 0 ] (current bar)  to test as swing point", GroupName = "Swing Points", Order = 1)]
        public int Bafter0
        {
            get { return bafter0; }
            set { bafter0 = Math.Max(1, value); }
        }

        [NinjaScriptProperty]		
        [Display(Name = "Show Swings", Description = "Display Swing Points ", GroupName = "Swing Points", Order = 1)]		
        public bool Displayswings0
        {
            get { return displayswings0; }
            set { displayswings0 = value; }
        }		
				
       [NinjaScriptProperty]				
       [Display(Name = "Swing Points Size", Description = "Size of swing point dot ", GroupName = "Swing Points", Order = 1)]				
       public int Dotsize
        {
            get { return dotsize; }
            set { dotsize = Math.Max(1, value); }
        }

//		[XmlIgnore()]		//I hope this line ensures that this value will be saved/recovered when you restart the system
        [NinjaScriptProperty]		
		[Display(Name = "Price Type", Description = "The type of price data, typically set this to the same as the chart", GroupName = "Price Data", Order = 1)]		
		public BarsPeriodType PriceDataType0
        {
            get { return priceDataType; }
            set { priceDataType = value; }
        }
/*		
		// Serialize our Color object
		[Browsable(false)]
		public string PriceDataType0Serialize
		{
			get { return NinjaTrader.Gui.Design.SerializableColor.ToString(priceDataType); }
			set { priceDataType = NinjaTrader.Gui.Design.SerializableColor.FromString(value); }
		}
*/					
		[XmlIgnore()]		//this line ensures that this value will be saved/recovered when you restart the system
		[NinjaScriptProperty][Display(Name = "Up Signal Color", Description = "Up signal color", GroupName = "Colors", Order = 1)]
        public Brush Uparrowcolor
        {
            get { return uparrowcolor; }
            set { uparrowcolor = value; }
        }
		// Serialize our Color object
		[Browsable(false)]
		public string UparrowcolorSerialize
		{
			get { return Serialize.BrushToString(uparrowcolor); }
			set { uparrowcolor = Serialize.StringToBrush(value); }
		}	

		[XmlIgnore()]		// this line ensures that this value will be saved/recovered when you restart the system
		[NinjaScriptProperty]
		[Display(Name = "Down Signal Color", Description = "Down signal color", GroupName = "Colors", Order = 1)]
        public Brush Dnarrowcolor
        {
            get { return dnarrowcolor; }
            set { dnarrowcolor = value; }
        }
		// Serialize our Color object
		[Browsable(false)]
		public string DnarrowcolorSerialize
		{
			get { return Serialize.BrushToString(dnarrowcolor); }
			set { dnarrowcolor = Serialize.StringToBrush(value); }
		}
			
		[XmlIgnore()]		// this line ensures that this value will be saved/recovered when you restart the system
		[NinjaScriptProperty]			
		[Display(Name = "Swing High Color", Description = "Swing Point High color", GroupName = "Colors", Order = 1)]
        public Brush Swinghighcolor
        {
            get { return swinghighcolor; }
            set { swinghighcolor = value; }
        }
		// Serialize our Color object
		[Browsable(false)]
		public string SwinghighcolorSerialize
		{
			get { return Serialize.BrushToString(swinghighcolor); }
			set { swinghighcolor = Serialize.StringToBrush(value); }
		}
					
		[XmlIgnore()]		// this line ensures that this value will be saved/recovered when you restart the system
		[NinjaScriptProperty]					
		[Display(Name = "Swing Low Color", Description = "Swing Point Low color", GroupName = "Colors", Order = 1)]
        public Brush Swinglowcolor
        {
            get { return swinglowcolor; }
            set { swinglowcolor = value; }
        }
		// Serialize our Color object
		[Browsable(false)]
		public string SwinglowcolorSerialize
		{
			get { return Serialize.BrushToString(swinglowcolor); }
			set { swinglowcolor = Serialize.StringToBrush(value); }
		}
        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private DivergenceInputSeriesNT8[] cacheDivergenceInputSeriesNT8;
		public DivergenceInputSeriesNT8 DivergenceInputSeriesNT8(int pricePeriod, int stoffset0, int swingoffset, int sigoffset, bool hl_mode, bool displaymsg, bool barSig_mode, int bbefore0, int bafter0, bool displayswings0, int dotsize, BarsPeriodType priceDataType0, Brush uparrowcolor, Brush dnarrowcolor, Brush swinghighcolor, Brush swinglowcolor)
		{
			return DivergenceInputSeriesNT8(Input, pricePeriod, stoffset0, swingoffset, sigoffset, hl_mode, displaymsg, barSig_mode, bbefore0, bafter0, displayswings0, dotsize, priceDataType0, uparrowcolor, dnarrowcolor, swinghighcolor, swinglowcolor);
		}

		public DivergenceInputSeriesNT8 DivergenceInputSeriesNT8(ISeries<double> input, int pricePeriod, int stoffset0, int swingoffset, int sigoffset, bool hl_mode, bool displaymsg, bool barSig_mode, int bbefore0, int bafter0, bool displayswings0, int dotsize, BarsPeriodType priceDataType0, Brush uparrowcolor, Brush dnarrowcolor, Brush swinghighcolor, Brush swinglowcolor)
		{
			if (cacheDivergenceInputSeriesNT8 != null)
				for (int idx = 0; idx < cacheDivergenceInputSeriesNT8.Length; idx++)
					if (cacheDivergenceInputSeriesNT8[idx] != null && cacheDivergenceInputSeriesNT8[idx].PricePeriod == pricePeriod && cacheDivergenceInputSeriesNT8[idx].Stoffset0 == stoffset0 && cacheDivergenceInputSeriesNT8[idx].Swingoffset == swingoffset && cacheDivergenceInputSeriesNT8[idx].Sigoffset == sigoffset && cacheDivergenceInputSeriesNT8[idx].Hl_mode == hl_mode && cacheDivergenceInputSeriesNT8[idx].Displaymsg == displaymsg && cacheDivergenceInputSeriesNT8[idx].BarSig_mode == barSig_mode && cacheDivergenceInputSeriesNT8[idx].Bbefore0 == bbefore0 && cacheDivergenceInputSeriesNT8[idx].Bafter0 == bafter0 && cacheDivergenceInputSeriesNT8[idx].Displayswings0 == displayswings0 && cacheDivergenceInputSeriesNT8[idx].Dotsize == dotsize && cacheDivergenceInputSeriesNT8[idx].PriceDataType0 == priceDataType0 && cacheDivergenceInputSeriesNT8[idx].Uparrowcolor == uparrowcolor && cacheDivergenceInputSeriesNT8[idx].Dnarrowcolor == dnarrowcolor && cacheDivergenceInputSeriesNT8[idx].Swinghighcolor == swinghighcolor && cacheDivergenceInputSeriesNT8[idx].Swinglowcolor == swinglowcolor && cacheDivergenceInputSeriesNT8[idx].EqualsInput(input))
						return cacheDivergenceInputSeriesNT8[idx];
			return CacheIndicator<DivergenceInputSeriesNT8>(new DivergenceInputSeriesNT8(){ PricePeriod = pricePeriod, Stoffset0 = stoffset0, Swingoffset = swingoffset, Sigoffset = sigoffset, Hl_mode = hl_mode, Displaymsg = displaymsg, BarSig_mode = barSig_mode, Bbefore0 = bbefore0, Bafter0 = bafter0, Displayswings0 = displayswings0, Dotsize = dotsize, PriceDataType0 = priceDataType0, Uparrowcolor = uparrowcolor, Dnarrowcolor = dnarrowcolor, Swinghighcolor = swinghighcolor, Swinglowcolor = swinglowcolor }, input, ref cacheDivergenceInputSeriesNT8);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.DivergenceInputSeriesNT8 DivergenceInputSeriesNT8(int pricePeriod, int stoffset0, int swingoffset, int sigoffset, bool hl_mode, bool displaymsg, bool barSig_mode, int bbefore0, int bafter0, bool displayswings0, int dotsize, BarsPeriodType priceDataType0, Brush uparrowcolor, Brush dnarrowcolor, Brush swinghighcolor, Brush swinglowcolor)
		{
			return indicator.DivergenceInputSeriesNT8(Input, pricePeriod, stoffset0, swingoffset, sigoffset, hl_mode, displaymsg, barSig_mode, bbefore0, bafter0, displayswings0, dotsize, priceDataType0, uparrowcolor, dnarrowcolor, swinghighcolor, swinglowcolor);
		}

		public Indicators.DivergenceInputSeriesNT8 DivergenceInputSeriesNT8(ISeries<double> input , int pricePeriod, int stoffset0, int swingoffset, int sigoffset, bool hl_mode, bool displaymsg, bool barSig_mode, int bbefore0, int bafter0, bool displayswings0, int dotsize, BarsPeriodType priceDataType0, Brush uparrowcolor, Brush dnarrowcolor, Brush swinghighcolor, Brush swinglowcolor)
		{
			return indicator.DivergenceInputSeriesNT8(input, pricePeriod, stoffset0, swingoffset, sigoffset, hl_mode, displaymsg, barSig_mode, bbefore0, bafter0, displayswings0, dotsize, priceDataType0, uparrowcolor, dnarrowcolor, swinghighcolor, swinglowcolor);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.DivergenceInputSeriesNT8 DivergenceInputSeriesNT8(int pricePeriod, int stoffset0, int swingoffset, int sigoffset, bool hl_mode, bool displaymsg, bool barSig_mode, int bbefore0, int bafter0, bool displayswings0, int dotsize, BarsPeriodType priceDataType0, Brush uparrowcolor, Brush dnarrowcolor, Brush swinghighcolor, Brush swinglowcolor)
		{
			return indicator.DivergenceInputSeriesNT8(Input, pricePeriod, stoffset0, swingoffset, sigoffset, hl_mode, displaymsg, barSig_mode, bbefore0, bafter0, displayswings0, dotsize, priceDataType0, uparrowcolor, dnarrowcolor, swinghighcolor, swinglowcolor);
		}

		public Indicators.DivergenceInputSeriesNT8 DivergenceInputSeriesNT8(ISeries<double> input , int pricePeriod, int stoffset0, int swingoffset, int sigoffset, bool hl_mode, bool displaymsg, bool barSig_mode, int bbefore0, int bafter0, bool displayswings0, int dotsize, BarsPeriodType priceDataType0, Brush uparrowcolor, Brush dnarrowcolor, Brush swinghighcolor, Brush swinglowcolor)
		{
			return indicator.DivergenceInputSeriesNT8(input, pricePeriod, stoffset0, swingoffset, sigoffset, hl_mode, displaymsg, barSig_mode, bbefore0, bafter0, displayswings0, dotsize, priceDataType0, uparrowcolor, dnarrowcolor, swinghighcolor, swinglowcolor);
		}
	}
}

#endregion
