using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;

namespace Common.Utilities
{
    public class PagedList<T> : List<T>
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalItemsCount { get; private set; }

        public bool HasPreviousPage
        {
            get
            {
                if (TotalPages != 0 && CurrentPage > 1)
                {
                    return true;
                }

                return false;
            }
        }
        public bool HasNextPage
        {
            get
            {
                if (TotalPages != 0 && CurrentPage < TotalPages)
                {
                    return true;
                }
                    
                return false;
            }
        }

        private PagedList(List<T> items, int itemsCount, int pageNumber, int pageSize)
        {
            TotalPages = (int)Math.Ceiling(itemsCount / (double)pageSize);
            CurrentPage = pageNumber;
            PageSize = pageSize;
            TotalItemsCount = itemsCount;

            AddRange(items);
        }

        /// <summary>
        /// Used to form a pagedList from provided query and page parameters.
        /// Does not work properly with MongoCollections' IQueryable implementation, use overrided method with IFindFluent instead.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<PagedList<T>> ToPagedListAsync(IQueryable<T> query, int pageNumber, int pageSize)
        {
            var itemsCount = query.Count();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedList<T>(items, itemsCount, pageNumber, pageSize);
        }

        /// <summary>
        /// Used to form a pagedList from provided query and page parameters.
        /// Work with MongoCollections properly.
        /// </summary>
        /// <param name="query"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public static async Task<PagedList<T>> ToPagedListAsync(IFindFluent<T, T> query, int pageNumber, int pageSize)
        {
            var itemsCount = query.CountDocuments();
            var items = await query
                .Skip((pageNumber - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return new PagedList<T>(items, (int)itemsCount, pageNumber, pageSize);
        }
    }
}
