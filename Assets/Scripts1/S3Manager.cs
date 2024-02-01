using UnityEngine; 
//using Amazon.S3; 
//using Amazon.S3.Model; 
//using Amazon.Runtime; 
using System.IO; 
using System.Collections.Generic; 
//using Amazon.CognitoIdentity; 
//using Amazon; 
using System;


public class S3Manager : MonoBehaviour
{
    #region VARIABLES 

    public static S3Manager Instance { get; set; }

    [Header("AWS Setup")]
    [SerializeField] private string identityPoolId;
    //[SerializeField] private string cognitoIdentityRegion = RegionEndpoint.USEast1.SystemName;
    //[SerializeField] private string s3Region = RegionEndpoint.USEast1.SystemName;

    // Variables privates 
    //  public Action<GetObjectResponse, string> OnResultGetObject;
    //  private IAmazonS3 s3Client;
    //  private AWSCredentials credentials;

    // Propertys 
    //  private RegionEndpoint CognitoIdentityRegion
    //{
    //    get { return RegionEndpoint.GetBySystemName(cognitoIdentityRegion); }
    //}
    //private RegionEndpoint S3Region
    //{
    //    get { return RegionEndpoint.GetBySystemName(s3Region); }
    //}
    //private AWSCredentials Credentials
    //{
    //    get
    //    {
    //        if (credentials == null)
    //            credentials = new CognitoAWSCredentials(identityPoolId, CognitoIdentityRegion);
    //        return credentials;
    //    }
    //}
    //private IAmazonS3 Client
    //{
    //    get
    //    {
    //        if (s3Client == null)
    //        {
    //            s3Client = new AmazonS3Client(Credentials, S3Region);
    //        }
    //        //test comment 
    //        return s3Client;
    //    }
    //}

    //#endregion

    //#region METHODS MONOBEHAVIOUR 

    //private void Awake()
    //{
    //    Instance = this;
    //}

    //void Start()
    //{
    //    if (testConnection.networkConnected)
    //    {
    //        UnityInitializer.AttachToGameObject(this.gameObject);
    //        AWSConfigs.HttpClient = AWSConfigs.HttpClientOption.UnityWebRequest;
    //    }
    //}


    //#endregion

    #region METHODS AWS SDK S3 

    /// <summary> 
    /// Get Objects from S3 Bucket 
    /// </summary> 
    //public void ListObjectsBucket(string nameBucket, Action<ListObjectsResponse, string> result)
    //{
    //    if (testConnection.networkConnected) {
    //        var request = new ListObjectsRequest()
    //        {
    //            BucketName = nameBucket
    //        };

    //        Client.ListObjectsAsync(request, (responseObject) =>
    //        {
    //            if (responseObject.Exception == null)
    //                result?.Invoke(responseObject.Response, "");
    //            else
    //                result?.Invoke(null, responseObject.Exception.ToString());
    //        });
    //    }
    //}

    /// <summary> 
    /// Get Object from S3 Bucket 
    /// </summary> 
    //public void GetObjectBucket(string S3BucketName, string fileNameOnBucket, Action<GetObjectResponse, string> result)
    //{
    //    if (testConnection.networkConnected) {
    //        resultTimeout = "";
    //        Invoke("ResultTimeoutGetObjectBucket", timeoutGetObject);

    //        var request = new GetObjectRequest
    //        {
    //            BucketName = S3BucketName,
    //            Key = fileNameOnBucket
    //        };

    //        Client.GetObjectAsync(request, (responseObj) =>
    //        {
    //            var response = responseObj.Response;

    //            if (response.ResponseStream != null)
    //            {
    //                result?.Invoke(responseObj.Response, "");
    //                resultTimeout = "success";
    //            }
    //            else
    //                result?.Invoke(null, responseObj.Exception.ToString());
    //        });
    //    }
    //}

    /// <summary> 
    /// Post Object to S3 Bucket. 
    /// </summary> 
    //    public void UploadObjectForBucket(string pathFile, string S3BucketName, string fileNameOnBucket, Action<PostObjectResponse, string> result)
    //    {
    //        if (testConnection.networkConnected) { 
    //        /* if (!File.Exists(pathFile)) 
    //        { 
    //        result?.Invoke(null, "FileNotFoundException: Could not find file " + pathFile); 
    //        return; 
    //        } 

    //        var stream = new FileStream(pathFile, FileMode.Open, FileAccess.Read, FileShare.Read); 
    //        var request = new PostObjectRequest() 
    //        { 
    //        Bucket = S3BucketName, 
    //        Key = fileNameOnBucket, 
    //        InputStream = stream, 
    //        CannedACL = S3CannedACL.Private, 
    //        Region = S3Region 
    //        }; 

    //        Client.PostObjectAsync(request, (responseObj) => 
    //        { 
    //        if (responseObj.Exception == null) 
    //        result?.Invoke(responseObj.Response, ""); 
    //        else 
    //        result?.Invoke(null, responseObj.Exception.ToString()); 
    //        // 

    //        */




    //        /////// 
    //        Debug.Log("entered s3 Manger script");
    //        string fileName = fileNameOnBucket;

    //        var stream = new FileStream(Application.persistentDataPath + pathFile, FileMode.Open, FileAccess.Read, FileShare.Read);

    //        // ResultText.text += "\nCreating request object"; 
    //        var request = new PutObjectRequest()
    //        {
    //            BucketName = S3BucketName,
    //            Key = fileName,
    //            InputStream = stream,
    //            CannedACL = S3CannedACL.Private
    //        };



    //        // ResultText.text += "\nMaking HTTP post call"; 


    //        Client.PutObjectAsync(request, (responseObj) =>
    //        {
    //            Debug.Log("entered putobjectasync");
    //            if (responseObj.Exception == null)
    //            {
    //                Debug.Log("if");
    //        // ResultText.text += string.Format("\nobject {0} posted to bucket {1}", responseObj.Request.Key, responseObj.Request.BucketName); 
    //    }
    //            else
    //            {
    //                Debug.Log("else");
    //        // ResultText.text += "\nException while posting the result object"; 
    //        Debug.Log(string.Format("\n receieved error {0}", responseObj.Response.HttpStatusCode.ToString()));
    //            }

    //        });
    //        // }); 
    //    }
    //}

    /// <summary> 
    /// Delete Objects in S3 Bucket 
    /// </summary> 
    //public void DeleteObjectOnBucket(string fileNameOnBucket, string S3BucketName, Action<DeleteObjectsResponse, string> result) 
    //{
    //        if (testConnection.networkConnected)
    //        {
    //            List<KeyVersion> objects = new List<KeyVersion>();
    //            objects.Add(new KeyVersion()
    //            {
    //                Key = fileNameOnBucket
    //            });

    //            var request = new DeleteObjectsRequest()
    //            {
    //                BucketName = S3BucketName,
    //                Objects = objects
    //            };

    //            Client.DeleteObjectsAsync(request, (responseObj) =>
    //            {
    //                if (responseObj.Exception == null)
    //                    result?.Invoke(responseObj.Response, "");
    //                else
    //                    result?.Invoke(null, responseObj.Exception.ToString());
    //            });
    //        }
    //} 

    #endregion 

    //#region METHODS UTILS 

    //private void ResultTimeoutGetObjectBucket() 
    //{ 
    //if(string.IsNullOrEmpty(resultTimeout)) 
    //{ 
    //OnResultGetObject?.Invoke(null, "Timeout GetObject"); 
    //} 
    //} 

    #endregion 
    }

