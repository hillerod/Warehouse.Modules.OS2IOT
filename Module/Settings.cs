using Bygdrift.Warehouse.Helpers.Attributes;

namespace Module
{
    /// <summary>
    /// The AppBase is initialized in 'Module.AppFunctions.TimerTrigger' with this class. Then the warehouse knows it should load all AppSettings to this class and you can reach them through appBase.Settings.
    /// </summary>
    public class Settings
    {
        ///// <summary>
        ///// By adding the attribute ConfigSetting, the warehouse will look in appSettings for this setting.
        ///// If no success, it returns the default value '5'.
        ///// 'NotSet.ShowLogError' gives an error in the log. If it was 'NotSet.ThrowError', the module would have stopped the excecution.
        ///// </summary>
        //[ConfigSetting(Default = "This is a default text because appSettings for 'DataNotSet' wasn't set", NotSet = NotSet.ShowLogWarning, ErrorMessage = "It is not necesary to set this setting. This warning is just an example")]
        //public string DataNotSet { get; set; }

        //[ConfigSetting]
        //public string DataFromSetting { get; set; }

        ///// <summary>
        ///// By adding the attribute ConfigSecret, the warehouse will look in Key vault for this setting.
        ///// </summary>
        //[ConfigSecret]
        //public string DataFromSecret { get; set; }
    }
}
