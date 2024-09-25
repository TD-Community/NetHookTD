using System;
using System.Runtime.InteropServices;
using NetHookTD;




namespace NetHookTDTest
{
     internal class Program
    {
        // For testing first set the needed TD version
        public const string TDVersion = "40";
        public const string cdlli_dll = "cdlli" + TDVersion + ".dll";
        public const string vti_dll = "vti" + TDVersion + ".dll";

        // Make sure to set the folder location of the needed TD runtime/IDE installation
        public const string TDInstallation = @"D:\Team Developer\Team Developer 4.0\";

        static void Main(string[] args)
        {
            // Make sure this test console application can find the needed cdlli and vti dll's
            // Change the const strings above to the needed TD version

            var name = "PATH";
            var scope = EnvironmentVariableTarget.Machine;
            var oldValue = Environment.GetEnvironmentVariable(name, scope);
            var newValue = oldValue + ";" + TDInstallation;
            Environment.SetEnvironmentVariable(name, newValue);

            Console.WriteLine($"***************** NetHookTD settings *****************\n");
            Console.WriteLine($"TD Version: {TDVersion}");
            Console.WriteLine($"IDE/Runtime location: {TDInstallation}\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"Change the constants for TD version and location to use a different TD version (v5.1 and up)\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"Press enter to start the tests");
            Console.ReadLine();

            // Force loading TD runtime dll. This simulates running the test within the needed TD version
            SalGetVersion();

            if (true)   // TEST HOOKS
            {
                Console.WriteLine($"================ Running hooking tests =======================\n");

                Test_SalGetVersion();
                Test_SalDateCurrent();
                Test_SalDateToStr();
                Test_VisStrChoose();
                Test_SalNumberRound();

                Console.WriteLine($"\n================ Hooking tests done =======================\n");

            }

            if (true)  // These are manual misc tests
            {
                Console.WriteLine($"\n================ Running misc tests =======================\n");
                // Must init sal api first
                SalApiInit();

                // Display which TD version was automatically detected
                LogToConsole($"Detected TD version -> {NetHookTDClient.GetDetectedTDVersion()}");

                // First test numbers. Using SalGetVersion

                ushort version = SalGetVersion();
                LogToConsole($"SalGetVersion() -> {version}");

                // Test datetime.
                DATETIME dateCurrent = SalDateCurrent();
                LogToConsole($"SalDateCurrent() -> {NetHookTDClient.DATETIMEToString(dateCurrent)}");

                // Create a new DATETIME with fixed values
                DATETIME dateConstruct = SalDateConstruct(2023, 11, 26, 13, 10, 59);
                LogToConsole($"SalDateConstruct() -> {NetHookTDClient.DATETIMEToString(dateConstruct)}");

                int len = 0;
                UIntPtr errorTextPtr = SqlGetErrorTextX(1);
                string ErrText = NetHookTDClient.HStringToString(errorTextPtr);
                LogToConsole($"SqlGetErrorTextX(1) -> {ErrText}");

                // Call function and receive a LPHSTRING
                UIntPtr dateTextPtr = new UIntPtr(0);
                
                int nret = SalDateToStr(dateCurrent, ref dateTextPtr);

                // Convert LPWSTR to .NET String.
                string dateText = NetHookTDClient.HStringToString(dateTextPtr);
                LogToConsole(@"SalDateToStr() -> " + dateText);


                // Try create a new HSTRING to be used in passing strings????
                string myText = "This is a text";

                //string managedString = Marshal.PtrToStringUni(buffer);
                UIntPtr newHStringPtr = new UIntPtr(0);

                if (NetHookTDClient.StringToHString(myText, ref newHStringPtr))
                {
                    string sUpper = NetHookTDClient.HStringToString(SalStrUpperX(newHStringPtr));
                    LogToConsole($"SalStrUpperX('{myText}') -> {sUpper}");
                    SalHStringUnRef(newHStringPtr);
                }

                UIntPtr newHStringTruePtr = new UIntPtr(0);
                UIntPtr newHStringFalsePtr = new UIntPtr(0);

                NetHookTDClient.StringToHString("True text is used", ref newHStringTruePtr);
                NetHookTDClient.StringToHString("False text is used", ref newHStringFalsePtr);

                string sChosen = NetHookTDClient.HStringToString(VisStrChoose(false, newHStringTruePtr, newHStringFalsePtr));
                LogToConsole($"VisStrChoose() -> {sChosen}");

                NUMBER numberVal = new NUMBER();
                double myvalue = 123.55;
                bool ok = SWinCvtDoubleToNumber(myvalue, ref numberVal);
                
                NUMBER roundedNum = SalNumberRound(numberVal);
                double rounded = new double();
                ok = SWinCvtNumberToDouble(ref roundedNum, ref rounded);

                LogToConsole($"SalNumberRound({myvalue}) -> {rounded}");

                Console.WriteLine($"\n================ Running misc tests done =======================\n");
            }

            Console.WriteLine($"Press enter");
            Console.ReadLine();
        }


        private static void Test_SalGetVersion()
        {
            LogToConsole($"\n=== Test_SalGetVersion ====\n");

            int hookId = (int)Hooks.SalGetVersion;

            NetHookTDClient.SetTestMode(true);

            LogToConsole($"SetTestMode({NetHookTDClient.GetTestMode()})");

            // First call the "original" function
            ushort version1 = SalGetVersion();
            LogToConsole($"(Original) SalGetVersion -> {version1}");

            // Install the hook
            bool error = NetHookTDClient.InstallHook(hookId, 0);
            LogToConsole("InstallHook", error, NetHookTDClient.GetLastMsg());

            // Call the hooked function
            ushort version2 = SalGetVersion();
            LogToConsole($"(Hooked) SalGetVersion -> {version2}");

            //Remove the hook
            error = NetHookTDClient.RemoveHook(hookId);
            LogToConsole("RemoveHook", error, NetHookTDClient.GetLastMsg());

            // Call the "original" function
            ushort version3 = SalGetVersion();
            LogToConsole($"(Original) SalGetVersion -> {version3}");

            // Install the hook again, see if that works
            error = NetHookTDClient.InstallHook(hookId, 0);
            LogToConsole("InstallHook", error, NetHookTDClient.GetLastMsg());

            // Call the hooked function
            ushort version4 = SalGetVersion();
            LogToConsole($"(Hooked) SalGetVersion -> {version4}");

            //Remove the hook
            error = NetHookTDClient.RemoveHook(hookId);
            LogToConsole("RemoveHook", error, NetHookTDClient.GetLastMsg());
        }

        private static void Test_SalDateCurrent()
        {
            LogToConsole($"\n=== Test_SalDateCurrent ====\n");

            int hookId = (int)Hooks.SalDateCurrent;

            //NetHookTDClient hookClient = new NetHookTDClient();
            NetHookTDClient.SetTestMode(true);
            LogToConsole($"SetTestMode({NetHookTDClient.GetTestMode()})");

            // First call the "original" function
            DATETIME date1 = SalDateCurrent();
            LogToConsole($"(Original) SalDateCurrent -> {NetHookTDClient.DATETIMEToString(date1)}");

            // Install the hook
            bool error = NetHookTDClient.InstallHook(hookId, 0);
            LogToConsole("InstallHook", error, NetHookTDClient.GetLastMsg());

            // Call the hooked function
            DATETIME date2 = SalDateCurrent();
            LogToConsole($"(Hooked) SalDateCurrent -> {NetHookTDClient.DATETIMEToString(date2)}");

            //Remove the hook
            error = NetHookTDClient.RemoveHook(hookId);
            LogToConsole("RemoveHook", error, NetHookTDClient.GetLastMsg());

            // Call the "original" function
            DATETIME date3 = SalDateCurrent();
            LogToConsole($"(Original) SalDateCurrent -> {NetHookTDClient.DATETIMEToString(date3)}");

            // Install the hook again, see if that works
            error = NetHookTDClient.InstallHook(hookId, 0);
            LogToConsole("InstallHook", error, NetHookTDClient.GetLastMsg());

            // Call the hooked function
            DATETIME date4 = SalDateCurrent();
            LogToConsole($"(Hooked) SalDateCurrent -> {NetHookTDClient.DATETIMEToString(date4)}");

            //Remove the hook
            error = NetHookTDClient.RemoveHook(hookId);
            LogToConsole("RemoveHook", error, NetHookTDClient.GetLastMsg());
        }

        private static void Test_SalDateToStr()
        {
            LogToConsole($"\n=== Test_SalDateToStr ====\n");

            int hookId = (int)Hooks.SalDateToStr;

            UIntPtr dateTextPtr;
            int len;
            IntPtr datePtr;
            string dateText;

            //NetHookTDClient hookClient = new NetHookTDClient();
            NetHookTDClient.SetTestMode(true);
            LogToConsole($"SetTestMode({NetHookTDClient.GetTestMode()})");

            // First call the "original" function
            // Call function and receive a LPHSTRING
            dateTextPtr = new UIntPtr(0);
            len = SalDateToStr(SalDateCurrent(), ref dateTextPtr);
            dateText = NetHookTDClient.HStringToString(dateTextPtr);
            LogToConsole($"(Original) SalDateToStr -> {dateText}");

            // Install the hook
            bool error = NetHookTDClient.InstallHook(hookId, 0);
            LogToConsole("InstallHook", error, NetHookTDClient.GetLastMsg());

            // Call function and receive a LPHSTRING
            dateTextPtr = new UIntPtr(0);
            len = SalDateToStr(SalDateCurrent(), ref dateTextPtr);
            dateText = NetHookTDClient.HStringToString(dateTextPtr);
            LogToConsole($"(Hooked) SalDateToStr -> {dateText}");

            //Remove the hook
            error = NetHookTDClient.RemoveHook(hookId);
            LogToConsole("RemoveHook", error, NetHookTDClient.GetLastMsg());
        }

        private static void Test_VisStrChoose()
        {
            LogToConsole($"\n=== Test_VisStrChoose ====\n");

            int hookId = (int)Hooks.VisStrChoose;

            NetHookTDClient.SetTestMode(true);
            LogToConsole($"SetTestMode({NetHookTDClient.GetTestMode()})");

            // First call the "original" function
            UIntPtr newHStringTruePtr = new UIntPtr(0);
            UIntPtr newHStringFalsePtr = new UIntPtr(0);
            NetHookTDClient.StringToHString("True text is used", ref newHStringTruePtr);
            NetHookTDClient.StringToHString("False text is used", ref newHStringFalsePtr);

            string sChosen = NetHookTDClient.HStringToString(VisStrChoose(false, newHStringTruePtr, newHStringFalsePtr));
            LogToConsole($"(Original) VisStrChoose -> {sChosen}");

            // Install the hook
            bool error = NetHookTDClient.InstallHook(hookId, 0);
            LogToConsole("InstallHook", error, NetHookTDClient.GetLastMsg());

            sChosen = NetHookTDClient.HStringToString(VisStrChoose(true, newHStringTruePtr, newHStringFalsePtr));
            LogToConsole($"(Hooked) VisStrChoose -> {sChosen}");

            //Remove the hook
            error = NetHookTDClient.RemoveHook(hookId);
            LogToConsole("RemoveHook", error, NetHookTDClient.GetLastMsg());
        }

        private static void Test_SalNumberRound()
        {
            LogToConsole($"\n=== Test_SalNumberRound ====\n");

            int hookId = (int)Hooks.SalNumberRound;

            NetHookTDClient.SetTestMode(true);
            LogToConsole($"SetTestMode({NetHookTDClient.GetTestMode()})");

            // First call the "original" function
            NUMBER numberVal = new NUMBER();
            double myvalue = 123.55;
            bool ok = SWinCvtDoubleToNumber(myvalue, ref numberVal);
            NUMBER rounded = SalNumberRound(numberVal);
            double roundedDouble = new double();
            ok = SWinCvtNumberToDouble(ref rounded, ref roundedDouble);
            LogToConsole($"(Original) SalNumberRound({myvalue}) -> {roundedDouble}");

            // Install the hook
            bool error = NetHookTDClient.InstallHook(hookId, 0);
            LogToConsole("InstallHook", error, NetHookTDClient.GetLastMsg());

            numberVal = new NUMBER();
            myvalue = 123.55;
            ok = SWinCvtDoubleToNumber(myvalue, ref numberVal);
            rounded = SalNumberRound(numberVal);
            roundedDouble = new double();
            ok = SWinCvtNumberToDouble(ref rounded, ref roundedDouble);
            LogToConsole($"(Hooked) SalNumberRound({myvalue}) -> {roundedDouble}");

            //Remove the hook
            error = NetHookTDClient.RemoveHook(hookId);
            LogToConsole("RemoveHook", error, NetHookTDClient.GetLastMsg());

            // Call the "original" function
            numberVal = new NUMBER();
            myvalue = 123.55;
            ok = SWinCvtDoubleToNumber(myvalue, ref numberVal);
            rounded = SalNumberRound(numberVal);
            roundedDouble = new double();
            ok = SWinCvtNumberToDouble(ref rounded, ref roundedDouble);
            LogToConsole($"(Hooked) SalNumberRound({myvalue}) -> {roundedDouble}");

            // Install the hook again, see if that works
            error = NetHookTDClient.InstallHook(hookId, 0);
            LogToConsole("InstallHook", error, NetHookTDClient.GetLastMsg());

            // Call the hooked function
            numberVal = new NUMBER();
            myvalue = 123.55;
            ok = SWinCvtDoubleToNumber(myvalue, ref numberVal);
            rounded = SalNumberRound(numberVal);
            roundedDouble = new double();
            ok = SWinCvtNumberToDouble(ref rounded, ref roundedDouble);
            LogToConsole($"(Hooked) SalNumberRound({myvalue}) -> {roundedDouble}");

            //Remove the hook
            error = NetHookTDClient.RemoveHook(hookId);
            LogToConsole("RemoveHook", error, NetHookTDClient.GetLastMsg());
        }

        private static void LogToConsole(string method, bool error, string msg)
        {
            Console.WriteLine($"{method}\t{error}\t{msg}");
        }

        private static void LogToConsole(string msg)
        {
            Console.WriteLine($"{msg}");
        }

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SalApiInit();

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern ushort SalGetVersion();

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern DATETIME SalDateCurrent();

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SalDateToStr(DATETIME date, ref UIntPtr datestring);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern DATETIME SalDateConstruct(int year, int month, int day, int hour, int minute, int second);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern int SWinInitLPHSTRINGParam(ref UIntPtr hstring, int len);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr SqlGetErrorTextX(int code);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr SWinStringGetBuffer(UIntPtr hstring, ref int len);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr SalStrUpperX(UIntPtr hstring);

        [DllImport(vti_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern UIntPtr VisStrChoose(bool expression, UIntPtr hstringTrue, UIntPtr hstringFalse);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern void SWinHStringUnlock(UIntPtr hstring);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern void SalHStringUnRef(UIntPtr hstring);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern NUMBER SalNumberRound(NUMBER value);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SWinCvtIntToNumber(int value, ref NUMBER newnumber);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SWinCvtDoubleToNumber(double value, ref NUMBER newnumber);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SWinCvtNumberToDouble(ref NUMBER value, ref double mydouble);

        [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
        public static extern bool SWinCvtNumberToInt(ref NUMBER value, ref int myInt);

    }
}
