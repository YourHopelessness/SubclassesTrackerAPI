namespace SubclassesTracker.Api.Extensions
{
    public static class RacesCounterExtensions
    {
        /// <summary>
        /// Count races
        /// </summary>
        public static void ChangeRacesQuantity(this Dictionary<string, int> racesQuantityDict,
            IEnumerable<string> playersRaces)
        {
            foreach (var races in playersRaces)
            {
                if (racesQuantityDict.TryGetValue(races, out int value))
                {
                    racesQuantityDict[races] = ++value;
                }
            }
        }
    }
}
