using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.Linq;

using log4net;

using PCAxis.Paxiom;
using PCAxis.PlugIn.Sql;
using PCAxis.Sql.Pxs;
using PCAxis.Sql.QueryLib_24;


namespace PCAxis.Sql.Parser_24
{
    public class PXSqlVariableClassification : PXSqlVariable
    {

        private static readonly ILog log = LogManager.GetLogger(typeof(PXSqlVariableClassification));

        #region properties

        private PXSqlGroupingInfos groupingInfos;

        //added 8.2.2010 No keyword to handle parsing og groupingInfos. Metode applied in PXSqlBuilder for directly settting properties of variable in Paxiom
        public PXSqlGroupingInfos GroupingInfos
        {
            get { return groupingInfos; }
        }

        /// <summary>Is null more often than not</summary>
        private PXSqlGrouping currentGrouping;
        // for PxsQuery construction and footnotes
        /// <summary>
        /// non null if a grouping has been applied
        /// </summary>
        internal string CurrentGroupingId
        {
            get
            {
                if (currentGrouping == null)
                {
                    return null;
                }
                else
                {

                    return currentGrouping.GroupingId;
                }
            }
        }

        /// <summary>
        /// Contains the ids of the real valueset ( never the "magic All")
        /// </summary>
        private StringCollection valusetIds;
        /// <summary>
        /// Contains the ids of the real valueset ( never the "magic All")
        /// </summary>
        internal StringCollection ValusetIds
        {
            get { return valusetIds; }
        }


        /// <summary> The variable in pxs with the same code(=id) if  such exists null otherwise (!meta.constructedFromPxs or variable eliminated. </summary>
        private readonly PQVariable pxsQueryVariable;


        private string aggregationType = "N";
        private string aggregatingStructureId;

        #endregion

        #region constructors
        //     public PXSqlVariableClassification() { }


        public PXSqlVariableClassification(MainTableVariableRow aTVRow, PXSqlMeta_24 meta)
            : base(aTVRow.Variable, meta, false, false, true)
        {


            if (this.meta.ConstructedFromPxs)
            {
                foreach (PQVariable tmpVar in this.meta.PxsFile.Query.Variables)
                {
                    if (this.Name == tmpVar.code)
                        this.pxsQueryVariable = tmpVar;
                }
            }
            SetSelected();

            if (this.isSelected)
            {
                this.mStoreColumnNo = int.Parse(aTVRow.StoreColumnNo);
                if (!this.meta.ConstructedFromPxs)
                {
                    this.mIndex = this.mStoreColumnNo;
                }

                SetValueset();
                SetValuePool();
                SetPresText();
                SetDefaultPresTextOption(); // would be overwritten if options set in pxs
                if (this.meta.ConstructedFromPxs)
                    SetOptionsFromPxs();

                if (this.meta.inSelectionModus)
                {

                    this.groupingInfos = new PXSqlGroupingInfos(this.meta, this.Name, valusetIds);
                }
            }
            else
            {
                SetValueset();
            }
            //  Elimination must be done after SetValues moved down.
            //   if (this.meta.InstanceModus == Instancemodus.selection)
            //       SetElimForSelection();
            //   else
            //       SetElimForPresentation();



            if (this.aggregationType.Equals("G"))
            {

                if (String.IsNullOrEmpty(this.aggregatingStructureId))
                {
                    throw new ApplicationException("Not implemented yet");
                }
                else if (this.aggregatingStructureId.Equals("UNKNOWNSTRUCTUREID"))
                {
                    List<PXSqlGroup> groupFromFile = GetListOfGroupFromPxs();

                    currentGrouping = new PXSqlGrouping(this.meta, this, groupFromFile);

                }
                else
                {
                    currentGrouping = new PXSqlGrouping(this.metaQuery.GetGroupingRow(this.aggregatingStructureId), this.meta, this, this.pxsQueryVariable.GetCodesNoWildcards());
                }
            }
            else
            {

                SetValues();
                SetCodelists();
            }

            SetMetaIdFromAllGroupings();

            if (this.meta.inSelectionModus)
                SetElimForSelection();
            else
                SetElimForPresentation();
        }

        /// <summary>
        /// Adds all metaids from grouping rows. We cannot wait for applygrouping since we need wo display all meta info for a variable
        /// </summary>
        private void SetMetaIdFromAllGroupings()
        {
            if (GroupingInfos == null || GroupingInfos.GroupingIDs == null) return;

            foreach (var groupingId in GroupingInfos.GroupingIDs)
            {
                var groupingRow = this.metaQuery.GetGroupingRow(groupingId);

                if (!string.IsNullOrEmpty(groupingRow.MetaId))
                {
                    addMetaId(groupingRow.MetaId);
                }
            }
        }

        #endregion constructors



        private void SetCodelists()
        {
            if (this.isSelected)
            {

                if (meta.inSelectionModus)
                {

                    foreach (KeyValuePair<string, PXSqlValueSet> tmpValueSet in this.ValueSets)
                    {
                        if (tmpValueSet.Key == PXSqlKeywords.FICTIONAL_ID_ALLVALUESETS)
                        {
                            foreach (PXSqlValue value in mValues.GetValuesSortedByValue(mValues.GetValuesForSelectedValueset(tmpValueSet.Key)))
                            {
                                tmpValueSet.Value.SortedListOfCodes.Add(value.ValueCode);
                            }
                        }
                        else
                        {
                            if (tmpValueSet.Value.SortCodeExists == metaQuery.DB.Codes.Yes)
                                foreach (PXSqlValue value in mValues.GetValuesSortedByVSValue(mValues.GetValuesForSelectedValueset(tmpValueSet.Key)))
                                {
                                    tmpValueSet.Value.SortedListOfCodes.Add(value.ValueCode);
                                }
                            else
                                foreach (PXSqlValue value in mValues.GetValuesSortedByValue(mValues.GetValuesForSelectedValueset(tmpValueSet.Key)))
                                {
                                    tmpValueSet.Value.SortedListOfCodes.Add(value.ValueCode);
                                }
                        }
                    }

                }

            }
        }


        private void SetOptionsFromPxs()
        {
            if (this.pxsQueryVariable != null)
            {
                if (this.pxsQueryVariable.PresTextOption != null) // added because PresTextOption not set anymore in PxsQuery_Paxiom_partial. see comment there
                {
                    this.mPresTextOption = ConvertToDbPresTextOption(this.pxsQueryVariable.PresTextOption);
                }
                this.aggregationType = this.pxsQueryVariable.Aggregation.ToString();
                this.aggregatingStructureId = this.pxsQueryVariable.StructureId;
                log.Debug("aggregationType:" + aggregationType);

            }
        }



        private void SetValues()
        {
            StringCollection selSubTables = new StringCollection();
            foreach (PXSqlSubTable subTable in meta.SubTables.Values)
            {
                selSubTables.Add(subTable.SubTable);
            }

            if (meta.ConstructedFromPxs)
            {
                if (pxsQueryVariable != null)
                {
                    this.SetValues(selSubTables, pxsQueryVariable);
                }
            }
            else
            {
                this.SetValues(selSubTables);
            }
        }

        // when no pxs
        internal void SetValues(StringCollection selSubTables)
        {

            ValueRow2HMDictionary mValueRowDictionary = meta.MetaQuery.GetValueRowDictionary(meta.MainTable.MainTable, selSubTables, this.Name, this.ValuePool.ValueTextExists);
            Dictionary<string, ValueRow2HM> mValueRows = mValueRowDictionary.ValueRows;

            this.Values = new PxSqlValues();

            foreach (ValueRow2HM myValueRow in mValueRows.Values)
            {
                PXSqlValue mValue = new PXSqlValue(myValueRow, meta.LanguageCodes, meta.MainLanguageCode);
                this.Values.Add(mValue.ValueCode, mValue);
            }
        }


        //when pxs
        internal void SetValues(StringCollection selSubTables, PQVariable var)
        {
            log.Debug("PQVariable code = " + var.code);
            StringCollection mSelectedValues = new StringCollection();
            // Defines a dictionary to hold all the sortorders. Necessary because of the wildcards
            Dictionary<string, int> mDefinedSortorder = new Dictionary<string, int>();

            DataSet mValueInfoTbl;
            DataRowCollection mValueInfo;
            string mPxsSubTableId = meta.PxsFile.Query.SubTable;


            #region foreach var.Values.Items
            if (var.Values.Items.Length > 0)
            {
                int documentOrder = 0;

                foreach (PCAxis.Sql.Pxs.ValueTypeWithGroup val in var.Values.Items)
                {
                    if (val.Group != null)
                    {

                        //currentGrouping = ...
                        //TODO; PXSqlVariableClassification, SetValues: group not implemented yet"
                        throw new NotImplementedException("PXSqlVariableClassification, SetValues: group not implemented yet");

                    }
                    if (val.code.Contains("*") || val.code.Contains("?"))
                    {
                        if (mPxsSubTableId == null)
                        {
                            mValueInfoTbl = meta.MetaQuery.GetValueWildCardBySubTable(meta.MainTable.MainTable, var.code, null, val.code);
                        }
                        else
                        {
                            mValueInfoTbl = meta.MetaQuery.GetValueWildCardBySubTable(meta.MainTable.MainTable, var.code, mPxsSubTableId, val.code);
                        }
                        mValueInfo = mValueInfoTbl.Tables[0].Rows;

                        foreach (DataRow row in mValueInfo)
                        {
                            string mTempCode = row[meta.MetaQuery.DB.Value.ValueCodeCol.PureColumnName()].ToString();
                            mSelectedValues.Add(mTempCode);
                            if (!mDefinedSortorder.ContainsKey(mTempCode))
                            {
                                mDefinedSortorder.Add(mTempCode, documentOrder);
                            }
                            documentOrder++;
                        }
                    }
                    else
                    {
                        mSelectedValues.Add(val.code);
                        if (!mDefinedSortorder.ContainsKey(val.code))
                        {
                            mDefinedSortorder.Add(val.code, documentOrder);
                        }
                    }

                    documentOrder++;
                }
                #endregion foreach var.Values.Items


                // mSelectedValues now contains all the selected values, including those defined by wildcards

                Dictionary<string, PXSqlValue> mTempPXSqlValues = new Dictionary<string, PXSqlValue>();
                List<PXSqlValue> mSortedValues = new List<PXSqlValue>();

                ValueRow2HMDictionary mValueRowDictionary = meta.MetaQuery.GetValueRowDictionary(meta.MainTable.MainTable, selSubTables, var.code, mSelectedValues, this.ValuePool.ValueTextExists);


                //todo; fortsette her
                Dictionary<string, ValueRow2HM> mValueRows = mValueRowDictionary.ValueRows;

                #region foreach mValueRows
                foreach (ValueRow2HM myValueRow in mValueRows.Values)
                {

                    PXSqlValue mValue = new PXSqlValue(myValueRow, meta.LanguageCodes, meta.MainLanguageCode);

                    // jfi: kommentaren sto i en kodeblock som forsvant inn i PXSqlValue
                    // todo; legge til sjekk om koden finnes blandt valgte i basen.
                    mValue.SortCodePxs = mDefinedSortorder[myValueRow.ValueCode];
                    mSortedValues.Add(mValue);
                }
                #endregion foreach mValueRows

                mValues = new PxSqlValues();

                foreach (PXSqlValue sortedValue in mSortedValues)
                {
                    mValues.Add(sortedValue.ValueCode, sortedValue);
                }

                this.Values = mValues;
            }
        }


        private List<PXSqlGroup> GetListOfGroupFromPxs()
        {
            if (this.pxsQueryVariable.Values.Items.Length == 0)
            {
                throw new ApplicationException("GetListOfGroupFromPxs(): No entries found");
            }
            List<PXSqlGroup> myOut = new List<PXSqlGroup>();

            foreach (PCAxis.Sql.Pxs.ValueTypeWithGroup val in this.pxsQueryVariable.Values.Items)
            {
                if (val.code.Contains("*") || val.code.Contains("?"))
                {
                    throw new ApplicationException("GetListOfGroupFromPxs(): Groups cannot contain wildcards");
                }
                PXSqlGroup group = new PXSqlGroup(val.code);

                if (val.Group == null || val.Group.GroupValue == null || val.Group.GroupValue.Length < 1)
                {
                    throw new ApplicationException("GetListOfGroupFromPxs(): Expected group children");
                }
                foreach (GroupValueType child in val.Group.GroupValue)
                {
                    group.AddChildCode(child.code);
                }
                myOut.Add(group);
            }
            return myOut;

        }


        /// <summary>
        /// True if a grouping has been applied and the grouping requires a sum in the dataextraction, i.e. not all data are stored .
        /// </summary>
        internal override bool UsesGroupingOnNonstoredData()
        {
            if (currentGrouping == null)
            {
                return false;
            }
            else
            {
                return currentGrouping.isOnNonstoredData();
            }

        }








        private void SetSelected()
        {
            this.mIsSelected = false;
            if (meta.ConstructedFromPxs)
            {
                if ((this.pxsQueryVariable != null) && (this.pxsQueryVariable.Values.Items.Length > 0))
                {
                    this.mIsSelected = true;
                }
            }
            else
            {
                this.isSelected = true;
            }
        }



        /// <summary>Started from GUI via builder, a grouping is applied.</summary>
        /// <param name="paxiomVariable">the paxiom Variable </param>
        /// <param name="groupingId">The id of the grouping</param>
        /// <param name="include">Emun value inducating what the new codelist should include:parents,childern or both </param>
        /// <param name="skipRecreateValues">should RecreateValues() be called </param>
        internal void ApplyGrouping(Variable paxiomVariable, string groupingId, GroupingIncludesType include, bool skipRecreateValues)
        {
            if (!skipRecreateValues)
            {
                paxiomVariable.RecreateValues();// per inge old comment: values for variable must be deleted before created for new valueset.
            }
            paxiomVariable.Hierarchy.Clear(); // must clear hierarchies.

            this.currentGrouping = new PXSqlGrouping(this.metaQuery.GetGroupingRow(groupingId), meta, this, include);
            this.selectedValueset = PXSqlKeywords.FICTIONAL_ID_ALLVALUESETS; //todo; or should it be valuset for vsgroup?

            //was   SetElimForSelection(); //TODO How should elimination for groups be?
            //            SetDefaultPresTextOption();


            this.PaxiomElimination = PXConstant.NO;
            this.PresTextOption = this.ValuePool.ValuePres;


            //send new state to paxiom:

            paxiomVariable.CurrentGrouping = this.currentGrouping.GetPaxiomGrouping();


        }

        internal void ApplyValueSet(string valueSetId)
        {
            this.currentGrouping = null;
            this.selectedValueset = valueSetId;
            SetElimForSelection();
            SetDefaultPresTextOption();
            //TODO; 

        }

        private void SetValueset()
        {
            valusetIds = new StringCollection();

            List<PXSqlValueSet> sortedValuesets = new List<PXSqlValueSet>();

            string defaultInGuiValueset = metaQuery.GetValuesetIdDefaultInGui(meta.MainTable.MainTable, this.Name);

            if (this.pxsQueryVariable == null || this.pxsQueryVariable.SelectedValueset == PXSqlKeywords.FICTIONAL_ID_ALLVALUESETS)
            {

                var valueSetsByPresText = new Dictionary<string, PXSqlValueSet>();
                bool shouldSubTableVarablesBeSortedAlphabetical = meta.MetaQuery.ShouldSubTableVarablesBeSortedAlphabetical(meta.MainTable.MainTable, this.Name);

                List<string> valuesetIds = meta.MetaQuery.GetValuesetIdsFromSubTableVariableOrderedBySortcode(meta.MainTable.MainTable, this.Name);

                foreach (string valuesetId in valuesetIds)
                {
                    bool isDefault = valuesetId.Equals(defaultInGuiValueset);

                    if (shouldSubTableVarablesBeSortedAlphabetical)
                    {
                        var valueSet = new PXSqlValueSet(meta.MetaQuery.GetValueSetRow(valuesetId), meta, isDefault);
                        valueSetsByPresText[valueSet.PresText[this.meta.MainLanguageCode]] = valueSet;
                    }
                    else
                    {
                        sortedValuesets.Add(new PXSqlValueSet(meta.MetaQuery.GetValueSetRow(valuesetId), meta, isDefault));
                    }

                }

                if (shouldSubTableVarablesBeSortedAlphabetical)
                {
                    var sortedValuesetPresTexts = valueSetsByPresText.Keys.OrderBy(x => x).ToArray();

                    foreach (var presText in sortedValuesetPresTexts)
                    {
                        sortedValuesets.Add(valueSetsByPresText[presText]);
                    }
                }
            }
            else
            {
                // for selected valueset without subtable stored in pxs.
                bool isDefault = this.pxsQueryVariable.SelectedValueset.Equals(defaultInGuiValueset);
                sortedValuesets.Add(new PXSqlValueSet(meta.MetaQuery.GetValueSetRow(this.pxsQueryVariable.SelectedValueset), meta, isDefault));

            }



            int NumberOfSelectedValueSets = sortedValuesets.Count;

            mValueSets = new Dictionary<string, PXSqlValueSet>();
            int totalNumberOfValues = 0;

            List<EliminationAux> elimValues = new List<EliminationAux>(); // For the magicAll valueSet 

            StringCollection tmpValuePres = new StringCollection(); // For the magicAll valueSet 
            List<string> metaIdValues = new List<string>(); // For all the magicAll ValueSet MetaId

            foreach (PXSqlValueSet valueSetItem in sortedValuesets)
            {
                mValueSet = valueSetItem;
                mValueSet.NumberOfValues = meta.MetaQuery.GetNumberOfValuesInValueSetById(mValueSet.ValueSet);

                totalNumberOfValues += mValueSet.NumberOfValues;

                mValueSets.Add(mValueSet.ValueSet, mValueSet);
                valusetIds.Add(mValueSet.ValueSet);
                metaIdValues.Add(mValueSet.MetaId);

                // helpers for the magicAll 
                elimValues.Add(mValueSet.GetEliminationAux());
                if (!tmpValuePres.Contains(mValueSet.ValuePres))
                    tmpValuePres.Add(mValueSet.ValuePres);

            }

            // Add the collection to the variable.
            //this.ValueSets = mValueSets;  a = a !

            this.TotalNumberOfValuesInDB = totalNumberOfValues;

            if (NumberOfSelectedValueSets == 1)
            {
                selectedValueset = mValueSet.ValueSet;
                addMetaId(mValueSet.MetaId);
            }
            else
            {
                string allValuePres;
                if (tmpValuePres.Count == 1)
                    allValuePres = tmpValuePres[0];
                else
                    allValuePres = "V"; //For valuepool TODO her m� det endres slik at codes V legges i config fila 

                PXSqlValueSet magicAll = new PXSqlValueSet(this.PresText, sortedValuesets[0].ValuePoolId, elimValues, metaQuery.DB.Codes.No, allValuePres);
                magicAll.NumberOfValues = totalNumberOfValues;
                this.ValueSets.Add(magicAll.ValueSet, magicAll);
                foreach (var metaId in metaIdValues)
                {
                    addMetaId(metaId);
                }
            }
        }



        private void SetDefaultPresTextOption()
        {
            if (string.IsNullOrEmpty(this.selectedValueset)) return;

            // if prestextoption not set e.g from Pxs then apply PresTextOption from db
            if (this.ValueSets[this.selectedValueset].ValuePres == "V" || this.ValueSets[this.selectedValueset].ValuePres == "")
            {
                this.PresTextOption = this.ValuePool.ValuePres;
            }
            else
            {
                this.PresTextOption = this.ValueSets[this.selectedValueset].ValuePres;
            }
        }

        internal string GetOneValuePoolId()
        {

            Dictionary<string, PXSqlValueSet>.Enumerator vSEnum;
            vSEnum = this.ValueSets.GetEnumerator();
            vSEnum.MoveNext();
            return vSEnum.Current.Value.ValuePoolId;
        }

        private void SetValuePool()
        {
            this.ValuePool = new PXSqlValuepool(metaQuery.GetValuePoolRow(this.GetOneValuePoolId()), meta);
        }
        /// <summary>
        /// 
        /// </summary>
        /// 

        protected void SetElimForSelection()
        {
            this.IsEliminatedByValue = false;  //jfi 5/4 2017: ??? How do we know?  

            if (selectedValueset == null || this.ValueSets[selectedValueset].GetEliminationAux().IsNotAllowed())
            {
                this.PaxiomElimination = PXConstant.NO;
            }
            else
            {
                this.PaxiomElimination = PXConstant.YES;
            }

        }
        protected override void SetElimForPresentation()
        {
            string tmpElim;
            PXSqlValue mValue;
            this.IsEliminatedByValue = false;

            List<decimal> NumberOfValuesInValuesets = new List<decimal>();


            if (pxsQueryVariable != null)
            {
                if (!string.IsNullOrEmpty(this.pxsQueryVariable.StructureId))
                {
                    int valueSetCount = this.ValueSets.Count();

                    if (valueSetCount > 1)
                    {
                        var valuesetsWithEliminationValue = this.ValueSets.Where(x => x.Value.Elimination != meta.Config.Codes.EliminationN && x.Value.Elimination != meta.Config.Codes.EliminationA);
                        PXSqlValueSet onlyVsWithEliminationValue = valuesetsWithEliminationValue.Count() == 1 ? valuesetsWithEliminationValue.First().Value : null;

                        if (this.ValueSets.Keys.Any(x => x == PXSqlKeywords.FICTIONAL_ID_ALLVALUESETS))
                        {
                            selectedValueset = PXSqlKeywords.FICTIONAL_ID_ALLVALUESETS;
                            PXSqlValueSet vs = this.ValueSets[PXSqlKeywords.FICTIONAL_ID_ALLVALUESETS];
                            NumberOfValuesInValuesets.Add(vs.NumberOfValues);
                            tmpElim = vs.Elimination;
                        }
                        else if (onlyVsWithEliminationValue != null)
                        {
                            NumberOfValuesInValuesets.Add(onlyVsWithEliminationValue.NumberOfValues);
                            tmpElim = onlyVsWithEliminationValue.Elimination;
                        }
                        else
                        {
                            tmpElim = meta.Config.Codes.EliminationN;
                        }
                    }
                    else if (valueSetCount == 1 && this.ValueSets.First().Value.Elimination != meta.Config.Codes.EliminationN)
                    {
                        PXSqlValueSet vs = this.ValueSets.First().Value;
                        NumberOfValuesInValuesets.Add(vs.NumberOfValues);
                        tmpElim = vs.Elimination;
                    }
                    else
                    {
                        tmpElim = meta.Config.Codes.EliminationN;
                    }
                }
                else
                {
                    PXSqlValueSet vs = this.ValueSets[selectedValueset];
                    NumberOfValuesInValuesets.Add(vs.NumberOfValues);
                    tmpElim = vs.Elimination;
                }
            }
            else
            {
                PXSqlValueSet vs = this.ValueSets[selectedValueset];
                NumberOfValuesInValuesets.Add(vs.NumberOfValues);
                tmpElim = vs.Elimination;
            }

            if (tmpElim == meta.Config.Codes.EliminationN || tmpElim.Length == 0)
            {

                if (!this.isSelected)
                {

                    throw new PCAxis.Sql.Exceptions.PxsException(11, this.Name);

                }
                else
                {

                    this.PaxiomElimination = PXConstant.NO;
                }
            }
            else if (tmpElim == meta.Config.Codes.EliminationA)
            {
                if (this.isSelected)
                {
                    // We have to compare values in the valuepool(s) with the values selected in the PxsFile

                    if (this.Values.Count == NumberOfValuesInValuesets[0])
                    {

                        this.PaxiomElimination = PXConstant.YES;
                    }
                    else
                    {

                        this.PaxiomElimination = PXConstant.NO;
                    }

                }
            }
            else
            { // An elimination value exist for the variable.
                if (this.isSelected)
                {
                    if (this.Values.TryGetValue(tmpElim, out mValue))
                    { // the elimination value is selected

                        this.PaxiomElimination = mValue.ValueCode;
                    }



                    else
                    { // The Elimination value is not selected.  Elimination in Paxiom should be NO.

                        this.PaxiomElimination = PXConstant.NO;
                    }

                }
                // If an elimiantion value exists and no values are selected for the variable, the elimination
                // value should be used when selecting data, and metadata should be marked as eliminated by value.
                else
                {
                    mValue = new PXSqlValue();
                    mValue.ValueCode = tmpElim;

                    this.Values.Add(mValue.ValueCode, mValue);
                    this.IsEliminatedByValue = true;
                }
            }

        }




        internal override List<PXSqlValue> GetValuesForParsing()
        {
            if (currentGrouping != null)
            {
                return currentGrouping.GetValuesForParsing();
            }

            if (selectedValueset == null)
            {
                return new List<PXSqlValue>();
            }

            if ((meta.inPresentationModus) && meta.ConstructedFromPxs)
            {
                return mValues.GetValuesSortedByPxs(mValues.GetValuesForSelectedValueset(selectedValueset));
                //return GetValuesSortedDefault(GetValuesForSelectedValueset()); // old sorting Thomas say its how Old Pcaxis does
            }
            else
            {
                PXSqlValueSet tmpValueSet = mValueSets[selectedValueset];
                List<PXSqlValue> myOut = new List<PXSqlValue>(tmpValueSet.NumberOfValues);
                foreach (string code in tmpValueSet.SortedListOfCodes)
                    myOut.Add(mValues[code]);
                return myOut;

            }
        }


        private string PresTextOptionToPxiomPresText(string presText)
        {
            if (presText == meta.Config.Codes.ValuePresC)
                return "0";

            if (presText == meta.Config.Codes.ValuePresT)
                return "1";

            if (presText == meta.Config.Codes.ValuePresB)
                return "2";
            else
                return "2";
        }

        internal List<PXSqlGroup> GetGroupsForDataParsing()
        {
            if (currentGrouping == null)
                throw new ApplicationException("BUG!");

            return currentGrouping.GetGroupsForDataParsing();
        }



        internal override void ParseMeta(PCAxis.Paxiom.IPXModelParser.MetaHandler handler, StringCollection LanguageCodes, string preferredLanguage)
        {
            base.ParseMeta(handler, LanguageCodes, preferredLanguage);
            if (this.isSelected)
            {

                // DOMAIN
                ParseDomain(handler, LanguageCodes);

                //MAP
                ParseMap(handler);

                //  VALUESET_X
                ParseValueSetKeywords(handler, LanguageCodes);
            }

            // ELIMINATION
            ParseElimination(handler, preferredLanguage);

            //CANDIDATEMUSTSELECT Extendet property
            ParseCandidateMustSelect(handler, preferredLanguage);

            //GROUPING  (only for selected and selectionMode)
            if (this.groupingInfos != null)
            {
                this.groupingInfos.ParseMeta(handler);
            }

        }

        /// <PXKeyword name="DOMAIN">
        ///   <rule>
        ///     <description>Deviates from the standard languagehandeling which would be to read the ValuePool column of secondary language table. Doamin is read 
        ///     from column ValuePoolEng(2.0) or ValuePoolAlias (later).  Solved in 2.3.</description>
        ///     <table modelName ="ValuePool">
        ///     <column modelName="ValuePool"/>
        ///     </table>
        ///     <table modelName ="ValuePool(secondary language)">
        ///     <column modelName="ValuePoolEng(2.0)"/>
        ///     <column modelName="ValuePoolAlias(later)"/>
        ///     </table>
        ///   </rule>
        /// </PXKeyword>
        private void ParseDomain(PCAxis.Paxiom.IPXModelParser.MetaHandler handler, StringCollection LanguageCodes)
        {
            StringCollection values = new StringCollection();
            string subkey = this.Name;

            foreach (string langCode in LanguageCodes)
            {
                values.Clear();
                values.Add(this.ValuePool.Domain[langCode]);
                handler(PXKeywords.DOMAIN, langCode, subkey, values);
            }

        }




        /// <PXKeyword name="MAP">
        ///   <rule>
        ///     <description> </description>
        ///     <table modelName ="ValueSet">
        ///       <column modelName="GeoAreaNo"/>
        ///     </table>     
        ///     <table modelName ="Grouping">
        ///       <column modelName="GeoAreaNo"/>
        ///     </table>
        ///     <table modelName ="GroupingLevel">
        ///       <column modelName="GeoAreaNo"/>
        ///     </table>
        ///     <table modelName ="TextCatalogt">
        ///       <column modelName="PresText (of main language)"/>
        ///     </table>     
        ///   </rule>
        /// </PXKeyword>
        private void ParseMap(PCAxis.Paxiom.IPXModelParser.MetaHandler handler)
        {
            StringCollection values = new StringCollection();
            string subkey = this.Name;
            string noLanguage = null;

            if (this.PaxiomMap != null)
            {
                values.Clear();
                values.Add(this.PaxiomMap);
                handler(PXKeywords.MAP, noLanguage, subkey, values);
            }
        }




        /// <PXKeyword name="VALUESET_ID">
        ///   <rule>
        ///     <description> </description>
        ///     <table modelName ="ValueSet">
        ///     <column modelName="ValueSet"/>
        ///     </table>
        ///   </rule>
        /// </PXKeyword>
        /// <PXKeyword name="VALUESET_NAME">
        ///   <rule>
        ///     <description> </description>
        ///     <table modelName ="ValueSet">
        ///     <column modelName="PresText"/>
        ///     </table>
        ///   </rule>
        /// </PXKeyword>
        private void ParseValueSetKeywords(PCAxis.Paxiom.IPXModelParser.MetaHandler handler, StringCollection LanguageCodes)
        {
            StringCollection values = new StringCollection();
            string subkey = this.Name;
            string noLanguage = null;
            bool parseValueSet;
            if (meta.inPresentationModus)
            {
                parseValueSet = true;
            }
            else
            {
                if ((this.ValueSets.Values.Count > 1) || (this.groupingInfos.Infos.Count > 0))
                {
                    parseValueSet = true;
                }
                else
                {
                    parseValueSet = false;
                }

            }


            if (parseValueSet)
            {
                foreach (PXSqlValueSet valueSet in this.ValueSets.Values)
                {
                    values.Add(valueSet.ValueSet);
                }
                handler(PXKeywords.VALUESET_ID, noLanguage, subkey, values);

                foreach (string langCode in LanguageCodes)
                {
                    values.Clear();

                    foreach (PXSqlValueSet valueSet in this.ValueSets.Values)
                    {
                        values.Add(valueSet.PresText[langCode]);
                    }
                    handler(PXKeywords.VALUESET_NAME, langCode, subkey, values);
                }

            }
        }

        /// <PXKeyword name="ELIMINATION">
        ///   <rule>
        ///     <description>Is set directly in paxiom.</description>
        ///     <table modelName ="ValueSet">
        ///     <column modelName="Elimination"/>
        ///     </table>
        ///   </rule>
        /// </PXKeyword>
        internal void ParseElimination(PCAxis.Paxiom.IPXModelParser.MetaHandler handler, string preferredLanguage)
        {
            string subkey = this.Name;
            string noLanguage = null;
            StringCollection values = new StringCollection();
            if (this.isSelected) //29.6.2010 This keyword should only be sent if the variable is selected.
            {
                if (this.PaxiomElimination == PXConstant.YES)
                {
                    if (this.ValueSets[selectedValueset].Elimination == meta.Config.Codes.EliminationA)
                    {
                        foreach (PXSqlContent pxsqlCont in meta.Contents.Values)
                        {
                            if (!pxsqlCont.AggregPossible)
                            {
                                this.PaxiomElimination = PXConstant.NO;
                                break;
                            }
                        }
                    }
                }

                values.Clear();
                values.Add(this.PaxiomElimination);
                handler(PXKeywords.ELIMINATION, noLanguage, subkey, values);
            }

        }

        internal void ParseCandidateMustSelect(PCAxis.Paxiom.IPXModelParser.MetaHandler handler, string preferredLanguage)
        {
            string subkey = this.Name;
            string noLanguage = null;
            StringCollection values = new StringCollection();



            values.Clear();
            if (isCandidateMustSelect())
            {
                values.Add(PXConstant.YES);
            }
            else
            {
                values.Add(PXConstant.NO);
            }

            //handler(PXKeywords.POSSIBLENOTELIM, noLanguage, subkey, values);
            handler("CandidateMustSelect", noLanguage, subkey, values);
        }




        internal bool isCandidateMustSelect()
        {
            if (this.PaxiomElimination == PXConstant.NO)
            {
                return true;
            }
            if (this.GroupingInfos != null)
            {
                if (this.GroupingInfos.Infos.Count > 0)
                {
                    return true;
                }

            }

            foreach (PXSqlContent pxsqlCont in meta.Contents.Values)
            {
                if (!pxsqlCont.AggregPossible)
                {
                    foreach (KeyValuePair<string, PXSqlValueSet> vs in this.ValueSets)
                    {
                        if (vs.Value.Elimination == PXConstant.YES)
                        {
                            return true;
                        }
                    }

                }
            }
            return false;
        }



        internal void ParseForApplyValueSet(PCAxis.Paxiom.IPXModelParser.MetaHandler handler, StringCollection LanguageCodes, string preferredLanguage)
        {
            string subkey = this.Name;

            StringCollection values = new StringCollection();
            // ELIMINATION
            ParseElimination(handler, preferredLanguage);
            //PresText
            base.ParsePresTextOption(handler, LanguageCodes, preferredLanguage);
            //Codes and values
            base.ParseCodeAndValues(handler, LanguageCodes, preferredLanguage);

            //Hierarchies
            if (this.currentGrouping != null)
            {
                if (this.currentGrouping.isHierarchy)
                {
                    ParseHierarchies(handler);
                    ParseHierarchyLevelsOpen(handler);
                    ParseHierarchyLevels(handler);
                    ParseHierarchyNames(handler, LanguageCodes);
                }
            }
        }
        /// <summary>
        /// Sends the hierachy for a variable to paxiom
        /// </summary>
        internal void ParseHierarchies(PCAxis.Paxiom.IPXModelParser.MetaHandler handler)
        {
            string subkey = this.Name;
            string noLanguage = null;
            StringCollection values = new StringCollection();
            //HIERARCHIES
            values.Clear();
            foreach (string hierarchyParentChild in currentGrouping.getHierarchyForParsing())
            {
                values.Add(hierarchyParentChild);
            }
            handler(PXKeywords.HIERARCHIES, noLanguage, subkey, values);
            values = null;
        }

        /// <summary>
        /// Sends the number of levels which should be initial opened.
        /// </summary>
        internal void ParseHierarchyLevelsOpen(PCAxis.Paxiom.IPXModelParser.MetaHandler handler)
        {
            string subkey = this.Name;
            string noLanguage = null;
            StringCollection values = new StringCollection();
            //HIERARCHYLEVELSOPEN
            values.Clear();
            values.Add(currentGrouping.HierarchyLevelsOpen);
            handler(PXKeywords.HIERARCHYLEVELSOPEN, noLanguage, subkey, values);
            values = null;
        }

        /// <summary>
        /// Sends the number of levels in a hiearchy.
        /// </summary>
        internal void ParseHierarchyLevels(PCAxis.Paxiom.IPXModelParser.MetaHandler handler)
        {
            string subkey = this.Name;
            string noLanguage = null;
            StringCollection values = new StringCollection();
            //HIERARCHYLEVELS
            values.Clear();
            values.Add(currentGrouping.HierarchyLevels);
            handler(PXKeywords.HIERARCHYLEVELS, noLanguage, subkey, values);
            values = null;
        }

        /// <summary>
        /// Sends the hierachynames to paxiom
        /// </summary>
        internal void ParseHierarchyNames(PCAxis.Paxiom.IPXModelParser.MetaHandler handler, StringCollection LanguageCodes)
        {
            string subkey = this.Name;
            //string noLanguage = null;
            StringCollection values = new StringCollection();
            //HIERARCYNAMES
            foreach (string langCode in LanguageCodes)
            {
                values.Clear();

                foreach (string hierarchyNames in currentGrouping.HierarchyNames[langCode])
                {
                    values.Add(hierarchyNames);
                }
                handler(PXKeywords.HIERARCHYNAMES, langCode, subkey, values);
            }
            values = null;
        }

    }

}
