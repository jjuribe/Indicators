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
	public class FiveDaysDown : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "FiveDaysDown";
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
				SendToFireBase					= false;
				Risk					= 100;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			if (Close[0] > SMA(200)[0] 
				&& Low[0] > Low[1]
				&& Low[1] < Low[2]
				&& Low[2] < Low[3]
				&& Low[3] < Low[4]
				&& Low[4] < Low[5]
				) {
				Draw.ArrowUp(this, "MyArrowUp"+CurrentBar.ToString(), false, 0, Low[0]- ( TickSize * 20), Brushes.LimeGreen);
				
			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="SendToFireBase", Order=1, GroupName="Parameters")]
		public bool SendToFireBase
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Risk", Order=2, GroupName="Parameters")]
		public int Risk
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private FiveDaysDown[] cacheFiveDaysDown;
		public FiveDaysDown FiveDaysDown(bool sendToFireBase, int risk)
		{
			return FiveDaysDown(Input, sendToFireBase, risk);
		}

		public FiveDaysDown FiveDaysDown(ISeries<double> input, bool sendToFireBase, int risk)
		{
			if (cacheFiveDaysDown != null)
				for (int idx = 0; idx < cacheFiveDaysDown.Length; idx++)
					if (cacheFiveDaysDown[idx] != null && cacheFiveDaysDown[idx].SendToFireBase == sendToFireBase && cacheFiveDaysDown[idx].Risk == risk && cacheFiveDaysDown[idx].EqualsInput(input))
						return cacheFiveDaysDown[idx];
			return CacheIndicator<FiveDaysDown>(new FiveDaysDown(){ SendToFireBase = sendToFireBase, Risk = risk }, input, ref cacheFiveDaysDown);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.FiveDaysDown FiveDaysDown(bool sendToFireBase, int risk)
		{
			return indicator.FiveDaysDown(Input, sendToFireBase, risk);
		}

		public Indicators.FiveDaysDown FiveDaysDown(ISeries<double> input , bool sendToFireBase, int risk)
		{
			return indicator.FiveDaysDown(input, sendToFireBase, risk);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.FiveDaysDown FiveDaysDown(bool sendToFireBase, int risk)
		{
			return indicator.FiveDaysDown(Input, sendToFireBase, risk);
		}

		public Indicators.FiveDaysDown FiveDaysDown(ISeries<double> input , bool sendToFireBase, int risk)
		{
			return indicator.FiveDaysDown(input, sendToFireBase, risk);
		}
	}
}

#endregion
