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
- **Easy API**: Provides intuitive methods to set up and manage hooks.
- **Compatibility**: TD versions starting from TD 2.0 and up 
- **Safe and Flexible**: Hooks can be installed and removed dynamically, without the need to alter the original TD runtime.
- **DLLExport**: NetHookTD interface is given as native dll exports (with ordinal numbers). No need for GAIL (.NET Explorer). Definition is done within the External Functions section in the TD IDE.
- **Automatic TD version detection**: No need to link to specific TD version runtime. NetHookTD automatically detects the used TD version.
- **Extensible**: Add your own function hooks easily. 
- **Sal Api integration**: Gives some examples how to call TD functions from within c#

### Example Use Case:
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