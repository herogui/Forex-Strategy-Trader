﻿// Aroon Histogram Indicator
// Last changed on 2009-04-15
// Part of Forex Strategy Builder & Forex Strategy Trader
// Website http://forexsb.com/
// Copyright (c) 2006 - 2009 Miroslav Popov - All rights reserved.
// This code or any part of it cannot be used in other applications without a permission.

using System;
using System.Drawing;

namespace Forex_Strategy_Trader
{
    /// <summary>
    /// Aroon Histogram Indicator
    /// </summary>
    public class Aroon_Histogram : Indicator
    {
        /// <summary>
        /// Sets the default indicator parameters for the designated slot type
        /// </summary>
        public Aroon_Histogram(SlotTypes slotType)
        {
            // General properties
            IndicatorName  = "Aroon Histogram";
            PossibleSlots  = SlotTypes.OpenFilter | SlotTypes.CloseFilter;
            SeparatedChart = true;
            SeparatedChartMinValue = -100;
            SeparatedChartMaxValue = 100;
            IsDescreteValues = true; // <------------

            // Setting up the indicator parameters
            IndParam = new IndicatorParam();
            IndParam.IndicatorName = IndicatorName;
            IndParam.SlotType      = slotType;

            // The ComboBox parameters
            IndParam.ListParam[0].Caption  = "Logic";
            IndParam.ListParam[0].ItemList = new string[]
            {
                "The Aroon Histogram rises",
                "The Aroon Histogram falls",
                "The Aroon Histogram is higher than the Level line",
                "The Aroon Histogram is lower than the Level line",
                "The Aroon Histogram crosses the Level line upward",
                "The Aroon Histogram crosses the Level line downward",
                "The Aroon Histogram changes its direction upward",
                "The Aroon Histogram changes its direction downward"
            };
            IndParam.ListParam[0].Index    = 0;
            IndParam.ListParam[0].Text     = IndParam.ListParam[0].ItemList[IndParam.ListParam[0].Index];
            IndParam.ListParam[0].Enabled  = true;
            IndParam.ListParam[0].ToolTip  = "Logic of application of the indicator.";

            IndParam.ListParam[1].Caption  = "Base price";
            IndParam.ListParam[1].ItemList = Enum.GetNames(typeof(BasePrice));
            IndParam.ListParam[1].Index    = (int)BasePrice.Close;
            IndParam.ListParam[1].Text     = IndParam.ListParam[1].ItemList[IndParam.ListParam[1].Index];
            IndParam.ListParam[1].Enabled  = true;
            IndParam.ListParam[1].ToolTip  = "The price the Aroon is based on.";

            // The NumericUpDown parameters
            IndParam.NumParam[0].Caption   = "Period";
            IndParam.NumParam[0].Value     = 9;
            IndParam.NumParam[0].Min       = 1;
            IndParam.NumParam[0].Max       = 200;
            IndParam.NumParam[0].Enabled   = true;
            IndParam.NumParam[0].ToolTip   = "Period used to calculate the Aroon value.";

            IndParam.NumParam[1].Caption   = "Level";
            IndParam.NumParam[1].Value     = 0;
            IndParam.NumParam[1].Min       = -100;
            IndParam.NumParam[1].Max       = 100;
            IndParam.NumParam[1].Enabled   = true;
            IndParam.NumParam[1].ToolTip   = "A critical level (for the appropriate logic).";

            // The CheckBox parameters
            IndParam.CheckParam[0].Caption = "Use previous bar value";
            IndParam.CheckParam[0].Checked = PrepareUsePrevBarValueCheckBox(slotType);
            IndParam.CheckParam[0].Enabled = true;
            IndParam.CheckParam[0].ToolTip = "Use the indicator value from the previous bar.";

            return;
        }

        /// <summary>
        /// Calculates the indicator's components
        /// </summary>
        public override void Calculate(SlotTypes slotType)
        {
            // Reading the parameters
            BasePrice basePrice = (BasePrice)IndParam.ListParam[1].Index;
            int       iPeriod   = (int)IndParam.NumParam[0].Value;
            double    dLevel    = IndParam.NumParam[1].Value;
            int       iPrvs     = IndParam.CheckParam[0].Checked ? 1 : 0;

            // Calculation
            int      iFirstBar   = iPeriod + 2;
            double[] adBasePrice = Price(basePrice);
            double[] adUp        = new double[Bars];
            double[] adDown      = new double[Bars];
            double[] adAroon     = new double[Bars];

            for (int iBar = iPeriod; iBar < Bars; iBar++)
            {
                double dHighestHigh = double.MinValue;
                double dLowestLow   = double.MaxValue;
                for (int i = 0; i < iPeriod; i++)
                {
                    int iBaseBar = iBar - iPeriod + 1 + i;
                    if (adBasePrice[iBaseBar] > dHighestHigh)
                    {
                        dHighestHigh = adBasePrice[iBaseBar];
                        adUp[iBar] = 100.0 * i / (iPeriod - 1);
                    }
                    if (adBasePrice[iBaseBar] < dLowestLow)
                    {
                        dLowestLow = adBasePrice[iBaseBar];
                        adDown[iBar] = 100.0 * i / (iPeriod - 1);
                    }
                }
            }

            for (int iBar = iFirstBar; iBar < Bars; iBar++)
            {
                adAroon[iBar] = adUp[iBar] - adDown[iBar];
            }

            // Saving the components
            Component = new IndicatorComp[5];

            Component[0] = new IndicatorComp();
            Component[0].CompName  = "Aroon Histogram";
            Component[0].DataType  = IndComponentType.IndicatorValue;
            Component[0].ChartType = IndChartType.Histogram;
            Component[0].FirstBar  = iFirstBar;
            Component[0].Value     = adAroon;

            Component[1] = new IndicatorComp();
            Component[1].CompName   = "Aroon Up";
            Component[1].DataType   = IndComponentType.IndicatorValue;
            Component[1].ChartType  = IndChartType.Line;
            Component[1].ChartColor = Color.Green;
            Component[1].FirstBar   = iFirstBar;
            Component[1].Value      = adUp;

            Component[2] = new IndicatorComp();
            Component[2].CompName   = "Aroon Down";
            Component[2].DataType   = IndComponentType.IndicatorValue;
            Component[2].ChartType  = IndChartType.Line;
            Component[2].ChartColor = Color.Red;
            Component[2].FirstBar   = iFirstBar;
            Component[2].Value      = adDown;

            Component[3] = new IndicatorComp();
            Component[3].ChartType = IndChartType.NoChart;
            Component[3].FirstBar  = iFirstBar;
            Component[3].Value     = new double[Bars];

            Component[4] = new IndicatorComp();
            Component[4].ChartType = IndChartType.NoChart;
            Component[4].FirstBar  = iFirstBar;
            Component[4].Value     = new double[Bars];

            // Sets the Component's type
            if (slotType == SlotTypes.OpenFilter)
            {
                Component[3].DataType = IndComponentType.AllowOpenLong;
                Component[3].CompName = "Is long entry allowed";
                Component[4].DataType = IndComponentType.AllowOpenShort;
                Component[4].CompName = "Is short entry allowed";
            }
            else if (slotType == SlotTypes.CloseFilter)
            {
                Component[3].DataType = IndComponentType.ForceCloseLong;
                Component[3].CompName = "Close out long position";
                Component[4].DataType = IndComponentType.ForceCloseShort;
                Component[4].CompName = "Close out short position";
            }

            // Calculation of the logic
            IndicatorLogic indLogic = IndicatorLogic.It_does_not_act_as_a_filter;

            switch (IndParam.ListParam[0].Text)
            {
                case "The Aroon Histogram rises":
                    indLogic = IndicatorLogic.The_indicator_rises;
                    SpecialValues = new double[1] { 0 };
                    break;

                case "The Aroon Histogram falls":
                    indLogic = IndicatorLogic.The_indicator_falls;
                    SpecialValues = new double[1] { 0 };
                    break;

                case "The Aroon Histogram is higher than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_higher_than_the_level_line;
                    SpecialValues = new double[2] { dLevel, -dLevel };
                    break;

                case "The Aroon Histogram is lower than the Level line":
                    indLogic = IndicatorLogic.The_indicator_is_lower_than_the_level_line;
                    SpecialValues = new double[2] { dLevel, -dLevel };
                    break;

                case "The Aroon Histogram crosses the Level line upward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_upward;
                    SpecialValues = new double[2] { dLevel, -dLevel };
                    break;

                case "The Aroon Histogram crosses the Level line downward":
                    indLogic = IndicatorLogic.The_indicator_crosses_the_level_line_downward;
                    SpecialValues = new double[2] { dLevel, -dLevel };
                    break;

                case "The Aroon Histogram changes its direction upward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_upward;
                    SpecialValues = new double[1] { 0 };
                    break;

                case "The Aroon Histogram changes its direction downward":
                    indLogic = IndicatorLogic.The_indicator_changes_its_direction_downward;
                    SpecialValues = new double[1] { 0 };
                    break;

                default:
                    break;
            }

            OscillatorLogic(iFirstBar, iPrvs, adAroon, dLevel, -dLevel, ref Component[3], ref Component[4], indLogic);

            return;
        }

        /// <summary>
        /// Sets the indicator logic description
        /// </summary>
        public override void SetDescription(SlotTypes slotType)
        {
            string sLevelLong  = (IndParam.NumParam[1].Value == 0 ? "0" : IndParam.NumParam[1].ValueToString);
            string sLevelShort = (IndParam.NumParam[1].Value == 0 ? "0" : "-" + IndParam.NumParam[1].ValueToString);

            EntryFilterLongDescription  = "the " + ToString() + " ";
            EntryFilterShortDescription = "the " + ToString() + " ";
            ExitFilterLongDescription   = "the " + ToString() + " ";
            ExitFilterShortDescription  = "the " + ToString() + " ";

            switch (IndParam.ListParam[0].Text)
            {
                case "The Aroon Histogram rises":
                    EntryFilterLongDescription  += "rises";
                    EntryFilterShortDescription += "falls";
                    ExitFilterLongDescription   += "rises";
                    ExitFilterShortDescription  += "falls";
                    break;

                case "The Aroon Histogram falls":
                    EntryFilterLongDescription  += "falls";
                    EntryFilterShortDescription += "rises";
                    ExitFilterLongDescription   += "falls";
                    ExitFilterShortDescription  += "rises";
                    break;

                case "The Aroon Histogram is higher than the Level line":
                    EntryFilterLongDescription  += "is higher than the Level " + sLevelLong;
                    EntryFilterShortDescription += "is lower than the Level "  + sLevelShort;
                    ExitFilterLongDescription   += "is higher than the Level " + sLevelLong;
                    ExitFilterShortDescription  += "is lower than the Level "  + sLevelShort;
                    break;

                case "The Aroon Histogram is lower than the Level line":
                    EntryFilterLongDescription  += "is lower than the Level "  + sLevelLong;
                    EntryFilterShortDescription += "is higher than the Level " + sLevelShort;
                    ExitFilterLongDescription   += "is lower than the Level "  + sLevelLong;
                    ExitFilterShortDescription  += "is higher than the Level " + sLevelShort;
                    break;

                case "The Aroon Histogram crosses the Level line upward":
                    EntryFilterLongDescription  += "crosses the Level " + sLevelLong  + " upward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " downward";
                    ExitFilterLongDescription   += "crosses the Level " + sLevelLong  + " upward";
                    ExitFilterShortDescription  += "crosses the Level " + sLevelShort + " downward";
                    break;

                case "The Aroon Histogram crosses the Level line downward":
                    EntryFilterLongDescription  += "crosses the Level " + sLevelLong  + " downward";
                    EntryFilterShortDescription += "crosses the Level " + sLevelShort + " upward";
                    ExitFilterLongDescription   += "crosses the Level " + sLevelLong  + " downward";
                    ExitFilterShortDescription  += "crosses the Level " + sLevelShort + " upward";
                    break;

                case "The Aroon Histogram changes its direction upward":
                    EntryFilterLongDescription  += "changes its direction upward";
                    EntryFilterShortDescription += "changes its direction downward";
                    ExitFilterLongDescription   += "changes its direction upward";
                    ExitFilterShortDescription  += "changes its direction downward";
                    break;

                case "The Aroon Histogram changes its direction downward":
                    EntryFilterLongDescription  += "changes its direction downward";
                    EntryFilterShortDescription += "changes its direction upward";
                    ExitFilterLongDescription   += "changes its direction downward";
                    ExitFilterShortDescription  += "changes its direction upward";
                    break;

                default:
                    break;
            }

            return;
        }

        /// <summary>
        /// Indicator to string
        /// </summary>
        public override string ToString()
        {
            string sString = IndicatorName +
                (IndParam.CheckParam[0].Checked ? "* (" : " (") +
                IndParam.ListParam[1].Text         + ", " + // Base price
                IndParam.NumParam[0].ValueToString + ")";   // Smoothing period

            return sString;
        }
    }
}
