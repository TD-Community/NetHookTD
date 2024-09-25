using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;


namespace NetHookTD
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DATETIME
    {
        public byte DATETIME_Length;

        public byte DATA1;
        public byte DATA2;
        public byte DATA3;
        public byte DATA4;
        public byte DATA5;
        public byte DATA6;
        public byte DATA7;
        public byte DATA8;
        public byte DATA9;
        public byte DATA10;
        public byte DATA11;
        public byte DATA12;

        public byte DATA13;
        public byte DATA14;
        public byte DATA15;
        public byte DATA16;
        public byte DATA17;
        public byte DATA18;
        public byte DATA19;
        public byte DATA20;
        public byte DATA21;
        public byte DATA22;
        public byte DATA23;
        public byte DATA24;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        //public byte[] DATETIME_Buffer;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct NUMBER
    {
        public byte NUMBER_Length;

        public byte DATA1;
        public byte DATA2;
        public byte DATA3;
        public byte DATA4;
        public byte DATA5;
        public byte DATA6;
        public byte DATA7;
        public byte DATA8;
        public byte DATA9;
        public byte DATA10;
        public byte DATA11;
        public byte DATA12;

        public byte DATA13;
        public byte DATA14;
        public byte DATA15;
        public byte DATA16;
        public byte DATA17;
        public byte DATA18;
        public byte DATA19;
        public byte DATA20;
        public byte DATA21;
        public byte DATA22;
        public byte DATA23;
        public byte DATA24;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HUDV
    {
        public byte DATA1;
        public byte DATA2;
        public byte DATA3;
        public byte DATA4;
        public byte DATA5;
        public byte DATA6;
        public byte DATA7;
        public byte DATA8;
        public byte DATA9;
        public byte DATA10;
        public byte DATA11;
        public byte DATA12;
    }

    public static partial class NetHookTDClient
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern void OutputDebugString(string message);

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SalApiInitDelegate();
        private static SalApiInitDelegate SalApiInit;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool SWinCvtIntToNumberDelegate(int value, ref NUMBER newnumber);
        private static SWinCvtIntToNumberDelegate SWinCvtIntToNumber;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool SWinCvtDoubleToNumberDelegate(double value, ref NUMBER newnumber);
        private static SWinCvtDoubleToNumberDelegate SWinCvtDoubleToNumber;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool SWinCvtNumberToDoubleDelegate(ref NUMBER value, ref double myDouble);
        private static SWinCvtNumberToDoubleDelegate SWinCvtNumberToDouble;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate bool SWinCvtNumberToIntDelegate(ref NUMBER value, ref int myInt);
        private static SWinCvtNumberToIntDelegate SWinCvtNumberToInt;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SWinInitLPHSTRINGParamDelegate(ref UIntPtr hstring, int len);
        private static SWinInitLPHSTRINGParamDelegate SWinInitLPHSTRINGParam;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate IntPtr SWinStringGetBufferDelegate(UIntPtr hstring, ref int len);
        private static SWinStringGetBufferDelegate SWinStringGetBuffer;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void SWinHStringUnlockDelegate(UIntPtr hstring);
        private static SWinHStringUnlockDelegate SWinHStringUnlock;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate void SalHStringUnRefDelegate(UIntPtr hstring);
        private static SalHStringUnRefDelegate SalHStringUnRef;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SalDateYearDelegate(DATETIME date);
        private static SalDateYearDelegate SalDateYear;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SalDateMonthDelegate(DATETIME date);
        private static SalDateMonthDelegate SalDateMonth;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SalDateDayDelegate(DATETIME date);
        private static SalDateDayDelegate SalDateDay;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SalDateHourDelegate(DATETIME date);
        private static SalDateHourDelegate SalDateHour;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SalDateSecondDelegate(DATETIME date);
        private static SalDateSecondDelegate SalDateSecond;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate int SalDateMinuteDelegate(DATETIME date);
        private static SalDateMinuteDelegate SalDateMinute;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        private delegate DATETIME SalDateConstructDelegate(int year, int month, int day, int hour, int minute, int second);
        private static SalDateConstructDelegate SalDateConstruct;

        private static T GetFunctionDelegate<T>(string dllPath, string functionName) where T : Delegate
        {
            // Use GetModuleHandle to retrieve the handle to the already-loaded DLL
            IntPtr hModule = GetModuleHandle(dllPath);

            if (hModule == IntPtr.Zero)
            {
                Trace($"GetFunctionDelegate({dllPath},{functionName}) -> Error loading dll");
                throw new Exception($"Failed to load DLL: {dllPath}");
            }

            // Get the address of the specified function
            IntPtr pFunction = GetProcAddress(hModule, functionName);

            if (pFunction == IntPtr.Zero)
            {
                Trace($"GetFunctionDelegate({dllPath},{functionName}) -> Error getting function address");
                throw new Exception($"Failed to get function address for {functionName}");
            }

            // Create a delegate for the function dynamically
            T functionDelegate = Marshal.GetDelegateForFunctionPointer<T>(pFunction);

            Trace($"GetFunctionDelegate({dllPath},{functionName}) -> Installed ok");

            return functionDelegate;
        }

        private static bool GeneralApiInitialised = false;  // When all needed helper functions are delegated

        // Below dynamically determined values for TD version and dll's to hook into
        private static string TDVersion = "";
        private static string cdlli_dll = "";
        private static string vti_dll = "";

        public static string HStringToString(UIntPtr HString)
        {
            int len = 0;

            IntPtr TextPtr = SWinStringGetBuffer(HString, ref len);
            // Convert LPWSTR to .NET String.
            string StringVal = Marshal.PtrToStringUni(TextPtr, (len - 2) / 2);
            return StringVal;
        }

        public static string DATETIMEToString(DATETIME datetime)
        {
            int year = SalDateYear(datetime);
            int month = SalDateMonth(datetime);
            int day = SalDateDay(datetime);
            int hour = SalDateHour(datetime);
            int minute = SalDateMinute(datetime);
            int second = SalDateSecond(datetime);

            return $"{year}-{month}-{day} {hour}:{minute}:{second}";
        }

        public static bool StringToHString(string text, ref UIntPtr hstring)
        {
            byte[] newStringBytes = Encoding.Unicode.GetBytes(text);
            byte[] nullTerminator = new byte[] { 0, 0 }; // null-terminate the string
            byte[] finalBytes = new byte[newStringBytes.Length + nullTerminator.Length];

            Array.Copy(newStringBytes, finalBytes, newStringBytes.Length);
            Array.Copy(nullTerminator, 0, finalBytes, newStringBytes.Length, nullTerminator.Length);

            int nRet = SWinInitLPHSTRINGParam(ref hstring, finalBytes.Length);

            if (nRet == 1)
            {
                int len = 0;
                IntPtr newStringPtr = SWinStringGetBuffer(hstring, ref len);
                Marshal.Copy(finalBytes, 0, newStringPtr, finalBytes.Length);
                SWinHStringUnlock(hstring);
                return true;
            }
            else
                return false;
        }

        static NetHookTDClient()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            Trace($"NetHookTDClient() constructor from {Assembly.GetExecutingAssembly().Location}");
            Trace($"NetHookTD version {version}");

            // Subscribe to the AssemblyResolve event to force loading easyhook dll's from assembly folder
            AppDomain.CurrentDomain.AssemblyResolve += OnResolveAssembly;

            // Detect application ending for cleanup or trace that this is the case
            AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
            {
                // Just let us know the application ended
                Trace($"\nNetHookTD ProcessExit");
            };

            // Automatic detection which TD version is in use
            DetectTDVersion();
        }

        // Event handler to resolve missing assemblies
        private static Assembly OnResolveAssembly(object sender, ResolveEventArgs args)
        {
            // Extract the assembly name
            string assemblyName = new AssemblyName(args.Name).Name;

            if (assemblyName.Equals("easyhook", StringComparison.OrdinalIgnoreCase) || assemblyName.Equals("easyhook32", StringComparison.OrdinalIgnoreCase))
            {
                // Get the path to the folder where MyAssembly.dll is located
                string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

                // Combine the folder path with the base.dll file name
                string assemblyPath = Path.Combine(assemblyFolder, assemblyName + ".dll");

                if (File.Exists(assemblyPath))
                {
                    Trace("OnResolveAssembly( ) -> Resolving helper assembly: " + assemblyPath);
                    // Load and return the assembly from the local folder
                    return Assembly.LoadFrom(assemblyPath);
                }
                else
                {
                    Trace("OnResolveAssembly( ) -> Error resolving helper assembly: " + assemblyPath);
                }
            }

            // If not found, return null (the runtime will continue searching)
            return null;
        }

        private static bool InitGeneralApi()
        {
            if (!GeneralApiInitialised)
            {
                SWinInitLPHSTRINGParam = GetFunctionDelegate<SWinInitLPHSTRINGParamDelegate>(cdlli_dll, "SWinInitLPHSTRINGParam");
                SWinStringGetBuffer = GetFunctionDelegate<SWinStringGetBufferDelegate>(cdlli_dll, "SWinStringGetBuffer");
                SWinHStringUnlock = GetFunctionDelegate<SWinHStringUnlockDelegate>(cdlli_dll, "SWinHStringUnlock");
                SalHStringUnRef = GetFunctionDelegate<SalHStringUnRefDelegate>(cdlli_dll, "SalHStringUnRef");
                SalDateYear = GetFunctionDelegate<SalDateYearDelegate>(cdlli_dll, "SalDateYear");
                SalDateMonth = GetFunctionDelegate<SalDateMonthDelegate>(cdlli_dll, "SalDateMonth");
                SalDateDay = GetFunctionDelegate<SalDateDayDelegate>(cdlli_dll, "SalDateDay");
                SalDateHour = GetFunctionDelegate<SalDateHourDelegate>(cdlli_dll, "SalDateHour");
                SalDateSecond = GetFunctionDelegate<SalDateSecondDelegate>(cdlli_dll, "SalDateSecond");
                SalDateMinute = GetFunctionDelegate<SalDateMinuteDelegate>(cdlli_dll, "SalDateMinute");
                SalDateConstruct = GetFunctionDelegate<SalDateConstructDelegate>(cdlli_dll, "SalDateConstruct");
                SWinCvtIntToNumber = GetFunctionDelegate<SWinCvtIntToNumberDelegate>(cdlli_dll, "SWinCvtIntToNumber");
                SWinCvtDoubleToNumber = GetFunctionDelegate<SWinCvtDoubleToNumberDelegate>(cdlli_dll, "SWinCvtDoubleToNumber");
                SWinCvtNumberToInt = GetFunctionDelegate<SWinCvtNumberToIntDelegate>(cdlli_dll, "SWinCvtNumberToInt");
                SWinCvtNumberToDouble = GetFunctionDelegate<SWinCvtNumberToDoubleDelegate>(cdlli_dll, "SWinCvtNumberToDouble");
                SalApiInit = GetFunctionDelegate<SalApiInitDelegate>(cdlli_dll, "SalApiInit");
                int ret = SalApiInit();
                if (ret == 1)
                    GeneralApiInitialised = true;
                Trace($"InitGeneralApi() -> {GeneralApiInitialised}");
            }

            return GeneralApiInitialised;
        }

        private static void DetectTDVersion()
        {
            // Get the current process
            Process currentProcess = Process.GetCurrentProcess();

            // Check all loaded modules (DLLs) in the process
            var loadedModules = currentProcess.Modules;

            // Look for DLLs matching the pattern "cdlliX.dll" where X is a two-digit number
            string dllNamePattern = "cdlli";

            foreach (ProcessModule module in loadedModules)
            {
                string moduleName = module.ModuleName;

                // Check if the module name starts with "cdlli" and ends with ".dll"
                if (moduleName.StartsWith(dllNamePattern, StringComparison.OrdinalIgnoreCase)
                    && moduleName.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    // Extract the part after "cdlli" (e.g., the two digits)
                    string versionPart = moduleName.Substring(dllNamePattern.Length, moduleName.Length - dllNamePattern.Length - 4);

                    // Ensure the version part is exactly 2 digits
                    if (versionPart.Length == 2 && versionPart.All(char.IsDigit))
                    {
                        // Depending on the detected version, perform different actions
                        TDVersion = versionPart;
                        break; // Assuming only one version is loaded
                    }
                }
            }

            if (string.IsNullOrEmpty(TDVersion))
            {
                Trace($"DetectTDVersion() -> Error detecting TD version");
            }
            else
            {
                // Now construct the actual dll names (eg cdlli74.dll)
                // When other dll's are needed, add them here (eg cstruct etc)
                cdlli_dll = $"cdlli{TDVersion}.dll";
                vti_dll = $"vti{TDVersion}.dll";
                Trace($"DetectTDVersion() -> Found TD version: '{TDVersion}' using these dll's : {cdlli_dll},{vti_dll}");
            }
        }

        private static void Trace(string trace)
        {
            trace = "[NetHookTD] " + trace + "\n";
            // Write to attached degugger like DebugView. At runtime start DebugView tool to see these messages
            OutputDebugString(trace);
        }
    }
}
