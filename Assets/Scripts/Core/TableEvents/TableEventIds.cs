namespace PlinkoPinball.Core.TableEvents
{
    public static class TableEventIds
    {
        public const string HitCapacitor = "hit.capacitor";
        public const string HitCoil = "hit.coil";
        public const string HitFanSpinner = "hit.fan_spinner";
        public const string HitSling = "hit.sling";

        public const string HitMosfetMemory = "hit.mosfet.memory";
        public const string HitMosfetProcessing = "hit.mosfet.processing";
        public const string HitErrorTarget = "hit.error_target";

        public const string PassMemoryEntry = "pass.signal_bridge.memory.entry";
        public const string PassMemoryExit = "pass.signal_bridge.memory.exit";
        public const string PassProcessingEntry = "pass.signal_bridge.processing.entry";
        public const string PassProcessingExit = "pass.signal_bridge.processing.exit";

        public const string RouteMemoryCompleted = "route.signal_bridge.memory.completed";
        public const string RouteProcessingCompleted = "route.signal_bridge.processing.completed";

        public const string PassOverclockDataBus = "pass.data_bus.overclock";

        public const string ZoneDrainEnter = "zone.drain.enter";
        public const string ZoneDrainExit = "zone.drain.exit";

        public const string ZoneCriticalEnter = "zone.power.critical.enter";
        public const string ZoneCriticalExit = "zone.power.critical.exit";
        public const string ZoneCriticalTick = "zone.power.critical.tick";
    }
}
