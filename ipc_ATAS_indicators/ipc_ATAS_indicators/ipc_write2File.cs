namespace ATAS.Indicators.IndexPerformance
{
    using System;
    using System.IO;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using ATAS.Indicators;        //-- <HintPath> ..\..\..\..\Program Files(x86)\ATAS Platform\ATAS.Indicators.dll
    using ATAS.Indicators.Drawing;
    using Microsoft.VisualBasic;
    using Utils.Common.Attributes;
    using Utils.Common.Logging;   //-- <HintPath> ..\..\..\..\Program Files(x86)\ATAS Platform\Utils.Common.dll


    [Category("IndexPerformance")]
    [DisplayName("ipc_write2File")]

    public class ipc_write2File : Indicator
    {
        #region Fields
        private int _period = 10;

        private string pathFile;
        private StreamWriter sw;


        public decimal bXmaxVol_Pi__Price;
        public decimal bXmaxVol_Pi__Volume;
        public decimal bXmaxVol_Pi__bid;
        public decimal bXmaxVol_Pi__ask;

        #endregion
        //--------------

        #region DataSeries

        private ValueDataSeries _Xwrite2File_Series = new("Xwrite2File_Series", "Xwrite2File")
        {
            //Color = DefaultColors.Lime.Convert(),
            Color = System.Drawing.Color.FromArgb(255, 238, 141, 61).Convert(),
            VisualType = VisualMode.Line
        };

        #endregion
        //--------

        public ipc_write2File()
        {
            Panel = IndicatorDataProvider.NewPanel;  //  ?? hidden
            DataSeries[0] = _Xwrite2File_Series;
            ((ValueDataSeries)DataSeries[0]).VisualType = VisualMode.Hide;
        }

        protected override void OnInitialize()
        {
            this.LogInfo($"Indicator: " + this.GetType().Name + "  added.");

            pathFile = @"C:\temp\"
                     + InstrumentInfo.Instrument
                     + @"_"  + ChartInfo.ChartType + "." + ChartInfo.TimeFrame.Replace(@"/", ".").Replace(@"\", ".")
                     + @"__" + DateTime.Now.ToString("yyyyMMdd")
                     + @"_"  + DateTime.Now.ToString("HHmmss")
                     + @"_ticks.txt";

            sw = File.AppendText(pathFile);
            sw.WriteLine(InstrumentInfo.Instrument + @"_" + ChartInfo.ChartType + @"." + ChartInfo.TimeFrame + @"__data");
            sw.WriteLine("Instrument | Date | UTC_Time | LastTime "
                       + "|     Open |     High |      Low |    Close "
                       + "|   Volume |    Delta | maxDelta | minDelta "
                       + "|    Ticks "
                       + "| POCprice |   POCvol |   POCmax |   POCmin"
                       );

            sw.Close();
            sw.Dispose();
        }

        protected override void OnCalculate(int bar, decimal value)
        {
            //var instru = GetInstrument();
            var candle = GetCandle(bar);

            //POC
            PriceVolumeInfo maxVol_Pi = candle.MaxVolumePriceInfo;
            bXmaxVol_Pi__Price = maxVol_Pi.Price;
            bXmaxVol_Pi__Volume = maxVol_Pi.Volume;

            var volumeInfo = candle.GetPriceVolumeInfo(bXmaxVol_Pi__Price);
            bXmaxVol_Pi__bid = volumeInfo.Bid;
            bXmaxVol_Pi__ask = volumeInfo.Ask;
            //----


            sw = File.AppendText(pathFile);
            sw.WriteLine(InstrumentInfo.Instrument + " | " + candle.Time.ToString("dd.MM.yyyy")
                                                   + " | " + candle.Time.ToString("HH:mm:ss")
                                                   + " | " + candle.LastTime.ToString("HH:mm:ss")

                                                   + " | " + Strings.StrDup(8 - candle.Open.ToString().Length, @" ")     + candle.Open
                                                   + " | " + Strings.StrDup(8 - candle.High.ToString().Length, @" ")     + candle.High
                                                   + " | " + Strings.StrDup(8 - candle.Low.ToString().Length, @" ")      + candle.Low
                                                   + " | " + Strings.StrDup(8 - candle.Close.ToString().Length, @" ")    + candle.Close     // -- format "####0,00" 

                                                   + " | " + Strings.StrDup(8 - candle.Volume.ToString().Length, @" ")   + candle.Volume
                                                   + " | " + Strings.StrDup(8 - candle.Delta.ToString().Length, @" ")    + candle.Delta
                                                   + " | " + Strings.StrDup(8 - candle.MaxDelta.ToString().Length, @" ") + candle.MaxDelta
                                                   + " | " + Strings.StrDup(8 - candle.MinDelta.ToString().Length, @" ") + candle.MinDelta
                                                   + " | " + Strings.StrDup(8 - candle.Ticks.ToString().Length, @" ")    + candle.Ticks

                                                   + " | " + Strings.StrDup(8 - bXmaxVol_Pi__Price.ToString().Length, @" ")  + bXmaxVol_Pi__Price   // -- POCprice 
                                                   + " | " + Strings.StrDup(8 - bXmaxVol_Pi__Volume.ToString().Length, @" ") + bXmaxVol_Pi__Volume
                                                   + " | " + Strings.StrDup(8 - bXmaxVol_Pi__bid.ToString().Length, @" ")    + bXmaxVol_Pi__bid
                                                   + " | " + Strings.StrDup(8 - bXmaxVol_Pi__ask.ToString().Length, @" ")    + bXmaxVol_Pi__ask

                        );
            sw.Close();
            sw.Dispose();

            // return
            _Xwrite2File_Series[bar] = 0;

        }
    }
}

//----

//  https://docs.atas.net/en/classATAS_1_1Indicators_1_1IndicatorCandle.html#a631481685e09bde12e5c5021094b792f
//----