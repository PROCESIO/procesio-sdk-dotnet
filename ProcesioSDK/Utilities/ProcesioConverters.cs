namespace ProcesioSDK.Utilities
{
    internal static class ProcesioConverters
    {
        public static string ConvertProcessStatus(int state) =>
             state switch
             {
                 1 => "None",
                 5 => "Inactive",
                 15 => "Initializing",
                 20 => "Actions Dispatched",
                 30 => "Running",
                 40 => "Running with Errors",
                 50 => "Finish",
                 60 => "Temporary Waiting",
                 _ => throw new System.NotImplementedException(),
             };
    }
}
