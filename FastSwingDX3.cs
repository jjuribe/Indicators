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
	public class FastSwingDX3 : Indicator
	{
		private FastPivotFinder			FastPivotFinder1;
		
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
		  /// get the starting and ending bars from what is rendered on the chart
		  float startX = chartControl.GetXByBarIndex(ChartBars,  ChartBars.FromIndex); //(int)FastPivotFinder1.ExposedVariable ); //
		  float endX = chartControl.GetXByBarIndex(ChartBars, ChartBars.ToIndex);
		 
		  /// Loop through each Plot Values on the chart
		  for (int seriesCount = 0; seriesCount < Values.Length; seriesCount++)
		  {
		    /// get the value at the last bar on the chart (if it has been set)
		    if (Values[seriesCount].IsValidDataPointAt(ChartBars.ToIndex))
		    {
		        double plotValue = Values[seriesCount].GetValueAt(ChartBars.ToIndex);
		 
		        /// convert the plot value to the charts "Y" axis point
		        float chartScaleYValue = chartScale.GetYByValue(plotValue);
		 
		        /// calculate the x and y values for the line to start and end
		        SharpDX.Vector2 startPoint = new SharpDX.Vector2(startX, chartScaleYValue);
		        SharpDX.Vector2 endPoint = new SharpDX.Vector2(endX, chartScaleYValue);
		
		        /// draw a line between the start and end point at each plot using the plots SharpDX Brush color and style
		        RenderTarget.DrawLine(startPoint, endPoint, Plots[seriesCount].BrushDX,
		          Plots[seriesCount].Width, Plots[seriesCount].StrokeStyle);
		 
		        /// use the chart control text form to draw plot values along the line
		        SharpDX.DirectWrite.TextFormat textFormat = chartControl.Properties.LabelFont.ToDirectWriteTextFormat();
		 
		        /// calculate the which will be rendered at each plot using it the plot name and its price
		        string textToRender = Plots[seriesCount].Name + ": " + plotValue;
		 
		        /// calculate the layout of the text to be drawn
		        SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory,
		          textToRender, textFormat, 200, textFormat.FontSize);
		 
		        /// draw a line at each plot using the plots SharpDX Brush color at the calculated start point
		        RenderTarget.DrawTextLayout(startPoint, textLayout, Plots[seriesCount].BrushDX);
		 
		        /// dipose of the unmanaged resources used
		        textLayout.Dispose();
		        textFormat.Dispose();
		    }
		  }
		}


		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Fast Swing DX 3";
				Calculate									= Calculate.OnBarClose;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
			    IsOverlay 									= true;
				swingPct	 								= 0.2;
			    AddPlot(Brushes.DarkGray, "LastHigh");
			    AddPlot(Brushes.DarkGray, "LastLow");
			    AddPlot(Brushes.Crimson, "Short");
			    AddPlot(Brushes.DodgerBlue, "Long");
			}
			else if (State == State.Configure)
			{
			}
			else if(State == State.DataLoaded)
			  {
				  ClearOutputWindow();     
				  FastPivotFinder1 = FastPivotFinder(false, false, 70, swingPct, 1);	
			  } 
		}

		protected override void OnBarUpdate()
		{
			if( FastPivotFinder1.LastHigh[0] == 0 || FastPivotFinder1.LastLow[0] == 0) { return; }
				
			Values[0][0] = FastPivotFinder1.LastHigh[0];
			Values[1][0] = FastPivotFinder1.LastLow[0];
			//int lastH =  (int)FastPivotFinder1.ExposedVariable;
			/// short entryLine
			double swingDistance = Math.Abs(FastPivotFinder1.LastHigh[0]  - FastPivotFinder1.LastLow[0]);
			double entryValue = Math.Abs(swingDistance * 0.382);
			Values[2][0] = Math.Abs(FastPivotFinder1.LastHigh[0]  - entryValue);
			/// long entry line 
			Values[3][0] = Math.Abs( FastPivotFinder1.LastLow[0] + entryValue);
		
		}
				
		[NinjaScriptProperty]
		[Range(0, double.MaxValue)]
		[Display(Name="MinSwing Pct", Order=1, GroupName="Parameters")]
		public double swingPct
		{ get; set; }
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FastSwingDX3[] cacheFastSwingDX3;
		public FastSwingDX3 FastSwingDX3(double swingPct)
		{
			return FastSwingDX3(Input, swingPct);
		}

		public FastSwingDX3 FastSwingDX3(ISeries<double> input, double swingPct)
		{
			if (cacheFastSwingDX3 != null)
				for (int idx = 0; idx < cacheFastSwingDX3.Length; idx++)
					if (cacheFastSwingDX3[idx] != null && cacheFastSwingDX3[idx].swingPct == swingPct && cacheFastSwingDX3[idx].EqualsInput(input))
						return cacheFastSwingDX3[idx];
			return CacheIndicator<FastSwingDX3>(new FastSwingDX3(){ swingPct = swingPct }, input, ref cacheFastSwingDX3);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FastSwingDX3 FastSwingDX3(double swingPct)
		{
			return indicator.FastSwingDX3(Input, swingPct);
		}

		public Indicators.FastSwingDX3 FastSwingDX3(ISeries<double> input , double swingPct)
		{
			return indicator.FastSwingDX3(input, swingPct);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FastSwingDX3 FastSwingDX3(double swingPct)
		{
			return indicator.FastSwingDX3(Input, swingPct);
		}

		public Indicators.FastSwingDX3 FastSwingDX3(ISeries<double> input , double swingPct)
		{
			return indicator.FastSwingDX3(input, swingPct);
		}
	}
}

#endregion
