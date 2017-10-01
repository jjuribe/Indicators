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
	public class CSVsave : Indicator
	{
		
		private string systemPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "CSV Save";
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
		private CSVsave[] cacheCSVsave;
		public CSVsave CSVsave()
		{
			return CSVsave(Input);
		}

		public CSVsave CSVsave(ISeries<double> input)
		{
			if (cacheCSVsave != null)
				for (int idx = 0; idx < cacheCSVsave.Length; idx++)
					if (cacheCSVsave[idx] != null &&  cacheCSVsave[idx].EqualsInput(input))
						return cacheCSVsave[idx];
			return CacheIndicator<CSVsave>(new CSVsave(), input, ref cacheCSVsave);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.CSVsave CSVsave()
		{
			return indicator.CSVsave(Input);
		}

		public Indicators.CSVsave CSVsave(ISeries<double> input )
		{
			return indicator.CSVsave(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.CSVsave CSVsave()
		{
			return indicator.CSVsave(Input);
		}

		public Indicators.CSVsave CSVsave(ISeries<double> input )
		{
			return indicator.CSVsave(input);
		}
	}
}

#endregion
