﻿namespace BlazorStoreFinder
{
    public class StoreSearchResult
    {
        public string? LocationName { get; set; }
        public string? LocationAddress { get; set; }
        public double LocationLatitude { get; set; }
        public double LocationLongitude { get; set; }
        public double Distance { get; set; }
    }
}