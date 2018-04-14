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
	public class TextOnScreen : Indicator
	{
		
		TextPosition position = TextPosition.TopLeft;
		
		protected override void OnStateChange()
		{
			
			
			
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Text On Screen";
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
				Text					= @"Trade Rules";
				Position				= 1;
				ColorForText			= Brushes.LightGray;
				Opacity = 10;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnBarUpdate()
		{
			
			switch (Position) {
				case 1:
					position = TextPosition.TopLeft;
					break;
				case 2:
					position = TextPosition.TopRight;
					break;
				case 3:
					position = TextPosition.BottomLeft;
					break;
				case 4:
					position = TextPosition.BottomRight;
					break;
				case 5:
					position = TextPosition.Center;
					break;
				default:
					position = TextPosition.TopLeft;
					break;
			}
			
			Draw.TextFixed(this, "myTextFixed", 
				"\nTrade Rules\n\n1. DCE Close to apex\n2. CCI Signal\n3. Heinkin Ashi reverse color\n\nDiscretion\n1. DCE will loose sync and flatten out\n    This will require a judgement call\n2. No entry if candle tail != color change\n", 
				position, ColorForText, 
  				ChartControl.Properties.LabelFont, Brushes.Gray, Brushes.Transparent, Opacity);
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="Text", Order=1, GroupName="Parameters")]
		public string Text
		{ get; set; }

		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Position", Description="1. Top Left, 2. Top Right, 3. Bottom Left, 4. Bottom Right", Order=2, GroupName="Parameters")]
		public int Position
		{ get; set; }

		[NinjaScriptProperty]
		[XmlIgnore]
		[Display(Name="ColorForText", Order=3, GroupName="Parameters")]
		public Brush ColorForText
		{ get; set; }

		[Browsable(false)]
		public string ColorForTextSerializable
		{
			get { return Serialize.BrushToString(ColorForText); }
			set { ColorForText = Serialize.StringToBrush(value); }
		}			
		
		[NinjaScriptProperty]
		[Range(1, int.MaxValue)]
		[Display(Name="Opacity", Description="1 to 10", Order=2, GroupName="Parameters")]
		public int Opacity
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TextOnScreen[] cacheTextOnScreen;
		public TextOnScreen TextOnScreen(string text, int position, Brush colorForText, int opacity)
		{
			return TextOnScreen(Input, text, position, colorForText, opacity);
		}

		public TextOnScreen TextOnScreen(ISeries<double> input, string text, int position, Brush colorForText, int opacity)
		{
			if (cacheTextOnScreen != null)
				for (int idx = 0; idx < cacheTextOnScreen.Length; idx++)
					if (cacheTextOnScreen[idx] != null && cacheTextOnScreen[idx].Text == text && cacheTextOnScreen[idx].Position == position && cacheTextOnScreen[idx].ColorForText == colorForText && cacheTextOnScreen[idx].Opacity == opacity && cacheTextOnScreen[idx].EqualsInput(input))
						return cacheTextOnScreen[idx];
			return CacheIndicator<TextOnScreen>(new TextOnScreen(){ Text = text, Position = position, ColorForText = colorForText, Opacity = opacity }, input, ref cacheTextOnScreen);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TextOnScreen TextOnScreen(string text, int position, Brush colorForText, int opacity)
		{
			return indicator.TextOnScreen(Input, text, position, colorForText, opacity);
		}

		public Indicators.TextOnScreen TextOnScreen(ISeries<double> input , string text, int position, Brush colorForText, int opacity)
		{
			return indicator.TextOnScreen(input, text, position, colorForText, opacity);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TextOnScreen TextOnScreen(string text, int position, Brush colorForText, int opacity)
		{
			return indicator.TextOnScreen(Input, text, position, colorForText, opacity);
		}

		public Indicators.TextOnScreen TextOnScreen(ISeries<double> input , string text, int position, Brush colorForText, int opacity)
		{
			return indicator.TextOnScreen(input, text, position, colorForText, opacity);
		}
	}
}

#endregion
