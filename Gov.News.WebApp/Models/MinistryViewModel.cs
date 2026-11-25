using Gov.News.Api.Models;
using System;
using System.Collections.Generic;

namespace Gov.News.Website.Models
{
    public class MinistryViewModel : ListViewModel
    {
        public Ministry Ministry { get; set; }

        public Minister Minister { get; set; }

        public List<Tuple<Ministry, Minister>> ChildMinistries { get; set; }
            = new List<Tuple<Ministry, Minister>>();
    }
}