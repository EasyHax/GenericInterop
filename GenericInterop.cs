public static class GenericInterop
    {
        public static T call<T>(this IntPtr addr, CallingConvention callingConvention, params object[] args)
        {
            var tArgs = new Type[args.Length];
            for (var i = 0; i < args.Length; i++)
                tArgs[i] = args[i].GetType();

            var @type = CreateDelegate(callingConvention, typeof(T), tArgs);
            var @delegate = Marshal.GetDelegateForFunctionPointer(addr, @type);
            return (T)@delegate.DynamicInvoke(args);
        }

        public unsafe static IntPtr byref<T>(ref T o)
        {
            var tr = __makeref(o);
            return *(IntPtr*)(&tr);
        }

        public static Delegate deleg(this IntPtr addr, Type type) => Marshal.GetDelegateForFunctionPointer(addr, type);
        unsafe public static IntPtr GetAddrOfVFunc(IntPtr pClass, int index_VFunc) => *(IntPtr*)(*(IntPtr*)pClass + index_VFunc * 0x4);

        public static Type CreateDelegate(CallingConvention callingConvention, Type returntype, params Type[] args)
        {
            Type tCallingConvention = typeof(System.Runtime.CompilerServices.CallConvStdcall);

            switch (callingConvention)
            {
                case CallingConvention.FastCall:
                    tCallingConvention = typeof(System.Runtime.CompilerServices.CallConvFastcall);
                    break;
                case CallingConvention.ThisCall:
                    tCallingConvention = typeof(System.Runtime.CompilerServices.CallConvThiscall);
                    break;
                case CallingConvention.Cdecl:
                    tCallingConvention = typeof(System.Runtime.CompilerServices.CallConvCdecl);
                    break;
            }

            var myAssemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName() { Name = "Temp" }, AssemblyBuilderAccess.Run);
            var dynamicMod = myAssemblyBuilder.DefineDynamicModule("TempModule");
            var tb = dynamicMod.DefineType(Guid.NewGuid().ToString(), TypeAttributes.Public | TypeAttributes.Sealed, typeof(MulticastDelegate));

            tb.DefineConstructor(MethodAttributes.RTSpecialName | MethodAttributes.SpecialName | MethodAttributes.Public | MethodAttributes.HideBySig, CallingConventions.Standard, 
                new Type[] { typeof(object), typeof(IntPtr) }).SetImplementationFlags(MethodImplAttributes.Runtime);
            tb.DefineMethod("Invoke", MethodAttributes.Public | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.HideBySig, CallingConventions.Standard, returntype, null, 
                new Type[] { tCallingConvention }, args, null, null).SetImplementationFlags(MethodImplAttributes.Runtime);

            return tb.CreateType();
        }
    }
