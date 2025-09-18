namespace ATAS.Indicators.IndexPerformance
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using ATAS.Indicators;        //-- <HintPath> ..\..\..\..\Program Files(x86)\ATAS Platform\ATAS.Indicators.dll
    using ATAS.Indicators.Drawing;
    using Microsoft.VisualBasic;
    using Utils.Common.Attributes;
    using Utils.Common.Logging;   //-- <HintPath> ..\..\..\..\Program Files(x86)\ATAS Platform\Utils.Common.dll


    [Category("IndexPerformance")]
    [DisplayName("ipc_Harami")]
    public class ipc_Harami : Indicator
    {
        #region Fields
        private int _vmulti = 1;  // default = 1 ...
        private int _period = 5;  // default 5 bars

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

        [Display(Name = "Period", GroupName = "Settings", Order = 20)]
        [Range(1, 100)]
        public int Period
        {
            get => _period;
            set
            {
                if (_period == value)
                    return;

                if (value <= 0)
                    return;

                _period = value;

                RaisePropertyChanged(nameof(Period));
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
        public ipc_Harami()
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
            decimal b1open = 0;
            decimal b1close = 0;

            decimal b0open = 0;
            decimal b0close = 0;

            var cAbove = 0;
            var cBelow = 0;

            if (bar > 2)   //-- avoid "Index was out of range.Error"
            {
                b1open  = GetCandle(bar - 1).Open;
                b1close = GetCandle(bar - 1).Close;

                b0open  = GetCandle(bar - 0).Open;
                b0close = GetCandle(bar - 0).Close;
            }

            var start = Math.Max(0, bar - Period + 1);
            var count = Math.Min(bar + 1, Period);

            var max = (decimal)SourceDataSeries[start];
            var min = (decimal)SourceDataSeries[start];

            for (var i = start + 1; i < start + count; i++)
            {
                max = Math.Max(max, (decimal)SourceDataSeries[i]);
                min = Math.Min(min, (decimal)SourceDataSeries[i]);
            }
            //----


            // bear  candle1 UP       && candle0 DOWN     && b1 highest     && 
            if (b1open < b1close && b0open > b0close && b1close == max && b1close > b0open && b1open < b0close)
            {
                cAbove = 0;
                cBelow = (-1 * vMulti);
            }
            // bull  candle1 DOWN     && candle0 UP       && b1 lowest      && 
            else if (b1open > b1close && b0open < b0close && b1close == min && b1open > b0close && b1close < b0open)
            {
                cAbove = (+1 * vMulti);
                cBelow = 0;
            }
            // ....       
            else
            {
                cAbove = 0;
                cBelow = 0;
            }

            // return
            this[bar] = cAbove + cBelow;  // the hidden ValueDataSeries
            _Xlong_Series[bar]  = cAbove;
            _Xshort_Series[bar] = cBelow;

        }
    }
}

//----
