using System;
using System.Collections.Generic;


namespace NetHookTD
{
    public static partial class NetHookTDClient
    {
        // Keep a list of installed hooks so we know when a hook has been registered and able to unregister
        private static Dictionary<int, InstalledHook> InstalledHooks = new Dictionary<int, InstalledHook>();

        // Get the function pointer of the to be hooked function within the dll
        private static IntPtr GetProcAddressEasyHook(string dll, string function)
        {
            IntPtr proc = EasyHook.LocalHook.GetProcAddress(dll, function);
            return proc;
        }
    }

    // To store the hook and the variant info to be used in the list of hooks
    internal class InstalledHook
    {
        private EasyHook.LocalHook myHook;
        private int myVariant;  // defines the hook variant so multiple implementations can be created for one hooked function

        public InstalledHook(EasyHook.LocalHook hook, int variant)
        {
            myHook = hook;
            myVariant = variant;
        }

        public void DisposeHook()
        {
            // Remove the hook (unregister)
            myHook.Dispose();
        }

        public int GetVariant()
        {
            return myVariant;
        }
    }
}
