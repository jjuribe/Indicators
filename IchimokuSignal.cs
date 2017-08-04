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
using NinjaTrader.Data;
using NinjaTrader.NinjaScript;
using NinjaTrader.Core.FloatingPoint;
using NinjaTrader.Gui.Tools;
using NinjaTrader.NinjaScript.DrawingTools;
#endregion

//This namespace holds Indicators in this folder and is required. Do not change it. 
namespace NinjaTrader.NinjaScript.Indicators
{
	public class IchimokuSignal : Indicator
	{
		//	for Ichimoku Signals by kkc2015, see document from IchimokuTrader.com
		private const int icTrendSpan	= 4;	//	4 previous trading days, used for avoiding whipsaw signals
		private const int icNoSignal	= 0;
		private const int icBelowCloud	= 1;	//	signal occurred below the cloud
		private const int icInCloud		= 2;
		private const int icAboveCloud	= 3;
		private const int icSignal_Base	= 11;	//	use for adjusting icSignal_ to start from 0
		private const int icSignal_TK	= icSignal_Base+0;	//	TK = signal for Tenkan / Kijun sen crossed up
		private const int icSignal_PK	= icSignal_Base+1;	//	do not change number sequence from _TK to _CP
		private const int icSignal_KB	= icSignal_Base+2;
		private const int icSignal_SS	= icSignal_Base+3;
		private const int icSignal_CP	= icSignal_Base+4;	//	do not change number sequence from _TK to _CP
		private const int icSignal_LS	= icSignal_Base+5;	//	trade record [Long/Short]
		private const int icSignal_DU	= icSignal_Base+6;	//	Alert [Down / Up trend]
		private const int icSignalType	= 5;	//	exclude icSignal_LS
		private const int icReadTradeData = -2;	//	flag to show that ReadTradeData is completed, do not repeat
		private const int icMACD_data = -3;		//	flag to show that MACD data is completed, do not repeat
		private const int icMACD_Peak = 1;		//	check for MACD peaks
		private const int icMACD_Valley = 2;	//	check for MACD valleys
		private const bool bMACD_Completed = true;	//	used by MACD function
		
		private static readonly int[] icSignals = {icSignal_TK,icSignal_PK,icSignal_KB,icSignal_SS,icSignal_CP};
		private static readonly string[] scSignals = {"TK", "PK", "KB", "SS", "CP", "LS", "DU"};
		
		private const int icSignal_start	= 100;	//	use for adjusting TK_DN to start from 1
		private const int icSignal_TK_DN	= 101;	//	_DN = odd number = signal for Tenkan / Kijun sen crossed down
		private const int icSignal_TK_UP	= 102;	//	_UP = even number = signal for Tenkan / Kijun sen crossed up
		private const int icSignal_PK_DN	= 103;
		private const int icSignal_PK_UP	= 104;
		private const int icSignal_KB_DN	= 105;
		private const int icSignal_KB_UP	= 106;
		private const int icSignal_SS_DN	= 107;
		private const int icSignal_SS_UP	= 108;
		private const int icSignal_CP_DN	= 109;
		private const int icSignal_CP_UP	= 110;
		
		private const int icSignal_Weak		= 1;
		private const int icSignal_Neutral	= 2;
		private const int icSignal_Strong	= 3;
		private const int icSignal_Trade	= 4;
		//	5 Ichimoku signal types
		private int iSignal			= icNoSignal;
		private string sSignalCode	= " ";
		private bool bArrowDn		= false;
		private int iSignalStrength	= icSignal_Neutral;
		private double dChart_Ymin	= 90.0; 	//	Y value (=Price) at the bottom of chart 
		private double dChart_Ymax	= 134.0; 	//	Y value (=Price) at the top of chart
		private double dChart_Y		= 0.0;		//	Y location for showing the arrow
		private	double dChart_Yspan = 100.0;
		private static int iNbrSameBar = 0;
		private string sLegend = "SignalColor: LightGray(weak), NoColor(Neutral), Green/Red(strong)\n" +
							"Cross:\tTK - Tenkan / Kijun,  PK - Price / Kijun,  KB - Price / Kumo,\n\tSS - Senkou SpanA / SpanB,  CP - Chikou / Price\n";
							
		NinjaTrader.Gui.Tools.SimpleFont LegendFont = new NinjaTrader.Gui.Tools.SimpleFont("Courier New", 12);
		private Brush ArrowBrush	= Brushes.DarkGray;		//	use for debugging, do not remove
		
		//	for SharpDX drawing of Ichimoku cloud and Indicator ColorBar
		private Brush upAreaBrush	= Brushes.LightGreen;
		private Brush dnAreaBrush	= Brushes.Pink;
		private Brush upLineBrush	= Brushes.DarkGreen;
		private Brush dnLineBrush	= Brushes.Red;
		private Brush textBrush		= Brushes.Black;
		int iareaOpacity = 55;		//	this provides reasonable cloud density, can be changed by user input
		const float fontHeight		= 12f;
		int iX_barWidth = 10;		//	space for each bar, initialize at OnRender()

		private SharpDX.Direct2D1.Brush	upAreaBrushDx, dnAreaBrushDx, upLineBrushDx, dnLineBrushDx, textBrushDx;
		
		private static int iSignalIdx = 0;
		private bool bRendering = false;
		private static bool bGetSignal = false;
		private const int icSignalMax = 2000;
		private const int icSignalSort = -999;	//	to indicate that stSignal_all[] requires sorting
		private struct stSignal
		{
			public int iBar;
			public int iSignal;
			public bool bTrendDown;
			public int iStrength;
			public int iNbrSignal;
		};
		//	[0].iBar = total number of signals. [0].iNbrSignal = icReadTradeData, [0].iStrength = icMACD_data
		private stSignal[] stSignal_all = new stSignal[icSignalMax+1];

		//	2017.02.06 - added for external access of CloudBreak signal
		private const int icCloudBreakUp	= 1;
		private const int icCloudBreakDn	= -1;
		private const int icCloudBreakNone	= 0;
		[XmlIgnore]
		public Series<int> iExt_Signal;				//	expose signals for external access, 2017.06.19
		[XmlIgnore]
		public Series<int> iExt_SignalStrength;		//	expose signal strength for external access, 2017.06.19

		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description					= @"Display Ichimoku cloud / indicators.";
				Name						= "IchimokuSignal_B2";
				Calculate					= Calculate.OnBarClose;
				IsOverlay					= true;
				DisplayInDataBox			= true;
				DrawOnPricePanel			= true;
				DrawHorizontalGridLines		= true;
				DrawVerticalGridLines		= true;
				PaintPriceMarkers			= true;
				ScaleJustification			= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive	= true;
				Conversion					= 9;
				BBase						= 26;
				SpanB						= 52;
				Lag							= 26;
				Isignal_Strong				= true;
				Isignal_Neutral				= false;
				Isignal_Weak				= false;
				Ilegend						= true;
				
				upAreaBrush 				= Brushes.LightGreen;
				dnAreaBrush 				= Brushes.Pink;
				upLineBrush 				= Brushes.DarkGreen;
				dnLineBrush 				= Brushes.Red;
				iareaOpacity				= 55;
				
				AddPlot(Brushes.Purple, "ConversionLine");	//	Plots[0]
				AddPlot(Brushes.Teal, "BaseLine");			//	Plots[1]
				AddPlot(Brushes.Transparent, "SpanALine");	//	Plots[2]
				AddPlot(Brushes.Transparent, "SpanBLine");	//	Plots[3]
				AddPlot(Brushes.Transparent, "LagLine");	//	Plots[5]
				AddPlot(Brushes.DarkGreen, "SpanALine_Kumo");	//	Plots[6], 2017.05.27 (option Kumo lines, remove comment lines to activate)
				AddPlot(Brushes.Red, "SpanBLine_Kumo");	//	Plots[7], 2017.05.27  (option Kumo lines, remove comment lines to activate)
			}
			else if (State == State.Configure)
			{
				//AddDataSeries(Instrument.FullName, Data.BarsPeriodType.Day, 1, Data.MarketDataType.Last);
				ZOrder = -1;	//	2016.05.24, per ReusableBrushExample
				iExt_Signal	= new Series<int>(this);
				iExt_SignalStrength	= new Series<int>(this);
			}
			else if (State == State.Historical)
			{
				//	2017.06.19,	avoid crashes when creating DxBrush(), also not required to clone brushes when RenderTarget==null
				if(RenderTarget != null)
				{				
					//	2016.05.24	from Ninjascript ReuseDxBrushesExample.cs
					if (upAreaBrush.IsFrozen)
						upAreaBrush = upAreaBrush.Clone();		//	this will ensure that previous drawing using this brush is not affected
					upAreaBrush.Opacity = iareaOpacity / 100d;		//	.Opacity[0..1]
					upAreaBrush.Freeze();	//	freeze brush so that it can be changed later by other functions		
					upAreaBrushDx = upAreaBrush.ToDxBrush(RenderTarget);
					
					if (dnAreaBrush.IsFrozen)
						dnAreaBrush = dnAreaBrush.Clone();
					dnAreaBrush.Opacity = iareaOpacity / 100d;
					dnAreaBrush.Freeze();
					dnAreaBrushDx = dnAreaBrush.ToDxBrush(RenderTarget);

					//	the following brushes are not to be changed
					upLineBrushDx = upLineBrush.ToDxBrush(RenderTarget);		
					dnLineBrushDx = dnLineBrush.ToDxBrush(RenderTarget);
					textBrushDx = textBrush.ToDxBrush(RenderTarget);
				}
			}
		}

		//	2016.05.24
		public override void OnRenderTargetChanged()
		{
			if (upAreaBrushDx != null)
				upAreaBrushDx.Dispose();

			if (dnAreaBrushDx != null)
				dnAreaBrushDx.Dispose();

			if (upLineBrushDx != null)
				upLineBrushDx.Dispose();

			if (dnLineBrushDx != null)
				dnLineBrushDx.Dispose();

			if (textBrushDx != null)
				textBrushDx.Dispose();

			if (RenderTarget != null)	//	another rendering is starting
			{
				try
				{
					upAreaBrushDx	= upAreaBrush.ToDxBrush(RenderTarget);
					dnAreaBrushDx	= dnAreaBrush.ToDxBrush(RenderTarget);
					upLineBrushDx	= upLineBrush.ToDxBrush(RenderTarget);
					dnLineBrushDx	= dnLineBrush.ToDxBrush(RenderTarget);
					textBrushDx		= textBrush.ToDxBrush(RenderTarget);
				}
				catch (Exception e) { }
			}
		}

		//	kkc_2015, 2016.01.06	
		protected override void OnRender(ChartControl chartControl, ChartScale chartScale)
		{
			if( Bars == null || ChartControl == null || Bars.Instrument == null || !IsVisible || IsInHitTest || bRendering) 
				return;

			bRendering = true;
			iX_barWidth = chartControl.GetXByBarIndex(chartControl.BarsArray[0], ChartBars.FromIndex+1)
							- chartControl.GetXByBarIndex(chartControl.BarsArray[0], ChartBars.FromIndex);
			SharpDX.Direct2D1.AntialiasMode oldAntialiasMode = RenderTarget.AntialiasMode;	//	save for restore later			
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.Aliased;
			RenderTarget.AntialiasMode = SharpDX.Direct2D1.AntialiasMode.PerPrimitive;
			
			SharpDxDrawCLoud(chartControl, chartScale, SpanALine, SpanBLine, BBase);	
			base.OnRender(chartControl, chartScale);
			
			// get chart XYscale data, global variables with Indicator
			dChart_Ymin = chartScale.MinValue;
			dChart_Ymax = chartScale.MaxValue;
			dChart_Yspan = chartScale.MaxMinusMin;
			dChart_Y = dChart_Ymin + dChart_Yspan*0.01;

			int iX1 = ChartBars.FromIndex;		//	global parameter, absolute bar number
			if(Isignal_Strong || Isignal_Neutral || Isignal_Weak)
				DrawSignals(chartControl, chartScale);
			
			//	display Legend for Ichimoku signals as per IchimokuTrader.com
			if(Ilegend && ChartBars.Bars.Count-iX1 > 50)
			{
				bool bPrintText = true;
				for(int i=0; i<50; i++)		//	legend display width = 50 bars
					if(dChart_Ymax - High.GetValueAt(iX1+i) < 0.1*dChart_Yspan)
						bPrintText = false;
				if(bPrintText)
					Draw.TextFixed(this, "Legend", sLegend, TextPosition.TopLeft, Brushes.Black, LegendFont, Brushes.Blue, Brushes.LightGreen, 35);
				else
					Draw.TextFixed(this, "Legend", "", TextPosition.TopLeft, Brushes.Transparent, LegendFont, Brushes.Transparent, Brushes.Transparent, 0);
			}
			
			//	restore render properties and dispose of resources
			RenderTarget.AntialiasMode = oldAntialiasMode;
			bRendering = false;	//	rendering is completed
		}

		protected override void OnBarUpdate()
		{
			//Add your custom indicator logic here.
			iExt_Signal[0]	= icNoSignal;	//	initialize to no signal, 2017.06.19
			iExt_SignalStrength[0] = icSignal_Neutral;		//	initialize, 2017.06.19
			
			if(CurrentBar < SpanB)
			{
				iSignalIdx = 0;
				stSignal_all[0].iBar = iSignalIdx;	//	initialize maximum number of bars
				stSignal_all[0].iSignal = icSignalSort;	//	initialize signal to sort the struct
				stSignal_all[0].iNbrSignal = -1;		//	initialize to indicate fresh set of data
				stSignal_all[0].iStrength = -1;
			}
			if(CurrentBar < SpanB || CurrentBar < Conversion || CurrentBar < Lag || CurrentBar < BBase){return;}

			//	Tenkan sen = ConversionLine, Kijun sen = BaseLine, Senkou Span A = SpanALine, Senkou Span B = SpanBLine
			ConversionLine[0] = ((MAX(High,Conversion)[0] + MIN(Low,Conversion)[0]) / 2);
			BaseLine[0] = ((MAX(High,BBase)[0] + MIN(Low,BBase)[0]) / 2);
			SpanALine[0] = ((ConversionLine[0] + BaseLine[0]) / 2);
			SpanBLine[0] = ((MAX(High,SpanB)[0] + MIN(Low,SpanB)[0]) / 2);
			LagLine[Lag] = Close[0];
			
			//	 2017.05.29 (option Kumo lines, remove comment lines to activate)
			if(CurrentBar < BBase+SpanB)
			{
				SpanALine_Kumo[0] = Close[0];	//	2017.05.29
				SpanBLine_Kumo[0] = Close[0];
			}
			else
			{
				SpanALine_Kumo[0] = SpanALine[BBase];	//	2017.05.27
				SpanBLine_Kumo[0] = SpanBLine[BBase];
			}

			//	kkc_2015 2015.12.31  display Ichimoku signals as per IchimokuTrader.com
			dChart_Y = Low[0] - 20*TickSize;
			if((Isignal_Strong || Isignal_Neutral || Isignal_Weak) && (iSignalIdx < icSignalMax-1))
			{
				for(int i=0; i<icSignalType; i++)
				{
					//	signal type is identified sequentially from icSignal_TK to _CP
					if((iSignal = iGetCrossSignal(icSignals[i])) > icNoSignal)
					{
						iSignalStrength = ArrowCodes(iSignal);		//	ArrowBrush & sSignalCode are updated icSignals
						++iSignalIdx;
						iSignalIdx = Math.Min(iSignalIdx,icSignalMax);	//	make sure not to exceed nbr of signals
						stSignal_all[iSignalIdx].iBar = (icSignals[i]==icSignal_CP) ? CurrentBar-Lag :CurrentBar;
						stSignal_all[iSignalIdx].iSignal = icSignals[i];
						stSignal_all[iSignalIdx].iStrength = iSignalStrength;
						stSignal_all[0].iBar = iSignalIdx;	//	total number of signals within the data
						bArrowDn = (iSignal % 2 != 0);	//	down signal = odd number
						stSignal_all[iSignalIdx].bTrendDown = bArrowDn;
						stSignal_all[iSignalIdx].iNbrSignal = 0;	//	number of signals for each bar, 0 = 1 signal
						
						//	added 2017.06.19 to allow external access of iExt_Signal and signal strength
						iExt_Signal[0]	= iSignal;
						iExt_SignalStrength[0]	= iSignalStrength;
					}
				}
			}
		}

		#region Misc_functions
		//	kkc_2015, 2016.05.25	call from OnRender() only
		protected void DrawSignals(ChartControl chartControl, ChartScale chartScale)
		{
			int ibar_count = stSignal_all[0].iBar;
			if(ibar_count < 1)
				return;
			//	ChartBars properties are visible to all functions
			int ibar_start = ChartBars.FromIndex;
			int ibar_end = ChartBars.ToIndex;
			int ibar_max = ChartBars.Bars.Count;
			double dChart_Ymin = chartScale.MinValue;
			double dChart_Span = chartScale.MaxMinusMin;
			int iYmin = chartScale.GetYByValue(dChart_Ymin);	//	chart coordinate value
			int iYmax = chartScale.GetYByValue(dChart_Ymax);
			
			int iChart_X1 = ibar_max - ibar_start - 2;
			int iChart_X2 = ibar_max - ibar_end - 2;
			int iChart_X  = iChart_X1;
			int iSignalMax = stSignal_all[0].iBar;
			bool bDownTrend = false;
			bool bPrtSignal = true;
			string sSignal = "NO";		//	initialize string
			double Y_step = dChart_Span*0.05;	//	for vertical spacing of indicator codes on the chart
			double dChart_Y = dChart_Ymin + Y_step;
			string sTag = "SignalCode";
			int iCurrentBar = 0;
			int iCurrentBar_prev = 0;
			Point P1 = new Point();
			Point P2 = new Point();
			Brush AreaBrush = Brushes.LightGray;		//	use stock brushes, no need to dispose
			Brush outlineBrush = Brushes.LightGray;
			Brush lineBrush;

			SharpDX.Direct2D1.StrokeStyle strokeStyle = new Stroke(Brushes.Gray, DashStyleHelper.DashDotDot, 2f).StrokeStyle;

			int i = 0;
			while(++i < iSignalMax && stSignal_all[i].iBar < ibar_start);
			if(i >= iSignalMax || stSignal_all[i].iBar > ibar_end)
				return;
			
			sSignal = "Nbr.Signal = " + stSignal_all[0].iBar;	//	for display at bottom right
			Draw.TextFixed(this, "Profit", sSignal, TextPosition.BottomRight, Brushes.Black, LegendFont, Brushes.Blue, Brushes.White, 100);
			
			bBarSort();		//	perform sorting after additional data from stBuySell_all
			
			while(i <= iSignalMax && stSignal_all[i].iBar <= ibar_end)
			{
				iCurrentBar = stSignal_all[i].iBar;
				iSignal = stSignal_all[i].iSignal;
				if(iCurrentBar_prev == iCurrentBar)
					iNbrSameBar++;	//	for Signal printing Ypos
				else
				{
					if(iCurrentBar - stSignal_all[i-1].iBar == 1)
						iNbrSameBar = stSignal_all[i-1].iNbrSignal+1;
					else
						iNbrSameBar = 0;
					iCurrentBar_prev = iCurrentBar;
				}

				if((iSignal < icSignal_TK) || (iSignal > icSignal_DU))
				{
					i++;
					iSignal = icNoSignal;
					iCurrentBar_prev = 0;
					continue;
				}
				
				iChart_X = ibar_max - iCurrentBar - 2;
				stSignal_all[i].iNbrSignal = iNbrSameBar;

				if(iChart_X < iChart_X1 && iChart_X > iChart_X2)
				{
					iSignalStrength = stSignal_all[i].iStrength;
					bDownTrend = stSignal_all[i].bTrendDown;
					AreaBrush = bDownTrend ? Brushes.Pink : Brushes.LightGreen;
					outlineBrush = bDownTrend ? Brushes.Red : Brushes.DarkGreen;
					sSignal = scSignals[iSignal-icSignal_Base];
					if(iSignal == icSignal_LS)
						sSignal = bDownTrend ? "S" : "L";
					else
						if(iSignal == icSignal_DU)
							sSignal = bDownTrend ? "D" : "U";
					
					if(iSignalStrength == icSignal_Neutral)
					{
						AreaBrush = Brushes.White;
						outlineBrush = Brushes.DarkGray;
					}
					else if(iSignalStrength == icSignal_Weak)
					{
						AreaBrush = Brushes.LightGray;
						outlineBrush = Brushes.DarkGray;
					}

					bPrtSignal = (iSignalStrength == icSignal_Strong && Isignal_Strong);
					bPrtSignal = bPrtSignal || (iSignalStrength == icSignal_Neutral && Isignal_Neutral);
					bPrtSignal = bPrtSignal || (iSignalStrength == icSignal_Weak && Isignal_Weak);
					bPrtSignal = bPrtSignal || (iSignal == icSignal_LS);
					if(!bPrtSignal)
					{
						i++;
						continue;
					}
					if(Low.GetValueAt(iCurrentBar) > dChart_Ymin + 0.3*dChart_Yspan)
						dChart_Y = dChart_Ymin + (iNbrSameBar+2) * Y_step;	//	bottom position
					else
					dChart_Y = dChart_Ymax - (iNbrSameBar+2) * Y_step;	//	top position
					
					//	get chart coordinate data
					P1.X = chartControl.GetXByBarIndex(chartControl.BarsArray[0], iCurrentBar);
					P1.Y = iYmin;
					P2.X = P1.X;
					P2.Y = iYmax;

					//	set up color brushes
					lineBrush = bDownTrend ? Brushes.Red : Brushes.DarkGreen;
					RenderTarget.DrawLine(P1.ToVector2(), P2.ToVector2(), lineBrush.ToDxBrush(RenderTarget), 1f, strokeStyle);
 
					P1.Y = chartScale.GetYByValue(dChart_Y);;
					SharpDxDrawSignalCode(sSignal, P1, outlineBrush.ToDxBrush(RenderTarget), AreaBrush.ToDxBrush(RenderTarget));				
				}
				i++;
			}		

			strokeStyle.Dispose();
		}
	
		//	sort the bar into sequential order
		protected bool bBarSort()
		{
			bool bSortReqd = false;
			if(stSignal_all[0].iSignal != icSignalSort)
				return(bSortReqd);

			int i = 1, j = 1, k = 1;
			stSignal stTemp;				
			while(++i <= stSignal_all[0].iBar)
			{
				if(stSignal_all[i].iBar < stSignal_all[i-1].iBar)
				{
					stTemp = stSignal_all[i];
					j = i-1;
					while((--j>0) && (stSignal_all[j].iBar > stSignal_all[i].iBar));
					for(k=i; k>j+1; k--)
						stSignal_all[k] = stSignal_all[k-1];
					stSignal_all[j+1] = stTemp;
				}
			}
			stSignal_all[0].iSignal = 0;		//	all sorted
			return(bSortReqd);
		}
		
		protected void SharpDxDrawDiamond(Point pStart, SharpDX.Direct2D1.Brush lineBrushDx, SharpDX.Direct2D1.Brush fillBrushDx)
		{
			const int icPoints = 5;
			float iX = (pStart.X > 6) ? (float)pStart.X : 6f;		//	make sure there is at least n pixels on chart left side
			float iY = (float)pStart.Y;		//	Y is always positive
			float [,] XY = new float[icPoints,2] {{0,0}, {6,-6}, {0,-12}, {-6,-6}, {0,0}};
			Point P = new Point();
			SharpDX.Vector2[] vector = new SharpDX.Vector2[icPoints];

			for(int i=0; i<icPoints; i++)
			{
				P.X = iX + XY[i,0];
				P.Y = iY + XY[i,1];
				vector[i] = P.ToVector2();
			}

			DrawGeometry(vector, lineBrushDx, fillBrushDx, true);
		}

		protected void SharpDxDrawArrow(Point pStart, SharpDX.Direct2D1.Brush lineBrushDx, SharpDX.Direct2D1.Brush fillBrushDx, bool iUpArrow=true)
		{
			const int icPoints = 9;
			float iX = (pStart.X > 6) ? (float)pStart.X : 6f;		//	make sure there is at least n pixels on chart left side
			float iY = (float)pStart.Y;		//	Y is always positive
			float [,] XY = new float[icPoints,2] {{0,0}, {2,0}, {2,-4}, {6,-4}, {0,-10}, {-6,-4}, {-2,-4}, {-2,0}, {0,0}};
			Point P = new Point();
			SharpDX.Vector2[] vector = new SharpDX.Vector2[icPoints];
			float sgn = iUpArrow ? 1f : -1f;

			for(int i=0; i<icPoints; i++)
			{
				P.X = iX + XY[i,0];
				P.Y = iY + sgn*XY[i,1];
				vector[i] = P.ToVector2();
			}

			DrawGeometry(vector, lineBrushDx, fillBrushDx, true);
		}

		//	kkc_2015, 2016.05.25, must be called by DrawSignals	
		protected void SharpDxDrawSignalCode(string sSignal, Point pStart, SharpDX.Direct2D1.Brush lineBrushDx, SharpDX.Direct2D1.Brush fillBrushDx)
		{
			float iX = (float)((iX_barWidth-1f)*2f);		//	leave one pixel on each side of the rect
			float fXstart = (float)(pStart.X - iX/2f);
			fXstart = (fXstart < 0) ? 0 : fXstart;		//	negative value will cause error preparing textLayout
			
			Point P = new Point(fXstart+1f, pStart.Y+2);	//	adjust location to place text
			SharpDX.RectangleF rectf = new SharpDX.RectangleF(fXstart, (float)pStart.Y, (float)iX, fontHeight+4f);		//	equal x, y dimensions
			SharpDX.DirectWrite.TextFormat textFormat = new SharpDX.DirectWrite.TextFormat(Core.Globals.DirectWriteFactory, "Courier New", SharpDX.DirectWrite.FontWeight.Normal,
															SharpDX.DirectWrite.FontStyle.Normal, SharpDX.DirectWrite.FontStretch.Normal, fontHeight)
			{
				TextAlignment = SharpDX.DirectWrite.TextAlignment.Leading,
				WordWrapping = SharpDX.DirectWrite.WordWrapping.NoWrap
			};
			RenderTarget.FillRectangle(rectf, fillBrushDx);		//	fill, then draw outline box
			RenderTarget.DrawRectangle(rectf, lineBrushDx);
			SharpDX.DirectWrite.TextLayout textLayout = new SharpDX.DirectWrite.TextLayout(Core.Globals.DirectWriteFactory, sSignal, textFormat, rectf.X+2f, fontHeight+4f);
			RenderTarget.DrawTextLayout(P.ToVector2(), textLayout, textBrushDx, SharpDX.Direct2D1.DrawTextOptions.NoSnap);	//	use textBrushDx
			
			textLayout.Dispose();
			textFormat.Dispose();		
		}

		//	kkc_2015, 2016.05.25, must be called by OnRender	
		protected void SharpDxDrawCLoud(ChartControl chartControl, ChartScale chartScale, ISeries<double> SpanA_s, ISeries<double> SpanB_s, int iOffset)
		{
			// RenderTarget is always full panel, so we need to be mindful which sub ChartPanel we're dealing with
			// always use ChartPanel X, Y, W, H - as ActualWidth, Width, ActualHeight, Height are in WPF units, so they can be drastically different depending on DPI set

			int iX_start = chartControl.GetXByBarIndex(chartControl.BarsArray[0], ChartBars.FromIndex);		//	panel.X location
			int iX_End = chartControl.GetXByBarIndex(chartControl.BarsArray[0], ChartBars.ToIndex);
			int iBarWidth = chartControl.GetXByBarIndex(chartControl.BarsArray[0], 1) - chartControl.GetXByBarIndex(chartControl.BarsArray[0], 0);
			int iBar_Start = (ChartBars.FromIndex > iOffset*3) ? ChartBars.FromIndex : iOffset*3;		//	global parameter, absolute bar number
			int iNbrBarSpaceAvailable = (ChartPanel.W - iX_End) / iBarWidth - 1;
			if(ChartBars.ToIndex - iBar_Start < 2)		//	
				return;
			
			int iBar_End = ChartBars.ToIndex + Math.Min(iOffset, iNbrBarSpaceAvailable);
			
			int iPointMax = iBar_End - iBar_Start +1;
			int[] iX = new int[iPointMax];		//	chartX coordinate, located at center of bar
			int[] iA = new int[iPointMax];		//	chartY coordinate for series A
			int[] iB = new int[iPointMax];

			int idx, idx_offset;		//	index for series A, B
			int i, j, k, m;
   			for(i = 0; i < iPointMax; i++)
			{
				idx = i + iBar_Start;
				idx_offset = idx - iOffset;
				iX[i] = chartControl.GetXByBarIndex(chartControl.BarsArray[0], idx);
				iA[i] = chartScale.GetYByValue(SpanA_s.GetValueAt(idx_offset));
				iB[i] = chartScale.GetYByValue(SpanB_s.GetValueAt(idx_offset));
			}

			double dX = iX[0];		//	initialize prior to entering while() loop
			double dY = iA[0];
			Point p = new Point();
			int iVectorMemberMax;
			bool bUpCloud = (iA[0] > iB[0]);		//	Uptrend
			bool bReqd = true;
			idx = iBar_Start;
			i = 0; j = 0;
			while(i < iPointMax-1)
			{
				k = i;	//	k has the starting point for each geometry
				while((i < iPointMax) && (iA[i] > iB[i]) == bUpCloud)		//	must check i<iPointMax first, prior to == bUpCloud
					i++;
				iVectorMemberMax = 2*(i-k+1);	//	Points for iA + iB + connecting point between iA and iB
				SharpDX.Vector2[] vectorSpan = new SharpDX.Vector2[iVectorMemberMax];
				m = 0;
				p.X = dX;	//	use last data point at the cross of SpanA_s and SpanB_s as starting point
				p.Y = dY;
				vectorSpan[m++] = p.ToVector2();
				
				for(j=k; j<i; j++)
				{
					p.X = iX[j];
					p.Y = iA[j]; 	//	data for SpanA_s series
					vectorSpan[m++] = p.ToVector2();
				}
				if(i < iPointMax-1)	//	Not the last point, need to create the intersection point
				{
					//	calculate iA, iB connecting point using equation
					dX = (iA[j-1]-iB[j-1]) * iX_barWidth / (iB[j]-iA[j]+iA[j-1]-iB[j-1]);	//	dX portion of iX_barWidth
					dY = (iB[j]-iB[j-1]) / iX_barWidth * dX + iB[j-1];
					dX += iX[j-1];	//	actual dX including iX_barWidth portion
					p.X = dX;
					p.Y = dY;
				}
				else
				{
					//	no iA / iB crossed, therefore draw straight line between iA & iB
					p.X = iX[j-1];
					p.Y = iB[j-1];
				}
				vectorSpan[m++] = p.ToVector2();
				for(j=i-1; j>=k; j--)
				{
					p.X = iX[j];
					p.Y = iB[j]; 	//	data for SpanB_s series
					vectorSpan[m++] = p.ToVector2();
				}
				//	no need to connect the line end to start
				if(!bUpCloud)	//	bUpCloud has the next cloud status, previous status = !bUpCloud
					DrawGeometry(vectorSpan, upLineBrushDx, upAreaBrushDx, false);
				else
					DrawGeometry(vectorSpan, dnLineBrushDx, dnAreaBrushDx, false);
				if(i < iPointMax)
					bUpCloud = (iA[i] > iB[i]);		//	placed in this location to ensure iA[i] is not out of range
			}
			//	draw the cloud outline
			SharpDX.Vector2[] vectorSpanA = new SharpDX.Vector2[iPointMax];
			SharpDX.Vector2[] vectorSpanB = new SharpDX.Vector2[iPointMax];
			Point p1 = new Point();
			Point p2 = new Point();

			for(i = 0; i < iPointMax-2; i++)
			{
				p1.X = iX[i];
				p2.X = iX[i+1];
				p1.Y = iA[i];
				p2.Y = iA[i+1];
				RenderTarget.DrawLine(p1.ToVector2(), p2.ToVector2(), upLineBrushDx, 1);
				p1.Y = iB[i];
				p2.Y = iB[i+1];
				RenderTarget.DrawLine(p1.ToVector2(), p2.ToVector2(), dnLineBrushDx, 1);
			}
		}

		protected void DrawGeometry(SharpDX.Vector2[] vectorSpan, SharpDX.Direct2D1.Brush LineBrushDx, SharpDX.Direct2D1.Brush AreaBrushDx, bool bDrawOutline)
		{
			//	the line in the vector must be continuous, unclosed gemoetry will be closed automatically
			//	all elements in the vector must be provided with data
			SharpDX.Direct2D1.PathGeometry geo1 = new SharpDX.Direct2D1.PathGeometry(Core.Globals.D2DFactory);
			SharpDX.Direct2D1.GeometrySink sink1 = geo1.Open();
			Point iP_start = new Point(vectorSpan[0].X, vectorSpan[0].Y);
			sink1.BeginFigure(iP_start.ToVector2(), SharpDX.Direct2D1.FigureBegin.Filled);
			sink1.AddLines(vectorSpan);
			sink1.EndFigure(SharpDX.Direct2D1.FigureEnd.Closed);
			sink1.Close();
			RenderTarget.FillGeometry(geo1, AreaBrushDx);
			if(bDrawOutline)
				RenderTarget.DrawGeometry(geo1, LineBrushDx);	//	draw shape outline
			geo1.Dispose();
			sink1.Dispose();
		}		
		
				
		//	kkc_2015 2015.12.31	Assign Arrow color code & Signal code; return Signal location status
		protected int ArrowCodes(int iSeekSignal)
		{
			int iCloudStatus = icInCloud;
			//	ConversionLine[barsAgo] = Tenkan Sen
			//	BaseLine[barsAgo] = Kijun Sen
			//	SpanALine[barsAgo] = Senkou Span A
			//	SpanBLine[barsAgo] = Senkou Span B
			ArrowBrush = Brushes.LightGray;
			sSignalCode = " ";		//	no signal code
			
			switch(iSeekSignal)
			{
				case icSignal_TK_DN:	//	Tenkan / Kijun Cross in relation to cloud
				case icSignal_TK_UP:
					sSignalCode = "TK";
					iCloudStatus = iGetCloudStatus(ConversionLine[0]);		//	use data[0 BarAgo], *** neeed checking ***
					break;
				case icSignal_PK_DN:
				case icSignal_PK_UP:
					sSignalCode = "PK";
					iCloudStatus = iGetCloudStatus(Close[0]);		//	use data[0 BarAgo]
					break;
				case icSignal_KB_DN:	//	the price is always inside Kumo cloud, waiting for breakout
				case icSignal_KB_UP:
					sSignalCode = "KB";
					break;
				case icSignal_SS_DN:
				case icSignal_SS_UP:
					sSignalCode = "SS";
					iCloudStatus = iGetCloudStatus(SpanALine[0]);		//	use data[0 BarAgo]
					break;
				case icSignal_CP_DN:
				case icSignal_CP_UP:
					sSignalCode = "CP";
					//	cloud and Chikoou Span data should be at Lag bar ago
					iCloudStatus = iGetCloudStatus(Close[Lag],Lag);
					break;
				default:
					break;
			}
			//	ArrowBrush color is for testing only, but it is also used to determine iSignalStrength
			bArrowDn = (iSeekSignal % 2 != 0);	//	down signal = odd number
			if(iSeekSignal==icSignal_KB_DN || iSeekSignal==icSignal_KB_UP)
				ArrowBrush = (iSeekSignal==icSignal_KB_DN) ? Brushes.Red : Brushes.Green;
			else
				switch(iCloudStatus)
				{
					case icBelowCloud:
						ArrowBrush = bArrowDn ? Brushes.Red : Brushes.DarkGray;
						break;
					case icInCloud:
						ArrowBrush = Brushes.LightGray;
						break;
					case icAboveCloud:
						ArrowBrush = bArrowDn ? Brushes.DarkGray : Brushes.Green;
						break;
					default:
						break;
				}
			//	set signal strength
			iSignalStrength = icSignal_Strong;
			if(ArrowBrush == Brushes.DarkGray)
				iSignalStrength = icSignal_Weak;
			else if(ArrowBrush == Brushes.LightGray)
				iSignalStrength = icSignal_Neutral;
			return(iSignalStrength);
		}
		
		protected int iGetCloudStatus(double dY, int iLag=0)
		{
			//	dY has data from 1 BarAgo 
			int iRetSignal = icInCloud;
			
			if(dY < Math.Min(SpanALine[0+BBase+iLag],SpanBLine[0+BBase+iLag]))
				iRetSignal = icBelowCloud;
			else
				if(dY > Math.Max(SpanALine[0+BBase+iLag],SpanBLine[0+BBase+iLag]))
					iRetSignal = icAboveCloud;
			return(iRetSignal);
		}
		
		//	2015.12.31, return CrossSignal, _DN, _UP
		protected int iGetCrossSignal(int iSeekSignal)
		{
			int i, iUpDown;
			int iRetSignal = icNoSignal;
			bool bConditionMet = false;
			if(CurrentBar < SpanB+icTrendSpan || CurrentBar < Conversion+icTrendSpan || CurrentBar < Lag+icTrendSpan || CurrentBar < BBase+icTrendSpan)
				return(iRetSignal);	

			switch(iSeekSignal)
			{
				case icSignal_TK:	//	check signal for Tenkan / Kijun Crossed
					bConditionMet = true;
					//	check for Tankan above Kijun BaseLine
					for(i=1; i<=icTrendSpan; i++)
						if(ConversionLine[i] <= BaseLine[i])
							bConditionMet = false;
					if(bConditionMet)	//	Tankan is above Kijan sen, precursory for downtrend
						iRetSignal = (ConversionLine[0] <= BaseLine[0]) ? icSignal_TK_DN : icNoSignal;
					else
					{
						bConditionMet = true;
						//	check for Tankan below Kijun BaseLine
						for(i=1; i<=icTrendSpan; i++)
							if(ConversionLine[i] >= BaseLine[i])
								bConditionMet = false;
						if(bConditionMet)	//	Tankan is below Kijan sen, precursory for uptrend
							iRetSignal = (ConversionLine[0] >= BaseLine[0]) ? icSignal_TK_UP : icNoSignal;
					}
					break;
				case icSignal_PK:	//	check signal for price crosses Kijun sen
					bConditionMet = true;
					//	check for Price above Kijun BaseLine
					for(i=1; i<=icTrendSpan; i++)
						if(Low[i] <= BaseLine[i])
							bConditionMet = false;
					if(bConditionMet)	//	Close Price is above Kijan sen, precursory for downtrend
						iRetSignal = (Close[0] <= BaseLine[0]) ? icSignal_PK_DN : icNoSignal;
					else
					{
						bConditionMet = true;
						//	check for Price below Kijun BaseLine
						for(i=1; i<=icTrendSpan; i++)
							if(High[i] >= BaseLine[i])
								bConditionMet = false;
						if(bConditionMet)	//	Price is below Kijun sen, precursory for uptrend
							iRetSignal = (Close[0] >= BaseLine[0]) ? icSignal_PK_UP : icNoSignal;
					}
					break;
				case icSignal_KB:	//	check for Kumo Breakout, Kumo data is from older data[BBase]
					bConditionMet = true;
					//	check for Price below top of Kumo cloud
					for(i=1; i<=icTrendSpan; i++)
						if(Close[i] >= Math.Max(SpanALine[i+BBase],SpanBLine[i+BBase]))
							bConditionMet = false;
					if(bConditionMet)	//	Close Price is below top of cloud, precursory for uptrend breakout
						iRetSignal = (Close[0] >= Math.Max(SpanALine[0+BBase],SpanBLine[0+BBase])) ? icSignal_KB_UP : icNoSignal;
					if(iRetSignal == icNoSignal)
					{
						bConditionMet = true;
						//	check for Price above bottom of Kumo cloud
						for(i=1; i<=icTrendSpan; i++)
							if(Close[i] <= Math.Min(SpanALine[i+BBase],SpanBLine[i+BBase]))
								bConditionMet = false;
						if(bConditionMet)	//	Price is above bottom of cloud, precursory for downtrend
							iRetSignal = (Close[0] <= Math.Min(SpanALine[0+BBase],SpanBLine[0+BBase])) ? icSignal_KB_DN : icNoSignal;
					}
					break;
				case icSignal_SS:	//	check signal for Senkou SpanA / SpanB cross
					bConditionMet = true;
					//	check for SpanA above SpanB
					for(i=1; i<=icTrendSpan; i++)
						if(SpanALine[i] <= SpanBLine[i])
							bConditionMet = false;
					if(bConditionMet)	//	SpanALine is above SpanBLine, precursory for downtrend
						iRetSignal = (SpanALine[0] <= SpanBLine[0]) ? icSignal_SS_DN : icNoSignal;
					else
					{
						bConditionMet = true;
						//	check for SpanA below SpanB
						for(i=1; i<=icTrendSpan; i++)
							if(SpanALine[i] >= SpanBLine[i])
								bConditionMet = false;
						if(bConditionMet)	//	SpanALine is below SpanBLine, precursory for uptrend
							iRetSignal = (SpanALine[0] >= SpanBLine[0]) ? icSignal_SS_UP : icNoSignal;
					}
					break;
				case icSignal_CP:	//	check signal for Chikou Span Cross, all data to be based on [Lag]
					bConditionMet = true;
					//	check for Chikou Span above Price, LagLine[Lag] = Close[0]
					for(i=1; i<=icTrendSpan; i++)
						if(LagLine[Lag+i] <= High[Lag+i])
							bConditionMet = false;
					if(bConditionMet)	//	Chikou Span is above Price, precursory for downtrend
						iRetSignal = (LagLine[Lag] <= Close[Lag]) ? icSignal_CP_DN : icNoSignal;
					else
					{
						bConditionMet = true;
						//	check for Chikou Span below Price
						for(i=1; i<=icTrendSpan; i++)
							if(LagLine[Lag+i] >= Low[Lag+i])
								bConditionMet = false;
						if(bConditionMet)	//	Price is below Kijun sen, precursory for uptrend
							iRetSignal = (LagLine[Lag] >= Close[Lag]) ? icSignal_CP_UP : icNoSignal;
					}
					break;
				default:
					break;
			}
			return(iRetSignal);
		}

		#endregion
		
		#region Properties
		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolShapesAreaBrush", Order=1, GroupName = "NinjaScriptGeneral")]
		public Brush UpAreaBrush
		{
			get { return upAreaBrush; }
			set
			{
				upAreaBrush = value;
				if (upAreaBrush != null)
				{
					if (upAreaBrush.IsFrozen)
						upAreaBrush = upAreaBrush.Clone();
					upAreaBrush.Opacity = iareaOpacity / 100d;
					upAreaBrush.Freeze();
				}
			}
		}

		[Browsable(false)]
		public string UpAreaBrushSerialize
		{
			get { return Serialize.BrushToString(UpAreaBrush); }
			set { UpAreaBrush = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolShapesAreaBrush", Order=2, GroupName = "NinjaScriptGeneral")]
		public Brush DnAreaBrush
		{
			get { return dnAreaBrush; }
			set
			{
				dnAreaBrush = value;
				if (dnAreaBrush != null)
				{
					if (dnAreaBrush.IsFrozen)
						dnAreaBrush = dnAreaBrush.Clone();
					dnAreaBrush.Opacity = iareaOpacity / 100d;
					dnAreaBrush.Freeze();
				}
			}
		}

		[Browsable(false)]
		public string DnAreaBrushSerialize
		{
			get { return Serialize.BrushToString(DnAreaBrush); }
			set { DnAreaBrush = Serialize.StringToBrush(value); }
		}

		[Range(0, 100)]
		[Display(ResourceType = typeof(Custom.Resource), Name = "NinjaScriptDrawingToolAreaOpacity", Order=3, GroupName = "NinjaScriptGeneral")]
		public int iAreaOpacity
		{
			get { return iareaOpacity; }
			set
			{
				iareaOpacity = Math.Max(0, Math.Min(100, value));
				if (upAreaBrush != null)
				{
					upAreaBrush = upAreaBrush.Clone();
					upAreaBrush.Opacity = iareaOpacity / 100d;
					upAreaBrush.Freeze();
				}
				if (dnAreaBrush != null)
				{
					dnAreaBrush = dnAreaBrush.Clone();
					dnAreaBrush.Opacity = iareaOpacity / 100d;
					dnAreaBrush.Freeze();
				}
			}
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "UpCloudBrush line color", Order=4, GroupName = "NinjaScriptGeneral")]
		public Brush UpLineBrush
		{
			get { return upLineBrush; }
			set { upLineBrush = value; }
		}

		[Browsable(false)]
		public string UpLineBrushSerialize
		{
			get { return Serialize.BrushToString(UpLineBrush); }
			set { UpLineBrush = Serialize.StringToBrush(value); }
		}

		[XmlIgnore]
		[Display(ResourceType = typeof(Custom.Resource), Name = "DnCloudBrush line color", Order=5, GroupName = "NinjaScriptGeneral")]
		public Brush DnLineBrush
		{
			get { return dnLineBrush; }
			set { dnLineBrush = value; }
		}

		[Browsable(false)]
		public string DnLineBrushSerialize
		{
			get { return Serialize.BrushToString(DnLineBrush); }
			set { DnLineBrush = Serialize.StringToBrush(value); }
		}
		
		[Range(1, 27)]
		[NinjaScriptProperty]
		[Display(Name="Conversion", Description = "Tenkan line - 9 typical", Order=1, GroupName="Parameters")]
		public int Conversion
		{ get; set; }

		[Range(1, 78)]
		[NinjaScriptProperty]
		[Display(Name="Base", Description = "Kijun line - 26 typical", Order=2, GroupName="Parameters")]
		public int BBase
		{ get; set; }

		[Range(1, 156)]
		[NinjaScriptProperty]
		[Display(Name="SpanB", Description = "Senkou Span B - 52 typical", Order=3, GroupName="Parameters")]
		public int SpanB
		{ get; set; }

		[Range(1, 78)]
		[NinjaScriptProperty]
		[Display(Name="Lag", Description = "Chikou Span - 26 typical", Order=4, GroupName="Parameters")]
		public int Lag
		{ get; set; }
	
		[NinjaScriptProperty]
		[Display(Name="Strong signal", Order=5, GroupName="Settings")]
		public bool Isignal_Strong
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Neutral signal", Order=6, GroupName="Settings")]
		public bool Isignal_Neutral
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Weak signal", Order=7, GroupName="Settings")]
		public bool Isignal_Weak
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="Signal legend", Order=8, GroupName="Settings")]
		public bool Ilegend
		{ get; set; }
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> ConversionLine
		{
			get { return Values[0]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> BaseLine
		{
			get { return Values[1]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SpanALine
		{
			get { return Values[2]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SpanBLine
		{
			get { return Values[3]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> LagLine
		{
			get { return Values[4]; }
		}
		//(option Kumo lines, remove comment lines to activate)
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SpanALine_Kumo
		{
			get { return Values[5]; }
		}

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> SpanBLine_Kumo
		{
			get { return Values[6]; }
		}

		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private IchimokuSignal[] cacheIchimokuSignal;
		public IchimokuSignal IchimokuSignal(int conversion, int bBase, int spanB, int lag, bool isignal_Strong, bool isignal_Neutral, bool isignal_Weak, bool ilegend)
		{
			return IchimokuSignal(Input, conversion, bBase, spanB, lag, isignal_Strong, isignal_Neutral, isignal_Weak, ilegend);
		}

		public IchimokuSignal IchimokuSignal(ISeries<double> input, int conversion, int bBase, int spanB, int lag, bool isignal_Strong, bool isignal_Neutral, bool isignal_Weak, bool ilegend)
		{
			if (cacheIchimokuSignal != null)
				for (int idx = 0; idx < cacheIchimokuSignal.Length; idx++)
					if (cacheIchimokuSignal[idx] != null && cacheIchimokuSignal[idx].Conversion == conversion && cacheIchimokuSignal[idx].BBase == bBase && cacheIchimokuSignal[idx].SpanB == spanB && cacheIchimokuSignal[idx].Lag == lag && cacheIchimokuSignal[idx].Isignal_Strong == isignal_Strong && cacheIchimokuSignal[idx].Isignal_Neutral == isignal_Neutral && cacheIchimokuSignal[idx].Isignal_Weak == isignal_Weak && cacheIchimokuSignal[idx].Ilegend == ilegend && cacheIchimokuSignal[idx].EqualsInput(input))
						return cacheIchimokuSignal[idx];
			return CacheIndicator<IchimokuSignal>(new IchimokuSignal(){ Conversion = conversion, BBase = bBase, SpanB = spanB, Lag = lag, Isignal_Strong = isignal_Strong, Isignal_Neutral = isignal_Neutral, Isignal_Weak = isignal_Weak, Ilegend = ilegend }, input, ref cacheIchimokuSignal);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.IchimokuSignal IchimokuSignal(int conversion, int bBase, int spanB, int lag, bool isignal_Strong, bool isignal_Neutral, bool isignal_Weak, bool ilegend)
		{
			return indicator.IchimokuSignal(Input, conversion, bBase, spanB, lag, isignal_Strong, isignal_Neutral, isignal_Weak, ilegend);
		}

		public Indicators.IchimokuSignal IchimokuSignal(ISeries<double> input , int conversion, int bBase, int spanB, int lag, bool isignal_Strong, bool isignal_Neutral, bool isignal_Weak, bool ilegend)
		{
			return indicator.IchimokuSignal(input, conversion, bBase, spanB, lag, isignal_Strong, isignal_Neutral, isignal_Weak, ilegend);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.IchimokuSignal IchimokuSignal(int conversion, int bBase, int spanB, int lag, bool isignal_Strong, bool isignal_Neutral, bool isignal_Weak, bool ilegend)
		{
			return indicator.IchimokuSignal(Input, conversion, bBase, spanB, lag, isignal_Strong, isignal_Neutral, isignal_Weak, ilegend);
		}

		public Indicators.IchimokuSignal IchimokuSignal(ISeries<double> input , int conversion, int bBase, int spanB, int lag, bool isignal_Strong, bool isignal_Neutral, bool isignal_Weak, bool ilegend)
		{
			return indicator.IchimokuSignal(input, conversion, bBase, spanB, lag, isignal_Strong, isignal_Neutral, isignal_Weak, ilegend);
		}
	}
}

#endregion
