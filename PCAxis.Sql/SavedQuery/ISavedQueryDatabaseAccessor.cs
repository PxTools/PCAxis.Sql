namespace PCAxis.Sql.SavedQuery
{
    public interface ISavedQueryDatabaseAccessor
    {
        string Load(int id);

        int Save(string savedQuery, int? id);

        bool MarkAsRunned(int name);
    }
}
