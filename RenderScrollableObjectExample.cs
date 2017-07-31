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
	public class RenderScrollableObjectExample : Indicator
	{
		private SharpDX.Direct2D1.Brush		brushDx;
		private Point						endPoint;
		private int							lastBarNum;
		private double						lastPrice;
		private Point						startPoint;

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "RenderScrollableObjectExample";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= true;
				DisplayInDataBox							= false;
				IsSuspendedWhileInactive					= true;
				BarsRequiredToPlot							= 1;
			}
			else if (State == State.DataLoaded)
			{
				lastBarNum = 0;
			}
		}

		protected override void OnBarUpdate()
		{
			if (State == State.Historical && Count > 10 && CurrentBar == Count - 2)
			{
				lastBarNum	= CurrentBar;
				lastPrice	= Close[0];
			}
		}

		public override void OnRenderTargetChanged()
		{
			if (brushDx != null)
				brushDx.Dispose();

			if (RenderTarget != null)
			{
				try
				{
					brushDx = Brushes.Blue.ToDxBrush(RenderTarget);
				}
				catch (Exception e) { }
			}
		}

		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if (!IsVisible)
				return;

			if (!IsInHitTest && lastBarNum > 0)
			{
				// start point is 10 bars back and 5 ticks up from 2nd to last bar and price
				startPoint		= new Point(ChartControl.GetXByBarIndex(ChartBars, (lastBarNum - 10)), chartScale.GetYByValue(lastPrice + 5 * TickSize));
				// end point is 2nd to last bar and 5 ticks down from price
				endPoint		= new Point(ChartControl.GetXByBarIndex(ChartBars, (lastBarNum)), chartScale.GetYByValue(lastPrice - 5 * TickSize));

				SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;

				RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
				RenderTarget.DrawLine(startPoint.ToVector2(), endPoint.ToVector2(), brushDx, 15);

				RenderTarget.AntialiasMode = oldAntialiasMode;
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
		private RenderScrollableObjectExample[] cacheRenderScrollableObjectExample;
		public RenderScrollableObjectExample RenderScrollableObjectExample()
		{
			return RenderScrollableObjectExample(Input);
		}

		public RenderScrollableObjectExample RenderScrollableObjectExample(ISeries<double> input)
		{
			if (cacheRenderScrollableObjectExample != null)
				for (int idx = 0; idx < cacheRenderScrollableObjectExample.Length; idx++)
					if (cacheRenderScrollableObjectExample[idx] != null &&  cacheRenderScrollableObjectExample[idx].EqualsInput(input))
						return cacheRenderScrollableObjectExample[idx];
			return CacheIndicator<RenderScrollableObjectExample>(new RenderScrollableObjectExample(), input, ref cacheRenderScrollableObjectExample);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.RenderScrollableObjectExample RenderScrollableObjectExample()
		{
			return indicator.RenderScrollableObjectExample(Input);
		}

		public Indicators.RenderScrollableObjectExample RenderScrollableObjectExample(ISeries<double> input )
		{
			return indicator.RenderScrollableObjectExample(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.RenderScrollableObjectExample RenderScrollableObjectExample()
		{
			return indicator.RenderScrollableObjectExample(Input);
		}

		public Indicators.RenderScrollableObjectExample RenderScrollableObjectExample(ISeries<double> input )
		{
			return indicator.RenderScrollableObjectExample(input);
		}
	}
}

#endregion
