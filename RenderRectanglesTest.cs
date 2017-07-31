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
	public class RenderRectanglesTest : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "RenderRectanglesTest";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				IsSuspendedWhileInactive					= true;
			}
		}

		protected override void OnBarUpdate() {	}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			for (int index = ChartBars.FromIndex; index <= ChartBars.ToIndex; index++)
			{

				// gets the pixel coordinate of the bar index passed to the method - X axis
				float xStart = chartControl.GetXByBarIndex(ChartBars, index);

				// gets the pixel coordinate of the price value passed to the method - Y axis
				float yStart = chartScale.GetYByValue(High.GetValueAt(index) + 2 * TickSize);

				float width = (float)chartControl.BarWidth * 4;


				// construct the rectangleF struct to describe the position and size the drawing
				SharpDX.RectangleF rect = new SharpDX.RectangleF(xStart, yStart, width, width);

				//				// define the brush used in the rectangle
				SharpDX.Direct2D1.SolidColorBrush customDXBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Blue);

				SharpDX.Direct2D1.SolidColorBrush outlineBrush = new SharpDX.Direct2D1.SolidColorBrush(RenderTarget, SharpDX.Color.Black);


				// The RenderTarget consists of two commands related to Rectangles.
				// The FillRectangle() method is used to "Paint" the area of a Rectangle
				// execute the render target fill rectangle with desired values
				RenderTarget.FillRectangle(rect, customDXBrush);

				// and DrawRectangle() is used to "Paint" the outline of a Rectangle
				RenderTarget.DrawRectangle(rect, outlineBrush, 2); //Added WH 6/5/2017

				// always dispose of a brush when finished
				customDXBrush.Dispose();
				outlineBrush.Dispose();

			}

			base.OnRender(chartControl, chartScale);
		}
	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private RenderRectanglesTest[] cacheRenderRectanglesTest;
		public RenderRectanglesTest RenderRectanglesTest()
		{
			return RenderRectanglesTest(Input);
		}

		public RenderRectanglesTest RenderRectanglesTest(ISeries<double> input)
		{
			if (cacheRenderRectanglesTest != null)
				for (int idx = 0; idx < cacheRenderRectanglesTest.Length; idx++)
					if (cacheRenderRectanglesTest[idx] != null &&  cacheRenderRectanglesTest[idx].EqualsInput(input))
						return cacheRenderRectanglesTest[idx];
			return CacheIndicator<RenderRectanglesTest>(new RenderRectanglesTest(), input, ref cacheRenderRectanglesTest);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RenderRectanglesTest RenderRectanglesTest()
		{
			return indicator.RenderRectanglesTest(Input);
		}

		public Indicators.RenderRectanglesTest RenderRectanglesTest(ISeries<double> input )
		{
			return indicator.RenderRectanglesTest(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RenderRectanglesTest RenderRectanglesTest()
		{
			return indicator.RenderRectanglesTest(Input);
		}

		public Indicators.RenderRectanglesTest RenderRectanglesTest(ISeries<double> input )
		{
			return indicator.RenderRectanglesTest(input);
		}
	}
}

#endregion
