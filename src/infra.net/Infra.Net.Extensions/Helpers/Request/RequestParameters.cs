using System;
using System.Collections.Generic;
using System.Linq;

namespace Infra.Net.Extensions.Helpers.Request
{
    public interface IRequestParameters<T>
    {
        int? Page { get; set; }

        int? PerPage { get; set; }

        string SortField { get; set; }

        bool? SortDirection { get; set; }

        int? ItemCount { get; set; }

        int? PageCount { get; set; }

        List<Filter<T>> Filters { get; set; }

        IQueryable<T> GetQuery(IQueryable<T> query, bool includeTotals = false);
    }
    public class RequestParameters<T> : IRequestParameters<T>
    {
        public int? Page { get; set; }

        public int? PerPage { get; set; }

        public string SortField { get; set; }

        public bool? SortDirection { get; set; }

        public List<Filter<T>> Filters { get; set; }

        public int? ItemCount { get; set; }

        public int? PageCount { get; set; }

        public bool IncludeTotals { get; set; }

        public virtual IQueryable<T> GetQuery(IQueryable<T> query, bool includeTotals = false)
        {
            if (query == null)
                return null;

            query = GetFiltersQuery(query);
            query = GetSortQuery(query);
            query = GetPaginationQuery(query, includeTotals || IncludeTotals);

            return query;
        }

        protected virtual IQueryable<T> GetFiltersQuery(IQueryable<T> query)
        {
            if (Filters == null || Filters.All(x => x == null))
                return query;

            return Filters.Aggregate(query, (x, item) => item.GetQuery(x));
        }

        protected virtual IQueryable<T> GetSortQuery(IQueryable<T> query)
        {
            if (SortField == null)
                return query;

            var prop = typeof(T).GetProperty(SortField);
            if (SortDirection ?? true)
                query = query.OrderBy(x => prop.GetValue(x));
            else
                query = query.OrderByDescending(x => prop.GetValue(x));

            return query;
        }

        protected virtual IQueryable<T> GetPaginationQuery(IQueryable<T> query, bool includeTotals = false)
        {
            Page = Page == null || Page < 0 ? 0 : Page;
            PerPage = PerPage == null || PerPage <= 0 ? 10 : PerPage;

            ItemCount = null;
            PageCount = null;
            if (includeTotals)
            {
                ItemCount = query.Count();
                PageCount = (int)Math.Ceiling(((decimal)ItemCount) / PerPage.Value);
            }

            return query.Skip(Page.Value * PerPage.Value).Take(PerPage.Value);
        }

    }
}