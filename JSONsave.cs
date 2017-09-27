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
using System.Windows.Forms;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{	
	public class JSONsave : Indicator
	{
		
		private string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "JSON Save";
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
				
			}
			else if(State == State.DataLoaded)
			{
				  ClearOutputWindow(); 
			}
		}

		protected override void OnBarUpdate()
		{
			checkForDirectory();
			createCSV();
		}
		
		private void checkForDirectory() {

			/// check to see if Firebase Dir exists
			bool folderExists = Directory.Exists(systemPath+ @"\Firebase");
			Print("\npath to documents: " + systemPath + " Does Firebase folder exists? " + folderExists);
		
			/// if not create the directory
			if (!folderExists) {
				Print("creating directory... " + systemPath+ @"\Firebase" );
				Directory.CreateDirectory(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Firebase"));
			} else {
				Print("found diretory... " + systemPath+ @"\Firebase");
			}
		}
		
		private void createCSV() {
			
			var filePath = systemPath+ @"\Firebase\PriceData.csv";
			Print("writing file... " + filePath);
			
			using (StreamWriter writer = new StreamWriter(filePath, true))
			{
				var newLine =  Time[0].ToString() + ", " + Open[0].ToString("0.00") + ", " + High[0].ToString("0.00") 
					+ ", " + Low[0].ToString("0.00") + ", " + Close[0].ToString("0.00");
		
				writer.WriteLine(newLine);
	
				writer.Dispose();
			}
		}

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private JSONsave[] cacheJSONsave;
		public JSONsave JSONsave()
		{
			return JSONsave(Input);
		}

		public JSONsave JSONsave(ISeries<double> input)
		{
			if (cacheJSONsave != null)
				for (int idx = 0; idx < cacheJSONsave.Length; idx++)
					if (cacheJSONsave[idx] != null &&  cacheJSONsave[idx].EqualsInput(input))
						return cacheJSONsave[idx];
			return CacheIndicator<JSONsave>(new JSONsave(), input, ref cacheJSONsave);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.JSONsave JSONsave()
		{
			return indicator.JSONsave(Input);
		}

		public Indicators.JSONsave JSONsave(ISeries<double> input )
		{
			return indicator.JSONsave(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.JSONsave JSONsave()
		{
			return indicator.JSONsave(Input);
		}

		public Indicators.JSONsave JSONsave(ISeries<double> input )
		{
			return indicator.JSONsave(input);
		}
	}
}

#endregion
