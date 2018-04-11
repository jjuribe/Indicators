// 
// Copyright (C) 2017, WhenToTrade.com
// www.whentotrade.com
// Info: You must use a valid WTT API key to run this script
// Book: Decoding The Hidden Market Rhythm - Part 1 (2017)
// Chapter 6: Dynamic Cycle Explorer
// Link: https://www.amazon.com/dp/1974658244
//
// License: 
// This work is licensed under a Creative Commons Attribution 4.0 International License.
// You are free to share the material in any medium or format and remix, transform, and build upon the material for any purpose, 
// even commercially. You must give appropriate credit to the authors book and website, provide a link to the license, and indicate 
// if changes were made. You may do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use. 
//

#region Declarations
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Serialization;

using NinjaTrader.Cbi;
using NinjaTrader.Gui;
using NinjaTrader.Gui.Chart;
using NinjaTrader.Gui.SuperDom;
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.NinjaScript.DrawingTools;
using Newtonsoft.Json;
#endregion

// This namespace holds all indicators and is required. Do not change it.
namespace NinjaTrader.NinjaScript.Indicators
{
		//using Newtonsoft.Json; // Additional reference to package needed
		    
		public class WTT_CycleExplorer : Indicator
		{		
		    //debugnew
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				DrawOnPricePanel = false; // Draw objects now paint on the indicator panel itself  
				
				// Adds a plot to our NinjaScript Indicator
		        AddPlot(new Stroke(Brushes.Red, 1), PlotStyle.Line, "Dominant Cycle");
		        AddPlot(new Stroke(Brushes.Orange, 1), PlotStyle.Line, "Price Cycle");

				plotForward				= 0;
		        minCycleLength			= 30;
				maxCycleLength			= 290;
				apiKey					= "wttpreview";
				Name					= "WTT Cycle Explorer";
		        Calculate				= Calculate.OnBarClose;
		        IsOverlay				= false;
			}
			
			else if (State == State.DataLoaded)
			{
				if (plotForward>0) Displacement = plotForward;
			} 
		       
		}

		protected override void OnBarUpdate()
		{
		   	                		
			if (Count - 2 == CurrentBar) //Calculate cycle on last finished, closed bar
			{
					string APIresultContent="";
				
					//Get and prepare the close data array
		            int seriesCount = Close.Count - 1;
		            List<double> closeList = new List<double>();
		            for (int i = 0; i <= seriesCount; i++)
		                closeList.Add(Close.GetValueAt(i));
											
				    //Call the WhenToTrade Cycles API endpoint POST CycleExplorer
					using (var client = new HttpClient())
		            {
						//WhenToTrade API URL
		                client.BaseAddress = new Uri("https://api.marketcycles.online/");			

						//Load the close dataset for HTTP API POST call
		                var contents = JsonConvert.SerializeObject(closeList.ToArray());
		                StringContent POSTcontent = new StringContent(contents, Encoding.UTF8, "application/json");

						//POST API CALL
		                var response = client.PostAsync("/api/CycleExplorer?minCycleLength="+minCycleLength+"&maxCycleLength="+maxCycleLength+"&plotForward="+plotForward+"&includeTimeseries=true&api_Key="+apiKey, POSTcontent).Result;			
						
						//Check for network errors
						if( response.IsSuccessStatusCode)
					    {
					        APIresultContent = response.Content.ReadAsStringAsync().Result;
					    }
					    else
					    {
					        string msg = response.Content.ReadAsStringAsync().Result;
					        Draw.TextFixed(this, "apierror", "API ERROR:"+msg, TextPosition.TopLeft);
							return;
					    }
		            }
				
					// Decode the cycle data from API call
					var dominantCycle = JsonConvert.DeserializeObject<dynamic>(APIresultContent);
					
					// Test for return errors from API?
					string statusCode=dominantCycle.StatusCode !=null ? dominantCycle.StatusCode : "";
					bool error = statusCode.IndexOf("error", StringComparison.OrdinalIgnoreCase) >= 0;
					if (error)
					{
						Draw.TextFixed(this, "apierror", "API ERROR:"+statusCode, TextPosition.TopLeft);
						return;
					}
					
					//get key cycle information from return object
		            var cyclelength = dominantCycle.length;
		            var currentPrice = dominantCycle.currentPrice;
					var timeSeries = dominantCycle.TimeSeries;
									
					// DEBUG: Print return values to log
					Print(dominantCycle);					
				
					// Fill cycle indicator values from timeSeries API return array (info: called just once for the full chart)
					for (int i = 0; i <= CurrentBar; i++)
					{
						try {
		                	Value[CurrentBar-i]=timeSeries[i+plotForward].dominantCycle;
							
							if (timeSeries[i+plotForward].cycleHighlighter!=null)
								Values[1][CurrentBar-i]=timeSeries[i+plotForward].cycleHighlighter;
						} catch (Exception ex) 
						{
							//should never happen, but in case lets ignore them and only print to log
							Print("Error: "+i+" - "+ex.Message);
						}
					}
					
					// Plot Dominant Cycle Information
					TextFixed myTF = Draw.TextFixed(this, "dctext", "Cycle Length: "+cyclelength.ToString("0")+Environment.NewLine+"Next Low: "+dominantCycle.nextlow.ToString("0.00")+Environment.NewLine+"Next Top: "+dominantCycle.nexttop.ToString("0.00")+Environment.NewLine+"Status: "+dominantCycle.phase_status, TextPosition.TopLeft);

					// Plot the dot at current bar cycle position
					double doty=timeSeries[CurrentBar].dominantCycle;				
					NinjaTrader.Gui.Tools.SimpleFont myFont = new NinjaTrader.Gui.Tools.SimpleFont("Wingdings", 12) { Size = 12 };				
					Draw.Text(this, "dot", true, "l", 0, doty, 0, Brushes.Green, myFont, TextAlignment.Center, Brushes.Transparent, Brushes.Transparent, 0);
					
			}		
		}
        
        #region Properties
		
		[Range(10, 291)]
		[NinjaScriptProperty]
		[Display(Name="minCycleLength", Description="Minimum cycle length range", Order=1, GroupName="Parameters")]
		public int minCycleLength
		{ get; set; }
		
		[Range(10, 291)]
		[NinjaScriptProperty]
		[Display(Name="maxCycleLength", Description="Maximzm cycle length range", Order=2, GroupName="Parameters")]
		public int maxCycleLength
		{ get; set; }
		
		[Range(0, 200)]
		[NinjaScriptProperty]
		[Display(Name="plotForward", Description="Cycle plot into the future", Order=1, GroupName="Parameters")]
		public int plotForward
		{ get; set; }
		
		//[NinjaScriptProperty]
		[Display(Name="API KEY", Description="WTT API key", Order=3, GroupName="WhenToTrade")]
		public string apiKey
		{ get; set; }
		
		#endregion
    }
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WTT_CycleExplorer[] cacheWTT_CycleExplorer;
		public WTT_CycleExplorer WTT_CycleExplorer(int minCycleLength, int maxCycleLength, int plotForward)
		{
			return WTT_CycleExplorer(Input, minCycleLength, maxCycleLength, plotForward);
		}

		public WTT_CycleExplorer WTT_CycleExplorer(ISeries<double> input, int minCycleLength, int maxCycleLength, int plotForward)
		{
			if (cacheWTT_CycleExplorer != null)
				for (int idx = 0; idx < cacheWTT_CycleExplorer.Length; idx++)
					if (cacheWTT_CycleExplorer[idx] != null && cacheWTT_CycleExplorer[idx].minCycleLength == minCycleLength && cacheWTT_CycleExplorer[idx].maxCycleLength == maxCycleLength && cacheWTT_CycleExplorer[idx].plotForward == plotForward && cacheWTT_CycleExplorer[idx].EqualsInput(input))
						return cacheWTT_CycleExplorer[idx];
			return CacheIndicator<WTT_CycleExplorer>(new WTT_CycleExplorer(){ minCycleLength = minCycleLength, maxCycleLength = maxCycleLength, plotForward = plotForward }, input, ref cacheWTT_CycleExplorer);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WTT_CycleExplorer WTT_CycleExplorer(int minCycleLength, int maxCycleLength, int plotForward)
		{
			return indicator.WTT_CycleExplorer(Input, minCycleLength, maxCycleLength, plotForward);
		}

		public Indicators.WTT_CycleExplorer WTT_CycleExplorer(ISeries<double> input , int minCycleLength, int maxCycleLength, int plotForward)
		{
			return indicator.WTT_CycleExplorer(input, minCycleLength, maxCycleLength, plotForward);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WTT_CycleExplorer WTT_CycleExplorer(int minCycleLength, int maxCycleLength, int plotForward)
		{
			return indicator.WTT_CycleExplorer(Input, minCycleLength, maxCycleLength, plotForward);
		}

		public Indicators.WTT_CycleExplorer WTT_CycleExplorer(ISeries<double> input , int minCycleLength, int maxCycleLength, int plotForward)
		{
			return indicator.WTT_CycleExplorer(input, minCycleLength, maxCycleLength, plotForward);
		}
	}
}

#endregion
