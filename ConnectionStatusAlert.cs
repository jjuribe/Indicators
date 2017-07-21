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
using System.IO;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class ConnectionStatusAlert : Indicator
	{
		string messageToDisplay = "no message yet";
		int signal = 0;
		/// a variable for the StreamWriter that will be used 
		private StreamWriter sw; 
		
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
				ComputerName								= "MBP";
				Path	=	@"C:\Users\MBPtrader\Documents\NT_CSV\connected.csv";
				AddPlot(Brushes.Red, "ConnetionUp");
			}
			else if (State == State.Configure)
			{
			} 
			// Necessary to call in order to clean up resources used by the StreamWriter object
			else if(State == State.Terminated)
			{
				if (sw != null)
				{
					sw.Close();
					sw.Dispose();
					sw = null;
				}
			}
		}

		protected override void OnConnectionStatusUpdate(ConnectionStatusEventArgs connectionStatusUpdate)
		{
			if(connectionStatusUpdate.Status == ConnectionStatus.Connected)
			  {
				messageToDisplay = ComputerName+ " Connected at " + DateTime.Now;
			    Print(messageToDisplay);
				signal = 1;
				sendMessage(message: messageToDisplay);
				//appendConnectionFile(message: messageToDisplay);
			  }
			  
			  else if(connectionStatusUpdate.Status == ConnectionStatus.ConnectionLost)
			  {
				messageToDisplay = ComputerName+" Connection lost at: " + DateTime.Now;
				Print(messageToDisplay);
				signal = -1;
				sendMessage(message: messageToDisplay);
				//appendConnectionFile(message: messageToDisplay);
			  }
		}

		protected override void OnBarUpdate()
		{
			Draw.TextFixed(this, "tag1", messageToDisplay, TextPosition.BottomRight);
			ConnetionUp[0] = signal;
		}
		
		public void appendConnectionFile(string message) {
			try {
				// write connection to a file
				sw = File.AppendText(Path);  // Open the path for writing
				sw.WriteLine( Instrument.MasterInstrument.Name +", "+ Time[0] + ", " + Open[0] + ", " + High[0] + ", " + Low[0] + ", " + Close[0] + ", " + signal ); // Append a new line to the file
				sw.WriteLine(message);
				sw.Close(); // Close the file to allow future calls to access the file again
			}
			catch(IOException e) {
						Print(
				        "{0}: The write operation could not " +
				        "be performed because the specified " +
				        "part of the file is locked." + 
				        e.GetType().Name);
					}
		}
		
		public void sendMessage(string message) {
			string messageBody =  message;
			string messageTitle = message;
				
			try {
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
					catch(IOException e) {
						Print(
				        "{0}: The write Mail could not " +
				        "be performed because the specified " +
				        "part of the file is locked." + 
				        e.GetType().Name);
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
		
		[NinjaScriptProperty]
		[Display(Name="Computer Name", Order=3, GroupName="Parameters")]
		public string ComputerName
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="File Path", Order=4, GroupName="Parameters")]
		public string Path
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
		public ConnectionStatusAlert ConnectionStatusAlert(bool sendMail, bool sendSMS, string computerName, string path)
		{
			return ConnectionStatusAlert(Input, sendMail, sendSMS, computerName, path);
		}

		public ConnectionStatusAlert ConnectionStatusAlert(ISeries<double> input, bool sendMail, bool sendSMS, string computerName, string path)
		{
			if (cacheConnectionStatusAlert != null)
				for (int idx = 0; idx < cacheConnectionStatusAlert.Length; idx++)
					if (cacheConnectionStatusAlert[idx] != null && cacheConnectionStatusAlert[idx].SendMail == sendMail && cacheConnectionStatusAlert[idx].SendSMS == sendSMS && cacheConnectionStatusAlert[idx].ComputerName == computerName && cacheConnectionStatusAlert[idx].Path == path && cacheConnectionStatusAlert[idx].EqualsInput(input))
						return cacheConnectionStatusAlert[idx];
			return CacheIndicator<ConnectionStatusAlert>(new ConnectionStatusAlert(){ SendMail = sendMail, SendSMS = sendSMS, ComputerName = computerName, Path = path }, input, ref cacheConnectionStatusAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(bool sendMail, bool sendSMS, string computerName, string path)
		{
			return indicator.ConnectionStatusAlert(Input, sendMail, sendSMS, computerName, path);
		}

		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(ISeries<double> input , bool sendMail, bool sendSMS, string computerName, string path)
		{
			return indicator.ConnectionStatusAlert(input, sendMail, sendSMS, computerName, path);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(bool sendMail, bool sendSMS, string computerName, string path)
		{
			return indicator.ConnectionStatusAlert(Input, sendMail, sendSMS, computerName, path);
		}

		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(ISeries<double> input , bool sendMail, bool sendSMS, string computerName, string path)
		{
			return indicator.ConnectionStatusAlert(input, sendMail, sendSMS, computerName, path);
		}
	}
}

#endregion
