﻿namespace ATAS.Indicators.IndexPerformance
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using ATAS.Indicators;        //-- <HintPath> ..\..\..\..\Program Files(x86)\ATAS Platform\ATAS.Indicators.dll
    using ATAS.Indicators.Drawing;
    using Utils.Common.Attributes;
    using Utils.Common.Logging;   //-- <HintPath> ..\..\..\..\Program Files(x86)\ATAS Platform\Utils.Common.dll

    using OFT.Attributes;
    using OFT.Localization;
    using ATAS.Indicators.Technical;

    [Category("IndexPerformance")]
    [DisplayName("ipc_HeikenAshiS_custom")]

    public class ipc_HeikenAshiS_custom : Indicator
    {
        #region Fields

        private readonly PaintbarsDataSeries _bars = new("BarsId", "Bars");
        //{
        //    IsHidden = true,
        //    HideChart = true
        //};

        private bool _newPanel;

        private readonly CandleDataSeries _candles = new("Candles");
        private readonly SMMA _smmaClose = new();
        private readonly SMMA _smmaHigh = new();
        private readonly SMMA _smmaLow = new();
        private readonly SMMA _smmaOpen = new();
        private readonly CandleDataSeries _smoothedCandles = new("SmoothedCandles", Strings.Visualization);
        private readonly WMA _wmaClose = new();
        private readonly WMA _wmaHigh = new();
        private readonly WMA _wmaLow = new();
        private readonly WMA _wmaOpen = new();

        #endregion

        #region Properties

        [Display(Name = "Smoothed moving average", GroupName = "Settings", Order = 100)]
        public int SmmaPeriod
        {
            get => _smmaOpen.Period;
            set
            {
                if (value <= 0)
                    return;

                _smmaOpen.Period = _smmaClose.Period = _smmaHigh.Period = _smmaLow.Period = value;
                RecalculateValues();
            }
        }

        [Display(Name = "Weighted moving average", GroupName = "Settings", Order = 110)]
        public int WmaPeriod
        {
            get => _wmaOpen.Period;
            set
            {
                if (value <= 0)
                    return;

                _wmaOpen.Period = _wmaClose.Period = _wmaHigh.Period = _wmaLow.Period = value;
                RecalculateValues();
            }
        }

        //[Display(Name = "New Panel", GroupName = "Visualization", Order = 200)]
        //public bool newPanel
        //{
        //    get => _newPanel;
        //    set => _newPanel = value;
        //}

        //[Display(Name = "Show Bars", GroupName = "Visualization", Order = 210)]
        //public bool ShowBars
        //{
        //    get => !_bars.HideChart;
        //    set => _bars.HideChart = !value;
        //}

        #endregion

        #region ctor

        public ipc_HeikenAshiS_custom()
            : base(true)
        {

            Panel = IndicatorDataProvider.NewPanel;
            //DenyToChangePanel = true;

            _wmaOpen.Period = _wmaClose.Period = _wmaHigh.Period = _wmaLow.Period = 4;
            _smmaOpen.Period = _smmaClose.Period = _smmaHigh.Period = _smmaLow.Period = 4;
            DataSeries[0] = _bars;
            DataSeries.Add(_smoothedCandles);
        }

        #endregion

        #region Protected methods

        protected override void OnApplyDefaultColors()
        {
            if (ChartInfo is null)
                return;

            _smoothedCandles.UpCandleColor = ChartInfo.ColorsStore.UpCandleColor.Convert();
            _smoothedCandles.DownCandleColor = ChartInfo.ColorsStore.DownCandleColor.Convert();
            _smoothedCandles.BorderColor = ChartInfo.ColorsStore.BarBorderPen.Color.Convert();
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            var candle = GetCandle(bar);

            _smmaOpen.Calculate(bar, candle.Open);
            _smmaClose.Calculate(bar, candle.Close);
            _smmaHigh.Calculate(bar, candle.High);
            _smmaLow.Calculate(bar, candle.Low);

            if (bar == 0)
            {
                _wmaOpen.Calculate(bar, candle.Open);
                _wmaClose.Calculate(bar, candle.Close);
                _wmaHigh.Calculate(bar, candle.High);
                _wmaLow.Calculate(bar, candle.Low);

                _candles[bar] = new Candle
                {
                    Close = candle.Close,
                    High = candle.High,
                    Low = candle.Low,
                    Open = candle.Open
                };
            }
            else
            {
                var open = (_candles[bar - 1].Open + _candles[bar - 1].Close) / 2;
                var high = Math.Max(_smmaHigh[bar], _candles[bar].Open);
                var low = Math.Min(_smmaLow[bar], open);
                var close = (_smmaOpen[bar] + _smmaHigh[bar] + _smmaLow[bar] + _smmaClose[bar]) / 4;

                _candles[bar] = new Candle
                {
                    Close = close,
                    High = high,
                    Low = low,
                    Open = open
                };

                var smoothedCandle = new Candle();

                smoothedCandle.Open = _wmaOpen.Calculate(bar, open);
                smoothedCandle.Close = _wmaClose.Calculate(bar, close);
                smoothedCandle.High = _wmaHigh.Calculate(bar, high);
                smoothedCandle.Low = _wmaLow.Calculate(bar, low);

                _smoothedCandles[bar] = smoothedCandle;
            }
        }

        #endregion
    }
}