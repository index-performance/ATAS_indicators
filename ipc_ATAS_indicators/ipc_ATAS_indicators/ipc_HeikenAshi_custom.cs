namespace ATAS.Indicators.IndexPerformance
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

    [Category("IndexPerformance")]
    [DisplayName("ipc_HeikenAshi_custom")]

    public class ipc_HeikenAshi_custom : Indicator
    {
        #region Fields

        // private readonly CrossColor _transparent = System.Drawing.Color.Transparent.Convert();    // !! CrossColor 

        private readonly PaintbarsDataSeries _bars = new("BarsId", "Bars"); // { IsHidden = true };  // !!
        private readonly CandleDataSeries _candles = new("Candles", "Heiken Ashi");
        private int _days;
        private int _targetBar;

        #endregion

        #region Properties

        [Display(Name = "Days", GroupName = "Settings", Order = 10)]
        public int Days
        {
            get => _days;
            set
            {
                if (value < 0)
                    return;

                _days = value;
                RecalculateValues();
            }
        }

        #endregion

        #region ctor

        public ipc_HeikenAshi_custom()
            : base(true)
        {
            Panel = IndicatorDataProvider.NewPanel;  // !!

            _days = 20;
            //DenyToChangePanel = true;  // !!
            DataSeries[0] = _bars;
            DataSeries.Add(_candles);
        }

        #endregion

        #region Protected methods

        protected override void OnApplyDefaultColors()
        {
            if (ChartInfo is null)
                return;

            _candles.UpCandleColor = ChartInfo.ColorsStore.UpCandleColor.Convert();
            _candles.DownCandleColor = ChartInfo.ColorsStore.DownCandleColor.Convert();
            _candles.BorderColor = ChartInfo.ColorsStore.BarBorderPen.Color.Convert();
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            // _bars[bar] = System.Drawing.Color.FromArgb( 0, 255, 255, 255).Convert();  // = _transparent;  !!

            if (bar == 0)
            {
                if (_days > 0)
                {
                    var days = 0;

                    for (var i = CurrentBar - 1; i >= 0; i--)
                    {
                        _targetBar = i;

                        if (!IsNewSession(i))
                            continue;

                        days++;

                        if (days == _days)
                            break;
                    }
                }
            }

            if (bar < _targetBar)
                return;

            if (bar == _targetBar)
            {
                var candle = GetCandle(bar);

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
                var candle = GetCandle(bar);
                var prevCandle = _candles[bar - 1];
                var close = (candle.Open + candle.Close + candle.High + candle.Low) * 0.25m;
                var open = (prevCandle.Open + prevCandle.Close) * 0.5m;
                var high = Math.Max(Math.Max(close, open), candle.High);
                var low = Math.Min(Math.Min(close, open), candle.Low);

                _candles[bar] = new Candle
                {
                    Close = close,
                    High = high,
                    Low = low,
                    Open = open
                };
            }
        }

        #endregion
    }
}