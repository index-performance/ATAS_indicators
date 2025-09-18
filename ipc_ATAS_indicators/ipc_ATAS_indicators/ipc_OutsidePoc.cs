namespace ATAS.Indicators.IndexPerformance
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using ATAS.Indicators;        //-- <HintPath> ..\..\..\..\Program Files(x86)\ATAS Platform\ATAS.Indicators.dll
    using ATAS.Indicators.Drawing;
    using ATAS.Indicators.Technical;
    using Microsoft.VisualBasic;
    using Utils.Common.Attributes;
    using Utils.Common.Logging;   //-- <HintPath> ..\..\..\..\Program Files(x86)\ATAS Platform\Utils.Common.dll


    [Category("index-performance")]  //-- [Category("Clusters, Profiles, Levels")]
    [DisplayName("ipc_OutsidePoc")]
    public class ipc_OutsidePoc : Indicator
    {
        #region Fields
        private int _vmulti = 1;  // default = 1 ...
        private int _vticksdelta = 2;

        #endregion
        //--------------

        #region Settings

        [Display(Name = "ValueMultiplicator", GroupName = "Settings", Order = 10)]
        [Range(1, 10)]
        public int vMulti
        {
            get => _vmulti;
            set
            {
                if (_vmulti == value)
                    return;

                if (value <= 0)
                    return;

                _vmulti = value;

                RaisePropertyChanged(nameof(vMulti));
                RecalculateValues();
            }
        }

        [Display(Name = "TicksDelta", GroupName = "Settings", Order = 20)]
        public int vTicksDelta
        {
            get => _vticksdelta;
            set
            {
                if (_vticksdelta == value)
                    return;

                if (value <= 0)
                    return;

                _vticksdelta = value;

                RaisePropertyChanged(nameof(_vticksdelta));
                RecalculateValues();
            }
        }
        #endregion
        //--------

        #region DataSeries

        private ValueDataSeries _Xlong_Series = new("Xlong_Series", "X_long")
        {
            //Color = DefaultColors.Lime.Convert(),
            Color = System.Drawing.Color.FromArgb(255, 110, 142, 44).Convert(),  //-- FF6E8E2C  green
            VisualType = VisualMode.Histogram
        };

        private ValueDataSeries _Xshort_Series = new("Xshort_Series", "X_short")
        {
            Color = System.Drawing.Color.FromArgb(255, 229, 117, 114).Convert(),  //-- FFE57572  red
            VisualType = VisualMode.Histogram
        };

        #endregion
        //--------
        public ipc_OutsidePoc()
        {
            Panel = IndicatorDataProvider.NewPanel;

            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide; //-- 4 returnValues 

            DataSeries.Add(_Xlong_Series);
            DataSeries.Add(_Xshort_Series);
        }

        protected override void OnInitialize()
        {
            this.LogInfo($"Indicator: " + this.GetType().Name + "  added.");
        }
        protected override void OnRecalculate()
        {
            DataSeries.ForEach(x => x.Clear());
        }
        protected override void OnCalculate(int bar, decimal value)
        {
            decimal b0open = GetCandle(bar - 0).Open;
            decimal b0close = GetCandle(bar - 0).Close;

            IndicatorCandle candle = GetCandle(bar - 0);
            PriceVolumeInfo pvi = candle.MaxVolumePriceInfo;
            decimal pocValue = pvi.Price;

            var cAbove = 0;
            var cBelow = 0;

            if (b0open < b0close && b0close < pocValue)  //-- candle UP & POC above
            {
                cAbove = 0;
                cBelow = (-1 * vMulti);
            }
            else if (b0open > b0close && b0close > pocValue)  //-- candle DOWN & POC below
            {
                cAbove = (+1 * vMulti);
                cBelow = 0;
            }
            else
            {
                cAbove = 0;
                cBelow = 0;
            }

            // return
            this[bar] = cAbove + cBelow;  // the hidden ValueDataSeries
            _Xlong_Series[bar] = cAbove;
            _Xshort_Series[bar] = cBelow;

        }
    }
}

//----
