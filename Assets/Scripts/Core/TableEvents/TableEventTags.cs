namespace PlinkoPinball.Core.TableEvents
{
    /// <summary>
    /// TableEvent tags used by global systems.
    /// Keep these strings stable because systems use them as routing keys.
    /// </summary>
    public static class TableEventTags
    {
        public const string Score = "score";
        public const string Compression = "compression";

        public const string PlinkoReward = "plinkoReward";
        public const string PinBonus = "pinBonus";
        public const string SlotBonus = "slotBonus";

        public const string Chipset = "chipset";
        public const string Memory = "memory";
        public const string Processing = "processing";
        public const string Overclock = "overclock";
        public const string Orbit = "orbit";
        public const string ErrorCorrection = "errorCorrection";

        public const string Lane = "lane";
        public const string Route = "route";
        public const string Sling = "sling";

        public const string Drain = "drain";
        public const string Warning = "warning";
        public const string Critical = "critical";
        public const string Zone = "zone";
    }
}
