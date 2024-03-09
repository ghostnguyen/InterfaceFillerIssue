## InterfaceFiller - Aspect Oriented Programming (AOP) in C# (AOT support)

The most minimal and concise but fully features AOP library in C#. All junk codes are 4 custom attributes:

    - InterfaceFiller(params string[] wrappers)
    - Wrapper()
    - CallerParamByIndex(int value, bool fromEnd = false)
    - CallerParamByName(string paramName)

Support:
- AOT
- Async (await) method
- C# Caller attribute and two more
- Roslyn analyzers

Within minutes reading, all magic is revealed by 'Find all References'.

**Table of content:**

 - [1. InterfaceFiller attribute]
 - [2. Wrapper attribute]
 - [3. Wrapper parameters]
 - [4. Wrapper parameters resolution]
 - [5. Wrapper (with parameter) for `Task` method]
 - [6. Wrapper (with parameter) for `Task<TResult>` method]
 - [7. C# Caller attributes]
 - [8. CallerParamByName and CallerParamByIndex attributes]
 - [9. Reuse your Wrapper logic]
 - [Void method]
 - [Issue Report]

## Changelog

### [5.0.0] - 2024-03-09

#### Added 

- Wrapper method returning type is supported.


### [3.1.0] - 2023-12-04

#### Added 

- Reuse your Wrapper logic

### [3.0.1] - 2023-11-22

#### Added 

- CallerParamByName and CallerParamByIndex attributes

#### Updated 

- Nuget package model but backward compatible

### [2.0.1] - 2023-08-28 - ***Breaking Change*** to version 1.1.x

#### Added 

- Support C# Caller attributes

#### Deprecated

- [string methodName]



### [1.1.0] - 2023-08-10
#### Added
- [string methodName]


## Specification


### 1. InterfaceFiller attribute
```csharp
public interface ITestApi
{
    int FunA(int x, int y);
    Task<StreamContent> FunB(Barrier barrier, Random randomAccess);
}
```

The must be `partial TestApi` class, contains `testApi` backup field (`ITestApi` type).

```csharp
public partial class TestApi : ITestApi
{
    [InterfaceFiller]
    private ITestApi testApi;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
    }
}
```

`TestApi` has no implementation for interface `ITestApi` but no compiler error
because `InterfaceFiller` attribute marks to auto-generate the default implementation using the backup field `testApi`.

```csharp
// auto-generated
namespace WrapperNormal1
{
    partial class TestApi
    {
        [InterfaceFillerCodeGen.IFCodeGen]
        public int FunA(int x, int y)
        {
            var f1 = this.testApi.FunA;
            
            return f1(x, y);
        }

        [InterfaceFillerCodeGen.IFCodeGen]
        public async System.Threading.Tasks.Task<System.Net.Http.StreamContent> FunB(System.Threading.Barrier barrier, System.Random randomAccess)
        {
            var f1 = this.testApi.FunB;
            
            return await f1(barrier, randomAccess);
        }
    }

}
```
**Note**
- Class must be `partial`
- Class must have backup-field which field type is same as the interface type.
- Backup-field has `[InterfaceFiller]` attribute.
- If there is an implementation for a method, code-gen will skips. 

### 2. Wrapper attribute

'Aspect' your interface with custom behavior before and/or after execution.   

#### 2.1 Normal method

Note: Normal method is the method NOT returns `Task` or `Task<T>`

```csharp
public interface ITestApi
{
    int FunA(int x);
    string FunB(int x, string y);
}

public class ApiClient : ITestApi
{
    public int FunA(int x) => x;
    public string FunB(int x, string y) => $"{x} {y}";
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller]
    private ITestApi testApi;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
    }

    [Wrapper]
    private T Wrapper<T>(Func<T> next)
    {
        Console.WriteLine("Hello");
        var r = next();
        Console.WriteLine(r);
        Console.WriteLine("World");
        return r;
    }
}
```
Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.FunA(1)
//Output:
// Hello
// 1
// World

api.FunB(1, "SJC")
//Output:
// Hello
// 1 SJC
// World
```

**Wrapper method:**
- Having `[Wrapper]` attribute
- The wrapper's return type must be convertible to wrapped's return type (in this case, the type int is convertible to the generic type `T`)
- The wrapper must have a `Func<>` parameter

#### 2.2 `Task` method

```csharp
public interface ITestApi
{
    Task FunA(int x);
    Task FunB(int x, string y);
}

public class ApiClient : ITestApi
{
    public async Task FunA(int x)
    {
        await Task.Delay(500);
        Console.WriteLine("Task await 500ms");
    }

    public async Task FunB(int x, string y)
    {
        await Task.Delay(600);
        Console.WriteLine("Task await 600ms");
    }
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller]
    private ITestApi testApi;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
    }

    [Wrapper]
    private async Task Wrapper(Func<Task> next)
    {
        Console.WriteLine("Hello");
        await next();
        Console.WriteLine("World");
    }
}
```
Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.FunA(1)
//Output:
// Hello
// Task await 500ms
// World

api.FunB(1, "SJC")
//Output:
// Hello
// Task await 600ms
// World
```

**Wrapper for `Task` method**:
- Having `[Wrapper]` attribute
- Signature: `async Task Wrapper(Func<Task> next)`

#### 2.3 `Task<TResult>` method

```csharp
public interface ITestApi
{
    Task<int> FunA(int x);
    Task<string> FunB(int x, string y);
}

public class ApiClient : ITestApi
{
    public async Task<int> FunA(int x)
    {
        await Task.Delay(100);
        Console.WriteLine("Task await 100ms");
        return x;
    }

    public async Task<string> FunB(int x, string y)
    {
        await Task.Delay(200);
        Console.WriteLine("Task await 200ms");
        return x + y;
    }
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller]
    private ITestApi testApi;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
    }

    [Wrapper]
    private async Task<T> Wrapper<T>(Func<Task<T>> next)
    {
        Console.WriteLine("Hello");
        var r = await next();
        Console.WriteLine("World");
        return r;
    }
}
```
Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.FunA(1)
//Output:
// Hello
// Task await 100ms
// World

api.FunB(1, "SJC")
//Output:
// Hello
// Task await 200ms
// World
```

**Wrapper method:**
- Having `[Wrapper]` attribute
- Signature: `async Task<T> Wrapper<T>(Func<Task<T>> next)`
- The wrapper's return type `Task<>` must be convertible to wrapped's return type `Task<>` (in this case, the generic type `Task<T>` is convertible to `Task<int>` or `Task<string>`)
- The wrapper must have a `Func<>` parameter

## 3. Wrapper parameters

### 3.1 Single param

```csharp
public interface ITestApi
{
    int FunA(int role);
    string FunB(int role, string y);
}

public class ApiClient : ITestApi
{
    public int FunA(int role)
    {
        Console.WriteLine(role);
        return role;
    }

    public string FunB(int role, string y)
    {
        Console.WriteLine($"{role} {y}");
        return $"{role} {y}";
    }
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller]
    private ITestApi testApi;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
    }

    [Wrapper]
    private T Wrapper<T>(int role, Func<int, T> next)
    {
        Console.WriteLine("Hello");
        var r = next(role);
        Console.WriteLine("World");
        return r;
    }
}
```
Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.FunA(1)
//Output:
// Hello
// 1
// World

api.FunB(1, "SJC")
//Output:
// Hello
// 1 SJC
// World
```

**Wrapper method:**
- Having `[Wrapper]` attribute
- Signature: `T Wrapper<T>(int role, Func<int, T> next)`
    - All parameters before the last param (`Func<int, T> next`) must be match exactly within parameters of each interface methods. If not, default implementation is used. E.g. `int userRole` or `double role` will not be match.
    - Last param signature `Func<int, T> next)`.
    - Param type in last param `Func` is `int` must match with `int role`

### 3.2 Too many param

```csharp
public interface ITestApi
{
    int FunA(int role);
    string FunB(int role, string y);
}

public class ApiClient : ITestApi
{
    public int FunA(int role)
    {
        Console.WriteLine(role);
        return role;
    }

    public string FunB(int role, string y)
    {
        Console.WriteLine($"{role} {y}");
        return $"{role} {y}";
    }
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller]
    private ITestApi testApi;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
    }

    [Wrapper]
    private T Wrapper<T>(int role, string name, Func<int, string, T> next)
    {
        Console.WriteLine("Hello");
        var r = next(role, name);
        Console.WriteLine("World");
        return r;
    }
}
```
It will generate the interface implementation using **default** not using **wrapper**.
Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.FunA(1)
//Output:
// 1

api.FunB(1, "SJC")
//Output:
// 1 SJC
```

**Wrapper method:**
- `string name` param in wrapper method match NO param in all interface methods.

### 3.3 Wrapper parameters resolution

#### 3.3.1 More params - higher precedence

```csharp
public interface ITestApi
{
    int FunA(int role, string name);
}

public class ApiClient : ITestApi
{
    public int FunA(int role, string name)
    {
        Console.WriteLine($"{role}");
        return role;
    }
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller]
    private ITestApi testApi;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
    }

    [Wrapper]
    private T Wrapper1<T>(int role, Func<int, T> next)
    {
        Console.WriteLine("Wrapper1");
        var r = next(role);
        return r;
    }

    [Wrapper]
    private T Wrapper2<T>(int role, string name, Func<int, string, T> next)
    {
        Console.WriteLine("Wrapper2");
        var r = next(role, name);
        return r;
    }
}
```
It will generate the interface implementation. 
- `Wrapper2` -> `FunA`
- `Wrapper2` is higher precedence than `Wrapper1` because it covers more aspect (params) of `FunA` than `Wrapper1`
Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.FunA(1, "SJC")
//Output:
// Wrapper2
// 1
```

#### 3.3.2 Equal params - compile error

```csharp
public interface ITestApi
{
    int FunA(int role, string name, DateTime dob, decimal amount);
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller]
    private ITestApi testApi;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
    }

    [Wrapper]
    private T Wrapper1<T>(int role, string name, Func<int, string, T> next)
    {
        // If role is VIP then call manager for permission
        var r = next(role, name);
        // More logging code here...
        return r;
    }

    [Wrapper]
    private T Wrapper2<T>(int role, DateTime dob, Func<int, DateTime, T> next)
    {
        // If role is VIP then call manager for permission
        var r = next(role, dob);
        // More logging code here...
        return r;
    }
}
```
It will raises two compiler errors:

`IFI1002 Wrapper method FunA matches Wrapper1`

`IFI1002 Wrapper method FunA matches Wrapper2`

because both `Wrapper1` and `Wrapper2` cover equally 2 params of `FunA`

## 4. Wrapper return

## 4.1 Convertible types

The following concrete types are convertible to other generic type or equal to itself:
- `int` is convertible to `int` (equally)
- `int` is convertible to generic `T`
- `IEnumerable<int>` is convertible to generic `T`
- `IEnumerable<int>` is convertible to generic `IEnumerable<T>`
- `IEnumerable<int>` is convertible to generic `IEnumerable<int>` (equally)

- `IEnumerable<IDictionary<int, string>>` is convertible to generic `T`
- `IEnumerable<IDictionary<int, string>>` is convertible to generic `IEnumerable<T>`
- `IEnumerable<IDictionary<int, string>>` is convertible to generic `IEnumerable<IDictionary<TKey, TValue>>`
- `IEnumerable<IDictionary<int, string>>` is convertible to generic `IEnumerable<IDictionary<int, TValue>>`
- `IEnumerable<IDictionary<int, string>>` is convertible to generic `IEnumerable<IDictionary<TKey, string>>`

- `IEnumerable<IDictionary<int, int>>` is convertible to generic `IEnumerable<IDictionary<T, T>>`
- `IEnumerable<IDictionary<int, int>>` is convertible to generic `IEnumerable<IDictionary<TKey, TValue>>`

Note: the following types are NOT convertible

- `IEnumerable<int>` is NOT convertible to generic `IEnumerable<decimal>`
- `IEnumerable<IDictionary<int, string>>` is NOT convertible to generic `IEnumerable<IDictionary<T, T>>`

## 4.2 Wrapper return type resolution

## 4.2.1 Exactly match (Type Equal)

```csharp
public interface ITest
{
    int Func(int arg1);
}

public class ApiClient : ITest
{
    public int Func(int arg1)
    {
        Console.WriteLine(arg1);
        return arg1;
    }
}
public partial class Test : ITest
{
    [InterfaceFiller]
    private ITest test1;

    public Test(ITest test1)
    {
        this.test1 = test1;
    }

    [Wrapper]
    private int Wrapper1(Func<int> next)
    {
        Console.WriteLine("Wrapper1");
        return next();
    }

    [Wrapper]
    private T Wrapper2<T>(Func<T> next)
    {
        Console.WriteLine("Wrapper2");
        return next();
    }
}
```
It will generate implementation using `Wrapper1` because it has exactly match (equal) return type `int` with method `ITest.Func`

Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.Func(1)
//Output:
// Wrapper1
// 1
```

## 4.2.2 Less generic parameter type

```csharp
public interface ITest
{
    IDictionary<int, decimal> Func(int arg1);
}

public class ApiClient : ITest
{
    public IDictionary<int, decimal> Func(int arg1)
    {
        Console.WriteLine(arg1);
        return default;
    }
}

public partial class Test : ITest
{
    [InterfaceFiller]
    private ITest test1;

    public Test(ITest test1)
    {
        this.test1 = test1;
    }

    [Wrapper]
    private IDictionary<TKey, TValue> Wrapper1<TKey, TValue>(Func<IDictionary<TKey, TValue>> next)
    {
        Console.WriteLine("Wrapper1");
        return next();
    }

    [Wrapper]
    private IDictionary<int, TValue> Wrapper2<TValue>(Func<IDictionary<int, TValue>> next)
    {
        Console.WriteLine("Wrapper2");
        return next();
    }
}
```
It will generate implementation using `Wrapper2` because it has less generic parameter type (`TValue`) than `Wrapper1` (`TKey`, `TValue`)

Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.Func(1)
//Output:
// Wrapper2
// 1
```

## 4.2.2 Less generic parameter type (2)

```csharp
public interface ITest
{
    IDictionary<int, int> Func(int arg1);
}
public class ApiClient : ITest
{
    public IDictionary<int, int> Func(int arg1)
    {
        Console.WriteLine(arg1);
        return default;
    }
}
public partial class Test : ITest
{
    [InterfaceFiller]
    private ITest test1;

    public Test(ITest test1)
    {
        this.test1 = test1;
    }

    [Wrapper]
    private IDictionary<TKey, TValue> Wrapper1<TKey, TValue>(Func<IDictionary<TKey, TValue>> next)
    {
        Console.WriteLine("Wrapper1");
        return next();
    }

    [Wrapper]
    private IDictionary<T, T> Wrapper2<T>(Func<IDictionary<T, T>> next)
    {
        Console.WriteLine("Wrapper2");
        return next();
    }
}
```
It will generate implementation using `Wrapper2` because it has less generic parameter type (`T`) than `Wrapper1` (`TKey`, `TValue`)

Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.Func(1)
//Output:
// Wrapper2
// 1
```

## 4.2.3 More generic level

```csharp
public interface ITest
{
    IEnumerable<int> Func(int arg1);
}

public class ApiClient : ITest
{
    public IEnumerable<int> Func(int arg1)
    {
        Console.WriteLine(arg1);
        return default;
    }
}

public partial class Test : ITest
{
    [InterfaceFiller]
    private ITest test1;

    public Test(ITest test1)
    {
        this.test1 = test1;
    }

    [Wrapper]
    private T Wrapper1<T>(Func<T> next)
    {
        Console.WriteLine("Wrapper1");
        return next();
    }

    [Wrapper]
    private IEnumerable<T> Wrapper2<T>(Func<IEnumerable<T>> next)
    {
        Console.WriteLine("Wrapper2");
        return next();
    }
}
```
It will generate implementation using `Wrapper2` because it has more generic level return type `IEnumerable<T>` than `T` in `Wrapper1`

Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.Func(1)
//Output:
// Wrapper2
// 1
```
## 4.2.4 More generic level (win over less generic param)

```csharp
public interface ITest
{
    IDictionary<int, IEnumerable<decimal>> Func(int arg1);
}

public class ApiClient : ITest
{
    public IDictionary<int, IEnumerable<decimal>> Func(int arg1)
    {
        Console.WriteLine(arg1);
        return default;
    }
}

public partial class Test : ITest
{
    [InterfaceFiller]
    private ITest test1;

    public Test(ITest test1)
    {
        this.test1 = test1;
    }

    [Wrapper]
    private IDictionary<int, T> Wrapper1<T>(Func<IDictionary<int, T>> next)
    {
        Console.WriteLine("Wrapper1");
        return next();
    }

    [Wrapper]
    private IDictionary<T, IEnumerable<T1>> Wrapper2<T, T1>(Func<IDictionary<T, IEnumerable<T1>>> next)
    {
        Console.WriteLine("Wrapper2");
        return next();
    }
}
```
It will generate implementation using `Wrapper2` because it has more generic level return type `IDictionary<IEnumerable<>>` than `IDictionary<>` in `Wrapper1`

Using
```csharp
TestApi api = new TestApi(new ApiClient());

api.Func(1)
//Output:
// Wrapper2
// 1
```

## 5. Wrapper (with parameter) for `Task` method

Signature: `async Task Wrapper(int a, Func<int, Task> next)`

## 6. Wrapper (with parameter) for `Task<TResult>` method

Signature: `async Task<T> Wrapper<T>(int a, Func<int, Task<T>> next)`

## 7. C# Caller Attributes

Support C# built-in [caller attributes](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/caller-information)

Wrapper resolution note: if two wrapper methods cover equally on param then which has more caller params will win.

```csharp
public interface ITestApi
{
    int FunA(int role, string name, DateTime dob, decimal amount);
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller]
    private ITestApi testApi;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
    }

    [Wrapper]
    private T Wrapper1<T>(int role, string name, Func<int, string, T> next, [CallerMemberName] string memberName = "", [CallerLineNumber] int line = 0, [CallerArgumentExpression("next")] string exp = "", [CallerFilePath] string sourceFilePath = "")
    {
        // If role is VIP then call manager for permission
        var r = next(role, name);
        // More logging code here...
        return r;
    }
}
```
## 8. CallerParamByName and CallerParamByIndex attributes

- Apply attribute [CallerParamByName("paramName")] or [CallerParamByIndex(1,true)] to wrapper method param to match with interface method param
    - [CallerParamByName("paramName")] will match by paramName
    - [CallerParamByIndex(1,true)] will match by applying index value to interface method param list
- The wrapper method param must have default value
- If no matching then default value is used
- The type of interface param and the type of wrapper param:
    - Equals then interface param is passed to wrapper param
    - Convertible then interface param is converted and passed to wrapper param
    - Not convertible then default value is used

Note: Should NOT update/modify the value matched param. It causes side effect if it is reference object.

```csharp
public class EncrytionArth
{
    public string Pseed;
}
public class S15 : EncrytionArth
{
    public string Title;
}

public interface ITestApi
{
    int FunA(int idm, EncrytionArth strat, string name, DateTime dob, decimal amount);
    int FunB(S15 strat, string hasCode);
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller]
    private ITestApi testApi;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
    }

    [Wrapper]
    private T Wrapper1<T>(Func<T> next, [CallerParamByName("strat")] EncrytionArth arth = default)
    {
        // this will match both FunA and FunB
        Log.Info(arth.Pseed);

        var r = next();
        // More logging code here...
        return r;
    }
}
```

## 9 Wrapper resolution in summary
1. Has more parameters
2. Then return type 
3. Then more caller parameters

NOTE: All wrapper methods name should NOT be the same in the class.

## 10. Reuse your Wrapper logic

- Move your Wrapper methods to other class. 
- Decorate it's methods with [Wrapper] attribute and make them public.
- Create the instance of it in the using class
- Add the variable name to [InterfaceFiller] 

```csharp
public interface ITestApi
{
    int FunA(int idm, EncrytionArth strat, string name, DateTime dob, decimal amount);
    int FunB(S15 strat, string hasCode);
}

public class WrapperLogic
{
    [Wrapper]
    public T Wrapper1<T>(Func<T> next, [CallerParamByName("strat")] EncrytionArth arth = default)
    {
        Log.Info("WrapperLogic");

        return next();
    }
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller(nameof(wrapperLogic))]
    private ITestApi testApi;

    private WrapperLogic wrapperLogic;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
        wrapperLogic = new WrapperLogic();
    }
}
```
#### 10.1 Combine a wrapper object with class own wrapper methods

```csharp
public interface ITestApi
{
    int FunA(int idm, EncrytionArth strat, string name, DateTime dob, decimal amount);
    int FunB(S15 strat, string hasCode);
}

public class WrapperLogic
{
    [Wrapper]
    public T Wrapper1<T>(Func<T> next, [CallerParamByName("strat")] EncrytionArth arth = default)
    {
        Log.Info("WrapperLogic message");

        return next();
    }
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller(nameof(wrapperLogic))]
    private ITestApi testApi;

    private WrapperLogic wrapperLogic;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
        wrapperLogic = new WrapperLogic();
    }

    [Wrapper]
    public T Wrapper1<T>(Func<T> next, [CallerParamByName("strat")] EncrytionArth arth = default)
    {
        Log.Info("Own class begin");
        var v = next();
        return v;

    }
}
```
- The calling order: WrapperLogic object -> class own wrapper methods
- Log result is: 
    - WrapperLogic message
    - Own class wrapper

#### 10.2 Combine multiple wrapper objects with class own wrapper methods

```csharp
public interface ITestApi
{
    int FunA(int idm, EncrytionArth strat, string name, DateTime dob, decimal amount);
    int FunB(S15 strat, string hasCode);
}

public class WrapperLogging
{
    [Wrapper]
    public T Wrapper<T>(Func<T> next, [CallerParamByName("strat")] EncrytionArth arth = default)
    {
        Log.Info("WrapperLogging begin");
        var v = next();
        Log.Info("WrapperLogging end");
        return v;
    }
}

public class WrapperTiming
{
    [Wrapper]
    public T Wrapper<T>(Func<T> next, [CallerParamByName("strat")] EncrytionArth arth = default)
    {
        Log.Info("WrapperTiming begin");
        var v = next();
        Log.Info("WrapperTiming end");
        return v;
    }
}

public partial class TestApi : ITestApi
{
    [InterfaceFiller(nameof(wrapperLogging), nameof(wrapperTiming))]
    private ITestApi testApi;

    private WrapperLogging wrapperLogging;
    private WrapperTiming wrapperTiming;

    public TestApi(ITestApi testApi)
    {
        this.testApi = testApi;
        wrapperLogging = new WrapperLogging();
        wrapperTiming = new WrapperTiming();

    }

    [Wrapper]
    public T Wrapper<T>(Func<T> next, [CallerParamByName("strat")] EncrytionArth arth = default)
    {
        Log.Info("Own class wrapper");

        return next();
    }
}
```
- The calling order (right to left): WrapperTiming -> WrapperLogging -> class own wrapper methods
- Log result is: 
    - WrapperTiming begin
        - WrapperLogging begin
            - Own class wrapper
        - WrapperLogging end
    - WrapperTiming end

## Void method

Void (Unsupported) 

## Issue Report

https://github.com/ghostnguyen/InterfaceFillerIssue/issues