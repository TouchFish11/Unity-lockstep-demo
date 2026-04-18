using System.Collections.Generic;
public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ AOT assemblies
	public static readonly IReadOnlyList<string> PatchedAOTAssemblyList = new List<string>
	{
		"CoreModule.dll",
		"UnityEngine.CoreModule.dll",
		"mscorlib.dll",
	};
	// }}

	// {{ constraint implement type
	// }} 

	// {{ AOT generic types
	// Core.AssetBundles.Management.PoolObject<object>
	// Core.UI.MVC.UIController<object,object>
	// System.Action<Core.AssetBundles.Management.PoolObject>
	// System.Action<byte>
	// System.Action<int,int>
	// System.Action<object,object>
	// System.Action<object>
	// System.Action<ulong,ulong>
	// System.Action<ulong>
	// System.Collections.Concurrent.ConcurrentDictionary.<GetEnumerator>d__35<object,byte>
	// System.Collections.Concurrent.ConcurrentDictionary.<GetEnumerator>d__35<object,object>
	// System.Collections.Concurrent.ConcurrentDictionary.DictionaryEnumerator<object,byte>
	// System.Collections.Concurrent.ConcurrentDictionary.DictionaryEnumerator<object,object>
	// System.Collections.Concurrent.ConcurrentDictionary.Node<object,byte>
	// System.Collections.Concurrent.ConcurrentDictionary.Node<object,object>
	// System.Collections.Concurrent.ConcurrentDictionary.Tables<object,byte>
	// System.Collections.Concurrent.ConcurrentDictionary.Tables<object,object>
	// System.Collections.Concurrent.ConcurrentDictionary<object,byte>
	// System.Collections.Concurrent.ConcurrentDictionary<object,object>
	// System.Collections.Generic.ArraySortHelper<Core.AssetBundles.Management.PoolObject>
	// System.Collections.Generic.ArraySortHelper<byte>
	// System.Collections.Generic.ArraySortHelper<object>
	// System.Collections.Generic.Comparer<Core.AssetBundles.Management.PoolObject>
	// System.Collections.Generic.Comparer<byte>
	// System.Collections.Generic.Comparer<object>
	// System.Collections.Generic.Dictionary.Enumerator<byte,object>
	// System.Collections.Generic.Dictionary.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<byte,object>
	// System.Collections.Generic.Dictionary.KeyCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.KeyCollection<byte,object>
	// System.Collections.Generic.Dictionary.KeyCollection<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<byte,object>
	// System.Collections.Generic.Dictionary.ValueCollection.Enumerator<object,object>
	// System.Collections.Generic.Dictionary.ValueCollection<byte,object>
	// System.Collections.Generic.Dictionary.ValueCollection<object,object>
	// System.Collections.Generic.Dictionary<byte,object>
	// System.Collections.Generic.Dictionary<object,object>
	// System.Collections.Generic.EqualityComparer<byte>
	// System.Collections.Generic.EqualityComparer<object>
	// System.Collections.Generic.ICollection<Core.AssetBundles.Management.PoolObject>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<byte,object>>
	// System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.ICollection<byte>
	// System.Collections.Generic.ICollection<object>
	// System.Collections.Generic.IComparer<Core.AssetBundles.Management.PoolObject>
	// System.Collections.Generic.IComparer<byte>
	// System.Collections.Generic.IComparer<object>
	// System.Collections.Generic.IDictionary<object,byte>
	// System.Collections.Generic.IDictionary<object,object>
	// System.Collections.Generic.IEnumerable<Core.AssetBundles.Management.PoolObject>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<byte,object>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerable<byte>
	// System.Collections.Generic.IEnumerable<object>
	// System.Collections.Generic.IEnumerator<Core.AssetBundles.Management.PoolObject>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<byte,object>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,byte>>
	// System.Collections.Generic.IEnumerator<System.Collections.Generic.KeyValuePair<object,object>>
	// System.Collections.Generic.IEnumerator<byte>
	// System.Collections.Generic.IEnumerator<object>
	// System.Collections.Generic.IEqualityComparer<byte>
	// System.Collections.Generic.IEqualityComparer<object>
	// System.Collections.Generic.IList<Core.AssetBundles.Management.PoolObject>
	// System.Collections.Generic.IList<byte>
	// System.Collections.Generic.IList<object>
	// System.Collections.Generic.IReadOnlyDictionary<object,object>
	// System.Collections.Generic.KeyValuePair<byte,object>
	// System.Collections.Generic.KeyValuePair<object,byte>
	// System.Collections.Generic.KeyValuePair<object,object>
	// System.Collections.Generic.List.Enumerator<Core.AssetBundles.Management.PoolObject>
	// System.Collections.Generic.List.Enumerator<byte>
	// System.Collections.Generic.List.Enumerator<object>
	// System.Collections.Generic.List<Core.AssetBundles.Management.PoolObject>
	// System.Collections.Generic.List<byte>
	// System.Collections.Generic.List<object>
	// System.Collections.Generic.LowLevelList<object>
	// System.Collections.Generic.LowLevelListWithIList.Enumerator<object>
	// System.Collections.Generic.LowLevelListWithIList<object>
	// System.Collections.Generic.ObjectComparer<Core.AssetBundles.Management.PoolObject>
	// System.Collections.Generic.ObjectComparer<byte>
	// System.Collections.Generic.ObjectComparer<object>
	// System.Collections.Generic.ObjectEqualityComparer<byte>
	// System.Collections.Generic.ObjectEqualityComparer<object>
	// System.Collections.ObjectModel.ReadOnlyCollection<Core.AssetBundles.Management.PoolObject>
	// System.Collections.ObjectModel.ReadOnlyCollection<byte>
	// System.Collections.ObjectModel.ReadOnlyCollection<object>
	// System.Comparison<Core.AssetBundles.Management.PoolObject>
	// System.Comparison<byte>
	// System.Comparison<object>
	// System.Func<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Func<Core.AssetBundles.Management.PoolObject<object>>
	// System.Func<System.Threading.Tasks.VoidTaskResult>
	// System.Func<byte,object>
	// System.Func<object,Core.AssetBundles.Management.AssetHandle<object>>
	// System.Func<object,Core.AssetBundles.Management.PoolObject<object>>
	// System.Func<object,System.Threading.Tasks.VoidTaskResult>
	// System.Func<object,byte>
	// System.Func<object,object,object>
	// System.Func<object,object>
	// System.Func<object>
	// System.Predicate<Core.AssetBundles.Management.PoolObject>
	// System.Predicate<byte>
	// System.Predicate<object>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<Core.AssetBundles.Management.PoolObject<object>>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable.ConfiguredTaskAwaiter<object>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<Core.AssetBundles.Management.PoolObject<object>>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.ConfiguredTaskAwaitable<object>
	// System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>
	// System.Runtime.CompilerServices.TaskAwaiter<System.Threading.Tasks.VoidTaskResult>
	// System.Runtime.CompilerServices.TaskAwaiter<object>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<Core.AssetBundles.Management.PoolObject<object>>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.ContinuationTaskFromResultTask<object>
	// System.Threading.Tasks.Task.WhenAllPromise<Core.AssetBundles.Management.PoolObject<object>>
	// System.Threading.Tasks.Task.WhenAllPromise<object>
	// System.Threading.Tasks.Task<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Threading.Tasks.Task<Core.AssetBundles.Management.PoolObject<object>>
	// System.Threading.Tasks.Task<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.Task<object>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<Core.AssetBundles.Management.PoolObject<object>>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory.<>c__DisplayClass35_0<object>
	// System.Threading.Tasks.TaskFactory<Core.AssetBundles.Management.AssetHandle<object>>
	// System.Threading.Tasks.TaskFactory<Core.AssetBundles.Management.PoolObject<object>>
	// System.Threading.Tasks.TaskFactory<System.Threading.Tasks.VoidTaskResult>
	// System.Threading.Tasks.TaskFactory<object>
	// UnityEngine.Events.UnityAction<object,byte>
	// UnityEngine.Events.UnityAction<object,float>
	// UnityEngine.Events.UnityAction<object,object>
	// UnityEngine.Events.UnityAction<object>
	// }}

	public void RefMethods()
	{
		// System.Threading.Tasks.Task<Core.AssetBundles.Management.PoolObject<object>> Core.AssetBundles.Management.ObjectSpawner.SpawnAsync<object>(string,UnityEngine.Transform,UnityEngine.Vector3,UnityEngine.Quaternion,bool)
		// Core.AssetBundles.Management.PoolObject<object> Core.AssetBundles.Management.PoolObject.Convert<object>()
		// object Core.Serialize.Json.IJsonManager.FromJson<object>(string,Core.Serialize.Json.E_JsonType,Newtonsoft.Json.JsonSerializerSettings)
		// System.Threading.Tasks.Task<object> Core.Serialize.Json.IJsonManager.FromJsonAsync<object>(string,Core.Serialize.Json.E_JsonType,Newtonsoft.Json.JsonSerializerSettings)
		// System.Threading.Tasks.Task<object> Core.UI.IUIManager.CreateViewAsync<object,object,object>(string,Core.UI.E_UILayer,UnityEngine.Vector2,UnityEngine.Quaternion)
		// Core.AssetBundles.Management.PoolObject<object>[] System.Array.Empty<Core.AssetBundles.Management.PoolObject<object>>()
		// Core.DI.ParameterArg[] System.Array.Empty<Core.DI.ParameterArg>()
		// object[] System.Array.Empty<object>()
		// object System.Collections.Generic.CollectionExtensions.GetValueOrDefault<object,object>(System.Collections.Generic.IReadOnlyDictionary<object,object>,object)
		// object System.Collections.Generic.CollectionExtensions.GetValueOrDefault<object,object>(System.Collections.Generic.IReadOnlyDictionary<object,object>,object,object)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Game.Inventory.UI.InventoryController.<OnShow>d__3>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Game.Inventory.UI.InventoryController.<OnShow>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Update.HotUpdateEntry.<EnterGame>d__5>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Update.HotUpdateEntry.<EnterGame>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Update.HotUpdateEntry.<LoadPlayerDataAsync>d__7>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Update.HotUpdateEntry.<LoadPlayerDataAsync>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Update.HotUpdateEntry.<Run>d__3>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Update.HotUpdateEntry.<Run>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>,HotUpdate.Game.Inventory.InventoryManager.<LoadItemConfig>d__8>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>&,HotUpdate.Game.Inventory.InventoryManager.<LoadItemConfig>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>,HotUpdate.Update.HotUpdateEntry.<InitInputSystemAsync>d__6>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>&,HotUpdate.Update.HotUpdateEntry.<InitInputSystemAsync>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>,HotUpdate.Game.Inventory.UI.InventoryController.<InitTypeOpt>d__5>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>&,HotUpdate.Game.Inventory.UI.InventoryController.<InitTypeOpt>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>,HotUpdate.Game.Inventory.UI.InventoryController.<UpdateDetail>d__7>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>&,HotUpdate.Game.Inventory.UI.InventoryController.<UpdateDetail>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdate.Game.Data.GameDataManager.<LoadDataAsync>d__8>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdate.Game.Data.GameDataManager.<LoadDataAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdate.Game.Inventory.UI.InventoryController.<UpdateItemsByType>d__6>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdate.Game.Inventory.UI.InventoryController.<UpdateItemsByType>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdate.Update.HotUpdateEntry.<Run>d__3>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdate.Update.HotUpdateEntry.<Run>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Game.Inventory.UI.InventoryController.<OnShow>d__3>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Game.Inventory.UI.InventoryController.<OnShow>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Update.HotUpdateEntry.<EnterGame>d__5>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Update.HotUpdateEntry.<EnterGame>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Update.HotUpdateEntry.<LoadPlayerDataAsync>d__7>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Update.HotUpdateEntry.<LoadPlayerDataAsync>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Update.HotUpdateEntry.<Run>d__3>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Update.HotUpdateEntry.<Run>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>,HotUpdate.Game.Inventory.InventoryManager.<LoadItemConfig>d__8>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>&,HotUpdate.Game.Inventory.InventoryManager.<LoadItemConfig>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>,HotUpdate.Update.HotUpdateEntry.<InitInputSystemAsync>d__6>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>&,HotUpdate.Update.HotUpdateEntry.<InitInputSystemAsync>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>,HotUpdate.Game.Inventory.UI.InventoryController.<InitTypeOpt>d__5>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>&,HotUpdate.Game.Inventory.UI.InventoryController.<InitTypeOpt>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>,HotUpdate.Game.Inventory.UI.InventoryController.<UpdateDetail>d__7>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>&,HotUpdate.Game.Inventory.UI.InventoryController.<UpdateDetail>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdate.Game.Data.GameDataManager.<LoadDataAsync>d__8>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdate.Game.Data.GameDataManager.<LoadDataAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdate.Game.Inventory.UI.InventoryController.<UpdateItemsByType>d__6>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdate.Game.Inventory.UI.InventoryController.<UpdateItemsByType>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<System.Threading.Tasks.VoidTaskResult>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdate.Update.HotUpdateEntry.<Run>d__3>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdate.Update.HotUpdateEntry.<Run>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>,HotUpdate.Game.Inventory.InventoryManager.<CreateItemDTO>d__10>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.AssetHandle<object>>&,HotUpdate.Game.Inventory.InventoryManager.<CreateItemDTO>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdate.Game.Inventory.UI.InventoryController.<CreateItemDTOs>d__9>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdate.Game.Inventory.UI.InventoryController.<CreateItemDTOs>d__9&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdate.Game.Data.GameDataManager.<LoadDataAsync>d__8>(HotUpdate.Game.Data.GameDataManager.<LoadDataAsync>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdate.Game.Inventory.InventoryManager.<LoadItemConfig>d__8>(HotUpdate.Game.Inventory.InventoryManager.<LoadItemConfig>d__8&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdate.Game.Inventory.UI.InventoryController.<InitTypeOpt>d__5>(HotUpdate.Game.Inventory.UI.InventoryController.<InitTypeOpt>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdate.Game.Inventory.UI.InventoryController.<OnShow>d__3>(HotUpdate.Game.Inventory.UI.InventoryController.<OnShow>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdate.Game.Inventory.UI.InventoryController.<UpdateDetail>d__7>(HotUpdate.Game.Inventory.UI.InventoryController.<UpdateDetail>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdate.Game.Inventory.UI.InventoryController.<UpdateItemsByType>d__6>(HotUpdate.Game.Inventory.UI.InventoryController.<UpdateItemsByType>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdate.Update.HotUpdateEntry.<EnterGame>d__5>(HotUpdate.Update.HotUpdateEntry.<EnterGame>d__5&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdate.Update.HotUpdateEntry.<InitInputSystemAsync>d__6>(HotUpdate.Update.HotUpdateEntry.<InitInputSystemAsync>d__6&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdate.Update.HotUpdateEntry.<LoadPlayerDataAsync>d__7>(HotUpdate.Update.HotUpdateEntry.<LoadPlayerDataAsync>d__7&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start<HotUpdate.Update.HotUpdateEntry.<Run>d__3>(HotUpdate.Update.HotUpdateEntry.<Run>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<Core.AssetBundles.Management.AssetHandle<object>>.Start<Core.AssetBundles.Management.GameAsset.<LoadAssetAsync>d__7<object>>(Core.AssetBundles.Management.GameAsset.<LoadAssetAsync>d__7<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<Core.AssetBundles.Management.PoolObject<object>>.Start<Core.AssetBundles.Management.ObjectSpawner.<SpawnAsync>d__2<object>>(Core.AssetBundles.Management.ObjectSpawner.<SpawnAsync>d__2<object>&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<HotUpdate.Game.Inventory.InventoryManager.<CreateItemDTO>d__10>(HotUpdate.Game.Inventory.InventoryManager.<CreateItemDTO>d__10&)
		// System.Void System.Runtime.CompilerServices.AsyncTaskMethodBuilder<object>.Start<HotUpdate.Game.Inventory.UI.InventoryController.<CreateItemDTOs>d__9>(HotUpdate.Game.Inventory.UI.InventoryController.<CreateItemDTOs>d__9&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.UI.Test.ABTest.<Start>d__0>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.UI.Test.ABTest.<Start>d__0&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Update.BeginController.<ButtonOnClick>d__19>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Update.BeginController.<ButtonOnClick>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Update.BeginController.<EnterMain>d__18>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Update.BeginController.<EnterMain>d__18&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter,HotUpdate.Update.HotUpdateEntry.<Start>d__2>(System.Runtime.CompilerServices.TaskAwaiter&,HotUpdate.Update.HotUpdateEntry.<Start>d__2&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>,HotUpdate.UI.Test.ABTest.<Start>d__0>(System.Runtime.CompilerServices.TaskAwaiter<Core.AssetBundles.Management.PoolObject<object>>&,HotUpdate.UI.Test.ABTest.<Start>d__0&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdate.Game.Main.UI.MainController.<ButtonOnClick>d__3>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdate.Game.Main.UI.MainController.<ButtonOnClick>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.AwaitUnsafeOnCompleted<System.Runtime.CompilerServices.TaskAwaiter<object>,HotUpdate.Update.BeginController.<OnUpdateOver>d__16>(System.Runtime.CompilerServices.TaskAwaiter<object>&,HotUpdate.Update.BeginController.<OnUpdateOver>d__16&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotUpdate.Game.Main.UI.MainController.<ButtonOnClick>d__3>(HotUpdate.Game.Main.UI.MainController.<ButtonOnClick>d__3&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotUpdate.UI.Test.ABTest.<Start>d__0>(HotUpdate.UI.Test.ABTest.<Start>d__0&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotUpdate.Update.BeginController.<ButtonOnClick>d__19>(HotUpdate.Update.BeginController.<ButtonOnClick>d__19&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotUpdate.Update.BeginController.<EnterMain>d__18>(HotUpdate.Update.BeginController.<EnterMain>d__18&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotUpdate.Update.BeginController.<OnUpdateOver>d__16>(HotUpdate.Update.BeginController.<OnUpdateOver>d__16&)
		// System.Void System.Runtime.CompilerServices.AsyncVoidMethodBuilder.Start<HotUpdate.Update.HotUpdateEntry.<Start>d__2>(HotUpdate.Update.HotUpdateEntry.<Start>d__2&)
		// object& System.Runtime.CompilerServices.Unsafe.As<object,object>(object&)
		// System.Void* System.Runtime.CompilerServices.Unsafe.AsPointer<object>(object&)
		// System.Threading.Tasks.Task<Core.AssetBundles.Management.PoolObject<object>[]> System.Threading.Tasks.Task.InternalWhenAll<Core.AssetBundles.Management.PoolObject<object>>(System.Threading.Tasks.Task<Core.AssetBundles.Management.PoolObject<object>>[])
		// System.Threading.Tasks.Task<object[]> System.Threading.Tasks.Task.InternalWhenAll<object>(System.Threading.Tasks.Task<object>[])
		// System.Threading.Tasks.Task<Core.AssetBundles.Management.PoolObject<object>[]> System.Threading.Tasks.Task.WhenAll<Core.AssetBundles.Management.PoolObject<object>>(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<Core.AssetBundles.Management.PoolObject<object>>>)
		// System.Threading.Tasks.Task<Core.AssetBundles.Management.PoolObject<object>[]> System.Threading.Tasks.Task.WhenAll<Core.AssetBundles.Management.PoolObject<object>>(System.Threading.Tasks.Task<Core.AssetBundles.Management.PoolObject<object>>[])
		// System.Threading.Tasks.Task<object[]> System.Threading.Tasks.Task.WhenAll<object>(System.Collections.Generic.IEnumerable<System.Threading.Tasks.Task<object>>)
		// System.Threading.Tasks.Task<object[]> System.Threading.Tasks.Task.WhenAll<object>(System.Threading.Tasks.Task<object>[])
		// object UnityEngine.Component.GetComponent<object>()
	}
}