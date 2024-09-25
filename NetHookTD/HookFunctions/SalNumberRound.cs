using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NetHookTD
{
    public static partial class NetHookTDClient
    {
        //Define delegate for exported function
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate NUMBER SalNumberRoundDelegate(NUMBER value );

        // Create function pointer to delegate
        private static SalNumberRoundDelegate SalNumberRound;

        // Define alternative function hook for original function
        private static NUMBER SalNumberRoundHook(NUMBER value)
        {
            // Get info on the specific installed hook from the list of registered hooks
            InstalledHook installedhook = InstalledHooks[(int)Hooks.SalNumberRound];
            // Get the hook variant which is registered
            int variant = installedhook.GetVariant();

            // If in TestMode, always use variant -1
            if (TestMode)
                variant = -1;

            switch (variant)
            {
                case -1:    // TestMode
                    // TestMode always returns a fixed value
                    NUMBER numberVal = new NUMBER();
                    double myvalue = 12345;
                    bool ok = SWinCvtDoubleToNumber(myvalue, ref numberVal);
                    return numberVal;
                case 1:
                    // Show messagebox and use the original function
                    string myfunction = (Hooks.SalGetVersion).ToString();
                    MessageBox.Show($"{myfunction} hook called", $"NetHookTD", MessageBoxButtons.OK);
                    return SalNumberRound(value);
                default:
                    // In all other cases, use the original function
                    return SalNumberRound(value);
            }
        }
    }
}
