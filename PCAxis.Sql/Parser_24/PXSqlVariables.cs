using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using PCAxis.Sql.QueryLib_24;
using PCAxis.Paxiom;
using PCAxis.Sql.Pxs;
using System.Configuration;
using log4net;

namespace PCAxis.Sql.Parser_24
{
    public class PXSqlVariables : Dictionary<string, PXSqlVariable>
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(PXSqlVariables));

        PXSqlMeta_24 meta;
        VariableSortForSelection sortForSelection;
        public PXSqlVariables(PXSqlMeta_24 meta)
        {
            this.meta = meta;
            sortForSelection = new VariableSortForSelection();
        }


        internal enum DefaultPivot
        {
            alg1,
            alg2,
            alg3,
            alg4,
            alg5,
            alg6
        }

        /// <summary>
        /// True if one or more variable has applied grouping and the grouping requires a sum in the dataextraction, i.e. not all data are stored .
        /// </summary>
        internal bool HasAnyoneGroupingOnNonstoredData()
        {

            foreach (PXSqlVariable var in this.Values)
            {
                if (var.UsesGroupingOnNonstoredData())
                {
                    return true;
                }
            }
            return false;

        }


        internal void setStubHeadPxs()
        {
            int highestUsedStubIndex = 0;
            if (meta.PxsFile.Presentation.Stub != null)
                foreach (AxisType stub in meta.PxsFile.Presentation.Stub)
                {
                    //mSqlVariable = this[stub.code];
                    this[stub.code].Index = stub.index;
                    if (stub.index > highestUsedStubIndex)
                    {
                        highestUsedStubIndex = stub.index;
                    }
                    this[stub.code].IsStub = true;
                }

            if (meta.PxsFile.Presentation.Heading != null)
            {
                foreach (AxisType heading in meta.PxsFile.Presentation.Heading)
                {
                    //mSqlVariable = this[heading.code];
                    this[heading.code].Index = heading.index;
                    this[heading.code].IsHeading = true;
                }
            }

            foreach (PXSqlVariable tmpVar in this.Values)
            {
                if (tmpVar.isSelected && (!tmpVar.IsHeading) && (!tmpVar.IsStub))
                {
                    log.Warn("Variable " + tmpVar.Name + " isSelected, but neither Heading nor Stub. Setting it to stub");
                    highestUsedStubIndex++;
                    tmpVar.IsStub = true;
                    tmpVar.Index = highestUsedStubIndex;

                }
            }
            string shouldPivot = ConfigurationManager.AppSettings["autopivot"];
            if (shouldPivot != null)
            {
                if (shouldPivot.ToLower() == "yes")
                {
                    setStubHeadOverridden();
                }
            }
        }
        internal void setStubHeadOverridden()
        {
            int numberOfVariables = this.Count;
            DefaultPivot pivotAlg;
            int selectedClassCount = 0;
            int selectedTimeValuesCount = 0;
            int selectedContentsValuesCount = 0;
            int selectedTimeContentsValuesCount = 0;
            int lowestSelectedClassValues = Int32.MaxValue;
            string VariableWithLowestSelected = "";

            //int stubIndex = 1;// eller 0 ??????
            foreach (PXSqlVariable tmpVar in this.Values)
            {
                if (tmpVar.isSelected)
                {
                    if (tmpVar.IsTimevariable)
                    {
                        selectedTimeValuesCount = tmpVar.Values.Count;
                    }
                    else if (tmpVar.IsContentVariable)
                    {
                        selectedContentsValuesCount = tmpVar.Values.Count;
                    }
                    else
                    {
                        selectedClassCount += 1;
                        if (tmpVar.Values.Count < lowestSelectedClassValues)
                        {
                            lowestSelectedClassValues = tmpVar.Values.Count;
                            VariableWithLowestSelected = tmpVar.Name;
                        }
                    }
                }

            }
            selectedTimeContentsValuesCount = selectedTimeValuesCount * selectedContentsValuesCount;
            pivotAlg = DefaultPivot.alg1;
            if (selectedTimeContentsValuesCount <= 12 && selectedClassCount > 0)
            {
                pivotAlg = DefaultPivot.alg1;
            }
            if (selectedTimeContentsValuesCount > 12 && selectedTimeValuesCount <= 24)
            {
                pivotAlg = DefaultPivot.alg2;
            }
            if (selectedTimeValuesCount > 24)
            {
                pivotAlg = DefaultPivot.alg3;
            }
            if (selectedTimeContentsValuesCount <= 12 && selectedClassCount == 0)
            {
                pivotAlg = DefaultPivot.alg4;
            }
            if ((selectedTimeContentsValuesCount == 1 && selectedClassCount > 1) && (lowestSelectedClassValues <= 24))
            {
                pivotAlg = DefaultPivot.alg5;
            }
            switch (pivotAlg)
            {
                case DefaultPivot.alg1:
                    break;
                case DefaultPivot.alg2:
                    foreach (PXSqlVariable tmpVar in this.Values)
                    {
                        if (tmpVar.isSelected)
                        {
                            if (tmpVar.IsTimevariable)
                            {
                                tmpVar.IsHeading = true;
                                tmpVar.IsStub = false;
                                tmpVar.Index = -2;
                            }
                            else if (tmpVar.IsContentVariable)
                            {
                                tmpVar.IsStub = true;
                                tmpVar.IsHeading = false;
                                tmpVar.Index = -2;
                            }
                            else
                            {
                                tmpVar.IsStub = true;
                                tmpVar.IsHeading = false;
                            }
                        }
                    }
                    break;
                case DefaultPivot.alg3:
                    foreach (PXSqlVariable tmpVar in this.Values)
                    {
                        if (tmpVar.isSelected)
                        {
                            if (tmpVar.IsTimevariable)
                            {
                                tmpVar.IsStub = true;
                                tmpVar.IsHeading = false;
                                tmpVar.Index = 100;
                            }
                            else if (tmpVar.IsContentVariable)
                            {
                                tmpVar.IsHeading = true;
                                tmpVar.IsStub = false;
                                tmpVar.Index = -100;
                            }
                            else
                            {
                                tmpVar.IsStub = true;
                                tmpVar.IsHeading = false;
                            }
                        }
                    }
                    break;
                case DefaultPivot.alg4:
                    foreach (PXSqlVariable tmpVar in this.Values)
                    {
                        if (tmpVar.isSelected)
                        {
                            if (tmpVar.IsTimevariable)
                            {
                                tmpVar.IsStub = false;
                                tmpVar.IsHeading = true;
                                tmpVar.Index = 1;
                            }
                            else if (tmpVar.IsContentVariable)
                            {
                                tmpVar.IsHeading = false;
                                tmpVar.IsStub = true;
                                tmpVar.Index = 1;
                            }
                            else
                            {
                                log.Warn("Variable " + tmpVar.Name + " isSelected, but neither time  nor contents, something wring with algorithm . Setting it to stub");
                                tmpVar.IsStub = true;
                                tmpVar.IsHeading = false;
                            }
                        }
                    }
                    break;
                case DefaultPivot.alg5:
                    foreach (PXSqlVariable tmpVar in this.Values)
                    {
                        if (tmpVar.isSelected)
                        {
                            if (tmpVar.IsContentVariable)
                            {
                                tmpVar.IsHeading = true;
                                tmpVar.IsStub = false;
                                tmpVar.Index = -300;
                            }
                            else if (tmpVar.IsTimevariable)
                            {
                                tmpVar.IsStub = false;
                                tmpVar.IsHeading = true;
                                tmpVar.Index = -200;
                            }

                            else if (tmpVar.Name == VariableWithLowestSelected)
                            {
                                tmpVar.IsHeading = true;
                                tmpVar.IsStub = false;
                                tmpVar.Index = -100;
                            }
                            else
                            {
                                tmpVar.IsHeading = false;
                                tmpVar.IsStub = true;
                            }
                        }
                    }
                    break;
                default:
                    break;
            }


        }

        internal void setStubHeadDefault()
        {
            //int stubIndex = 1;// eller 0 ??????
            foreach (PXSqlVariable tmpVar in this.Values)
            {
                if (tmpVar.isSelected)
                {
                    if (tmpVar.IsTimevariable)
                    {
                        // tmpVar.Index = 1;
                        tmpVar.IsHeading = true;
                    }
                    else if (tmpVar.IsContentVariable)
                    {
                        // tmpVar.Index = 2;
                        tmpVar.IsHeading = true;
                    }
                    else
                    {
                        // tmpVar.Index = stubIndex;
                        // stubIndex++;
                        tmpVar.IsStub = true;
                    }
                }
            }

        }


        internal StringCollection GetSelectedClassificationVarableIds()
        {
            StringCollection mOut = new StringCollection();
            foreach (PXSqlVariable var in this.Values)
            {
                if (var.isSelected && var.IsClassificationVariable)
                {
                    mOut.Add(var.Name);
                }
            }
            return mOut;
        }


        internal List<PXSqlVariable> GetHeadingSorted()
        {
            List<PXSqlVariable> mHeadings = new List<PXSqlVariable>();
            foreach (PXSqlVariable var in this.Values)
            {
                if (var.isSelected)
                {
                    if (var.IsHeading)
                    {
                        mHeadings.Add(var);
                    }
                }
            }
            mHeadings.Sort();
            return mHeadings;
        }
        internal List<PXSqlVariable> GetStubSorted()
        {
            List<PXSqlVariable> mStubs = new List<PXSqlVariable>();
            foreach (PXSqlVariable var in this.Values)
            {
                if (var.isSelected)
                {
                    if (var.IsStub)
                    {
                        mStubs.Add(var);
                    }
                }
            }
            mStubs.Sort();
            return mStubs;
        }
        internal void ParseMeta(PCAxis.Paxiom.IPXModelParser.MetaHandler handler, StringCollection LanguageCodes)
        {
            string noLanguage = null;
            string keyword;
            string subkey = null;
            StringCollection values = new StringCollection();
            //STUB
            keyword = PXKeywords.STUB;
            foreach (PXSqlVariable var in this.GetStubSorted())
            {
                values.Add(var.Name);
            }
            handler(keyword, noLanguage, subkey, values);
            values.Clear();

            //HEADING
            keyword = PXKeywords.STUB;
            values = new StringCollection();
            foreach (PXSqlVariable var in this.GetHeadingSorted())
            {
                values.Add(var.Name);
            }
            handler(keyword, noLanguage, subkey, values);
            values.Clear();
        }
    }
}



