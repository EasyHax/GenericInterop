# GenericInterop
Class to make interop easier, especially with managed code injected into unmanaged process.


### Code Simplification :
```cs
public static Type tIntPtr = typeof(IntPtr);
public static Type tInt32  = typeof(int   );
public static Type tInt64  = typeof(long  );
public static Type tByte   = typeof(byte  );
public static Type tUint   = typeof(uint  );
public static Type tBool   = typeof(bool  );
public static Type tFloat  = typeof(float );
public static Type tString = typeof(string);
public static Type tVoid   = null;
```

## Example 1 : Call Virtual Functions ( Useful in game hacking )

### Some c++ interface we want to interact with
```cpp
struct SomeStruct { };

class SomeNativeClass {
public:
	virtual void Func0(void);
	virtual int Func1(int x, int y);
	virtual SomeStruct* Func2(const char* c);
};
```

### Call Func1
```cs
var pSomeNativeClass = (IntPtr)192156486; // pointer to SomeNativeClass

// Wrap Func1 and DynamicInvoke it later
var VFunc1 = pSomeNativeClass.WrapVFunc(1, tInt32, tInt32, tInt32);
// call Func1, don't forget this pointer
var ret_vfunc1 = VFunc1.DynamicInvoke(pSomeNativeClass, 12, 13); 

// Directly call Func1
var addr_VFunc1 = GenericInterop.GetAddrOfVFunc(pSomeNativeClass, 1);
// call Func1, don't forget this pointer
var ret_vfunc1  = addr_VFunc1.call<int>(CallingConvention.ThisCall, pSomeNativeClass, 12, 13);
```
### Call Func2
```cs
var pSomeNativeClass = (IntPtr)192156486; // pointer to SomeNativeClass
var addr_VFunc1      = GenericInterop.GetAddrOfVFunc(pSomeNativeClass, 2);

// Wrap Func2 and DynamicInvoke it later
var VFunc2 = addr_VFunc2.deleg(CallingConvention.ThisCall, tIntPtr, tString);
  // call Func2, don't forget this pointer
var ret_vfunc2 = VFunc2.DynamicInvoke(pSomeNativeClass, "SomeString"); 

// Directly call Func2
  // call Func2, don't forget this pointer
var ret_vfunc2 = addr_VFunc2.call<int>(CallingConvention.ThisCall, pSomeNativeClass, "SomeString");
```

## Example 2 : Call WinApi Functions ( Useless, except for dynamic api call )

```cs
class WinApi
{
    [DllImport("kernel32.dll")]
    public static extern IntPtr LoadLibrary(string lib_name);
    [DllImport("kernel32.dll")]
    public static extern IntPtr GetProcAddress(IntPtr address, string func_name);
}
```

### Call GetCurrentThreadStackLimits

```cs
//void GetCurrentThreadStackLimits(out uint lowLimit, out uint highLimit);
//Since it has 'out' parameters, we have to call deleg first

var addr_GetCTSL = WinApi.GetProcAddress(WinApi.LoadLibrary("kernel32.dll"), "GetCurrentThreadStackLimits");
var GetCTSL = addr_GetSysColor.deleg(CallingConvention.Winapi, tVoid, tIntPtr, tIntPtr);
var lowLimit  = 0u;
var highLimit = 0u;

GetCTSL.DynamicInvoke(GenericInterop.byref(ref lowLimit), GenericInterop.byref(ref highLimit));
GetCTSL.DynamicInvoke((IntPtr)(&lowLimit), (IntPtr)(&highLimit));
//lowLimit  = 6291456
//highLimit = 7340032
```
