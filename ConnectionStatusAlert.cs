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
	public class ConnectionStatusAlert : Indicator
	{
		string messageToDisplay = "no message yet";
		int signal = 0;
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Connection Status Alert";
				Calculate									= Calculate.OnPriceChange;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= true;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive					= true;
				SendMail									= true;
				SendSMS										= true;
				AddPlot(Brushes.Red, "ConnetionUp");
			}
			else if (State == State.Configure)
			{
			}
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if(connectionStatusUpdate.Status == ConnectionStatus.Connected)
			  {
				string message = " Connected at " + DateTime.Now;
				messageToDisplay = message;
			    Print(message);
				signal = 1;
				sendMessage(message: messageToDisplay);
			  }
			  
			  else if(connectionStatusUpdate.Status == ConnectionStatus.ConnectionLost)
			  {
				string message = " Connection lost at: " + DateTime.Now;
				messageToDisplay = message;
				Print(message);
				signal = -1;
				sendMessage(message: messageToDisplay);
			  }
		}

		protected override void OnBarUpdate()
		{
			Draw.TextFixed(this, "tag1", messageToDisplay, TextPosition.TopRight);
			ConnetionUp[0] = signal;
		}
		
		public void sendMessage(string message) {
			string messageBody =  message;
			string messageTitle = message;
				
				// in order to send mail and SMS, you must delay each call
				if (IsFirstTickOfBar &&  State == State.Realtime)
				  {
				    // Instead of Thread.Sleep for, create a timer that runs at the desired interval
				    System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer {Interval = 5000};
				 
				    // queue the "after" logic to run when the timer elapses
				    timer.Tick += delegate
				    {
				        timer.Stop(); // make sure to stop the timer to only fire ones (if desired)
				        Print("Run SMS after: " + DateTime.Now);
						if( SendSMS )
							Share("EcoMail", messageBody, new object[]{ "3103824522@tmomail.net", messageTitle });
				        timer.Dispose(); // make sure to dispose of the timer
				    };
	 
				    Print("Run Mail before: " + DateTime.Now);
					if( SendMail )
				 		Share("EcoMail", messageBody, new object[]{ "whansen1@mac.com", messageTitle });
				    timer.Start(); // start the timer immediately following the "before" logic
	  			}
		}

		#region Properties
		[NinjaScriptProperty]
		[Display(Name="SendMail", Order=1, GroupName="Parameters")]
		public bool SendMail
		{ get; set; }

		[NinjaScriptProperty]
		[Display(Name="SendSMS", Order=2, GroupName="Parameters")]
		public bool SendSMS
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ConnetionUp
		{
			get { return Values[0]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ConnectionStatusAlert[] cacheConnectionStatusAlert;
		public ConnectionStatusAlert ConnectionStatusAlert(bool sendMail, bool sendSMS)
		{
			return ConnectionStatusAlert(Input, sendMail, sendSMS);
		}

		public ConnectionStatusAlert ConnectionStatusAlert(ISeries<double> input, bool sendMail, bool sendSMS)
		{
			if (cacheConnectionStatusAlert != null)
				for (int idx = 0; idx < cacheConnectionStatusAlert.Length; idx++)
					if (cacheConnectionStatusAlert[idx] != null && cacheConnectionStatusAlert[idx].SendMail == sendMail && cacheConnectionStatusAlert[idx].SendSMS == sendSMS && cacheConnectionStatusAlert[idx].EqualsInput(input))
						return cacheConnectionStatusAlert[idx];
			return CacheIndicator<ConnectionStatusAlert>(new ConnectionStatusAlert(){ SendMail = sendMail, SendSMS = sendSMS }, input, ref cacheConnectionStatusAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(bool sendMail, bool sendSMS)
		{
			return indicator.ConnectionStatusAlert(Input, sendMail, sendSMS);
		}

		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(ISeries<double> input , bool sendMail, bool sendSMS)
		{
			return indicator.ConnectionStatusAlert(input, sendMail, sendSMS);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(bool sendMail, bool sendSMS)
		{
			return indicator.ConnectionStatusAlert(Input, sendMail, sendSMS);
		}

		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(ISeries<double> input , bool sendMail, bool sendSMS)
		{
			return indicator.ConnectionStatusAlert(input, sendMail, sendSMS);
		}
	}
}

#endregion
