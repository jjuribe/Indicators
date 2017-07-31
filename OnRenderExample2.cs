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
	public class OnRenderExample2 : Indicator
	{
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
		  /// get the starting and ending bars from what is rendered on the chart
		  float startX = chartControl.GetXByBarIndex(ChartBars, ChartBars.FromIndex);
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
		 
//		        /// dipose of the unmanaged resources used
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
				Name										= "_OnRender Example 2";
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
			    IsOverlay = true;
			    AddPlot(Brushes.DarkKhaki, "Open");
			    AddPlot(Brushes.SeaGreen, "High");
			    AddPlot(Brushes.Crimson, "Low");
			    AddPlot(Brushes.DodgerBlue, "Close");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			Values[0][0] = Open[0];
			  Values[1][0] = High[0];
			  Values[2][0] = Low[0];
			  Values[3][0] = Close[0];
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private OnRenderExample2[] cacheOnRenderExample2;
		public OnRenderExample2 OnRenderExample2()
		{
			return OnRenderExample2(Input);
		}

		public OnRenderExample2 OnRenderExample2(ISeries<double> input)
		{
			if (cacheOnRenderExample2 != null)
				for (int idx = 0; idx < cacheOnRenderExample2.Length; idx++)
					if (cacheOnRenderExample2[idx] != null &&  cacheOnRenderExample2[idx].EqualsInput(input))
						return cacheOnRenderExample2[idx];
			return CacheIndicator<OnRenderExample2>(new OnRenderExample2(), input, ref cacheOnRenderExample2);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.OnRenderExample2 OnRenderExample2()
		{
			return indicator.OnRenderExample2(Input);
		}

		public Indicators.OnRenderExample2 OnRenderExample2(ISeries<double> input )
		{
			return indicator.OnRenderExample2(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.OnRenderExample2 OnRenderExample2()
		{
			return indicator.OnRenderExample2(Input);
		}

		public Indicators.OnRenderExample2 OnRenderExample2(ISeries<double> input )
		{
			return indicator.OnRenderExample2(input);
		}
	}
}

#endregion
