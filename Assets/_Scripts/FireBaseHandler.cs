using Firebase;
using Firebase.Storage;
using System;
using System.Collections;
using UnityEngine;

public class FireBaseHandler : MonoBehaviour
{

	public string storageBucketAddress;
	public string textureFIleName;
	public GameObject cube;
	
	private DependencyStatus dependencyStatus = DependencyStatus.UnavailableOther;
	private FirebaseStorage storage;
	private Firebase.LogLevel logLevel = Firebase.LogLevel.Info;

	// Use this for initialization
	void Start ()
	{
		FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
			dependencyStatus = task.Result;
			if (dependencyStatus == DependencyStatus.Available) {
				InitializeFirebase();
				StartCoroutine(DownloadTexture());
				//DownloadTexture();
			} else {
				Debug.LogError(
					"Could not resolve all Firebase dependencies: " + dependencyStatus);
			}
		});
	}
	
	private void InitializeFirebase() 
	{
		var appBucket = FirebaseApp.DefaultInstance.Options.StorageBucket;
		storage = FirebaseStorage.DefaultInstance;
		if (!String.IsNullOrEmpty(appBucket)) {
			storageBucketAddress = String.Format("gs://{0}/", appBucket);
		}
		storage.LogLevel = logLevel;
	}

	// Retrieve a storage reference from the user specified path.
	private StorageReference GetStorageReference()
	{
		return storage.GetReferenceFromUrl(storageBucketAddress);
	}

	IEnumerator DownloadTexture()
	{
		Firebase.Storage.StorageReference texture_ref = GetStorageReference().Child(textureFIleName);
		// Fetch the download URL
		var task = texture_ref.GetDownloadUrlAsync();
		yield return new WaitUntil(() => task.IsCompleted);
		
		using (WWW www = CachedDownloader.GetCachedWWW(task.Result.ToString()))
		{
			yield return www;
			Renderer renderer = cube.GetComponent<Renderer>();
			renderer.material.mainTexture = www.texture;
		}
	}

}
