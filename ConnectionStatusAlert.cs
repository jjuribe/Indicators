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

		private StreamWriter sw; 
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Connection Status Alert";
				Calculate									= Calculate.OnPriceChange;
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
				ComputerName								= "MBP";
				Path	=	@"C:\Users\MBPtrader\Documents\NT_CSV\connected.csv";
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
				messageToDisplay = ComputerName+ " Connected at " + DateTime.Now + " " + Instrument.MasterInstrument.Name;
			    Print(messageToDisplay);
				appendConnectionFile(message: messageToDisplay);
			  }
			  
			  else if(connectionStatusUpdate.Status == ConnectionStatus.ConnectionLost)
			  {
				messageToDisplay = ComputerName+" Disconnected at " + DateTime.Now + " " + Instrument.MasterInstrument.Name;
				Print(messageToDisplay);
				appendConnectionFile(message: messageToDisplay);
			  }
			  
			  else if(connectionStatusUpdate.Status == ConnectionStatus.Disconnected)
			  {
				messageToDisplay = ComputerName+" Disconnected at " + DateTime.Now + " " + Instrument.MasterInstrument.Name;
				Print(messageToDisplay);
				appendConnectionFile(message: messageToDisplay);
			  }
		}

		protected override void OnBarUpdate()
		{
			Draw.TextFixed(this, "tag1", messageToDisplay, TextPosition.BottomRight);
		}
		
		public void appendConnectionFile(string message) {
			try {
				// write connection to a file
				sw = File.AppendText(Path);  // Open the path for writing
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

		#region Properties
		
		[NinjaScriptProperty]
		[Display(Name="Computer Name", Order=1, GroupName="Parameters")]
		public string ComputerName
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="File Path", Order=2, GroupName="Parameters")]
		public string Path
		{ get; set; }

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private ConnectionStatusAlert[] cacheConnectionStatusAlert;
		public ConnectionStatusAlert ConnectionStatusAlert(string computerName, string path)
		{
			return ConnectionStatusAlert(Input, computerName, path);
		}

		public ConnectionStatusAlert ConnectionStatusAlert(ISeries<double> input, string computerName, string path)
		{
			if (cacheConnectionStatusAlert != null)
				for (int idx = 0; idx < cacheConnectionStatusAlert.Length; idx++)
					if (cacheConnectionStatusAlert[idx] != null && cacheConnectionStatusAlert[idx].ComputerName == computerName && cacheConnectionStatusAlert[idx].Path == path && cacheConnectionStatusAlert[idx].EqualsInput(input))
						return cacheConnectionStatusAlert[idx];
			return CacheIndicator<ConnectionStatusAlert>(new ConnectionStatusAlert(){ ComputerName = computerName, Path = path }, input, ref cacheConnectionStatusAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(string computerName, string path)
		{
			return indicator.ConnectionStatusAlert(Input, computerName, path);
		}

		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(ISeries<double> input , string computerName, string path)
		{
			return indicator.ConnectionStatusAlert(input, computerName, path);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(string computerName, string path)
		{
			return indicator.ConnectionStatusAlert(Input, computerName, path);
		}

		public Indicators.ConnectionStatusAlert ConnectionStatusAlert(ISeries<double> input , string computerName, string path)
		{
			return indicator.ConnectionStatusAlert(input, computerName, path);
		}
	}
}

#endregion
