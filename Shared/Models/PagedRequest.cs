using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Models
{
    public record PagedRequest
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 20;
        private int _page = 1;

        public int Page
        {
            get => _page;
            init => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            init => _pageSize = value < 1 ? 20 : Math.Min(value, MaxPageSize);
        }
    }
}
