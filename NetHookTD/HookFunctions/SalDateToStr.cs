using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NetHookTD
{
    public static partial class NetHookTDClient
    {
        //Define delegate for exported function
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate int SalDateToStrDelegate(DATETIME date, ref UIntPtr datestring);

        // Create function pointer to delegate
        private static SalDateToStrDelegate SalDateToStr;

        // Define alternative function hook for original function
        private static int SalDateToStrHook(DATETIME date, ref UIntPtr datestring)
        {
            // Get info on the specific installed hook from the list of registered hooks
            InstalledHook installedhook = InstalledHooks[(int)Hooks.SalDateToStr];
            // Get the hook variant which is registered
            int variant = installedhook.GetVariant();

            // If in TestMode, always use variant -1
            if (TestMode)
                variant = -1;

            switch (variant)
            {
                case -1:
                    // TestMode always returns a fixed value
                    DATETIME newdate = SalDateConstruct(2000, 10, 28, 12, 55, 23);
                    return SalDateToStr(newdate, ref datestring);
                case 1:
                    // Show messagebox and use the original function
                    string myfunction = (Hooks.SalDateToStr).ToString();
                    MessageBox.Show($"{myfunction} hook called", $"NetHookTD", MessageBoxButtons.OK);
                    return SalDateToStr(date, ref datestring);
                default:
                    // In all other cases, use the original function
                    return SalDateToStr(date, ref datestring);
            }
        }


    }
}
