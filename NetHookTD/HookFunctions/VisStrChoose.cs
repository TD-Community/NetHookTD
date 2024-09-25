using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NetHookTD
{
    public static partial class NetHookTDClient
    {
        //Define delegate for exported function
        [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
        private delegate UIntPtr VisStrChooseDelegate(bool expression, UIntPtr hstringTrue, UIntPtr hstringFalse);

        // Create function pointer to delegate
        private static VisStrChooseDelegate VisStrChoose;

        // Define alternative function hook for original function
        private static UIntPtr VisStrChooseHook(bool expression, UIntPtr hstringTrue, UIntPtr hstringFalse)
        {
            // Get info on the specific installed hook from the list of registered hooks
            InstalledHook installedhook = InstalledHooks[(int)Hooks.VisStrChoose];
            // Get the hook variant which is registered
            int variant = installedhook.GetVariant();

            // If in TestMode, always use variant -1
            if (TestMode)
                variant = -1;

            switch (variant)
            {
                case -1:    // TestMode
                    // TestMode always returns a fixed value
                    UIntPtr newHStringTruePtr = new UIntPtr(0);
                    UIntPtr newHStringFalsePtr = new UIntPtr(0);
                    StringToHString("This TRUE string is altered", ref newHStringTruePtr);
                    StringToHString("This FALSE string is altered", ref newHStringFalsePtr);
                    return VisStrChoose(expression, newHStringTruePtr, newHStringFalsePtr);
                case 1:
                    // Show messagebox and use the original function
                    string myfunction = (Hooks.SalGetVersion).ToString();
                    MessageBox.Show($"{myfunction} hook called", $"NetHookTD", MessageBoxButtons.OK);
                    return VisStrChoose(expression, hstringTrue, hstringFalse);
                default:
                    // In all other cases, use the original function
                    return VisStrChoose(expression, hstringTrue, hstringFalse);
            }
        }
    }
}
