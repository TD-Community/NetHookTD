using System;
using System.Runtime.InteropServices;


namespace NetHookTD
{
    // List of functions which can be hooked. For new functions to be hooked, add it here
    public enum Hooks
    {
        SalGetVersion = 1,
        SalDateCurrent = 2,
        SalDateToStr = 3,
        VisStrChoose = 4,
        SalNumberRound = 5
    }

    public static partial class NetHookTDClient
    {
        private static string LastMsg = "";     // Info string kept for each called NetHookTD method to query for debug purposes
        private static bool TestMode = false;   // When TRUE all hooks use fixed values for testing purposes

        [DllExport(CallingConvention.StdCall)]
        private static bool NH_InstallHook(int hookId, int variant)
        {
            // Win32 unmanaged side. Called from TD exported functions
            return InstallHook(hookId, variant);
        }

        [DllExport(CallingConvention.StdCall)]
        private static bool NH_RemoveHook(int hookId)
        {
            // Win32 unmanaged side. Called from TD exported functions
            return RemoveHook(hookId);
        }

        [DllExport(CallingConvention.StdCall)]
        private static bool NH_RemoveAllHooks()
        {
            // Win32 unmanaged side. Called from TD exported functions
            return RemoveAllHooks();
        }

        [DllExport(CallingConvention.StdCall)]
        private static void NH_SetTestMode(bool enable)
        {
            // Win32 unmanaged side. Called from TD exported functions
            SetTestMode(enable);
        }

        [DllExport(CallingConvention.StdCall)]
        private static bool NH_GetTestMode()
        {
            // Win32 unmanaged side. Called from TD exported functions
            return GetTestMode();
        }

        [DllExport(CallingConvention.StdCall)]
        private static bool NH_GetHookInfo(int hookId, out bool IsInstalled, out int variant)
        {
            // Win32 unmanaged side. Called from TD exported functions
            return GetHookInfo(hookId, out IsInstalled, out variant);
        }

        [DllExport(CallingConvention.StdCall)]
        private static IntPtr NH_GetLastMsg()
        {
            // Win32 unmanaged side. Called from TD exported functions

            string managedString = LastMsg;
            IntPtr unmanagedString;

            // Allocate unmanaged memory and copy the string data to unmanaged memory
            if (IsUnicode)
                unmanagedString = Marshal.StringToHGlobalUni(managedString);
            else
                unmanagedString = Marshal.StringToHGlobalAnsi(managedString);


            // Return the pointer to the unmanaged memory
            return unmanagedString;
        }

        [DllExport(CallingConvention.StdCall)]
        private static IntPtr NH_GetLastMsgA()
        {
            // Win32 unmanaged side. Called from TD exported functions

            string managedString = LastMsg;
            IntPtr unmanagedString;

            // Allocate unmanaged memory and copy the string data to unmanaged memory
            if (IsUnicode)
                unmanagedString = Marshal.StringToHGlobalUni(managedString);
            else
                unmanagedString = Marshal.StringToHGlobalAnsi(managedString);


            // Return the pointer to the unmanaged memory
            return unmanagedString;
        }

        [DllExport(CallingConvention.StdCall)]
        private static IntPtr NH_GetDetectedTDVersion()
        {
            // Win32 unmanaged side. Called from TD exported functions

            string managedString = GetDetectedTDVersion();
            IntPtr unmanagedString;

            // Allocate unmanaged memory and copy the string data to unmanaged memory
            if (IsUnicode)
                unmanagedString = Marshal.StringToHGlobalUni(managedString);
            else
                unmanagedString = Marshal.StringToHGlobalAnsi(managedString);

            // Return the pointer to the unmanaged memory
            return unmanagedString;
        }

        [DllExport(CallingConvention.StdCall)]
        private static IntPtr NH_GetDetectedTDVersionA()
        {
            // Win32 unmanaged side. Called from TD exported functions

            string managedString = GetDetectedTDVersion();
            IntPtr unmanagedString;

            // Allocate unmanaged memory and copy the string data to unmanaged memory
            if (IsUnicode)
                unmanagedString = Marshal.StringToHGlobalUni(managedString);
            else
                unmanagedString = Marshal.StringToHGlobalAnsi(managedString);

            // Return the pointer to the unmanaged memory
            return unmanagedString;
        }

        public static bool InstallHook(int hookId, int variant)
        {
            ClearLastMsg();

            if (!Enum.IsDefined(typeof(Hooks), hookId))
            {
                SetLastMsg($"InstallHook( ) -> Error: Unknown hook (id={hookId})");
                return false;
            }

            if (!InitGeneralApi())
            {
                SetLastMsg($"InstallHook( ) -> Error: general api not initialised");
                return false;
            }

            IntPtr myproc;
            string myfunction;
            string mydll;
            Delegate mydelegate;

            myfunction = ((Hooks)hookId).ToString();

            if (InstalledHooks.ContainsKey(hookId))
            {
                if ( InstalledHooks[hookId].GetVariant() != variant)
                {
                    // Install a different variant. First unregister the existing hook
                    bool ok = RemoveHook(hookId);
                }
                else
                {
                    // Hook and variant already installed. So we can stop here
                    SetLastMsg($"InstallHook( ) -> Hook for {myfunction} (id={hookId}, variant={variant}) is already installed");
                    return true;
                }
            }

            try
            {
                switch ((Hooks)hookId)
                {
                    case Hooks.SalGetVersion:
                        mydll = cdlli_dll;  // THe dll where the function resides
                        if (SalGetVersion==null)
                            SalGetVersion = GetFunctionDelegate<SalGetVersionDelegate>(mydll, myfunction);
                        mydelegate = new SalGetVersionDelegate(SalGetVersionHook);
                        break;
                    case Hooks.SalDateCurrent:
                        mydll = cdlli_dll;  // THe dll where the function resides
                        if (SalDateCurrent == null)
                            SalDateCurrent = GetFunctionDelegate<SalDateCurrentDelegate>(mydll, myfunction);
                        mydelegate = new SalDateCurrentDelegate(SalDateCurrentHook);
                        break;
                    case Hooks.SalDateToStr:
                        mydll = cdlli_dll;  // THe dll where the function resides
                        if (SalDateToStr == null)
                            SalDateToStr = GetFunctionDelegate<SalDateToStrDelegate>(mydll, myfunction);
                        mydelegate = new SalDateToStrDelegate(SalDateToStrHook);
                        break;
                    case Hooks.VisStrChoose:
                        mydll = vti_dll;    // THe dll where the function resides
                        if (VisStrChoose == null)
                            VisStrChoose = GetFunctionDelegate<VisStrChooseDelegate>(mydll, myfunction);
                        mydelegate = new VisStrChooseDelegate(VisStrChooseHook);
                        break;
                    case Hooks.SalNumberRound:
                        mydll = cdlli_dll;  // THe dll where the function resides
                        if (SalNumberRound == null)
                            SalNumberRound = GetFunctionDelegate<SalNumberRoundDelegate>(mydll, myfunction);
                        mydelegate = new SalNumberRoundDelegate(SalNumberRoundHook);
                        break;
                    default:
                        return false;
                }

                myproc = GetProcAddressEasyHook(mydll, myfunction);
                EasyHook.LocalHook myhook = EasyHook.LocalHook.Create(myproc, mydelegate, null);
                myhook.ThreadACL.SetInclusiveACL(new int[] { 0 });

                // Add the hook and variant into a list of hooks to be used later
                InstalledHooks.Add(hookId, new InstalledHook(myhook, variant));

                SetLastMsg($"InstallHook( ) -> Hook for {myfunction} (id={hookId}, variant={variant}) is installed");

                return true;
            }
            catch(Exception ex)
            {
                SetLastMsg($"InstallHook( ) -> Error installing hook for {myfunction} (id={hookId}) : {ex.Message}");
                return false;
            }
        }

        public static bool RemoveHook(int hookId)
        {
            ClearLastMsg();

            if (!Enum.IsDefined(typeof(Hooks), hookId))
            {
                SetLastMsg($"RemoveHook( ) -> Error: Unknown hook (id={hookId})");
                return false;
            }

            string myfunction = ((Hooks)hookId).ToString();

            if (!InstalledHooks.ContainsKey(hookId))
            {
                SetLastMsg($"RemoveHook( ) -> Hook for {myfunction} (id={hookId}) was not installed");
                return false;
            }

            try
            {
                InstalledHook installedhook = InstalledHooks[hookId];

                installedhook.DisposeHook();

                InstalledHooks.Remove(hookId);

                SetLastMsg($"RemoveHook( ) -> Hook for {myfunction} (id={hookId}) is removed");
                return true;
            }
            catch(Exception ex)
            {
                SetLastMsg($"RemoveHook( ) -> Error removing hook for {myfunction} (id={hookId}) : {ex.Message}");
                return false;
            }
        }

        public static bool RemoveAllHooks()
        {
            ClearLastMsg();

            if (!(InstalledHooks.Count > 0))
            {
                SetLastMsg($"RemoveAllHooks() -> No hooks are installed");
                return true;
            }

            try
            {
                foreach (var hook in InstalledHooks)
                {
                    InstalledHook installedhook = hook.Value;
                    installedhook.DisposeHook();
                }

                InstalledHooks.Clear();

                SetLastMsg($"RemoveAllHooks() -> All hooks are removed");
                return true;
            }
            catch (Exception ex)
            {
                SetLastMsg($"RemoveAllHooks() -> Error removing all hooks : {ex.Message}");
                return false;
            }
        }

        public static void SetTestMode( bool enable)
        {
            Trace($"SetTestMode({enable})");
            TestMode = enable;
        }

        public static bool GetTestMode()
        {
            Trace($"GetTestMode() -> {TestMode}");
            return TestMode;
        }

        public static bool GetHookInfo(int hookId, out bool IsInstalled, out int variant)
        {
            ClearLastMsg();

            variant = 0;
            IsInstalled = false;

            if (!Enum.IsDefined(typeof(Hooks), hookId))
            {
                SetLastMsg($"GetHookInfo() -> Error: Unknown hook (id={hookId})");
                return false;
            }

            string myfunction = ((Hooks)hookId).ToString();

            if (InstalledHooks.ContainsKey(hookId))
            {
                InstalledHook installedhook = InstalledHooks[hookId];
                IsInstalled = true;
                variant = installedhook.GetVariant();
                SetLastMsg($"GetHookInfo() -> Hook for {myfunction} (id={hookId}, variant={variant}) is installed");
                return true;
            }
            else
            {
                SetLastMsg($"GetHookInfo() -> Hook for {myfunction} (id={hookId}) is not installed");
                return true;
            }
        }

        public static string GetLastMsg()
        {
            return LastMsg;
        }

        public static string GetDetectedTDVersion()
        {
            return TDVersion;
        }

        private static void ClearLastMsg()
        {
            LastMsg = "";
        }

        private static void SetLastMsg(string msg)
        {
            LastMsg = msg;
            Trace(LastMsg);
        }
    }
}
