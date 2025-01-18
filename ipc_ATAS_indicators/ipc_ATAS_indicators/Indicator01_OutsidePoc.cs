using System;
using System.ComponentModel;
using ATAS.Indicators;
using Utils.Common.Logging;


namespace ipc_ATAS_indicators
{
    [Category("index-performance")]  //-- [Category("Clusters, Profiles, Levels")]
    [DisplayName("ipc_OutsidePoc")]
    public class Indicator01_OutsidePoc : Indicator
    {
        #region Properties
        private ValueDataSeries _OutsidePocSeriesBull = new ValueDataSeries("ospBull")
        {
            VisualType = VisualMode.UpArrow  //-- ?? ArrowColor
        };
        private ValueDataSeries _OutsidePocSeriesBear = new ValueDataSeries("ospBear")
        {
            VisualType = VisualMode.DownArrow
        };
        #endregion
        //--------

        #region DataSeries
        public void OutSidePocBull()
        {
            DataSeries[0] = _OutsidePocSeriesBull;
            base.DataSeries.Add(_OutsidePocSeriesBear);
        }
        #endregion
        //--------

        protected override void OnInitialize()
        {
            this.LogInfo($"'ipc_OutsidePoc' added ... ");
        }
        //-----------------------------------------------

        protected override void OnCalculate(int bar, decimal value)
        {
            IndicatorCandle candle = GetCandle(bar - 1);
            PriceVolumeInfo poc = candle.MaxVolumePriceInfo;
            decimal pocValue = poc.Price;


            if (candle.Open < candle.Close)  //-- bullish Candle
            {
                if (pocValue < candle.Open || pocValue > candle.Close)
                {
                    _OutsidePocSeriesBull[bar - 1] = candle.Low - 2 * base.InstrumentInfo.TickSize;
                }
            }
            else if (candle.Open > candle.Close)  //-- bearish Candle
            {
                if (pocValue > candle.Open || pocValue < candle.Close)
                {
                    _OutsidePocSeriesBear[bar - 1] = candle.High + 2 * base.InstrumentInfo.TickSize;
                }

            }
        }
    }
}

