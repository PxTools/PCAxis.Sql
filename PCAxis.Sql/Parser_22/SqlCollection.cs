using System.Collections.Specialized;

namespace PCAxis.Sql.Parser_22
{
    // brukes ikke lenger da generisk dictionary benyttes istedet
    class SqlCollection<T> : NameObjectCollectionBase where T : ISqlItem
    {
        #region add
        public void Add(T collItem)
        {
            base.BaseAdd(collItem.Name, collItem);
        }
        #endregion
        #region remove
        public void Remove(T collItem)
        {
            base.BaseRemove(collItem.Name/*.ToUpper()*/);
        }
        #endregion
        #region locate
        public T Item(string Name)
        {
            return (T)base.BaseGet(Name/*.ToUpper()*/);

        }
        #endregion


    }
}
