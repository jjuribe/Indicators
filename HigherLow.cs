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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
    public class HigherLow : Indicator
    {
        #region Variables
			// Higher Pivot Low
			private int 		strength = 3; 
			private	int[] 		pivLowBar 	= new int[3];
			private	double[]	pivLowPrice	= new double[3];
			private int  HPL_Counter;
			// Lower Pivot High
			private	double[] 	pivHighPrice = new double[3];
			private	int[] 		pivHighBar	= new int[3];
			private int  LPH_Counter;
		
        #endregion

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{				
				Description							= @"Higher Pivots Lows";
				Name								= "HigherLow";				
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= false;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
			}
			else if (State == State.Configure)
			{
				
			}
		}

 
        protected override void OnBarUpdate()
        {
			
		//******************************************		Swing Low - Higher Low	************************************************
		if( CurrentBar > strength + 1 )
		if (Swing(Low, 3).SwingLow[0] == Low[strength + 1])
            {
				//DrawDot( "swingL" + CurrentBar, true, strength + 1, Low[strength + 1] , Color.Lime );	
				// update bar array
					pivLowBar[2] = pivLowBar[1];
					pivLowBar[1] = pivLowBar[0];
					pivLowBar[0] = CurrentBar- (strength+1);				
					//update price array
					pivLowPrice[2] = pivLowPrice[1];
					pivLowPrice[1] = pivLowPrice[0];
					pivLowPrice[0] = Low[strength+1];
			
					// mark Higher pivot Low
					if( pivLowPrice[0] > pivLowPrice[1] )	
						{
							if( HPL_Counter == 0 )
								Draw.Dot(this, "swingL"+ CurrentBar.ToString(), true, strength + 1, Low[strength + 1]  - TickSize, Brushes.LimeGreen);
							HPL_Counter = HPL_Counter +1;
							//LPH_Counter = 0;
							//if( HPL_Counter > 2 )
							//	DrawText("hplc"+CurrentBar, HPL_Counter.ToString(),0, pivLowPrice[0]-TickSize , Color.Lime);
							if( HPL_Counter == 2 )
								{
								// DrawArrowUp("hpl"+CurrentBar, 0, pivLowPrice[0]-TickSize, Color.Lime);
								ArrowUp myArrow = Draw.ArrowUp(this, "hpl"+CurrentBar.ToString(), true, 0, pivLowPrice[0]-TickSize, Brushes.LimeGreen);
								myArrow.OutlineBrush =  Brushes.Green;
								//DrawText("hplc"+CurrentBar, "_____",2, pivLowPrice[0] , Color.Lime);
								Draw.Text(this, "hplc"+CurrentBar.ToString(), "_____", 2, pivLowPrice[0] , Brushes.Green);
								}
								//DrawLine( "BotLine"+CurrentBar,  pivLowBar[0]+(strength+1) - pivLowBar[1], pivLowPrice[1], 
								//strength+1, pivLowPrice[0], Color.Green);
						}
					if( pivLowPrice[0] < pivLowPrice[1] )	
						{
							HPL_Counter = 0;
						}
			}
			
		//******************************************		Swing High - Lower High	************************************************			
		if( CurrentBar > strength + 1 )
		if (Swing(High, 3).SwingHigh[0] == High[strength + 1])
            {
				
				// Update Bar Array
					pivHighBar[2] = pivHighBar[1];
					pivHighBar[1] = pivHighBar[0];
					pivHighBar[0] = CurrentBar- (strength+1);
				// update price array
					pivHighPrice[2] = pivHighPrice[1];
					pivHighPrice[1] = pivHighPrice[0];
					pivHighPrice[0] =  High[strength + 1];
					// mark Lower pivot High -- Top
					if( pivHighPrice[0] < pivHighPrice[1] 	)	//
						{
							if( LPH_Counter == 0 )
								Draw.Dot(this, "swingH"+ CurrentBar.ToString(), true, strength + 1, High[strength + 1] + TickSize, Brushes.Crimson);
							LPH_Counter = LPH_Counter +1;
							//HPL_Counter = 0;
							//if( LPH_Counter > 2 )
							//	DrawText("lphc"+CurrentBar, LPH_Counter.ToString(), strength+1, pivHighPrice[0]+TickSize , Color.Red);
							if( LPH_Counter == 2 )
								{
								//DrawArrowDown("lph"+CurrentBar, 0, pivHighPrice[0]+TickSize, Color.Red);
								ArrowDown myArrowDn = Draw.ArrowDown(this, "lph"+CurrentBar.ToString(), true, 0, pivHighPrice[0]+TickSize, Brushes.Crimson);
								myArrowDn.OutlineBrush =  Brushes.Red;
								//DrawText("lphc"+CurrentBar, "_____", 2, pivHighPrice[0] , Color.Red);
								Draw.Text(this, "lphc"+CurrentBar.ToString(), "_____", 2, pivHighPrice[0] , Brushes.Red);
								}
								//DrawLine( "TopLine"+CurrentBar,  pivHighBar[0]+(strength+1) - pivHighBar[1], pivHighPrice[1], 
								//strength+1, pivHighPrice[0], Color.DarkRed);
						}
					if(  pivHighPrice[0] > pivHighPrice[1] )	
						{
							LPH_Counter = 0;
						}
			}
							

        }

        #region Properties

        #endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private HigherLow[] cacheHigherLow;
		public HigherLow HigherLow()
		{
			return HigherLow(Input);
		}

		public HigherLow HigherLow(ISeries<double> input)
		{
			if (cacheHigherLow != null)
				for (int idx = 0; idx < cacheHigherLow.Length; idx++)
					if (cacheHigherLow[idx] != null &&  cacheHigherLow[idx].EqualsInput(input))
						return cacheHigherLow[idx];
			return CacheIndicator<HigherLow>(new HigherLow(), input, ref cacheHigherLow);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.HigherLow HigherLow()
		{
			return indicator.HigherLow(Input);
		}

		public Indicators.HigherLow HigherLow(ISeries<double> input )
		{
			return indicator.HigherLow(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.HigherLow HigherLow()
		{
			return indicator.HigherLow(Input);
		}

		public Indicators.HigherLow HigherLow(ISeries<double> input )
		{
			return indicator.HigherLow(input);
		}
	}
}

#endregion
