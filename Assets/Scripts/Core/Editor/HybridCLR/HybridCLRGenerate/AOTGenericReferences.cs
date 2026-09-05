using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"CoreModule.dll",
		"System.Core.dll",
		"Unity.InputSystem.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Core.AssetBundles.Management.GameAsset.<>c__DisplayClass10_0<object>
	// Core.AssetBundles.Management.GameAsset.<LoadAssetAsync>d__10<object>
	// Core.AssetBundles.Management.ObjectSpawner.<SpawnAsync>d__6<object>
	// Core.UI.ViewController.UIController.<Activate>d__12<object>
	// Core.UI.ViewController.UIController.<Dispose>d__30<object>
	// Core.UI.ViewController.UIController.<InActivate>d__13<object>
	// Core.UI.ViewController.UIController.<Init>d__11<object>
	// Core.UI.ViewController.UIController<object>
	// System.Action<Core.AssetBundles.Management.AssetHandle>
	// System.Action<UnityEngine.InputSystem.InputAction.CallbackContext>
	// System.Action<int,object>
	// System.Action<long>
	// System.Action<object,UnityEngine.Vector2>
	// System.Action<object,byte>
	// System.Action<object,float>
	// System.Action<object,int>
	// System.Action<object,object>
	// System.Action<object>
	// System.Collections.Generic.ArraySortHelper<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<byte,object>
	// System.Collections.Generic.Dictionary.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,Core.AssetBundles.Management.AssetHandle<object>>
	// System.Collections.Generic.Dictionary.Enumerator<object,Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<byte,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,Core.AssetBundles.Management.AssetHandle<object>>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<byte,object>
	// System.Collections.Generic.Dictionary.KeyCollection<int,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,Core.AssetBundles.Management.AssetHandle<object>>
	// System.Collections.Generic.Dictionary.KeyCollection<object,Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<byte,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,Core.AssetBundles.Management.AssetHandle<object>>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<byte,object>
	// System.Collections.Generic.Dictionary.ValueCollection<int,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,Core.AssetBundles.Management.AssetHandle<object>>
	// System.Collections.Generic.Dictionary.ValueCollection<object,Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<byte,object>
	// System.Collections.Generic.Dictionary<int,object>
	// System.Collections.Generic.Dictionary<object,Core.AssetBundles.Management.AssetHandle<object>>
	// System.Collections.Generic.Dictionary<object,Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Collections.Generic.EqualityComparer<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<int>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.HashSet.Enumerator<object>
	// System.Collections.Generic.HashSet<object>
	// System.Collections.Generic.ICollection<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<byte,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,Core.AssetBundles.Management.AssetHandle<object>>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,Core.AssetBundles.Management.AssetHandle>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IEnumerable<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<byte,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,Core.AssetBundles.Management.AssetHandle<object>>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,Core.AssetBundles.Management.AssetHandle>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<byte,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<int,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,Core.AssetBundles.Management.AssetHandle<object>>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,Core.AssetBundles.Management.AssetHandle>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<byte>
	// System.Collections.Generic.IEqualityComparer<int>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.KeyValuePair<byte,object>
	// System.Collections.Generic.KeyValuePair<int,object>
	// System.Collections.Generic.KeyValuePair<object,Core.AssetBundles.Management.AssetHandle<object>>
	// System.Collections.Generic.KeyValuePair<object,Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.ObjectComparer<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Collections.Generic.ObjectEqualityComparer<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<int>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.Generic.Queue.Enumerator<int>
	// System.Collections.Generic.Queue<int>
	// System.Collections.Generic.Stack.Enumerator<object>
	// System.Collections.Generic.Stack<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<Core.AssetBundles.Management.AssetHandle>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<Core.AssetBundles.Management.AssetHandle>
	// System.Comparison<object>
	// System.Converter<Core.AssetBundles.Management.AssetHandle,object>
	// System.Func<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Func<System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,Core.AssetBundles.Management.AssetHandle<object>>
	// System.Func<object,System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.Predicate<Core.AssetBundles.Management.AssetHandle>
	// System.Predicate<object>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Threading.Tasks.Task<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Threading.Tasks.TaskFactory<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory<object>
	// UnityEngine.InputSystem.InputBindingComposite<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>
	// UnityEngine.InputSystem.InputProcessor<UnityEngine.Vector2>
	// UnityEngine.InputSystem.Utilities.InlinedArray<object>
	// }}

	public void RefMethods()
	{
		// System.Threading.Tasks.Task<Core.AssetBundles.Management.AssetHandle<object>> Core.AssetBundles.Management.GameAsset.LoadAssetAsync<object>(string)
		// bool Core.AssetBundles.Management.ObjectSpawner.Release<object>(object,bool)
		// System.Threading.Tasks.Task<object> Core.AssetBundles.Management.ObjectSpawner.SpawnAsync<object>(string,UnityEngine.Transform,UnityEngine.Vector3,UnityEngine.Quaternion,bool)
		// System.Void Core.Pool.IPoolManager.PushObj<object>(object)
		// System.Threading.Tasks.Task<object> Core.UI.IUIManager.CreateViewAsync<object,object>(string,Core.UI.E_UILayer,UnityEngine.Vector2,UnityEngine.Quaternion)
		// object Core.UI.IUIManager.GetController<object>()
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,object>(System.Runtime.CompilerServices.TaskAwaiter&,object&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,object>(System.Runtime.CompilerServices.TaskAwaiter<object>&,object&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,object>(System.Runtime.CompilerServices.TaskAwaiter&,object&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,object>(System.Runtime.CompilerServices.TaskAwaiter<object>&,object&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>,object>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>&,object&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<object>(object&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<Core.AssetBundles.Management.AssetHandle<object>>.Start<object>(object&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<object>(object&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,object>(System.Runtime.CompilerServices.TaskAwaiter&,object&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,object>(System.Runtime.CompilerServices.TaskAwaiter<object>&,object&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<object>(object&)
		// System.Void* Unity.Collections.LowLevel.Unsafe.UnsafeUtility.AddressOf<UnityEngine.Vector2>(UnityEngine.Vector2&)
		// int Unity.Collections.LowLevel.Unsafe.UnsafeUtility.SizeOf<UnityEngine.Vector2>()
		// object UnityEngine.Component.GetComponent<object>()
		// object UnityEngine.Component.GetComponentInChildren<object>()
		// object UnityEngine.GameObject.AddComponent<object>()
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputAction.ReadValue<UnityEngine.Vector2>()
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputActionState.ApplyProcessors<UnityEngine.Vector2>(int,UnityEngine.Vector2,UnityEngine.InputSystem.InputControl<UnityEngine.Vector2>)
		// UnityEngine.Vector2 UnityEngine.InputSystem.InputActionState.ReadValue<UnityEngine.Vector2>(int,int,bool)
	}
}