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
	public class SendMail : Indicator
	{
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "SendMail";
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
				SendMailOn					= true;
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			
		}

		protected override void OnBarUpdate()
		{
			
			string messageBody = "On " + Time[0].ToShortDateString() + " at " + Time[0].ToShortTimeString() + " A Long Entry on " + Instrument.MasterInstrument.Name + " was generated at " + Close[0];
			
			string messageTitle = "Long Entry On " + Instrument.MasterInstrument.Name;

			Share("Hotmail", messageBody, new object[]{ "whansen1@mac.com", messageTitle, @"C:\Users\MBPtrader\Pictures\EURUSD_Opt_6_27.PNG"});
			
			
			/*
			Share(string serviceName, string message)
			Share(string serviceName, string message, object[] args)
			Share(string serviceName, string message, string screenshotPath)
			Share(string serviceName, string message, string screenshotPath, object[] args)
			*/
			
			//Share("Hotmail", messageBody, @"C:\Users\MBPtrader\Pictures\EURUSD_Opt_6_27.PNG");
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="SendMailOn", Order=1, GroupName="Parameters")]
		public bool SendMailOn
		{ get; set; }
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private SendMail[] cacheSendMail;
		public SendMail SendMail(bool sendMailOn)
		{
			return SendMail(Input, sendMailOn);
		}

		public SendMail SendMail(ISeries<double> input, bool sendMailOn)
		{
			if (cacheSendMail != null)
				for (int idx = 0; idx < cacheSendMail.Length; idx++)
					if (cacheSendMail[idx] != null && cacheSendMail[idx].SendMailOn == sendMailOn && cacheSendMail[idx].EqualsInput(input))
						return cacheSendMail[idx];
			return CacheIndicator<SendMail>(new SendMail(){ SendMailOn = sendMailOn }, input, ref cacheSendMail);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.SendMail SendMail(bool sendMailOn)
		{
			return indicator.SendMail(Input, sendMailOn);
		}

		public Indicators.SendMail SendMail(ISeries<double> input , bool sendMailOn)
		{
			return indicator.SendMail(input, sendMailOn);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.SendMail SendMail(bool sendMailOn)
		{
			return indicator.SendMail(Input, sendMailOn);
		}

		public Indicators.SendMail SendMail(ISeries<double> input , bool sendMailOn)
		{
			return indicator.SendMail(input, sendMailOn);
		}
	}
}

#endregion
