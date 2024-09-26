# TD Runtime Hooking Framework

## What is Hooking?

**Hooking** is a technique used in software development to intercept and modify the behavior of a function or event within a system at runtime. By using hooks, developers can inject custom code or logic into existing TD runtime behavior without modifying the original source code. This is particularly useful for:

- **Monitoring and logging**: Observing function calls and their data.
- **Altering behavior**: Modifying how certain functions operate by replacing them with custom implementations.
- **Extending functionality**: Adding new features or logic that augment existing system behavior.

In a typical hooking scenario, a developer intercepts a call to a TD runtime function (Sal/Vis), allowing them to execute their own code before, after, or even in place of the original function.

## TD Runtime Hooking Framework

This framework allows developers to **hook into the TD runtime**. It provides a simple API for intercepting function calls, enabling you to replace or extend functionality seamlessly.

With this framework, you can:

- **Intercept function calls** within the TD runtime.
- **Inject custom logic** that runs alongside or instead of the existing code.
- **Control execution flow** by modifying the behavior of TD runtime functions at runtime.

### Key Features:
- **.NET**: Implement alternative functions in c# instead of c++.
- **Easy API**: Provides intuitive methods to set up and manage hooks.
- **Compatibility**: TD versions starting from TD 2.0 and up 
- **Safe and Flexible**: Hooks can be installed and removed dynamically, without the need to alter the original TD runtime.
- **DLLExport**: NetHookTD interface is given as native dll exports (with ordinal numbers). No need for GAIL (.NET Explorer). Definition is done within the External Functions section in the TD IDE.
- **Automatic TD version detection**: No need to link to specific TD version runtime. NetHookTD automatically detects the used TD version.
- **Extensible**: Add your own function hooks easily. 
- **Sal Api integration**: Gives some examples how to call TD functions from within c#

### Example Use Case (1):
Suppose the TD developer wants to set a specific Date/Time to be used within the TD application. This could be part of testing specific functionality.

The TD function

    Date/Time SalDateCurrent( )

is used to get the current date/time of the system.

This function is defined within the TD runtime library: **cdlliX.dll**
*(where X stands for the used TD version)*

Using NetHookTD, the behavior of this function can be altered. Assume the TD developer wants this function to always return a specific date : 01-01-2000

By hooking SalDateCurrent function, when the TD application calls it, an alternative function will be called instead. This alternate function is defined and coded within the NetHookTD assembly.

The alternative function will replace the original logic and will returm the needed date.

The TD application in turn will get the altered date as if this was the original value and will use it as given. This is completely transparent. The TD application does not "know" the value is not the original current date.

### Example Use Case (2):
Suppose the TD developer wants to have extra logging when using particular TD functions.
Suppose a Sal function is called with parameters. At runtime, the parameter values should be logged into a file for debugging purposes.

By hooking into that function, the parameters can be read and logged into the file
The original hooked Sal function is called and returns the original value back to the calling application.

No change how the original function works but it has now the extra ability to log the passed parameter values for a more detailed debuging experience.

## Usage

### NetHookTD c# solution:
The solution consists of two projects:
- **NetHookTD** : The actual NetHookTD assembly source code
- **NetHookTDTest** : Console test application for testing NetHook functionality

NetHookTD requires two NuGet packages which will be downloaded using the NuGet package Manager within Visual Studio 2022:

- EasyHook
- DllExport

The NetHookTest project is best to be used when implementing your own hooks.
Follow the same principle given by the examples.

Make sure the original solution compiles and builds.
Run the test application and check the output if the solution is working correctly on your system.

### Implement TD function call from c#:

First declare the the DllImport for the TD function in the test application. In this example **SalGetVersion** is used:

    [DllImport(cdlli_dll, CallingConvention = CallingConvention.StdCall)]
    public static extern ushort SalGetVersion();

You can find info on return value type and parameter types in centura.h file within the inc folder of your TD installation. Make sure you use the correct types for p-invoke.
You can find the list of types here:

https://learn.microsoft.com/en-us/dotnet/framework/interop/marshalling-data-with-platform-invoke

Then for initial testing call the function.
Place this in the section for manual misc tests within the Main() method.

    ushort version = SalGetVersion();
    LogToConsole($"SalGetVersion() -> {version}");

Run the test and be sure the function works as expected.

Now you can add the hook for this function within the NetHookTD project.

### Implement TD function hook:

First add the TD function name to the list of hooked functions. Each hooked function has its own hook ID. The Hook enum can be found in *NetHookTD_Client.cs*

        public enum Hooks
        {
            SalGetVersion = 1,
            SalDateCurrent = 2,
            SalDateToStr = 3,
            etc etc
        }

Create a new .cs file within the folder **HookFunctions**
Copy paste an existing file to be sure all needed code is present. Rename the file to the name of the TD function for clearity. In this example the file would be **SalGetVersion.cs**

Add the file to the NetHookTD project under folder HookFunctions.

Edit the file and change the existing code accoring to the TD function signature.
Here as example for SalGetVersion we need one delegate and one variable to hold the function pointer:

            //Define delegate for exported function
            [UnmanagedFunctionPointer(CallingConvention.StdCall, SetLastError = true)]
            private delegate ushort SalGetVersionDelegate();
    
            // Create function pointer to delegate
            private static SalGetVersionDelegate SalGetVersion;

Now it is time to implement the alternative method which will replace the behavior of the original one.

    // Define alternative function hook for original function
    private static ushort SalGetVersionHook()
    {
    	return SalGetVersion( );
    }

Above implementation shows that the alternative hook function will call the original **SalGetVersion** function and returns the value.

In this case, when SalGetVersion is hooked at runtime, no change is done to the behavior of the original function. It acts the same as normal.

But we want to change the behavior. Just as an example we want to have a messagebox shown when the TD application calls SalGetVersion and return the original value.

Implementation would be:

    // Define alternative function hook for original function
    private static ushort SalGetVersionHook()
    {
    	// Show messagebox and use the original function
    	string myfunction = (Hooks.SalGetVersion).ToString();
    	MessageBox.Show($"{myfunction} hook called", $"NetHookTD", MessageBoxButtons.OK);
    	return SalGetVersion();
    }

On each call of SalGetVersion a messagebox is shown and returns the original value.

To change the actual value which is returned:

    // Define alternative function hook for original function
    private static ushort SalGetVersionHook()
    {
    	ushort number = 80;
    	return number;
    }

When SalGetVersion is called it always returns 80.

