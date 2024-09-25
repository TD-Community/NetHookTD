using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NetHookTD
{
    public static partial class NetHookTDClient
    {
        //Define delegate for exported function
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate DATETIME SalDateCurrentDelegate();

        // Create function pointer to delegate
        private static SalDateCurrentDelegate SalDateCurrent;

        // Define alternative function hook for original function
        private static DATETIME SalDateCurrentHook()
        {
            // Get info on the specific installed hook from the list of registered hooks
            InstalledHook installedhook = InstalledHooks[(int)Hooks.SalDateCurrent];
            // Get the hook variant which is registered
            int variant = installedhook.GetVariant();

            // If in TestMode, always use variant -1
            if (TestMode)
                variant = -1;

            switch (variant)
            {
                case -1:
                    // TestMode always returns a fixed value
                    return SalDateConstruct(1970, 5, 2, 13, 36, 12);
                case 1:
                    // Show messagebox and use the original function
                    string myfunction = (Hooks.SalDateCurrent).ToString();
                    MessageBox.Show($"{myfunction} hook called", $"NetHookTD", MessageBoxButtons.OK);
                    return SalDateCurrent();
                default:
                    // In all other cases, use the original function
                    return SalDateCurrent();
            }
        }
    }
}
