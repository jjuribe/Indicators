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
using NinjaTrader.NinjaScript.DrawingTools;

#endregion

//This namespace holds Indicators in this folder and is required. Do not change it.

namespace NinjaTrader.NinjaScript.Indicators
{
	public class TSSuperTrend : Indicator
	{
		
		private Series<double> Avg;
		private Series<bool> _trend;
		private double _offset;
        private double _th;
        private double _tl = double.MaxValue;
        private int _thisbar = -1;
		private double jI;
		private double jQ;		
		private double[] CSmooth			= new double[] { 0, 0, 0, 0, 0, 0, 0 };
		private double[] Detrender		= new double[] { 0, 0, 0, 0, 0, 0, 0 };
		private double[] I1				= new double[] { 0, 0, 0, 0, 0, 0, 0 };
		private double[] Q1				= new double[] { 0, 0, 0, 0, 0, 0, 0 };
		private double[] I2				= new double[] { 0, 0, };
		private double[] Q2				= new double[] { 0, 0, };
		private double[] Re				= new double[] { 0, 0, };
		private double[] Im				= new double[] { 0, 0, };
		private double[] Period			= new double[] { 0, 0, };
		private double[] SmoothPeriod 	= new double[] { 0, 0, };
		
		protected override void OnStateChange()
		{
			if (State == State.SetDefaults)
			{
				Description							= @"Indicator TSSuperTrend";
				Name								= "TSSuperTrend";				
				Calculate							= Calculate.OnBarClose;
				IsOverlay							= true;
				DisplayInDataBox					= true;
				DrawOnPricePanel					= true;
				DrawHorizontalGridLines				= true;
				DrawVerticalGridLines				= true;
				PaintPriceMarkers					= true;
				ScaleJustification					= NinjaTrader.Gui.Chart.ScaleJustification.Right;
				//Disable this property if your indicator requires custom values that cumulate with each new market data event. 
				//See Help Guide for additional information.
				IsSuspendedWhileInactive			= true;
				Length					= 14;
				Multiplier				= 2.2;
				Smooth					= 14;
       			MaType = MovingAverageType.HMA;
        		StMode = SuperTrendMode.ATR;				
				ShowIndicator			= true;
				ShowArrows				= false;
				ColorBars				= false;
				PlayAlert				= false;
				UpColor					= Brushes.DodgerBlue;
				DownColor				= Brushes.Red;
				AddPlot(new Stroke(Brushes.Transparent, 2), PlotStyle.Hash, "TrendPlot");
			}
			else if (State == State.Configure)
			{
				Avg = new Series<double>(this, MaximumBarsLookBack.Infinite);
				_trend = new Series<bool>(this, MaximumBarsLookBack.Infinite);
				
			}
		}

		protected override void OnBarUpdate()
		{
            if (CurrentBar > Smooth && CurrentBar > Length)
            {
                    switch (MaType)
                    {
                        case MovingAverageType.SMA:
                            Avg[0] = SMA(Input, Smooth)[0];
                            break;
                        case MovingAverageType.SMMA:
                            Avg[0] = SMMA(Input, Smooth)[0];
                            break;
                        case MovingAverageType.TMA:
                            Avg[0] = TMA(Input, Smooth)[0];
                            break;
                        case MovingAverageType.WMA:
                            Avg[0] = WMA(Input, Smooth)[0];
                            break;
                        case MovingAverageType.VWMA:
                            Avg[0] = VWMA(Input, Smooth)[0];
                            break;
                        case MovingAverageType.TEMA:
                           	Avg[0] = TEMA(Input, Smooth)[0];
                            break;
                        case MovingAverageType.HMA:
                            Avg[0] = HMA(Input, Smooth)[0];
                            break;
                        case MovingAverageType.VMA:
                            Avg[0] = VMA(Input, Smooth, Smooth)[0];
                            break;
                        default:
                            Avg[0] = EMA(Input, Smooth)[0];
                            break;
					}
			}
			else
			{

                _trend[0] = (true);
                Values[0][0] = (Input[0]);
                return;
            }
				
           switch (StMode)
            {
                case SuperTrendMode.ATR:
                    _offset = ATR(Length)[0] * Multiplier;
                    break;
                case SuperTrendMode.Adaptive:
				   double per = CycleSmootherPeriod();
                    _offset = ATR(Length)[0] *  per/ 10;				 
                    break;
                case SuperTrendMode.DualThrust:
                    _offset = Dtt(Length, Multiplier);
                    break;
				    
            }
			
		  	if (Close[0] > Value[1])
			{
				_trend[0] = (true);
			}
			else if (Close[0] < Value[1])
			{
				_trend[0] = (false);
			}
			else
			{
				_trend[0] = (_trend[1]);
			}

            if (_trend[0] && !_trend[1])
            {
                _th = High[0];
                Value[0] = (Math.Max(Avg[0] - _offset, _tl));
				if (ShowIndicator)
				{
					PlotBrushes[0][0] = UpColor;
				}
				else
				{
					PlotBrushes[0][0] = Brushes.Transparent;
				}
                if (ShowArrows)
				{
                    Draw.ArrowUp(this, CurrentBar.ToString(), true, 0, Value[0] - TickSize, UpColor);
                }
				if (ColorBars)
					{
						BarBrush = UpColor;
					}
				if(PlayAlert && _thisbar != CurrentBar)
                {
                    _thisbar = CurrentBar;
                   PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav");
                }
            }
            else
                if (!_trend[0] && _trend[1])
                {
                    _tl = Low[0];
                    Value[0] = (Math.Min(Avg[0] + _offset, _th));
					if (ShowIndicator)
					{
						PlotBrushes[0][0] = DownColor;
					}
					else
					{
						PlotBrushes[0][0] = Brushes.Transparent;
					}
                    if (ShowArrows)
					{
                        Draw.ArrowDown(this, CurrentBar.ToString(), true, 0, Value[0] + TickSize, DownColor);
                   	}
					if (ColorBars)
					{
						BarBrush = DownColor;
					}
					if (PlayAlert && _thisbar != CurrentBar)
                    {
                        _thisbar = CurrentBar;
                        PlaySound(NinjaTrader.Core.Globals.InstallDir + @"\sounds\Alert1.wav");
                    }
                }
                else
                {
                    if (_trend[0])
                    {
                        Value[0] = ((Avg[0] - _offset) > Value[1] ? (Avg[0] - _offset) : Value[1]);
                        _th = Math.Max(_th, High[0]);
						if (ShowIndicator)
						{
							PlotBrushes[0][0] = UpColor;
						}
						else
						{
							PlotBrushes[0][0] = Brushes.Transparent;
						}
						if (ColorBars)
						{
							BarBrush = UpColor;
						}
                    }
                    else
                    {
                        Value[0] = ((Avg[0] + _offset) < Value[1] ? (Avg[0] + _offset) : Value[1]);
                        _tl = Math.Min(_tl, Low[0]);
						if (ShowIndicator)
						{
							PlotBrushes[0][0] = DownColor;
						}
						else
						{
							PlotBrushes[0][0] = Brushes.Transparent;
						}
						if (ColorBars)
						{
							BarBrush = DownColor;
						}
                    }
                    RemoveDrawObject(CurrentBar.ToString());
                }
		}
		
       private double Dtt(int nDay, double mult)
        {
            double hh = MAX(High, nDay)[0];
            double hc = MAX(Close, nDay)[0];
            double ll = MIN(Low, nDay)[0];
            double lc = MIN(Close, nDay)[0];
            return mult * Math.Max((hh - lc), (hc - ll));
        }
		
		private double CycleSmootherPeriod()		
		{			
			// We have to move things along the arrays, but enforce only doing it once per bar.
			
			for ( int i = 6; i > 0; i-- )
			{
				CSmooth[i] = CSmooth[i-1];
				Detrender[i] = Detrender[i-1];
				I1[i] = I1[i-1];
				Q1[i] = Q1[i-1];
			}
			I2[1] = I2[0];
			Q2[1] = Q2[0];
			Re[1] = Re[0];
			Im[1] = Im[0];				
			
						
			CSmooth[0] = ((4*Median[0] + 3*Median[1] + 2*Median[2] + Median[3])/10);
			Detrender[0] = ((0.0962*CSmooth[0]+0.5769*CSmooth[2]-0.5769*CSmooth[4]-0.0962*CSmooth[6])*(0.075*Period[1]+.54));
			
			
			//InPhase and Quadrature components			
			Q1[0] = ((0.0962*Detrender[0]+0.5769*Detrender[2]-0.5769*Detrender[4]-0.0962*Detrender[6])*(0.075*Period[1]+0.54));			
			I1[0] = (Detrender[3]);
			
			//Advance the phase of I1 and Q1 by 90 degrees
			jI = ((0.0962*I1[0]+0.5769*I1[2]-0.5769*I1[4]-0.0962*I1[6])*(0.075*Period[1]+.54));
			jQ = ((0.0962*Q1[0]+0.5769*Q1[2]-0.5769*Q1[4]-0.0962*Q1[6])*(0.075*Period[1]+.54));
			
			//Phasor Addition
			I2[0] = (I1[0]-jQ);
			Q2[0] = (Q1[0]+jI);
			
			//Smooth the I and Q components before applying the discriminator
			I2[0] = (0.2*I2[0]+0.8*I2[1]);
			Q2[0] = (0.2*Q2[0]+0.8*Q2[1]);
			
			//Homodyne Discriminator
			Re[0] = (I2[0]*I2[1] + Q2[0]*Q2[1]);
			Im[0] = (I2[0]*Q2[1] - Q2[0]*I2[1]);
			Re[0] = (0.2*Re[0] + 0.8*Re[1]);
			Im[0] = (0.2*Im[0] + 0.8*Im[1]);
			
			double rad2Deg = 180.0 / (4.0 * Math.Atan (1));
			
			if(Im[0]!=0 && Re[0]!=0)
				Period[0] = (360/(Math.Atan(Im[0]/Re[0])*rad2Deg ));
			
			if(Period[0]>(1.5*Period[1]))
				Period[0] = (1.5*Period[1]);
			
			if(Period[0]<(0.67*Period[1]))
				Period[0] = (0.67*Period[1]);
			
			if(Period[0]<6)
				Period[0] = (6);
			
			if(Period[0]>50)
				Period[0] = (50);
			
			Period[0] = (0.2*Period[0] + 0.8*Period[1]);
			SmoothPeriod[0] = (0.33*Period[0] + 0.67*SmoothPeriod[1]);
				            
            return SmoothPeriod[0];
		}	

		#region Properties
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<double> TrendPlot
		{
			get { return Values[0]; }
		}
		
		[Browsable(false)]
		[XmlIgnore]
		public Series<bool> Trend
		{
			get { return _trend; }
		}		
		
		[NinjaScriptProperty]
        [Display(Name = "ST Mode", Description = "SuperTrend Mode", Order = 1, GroupName = "1. Parameters")]
        public SuperTrendMode StMode
        { get; set; }
		
		[NinjaScriptProperty]
        [Display(Name = "MA Type", Description = "Moving average Type", Order = 2, GroupName = "1. Parameters")]
        public MovingAverageType MaType
        { get; set; }		
		
		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Length", Description="ATR Period", Order=3, GroupName="1. Parameters")]
		public int Length
		{ get; set; }

		[Range(0.01, double.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Multiplier", Order=4, GroupName="1. Parameters")]
		public double Multiplier
		{ get; set; }

		[Range(1, int.MaxValue)]
		[NinjaScriptProperty]
		[Display(Name="Smooth", Description="MA Period", Order=5, GroupName="1. Parameters")]
		public int Smooth
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ShowIndicator", Order=6, GroupName="1. Parameters")]
		public bool ShowIndicator
		{ get; set; }		

		[NinjaScriptProperty]
		[Display(Name="ShowArrows", Order=7, GroupName="1. Parameters")]
		public bool ShowArrows
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="ColorBars", Order=8, GroupName="1. Parameters")]
		public bool ColorBars
		{ get; set; }
		
		[NinjaScriptProperty]
		[Display(Name="PlayAlert", Order=9, GroupName="1. Parameters")]
		public bool PlayAlert
		{ get; set; }		

		[XmlIgnore()]
		[Display(Name = "UpColor", GroupName = "2. Colors", Order = 1)]
		public Brush UpColor
		{ get; set; }
		
		[XmlIgnore()]
		[Display(Name = "DownColor", GroupName = "2. Colors", Order = 2)]
		public Brush DownColor
		{ get; set; }
		
		[Browsable(false)]
		public string UpColorSerialize
		{
		   get { return Serialize.BrushToString(UpColor); }
		   set { UpColor = Serialize.StringToBrush(value); }
		}
		
		[Browsable(false)]
		public string DownColorSerialize
		{
		   get { return Serialize.BrushToString(DownColor); }
		   set { DownColor = Serialize.StringToBrush(value); }
		}		
		#endregion
	}
}

	public enum SuperTrendMode
    {
        ATR,
        DualThrust,
        Adaptive
    }

    public enum MovingAverageType
    {
        SMA,
        SMMA,
        TMA,
        WMA,
        VWMA,
        TEMA,
        HMA,
        EMA,
        VMA
    }

#region NinjaScript generated code. Neither change nor remove.

namespace NinjaTrader.NinjaScript.Indicators
{
	public partial class Indicator : NinjaTrader.Gui.NinjaScript.IndicatorRenderBase
	{
		private TSSuperTrend[] cacheTSSuperTrend;
		public TSSuperTrend TSSuperTrend(SuperTrendMode stMode, MovingAverageType maType, int length, double multiplier, int smooth, bool showIndicator, bool showArrows, bool colorBars, bool playAlert)
		{
			return TSSuperTrend(Input, stMode, maType, length, multiplier, smooth, showIndicator, showArrows, colorBars, playAlert);
		}

		public TSSuperTrend TSSuperTrend(ISeries<double> input, SuperTrendMode stMode, MovingAverageType maType, int length, double multiplier, int smooth, bool showIndicator, bool showArrows, bool colorBars, bool playAlert)
		{
			if (cacheTSSuperTrend != null)
				for (int idx = 0; idx < cacheTSSuperTrend.Length; idx++)
					if (cacheTSSuperTrend[idx] != null && cacheTSSuperTrend[idx].StMode == stMode && cacheTSSuperTrend[idx].MaType == maType && cacheTSSuperTrend[idx].Length == length && cacheTSSuperTrend[idx].Multiplier == multiplier && cacheTSSuperTrend[idx].Smooth == smooth && cacheTSSuperTrend[idx].ShowIndicator == showIndicator && cacheTSSuperTrend[idx].ShowArrows == showArrows && cacheTSSuperTrend[idx].ColorBars == colorBars && cacheTSSuperTrend[idx].PlayAlert == playAlert && cacheTSSuperTrend[idx].EqualsInput(input))
						return cacheTSSuperTrend[idx];
			return CacheIndicator<TSSuperTrend>(new TSSuperTrend(){ StMode = stMode, MaType = maType, Length = length, Multiplier = multiplier, Smooth = smooth, ShowIndicator = showIndicator, ShowArrows = showArrows, ColorBars = colorBars, PlayAlert = playAlert }, input, ref cacheTSSuperTrend);
		}
	}
}

namespace NinjaTrader.NinjaScript.MarketAnalyzerColumns
{
	public partial class MarketAnalyzerColumn : MarketAnalyzerColumnBase
	{
		public Indicators.TSSuperTrend TSSuperTrend(SuperTrendMode stMode, MovingAverageType maType, int length, double multiplier, int smooth, bool showIndicator, bool showArrows, bool colorBars, bool playAlert)
		{
			return indicator.TSSuperTrend(Input, stMode, maType, length, multiplier, smooth, showIndicator, showArrows, colorBars, playAlert);
		}

		public Indicators.TSSuperTrend TSSuperTrend(ISeries<double> input , SuperTrendMode stMode, MovingAverageType maType, int length, double multiplier, int smooth, bool showIndicator, bool showArrows, bool colorBars, bool playAlert)
		{
			return indicator.TSSuperTrend(input, stMode, maType, length, multiplier, smooth, showIndicator, showArrows, colorBars, playAlert);
		}
	}
}

namespace NinjaTrader.NinjaScript.Strategies
{
	public partial class Strategy : NinjaTrader.Gui.NinjaScript.StrategyRenderBase
	{
		public Indicators.TSSuperTrend TSSuperTrend(SuperTrendMode stMode, MovingAverageType maType, int length, double multiplier, int smooth, bool showIndicator, bool showArrows, bool colorBars, bool playAlert)
		{
			return indicator.TSSuperTrend(Input, stMode, maType, length, multiplier, smooth, showIndicator, showArrows, colorBars, playAlert);
		}

		public Indicators.TSSuperTrend TSSuperTrend(ISeries<double> input , SuperTrendMode stMode, MovingAverageType maType, int length, double multiplier, int smooth, bool showIndicator, bool showArrows, bool colorBars, bool playAlert)
		{
			return indicator.TSSuperTrend(input, stMode, maType, length, multiplier, smooth, showIndicator, showArrows, colorBars, playAlert);
		}
	}
}

#endregion
