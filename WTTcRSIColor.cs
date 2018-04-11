// 
// Copyright (C) 2017 CC BY, www.whentotrade.com / Lars von Thienen
// Book: Decoding The Hidden Market Rhythm - Part 1 (2017)
// Chapter 4 "Fine-tuning technical indicators"
// Link: https://www.amazon.com/dp/1974658244
//
// Usage: 
// You need to derive the dominant cycle as input parameter for the cycle length as described in chapter 4.
//
// License: 
// This work is licensed under a Creative Commons Attribution 4.0 International License.
// You are free to share the material in any medium or format and remix, transform, and build upon the material for any purpose, 
// even commercially. You must give appropriate credit to the authors book and website, provide a link to the license, and indicate 
// if changes were made. You may do so in any reasonable manner, but not in any way that suggests the licensor endorses you or your use. 
//

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

namespace NinjaTrader.NinjaScript.Indicators
{
	public class WTTcRSIColor : Indicator
	{
		private Series<double> avgDown;
		private Series<double> avgUp;
		private Series<double> down;
		private Series<double> up;
		private Series<double> raw;
		private double cycleConstant;
		private double ad=0;
		private double au=0;
		private double torque;
		private int	phasingLag;
		
		protected override void OnStateChange()
		{ 
			if (State == State.SetDefaults)
			{
				Description									= @"Cyclic Smoothed RSI Indicator";
				Name										= "WTT cRSI Color";
				Calculate									= Calculate.OnBarClose;
				IsOverlay									= false;
				DisplayInDataBox							= true;
				DrawOnPricePanel							= true;
				DrawHorizontalGridLines						= true;
				DrawVerticalGridLines						= true;
				PaintPriceMarkers							= false;
				ScaleJustification							= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				IsSuspendedWhileInactive	= true;
				CycleLength					= 20;
				Vibration				 	= 10;
				CyclicMemory				= 40; 
				Leveling					= 10;
								
				AddPlot(Brushes.Red, "CRSI");
				
				AddPlot(Brushes.Blue, "cRSIUpper");
				AddPlot(Brushes.Gray, "cRSINeutral");
				AddPlot(Brushes.Blue, "cRSILower");
			}
			else if (State == State.Configure)
			{
				cycleConstant = (CycleLength - 1);
				torque		= 2.0 / (Vibration + 1);
				phasingLag	= (int) Math.Ceiling((Vibration - 1) / 2.0);
			}
			else if (State == State.DataLoaded)
			{
				//init
				avgUp	= new Series<double>(this);
				avgDown = new Series<double>(this);
				down	= new Series<double>(this);
				up		= new Series<double>(this);
				raw 	= new Series<double>(this);
			}
		}

		protected override void OnBarUpdate()
		{			
			if (CurrentBar == 0)
			{
				down[0]		= 0;
				up[0]		= 0;
				return;
			}

			double input0	= Input[0];
			double input1	= Input[1];
			down[0]			= Math.Max(input1 - input0, 0);
			up[0]			= Math.Max(input0 - input1, 0);
						
			if (CurrentBar + 1 < CycleLength) 
			{
				ad+=down[0];
				au+=up[0];
				return;
			}

			if ((CurrentBar + 1) == CycleLength) 
			{
				// initial load
				avgDown[0]	= ad/CycleLength;
				avgUp[0]	= au/CycleLength;
			}  
			else 
			{
				// RSI prep
				avgDown[0]	= (avgDown[1] * cycleConstant + down[0]) / CycleLength;
				avgUp[0]	= (avgUp[1] * cycleConstant + up[0]) / CycleLength;
			}

			double avgDown0	= avgDown[0];
			double rawRSI	= avgDown0 == 0 ? 100 : 100 - 100 / (1 + avgUp[0] / avgDown0);
			double cRSI = torque * (2 * rawRSI - raw[phasingLag]) + (1-torque) * CRSI[1];
			
			raw[0] = rawRSI;
						
			if (CurrentBar < CycleLength+phasingLag)
				CRSI[0] = rawRSI;
			else 
				CRSI[0]	= cRSI;
			
			if (CurrentBar < CycleLength+phasingLag+CyclicMemory) return;
			
			double ub; double db; double lmax; double lmin; double ratio;
			double testvalue; int above; int below; double mstep;
			double aperc = (double)Leveling / 100;
			
			lmax=-999999; lmin=999999;
			for (int i=0; i<CyclicMemory; i++){
				if (CRSI[i]>lmax) lmax=CRSI[i]; 
				else if (CRSI[i]<lmin) lmin=CRSI[i];
			}
			
			mstep=(lmax-lmin)/100;

			db=0;
			for (int steps=0; steps<=100; steps++)
			{
				testvalue=lmin+(mstep*steps);
				above=0; below=0;
				
				for (int m=0; m<CyclicMemory; m++)
					if (CRSI[m]>=testvalue) above++; else below++;
					
				ratio=(double)below / (double)CyclicMemory;
				if (ratio>=aperc)  { db=testvalue; break; }
			}
			
			
			ub=0;
			for (int steps=0; steps<=100; steps++)
			{
				testvalue=lmax-(mstep*steps);
				above=0; below=0;
				
				for (int m=0; m<CyclicMemory; m++)
					if (CRSI[m]>=testvalue) above++; else below++;
					
				ratio=(double)above / (double)CyclicMemory;
				if (ratio>=aperc)  { ub=testvalue; break; }
			}
			
			cRSIUpper[0]=ub;
			cRSILower[0]=db;
			cRSINeutral[0]=(double)((ub+db)/2);
			
			
			/// Color bars
			if ( CRSI[0] >= cRSIUpper[0] - (cRSIUpper[0] * 0.02) ) {
				CandleOutlineBrush = Brushes.Red;
				BarBrush = Brushes.Red;
				Print("Sell");
			}
			
			if ( CRSI[0] <= cRSILower[0] + (cRSILower[0]  * 0.02) ) {
				CandleOutlineBrush = Brushes.DodgerBlue;
				BarBrush = Brushes.DodgerBlue;
			}
			
//			double line = CRSI[0];
//			double band = ub;
			
//			if (CrossBelow(CRSI[0], band, 1))
//   			{
				
//			}
			
//			if (CrossBelow(CRSI[0], cRSIUpper[0], 1)) {
				
//			}
			
			
		}

		#region Properties
		[NinjaScriptProperty]
		[Range(5, int.MaxValue)]
		[Display(Name="CycleLength", Description="Dominant Cycle Length", Order=1, GroupName="Parameters")]
		public int CycleLength
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(2, 100)]
		[Display(Name="Vibration", Description="Vibration", Order=2, GroupName="Parameters")]
		public int Vibration
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(5, 200)]
		[Display(Name="CyclicMemory", Description="Cyclic Memory", Order=3, GroupName="Parameters")]
		public int CyclicMemory
		{ get; set; }
		
		[NinjaScriptProperty]
		[Range(5, 100)]
		[Display(Name="Leveling", Description="Leveling Sensor", Order=4, GroupName="Parameters")]
		public int Leveling
		{ get; set; }

		[Browsable(false)]
		[XmlIgnore]
		public Series<double> CRSI
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> cRSIUpper
		{
			get { return Values[1]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> cRSINeutral
		{
			get { return Values[2]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> cRSILower
		{
			get { return Values[3]; }
		}
		#endregion

	}
}

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private WTTcRSIColor[] cacheWTTcRSIColor;
		public WTTcRSIColor WTTcRSIColor(int cycleLength, int vibration, int cyclicMemory, int leveling)
		{
			return WTTcRSIColor(Input, cycleLength, vibration, cyclicMemory, leveling);
		}

		public WTTcRSIColor WTTcRSIColor(ISeries<double> input, int cycleLength, int vibration, int cyclicMemory, int leveling)
		{
			if (cacheWTTcRSIColor != null)
				for (int idx = 0; idx < cacheWTTcRSIColor.Length; idx++)
					if (cacheWTTcRSIColor[idx] != null && cacheWTTcRSIColor[idx].CycleLength == cycleLength && cacheWTTcRSIColor[idx].Vibration == vibration && cacheWTTcRSIColor[idx].CyclicMemory == cyclicMemory && cacheWTTcRSIColor[idx].Leveling == leveling && cacheWTTcRSIColor[idx].EqualsInput(input))
						return cacheWTTcRSIColor[idx];
			return CacheIndicator<WTTcRSIColor>(new WTTcRSIColor(){ CycleLength = cycleLength, Vibration = vibration, CyclicMemory = cyclicMemory, Leveling = leveling }, input, ref cacheWTTcRSIColor);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.WTTcRSIColor WTTcRSIColor(int cycleLength, int vibration, int cyclicMemory, int leveling)
		{
			return indicator.WTTcRSIColor(Input, cycleLength, vibration, cyclicMemory, leveling);
		}

		public Indicators.WTTcRSIColor WTTcRSIColor(ISeries<double> input , int cycleLength, int vibration, int cyclicMemory, int leveling)
		{
			return indicator.WTTcRSIColor(input, cycleLength, vibration, cyclicMemory, leveling);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.WTTcRSIColor WTTcRSIColor(int cycleLength, int vibration, int cyclicMemory, int leveling)
		{
			return indicator.WTTcRSIColor(Input, cycleLength, vibration, cyclicMemory, leveling);
		}

		public Indicators.WTTcRSIColor WTTcRSIColor(ISeries<double> input , int cycleLength, int vibration, int cyclicMemory, int leveling)
		{
			return indicator.WTTcRSIColor(input, cycleLength, vibration, cyclicMemory, leveling);
		}
	}
}

#endregion
