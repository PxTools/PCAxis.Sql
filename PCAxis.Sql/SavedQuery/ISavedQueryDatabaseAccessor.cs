namespace PCAxis.Sql.SavedQuery
{
    public interface ISavedQueryDatabaseAccessor
    {
        string LoadDefaultSelection(string tableId);

        string Load(int id);

        int Save(string savedQuery, string mainTable, int? id);

        bool MarkAsRunned(int queryId);
    }
}
