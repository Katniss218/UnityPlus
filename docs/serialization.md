
# Serialization v4 Design Document

## Core Requirements

### Remaining TODO:
- dependency (reference and possibly more) handling and load reordering
- version migration and data transformation
- streaming serialization for large objects
- memory-efficient collection handling

### New v4 Requirements:
- **Unified Serializer/Deserializer**: Single objects handling both serialization and deserialization in one place
- **Simplified Mappings**: Mappings only provide interface to access members, not orchestrate serialization
- **Comprehensive Error Handling**: Robust error collection, reporting, and recovery mechanisms
- **Eliminate Recursive Memberwise**: Remove recursive nature of memberwise mappings for better performance
- **Built-in Logic**: More serialization logic built into core system instead of relying entirely on mappings
- **Automatic Member Discovery**: Configurable automatic member discovery via extension methods on memberwise mapping
- **Time-budgeted Serialization**: Retain and improve async/time-budgeted serialization capabilities
- **Unlimited Reference Depth**: Support unlimited reference graph depth without stack overflow

### Retained v3 Requirements:
- **Asset Handling**: Unity asset references and management
- **Custom Mappings**: Extensible mapping system for custom types
- **Reference Handling**: Circular references and reference remapping
- **Polymorphism**: Runtime type name handling and inheritance
- **Multi-context**: Context-based mapping selection

## Architecture Overview

- some errors are unrecoverable (e.g. a missing type), vs some errors are recoverable (e.g. referenced object not created yet).
- mapping members are created once and cached. getters/setters are compiled lambda expressions.
- API remains similar to v3

Build a 'failed member' graph from retryable reference failures and reorder the loads so that objects without dependencies load first?

Sometimes a missing member might need to trigger an unrecoverable failure (invalid state).

### Core Components

#### 1. Unified Serialization Engine
```csharp
public class SerializationEngine
{
    public SerializationResult Serialize<T>(T obj, SerializationContext context);
    public SerializationResult Deserialize<T>(SerializedData data, SerializationContext context);
    public SerializationResult Populate<T>(T obj, SerializedData data, SerializationContext context);
}
```

#### 2. Serialization Unit - stores the serialization work.
```csharp
public class SerializationUnit
{
    public IReferenceMap ReferenceMap { get; set; }
    public IErrorCollector ErrorCollector { get; set; }
    public SerializationOptions Options { get; set; }
    public TimeBudget TimeBudget { get; set; }
    public IDependencyResolver DependencyResolver { get; set; }
    public ISerializationCache InProgressMembers { get; set; }
}
```

#### 3. Enhanced Mapping System
```csharp
public abstract class SerializationMapping
{
    // Simplified interface - no orchestration logic
    public abstract SerializedData SaveMember<T>(T value, SerializationContext context);
    public abstract T LoadMember<T>(SerializedData data, SerializationContext context);
    public abstract bool CanHandle(Type type);
    public virtual bool IsRecoverableError(Exception ex) => true;
}

#### 4. Dependency Resolution System
```csharp
public interface IDependencyResolver
{
    void AddDependency(object dependent, object dependency);
    List<object> GetResolutionOrder(IEnumerable<object> objects);
    bool HasCircularDependency(object obj, HashSet<object> visited = null);
    void BreakCircularDependency(object obj, object dependency);
}

public class TopologicalDependencyResolver : IDependencyResolver
{
    private readonly Dictionary<object, HashSet<object>> _dependencies = new();
    private readonly Dictionary<object, int> _incomingCount = new();
    
    public List<object> GetResolutionOrder(IEnumerable<object> objects)
    {
        // Topological sort with cycle detection
        // Returns optimal processing order for deserialization
    }
}
```

#### 5. Error Handling System
```csharp
public interface IErrorCollector
{
    void AddError(SerializationError error);
    void AddWarning(SerializationWarning warning);
    bool HasErrors { get; }
    bool HasWarnings { get; }
    IEnumerable<SerializationError> GetErrors();
    IEnumerable<SerializationWarning> GetWarnings();
}

public class SerializationError
{
    public string Message { get; set; }
    public string Path { get; set; }
    public Exception Exception { get; set; }
    public ErrorSeverity Severity { get; set; }
    public ErrorRecoveryStrategy RecoveryStrategy { get; set; }
}
```

#### 6. Time-budgeted Processing
```csharp
public class TimeBudget
{
    public TimeSpan MaxTimePerFrame { get; set; }
    public TimeSpan TotalTimeLimit { get; set; }
    public bool ShouldPause { get; }
    public void RecordWork(TimeSpan duration);
}

public class SerializationResult
{
    public bool IsComplete { get; set; }
    public bool HasErrors { get; set; }
    public bool WasPaused { get; set; }
    public TimeSpan ElapsedTime { get; set; }
    public int ObjectsProcessed { get; set; }
    public IErrorCollector Errors { get; set; }
}
```

## Key Architectural Changes

### 1. Unified Processing Pipeline
- Single `SerializationEngine` handles both save and load operations
- Context-driven processing with shared state management

### 2. Simplified Mapping Architecture
- Mappings become pure data transformers without orchestration logic. Core engine handles iteration, retry logic, and reference resolution

### 3. Automatic Member Discovery
- Automatic reflection-based member discovery with configurable filtering
- Uses the same memberwise mapping, but with a method that finds and creates members for a type.
- Selection rules via attributes on members

### 4. Comprehensive Error Handling
- Centralized error collection and reporting
- Error recovery strategies (skip, retry, abort, substitute)
- Detailed error context including object paths and member names

### 5. Improved Reference Handling
- Iterative reference resolution without recursion
- Support for unlimited reference graph depth
- Enhanced circular reference detection and handling

### 6. Performance Optimizations
- Batch processing for collections
- Lazy evaluation of complex mappings
- Caching of discovered members and mappings
- Object pooling for SerializedData instances
- Streaming serialization for large objects
- Memory-efficient collection handling

### 7. Version Migration and Data Transformation
- Automatic data migration between versions
- Backward compatibility with v3 data
- Forward compatibility for future versions
- Data transformation pipelines

### 8. Enhanced Error Classification
- Recoverable vs unrecoverable error distinction
- Error severity levels (Info, Warning, Error, Fatal)
- Context-aware error recovery strategies
- Error aggregation and reporting

## Detailed Technical Specifications

### Reference Resolution Algorithm
```csharp
public class IterativeReferenceResolver
{
    private Queue<ReferenceResolutionTask> _pendingTasks;
    private Dictionary<object, SerializedData> _resolvedObjects;
    private HashSet<object> _currentlyResolving;
    
    public void ResolveReferences(SerializationContext context)
    {
        while (_pendingTasks.Count > 0 && !context.TimeBudget.ShouldPause)
        {
            var task = _pendingTasks.Dequeue();
            ProcessReferenceTask(task, context);
        }
    }
    
    private void ProcessReferenceTask(ReferenceResolutionTask task, SerializationContext context)
    {
        // Iterative processing without recursion
        // Handles circular references through state tracking
    }
}
```

### Member Discovery Configuration
```csharp
// Attribute-based member control
[SerializationMember(Include = true, Priority = 1)]
public string Name { get; set; }

[SerializationMember(Include = false, Reason = "Internal state")]
private object _internalState;

[SerializationMember(Include = true, CustomSerializer = typeof(CustomSerializer))]
public ComplexType ComplexProperty { get; set; }

// Discovery options
public class MemberDiscoveryOptions
{
    public BindingFlags BindingFlags { get; set; } = BindingFlags.Public | BindingFlags.Instance;
    public bool IncludeFields { get; set; } = true;
    public bool IncludeProperties { get; set; } = true;
    public bool IncludePrivateMembers { get; set; } = false;
    public string[] ExcludeMembers { get; set; } = Array.Empty<string>();
    public Type[] ExcludeTypes { get; set; } = Array.Empty<Type>();
    public IMemberFilter CustomFilter { get; set; }
}
```

### Error Recovery Strategies
```csharp
public enum ErrorRecoveryStrategy
{
    Skip,           // Skip the problematic member/object
    Retry,          // Retry the operation (with backoff)
    Abort,          // Abort the entire serialization
    Substitute,     // Use a default/substitute value
    Ignore          // Log but continue processing
}

public class ErrorRecoveryHandler
{
    public SerializationResult HandleError(SerializationError error, SerializationContext context)
    {
        switch (error.RecoveryStrategy)
        {
            case ErrorRecoveryStrategy.Skip:
                return SkipAndContinue(error, context);
            case ErrorRecoveryStrategy.Retry:
                return RetryWithBackoff(error, context);
            case ErrorRecoveryStrategy.Substitute:
                return UseSubstituteValue(error, context);
            default:
                return AbortSerialization(error, context);
        }
    }
}
```


### Memory-Efficient Collections
```csharp
public class MemoryEfficientCollectionMapping<T> : SerializationMapping where T : ICollection
{
    private readonly int _batchSize;
    private readonly bool _streamingMode;
    
    public MemoryEfficientCollectionMapping(int batchSize = 1000, bool streamingMode = true)
    {
        _batchSize = batchSize;
        _streamingMode = streamingMode;
    }
    
    public override SerializedData SaveMember<T>(T value, SerializationContext context)
    {
        if (_streamingMode && value.Count > _batchSize)
        {
            return SaveAsStream(value, context);
        }
        return SaveAsBatch(value, context);
    }
    
    private SerializedData SaveAsStream(T collection, SerializationContext context)
    {
        // Stream large collections to avoid memory spikes
        var streamData = new SerializedArray();
        streamData.Add(new SerializedPrimitive("__streaming__"));
        streamData.Add(new SerializedPrimitive(collection.Count));
        // Add streaming metadata
        return streamData;
    }
}
```

## Key Improvements Summary

### 1. **Unified Architecture**
- Single `SerializationEngine` replaces separate saver/loader classes
- Simplified `SerializationContext` with all necessary state
- Clean separation between orchestration and data transformation

### 2. **Intelligent Dependency Resolution**
- Topological sorting for optimal processing order
- Automatic cycle detection and breaking strategies
- Reduces retry cycles and improves performance

### 3. **Enhanced Error Handling**
- Classified errors with severity levels and suggested fixes
- Recoverable vs unrecoverable error distinction
- Context-aware recovery strategies

### 4. **Performance Optimizations**
- Compiled getters/setters for member access
- Object pooling for SerializedData instances
- Streaming serialization for large objects
- Memory-efficient collection handling

### 5. **Automatic Member Discovery**
- Attribute-based member control
- Configurable discovery options
- Cached reflection with compiled accessors

### 6. **Version Migration**
- Automatic data migration between versions
- Backward compatibility with v3
- Forward compatibility for future versions

### 7. **Production-Ready Features**
- Time-budgeted processing with pause/resume
- Comprehensive error reporting
- Memory management for large datasets
- Streaming support for massive objects

## Migration Strategy

### Phase 1: Core Engine (Weeks 1-2)
- Implement `SerializationEngine` and `SerializationContext`
- Create simplified mapping interface
- Add basic error handling

### Phase 2: Advanced Features (Weeks 3-4)
- Implement dependency resolution
- Add automatic member discovery
- Create version migration system

### Phase 3: Performance & Polish (Weeks 5-6)
- Add streaming serialization
- Implement object pooling
- Create comprehensive error classification

### Phase 4: Migration & Testing (Weeks 7-8)
- Build v3 compatibility layer
- Create migration tools
- Extensive testing and validation

## API Usage Examples

### Basic Serialization
```csharp
// v4 API - unified engine with better error handling
var engine = new SerializationEngine();
var context = new SerializationContext
{
    ContextId = ObjectContext.Default,
    ErrorCollector = new ErrorCollector(),
    TimeBudget = new TimeBudget(TimeSpan.FromMilliseconds(16)), // 60 FPS
    DependencyResolver = new TopologicalDependencyResolver(),
    Cache = new SerializationCache()
};

var result = engine.Serialize(myObject, context);
if (result.IsComplete)
{
    var serializedData = result.Data;
    // Process serialized data
}
else if (result.WasPaused)
{
    // Continue serialization in next frame
    var continueResult = engine.ContinueSerialization(context);
}
```

### Automatic Member Discovery
```csharp
// Configure automatic member discovery with attributes
public class MyClass
{
    [SerializationMember(Include = true, Priority = 1)]
    public string Name { get; set; }
    
    [SerializationMember(Include = false, Reason = "Internal state")]
    private object _internalState;
    
    [SerializationMember(Include = true, CustomSerializer = typeof(CustomSerializer))]
    public ComplexType ComplexProperty { get; set; }
}

// Auto-discovering mapping
var memberwiseMapping = new MemberwiseMapping<MyClass>()
    .AutoMap(... optional_params);
);
```

### Dependency Resolution
```csharp
// Automatic dependency resolution with cycle detection
var resolver = new TopologicalDependencyResolver();
var objects = GetComplexObjectGraph();

// Add dependencies as they're discovered
foreach (var obj in objects)
{
    var dependencies = FindDependencies(obj);
    foreach (var dep in dependencies)
    {
        resolver.AddDependency(obj, dep);
    }
}

// Get optimal processing order
var resolutionOrder = resolver.GetResolutionOrder(objects);
if (resolver.HasCircularDependency(someObject))
{
    // Handle circular dependencies
    resolver.BreakCircularDependency(someObject, problematicDependency);
}

// Process objects in optimal order
foreach (var obj in resolutionOrder)
{
    var result = engine.Serialize(obj, context);
}
```

### Error Handling
```csharp
var errorCollector = new ErrorCollector();
var context = new SerializationContext
{
    ErrorCollector = errorCollector,
    Options = new SerializationOptions
    {
        ErrorRecoveryStrategy = ErrorRecoveryStrategy.Skip,
        ContinueOnError = true
    }
};

var result = engine.Serialize(complexObject, context);

if (errorCollector.HasErrors)
{
    foreach (var error in errorCollector.GetErrors())
    {
        Debug.LogError($"Serialization error at {error.Path}: {error.Message}");
    }
}
```
