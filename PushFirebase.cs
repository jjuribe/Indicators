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

using System.Net;
using System.Net.Cache;
using System.Web.Script.Serialization;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{	
	public class PushFirebase : Indicator
	{
		private struct PriceData
		{
			public  string 	date 	{ get; set; }
			public  string 	ticker 	{ get; set; }
			public  string 	bartype 	{ get; set; }
			public	double 	open	{ get; set; }
			public	double 	high 	{ get; set; }
			public	double 	low		{ get; set; }
			public  double 	close	{ get; set; }
			public	double 	signal	{ get; set; }
			public	int 	trade	{ get; set; }

		}
		
		private PriceData priceData = new PriceData{};
	
		private List<PriceData> myList = new List<PriceData>();

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description									= @"Enter the description for your new custom Indicator here.";
				Name										= "Push Firebase";
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

			// create date
			priceData.date	= Time[0].ToString();
			priceData.open	= Open[0];
			priceData.high 	= High[0];
			priceData.low	=	Low[0];
			priceData.close	=	Close[0];
			priceData.signal = High[0];
			priceData.trade  =	0;
			priceData.ticker =  Instrument.MasterInstrument.Name;
			priceData.bartype	=	BarsPeriod.Value.ToString() + " " + BarsPeriod.BarsPeriodType.ToString();
			
			//add to array
			myList.Add(priceData);
			
			// serialize
			string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(myList);
			//Print(json);
	
			// post to firebase
			var jsonDataset = json;
			var myFirebase = "https://mtdash01.firebaseio.com/.json";
            var request = WebRequest.CreateHttp(myFirebase);
			request.Method = "PUT";		// put wrote over
            byte[] byteArray = Encoding.UTF8.GetBytes(jsonDataset);
            request.ContentType = "application/json";
            request.ContentLength = byteArray.Length;
			Stream dataStream = request.GetRequestStream();
            dataStream.Write(byteArray, 0, byteArray.Length);
            dataStream.Close();
            WebResponse response = request.GetResponse();
			dataStream = response.GetResponseStream();
            StreamReader reader = new StreamReader(dataStream);
            string responseFromServer = reader.ReadToEnd();
			reader.Close();
            dataStream.Close();
            response.Close();
            request.Abort();
			
		}
		


	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private PushFirebase[] cachePushFirebase;
		public PushFirebase PushFirebase()
		{
			return PushFirebase(Input);
		}

		public PushFirebase PushFirebase(ISeries<double> input)
		{
			if (cachePushFirebase != null)
				for (int idx = 0; idx < cachePushFirebase.Length; idx++)
					if (cachePushFirebase[idx] != null &&  cachePushFirebase[idx].EqualsInput(input))
						return cachePushFirebase[idx];
			return CacheIndicator<PushFirebase>(new PushFirebase(), input, ref cachePushFirebase);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.PushFirebase PushFirebase()
		{
			return indicator.PushFirebase(Input);
		}

		public Indicators.PushFirebase PushFirebase(ISeries<double> input )
		{
			return indicator.PushFirebase(input);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.PushFirebase PushFirebase()
		{
			return indicator.PushFirebase(Input);
		}

		public Indicators.PushFirebase PushFirebase(ISeries<double> input )
		{
			return indicator.PushFirebase(input);
		}
	}
}

#endregion
